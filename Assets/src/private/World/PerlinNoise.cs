using System;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
public class PerlinNoise
{
    private int[] p;

    public PerlinNoise(int seed)
    {
        p = new int[512];
        var perm = Enumerable.Range(0, 256).ToArray();
        var rand = new System.Random(seed);

        // shuffle
        for (int i = 0; i < 256; i++)
        {
            int swapIndex = rand.Next(256);
            int tmp = perm[i];
            perm[i] = perm[swapIndex];
            perm[swapIndex] = tmp;
        }

        for (int i = 0; i < 512; i++)
        {
            p[i] = perm[i & 255];
        }
    }

     private static float Fade(float t) =>
        t * t * t * (t * (t * 6 - 15) + 10);

    private static float Lerp(float a, float b, float t) =>
        a + t * (b - a);

    private static float Grad(int hash, float x, float y)
    {
        int h = hash & 7; // only need 8 directions
        float u = h < 4 ? x : y;
        float v = h < 4 ? y : x;
        return ((h & 1) == 0 ? u : -u) +
               ((h & 2) == 0 ? v : -v);
    }

    public float Noise(float x, float y)
    {
        int xi = (int)MathF.Floor(x) & 255;
        int yi = (int)MathF.Floor(y) & 255;

        float xf = x - MathF.Floor(x);
        float yf = y - MathF.Floor(y);

        float u = Fade(xf);
        float v = Fade(yf);

        int aa = p[p[xi] + yi];
        int ab = p[p[xi] + yi + 1];
        int ba = p[p[xi + 1] + yi];
        int bb = p[p[xi + 1] + yi + 1];

        float x1 = Lerp(Grad(aa, xf, yf), Grad(ba, xf - 1, yf), u);
        float x2 = Lerp(Grad(ab, xf, yf - 1), Grad(bb, xf - 1, yf - 1), u);

        return (Lerp(x1, x2, v) + 1f) / 2f; // normalize to [0,1]
    }

    public float Elevation(float x, float y, int layers)
    {
        float e = 0f;
        float maxAmpl = 0f;

        for (int i = 0; i < layers; i++)
        {
            int fac = 1 << i;
            float ampl = MathF.Pow(0.5f, i);

            e += ampl * Noise(fac * x, fac * y);
            maxAmpl += ampl;
        }

        return e / maxAmpl;
    }

    public float Val(float x, float y, int layers, float flatness)
    {
        float e = Elevation(x, y, layers);
        if (e < 0f) e = 0f;
        return MathF.Pow(e, flatness);
    }
}
