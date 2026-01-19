using System;
using Silk.NET.Maths;

namespace Utils.Noises;

public struct AnyVector<T> where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
{
    public Vector4D<T> Base { get; }

    public AnyVector(T vector1D)
    {
        Base = new Vector4D<T>(vector1D, vector1D, default, default);
    }

    public AnyVector(Vector2D<T> vector2D)
    {
        Base = new Vector4D<T>(vector2D, default, default);
    }

    public AnyVector(Vector3D<T> vector3D)
    {
        Base = new Vector4D<T>(vector3D, default);
    }

    public AnyVector(Vector4D<T> vector4D)
    {
        Base = vector4D;
    }

    public static implicit operator AnyVector<T>(Vector2D<T> vector2D)
    {
        return new AnyVector<T>(vector2D);
    }

    public static implicit operator AnyVector<T>(Vector3D<T> vector2D)
    {
        return new AnyVector<T>(vector2D);
    }

    public static implicit operator AnyVector<T>(Vector4D<T> vector2D)
    {
        return new AnyVector<T>(vector2D);
    }

    public static implicit operator AnyVector<T>(T vector1D)
    {
        return new AnyVector<T>(vector1D);
    }

    public static explicit operator T(AnyVector<T> vector)
    {
        return vector.Base.X;
    }

    public static explicit operator Vector2D<T>(AnyVector<T> vector)
    {
        return new Vector2D<T>(vector.Base.X, vector.Base.Y);
    }
    
    public static explicit operator Vector3D<T>(AnyVector<T> vector)
    {
        return new Vector3D<T>(vector.Base.X, vector.Base.Y, vector.Base.Z);
    }
    
    public static implicit operator Vector4D<T>(AnyVector<T> vector)
    {
        return vector.Base;
    }
}
