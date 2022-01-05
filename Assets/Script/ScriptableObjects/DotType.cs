using UnityEngine;
using System;
using System.Collections;

namespace Dots
{
    /// <summary>
    /// A container object for all the little dot data.
    /// Uses a different curve for animation and dot
    /// 
    /// </summary>
    [CreateAssetMenu(fileName = "NormalDotAsset", menuName = "Dot Types/Normal Type")]
    public class DotType : ScriptableObject
    {
        [Tooltip("Dot's Color")]
        public Color color;
        public bool useTexture;
        public Texture2D texture;
        public FloatParameter dotSize;
        [Tooltip("Tween curve for disappearing")]
        public CurveParameter clearAnimation;
        [Tooltip("Tween curve for moving between locaitons like shuffling")]
        public CurveParameter moveAnimation;
        [Tooltip("Tween curve for dropping in")]
        public CurveParameter dropAnimation;
    }
}