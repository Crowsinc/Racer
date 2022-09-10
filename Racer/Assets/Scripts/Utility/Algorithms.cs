using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

namespace Assets.Scripts.Utility
{
    public class Algorithms
    {
        /// <summary>
        /// Tests if three points are collinear
        /// </summary>
        /// <param name="a"> point a </param>
        /// <param name="b"> point b </param>
        /// <param name="c"> point c </param>
        /// <returns> true if the points are collinear </returns>
        public static bool Collinear(Vector2 a, Vector2 b, Vector2 c)
        {
            // This is a standard collinearity test, which forms vectors from a to b,
            // and b to c, then checks if their cross product is zero (or close to).
            return Mathf.Abs((c.y - b.y) * (b.x - a.x) - (b.y - a.y) * (c.x - b.x)) <= 0.001;
        }


        /// <summary>
        /// Tests if a point is within the given bounds
        /// </summary>
        /// <param name="point">the point to test</param>
        /// <param name="min">the minimum point of the bound</param>
        /// <param name="max">the maximu point of the bound</param>
        /// <returns>true if within bounds, otherwise false</returns>
        public static bool WithinBounds(Vector2 point, Vector2 min, Vector2 max)
        {
            return !(point.x > max.x || point.x < min.x || point.y > max.y || point.y < min.y);
        }

        /// <summary>
        /// Tests whether a point is found within a polygon, or on its border
        /// </summary>
        /// <param name="point"> the point in question </param>
        /// <param name="polygon"> the vertices of the polygon in counter-clockwise order </param>
        /// <returns> True if the point is inside or on the polygon border, otherwise false</returns>
        public static bool PointInPolygon(Vector2 point, List<Vector2> polygon)
        {
            // Run a simple test to check if the point is within the AABB of the polygon
            Vector2 max = polygon[0], min = polygon[0];
            foreach (var p in polygon)
            {
                max = Vector2.Max(max, p);
                min = Vector2.Min(min, p);

                // If the given point is equal to (or close to) a vertex, then it must be inside
                if ((point - p).magnitude < 0.001)
                    return true;
            }

            // If outside of the AABB, then the point cannot be inside the polygon
            if (!WithinBounds(point, min, max))
                return false;

            // Perform a standard ray-intersection point in polygon test
            bool inside = false;
            var s1 = polygon[^1];
            foreach (var s2 in polygon)
            {
                var segment = s2 - s1;
                var segMin = Vector2.Min(s1, s2);
                var segMax = Vector2.Max(s1, s2);

                // The point is on the segment, so is inside the polygon
                if (Collinear(s1, s2, point) && WithinBounds(point, segMin, segMax))
                    return true;

                
                // Define a plane parallel to the segment and shoot a rightwards 
                // ray from our point into it. If the ray hits the plane within
                // the segment, then the line intersects the segment. 
                Plane plane = new Plane(Vector2.Perpendicular(segment), s1);
                Ray ray = new Ray(point, Vector2.right);

                if(plane.Raycast(ray, out float distance))
                {
                    Vector2 intersectPoint = ray.GetPoint(distance);

                    if (WithinBounds(intersectPoint, segMin, segMax))
                        inside = !inside;
                }

                s1 = s2;
            }
            return inside;
        }

      
    }
}


 
