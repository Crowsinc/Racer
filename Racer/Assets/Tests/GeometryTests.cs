using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Assets.Scripts.Utility;

public class GeometryTests
{
    [Test]
    public void TestWithinBounds()
    {
        Vector2 min = new Vector2(0, 0);
        Vector2 max = new Vector2(1, 1);

        // Within borders and vertices
        Assert.IsTrue(Algorithms.WithinBounds(new Vector2(0, 0), min, max));
        Assert.IsTrue(Algorithms.WithinBounds(new Vector2(1, 1), min, max));
        Assert.IsTrue(Algorithms.WithinBounds(new Vector2(0.5f, 0.5f), min, max));
        
        // Slightly outside borders
        Assert.IsFalse(Algorithms.WithinBounds(new Vector2(-0.1f, 0), min, max, 0.05f));
        Assert.IsFalse(Algorithms.WithinBounds(new Vector2(0, -0.1f), min, max, 0.05f));
        Assert.IsFalse(Algorithms.WithinBounds(new Vector2(1.1f, 0), min, max, 0.05f));
        Assert.IsFalse(Algorithms.WithinBounds(new Vector2(0, 1.1f), min, max, 0.05f));
    }

    [Test]
    public void TestPointInSegment()
    {
        // Diagonal segment
        Vector2 p1 = new Vector2(0, 0);
        Vector2 p2 = new Vector2(1, 1);

        // Edge test
        Assert.IsTrue(Algorithms.PointInSegment(p1, p1, p2));
        Assert.IsTrue(Algorithms.PointInSegment(p2, p1, p2));

        // Inside Test
        Assert.IsTrue(Algorithms.PointInSegment(new Vector2(0.5f, 0.5f), p1, p2));
        Assert.IsTrue(Algorithms.PointInSegment(new Vector2(0.99f, 0.99f), p1, p2));

        // Outside Test
        Assert.IsFalse(Algorithms.PointInSegment(new Vector2(1.1f, 1.1f), p1, p2));
        Assert.IsFalse(Algorithms.PointInSegment(new Vector2(-0.1f, -0.1f), p1, p2));
        Assert.IsFalse(Algorithms.PointInSegment(new Vector2(0.5f, 0.6f), p1, p2));
        Assert.IsFalse(Algorithms.PointInSegment(new Vector2(2f, 2f), p1, p2));
    }

    [Test]
    public void TestPointInPolygon()
    {
        // Test Shape
        //  X-----------------X
        //  |                /
        //  |               /
        //  |              X------X
        //  |                     |
        //  X---------------------X

        List<Vector2> polygon = new();
        polygon.Add(new(0, 0));
        polygon.Add(new(3, 0));
        polygon.Add(new(3, 1));
        polygon.Add(new(2.5f, 1));
        polygon.Add(new(2.75f, 2));
        polygon.Add(new(0, 2));

        // Test inside
        Assert.IsTrue(Algorithms.PointInPolygon(new(0.5f, 0.5f), polygon));
        Assert.IsTrue(Algorithms.PointInPolygon(new(2.5f, 0.5f), polygon));

        // Test on edge
        Assert.IsTrue(Algorithms.PointInPolygon(new(3f, 0.5f), polygon));
        Assert.IsTrue(Algorithms.PointInPolygon(new(1.5f, 2), polygon));

        // Test on vertex
        Assert.IsTrue(Algorithms.PointInPolygon(polygon[2], polygon));
        Assert.IsTrue(Algorithms.PointInPolygon(polygon[4], polygon));

        // Test outside
        Assert.IsFalse(Algorithms.PointInPolygon(new(3f, 1.5f), polygon));
        Assert.IsFalse(Algorithms.PointInPolygon(new(-0.5f, 1f), polygon));
    }

}
