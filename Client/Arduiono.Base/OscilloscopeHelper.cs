using System;
using System.Collections.Generic;
using System.IO.Ports;
using Arduino.Osci.Base.Logic;


namespace Arduiono.Osci.Base
{
	/// <summary>
	/// Defines several helper classes that can be used by application to 
	/// use the oscilloscope
	/// </summary>
	public static class OscilloscopeHelper
	{
		public const string TestingPortName = "Sinus Testing";
		
		public static List<string> GetPorts ()
		{
			var result = new List<string> ();
			
			result.Add (TestingPortName);
			
			// Gets the real serial port
			result.AddRange (SerialPort.GetPortNames ());
			
			return result;
		}
		
		public static IConnection CreateConnection ( string serialPortName)
		{
			if ( serialPortName == TestingPortName )
			{
				return new DummyConnection();
			}
			
			else{
				return new OscilloscopeConnection(serialPortName );
			}
		}
	}
}

