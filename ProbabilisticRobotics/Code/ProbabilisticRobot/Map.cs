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
using System.IO;
using System.Globalization;

namespace ProbabilisticRobot
{
	public class Map
	{
		public static Map ParseMultiLinePointsString(string pointsString)
		{
			List<Point> points = new List<Point>();
			StringReader stringReader = new StringReader(pointsString);
			string line;
			while (null != (line = stringReader.ReadLine()))
			{
				if (String.IsNullOrEmpty(line))
				{
					continue;
				}
				line = line.Trim();
				if (line.StartsWith("#", StringComparison.Ordinal) || line.Length == 0)
				{
					// comment line or empty line
					continue;
				}
				else
				{
					points.Add(Point.Parse(line));
				}
			}

			return new Map(points.ToArray());
		}

		public Map(Point[] points)
		{
			this.Points = points;
			InitializeMinsAndMaxs();
		}

		private void InitializeMinsAndMaxs()
		{
			this.XMin = double.MaxValue;
			this.XMax = double.MinValue;
			this.YMin = double.MaxValue;
			this.YMax = double.MinValue;

			foreach(Point point in Points)
			{
				if (point.X < XMin)
				{
					XMin = point.X;
				}
				if (point.X > XMax)
				{
					XMax = point.X;
				}
				if (point.Y < YMin)
				{
					YMin = point.Y;
				}
				if (point.Y > YMax)
				{
					YMax = point.Y;
				}
			}
		}

		public Point[] Points { get; private set; }
		public double XMin { get; private set; }
		public double XMax { get; private set; }
		public double YMin { get; private set; }
		public double YMax { get; private set; }

		public double Width
		{
			get { return XMax - XMin; }
		}

		public double Height
		{
			get { return YMax - YMin; }
		}

		/// <summary>
		/// The algorithm is described here: http://www.ecse.rpi.edu/Homepages/wrf/Research/Short_Notes/pnpoly.html
		/// </summary>
		/// <param name="point">Point to check</param>
		public bool IsInside(Point point)
		{
			return IsInside(point.X, point.Y);
		}

		/// <summary/>
		public bool IsInside(double x, double y)
		{
			bool isIn = false;
			for (int i = 0; i < Points.Length; i++)
			{
				int j = (i + Points.Length - 1) % Points.Length;
				if (
					((Points[i].Y > y) != (Points[j].Y > y)) &&
					(x < (Points[j].X - Points[i].X) * (y - Points[i].Y) / (Points[j].Y - Points[i].Y) + Points[i].X)
				   )
				{
					isIn = !isIn;
				}
			}
			return isIn;
		}

		public double GetClosestWallDistance(Point rayOrigin, Angle heading)
		{
			double closestDistance = Double.MaxValue;

			for (int i = 0; i < this.Points.Length; i++)
			{
				Point point1 = this.Points[i];
				Point point2 = this.Points[(i + 1) % this.Points.Length];

				double distance = IntersectionTest.IntersectsAtDistanceDegree(rayOrigin, heading, point1, point2);
				if (distance < 0)
				{
					continue;
				}
				if (distance < closestDistance)
				{
					closestDistance = distance;
				}
			}

			return closestDistance;
		}
	}
}
