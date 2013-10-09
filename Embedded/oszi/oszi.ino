int ledPin = 13;
int analogPin = 3;
bool viaUART = true;

void setup()
{
  pinMode(ledPin, OUTPUT);
  Serial.begin(9600);
  
  Serial.println("Depon.Net Oszillator");
}


void loop()
{
  int voltage = analogRead(analogPin);
  if(viaUART)
  {
    Serial.println(voltage);
  }
  
  if(voltage > 512)
  {
    digitalWrite(ledPin,HIGH);
  }
  else
  {
    digitalWrite(ledPin,LOW);
  }
  
  if (Serial.available() > 0)
  {
    int incoming = Serial.read();
    if (incoming == '1' )
    {
      viaUART = true;
    }
    else
    {
      viaUART = false;
    }
  }
}
