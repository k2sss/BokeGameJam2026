using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Rope2DGenerator))]
public class Rope2DGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Rope2DGenerator rope = (Rope2DGenerator)target;

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate Rope"))
        {
            Undo.RegisterFullObjectHierarchyUndo(rope.gameObject, "Generate Rope");
            rope.GenerateRope();
            EditorUtility.SetDirty(rope.gameObject);
        }

        if (GUILayout.Button("Clear Rope"))
        {
            Undo.RegisterFullObjectHierarchyUndo(rope.gameObject, "Clear Rope");
            rope.ClearRope();
            EditorUtility.SetDirty(rope.gameObject);
        }
    }
}

