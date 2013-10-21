using Oszillator.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Oszillator
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DataAcquisition acquisition;

        private TextBox[] textBoxes;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            this.textBoxes = new TextBox[] 
            {
                this.Voltage1,
                this.Voltage2,
                this.Voltage3,
                this.Voltage4,
                this.Voltage5,
                this.Voltage6
            };
        }

        /// <summary>
        /// Starts the oszilloscope with Serial connection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            var channelCount = 1;

            this.Oszilloskop.SetChannelCount(1);
            this.Oszilloskop.Start();

            this.acquisition = new DataAcquisition(new OszillatorConnection(3), channelCount);
            this.acquisition.Start();
            this.acquisition.SampleAction = OnSample;
        }

        /// <summary>
        /// Starts the oscilloscope with Dummy connection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DummyStart_Click(object sender, RoutedEventArgs e)
        {
            var channelCount = 2;

            this.Oszilloskop.SetChannelCount(channelCount);
            this.Oszilloskop.Start();
            
            this.acquisition = new DataAcquisition(new DummyConnection(), channelCount);
            this.acquisition.Start();
            this.acquisition.SampleAction = OnSample;
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            this.acquisition.Stop();
            this.acquisition = null;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.acquisition != null && this.acquisition.IsStarted)
            {
                this.acquisition.Stop();
                this.acquisition = null;
            }
        }

        private void OnSample(Sample sample)
        {
            this.Dispatcher.Invoke(() =>
            {
                for (var n = 0; n < sample.SampleCount; n++)
                {
                    this.textBoxes[n].Text = sample.Voltages[n].ToString("n3") + " V";
                }

                for (var n = 0; n < sample.SampleCount; n++)
                {
                    this.Oszilloskop.AddValue(sample.SampleTime, sample.Voltages[n], n);
                }
            });
        }
    }
}
