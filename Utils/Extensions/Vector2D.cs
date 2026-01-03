using System;
using Silk.NET.Maths;
using static System.Math;

namespace Utils.Extensions;

public static class Vector2DExtensions
{
    public static Vector2D<int> Ceil(this Vector2D<float> v)
    {
        return new Vector2D<int>((int)Ceiling(v.X), (int)Ceiling(v.Y));
    }
    
    public static Vector2D<int> Floor(this Vector2D<float> v)
    {
        return new Vector2D<int>((int)Math.Floor(v.X), (int)Math.Floor(v.Y));
    }
}
