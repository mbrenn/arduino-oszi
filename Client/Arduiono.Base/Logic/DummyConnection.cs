using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Arduino.Osci.Base.Logic
{
    public class DummyConnection : IConnection
    {
        private int channelCount = 0;
        private DateTime startTime;

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

        public void Close()
        {
        }

        public Sample Read()
        {
            Thread.Sleep(10);

            var sample = new Sample(this.channelCount);
            var timeDone = (DateTime.Now - startTime).TotalSeconds;

            for (var n = 0; n < this.channelCount; n++)
            {
                if (n % 2 == 0)
                {
                    sample.Voltages[n] = Math.Sin(timeDone * 2 * (n + 1)) + 2 + 0.1 * n;
                }
                else
                {
                    sample.Voltages[n] = Math.Cos(timeDone * 2 * n) + 1.5 + 0.1 * n;
                }
            }

            sample.SampleCount = this.channelCount;
            sample.SampleTime = DateTime.Now;
            return sample;
        }
    }
}
