using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PerlinColorPair
{
    [Range(0f, 5f)] public float perlinValue;
    public Color color;
}

public class PerlinColor : MonoBehaviour
{
    [Header("Edit color mapping")]
    public List<PerlinColorPair> colorMap = new List<PerlinColorPair>();

    private Dictionary<float, Color> lookup;


    public void Init()
    {
        lookup = new Dictionary<float, Color>();
        foreach (var entry in colorMap)
        {
            if (!lookup.ContainsKey(entry.perlinValue))
            {
                lookup.Add(entry.perlinValue, entry.color);
            }
        }
    }

    public Color GetColor(float perlin)
    {
        if (lookup.ContainsKey(perlin))
        {
            return lookup[perlin];
        }

        float closest = float.MaxValue;
        Color closestColor = Color.white;

        foreach (var kv in lookup)
        {
            float diff = Mathf.Abs(kv.Key - perlin);
            if (diff < closest)
            {
                closest = diff;
                closestColor = kv.Value;
            }
        }

        return closestColor;
    }


}