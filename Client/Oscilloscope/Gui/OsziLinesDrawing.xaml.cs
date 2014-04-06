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
    /// Interaction logic for OsziLinesDrawing.xaml
    /// </summary>
    public partial class OsziLinesDrawing : UserControl
    {
        /// <summary>
        /// Stores the datetime of the pixel that is at the left border of the screen
        /// </summary>
        public DateTime timeLeftBorder;

        /// <summary>
        /// Stores the span from the left to the right side of the window
        /// </summary>
        public TimeSpan spanWindow = TimeSpan.FromSeconds(2);

        /// <summary>
        /// Gives the value at top of screen
        /// </summary>
        public double ValueAtTop = 6;

        /// <summary>
        /// Gives the value at bottom of screen
        /// </summary>
        public double ValueAtBottom = -1;

        private Color[] lineColors = new Color[]
        {
            Colors.LightBlue,
            Colors.Red,
            Colors.Green,
            Colors.Pink,
            Colors.Yellow,
            Colors.White
        };

        private Pen[] pens;

        /// <summary>
        /// Gets or sets the data acquisition to be used
        /// </summary>
        public DataAcquisition DataAcquisition
        {
            get;
            set;
        }

        public OsziLinesDrawing()
        {
            InitializeComponent();

            this.pens = new Pen[lineColors.Length];
            for (var n = 0; n < lineColors.Length; n++)
            {
                this.pens[n] = new Pen(new SolidColorBrush(this.lineColors[n]), 2);
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var width = this.ActualWidth;
            var height = this.ActualHeight;

            drawingContext.DrawRectangle(Brushes.Black, null, new Rect(0, 0, width, height));

            if (this.DataAcquisition == null)
            {
                // Nothing to draw
                return;
            }

            // Perfoms the wrap, if we have reached the right side. 
            var offset = DateTime.Now - this.timeLeftBorder;
            while (offset > this.spanWindow)
            {
                offset -= this.spanWindow;
                this.timeLeftBorder += this.spanWindow;
            }

            // Calculate starting samples
            var startingSamples = this.timeLeftBorder;
            startingSamples -= this.spanWindow - offset;

            // Calculates some factors
            var a = height / (this.ValueAtTop - this.ValueAtBottom);
            var b = -height * this.ValueAtBottom / (this.ValueAtTop - this.ValueAtBottom);

            // Now paint
            for (var channel = 0; channel < this.DataAcquisition.ChannelCount; channel++)
            {
                var lastX = Double.MinValue;
                var lastY = Double.MinValue;

                foreach (var sample in this.DataAcquisition.GetBuffer())
                {
                    if (sample.SampleTime < startingSamples)
                    {
                        // In past
                        continue;
                    }

                    // Get X-Coordinate
                    var x = ((sample.SampleTime - this.timeLeftBorder).TotalSeconds / this.spanWindow.TotalSeconds) * width;
                    if (x < 0)
                    {
                        // Left of screen (due to repainting of old data even when we reached right side)
                        x += width;
                    }

                    // Gets the Y-Position
                    var y = height - (a * sample.Voltages[channel] + b);

                    if (lastX != Double.MinValue && lastX <= x)
                    {
                        drawingContext.DrawLine(this.pens[channel], new Point(lastX, lastY), new Point(x, y));
                    }

                    lastX = x;
                    lastY = y;
                }
            }
        }

        public void Start()
        {
            this.timeLeftBorder = DateTime.Now;
        }

        internal void ShowSamplesOnHold()
        {
            if (this.DataAcquisition != null && this.DataAcquisition.IsRunning)
            {
                this.InvalidateVisual();
            }
        }
    }
}
