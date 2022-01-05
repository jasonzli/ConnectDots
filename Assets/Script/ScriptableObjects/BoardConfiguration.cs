using UnityEngine;
using UnityEngine.UIElements;

namespace Dots
{
    /// <summary>
    /// a container for all the board configuration data
    /// Can be swapped out if you want different tiles or different dots
    /// </summary>
    [CreateAssetMenu(fileName = "Active Pieces", menuName = "BoardConfiguration", order = 0)]
    public class BoardConfiguration : ScriptableObject
    {
        private const int BOARD_DIM_MAX = 30;

        public int MaxBoardDimension
        {
            get { return BOARD_DIM_MAX; }
        }
        private const int BOARD_DIM_MIN = 3;

        public int MinBoardDimension
        {
            get { return BOARD_DIM_MIN; }
        }
        [Range(2,20)]
        public int width = 2;
        [Range(2,20)]
        public int height = 2;
        public TileType[] tileTypes;
        public DotType[] dotTypes;
        public GameObject tilePoolPrefab;
        public GameObject dotPoolPrefab;
        public FloatParameter rowDropDelay;
        public FloatParameter dotDropTime;
    }
}