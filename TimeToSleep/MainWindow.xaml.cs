using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;

namespace TimeToSleep
{
    public enum StandbyMode
    {
        Hibernate,
        Sleep
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Dispatcher dispatcherThreadReference;
        private System.Timers.Timer timer;
        private System.Timers.Timer displayTimer;
        private StandbyMode mode;
        private DateTime stopTime;
        private string timeRemainingString;

        public MainWindow()
        {
            InitializeComponent();

            dispatcherThreadReference = Dispatcher.CurrentDispatcher;
        }

        public void Set(int minutes)
        {
            timer = new System.Timers.Timer();
            timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            timer.Interval = 1000 * 60 * minutes;
            
            stopTime = DateTime.Now;
            stopTime = stopTime.AddMilliseconds(1000 * 60 * minutes);

            displayTimer = new System.Timers.Timer();
            displayTimer.Elapsed += new ElapsedEventHandler(EverySecondEvent);
            displayTimer.Interval = 1000;

            timer.Start();
            displayTimer.Start();
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            try
            {
                timer.Stop();
                displayTimer.Stop();

                // Take action on the UI thread
                Action action = new Action(() => { SetButton.IsEnabled = true; });
                dispatcherThreadReference.Invoke(action);
            }
            catch { }

            if (mode == StandbyMode.Hibernate)
            {
                Hibernate();
            }
            else if (mode == StandbyMode.Sleep)
            {
                Sleep();
            }
        }

        private void EverySecondEvent(object source, ElapsedEventArgs e)
        {
            TimeSpan span = stopTime - DateTime.Now;
            if (span.TotalMilliseconds < 0)
            {
                // time expired
                displayTimer.Stop();
            }
            else
            {
                int hoursRemaining = span.Hours;
                int minutesRemaining = span.Minutes;
                int secondsRemaining = span.Seconds;

                if (hoursRemaining > 0)
                {
                    int numberOfDigits = 2;
                    string formattingString_twoDigitsPadded = String.Format("D{0}", numberOfDigits);
                    timeRemainingString = String.Format("Time Remaining: {0}:{1}:{2}", 
                        hoursRemaining, minutesRemaining.ToString(formattingString_twoDigitsPadded), secondsRemaining.ToString(formattingString_twoDigitsPadded));
                }
                else
                {
                    int numberOfDigits = 2;
                    string formattingString_twoDigitsPadded = String.Format("D{0}", numberOfDigits);
                    timeRemainingString = String.Format("Time Remaining: {0}:{1}", minutesRemaining, secondsRemaining.ToString(formattingString_twoDigitsPadded));
                }

                // Take action on the UI thread
                Action action = new Action(() => { TimeRemainingLabel.Content = timeRemainingString; });
                dispatcherThreadReference.Invoke(action);
            }
        }

        public void Hibernate()
        {
            bool retVal = System.Windows.Forms.Application.SetSuspendState(PowerState.Hibernate, false, false);
            if (!retVal)
            {
                Sleep();
            }
        }

        public void Sleep()
        {
            bool retVal = System.Windows.Forms.Application.SetSuspendState(PowerState.Suspend, false, false);
            if (retVal == false)
            {
                System.Windows.Forms.MessageBox.Show("Could not put the system into Standby.");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SetButton.IsEnabled = false;

            if (ModeSelection.SelectedIndex == 0) // hibernate
            {
                mode = StandbyMode.Hibernate;
            }
            else if (ModeSelection.SelectedIndex == 1) // sleep
            {
                mode = StandbyMode.Sleep;
            }

            try
            {
                int mins = Convert.ToInt32(minutes.Text);
                Set(mins);
            }
            catch 
            {
                SetButton.IsEnabled = true;
                System.Windows.Forms.MessageBox.Show("Please enter an integer number.");
            }
        }

        /// <summary>
        /// Restricts textbox input to integers
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void minutes_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            bool enteredLetter = false;
            Queue<char> text = new Queue<char>();
            foreach (var ch in this.minutes.Text)
            {
                if (char.IsDigit(ch))
                {
                    text.Enqueue(ch);
                }
                else
                {
                    enteredLetter = true;
                }
            }

            if (enteredLetter)
            {
                StringBuilder sb = new StringBuilder();
                while (text.Count > 0)
                {
                    sb.Append(text.Dequeue());
                }

                this.minutes.Text = sb.ToString();
                this.minutes.SelectionStart = this.minutes.Text.Length;
            }
        }
    }
}
