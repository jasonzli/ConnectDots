using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dots
{
    [RequireComponent(typeof(MeshRenderer))]
    public class GamePiece : MonoBehaviour
    {
        public int xIndex;
        public int yIndex;
        public PieceType type;

        public void Init(int x, int y, PieceType pieceType)
        {
            xIndex = x;
            yIndex = y;
            type = pieceType;
        }
    }
}