using UnityEngine;
using System;
using System.Collections;

namespace Dots
{
    [CreateAssetMenu(fileName = "NormalDotAsset", menuName = "PieceTypes/Normal Type")]
    public class DotType : ScriptableObject
    {
        public Color color;
        public bool useTexture;
        public Texture2D texture;
    }
}