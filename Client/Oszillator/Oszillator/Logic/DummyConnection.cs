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
        private int channelCount = 0;
        private DateTime startTime;

        private Random random = new Random();

        public void Setup(int channelCount)
        {
            this.channelCount = channelCount;
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
            Thread.Sleep(10);

            var sample = new Sample(2);
            var timeDone = (DateTime.Now- startTime).TotalSeconds;

            sample.Voltages[0] = Math.Sin(timeDone * 2) + 2;
            if (this.channelCount > 1)
            {
                sample.Voltages[1] = Math.Cos(timeDone * 3) + 1.5;
            }

            sample.SampleCount = 2;
            sample.SampleTime = DateTime.Now;
            return sample;
        }
    }
}
