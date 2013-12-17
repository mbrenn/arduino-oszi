using Arduino.Base.Logic.Messages;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arduino.Base.Logic
{
    public class OszillatorConnection : IConnection
    {
        private int channelCount;
		
		public int ChannelCount 
		{
			get{ return this.channelCount; }
		}

        private ArduinoProtocol connection;

        public OszillatorConnection(int comPort)
        {
            this.ComPort = comPort;
        }

        public int ComPort { get; set; }

        public SerialPort SerialPort { get; set; }

        public void Setup(int channelCount)
        {
            this.channelCount = channelCount;

            this.SerialPort = new SerialPort();
            this.SerialPort.PortName = "COM" + this.ComPort.ToString();
            this.SerialPort.BaudRate = 38400;
            this.SerialPort.Parity = Parity.None;
            this.SerialPort.StopBits = StopBits.One;
            this.SerialPort.DataBits = 8;
            this.SerialPort.Handshake = Handshake.None;

            try
            {
                this.SerialPort.Open();
            }
            catch (UnauthorizedAccessException exc)
            {
                throw new InvalidOperationException(exc.Message);
            }

            this.connection = new ArduinoProtocol(this.SerialPort);
            this.connection.SetAnalogChannelCount(this.channelCount);
        }

        public void Start()
        {
            this.connection.SendStartCommand();
        }

        public void Stop()
        {
            this.connection.SendStopCommand();
            this.connection.WaitForStop();
            this.connection.SendStopConfirmationCommand();
        }

        public void Close()
        {
            this.SerialPort.Close();
        }

        public Sample Read()
        {
            var result = this.connection.Pull();
            var asSample = result as SampleSequence;

            if (asSample != null)
            {
                return asSample.Sample;
            }

            return null;
        }
    }
}
