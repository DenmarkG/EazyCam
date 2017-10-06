// fix sliding bug so that camera moves smoothly across floor
// 
// implement reset
// implement targetting
// implement cinematic triggers
// add menu to create new camera settings (already done, but move to top menu, not right click
// polish
// make plug an play (separate component functionality)




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