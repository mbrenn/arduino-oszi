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
        /// Defines the number of pixels being used per second
        /// </summary>
        private double pixelsPerSecond = 100;

        /// <summary>
        /// Stores the time value of the left side of the oszilloscope
        /// </summary>
        private DateTime timeLeft = DateTime.MinValue;

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

        public void Start()
        {
            this.timeLeft = DateTime.Now;
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

        public void AddValue(DateTime time, double value, int channel = 0)
        {
            if ( this.timeLeft == DateTime.MinValue )
            {
                throw new InvalidOperationException("Oszilloscope has not been started");
            }

            // Calculates the Y-Position of the point by value
            var a = this.controlHeight / (this.valueTop - this.valueBottom);
            var b = -this.controlHeight * this.valueBottom / (this.valueTop - this.valueBottom);
            var positionHeight = this.controlHeight - (a * value + b);

            // Calculates the X-Position of the point by time
            var secondsFromLeft = (DateTime.Now - timeLeft).TotalSeconds;
            var xValue = secondsFromLeft * this.pixelsPerSecond;

            while (xValue > this.widthSamplePoints)
            {
                xValue -= this.widthSamplePoints;
            }

            // Now render all the points between last point and this point
            var xValueAsInteger = Convert.ToInt32(Math.Floor(xValue));

            // Paint X: this.currentPosition to xValueAsInteger
            // Paint Y: this.lastPositionOnScreen to positionHeight
            while (this.currentPosition != xValueAsInteger)
            {
                this.lines[channel][this.currentPosition].Y1 = this.lastPositionOnScreen;
                this.lines[channel][this.currentPosition].Y2 = positionHeight;

                this.lastPositionOnScreen = positionHeight;

                this.currentPosition++;
                if (this.currentPosition >= this.widthSamplePoints)
                {
                    this.currentPosition = 0;
                }
            }

            // Done
            // Stores the last position, being used to transfer
            this.lastPositionOnScreen = positionHeight;
        }
    }
}
