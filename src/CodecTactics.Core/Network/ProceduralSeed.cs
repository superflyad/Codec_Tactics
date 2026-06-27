using System.Globalization;
using System.Security.Cryptography;

namespace CodecTactics.Core.Network;

public readonly record struct ProceduralSeed(int Value, string Text)
{
    public static ProceduralSeed FromInt(int seed)
    {
        return new ProceduralSeed(seed, seed.ToString(CultureInfo.InvariantCulture));
    }

    public static ProceduralSeed FromText(string seedText)
    {
        if (string.IsNullOrWhiteSpace(seedText))
        {
            throw new ArgumentException("Seed text cannot be empty.", nameof(seedText));
        }

        if (int.TryParse(seedText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var numericSeed))
        {
            return FromInt(numericSeed);
        }

        unchecked
        {
            var hash = 2166136261u;
            foreach (var character in seedText.Trim())
            {
                hash ^= character;
                hash *= 16777619u;
            }

            return new ProceduralSeed((int)hash, seedText.Trim());
        }
    }

    public static ProceduralSeed CreateRandom()
    {
        var seed = RandomNumberGenerator.GetInt32(int.MinValue, int.MaxValue);
        return FromInt(seed);
    }
}
