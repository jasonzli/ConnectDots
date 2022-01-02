using UnityEngine;

namespace Dots
{
    [CreateAssetMenu(fileName = "Curve Asset", menuName = "Scriptable Parameters/Curve Parameter", order = 0)]
    public class CurveParameter : ScriptableObject
    {
        public AnimationCurve curve;

        public float Evaluate(float t) 
        {
            return curve.Evaluate(t);
        }
    
    }
}