using LibNoise;
using Silk.NET.Maths;

namespace Utils.Noises;

public class TiledNoise<T> where T : PrimitiveModule
{
    public const float ScaleMultiplier = 0.56235189789237897214f;
    public const float PositionMultiplier = 0.5f;

    public T Noise { get; set; }
    public AnyVector<float> Position { get; set; } = new Vector4D<float>(1, 1, 0, 0);
    public AnyVector<float> Scale { get; set; } = new Vector4D<float>(1, 1, 0, 0);

    public TiledNoise(T noise)
    {
        Noise = noise;
    }

    public float Sample(AnyVector<int> tile)
    {
        return Noise.Sample(tile.Base.As<float>());
    }

    public float Sample(float x = 0, float y = 0, float z = 0, float w = 0)
    {
        return Sample(new Vector4D<float>(x, y, z, w));
    }

    public float Sample(AnyVector<float> tile)
    {
        var pos = Position.Base * PositionMultiplier + tile * (Scale.Base * ScaleMultiplier);
        return Noise.Sample(pos);
    }
}
