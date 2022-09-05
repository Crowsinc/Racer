using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Utility
{
    public class Algorithms
    {
        /// <summary>
        /// Tests whether a point is found within a polygon
        /// </summary>
        /// <param name="point"> the point in question </param>
        /// <param name="polygonPath"> the vertices of the polygon in counter-clockwise order </param>
        /// <returns> True if the point is inside, otherwise false</returns>
        public static bool PointInPolygon(Vector2 point, List<Vector2> polygonPath)
        {
            // The Point in polygon test is performed through the standard raycast crossing number test. 
            int intersections = 0;
            var s1 = polygonPath[^1];
            foreach (var s2 in polygonPath)
            {
                var segment = s2 - s1;
                var min = Vector2.Min(s1, s2);
                var max = Vector2.Max(s1, s2);
                var plane = new Plane(Vector2.Perpendicular(segment), s1);

                Ray ray = new Ray(point, Vector2.right);
                if (plane.Raycast(ray, out float distance))
                {
                    var intersectPoint = ray.GetPoint(distance);
                    if (intersectPoint.x >= min.x && intersectPoint.x <= max.x
                     && intersectPoint.y >= min.y && intersectPoint.y <= max.y)
                        intersections++;
                }

                s1 = s2;
            }

            // If there is an odd number of intersections,
            // then the point must be inside the polygon
            return intersections % 2 == 1;
        }

    }
}
