using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arduino.Base.Logic
{
    /// <summary>
    /// Stores one sample point
    /// </summary>
    public class Sample
    {
        public Sample(int sampleCount)
        {
            this.Voltages = new double[sampleCount];
            this.SampleCount = sampleCount;
            this.SampleTime = DateTime.Now;
        }

        /// <summary>
        /// Stores the number of voltages being stored
        /// </summary>
        public int SampleCount; 

        /// <summary>
        /// Stores the current sample time
        /// </summary>
        public DateTime SampleTime;

        /// <summary>
        /// Stires the voltages
        /// </summary>
        public double[] Voltages;
    }
}
