using UnityEngine;

namespace Dots
{
    [CreateAssetMenu(fileName = "Float Asset", menuName = "Scriptable Parameters/Float Parameter", order = 0)]
    public class FloatParameter : ScriptableObject
    {
        public float value;
    }
}