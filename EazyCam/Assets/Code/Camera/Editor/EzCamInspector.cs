using System.Collections;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(EzCamera))]
public class EzCamInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EzCamera cam = (EzCamera)target;
        
        if (cam != null)
        {
            string buttonText = null;
            // Default State
            EzCameraState.State currentDefaultState = cam.DefaultState;

            // turn the states to a strinig list
            List<string> stateNames = new List<string>(System.Enum.GetNames(typeof(EzCameraState.State)));
            
            // remove lock on state
            stateNames.Remove(EzCameraState.State.LOCKON.ToString());

            // turn list to enum drop down
            int selectedIndex = stateNames.IndexOf(currentDefaultState.ToString());
            int nextIndex = EditorGUILayout.Popup(selectedIndex, stateNames.ToArray());

            // set value as default state
            if (nextIndex != selectedIndex)
            {
                cam.SetDefaultState((EzCameraState.State)System.Enum.Parse(typeof(EzCameraState), stateNames[nextIndex]));
            }

            // Additional States
            


            // Cmaera Options
            buttonText = cam.ZoomEnabled ? "Disable Zoom" : "Enable Zoom";
            if (GUILayout.Button(buttonText))
            {
                cam.SetZoomEnabled(!cam.ZoomEnabled);
            }

            buttonText = cam.CollisionsEnabled ? "Disable Collisions" : "Enable Collisions";
            if (GUILayout.Button(buttonText))
            {
                cam.EnableCollisionCheck(!cam.CollisionsEnabled, true);
            }

        }


    }
}
