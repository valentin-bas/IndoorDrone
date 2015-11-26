#include <ZumoMotors.h>

ZumoMotors motors;
String CommandBuffer;

void setup()
{
  pinMode(13, OUTPUT);
  Serial.begin(9600);
  digitalWrite(13, HIGH);
}


void loop()
{
  if (Serial.available() > 0)
  {
    // read the incoming byte:
    char incomingByte = Serial.read();
    CommandBuffer += incomingByte;
    int endCmdIdx = CommandBuffer.indexOf('\n');
    if (endCmdIdx != -1)
    {
	  String Command = CommandBuffer.substring(0, endCmdIdx);
	  CommandBuffer = CommandBuffer.substring(endCmdIdx + 1);
	  if (Command.startsWith("l"))
	  {
                int speed = 0;
                //int argIdx = Command.indexOf(' ');
                //if (argIdx != -1)
                //  speed = Command.substring(argIdx + 1).toInt();
                speed = Command.substring(1).toInt();
		digitalWrite(13, LOW);
		motors.setLeftSpeed(speed);
	  }
          else if (Command.startsWith("r"))
	  {
                int speed = 0;
                //int argIdx = Command.indexOf(' ');
                //if (argIdx != -1)
                //  speed = Command.substring(argIdx + 1).toInt();
                speed = Command.substring(1).toInt();
		digitalWrite(13, LOW);
		motors.setRightSpeed(speed);
	  }
	  else
	  {
		digitalWrite(13, HIGH);
		motors.setLeftSpeed(0);
                motors.setRightSpeed(0);
	  }
      //motors.setRightSpeed(400);
    }
    //else
    //{
    //  digitalWrite(13, HIGH);
    //  motors.setLeftSpeed(0);
    //  motors.setRightSpeed(0);
    //}
  }
  //Serial.println("toto");
}