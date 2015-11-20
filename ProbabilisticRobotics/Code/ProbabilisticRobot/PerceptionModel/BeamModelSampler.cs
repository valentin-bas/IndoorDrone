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
using Troschuetz.Random;
using System.Diagnostics;
using ProbabilisticRobot.Sampling;

namespace ProbabilisticRobot.PerceptionModel
{
	public class BeamModelSampler
	{
		private NormalDistribution m_NormalDistribution;
		private ExponentialDistribution m_ExponentialDistribution;

		public BeamModel BeamModel { get; private set; }
		private double EffectiveDistance { get; set; }

		internal BeamModelSampler(BeamModel beamModel, double realDistance)
		{
			this.BeamModel = beamModel;
			this.EffectiveDistance = realDistance > beamModel.MaxRange ? beamModel.MaxRange : realDistance;

			m_NormalDistribution = new NormalDistribution();
			m_NormalDistribution.Mu = realDistance;
			m_NormalDistribution.Sigma = BeamModel.MeasurementSigma;

			m_ExponentialDistribution = new ExponentialDistribution();
			m_ExponentialDistribution.Lambda = BeamModel.LambdaShort;
		}

		/// <summary>
		/// Creates a distance measurement sample under the assumption that the real distance
		/// is the provided distance value.
		/// </summary>
		/// <returns>A distance measurement sample.</returns>
		public double GetSample()
		{
			// First we figure out what probability distribution to use
			double random = Sampler.Random.NextDouble();

			if (random <= BeamModel.WeighingFactors.ZHitRaw)
			{
				return SamplePHit();
			}
			if (random <= BeamModel.WeighingFactors.ZHitRaw + BeamModel.WeighingFactors.ZShort)
			{
				return SamplePShort();
			}
			if (random <= BeamModel.WeighingFactors.ZHitRaw + BeamModel.WeighingFactors.ZShort + BeamModel.WeighingFactors.ZMax)
			{
				return BeamModel.MaxRange;
			}
			else
			{
				return SamplePRandom();
			}
		}

		private double SamplePHit()
		{
			double measurementSample;
			do
			{
				measurementSample = m_NormalDistribution.NextDouble();
			} while (measurementSample > BeamModel.MaxRange || measurementSample < 0);
			//Debug.WriteLine("SamplePHit: " + measurementSample);
			return measurementSample;
		}

		private double SamplePShort()
		{
			double measurementSample;
			do
			{
				measurementSample = m_ExponentialDistribution.NextDouble();
			} while (measurementSample > EffectiveDistance);
			//Debug.WriteLine("SamplePShort: " + measurementSample);
			return measurementSample;
		}

		private double SamplePRandom()
		{
			return Sampler.Random.NextDouble() * BeamModel.MaxRange;
		}
	}
}
