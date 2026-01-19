using LibNoise;
using LibNoise.Primitive;

namespace Utils.Noises;

public static class NoiseDefaults
{
    public static ImprovedPerlin Default(int seed = 0)
    {
        return new ImprovedPerlin
        {
            Seed = seed,
            Quality = NoiseQuality.Best,
        };
    }
    
    public static TiledNoise<ImprovedPerlin> DefaultTiled(int seed = 0)
    {
        return new TiledNoise<ImprovedPerlin>(Default(seed));
    }
}
