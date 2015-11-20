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

namespace ProbabilisticRobot.PerceptionModel
{
	public class WeighingFactors : PropertyChangeNotifier
	{
		private double m_ZHitRaw;
		private double m_ZShortRaw;
		private double m_ZMaxRaw;
		private double m_ZRandRaw;

		/// <summary>
		/// The passed in parameters is normalized so that their sum is 1.
		/// </summary>
		/// <param name="zHit"></param>
		/// <param name="zShort"></param>
		/// <param name="zMax"></param>
		/// <param name="zRand"></param>
		public WeighingFactors(double zHit, double zShort, double zMax, double zRand)
		{
			m_ZHitRaw = zHit;
			m_ZShortRaw = zShort;
			m_ZMaxRaw = zMax;
			m_ZRandRaw = zRand;

			Normalize();
		}

		public double ZHitRaw
		{
			get { return m_ZHitRaw; }
			set
			{
				if (value != m_ZHitRaw)
				{
					m_ZHitRaw = value;
					Normalize();
					OnPropertyChanged("ZHitRaw");
				}
			}
		}

		public double ZShortRaw
		{
			get { return m_ZShortRaw; }
			set
			{
				if (value != m_ZShortRaw)
				{
					m_ZShortRaw = value;
					Normalize();
					OnPropertyChanged("ZShortRaw");
				}
			}
		}

		public double ZMaxRaw
		{
			get { return m_ZMaxRaw; }
			set
			{
				if (value != m_ZMaxRaw)
				{
					m_ZMaxRaw = value;
					Normalize();
					OnPropertyChanged("ZMaxRaw");
				}
			}
		}

		public double ZRandRaw
		{
			get { return m_ZRandRaw; }
			set
			{
				if (value != m_ZRandRaw)
				{
					m_ZRandRaw = value;
					Normalize();
					OnPropertyChanged("ZRandRaw");
				}
			}
		}

		private void Normalize()
		{
			double total = ZHitRaw + ZShortRaw + ZMaxRaw + ZRandRaw;

			ZHit = ZHitRaw / total;
			ZShort = ZShortRaw / total;
			ZMax = ZMaxRaw / total;
			ZRand = ZRandRaw / total;
		}

		public double ZHit { get; private set; }
		public double ZShort { get; private set; }
		public double ZMax { get; private set; }
		public double ZRand { get; private set; }
	}
}
