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
        /// Stores the number of channels
        /// </summary>
        private int channelCount = 0;


        public void SetChannelCount(int channels)
        {
            if (channels < 1 || channels > 6)
            {
                throw new InvalidOperationException("Channels must be between 1 and 6");
            }

            this.channelCount = channels;
        }

        public OsziLinesDrawing()
        {
            InitializeComponent();
        }

        Pen dPen = new Pen(Brushes.Red, 1);

        double size = 50;

        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawEllipse(Brushes.Green, dPen, new Point(10.0, 10.0), size, size);

            size -= 0.1;
            if (size < 5)
            {
                size = 50;
            }
        }

        public void Start()
        {
        }

        internal void AddSampleForView(Arduino.Osci.Base.Logic.Sample sample)
        {
        }

        internal void ShowSamplesOnHold()
        {
            this.InvalidateVisual();
        }
    }
}
