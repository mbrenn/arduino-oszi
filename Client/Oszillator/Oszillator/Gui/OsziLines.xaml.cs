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
        /// Stores the lines
        /// </summary>
        private Line[] lines;

        private int currentPosition;

        private double controlHeight;

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
            this.currentPosition = 0;

            var width = Convert.ToInt32(e.NewSize.Width);
            this.lines = new Line[width];
            for (var x = 0; x < width; x++)
            {
                this.lines[x] = new Line();
                this.lines[x].X1 = x;
                this.lines[x].X2 = x + 1;
                this.lines[x].Y1 = e.NewSize.Height / 2;
                this.lines[x].Y2 = e.NewSize.Height / 2;
                this.lines[x].Stroke = Brushes.Red;
                this.lines[x].StrokeThickness = 1;

                this.Surface.Children.Add(this.lines[x]);
            }

            this.controlHeight = e.NewSize.Height;
            this.lastPositionOnScreen = this.controlHeight;
        }

        public void AddValue(double value)
        {
            var a = this.controlHeight / (this.valueTop - this.valueBottom );
            var b = - this.controlHeight * this.valueBottom  / ( this.valueTop - this.valueBottom );
            var positionHeight = this.controlHeight - (a * value + b);
            this.lines[this.currentPosition].Y1 = this.lastPositionOnScreen;
            this.lines[this.currentPosition].Y2 = positionHeight;

            this.lastPositionOnScreen = positionHeight;

            this.currentPosition++;
            if (this.currentPosition >= this.lines.Length)
            {
                this.currentPosition = 0;
            }
        }
    }
}
