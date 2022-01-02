using UnityEngine;

namespace Dots
{
    public class Board : MonoBehaviour
    {
        public BoardConfiguration config;
        public int width, height;
        public int marginSize = 2;

        public GameObject tilePrefab;
        public GameObject dotPrefab;

        private Tile[,] m_allTiles;
        private Dot[,] m_allDots;
        
        private void Start()
        {
            Setup(config);
        }

        /// <summary>
        /// Setup
        /// </summary>
        
        //Get local versions of the variables--and possibly reset the board at some point.
        void Setup(BoardConfiguration boardConfig)
        {
            //get local versions of the width and height
            width = boardConfig.width;
            height = boardConfig.height;
            dotPrefab = boardConfig.dotPrefab;
            tilePrefab = boardConfig.tilePrefab;
            m_allTiles = SetupTiles(boardConfig);
            m_allDots = SetupPieces(boardConfig);
            SetupCamera();
        }
        
        Tile[,] SetupTiles(BoardConfiguration boardConfig)
        {
            var newTiles = new Tile[width, height];
            //fill the tiles in the board
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (newTiles[i, j] != null) continue;
                    newTiles[i, j] = CreateNormalTile(i, j);
                }
            }
            return newTiles;
        }

        //Calculate the orthographic size
        void SetupCamera()
        {
            //move the middle position, -10 so it's far back enough
            Camera.main.transform.position = new Vector3((width - 1) * .5f, (height - 1) * .5f, -10f);

            float aspectRatio = 9f / 16f; //widescreen
            //calculate the orthographic size
            float verticalSize = height * .5f + (float) marginSize;
            float horizontalSize = (width * .5f + (float) marginSize) / aspectRatio;
            
            //Use the larger of the two
            float orthographicSize = verticalSize > horizontalSize ? verticalSize : horizontalSize;

            Camera.main.orthographicSize = orthographicSize;
        }
        
        Dot[,] SetupPieces(BoardConfiguration boardConfig)
        {
            var newDots = new Dot[width, height];
            //fill the tiles in the board
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (newDots[i, j] != null) continue;
                    newDots[i, j] = CreateRandomDot(i, j);
                }
            }
            return newDots;
        }
        
        
        
        /// <summary>
        /// Utilities
        /// </summary>
        
        //Check if things are in range or not
        bool IsCoordInBoard(int x, int y)
        {
            return (x >= 0 && y >= 0 && x < width && y < height);
        }
        
        
        /// <summary>
        /// Tile and Dot factories
        /// </summary>
        
        //Factory behaviors for creating objects with types
        Tile CreateNormalTile(int x, int y, int z = 0)
        {
            if (tilePrefab == null) {
                Debug.LogWarning($"CreateNormalTile Error: Tile prefab not set");
                return null;
            } 
            if (!IsCoordInBoard(x, y)) {
                Debug.LogWarning($"CreateNormalTile Error: Coord out of bounds");
                return null;
            } 
            //For now uses the only one in the array, normal
            if (config.tileTypes[0] == null)
            {
                Debug.LogWarning($"No TileTypes set in the board configuration!");
                return null;
            }
            var newTile = Instantiate(tilePrefab, new Vector3(x,y,z), Quaternion.identity);
            newTile.name = $"Tile ({x},{y})";
            newTile.transform.parent = transform;
            newTile.GetComponent<Tile>().Init(x,y, config.tileTypes[0]);
        
            return newTile.GetComponent<Tile>();
        }
        

        //Factory for creating random dots
        Dot CreateRandomDot(int x, int y, int z = 0)
        {
            if (!IsCoordInBoard(x, y)) {
                Debug.LogWarning($"CreateDot Error: Coord out of bounds");
                return null;
            } 
            
            int randomIndex = UnityEngine.Random.Range(0, config.dotTypes.Length);
            if (config.dotTypes[randomIndex] == null)
            {
                Debug.LogWarning($"DotTypes not set in the board configuration!");
                return null;
            }

            var newDot = Instantiate(dotPrefab, new Vector3(x,y,z), Quaternion.identity);
            newDot.name = config.dotTypes[randomIndex].name;
            newDot.GetComponent<Dot>().Init(x,y, config.dotTypes[randomIndex]);

            return newDot.GetComponent<Dot>();
        }
    }
}