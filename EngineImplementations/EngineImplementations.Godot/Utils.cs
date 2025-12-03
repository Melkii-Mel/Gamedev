using System;
using Godot;
using Silk.NET.Maths;

namespace EngineImplementations.Godot;

public static class Utils
{
    public static T As<T>(object obj)
    {
        if (obj is not T t)
        {
            throw new NotSupportedException($"Can't cast object to target type {typeof(T).Name}.");
        }

        return t;
    }

    public static Rectangle<float> FromRect2(Rect2 rect2)
    {
        return new Rectangle<float>(rect2.Position.x, rect2.Position.y, rect2.Size.x, rect2.Size.y);
    }

    public static void FromRectangle(ref float left, ref float top, ref float right, ref float bottom,
        Rectangle<float> rectangle)
    {
        left = rectangle.Origin.X;
        right = rectangle.Origin.X + rectangle.Size.X;
        top = rectangle.Origin.Y;
        bottom = rectangle.Origin.Y + rectangle.Size.Y;
    }

    public static Rect2 ToRect2(Rectangle<float> rect)
    {
        return new Rect2(rect.Origin.X, rect.Origin.Y, rect.Size.X, rect.Size.Y);
    }

    public static Rectangle<float> ToRectangle(float left, float top, float right, float bottom)
    {
        return new Rectangle<float>(left, top, right - left, top - bottom);
    }

    public static Vector2D<float> FromVector2(Vector2 vector2)
    {
        return new Vector2D<float>(vector2.x, vector2.y);
    }

    public static Vector2 ToVector2(Vector2D<float> vector2d)
    {
        return new Vector2(vector2d.X, vector2d.Y);
    }
}