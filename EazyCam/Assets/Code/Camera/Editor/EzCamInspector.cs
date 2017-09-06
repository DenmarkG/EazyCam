using System.Collections;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EzCamera))]
public class EzCamInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EzCamera cam = (EzCamera)target;
    }
}
