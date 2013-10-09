using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oszillator.Logic
{
    public interface IConnection
    {
        void Setup();
        void Start();
        void Stop();
        double Read();
    }
}
