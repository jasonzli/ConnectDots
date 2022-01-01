using UnityEngine;

namespace Dots
{
    [CreateAssetMenu(fileName = "Active Pieces", menuName = "PieceSet", order = 0)]
    public class PieceSet : ScriptableObject
    {
        public PieceType[] pieces;
    }
}