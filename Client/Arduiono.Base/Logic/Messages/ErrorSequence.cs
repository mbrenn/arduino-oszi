using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arduino.Osci.Base.Logic.Messages
{
    /// <summary>
    /// Defines that an error sequence had been received
    /// </summary>
    public class ErrorSequence
    {
        public byte ErrorCode
        {
            get;
            set;
        }
    }
}
