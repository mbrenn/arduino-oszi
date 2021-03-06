﻿using Arduino.Generic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Arduino.Osci.Base.Logic
{
    public class DataAcquisitionEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the acquisition
        /// </summary>
        public DataAcquisition Acquisition
        {
            get;
            set;
        }

        public DataAcquisitionEventArgs(DataAcquisition acq)
        {
            this.Acquisition = acq;
        }

    }

    /// <summary>
    /// Responsible for data acquisition
    /// </summary>
    public class DataAcquisition
    {
        private int channelCount = 0;

        private const int BufferSize = 10000;

        /// <summary>
        /// Gets the value whether the data acquisition is currently running.
        /// If this value gets changed
        /// </summary>
        public bool IsRunning
        {
            get;
            private set;
        }

        /// <summary>
        /// This event is thrown, when the IsRunning variable has been changed
        /// </summary>
        public event EventHandler IsRunningChanged;

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

        /// <summary>
        /// Gets the number of channels to be used
        /// </summary>
        public int ChannelCount
        {
            get { return this.channelCount; }
        }

        /// <summary>
        /// Stores the samples for a certain amount of time
        /// </summary>
        private RingBuffer<Sample> buffer = new RingBuffer<Sample>(BufferSize);

        /// <summary>
        /// Gets the ring buffer. Not Threadsafe
        /// </summary>
        public RingBuffer<Sample> Buffer
        {
            get { return this.buffer; }
        }

        public DataAcquisition(IConnection connection, int channelCount)
        {
            this.channelCount = channelCount;
            this.Connection = connection;
        }

        private Sample[] temporaryBuffer = new Sample[BufferSize];

        /// <summary>
        /// Returns the buffer as threadsafe instance
        /// </summary>
        /// <returns>Gets the buffer of the list</returns>
        public Sample[] GetBuffer()
        {
            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();
            lock (this)
            {
                var totalRingBufferSize = this.Buffer.Count;

                var starting = Math.Max(0, this.Buffer.Count - BufferSize + 1);
                var m = 0;
                for (var n = starting; n < totalRingBufferSize; n++)
                {
                    this.temporaryBuffer[m] = this.buffer[n];
                    m++;
                }

                stopWatch.Stop();
                //System.Diagnostics.Debug.WriteLine("Duration: " + stopWatch.Elapsed.TotalMilliseconds.ToString() + " ms");

                return this.temporaryBuffer;
            }
        }

        /// <summary>
        /// Calls the IsRunningChanged event
        /// </summary>
        private void OnRunningChanged()
        {
            var e = this.IsRunningChanged;
            if (e != null)
            {
                e(this, EventArgs.Empty);
            }
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

            this.OnRunningChanged();

            this.SampleThread = new Thread(this.SampleLoop);
            this.SampleThread.Name = "Sample Thread";
            this.SampleThread.Start();

            // Stores the start date
            this.SampleStartDate = DateTime.Now;
            this.TotalSampleCount = 0;
        }

        public static DateTime lastUpdate;

        public void SampleLoop()
        {
            try
            {
                Debug.WriteLine("Starting sample loop with thread: " + 
                    Thread.CurrentThread.ManagedThreadId.ToString("X4"));

                while (true)
                {
                    lastUpdate = DateTime.Now;

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
                            this.buffer.Add(sample);
                        }

                        if (this.SampleAction != null)
                        {
                            this.SampleAction(sample);
                        }
                    }
                }

                Debug.WriteLine("Stopping sample loop");
            }
            finally
            {
                Debug.WriteLine("Finalizing sample loop");
            }
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

            this.OnRunningChanged();

            this.SampleThread.Join(1000);

            this.Connection.Close();
        }
    }
}
