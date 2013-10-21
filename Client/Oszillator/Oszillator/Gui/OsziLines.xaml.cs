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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Oszillator.Gui
{
    /// <summary>
    /// Interaktionslogik für OsziLines.xaml
    /// </summary>
    public partial class OsziLines : UserControl
    {
        /// <summary>
        /// Stores the lines.
        /// First array element is the channel number, second number the pixel
        /// </summary>
        private Line[][] lines;

        private int currentPosition;

        private double controlHeight;

        /// <summary>
        /// Stores the width of the diagram in seconds
        /// </summary>
        private double widthSamplePoints;

        /// <summary>
        /// Stores the y-Position on screen
        /// </summary>
        private double lastPositionOnScreen = 0.0;

        private double valueTop = 5.2;

        private double valueBottom = -0.2;

        public OsziLines()
        {
            InitializeComponent();
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Surface.Children.Clear();
            this.currentPosition = 0;

            var width = Convert.ToInt32(e.NewSize.Width);

            var channels = 1;
            this.lines = new Line[channels][];

            for (var n = 0; n < channels; n++)
            {
                this.lines[n] = new Line[width];
                for (var x = 0; x < width; x++)
                {
                    this.lines[n][x] = new Line();
                    this.lines[n][x].X1 = x;
                    this.lines[n][x].X2 = x + 1;
                    this.lines[n][x].Y1 = e.NewSize.Height / 2;
                    this.lines[n][x].Y2 = e.NewSize.Height / 2;
                    this.lines[n][x].Stroke = Brushes.Red;
                    this.lines[n][x].StrokeThickness = 1;

                    this.Surface.Children.Add(this.lines[n][x]);
                }
            }

            this.controlHeight = e.NewSize.Height;
            this.lastPositionOnScreen = this.controlHeight / 2.0;
            this.widthSamplePoints = e.NewSize.Width;
        }

        public void AddValue(double value, int channel = 0)
        {
            var a = this.controlHeight / (this.valueTop - this.valueBottom);
            var b = -this.controlHeight * this.valueBottom / (this.valueTop - this.valueBottom);
            var positionHeight = this.controlHeight - (a * value + b);
            this.lines[channel][this.currentPosition].Y1 = this.lastPositionOnScreen;
            this.lines[channel][this.currentPosition].Y2 = positionHeight;

            this.lastPositionOnScreen = positionHeight;

            this.currentPosition++;
            if (this.currentPosition >= this.widthSamplePoints)
            {
                this.currentPosition = 0;
            }
        }
    }
}
