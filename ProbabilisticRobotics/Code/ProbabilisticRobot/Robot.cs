/****************************************************************************
Copyright (c) 2010 Dr. Rainer Hessmer

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
 ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace ProbabilisticRobot
{
	public class Robot
	{
		public double Radius { get; private set; }
		public Angle[] SensorDirections { get; private set; }

		public Robot()
		{
			this.Radius = 0.25;
			this.SensorDirections = new Angle[]
			{
				Angle.FromDegrees(-120),
				Angle.FromDegrees(-60),
				Angle.FromDegrees(0),
				Angle.FromDegrees(60),
				Angle.FromDegrees(120)
			};
		}

		public double[] GetMeasurements(Pose pose, MonteCarloLocalization monteCarloLocalization)
		{
			double[] measurements = new double[this.SensorDirections.Length];
			for(int i = 0; i < this.SensorDirections.Length; i++)
			{
				Angle heading = pose.Heading + this.SensorDirections[i];
				double realDistanceToWall = monteCarloLocalization.Map.GetClosestWallDistance(pose.Location, heading);
				// Now we sample the perception model to simulate a real measurement
				measurements[i] = monteCarloLocalization.BeamModel.GetSample(realDistanceToWall);	 
			}
			return measurements;
		}
	}
}
