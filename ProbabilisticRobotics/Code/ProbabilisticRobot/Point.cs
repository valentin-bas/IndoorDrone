using System;
using System.Collections.Generic;
using System.Text;

namespace ProbabilisticRobot
{
	public class Point
	{
		public double X;
		public double Y;

		public Point(double x, double y)
		{
			X = x;
			Y = y;
		}

		public static Point Parse(string line)
		{
			string[] arr = line.Split(',');
			return new Point(double.Parse(arr[0]), double.Parse(arr[1]));
		}
	}
}
