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
using System.Globalization;

namespace ProbabilisticRobot
{
	public struct Angle
	{
		private static readonly double s_RadToDegree = 180.0/Math.PI;
		private static readonly double s_DegreeToRad = Math.PI/180.0;

		public static Angle FromRads(double rad)
		{
			return new Angle(rad);
		}

		public static Angle FromDegrees(double degree)
		{
			return new Angle(degree * s_DegreeToRad);
		}

		public static Angle operator +(Angle a1, Angle a2)
		{
			return new Angle(a1.Rads + a2.Rads);
		}

		public static Angle operator -(Angle a1, Angle a2)
		{
			return new Angle(a1.Rads - a2.Rads);
		}

		private Angle(double rad)
		{
			Rads = rad;
		}

		public double Degrees
		{
			get { return Rads * s_RadToDegree; }
		}

		public double Rads;

		public override string ToString()
		{
			return this.Degrees.ToString(CultureInfo.InvariantCulture) + " deg";
		}
	}
}
