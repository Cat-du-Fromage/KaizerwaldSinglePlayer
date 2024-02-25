using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.Mathematics;

namespace Kaizerwald.TerrainBuilder
{
    [CustomEditor(typeof(SimpleTerrain))]
    public class MapGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            SimpleTerrain mapGen = (SimpleTerrain)target;

            if (DrawDefaultInspector())
            {
                if (mapGen.AutoUpdate == true)
                {
                    mapGen.DrawMapInEditor();
                }
            }

            if (GUILayout.Button("Generate"))
            {
                mapGen.DrawMapInEditor();
            }
        }
    }
}
