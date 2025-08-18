using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Entity", menuName = "Entity/Entity")]
public class Entity :ScriptableObject
{
    public GameObject prefab;

    [Header("Spawn rules")]
    public float minHeight = 0f;
    public float maxHeight = 1f;
    public float slopeThreshold = 0.7f; // dot(normal, up)
    public float density = 0.1f; // chance per candidate
}
