using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arduino.Base.Logic
{
    public interface IConnection
    {
        void Setup(int channelCount);
        void Start();
        void Stop();
        void Close();
        Sample Read();
    }
}
