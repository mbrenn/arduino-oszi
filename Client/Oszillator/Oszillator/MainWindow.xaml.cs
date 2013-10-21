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

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            this.acquisition = new DataAcquisition(new OszillatorConnection(3));
            this.acquisition.Start();
            this.acquisition.SampleAction = LoopOszi;
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            this.acquisition.Stop();
            this.acquisition = null;
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
        }

        private void LoopOszi(Sample sample)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.Voltage.Text = sample.Voltages[0].ToString();
                this.Oszilloskop.AddValue(sample.Voltages[0]);
            });
        }

        private void Dummy_Click(object sender, RoutedEventArgs e)
        {
            this.acquisition = new DataAcquisition(new DummyConnection());
            this.acquisition.Start();
            this.acquisition.SampleAction = LoopOszi;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.acquisition != null && this.acquisition.IsStarted)
            {
                this.acquisition.Stop();
                this.acquisition = null;
            }
        }
    }
}
