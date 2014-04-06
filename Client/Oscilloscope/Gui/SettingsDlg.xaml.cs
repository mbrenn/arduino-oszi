using Arduiono.Osci.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Oszillator.Gui
{
    /// <summary>
    /// Interaktionslogik für SettingsDlg.xaml
    /// </summary>
    public partial class SettingsDlg : Window
    {
        private OsciSettings settings;

        public Button[] buttons;

        public SettingsDlg(OsciSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            this.settings = settings;

            InitializeComponent();
        }

        private List<string> ports = new List<string>();

        private void Window_Initialized(object sender, EventArgs e)
        {
            this.buttons = new Button[] {
                this.Channel1, 
                this.Channel2, 
                this.Channel3, 
                this.Channel4, 
                this.Channel5, 
                this.Channel6
            };

            this.ports = OscilloscopeHelper.GetPorts();
            this.SerialPorts.ItemsSource = this.ports;

            var channels = new[] { 1, 2, 3, 4, 5, 6 };

            this.SerialPorts.SelectedItem = this.ports.Where(x => x == this.settings.SerialPort).FirstOrDefault();

            this.UpdateButtonColor();

        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            this.settings.SerialPort = Convert.ToString(this.SerialPorts.SelectedValue);

            this.DialogResult = true;
            this.Close();
        }

        private void Channel1_Click(object sender, RoutedEventArgs e)
        {
            this.settings.ChannelCount = 1;
            this.UpdateButtonColor();

        }

        private void Channel2_Click(object sender, RoutedEventArgs e)
        {
            this.settings.ChannelCount = 2;
            this.UpdateButtonColor();

        }

        private void Channel3_Click(object sender, RoutedEventArgs e)
        {
            this.settings.ChannelCount = 3;
            this.UpdateButtonColor();

        }

        private void Channel4_Click(object sender, RoutedEventArgs e)
        {
            this.settings.ChannelCount = 4;
            this.UpdateButtonColor();

        }

        private void Channel5_Click(object sender, RoutedEventArgs e)
        {
            this.settings.ChannelCount = 5;
            this.UpdateButtonColor();

        }

        private void Channel6_Click(object sender, RoutedEventArgs e)
        {
            this.settings.ChannelCount = 6;
            this.UpdateButtonColor();
        }

        private void UpdateButtonColor()
        {
            for (var n = 0; n < this.buttons.Length; n++)
            {
                if (n == (this.settings.ChannelCount - 1))
                {
                    this.buttons[n].Background = Brushes.Green;
                }
                else
                {
                    this.buttons[n].Background = Brushes.Gray;
                }
            }
        }

    }
}
