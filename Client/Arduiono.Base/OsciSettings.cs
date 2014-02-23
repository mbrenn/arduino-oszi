using System;
namespace Arduiono.Osci.Base
{
    public class OsciSettings
    {
        public string SerialPort
        {
            get;
            set;
        }

        public int ChannelCount
        {
            get;
            set;
        }

        public OsciSettings()
        {
            this.ChannelCount = 1;
        }
    }
}