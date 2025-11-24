using System;

namespace TGC.MonoGame.TP.Util;

internal class Utils
{
    private static readonly Random random = new();

    public static float GenerateNumber(float x)
    {
        return (float)((random.NextDouble() * 2 - 1) * x);
    }
}