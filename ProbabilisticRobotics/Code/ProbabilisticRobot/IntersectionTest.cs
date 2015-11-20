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
	public static class IntersectionTest
	{
		/// <summary>
		/// Calculates whether a ray intersects a line segment
		/// </summary>
		/// <param name="rayOrigin">The origin of the ray</param>
		/// <param name="heading">The direction of the ray in degrees</param>
		/// <param name="point1">The start point of the line segment</param>
		/// <param name="point2">The end point of the line segment</param>
		/// <returns>True if the ray intersects with the line segment; otherwise false.</returns>
		public static bool IntersectsDegree(Point rayOrigin, Angle heading, Point point1, Point point2)
		{
			return IntersectsAtDistanceDegree(rayOrigin, heading, point1, point2) >= 0;
		}

		// This algorithm is inspired by this post: http://newsgroups.derkeiler.com/Archive/Comp/comp.graphics.algorithms/2006-08/msg00022.html
		/// <summary>
		/// Calculates whether a ray intersects a line segment
		/// </summary>
		/// <param name="rayOrigin">The origin of the ray</param>
		/// <param name="heading">The direction of the ray in degrees</param>
		/// <param name="point1">The start point of the line segment</param>
		/// <param name="point2">The end point of the line segment</param>
		/// <returns>The distance to the intersection; or -1 if the there is not intersection.</returns>
		public static double IntersectsAtDistanceDegree(Point rayOrigin, Angle heading, Point point1, Point point2)
		{
			/*
			As mentioned by others, the 2D problem may be solved by
			equating the parametric equations. The ray is P0+s*D0, where
			P0 is the ray origin, D0 is a direction vector (not necessarily
			unit length), and s >= 0. The segment is P1+t*D1, where P1
			and P1+D1 are the endpoints, and 0 <= t <= 1. Equating, you
			have P0+s*D0 = P1+t*D1. Define perp(x,y) = (y,-x). Then
			if Dot(perp(D1),D0) is not zero,
			s = Dot(perp(D1),P1-P0)/Dot(perp(D1),D0)
			t = Dot(perp(D0),P1-P0)/Dot(perp(D1),D0)
			The s and t value are where the *lines* containing the ray
			and segment intersect. The ray and segment intersect as
			long as s >= 0 and 0 <= t <= 1. If Dot(perp(D1),D0) is zero,
			then the ray and segment are parallel. If they do not lie on
			the same line, there is no intersection. If they do lie on the
			same line, you have to test for overlap, which is a 1D problem.
			*/


			// First we create a pseudo cross product to determine whether the ray and the line segment are parallel
			double cosTheta = Math.Cos(heading.Rads);
			double sinTheta = Math.Sin(heading.Rads);

			double deltaX = point2.X - point1.X;
			double deltaY = point2.Y - point1.Y;

			// are the lines parallel?
			if (Math.Abs(deltaY * cosTheta - deltaX * sinTheta) < Double.Epsilon)
			{
				// the lines are parallel
				return -1;
			}

			// s = Dot(perp(D1),P1-P0)/Dot(perp(D1),D0)
			// t = Dot(perp(D0),P1-P0)/Dot(perp(D1),D0)

			double s = (deltaY * (point1.X - rayOrigin.X) - deltaX * (point1.Y - rayOrigin.Y)) /
					   (deltaY * cosTheta - deltaX * sinTheta);

			double t = (sinTheta * (point1.X - rayOrigin.X) - cosTheta * (point1.Y - rayOrigin.Y)) /
					   (deltaY * cosTheta - deltaX * sinTheta);

			//Point intersection = new Point(rayOrigin.X + s * cosTheta, rayOrigin.Y + s * sinTheta);

			if (s < 0)
			{
				return -1;
			}
			if (t < 0 || t > 1)
			{
				return -1;
			}
			return s;
		}
	}
}
