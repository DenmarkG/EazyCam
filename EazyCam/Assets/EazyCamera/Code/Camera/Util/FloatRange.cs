using UnityEngine;

namespace EazyCam
{
    [System.Serializable]
    public struct FloatRange
    {
        public FloatRange(float min, float max)
        {
            Min = min;
            Max = max;
        }

        public float Min;
        public float Max;
    }
}