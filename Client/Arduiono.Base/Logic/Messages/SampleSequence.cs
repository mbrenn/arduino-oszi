using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arduino.Base.Logic.Messages
{
    /// <summary>
    /// Defines that a sample sequence had been received
    /// </summary>
    public class SampleSequence
    {
        public Sample Sample
        {
            get;
            set;
        }
    }
}
