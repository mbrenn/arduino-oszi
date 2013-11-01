using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Oszillator.Logic
{
    /// <summary>
    /// Responsible for data acquisition
    /// </summary>
    public class DataAcquisition
    {
        private int channelCount = 0;

        public bool IsStarted
        {
            get;
            private set;
        }

        public int TotalSampleCount
        {
            get;
            set;
        }

        public DateTime SampleStartDate
        {
            get;
            set;
        }

        public IConnection Connection
        {
            get;
            set;
        }

        public Thread SampleThread
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets an action that will be executed, when a sample has been received
        /// </summary>
        public Action<Sample> SampleAction
        {
            get;
            set;
        }

        public DataAcquisition(IConnection connection, int channelCount)
        {
            this.channelCount = channelCount;
            this.Connection = connection;
        }

        public void Start()
        {
            if (this.IsStarted)
            {
                throw new InvalidOperationException("Already started");
            }

            this.Connection.Setup(this.channelCount);
            this.Connection.Start();

            lock (this)
            {
                this.IsStarted = true;
            }

            this.SampleThread = new Thread(this.SampleLoop);
            this.SampleThread.Name = "Sample Thread";
            this.SampleThread.Start();

            // Stores the start date
            this.SampleStartDate = DateTime.Now;
            this.TotalSampleCount = 0;
        }

        public void SampleLoop()
        {
            while (true)
            {
                lock (this)
                {
                    if (!this.IsStarted)
                    {
                        break;
                    }
                }

                // Do sampling
                var sample = this.Connection.Read();
                if (sample != null)
                {
                    lock (this)
                    {
                        this.TotalSampleCount++;
                    }

                    if (this.SampleAction != null)
                    {
                        this.SampleAction(sample);
                    }
                }
            }
        }

        /// <summary>
        /// Stops the sample
        /// </summary>
        public void Stop()
        {
            if (!this.IsStarted)
            {
                throw new InvalidOperationException("Not Started");
            }

            lock (this)
            {
                this.IsStarted = false;
            }

            this.SampleThread.Join(1000);
            this.Connection.Stop();
        }
    }
}
