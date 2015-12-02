#include <ZumoMotors.h>
#include <Wire.h>
#include <LSM303.h>

#define CALIBRATION_SAMPLES 70  // Number of compass readings to take when calibrating
#define CRB_REG_M_2_5GAUSS 0x60 // CRB_REG_M value for magnetometer +/-2.5 gauss full scale
#define CRA_REG_M_220HZ    0x1C // CRA_REG_M value for magnetometer 220 Hz update rate

// Allowed deviation (in degrees) relative to target angle that must be achieved before driving straight
#define DEVIATION_THRESHOLD 5


//-------------------------------------------------------------------------------------------------


class CompassTools
{
private:
	float	_targetHeading;
	int		_speed;
	int		_turnBaseSpeed;
	LSM303*	_compass;

	// Converts x and y components of a vector to a heading in degrees.
	// This function is used instead of LSM303::heading() because we don't
	// want the acceleration of the Zumo to factor spuriously into the
	// tilt compensation that LSM303::heading() performs. This calculation
	// assumes that the Zumo is always level.
	template <typename T> float _Heading(LSM303::vector<T> v)
	{
		float x_scaled = 2.0*(float)(v.x - _compass->m_min.x) / (_compass->m_max.x - _compass->m_min.x) - 1.0;
		float y_scaled = 2.0*(float)(v.y - _compass->m_min.y) / (_compass->m_max.y - _compass->m_min.y) - 1.0;

		float angle = atan2(y_scaled, x_scaled) * 180 / M_PI;
		if (angle < 0)
			angle += 360;
		return angle;
	}

	// Yields the angle difference in degrees between two headings
	float _RelativeHeading(float heading_from, float heading_to)
	{
		float relative_heading = heading_to - heading_from;

		// constrain to -180 to 180 degree range
		if (relative_heading > 180)
			relative_heading -= 360;
		if (relative_heading < -180)
			relative_heading += 360;

		return relative_heading;
	}

public:

	CompassTools() : _speed(200), _turnBaseSpeed(100), _compass(NULL) {}

	bool	IsCalibrated() { return _compass != NULL; }
	void	SetTargetHeading(float angle) { _targetHeading = fmod(angle, 360); }

	// Average 10 vectors to get a better measurement and help smooth out
	// the motors' magnetic interference.
	float AverageHeading()
	{
		LSM303::vector<int32_t> avg = { 0, 0, 0 };

		for (int i = 0; i < 10; i++)
		{
			_compass->read();
			avg.x += _compass->m.x;
			avg.y += _compass->m.y;
		}
		avg.x /= 10.0;
		avg.y /= 10.0;

		// avg is the average measure of the magnetic vector.
		return _Heading(avg);
	}

	void Calibrate(LSM303* compass, ZumoMotors* motors)
	{
		// To calibrate the magnetometer, the Zumo spins to find the max/min
		// magnetic vectors. This information is used to correct for offsets
		// in the magnetometer data.
		motors->setLeftSpeed(_speed);
		motors->setRightSpeed(-_speed);

		LSM303::vector<int16_t> running_min = { 32767, 32767, 32767 }, running_max = { -32767, -32767, -32767 };

		for (int index = 0; index < CALIBRATION_SAMPLES; index++)
		{
			// Take a reading of the magnetic vector and store it in compass.m
			compass->read();

			running_min.x = min(running_min.x, compass->m.x);
			running_min.y = min(running_min.y, compass->m.y);

			running_max.x = max(running_max.x, compass->m.x);
			running_max.y = max(running_max.y, compass->m.y);

			delay(50);
		}

		motors->setLeftSpeed(0);
		motors->setRightSpeed(0);

		// Set calibrated values to compass.m_max and compass.m_min
		compass->m_max.x = running_max.x;
		compass->m_max.y = running_max.y;
		compass->m_min.x = running_min.x;
		compass->m_min.y = running_min.y;

		_compass = compass;
		_targetHeading = AverageHeading();
	}

	void Update(ZumoMotors* motors)
	{
		if (!IsCalibrated())
			return;

		float diffAngle = _RelativeHeading(AverageHeading(), _targetHeading);
		if (abs(diffAngle) > DEVIATION_THRESHOLD)
		{
			// To avoid overshooting, the closer the Zumo gets to the target
			// heading, the slower it should turn. Set the motor speeds to a
			// minimum base amount plus an additional variable amount based
			// on the heading difference.

			int speed = _speed * diffAngle / 180;

			if (speed < 0)
				speed -= _turnBaseSpeed;
			else
				speed += _turnBaseSpeed;

			motors->setSpeeds(speed, -speed);
		}
	}
};


//-------------------------------------------------------------------------------------------------


class CommandParser
{
private:
	String	_commandBuffer;

	void _HandleCommand(const String& Command, ZumoMotors* motors, LSM303* compass, CompassTools* compassTools)
	{
		digitalWrite(13, LOW);

		if (Command.startsWith("l")) //Left Motor Speed
		{
			int speed = Command.substring(1).toInt();
			motors->setLeftSpeed(speed);
		}
		else if (Command.startsWith("r")) //Right Motor Speed
		{
			int speed = Command.substring(1).toInt();
			motors->setRightSpeed(speed);
		}
		else if (Command.startsWith("b")) // Battery Voltage
		{
			unsigned int batteryVoltage = analogRead(1) * 5000L * 3 / 2 / 1023;
			Serial.print("b");
			Serial.println(batteryVoltage);
		}
		else if (Command.startsWith("c")) // Calibrate Compass
		{
			compassTools->Calibrate(compass, motors);
		}
		else if (Command.startsWith("h")) // Heading angle
		{
			int angle = Command.substring(1).toInt();
			if (angle < 0)
				angle = -angle;
			compassTools->SetTargetHeading((float)angle);
		}
		else
		{
			digitalWrite(13, HIGH);
			motors->setLeftSpeed(0);
			motors->setRightSpeed(0);
		}
	}

public:
	void Update(ZumoMotors* motors, LSM303* compass, CompassTools* compassTools)
	{
		if (Serial.available() > 0)
		{
			// read the incoming byte:
			char incomingByte = Serial.read();
			_commandBuffer += incomingByte;
			int endCmdIdx = _commandBuffer.indexOf('\n');
			if (endCmdIdx != -1)
			{
				String Command = _commandBuffer.substring(0, endCmdIdx);
				_commandBuffer = _commandBuffer.substring(endCmdIdx + 1);
				_HandleCommand(Command, motors, compass, compassTools);
			}
		}
	}
};


//-------------------------------------------------------------------------------------------------


ZumoMotors		g_Motors;
CommandParser	g_CommandHandler;
LSM303			g_Compass;
CompassTools	g_CompassTools;


//-------------------------------------------------------------------------------------------------


void setup()
{
	// Turn on the LED
	pinMode(13, OUTPUT);
	digitalWrite(13, HIGH);
	// Initiate Serial
	Serial.begin(9600);
	// Initiate the Wire library and join the I2C bus as a master
	Wire.begin();
	// Initiate LSM303
	g_Compass.init();
	g_Compass.enableDefault();
	g_Compass.writeReg(LSM303::CRB_REG_M, CRB_REG_M_2_5GAUSS); // +/- 2.5 gauss sensitivity to hopefully avoid overflow problems
	g_Compass.writeReg(LSM303::CRA_REG_M, CRA_REG_M_220HZ);    // 220 Hz compass update rate
}


void loop()
{
	g_CommandHandler.Update(&g_Motors, &g_Compass, &g_CompassTools);
	g_CompassTools.Update(&g_Motors);
}