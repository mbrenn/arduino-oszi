using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public bool IsRunning
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
            if (this.IsRunning)
            {
                throw new InvalidOperationException("Already started");
            }

            this.Connection.Setup(this.channelCount);
            this.Connection.Start();

            lock (this)
            {
                this.IsRunning = true;
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
            Debug.WriteLine("Starting sample loop");

            while (true)
            {
                lock (this)
                {
                    if (!this.IsRunning)
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

            Debug.WriteLine("Stopping sample loop");
        }

        /// <summary>
        /// Stops the sample
        /// </summary>
        public void Stop()
        {
            if (!this.IsRunning)
            {
                throw new InvalidOperationException("Not Started");
            }

            // Stops the connection
            this.Connection.Stop();

            lock (this)
            {
                this.IsRunning = false;
            }

            this.SampleThread.Join(1000);

            this.Connection.Close();
        }
    }
}
