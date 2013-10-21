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
        private DateTime startTime;

        private Random random = new Random();

        public void Setup()
        {
        }

        public void Start()
        {
            this.startTime = DateTime.Now;
        }

        public void Stop()
        {
        }

        public Sample Read()
        {
            // Create Jitter
            if (random.Next(100) > 95)
            {
                Thread.Sleep(300);
            }

            Thread.Sleep(10);

            var sample = new Sample(1);
            var timeDone = (DateTime.Now- startTime).TotalSeconds;

            sample.Voltages[0] = Math.Sin(timeDone * 2) + 2;
            sample.SampleCount = 0;
            sample.SampleTime = DateTime.Now;
            return sample;
        }
    }
}
