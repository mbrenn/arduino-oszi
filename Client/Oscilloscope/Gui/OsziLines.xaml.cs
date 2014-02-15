using Arduino.Osci.Base.Logic;
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
        private Color[] lineColors = new Color[]
        {
            Colors.LightBlue,
            Colors.Red,
            Colors.Green,
            Colors.Pink,
            Colors.Yellow,
            Colors.White
        };

        /// <summary>
        /// Stores the lines.
        /// First array element is the channel number, second number the pixel
        /// </summary>
        private Line[][] lines;

        private double controlHeight;

        /// <summary>
        /// Stores the width of the diagram in seconds
        /// </summary>
        private double widthSamplePoints;

        /// <summary>
        /// Defines the number of pixels being used per second
        /// </summary>
        private double pixelsPerSecond = 300;

        /// <summary>
        /// Stores the time value of the left side of the oszilloscope
        /// </summary>
        private DateTime[] timeLeft;

        /// <summary>
        /// Stores the y-Position on screen
        /// </summary>
        private double[] lastPositionOnScreen;

        /// <summary>
        /// Stores the current X-position for the given plot
        /// </summary>
        private int[] currentPosition;

        private double valueTop = 5.2;

        private double valueBottom = -0.2;

        private List<Sample> samples = new List<Sample>();

        /// <summary>
        /// Stores the number of channels
        /// </summary>
        private int channelCount = 0;

        public OsziLines()
        {
            InitializeComponent();
        }

        public void SetChannelCount(int channels)
        {
            if (channels < 1 || channels > 6)
            {
                throw new InvalidOperationException("Channels must be between 1 and 6");
            }

            this.channelCount = channels;

            this.timeLeft = new DateTime[this.channelCount];
            this.lastPositionOnScreen = new double[this.channelCount];
            this.currentPosition = new int[this.channelCount];

            this.ResetView();
        }

        public void Start()
        {
            for (var n = 0; n < this.channelCount; n++)
            {
                this.timeLeft[n] = DateTime.Now;
                this.lastPositionOnScreen[n] = this.RenderSize.Height / 2.0;
            }
        }

        /// <summary>
        /// Resets the complete view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.ResetView();
        }

        private void ResetView()
        {
            if (Double.IsNaN(this.RenderSize.Height) || Double.IsNaN(this.RenderSize.Width))
            {
                // Nothing to do here
                return;
            }

            this.Surface.Children.Clear();
            
            var width = Convert.ToInt32(this.RenderSize.Width);
            var height = Convert.ToInt32(this.RenderSize.Height);

            this.lines = new Line[this.channelCount][];

            for (var n = 0; n < this.channelCount; n++)
            {
                this.currentPosition[n] = 0;

                var brush = new SolidColorBrush(this.lineColors[n]);

                this.lines[n] = new Line[width];
                for (var x = 0; x < width; x++)
                {
                    this.lines[n][x] = new Line();
                    this.lines[n][x].X1 = x;
                    this.lines[n][x].X2 = x + 1;
                    this.lines[n][x].Y1 = height / 2;
                    this.lines[n][x].Y2 = height / 2;
                    this.lines[n][x].Stroke = brush;
                    this.lines[n][x].StrokeThickness = 1;

                    this.Surface.Children.Add(this.lines[n][x]);
                }

                this.lastPositionOnScreen[n] = this.controlHeight / 2.0;
            }

            this.controlHeight = height;
            this.widthSamplePoints = width;
        }

        public void AddValue(DateTime time, double value, int channel)
        {
            if (this.timeLeft[channel] == DateTime.MinValue)
            {
                throw new InvalidOperationException("Oszilloscope has not been started");
            }

            // Calculates the Y-Position of the point by value
            var a = this.controlHeight / (this.valueTop - this.valueBottom);
            var b = -this.controlHeight * this.valueBottom / (this.valueTop - this.valueBottom);
            var positionHeight = this.controlHeight - (a * value + b);

            // Calculates the X-Position of the point by time
            var secondsFromLeft = (time - timeLeft[channel]).TotalSeconds;
            var xValue = secondsFromLeft * this.pixelsPerSecond;

            while (xValue > this.widthSamplePoints)
            {
                xValue -= this.widthSamplePoints;
            }

            // Now render all the points between last point and this point
            var xValueAsInteger = Convert.ToInt32(Math.Floor(xValue));
            if (xValueAsInteger < 0)
            {
                xValueAsInteger = 0;
            }

            // Paint X: this.currentPosition to xValueAsInteger
            // Paint Y: this.lastPositionOnScreen to positionHeight
            while (this.currentPosition[channel] != xValueAsInteger)
            {
                this.lines[channel][this.currentPosition[channel]].Y1 = this.lastPositionOnScreen[channel];
                this.lines[channel][this.currentPosition[channel]].Y2 = positionHeight;

                this.lastPositionOnScreen[channel] = positionHeight;

                this.currentPosition[channel]++;
                if (this.currentPosition[channel] >= this.widthSamplePoints)
                {
                    this.currentPosition[channel] = 0;
                }
            }
        }

        /// <summary>
        /// Adds a sample on queue to draw into window. 
        /// Ths sample will only be drawn, when 'ShowSamplesOnHold' has been called
        /// </summary>
        /// <param name="sample">Sample to be added</param>
        internal void AddSampleForView(Sample sample)
        {
            lock (this)
            {
                this.samples.Add(sample);
            }
        }

        /// <summary>
        /// Draws all the samples, that were put on hold
        /// </summary>
        public void ShowSamplesOnHold()
        {
            List<Sample> tempSamples;
            lock (this)
            {
                tempSamples = this.samples.ToList();
                this.samples.Clear();
            }

            foreach (var sample in tempSamples)
            {
                for (var n = 0; n < sample.SampleCount; n++)
                {
                    this.AddValue(sample.SampleTime, sample.Voltages[n], n);
                }
            }
        }
    }
}
