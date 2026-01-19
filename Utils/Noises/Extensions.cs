using System;
using LibNoise;
using LibNoise.Primitive;

namespace Utils.Noises;

public static class NoiseExtensions
{
    public const float P1D = 1;
    public const float P2D = 0.7071067812f;
    public const float P3D = 0.8660254038f;
    public const float P4D = 1;
    
    public static float Sample(this PrimitiveModule noiseModule, AnyVector<float> position)
    {
        var v = position.Base;
        var x = v.X;
        var y = v.Y;
        var z = v.Z;
        var w = v.W;
        
        return noiseModule switch
        {
            Constant c => c.GetValue(x, y, z, w),
            SimplexPerlin s => s.GetValue(x, y, z, w),
            BevinsGradient g => g.GetValue(x, y, z),
            BevinsValue b => b.GetValue(x, y, z),
            Checkerboard c => c.GetValue(x, y, z),
            Cylinders c => c.GetValue(x, y, z),
            ImprovedPerlin p => p.GetValue(x, y, z).NormalizePerlin3D(),
            Spheres s => s.GetValue(x, y, z),

            _ => throw new NotSupportedException(),
        };
    }

    private static float NormalizePerlin3D(this float value)
    {
        return (value / P3D + 0.955f) / 2;
    }
}