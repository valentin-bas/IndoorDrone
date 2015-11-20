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

namespace ProbabilisticRobot.Statistics
{
	public static class CommonFunctions
	{
		private static readonly double a = 8 * (Math.PI - 3) / (3 * Math.PI * (4 - Math.PI));

		/// <summary>
		/// Numeric approximation of the error function.
		/// For details see: http://en.wikipedia.org/wiki/Error_function
		/// </summary>
		public static double ErrorFunction(double x)
		{
			double xSquared = x * x;
			return Math.Sign(x) * Math.Sqrt(1 - Math.Exp(-xSquared * (4 / Math.PI + a * xSquared) / (1 + a * xSquared)));
		}

		private static readonly double s_SquareRoot2 = Math.Sqrt(2.0);

		/// <summary>
		/// For details see http://mathworld.wolfram.com/NormalDistribution.html and
		/// http://mathworld.wolfram.com/DistributionFunction.html.
		/// </summary>
		/// <returns></returns>
		public static double DistributionFunction(double x, double mean, double variance)
		{
			return 0.5 * (1 + ErrorFunction((x - mean) / (Math.Sqrt(variance) * s_SquareRoot2)));
		}
	}
}
