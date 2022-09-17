using UnityEngine;
using UnityEditor;

namespace EazyCam
{
    [CustomPropertyDrawer(typeof(FloatRange))]
    public class FloatRangePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            SerializedProperty maxProperty = property.FindPropertyRelative("Max");
            float max = maxProperty.floatValue;

            SerializedProperty minProperty = property.FindPropertyRelative("Min");
            float min = minProperty.floatValue;

            float defaultMax = ((FloatRange)(property.boxedValue)).DefaultMax;
            float defaultMin = ((FloatRange)(property.boxedValue)).DefaultMin;

            EditorGUI.MinMaxSlider(new Rect(position.x, position.y, position.width, 20), label, ref min, ref max, defaultMin, defaultMax);

            minProperty.floatValue = min;
            maxProperty.floatValue = max;

            EditorGUI.EndProperty();
        }
    }
}
