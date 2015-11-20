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

namespace ProbabilisticRobot
{
	public class Pose
	{
		private static readonly double s_TwoPI = 2 * Math.PI;

		/// <summary>
		/// Nomalizes the provided angle (degree) so that it falls in the range
		/// (-180, 180]
		/// </summary>
		/// <param name="angle">The angle to normalize in degree.</param>
		/// <returns></returns>
		public static Angle NormalizeAngle(Angle angle)
		{
			double factor = Math.Floor(angle.Rads / s_TwoPI);
			double normalizedAngleRad = angle.Rads - factor * s_TwoPI;
			if (normalizedAngleRad < 0)
			{
				normalizedAngleRad = angle.Rads + s_TwoPI;
			}

			if (normalizedAngleRad > Math.PI)
			{
				normalizedAngleRad = normalizedAngleRad - s_TwoPI;
			}
			return Angle.FromRads(normalizedAngleRad);
		}

		public double X
		{
			get { return this.Location.X; }
			set { this.Location = new Point(value, this.Y); }
		}

		public double Y
		{
			get { return this.Location.Y; }
			set { this.Location = new Point(this.X, value); }
		}

		private Angle m_Heading;
		public Angle Heading
		{
			get { return m_Heading; }
			set { m_Heading = NormalizeAngle(value); }
		}

		public Pose(Point location, Angle heading)
		{
			this.Location = location;
			this.Heading = heading;
		}

		public Pose(double x, double y, Angle heading)
		{
			this.Location = new Point(x, y);
			this.Heading = heading;
		}

		public Point Location { get; set; }

		public override string ToString()
		{
			return String.Format("x={0}, y={1}, heading={2}", this.X, this.Y, this.Heading.Degrees);
		}
	}
}
