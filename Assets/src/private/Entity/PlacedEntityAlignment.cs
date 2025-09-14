using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EntityStats))]
public class PlacedEntityAlignment : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EntityStats entity = (EntityStats)target;

        if (GUILayout.Button("Snap to triangle center"))
        {
            Debug.Log("Aligning entity");
            entity.SnapToTriangle();   
        }
    }
}
