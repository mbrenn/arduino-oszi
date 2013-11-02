using Oszillator.Logic.Messages;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oszillator.Logic
{
    public class OszillatorConnection : IConnection
    {
        private int channelCount = 0;

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
            this.SerialPort.BaudRate = 9600;
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
            this.connection.SetAnalogChannelCount(channelCount);
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
