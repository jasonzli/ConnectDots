using UnityEngine;
using System;
using System.Collections;

namespace Dots
{
    [CreateAssetMenu(fileName = "NormalDotAsset", menuName = "PieceTypes/Normal Type")]
    public class PieceType : ScriptableObject
    {
        public Color color;
    }
}