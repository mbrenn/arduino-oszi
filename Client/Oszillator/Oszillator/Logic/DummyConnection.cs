using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Oszillator.Logic
{
    public class DummyConnection : IConnection
    {
        public void Setup()
        {
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public double Read()
        {
            Thread.Sleep(10);
            return Math.Sin(((double)(DateTime.Now.Ticks % 10000)) * 0.001);
        }
    }
}
