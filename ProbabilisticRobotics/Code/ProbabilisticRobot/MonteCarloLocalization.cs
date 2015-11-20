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
using ProbabilisticRobot.MotionModel;
using ProbabilisticRobot.PerceptionModel;
using System.Diagnostics;
using ProbabilisticRobot.Sampling;

namespace ProbabilisticRobot
{
	public class MonteCarloLocalization
	{
		private double[] m_AggregatedWeights;
		private Pose[] m_TempNewParticles;
		private Pose[] m_Swap;

		public MonteCarloLocalization(Map map, Robot robot, int particleCount, VelocityModel velocityModel, BeamModel beamModel)
		{
			this.Map = map;
			this.Robot = robot;
			this.ParticleCount = particleCount;
			this.VelocityModel = velocityModel;
			this.BeamModel = beamModel;

			this.DeltaR = 0.1;
			this.DeltaTheta = Angle.FromDegrees(5.0);

			this.Particles = new Pose[this.ParticleCount];
			m_AggregatedWeights = new double[this.ParticleCount];
			m_TempNewParticles = new Pose[this.ParticleCount];

			InitializeParticles();
		}

		public Map Map { get; private set; }
		public Robot Robot { get; private set; }
		public int ParticleCount { get; private set; }
		public Pose[] Particles { get; private set; }
		public VelocityModel VelocityModel { get; private set; }
		public BeamModel BeamModel { get; private set; }
		public double DeltaR { get; private set; }
		public Angle DeltaTheta { get; private set; }

		public void InitializeParticles()
		{
			for (int i = 0; i < this.ParticleCount; i++)
			{
				this.Particles[i] = CreateRandomPose();
			}
		}

		private Pose CreateRandomPose()
		{
			int angleBucketCount = (int)(360 / DeltaTheta.Degrees);
			Angle heading = Angle.FromDegrees(Sampler.Random.Next(angleBucketCount) * DeltaTheta.Degrees);

			double x, y;
			do
			{
				x = this.Map.XMin + Sampler.Random.NextDouble() * (this.Map.XMax - this.Map.XMin);
				y = this.Map.YMin + Sampler.Random.NextDouble() * (this.Map.YMax - this.Map.YMin);
			}
			while (!this.Map.IsInside(x, y));

			return new Pose(x, y, heading);
		}

		public void Update(DriveCommand driveCommand, double[] measurements)
		{
			double weightsSum = 0;
			for (int i = 0; i < this.ParticleCount; i++)
			{
				Pose originalParticle = this.Particles[i];
				Pose movedParticle = this.VelocityModel.Sample(originalParticle, driveCommand);

				while(!this.Map.IsInside(movedParticle.Location))
				{
					// The particle moved out of the map. We replace it with a new random particle
					originalParticle = CreateRandomPose();
					movedParticle = this.VelocityModel.Sample(originalParticle, driveCommand);
				}

				double weight = CalculateWeight(movedParticle, measurements);

				weightsSum += weight;
				m_AggregatedWeights[i] = weightsSum;

				this.Particles[i] = movedParticle;
			}

			Debug.WriteLine(weightsSum);
			DrawParticles(weightsSum);
		}

		private double CalculateWeight(Pose pose, double[] measurements)
		{
			double weight = 1.0;
			for (int i = 0; i < this.Robot.SensorDirections.Length; i++)
			{
				Angle heading = pose.Heading + this.Robot.SensorDirections[i];
				double distanceToWall = this.Map.GetClosestWallDistance(pose.Location, heading);

				weight *= this.BeamModel.GetProbability(measurements[i], distanceToWall, DeltaR);
			}
			return weight;
		}

		private void DrawParticles(double sumOfWeights)
		{
			for (int i = 0; i < this.ParticleCount; i++)
			{
				double random = Sampler.Random.NextDouble() * sumOfWeights;
				m_TempNewParticles[i] = DrawParticle(random);
			}

			m_Swap = this.Particles;
			this.Particles = m_TempNewParticles;
			m_TempNewParticles = m_Swap;
		}

		private Pose DrawParticle(double random)
		{
			// binary search for the index; see http://www.dreamincode.net/code/snippet1835.htm
			int lowIndex = 0;
			int highIndex = this.ParticleCount;

			//loop while the low index is less or equal to the high index
			while (lowIndex <= highIndex)
			{
				//get the middle point in the array
				int midNum = (lowIndex + highIndex) / 2;

				double rangeLower = midNum - 1 < 0 ? 0 : m_AggregatedWeights[midNum - 1];
				double rangeUpper = m_AggregatedWeights[midNum];

				//now start checking the values
				if (random < rangeLower)
				{
					//search value is lower than this index of our array
					//so set the high number equal to the middle number - 1
					highIndex = midNum - 1;
				}
				else if (random >= rangeUpper)
				{
					//search value is higher than this index of our array
					//so set the low number to the middle number + 1
					lowIndex = midNum + 1;
				}
				else if (random >= rangeLower && random < rangeUpper)
				{
					//we found a match
					//Debug.WriteLine(rangeLower + " - " + rangeUpper + ": " + random);
					return this.Particles[midNum];
				}
			}

			throw new Exception("Program error");
		}
	}
}
