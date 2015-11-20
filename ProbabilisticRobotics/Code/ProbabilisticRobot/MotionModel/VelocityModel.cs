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
using ProbabilisticRobot.Sampling;

namespace ProbabilisticRobot.MotionModel
{
	public class VelocityModel
	{
		public VelocityModel()
			: this(0.01, 0.001, 0.001, 0.01, 0.01, 0.01)
		{
		}

		public VelocityModel(double a1, double a2, double a3, double a4, double a5, double a6)
		{
			A1 = a1;
			A2 = a2;
			A3 = a3;
			A4 = a4;
			A5 = a5;
			A6 = a6;
		}

		public double A1 { get; set; }
		public double A2 { get; set; }
		public double A3 { get; set; }
		public double A4 { get; set; }
		public double A5 { get; set; }
		public double A6 { get; set; }

		public Pose Sample(Pose currentPose, DriveCommand driveCommand)
		{
			if (driveCommand == null)
			{
				return currentPose;
			}
			return Sample(currentPose, driveCommand.Velocity, driveCommand.Duration);
		}

		public Pose Sample(Pose currentPose, Velocity commandedVelocity, TimeSpan deltaT)
		{
			double vSquared = commandedVelocity.V * commandedVelocity.V;
			double omegaRadSquared = commandedVelocity.Omega.Rads * commandedVelocity.Omega.Rads;

			double vVariance = A1 * +vSquared + A2 * omegaRadSquared;
			double vNoisy = commandedVelocity.V + Sampler.Sample(vVariance);

			double omegaVariance = A3 * +vSquared + A4 * omegaRadSquared;
			double omegaRadNoisy = commandedVelocity.Omega.Rads + Sampler.Sample(omegaVariance);

			double gammaVariance = A5 * +vSquared + A6 * omegaRadSquared;
			double gammaNoisy = Sampler.Sample(gammaVariance);

			double vOverOmegaNoisy = vNoisy / omegaRadNoisy;

			double newX = currentPose.X - vOverOmegaNoisy * Math.Sin(currentPose.Heading.Rads) + vOverOmegaNoisy * Math.Sin(currentPose.Heading.Rads + omegaRadNoisy * deltaT.TotalSeconds);
			double newY = currentPose.Y + vOverOmegaNoisy * Math.Cos(currentPose.Heading.Rads) - vOverOmegaNoisy * Math.Cos(currentPose.Heading.Rads + omegaRadNoisy * deltaT.TotalSeconds);
			double newThetaRad = currentPose.Heading.Rads + omegaRadNoisy * deltaT.TotalSeconds + gammaNoisy * deltaT.TotalSeconds;

			Pose newPose = new Pose(newX, newY, Angle.FromRads(newThetaRad));
			return newPose;
		}

		/// <summary>
		/// Calaculates the exact (noise or error free) pose
		/// </summary>
		public Pose MoveExact(Pose currentPose, DriveCommand driveCommand)
		{
			if (driveCommand == null)
			{
				return currentPose;
			}
			return MoveExact(currentPose, driveCommand.Velocity, driveCommand.Duration);
		}

		/// <summary>
		/// Calaculates the exact (noise or error free) pose
		/// </summary>
		public Pose MoveExact(Pose currentPose, Velocity commandedVelocity, TimeSpan deltaT)
		{
			double vOverOmegaRad = commandedVelocity.V / commandedVelocity.Omega.Rads;

			double newX, newY, newThetaRad;

			if (!Double.IsInfinity(vOverOmegaRad))
			{
				newX = currentPose.X - vOverOmegaRad * Math.Sin(currentPose.Heading.Rads) + vOverOmegaRad * Math.Sin(currentPose.Heading.Rads + commandedVelocity.Omega.Rads * deltaT.TotalSeconds);
				newY = currentPose.Y + vOverOmegaRad * Math.Cos(currentPose.Heading.Rads) - vOverOmegaRad * Math.Cos(currentPose.Heading.Rads + commandedVelocity.Omega.Rads * deltaT.TotalSeconds);
				newThetaRad = currentPose.Heading.Rads + commandedVelocity.Omega.Rads * deltaT.TotalSeconds;
			}
			else
			{
				// Omega is too small; i.e., we move in a straight line.
				newThetaRad = currentPose.Heading.Rads;

				double distanceTraveled = commandedVelocity.V * deltaT.TotalSeconds;

				newX = currentPose.X + distanceTraveled * Math.Cos(currentPose.Heading.Rads);
				newY = currentPose.Y + distanceTraveled * Math.Sin(currentPose.Heading.Rads);
			}

			Pose newPose = new Pose(newX, newY, Angle.FromRads(newThetaRad));
			return newPose;
		}
	}
}
