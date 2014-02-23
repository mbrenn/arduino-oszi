using Arduino.Osci.Base.Logic;
using Arduiono.Osci.Base;
using Oszillator.Gui;
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
        /// <summary>
        /// Stores the osci settings
        /// </summary>
        private OsciSettings settings;

        private DataAcquisition acquisition;

        private TextBox[] textBoxes;

        private DateTime lastUpdate = DateTime.Now;

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

            CompositionTarget.Rendering += OnFrameUpdate;

            this.settings = OscilloscopeHelper.GetDefaultOsciSettings();

            this.UpdateStatusLine();
        }

        /// <summary>
        /// Starts the oszilloscope with Serial connection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var channelCount = this.settings.ChannelCount;

                this.Oszilloskop.SetChannelCount(channelCount);
                this.Oszilloskop.Start();

                this.acquisition = new DataAcquisition(OscilloscopeHelper.CreateConnection(this.settings), channelCount);
                this.acquisition.IsRunningChanged += (x, y) => { this.UpdateStatusLine(); };
                this.acquisition.Start();
                this.acquisition.SampleAction = OnSample;
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
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

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SettingsDlg(this.settings);
            dlg.Owner = this;
            if (dlg.ShowDialog() == true)
            {
                // Nothing to do, always true
            }
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            if (this.acquisition == null)
            {
                MessageBox.Show(this, "Not yet started");
                return;
            }

            this.acquisition.Stop();
            this.acquisition = null;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.acquisition != null && this.acquisition.IsRunning)
            {
                this.acquisition.Stop();
                this.acquisition = null;
            }
        }

        private void OnSample(Sample sample)
        {
            this.Oszilloskop.AddSampleForView(sample);

            if ((DateTime.Now - this.lastUpdate).TotalMilliseconds > 100)
            {
                this.lastUpdate = DateTime.Now;
                this.Dispatcher.Invoke(() =>
                {
                    if (this.acquisition == null)
                    {
                        // We are currently closing the window (or have closed it)
                        return;
                    }

                    for (var n = 0; n < sample.SampleCount; n++)
                    {
                        this.textBoxes[n].Text = sample.Voltages[n].ToString("n3") + " V";
                    }

                    var totalSeconds = (DateTime.Now - this.acquisition.SampleStartDate).TotalSeconds;
                    if (totalSeconds > 1)
                    {
                        var sps = this.acquisition.TotalSampleCount / totalSeconds;
                        this.sps.Text = sps.ToString("n1");
                    }
                });
            }
        }

        private void OnFrameUpdate(object sender, EventArgs e)
        {
            this.Oszilloskop.ShowSamplesOnHold();
        }

        private void UpdateStatusLine()
        {
            var text = string.Format("{0} channels on \"{1}\"", this.settings.ChannelCount, this.settings.SerialPort);

            if (this.acquisition != null && this.acquisition.IsRunning)
            {
                text += ", Running";
            }

            this.StatusLine.Text = text;
        }
    }
}
