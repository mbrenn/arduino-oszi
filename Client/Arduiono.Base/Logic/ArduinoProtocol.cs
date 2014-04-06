using Arduino.Osci.Base.Logic.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Arduino.Osci.Base.Logic
{
    /// <summary>
    /// Performs an abstraction of the connection interface to arduino board
    /// </summary>
    public class ArduinoProtocol
    {
        /// <summary>
        /// Synchronisationobject for message sending
        /// </summary>
        private object messageSync = new object();

        /// <summary>
        /// Stores the number of channels
        /// </summary>
        private int analogChannelCount = 1;

        /// <summary>
        /// Defines the client state
        /// </summary>
        private volatile bool isRunning = false;

        /// <summary>
        /// Stores the port
        /// </summary>
        private SerialPort port;

        private List<byte> buffer = new List<byte>();

        /// <summary>
        /// This event is triggered, when the stop message had been received
        /// </summary>
        private AutoResetEvent stopMessageReceived = new AutoResetEvent(false);

        private int[] messageLengths =
        {
            2, 3, 4, 5, 7, 8        
        };

        /// <summary>
        /// Gets the buffer of bytes.
        /// Public for unit testing
        /// </summary>
        public List<byte> Buffer
        {
            get { return this.buffer; }
        }

        /// <summary>
        /// Initializes a new instance of the ArduinoConnection class. 
        /// </summary>
        /// <param name="port">A full initialized Serial Port connection</param>
        public ArduinoProtocol(SerialPort port)
        {
            this.port = port;
            this.port.ReadTimeout = 200;
            Debug.WriteLine("Constructing protocol");
        }

        public void SetAnalogChannelCount(int channelCount)
        {
            if (channelCount < 1 || channelCount > 6)
            {
                throw new InvalidOperationException("Channelcount is not between 1 and 6");
            }

            this.analogChannelCount = channelCount;

            lock (this.messageSync)
            {
                this.port.Write(new char[] { 'a', channelCount.ToString()[0] }, 0, 2);
            }
        }

        public void SendStartCommand()
        {
            if (this.isRunning)
            {
                throw new InvalidOperationException("Client is already running");
            }

            lock (this.messageSync)
            {
                this.port.Write(new char[] { 'g', analogChannelCount.ToString()[0] }, 0, 1);
            }

            this.isRunning = true;
            Debug.WriteLine("Go has been sent");
        }

        public void SendStopCommand()
        {
            if (!this.isRunning)
            {
                throw new InvalidOperationException("Client is not running");
            }

            lock (this.messageSync)
            {
                this.port.Write(new char[] { 's', analogChannelCount.ToString()[0] }, 0, 1);
            }

            // Waiting for stop in pull
            Debug.WriteLine("Stop has been sent");
        }

        public void SendStopConfirmationCommand()
        {
            if (this.isRunning)
            {
                throw new InvalidOperationException("Stop sequence has not yet been received");
            }

            lock ( this.messageSync)
            {
                this.port.Write(new char[] { 't' }, 0, 1);
            }

            Debug.WriteLine("Stop Confirmation has been sent");
        }

        /// <summary>
        /// Pulls a byte from board and returns a sequence, when a message is complete.
        /// This function has to be called continously.
        /// </summary>
        /// <param name="synchron">true, if call shall be blocked until data had been received</param>
        /// <returns>true, if object is available</returns>
        public object Pull(bool synchron = true)
        {
            try
            {
                if (this.port.BytesToRead > 0 || synchron)
                {
                    var value = (byte)this.port.ReadByte();

                    // Add value to internal array
                    this.buffer.Add(value);
                    return this.EvaluateBuffer();
                }
            }
            catch (TimeoutException)
            {
                // Nothing to to
            }

            return null;
        }

        /// <summary>
        /// Evaluates the internal buffer
        /// </summary>
        /// <returns></returns>
        public object EvaluateBuffer()
        {
            // Checks, if we have two bytes
            if (this.buffer.Count < 2)
            {
                // Nothing to return
                return null;
            }

            // Check for sync
            var length = this.buffer.Count;
            if (length >= 3 &&
                this.buffer[length - 1] == 0xFF &&
                this.buffer[length - 2] == 0xFF &&
                this.buffer[length - 3] == 0xFF)
            {
                this.buffer.Clear();
                return new SyncSequence();
            }

            // Check for stop
            if (length == 3 &&
                this.buffer[0] == 0xFF &&
                this.buffer[1] == 0xFF &&
                this.buffer[2] == 0x01)
            {
                Debug.WriteLine("Stop has been received");
                this.buffer.Clear();
                this.isRunning = false;

                this.stopMessageReceived.Set();

                return new StopSequence();
            }
    
            // Check for error
            if (length == 4 &&
                this.buffer[0] == 0xFF &&
                this.buffer[1] == 0xFF &&
                this.buffer[2] == 0xFE)
            {
                var code = this.buffer[3];
                this.buffer.Clear();

                Debug.WriteLine("Error has been received: " + code.ToString());
                return new ErrorSequence()
                {
                    ErrorCode = code
                };
            }

            // Check for use data
            if (length == this.messageLengths[this.analogChannelCount - 1]
                && !(this.buffer[0] == 0xFF && this.buffer[1] == 0xFF))
            {
                var result = this.TranslateToSampleSequence();
                this.buffer.Clear();
                return result;
            }

            return null;
        }

        public SampleSequence TranslateToSampleSequence()
        {
            var sample = new Sample(this.analogChannelCount);
            var tempBuffer = this.buffer.ConvertAll(x => (int)x);

            if (this.analogChannelCount >= 1)
            {
                sample.Voltages[0] = (tempBuffer[0] << 2) + ((tempBuffer[1] & 0xC0) >> 6);
            }

            if (this.analogChannelCount >= 2)
            {
                sample.Voltages[1] = ((tempBuffer[1] & 0x3F) << 4) + ((tempBuffer[2] & 0xF0) >> 4);
            }

            if (this.analogChannelCount >= 3)
            {
                sample.Voltages[2] = ((tempBuffer[2] & 0x0F) << 6) + ((tempBuffer[3] & 0xFC) >> 2);
            }

            if (this.analogChannelCount >= 4)
            {
                sample.Voltages[3] = ((tempBuffer[3] & 0x03) << 6) + ((tempBuffer[4] & 0xFF) >> 0);
            }

            if (this.analogChannelCount >= 5)
            {
                sample.Voltages[4] = (tempBuffer[5] << 2) + ((tempBuffer[6] & 0xC0) >> 6);
            }

            if (this.analogChannelCount >= 6)
            {
                sample.Voltages[5] = ((tempBuffer[6] & 0x3F) << 4) + ((tempBuffer[7] & 0xF0) >> 4);
            }

            for (var n = 0; n < this.analogChannelCount; n++)
            {
                if (sample.Voltages[n] == 1022)
                {
                    sample.Voltages[n] = 1023;
                }

                sample.Voltages[n] *= 5.0 / 1023;
            }

            return new SampleSequence()
            {
                Sample = sample
            };
        }

        /// <summary>
        /// Waits until stop message had been received
        /// </summary>
        public void WaitForStop()
        {
            while (this.isRunning)
            {
                this.stopMessageReceived.WaitOne(500);
            }
        }
    }
}