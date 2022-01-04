using UnityEngine;
using System;
using System.Collections;

namespace Dots
{
    [CreateAssetMenu(fileName = "NormalDotAsset", menuName = "Dot Types/Normal Type")]
    public class DotType : ScriptableObject
    {
        public Color color;
        public bool useTexture;
        public Texture2D texture;
        public FloatParameter dotSize;
        public CurveParameter clearAnimation;
        public CurveParameter dropAnimation;
    }
}