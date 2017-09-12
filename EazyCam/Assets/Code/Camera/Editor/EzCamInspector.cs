using System.Collections;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(EzCamera))]
public class EzCamInspector : Editor
{
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();

        EzCamera cam = (EzCamera)target;
        
        if (cam != null)
        {
            Transform target = EditorGUILayout.ObjectField("Target", cam.Target, typeof(Transform), true) as Transform;
            if (target != cam.Target)
            {
                cam.SetCameraTarget(target);
            }

            EzCameraSettings settings = EditorGUILayout.ObjectField("Camera Settings", cam.Settings, typeof(EzCameraSettings), false) as EzCameraSettings;
            if (settings != cam.Settings)
            {
                cam.ReplaceSettings(settings);
            }

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
                cam.SetDefaultState((EzCameraState.State)System.Enum.Parse(typeof(EzCameraState.State), stateNames[nextIndex]));
            }

            // Additional States
            buttonText = cam.OribtEnabled ? "Disable Orbit" : "Enable Orbit";
            if (GUILayout.Button(buttonText))
            {
                cam.SetOrbitEnabled(!cam.OribtEnabled, true);
            }

            buttonText = cam.FollowEnabled ? "Make Stationary" : "Enable Follow";
            if (GUILayout.Button(buttonText))
            {
                cam.SetFollowEnabled(!cam.FollowEnabled, true);
            }

            buttonText = cam.LockOnEnabled ? "Disable Lock-On" : "Enable Lock-On";
            if (GUILayout.Button(buttonText))
            {
                cam.SetOrbitEnabled(!cam.LockOnEnabled, true);
            }

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
