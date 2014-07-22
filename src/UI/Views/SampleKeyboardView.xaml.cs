using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Threading;

namespace JuliusSweetland.ETTA.UI.Views
{
    /// <summary>
    /// Interaction logic for SampleKeyboardView.xaml
    /// </summary>
    public partial class SampleKeyboardView : UserControl, INotifyPropertyChanged
    {
        public SampleKeyboardView()
        {
            InitializeComponent();

            var timer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(10)};
            timer.Tick += timer_Tick;
            timer.Start();

            DataContext = this;
        }

        private double value;
        public double Value
        {
            get { return value; }
            set { this.value = value; OnPropertyChanged("Value"); }
        }
        
        void timer_Tick(object sender, EventArgs e)
        {
            double newValue = Value + 1;
            if (newValue > 100)
            {
                newValue = 0;
            }
            Value = newValue;
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        #endregion
    }
}
