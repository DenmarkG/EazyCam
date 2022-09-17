using UnityEngine;

namespace EazyCam
{
    [System.Serializable]
    public struct FloatRange
    {
        public FloatRange(float min, float max)
        {
            DefaultMin = min;
            DefaultMax = max;

            Min = min;
            Max = max;
        }

        public float DefaultMin;
        public float DefaultMax;

        public float Min;
        public float Max;
    }
}