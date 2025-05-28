using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WindowFrameHelper))]
public class FrameHelperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Update Transform"))
        {
            target.GetComponent<WindowFrameHelper>().UpdateTransform();
        }
    }
}
