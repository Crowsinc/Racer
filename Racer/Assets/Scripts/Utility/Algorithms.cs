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
                Plane plane = new Plane(Vector2.Perpendicular(segment), s1);
                
                Vector2 rayDir = Vector2.up;
                Ray ray = new Ray(point, rayDir);

                // Do not consider segments which are parallel to the ray direction
                // for intersect testing. In order to hit such a segment, we must also
                // hit the adjacent vertices. Adding in the collision of the parallel
                // segment itself, leads to an extra collision, hence incorrect results. 
                if (rayDir != segment.normalized && plane.Raycast(ray, out float distance))
                {
                    Vector2 intersectPoint = ray.GetPoint(distance);

                    // We have an intersection if the intersection point is found on the segment
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
