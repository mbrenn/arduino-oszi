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
        private IConnection connection;

        private Thread osziThread;

        private bool running;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            this.connection = new OszillatorConnection(3);
            this.connection.Setup();
            this.connection.Start();
            this.StartDisplayThread();
        }

        /// <summary>
        /// Starts the display thread to render the current input
        /// </summary>
        private void StartDisplayThread()
        {
            this.osziThread = new Thread(this.LoopOszi);
            this.running = true;
            this.osziThread.Start();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            this.running = false;
            this.connection.Stop();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
        }

        private void LoopOszi(object obj)
        {
            while (true)
            {
                lock (this)
                {
                    if (this.running == false)
                    {
                        break;
                    }
                }

                var value = this.connection.Read();
                this.Dispatcher.Invoke(() =>
                {
                    this.Voltage.Text = value.ToString();
                    this.Oszilloskop.AddValue(value);
                });
            }
        }

        private void Dummy_Click(object sender, RoutedEventArgs e)
        {
            this.connection = new DummyConnection();
            this.connection.Setup();
            this.connection.Start();

            this.StartDisplayThread();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.running = false;
            this.connection.Stop();
            this.osziThread.Join(1000);
        }
    }
}
