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
            this.ports = OscilloscopeHelper.GetPorts();
            this.SerialPorts.ItemsSource = this.ports;

            var channels = new[] { 1, 2, 3, 4, 5, 6 };
            this.Channels.ItemsSource = channels;

            this.Channels.SelectedIndex = settings.ChannelCount - 1;
            this.SerialPorts.SelectedItem = this.ports.Where(x => x == this.settings.SerialPort).FirstOrDefault();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            this.settings.ChannelCount = Convert.ToInt32(this.Channels.SelectedValue);
            this.settings.SerialPort = Convert.ToString(this.SerialPorts.SelectedValue);

            this.DialogResult = true;
            this.Close();
        }
    }
}
