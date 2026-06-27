namespace CodecTactics.Core.Network;

internal sealed class DeterministicRandom
{
    private uint _state;

    public DeterministicRandom(int seed)
    {
        _state = seed == 0 ? 0x9E3779B9u : unchecked((uint)seed);
    }

    public int Next(int exclusiveMax)
    {
        if (exclusiveMax <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(exclusiveMax));
        }

        return (int)(NextUInt() % (uint)exclusiveMax);
    }

    public double NextDouble()
    {
        return NextUInt() / (double)uint.MaxValue;
    }

    public int NextInclusive(int min, int max)
    {
        if (max < min)
        {
            throw new ArgumentOutOfRangeException(nameof(max));
        }

        return min + Next(max - min + 1);
    }

    private uint NextUInt()
    {
        var x = _state;
        x ^= x << 13;
        x ^= x >> 17;
        x ^= x << 5;
        _state = x;
        return x;
    }
}
