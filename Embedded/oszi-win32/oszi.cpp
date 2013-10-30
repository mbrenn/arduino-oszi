// oszi.cpp : Definiert den Einstiegspunkt für die Konsolenanwendung.
//

#include "stdafx.h"

unsigned char convertToStream(unsigned int* pChannels, int nChannels, unsigned char* targetBuffer);

int _tmain(int argc, _TCHAR* argv[])
{
	unsigned char buffer[8];
	unsigned int channels[6];

	channels[0] = 1022;
	channels[1] = 1022;
	channels[2] = 1022;
	channels[3] = 1022;
	channels[4] = 1022;
	channels[5] = 1022;

	unsigned char result = convertToStream(channels, 6, buffer);

	for (int n = 0; n < result; n++)
	{
		printf("%d\r\n", buffer[n]);
	}
	
	_getwch();

	return 0;
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