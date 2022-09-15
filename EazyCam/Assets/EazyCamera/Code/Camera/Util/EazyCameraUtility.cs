using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EazyCam
{
    public static class EazyCameraUtility
    {
        public static Vector3 ConvertMoveInputToCameraSpace(Transform cameraTransform, float horz, float vert)
        {
            float moveX = (horz * cameraTransform.right.x) + (vert * cameraTransform.forward.x);
            float moveZ = (horz * cameraTransform.right.z) + (vert * cameraTransform.forward.z);
            return new Vector3(moveX, 0f, moveZ);
        }
    }
}
