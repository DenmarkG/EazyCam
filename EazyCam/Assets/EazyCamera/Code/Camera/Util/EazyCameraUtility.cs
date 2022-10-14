using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EazyCam
{
    public enum EnabledState
    {
        Disabled = 0,
        Enabled = 1,
    }

    public static class EazyCameraUtility
    {
        public static readonly string MouseX = "Mouse X";
        public static readonly string MouseY = "Mouse Y";

        public static Vector3 ConvertMoveInputToCameraSpace(Transform cameraTransform, float horz, float vert)
        {
            float moveX = (horz * cameraTransform.right.x) + (vert * cameraTransform.forward.x);
            float moveZ = (horz * cameraTransform.right.z) + (vert * cameraTransform.forward.z);
            return new Vector3(moveX, 0f, moveZ);
        }

        public static float Squared(this float num)
        {
            return num * num;
        }

        public static float ClampToRange(float value, FloatRange range)
        {
            return Mathf.Clamp(value, range.Min, range.Max);
        }

    }
}
