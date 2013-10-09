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
        public OszillatorConnection(int comPort)
        {
            this.ComPort = comPort;
        }

        public int ComPort { get; set; }

        public SerialPort SerialPort { get; set; }

        public void Setup()
        {
            this.SerialPort = new SerialPort();
            this.SerialPort.PortName = "COM" + this.ComPort.ToString();
            this.SerialPort.BaudRate = 9600;
            this.SerialPort.Parity = Parity.None;
            this.SerialPort.StopBits = StopBits.One;
            this.SerialPort.DataBits = 8;
            this.SerialPort.Handshake = Handshake.None;
        }

        public void Start()
        {
            this.SerialPort.Open();
        }

        public void Stop()
        {
            this.SerialPort.Close();
        }

        public double Read()
        {
            var line = this.SerialPort.ReadLine();

            int value;
            if (int.TryParse(line, out value))
            {
                return ((double)value) * 5.0 / 1024.0;
            }

            return -1;
        }
    }
}
