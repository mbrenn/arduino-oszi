int ledPin = 13;                   // Defines the PIN for the LED
byte numberAnalogChannels = 1;     // Defines the number of channels to be read

// Stores the state whether the server is currently running
enum ServerStates
{
  Stop = 1,
  Stopping = 2,
  Running = 3
};

ServerStates stateServerRun = Stop;       // true, if data acquisition is running
byte inputState = 0;               // 0 for start of sequence, 1 if next symbol is defining the number of analog channels

byte frameCount = 0;               // Number of frames to be sent

unsigned char convertToStream(unsigned int* pChannels, int nChannels, unsigned char* targetBuffer);

void setup()
{
  pinMode(ledPin, OUTPUT);
  Serial.begin(38400);
  
  Serial.println("Depon.Net Oszillator");
  
  inputState = 0;
  frameCount = 0;
  stateServerRun = Stop;
  digitalWrite(ledPin,LOW);
}

void loop()
{
  loopOszi();
}

// Performs the looping
void loopOszi()
{
  if (Serial.available() > 0)
  {
    int incoming = Serial.read();
    if (inputState == 0)
    {      
      if (incoming == 'g')
      {
        if(stateServerRun != Stop)
        {
          sendErrorSequence(0x06);
        }
        else
        {
          stateServerRun = Running;
          sendSyncSequence();
          digitalWrite(ledPin,HIGH);
        }
      }
      else if (incoming == 's')
      {
        if(stateServerRun != Running)
        {
          sendErrorSequence(0x07);
        }
        else
        {
          stateServerRun = Stopping;
          digitalWrite(ledPin,LOW);
        }
      }      
      else if (incoming == 't')
      {
        if(stateServerRun != Stopping)
        {
          sendErrorSequence(0x05);
        }
        else
        {
          stateServerRun = Stop;
        }
      }
      else if (incoming == 'a')
      {
        if(stateServerRun != Stop)
        {
          sendErrorSequence(0x04);
        }
        else
        {
          // Waiting for number of channels
          inputState = 1;
        }
      }
      else
      {
        sendErrorSequence(0x01);
      }
    }    
    else if (inputState == 1)
    {
      if (incoming >= '1' && incoming <= '6')
      {
        if(stateServerRun == Stop)
        {
          numberAnalogChannels = incoming - '0';
        }
        else
        {
          sendErrorSequence(0x02);
        }
      }
      else
      {
        sendErrorSequence(0x03);
      }
      
      // Reset input state
      inputState = 0;
    }    
  }   

  if(stateServerRun == Running)
  {
    unsigned int channels[6];
    for (int n = 0; n < numberAnalogChannels; n++)
    {
      channels[n] = analogRead(n);
    }
    
    sendStream(channels, numberAnalogChannels);
    
    frameCount++;
    if(frameCount == 100)
    {
      sendSyncSequence();
      frameCount = 0;
    }
  }
  else if (stateServerRun == Stopping)
  {
    // Sends out stop sequences until client has confirmed by 't'
    sendStopStreamSequence();
  }
}


/*
 * Stores the functions for sending and receiving
 */
void sendSyncSequence()
{
  byte array[] = { 0xFF, 0xFF, 0xFF };
  Serial.write(array, 3);
}

void sendErrorSequence(byte error)
{
  byte array[] = { 0xFF, 0xFF, 0xFE, error };
  Serial.write(array, 4);
}

void sendStopStreamSequence()
{
  byte array[] = { 0xFF, 0xFF, 0x01 };
  Serial.write(array, 3);
}

void sendStream(unsigned int* pChannels, byte numberChannels)
{
  // First of all, modify 1023 to 1022
  for(int n = 0; n < numberChannels; n++)
  {
    if(pChannels[n] == 1023)
    {
      pChannels[n] = 1022;
    }
  }
  
  // Now, combine the values
  unsigned char data[8];
  unsigned char numberBytes = convertToStream(pChannels, numberChannels, data);
  
  Serial.write(data, numberBytes);  
}

unsigned char convertToStream(unsigned int* pChannels, int nChannels, unsigned char* targetBuffer)
{
	int size = 0;
	// Target buffer has to be at least 6 bytes
	if (nChannels > 0)
	{
		// Allocate 1 value
		targetBuffer[0] = (pChannels[0] & 0x3FC) >> 2;				// All 8 bits
		targetBuffer[1] = (pChannels[0] & 0x03) << 6;				// Just the first two bits

		size = 2;
	}

	if (nChannels > 1)
	{
		// At least 2 values
		targetBuffer[1] = targetBuffer[1] + ((pChannels[1] & 0x3F0) >> 4);	// First 6 bytes 
		targetBuffer[2] = (pChannels[1] & 0x0F) << 4;						// Last 4 bytes

		size = 3;
	}

	
	if (nChannels > 2)
	{
		// At least 3 values
		targetBuffer[2] = targetBuffer[2] + ((pChannels[2] & 0x3C0) >> 6);	// First 4 bytes
		targetBuffer[3] = (pChannels[2] & 0x3F) << 2;							// Next 6 bytes

		size = 4;
	}

	if (nChannels > 3)
	{
		// At least 4 values
		targetBuffer[3] = targetBuffer[3] + ((pChannels[3] & 0x300) >> 8);
		targetBuffer[4] = pChannels[3] & 0xFF;

		size = 5;
	}

	if (nChannels > 4)
	{
		// Allocate 5 value
		targetBuffer[5] = (pChannels[4] & 0x3FC) >> 2;				// All 8 bits
		targetBuffer[6] = (pChannels[4] & 0x03) << 6;				// Just the first two bits

		size = 7;
	}

	if (nChannels > 5)
	{
		// At least 2 values
		targetBuffer[6] = targetBuffer[6] + ((pChannels[5] & 0x3F0) >> 4);	// First 6 bytes 
		targetBuffer[7] = (pChannels[5] & 0x0F) << 4;						// Last 4 bytes

		size = 8;
	}

	return size;
}
