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
using System.ComponentModel;
using ProbabilisticRobot.Sampling;

namespace ProbabilisticRobot.PerceptionModel
{
	public class BeamModel : PropertyChangeNotifier
	{
		private static readonly double s_Sqrt2PI = Math.Sqrt(2 * Math.PI);

		private double m_MaxRange;
		private double m_MeasurementVariance;
		private double m_MeasurementSigma;
		private double m_LambdaShort;

		private double m_NormalDistributionFactor;

		public BeamModel(double maxRange, double measurementVariance, double lambdaShort, WeighingFactors weighingFactors)
		{
			MaxRange = maxRange;
			MeasurementVariance = measurementVariance;
			MeasurementSigma = Math.Sqrt(MeasurementVariance);
			LambdaShort = lambdaShort;
			WeighingFactors = weighingFactors;
			weighingFactors.PropertyChanged += new PropertyChangedEventHandler(OnWeighingFactorsChanged);
		}

		private void OnWeighingFactorsChanged(object sender, PropertyChangedEventArgs e)
		{
			OnPropertyChanged("WeighingFactors");
		}

		public double MaxRange
		{
			get { return m_MaxRange; }
			set
			{
				if (value != m_MaxRange)
				{
					m_MaxRange = value;
					OnPropertyChanged("MaxRange");
				}
			}
		}

		public double MeasurementVariance
		{
			get { return m_MeasurementVariance; }
			set
			{
				if (value != m_MeasurementVariance)
				{
					m_MeasurementVariance = value;
					double sigma = Math.Sqrt(m_MeasurementVariance);
					m_NormalDistributionFactor = 1.0 / s_Sqrt2PI / sigma;
					MeasurementSigma = sigma;
					OnPropertyChanged("MeasurementVariance");
				}
			}
		}

		public double MeasurementSigma
		{
			get { return m_MeasurementSigma; }
			set
			{
				if (value != m_MeasurementVariance)
				{
					m_MeasurementSigma = value;
					MeasurementVariance = m_MeasurementSigma * m_MeasurementSigma;
					OnPropertyChanged("MeasurementSigma");
				}
			}
		}

		public double LambdaShort
		{
			get { return m_LambdaShort; }
			set
			{
				if (value != m_LambdaShort)
				{
					m_LambdaShort = value;
					OnPropertyChanged("LambdaShort");
				}
			}
		}
		public WeighingFactors WeighingFactors { get; private set; }

		public double GetProbability(double measuredDistance, double realDistance, double deltaR)
		{
			//if (measuredDistance == this.MaxRange && realDistance > this.MaxRange)
			//{
			//    return 1.0;
			//}

			if (realDistance > this.MaxRange)
			{
				realDistance = MaxRange;
			}
			double probability =
				WeighingFactors.ZHit * GetPHit(measuredDistance, realDistance, deltaR) +
				WeighingFactors.ZShort * GetPShort(measuredDistance, realDistance, deltaR) +
				WeighingFactors.ZMax * GetPMax(measuredDistance, deltaR) +
				WeighingFactors.ZRand * GetPRandom(measuredDistance, deltaR);

			if (Double.IsNaN(probability))
			{
				Debug.WriteLine("NaN");
			}

			return probability;
		}

		private double GetPHit(double measuredDistance, double realDistance, double deltaR)
		{
			//Debug.WriteLine("In GetPHit");
			if (realDistance > MaxRange)
			{
				return 1 - Statistics.CommonFunctions.DistributionFunction(MaxRange, MaxRange, MeasurementVariance);
			}

			//double distanceDiff = measuredDistance - realDistance;
			//double probability = m_NormalDistributionFactor * Math.Exp(-0.5 * distanceDiff * distanceDiff / MeasurementVariance);

			//if (probability == 0.0)
			//{
			//    return 0;
			//}

			double lower = Statistics.CommonFunctions.DistributionFunction(measuredDistance - deltaR, realDistance, MeasurementVariance);
			double upper = Statistics.CommonFunctions.DistributionFunction(measuredDistance + deltaR, realDistance, MeasurementVariance);
			return upper - lower;
		}

		private double GetPShort(double measuredDistance, double realDistance, double deltaR)
		{
			//Debug.WriteLine("In GetPShort");
			if (measuredDistance > realDistance)
			{
				return 0.0;
			}

			//double probability = LambdaShort * Math.Exp(-LambdaShort * measuredDistance);
			//if (probability == 0.0)
			//{
			//    return 0;
			//}

			double lower = 1 - Math.Exp(-LambdaShort * (measuredDistance - deltaR));
			double upper = 1 - Math.Exp(-LambdaShort * (measuredDistance + deltaR));
			//Debug.WriteLine("GetPShort: " + upper - lower);
			return upper - lower;
		}

		private double GetPMax(double measuredDistance, double deltaR)
		{
			//Debug.WriteLine("In GetPMax");
			if (Math.Abs(MaxRange - measuredDistance) < deltaR)
			{
				return 1.0 / deltaR;
			}
			return 0.0;
		}

		private double GetPRandom(double measuredDistance, double deltaR)
		{
			//Debug.WriteLine("In GetPRandom");
			if (measuredDistance > MaxRange)
			{
				return 0.0;
			}
			return deltaR / MaxRange;
		}

		/// <summary>
		/// Creates a distance measurement sample under the assumption that the real distance
		/// is the provided distance value.
		/// </summary>
		/// <param name="realDistance"></param>
		/// <returns>A distance measurement sample.</returns>
		public double GetSample(double realDistance)
		{
			double effectiveDistance = realDistance > this.MaxRange ? this.MaxRange : realDistance;
			if (realDistance > this.MaxRange)
			{
				return this.MaxRange;
			}

			// First we figure out what probability distribution to use
			double random = Sampler.Random.NextDouble();

			if (random <= WeighingFactors.ZHitRaw)
			{
				return SamplePHit(effectiveDistance);
			}
			if (random <= WeighingFactors.ZHitRaw + WeighingFactors.ZShort)
			{
				return SamplePShort(effectiveDistance);
			}
			if (random <= WeighingFactors.ZHitRaw + WeighingFactors.ZShort + WeighingFactors.ZMax)
			{
				return MaxRange;
			}
			else
			{
				return SamplePRandom();
			}
		}

		private NormalDistribution m_NormalDistribution = new NormalDistribution();

		private double SamplePHit(double realDistance)
		{
			double measurementSample;
			//NormalDistribution normalDistribution = new NormalDistribution();
			m_NormalDistribution.Mu = realDistance;
			m_NormalDistribution.Sigma = MeasurementSigma;

			do
			{
				measurementSample = m_NormalDistribution.NextDouble();
			} while (measurementSample > MaxRange);
			return measurementSample;
		}

		private ExponentialDistribution m_ExponentialDistribution = new ExponentialDistribution();

		private double SamplePShort(double realDistance)
		{
			double measurementSample;
			//ExponentialDistribution exponentialDistribution = new ExponentialDistribution();
			m_ExponentialDistribution.Lambda = LambdaShort;

			do
			{
				measurementSample = m_ExponentialDistribution.NextDouble();
			} while (measurementSample > realDistance);
			return measurementSample;
		}

		private double SamplePRandom()
		{
			return Sampler.Random.NextDouble() * MaxRange;
		}

		/// <summary>
		/// Returns a <see cref="BeamModelSampler"/> instance which should be used when
		/// multiple samples need to be taken for the same real distance.
		/// </summary>
		/// <param name="realDistance"></param>
		/// <returns></returns>
		public BeamModelSampler GetSampler(double realDistance)
		{
			return new BeamModelSampler(this, realDistance);
		}
	}
}
