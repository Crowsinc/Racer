using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.Assertions;

namespace Assets.Scripts.Utility
{
    public class Algorithms
    {

        /// <summary>
        /// Tests if a point is within the given bounds
        /// </summary>
        /// <param name="point">the point to test</param>
        /// <param name="min">the minimum point of the bound</param>
        /// <param name="max">the maximum point of the bound</param>
        /// <returns>true if within bounds, otherwise false</returns>
        public static bool WithinBounds(Vector2 point, Vector2 min, Vector2 max, float slack = 0.1f)
        {
            Assert.IsTrue(min.x <= max.x && min.y <= max.y);
            return !(point.x > max.x + slack || point.x < min.x - slack|| point.y > max.y + slack || point.y < min.y - slack);
        }

        /// <summary>
        /// Tests if a ponit is wtihin the given line segment
        /// </summary>
        /// <param name="point"> the point being tested </param>
        /// <param name="p1"> one end of the line segment </param>
        /// <param name="p2"> the other end of the line segment </param>
        /// <param name="slack"> the amount of slack allowed in floating point comparisons </param>
        /// <returns></returns>
        public static bool PointInSegment(Vector2 point, Vector2 p1, Vector2 p2, float slack = 0.001f)
        {
            // This uses a simple point on segment test, which checks that the vectors from p1-point
            // and p2-point have the same combined magnitude as the vector from p1 to p2. 
            return Mathf.Abs((p2 - p1).magnitude - ((p1 - point).magnitude + (p2 - point).magnitude)) < slack;
        }

        /// <summary>
        /// Tests whether a point is found within a polygon, or on its border
        /// </summary>
        /// <param name="point"> the point in question </param>
        /// <param name="polygon"> the vertices of the polygon in counter-clockwise order </param>
        /// <param name="slack"> the amount of slack allowed in floating point comparisons </param>
        /// <returns> True if the point is inside or on the polygon border, otherwise false</returns>
        public static bool PointInPolygon(Vector2 point, List<Vector2> polygon, float slack = 0.1f)
        {
            // Run a simple test to check if the point is within the AABB of the polygon
            Vector2 max = polygon[0], min = polygon[0];
            foreach (var p in polygon)
            {
                max = Vector2.Max(max, p);
                min = Vector2.Min(min, p);
            }

            // If outside of the AABB, then the point cannot be inside the polygon
            if (!WithinBounds(point, min, max))
                return false;

            // Perform a simple window number / angle summation test on the polygon. This is by 
            // far the slowest way of doing a PIP test, but it is numerically stable and easy
            // to implement. Past testing with the standard ray-intersection or even/odd rule 
            // algorithm lead to *many* issues due to their lack of numerical stability near
            // the edges of the polygon, where floating point error is significant.
            float totalAngle = 0.0f;

            var s1 = polygon[^1];
            foreach (var s2 in polygon)
            {
                // Test if the point is on the segment
                if (PointInSegment(point, s1, s2, slack))
                    return true;
                
                var r1 = s1 - point;
                var r2 = s2 - point;

                totalAngle += Vector2.SignedAngle(r1, r2);

                s1 = s2;
            }

            // A point is inside if the sum of all signed angles subtended 
            // by segments of the polygon and test point is greater than 0.
            return totalAngle > slack;
        }

        /// <summary>
        /// Updates all layers of an object and its children to the given mask
        /// </summary>
        /// <param name="obj"> The object to update </param>
        /// <param name="layerMask"> The mask for the layer to set </param>
        public static void SetLayers(GameObject obj, int layerMask)
        {
            var transforms = obj.GetComponentsInChildren<Transform>();
            foreach(var t in transforms)
                t.gameObject.layer = layerMask;
        }
    }

}


 
