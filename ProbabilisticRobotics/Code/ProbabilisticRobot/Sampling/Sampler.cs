﻿/****************************************************************************
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

namespace ProbabilisticRobot.Sampling
{
	public static class Sampler
	{
		public static readonly Random Random = new Random();

		internal static double GetRandomInRange(double b)
		{
			return Random.NextDouble() * 2 * b - b;
		}

		private static ISampler s_Sampler;

		public static void Initialize(ISampler sampler)
		{
			s_Sampler = sampler;
		}

		public static double Sample(double variance)
		{
			return s_Sampler.Sample(variance);
		}
	}
}
