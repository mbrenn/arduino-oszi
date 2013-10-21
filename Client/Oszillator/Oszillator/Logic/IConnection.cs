using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oszillator.Logic
{
    public interface IConnection
    {
        void Setup(int channelCount);
        void Start();
        void Stop();
        Sample Read();
    }
}
