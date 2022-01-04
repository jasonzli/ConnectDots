using UnityEngine;

namespace Dots
{
    [CreateAssetMenu(fileName = "Int Asset", menuName = "Scriptable Parameters/Int Parameter", order = 0)]
    public class IntParameter : ScriptableObject
    {
        public int value;
    }
}