using UnityEngine;
using UnityEngine.UIElements;

namespace Dots
{
    [CreateAssetMenu(fileName = "Active Pieces", menuName = "BoardConfiguration", order = 0)]
    public class BoardConfiguration : ScriptableObject
    {
        [Range(2,100)]
        public int width = 2;
        [Range(2,100)]
        public int height = 2;
        public TileType[] tiles;
        public PieceType[] pieces;
    }
}