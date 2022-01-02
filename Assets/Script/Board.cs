using System.Collections.Generic;
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
        public GameObject lineControllerPrefab;

        private Tile[,] m_allTiles;
        private Dot[,] m_allDots;

        private Stack<Dot> m_selectedDots;
        private Stack<Tile> m_selectedTiles;
        private LineController m_line;

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
            m_line = Instantiate(lineControllerPrefab, Vector3.zero, Quaternion.identity).GetComponent<LineController>();
            m_selectedDots = new Stack<Dot>();
            m_selectedTiles = new Stack<Tile>();
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
        /// Dot Selection
        /// </summary>
        /// When picking a tile, we add the dot that it is associated with to a set of dots
        /// Moving the mouse over another tile adds that dot to the selection
        /// If we move away and then move back over, that unselects the dot at the location
        /// Selections add tile positions into the list of selections
        /// When we release the press, we submit the dots and clear them from the board

        //Select a first tile to add to the list
        public void SelectDotAtTile(Tile tile)
        {
            if (tile == null) return;
            
            //Get the dot
            var chosenDot = m_allDots[tile.xIndex, tile.yIndex];
            //Add the dot to the list
            m_selectedDots.Push(chosenDot);
            //Set the tile to the previous Tile
            m_selectedTiles.Push(tile);
            //Update the line
            m_line.SetLineColor(chosenDot.type);
            m_line.UpdateLinePositions(m_selectedTiles.ToArray());
        }

        //Add a new tile dot to the list
        public void AddDotAtTile(Tile tile)
        {
            if (tile == null) return;
            if (m_selectedDots.Count == 0 || m_selectedTiles.Count == 0) return;
            

            var chosenDot = m_allDots[tile.xIndex, tile.yIndex];
            
            if (chosenDot == null) return;
            var lastDot = m_selectedDots.Peek();
            
            //and if dot is not a type match, skip
            if (chosenDot.type != lastDot.type) return;
            
            //If we contain the dot, remove it, otherwise, add it!
            if (lastDot.gameObject.GetInstanceID() == chosenDot.gameObject.GetInstanceID())
            {
                Debug.Log("Attempting to remove");
                RemoveLastDot(tile);
                return;
            }
            
            //if tile is not cardinal, skip. Do this after we compare at other points
            if (!IsTileCardinalTo(m_selectedTiles.Peek(), tile)) return;
            
            m_selectedDots.Push(chosenDot);
            m_selectedTiles.Push(tile);
            m_line.UpdateLinePositions(m_selectedTiles.ToArray());
        }

        //Remove a tile dot from the list (except the first)
        void RemoveLastDot(Tile tile)
        {
            if (tile == null) return;
            if (m_allDots[tile.xIndex, tile.yIndex] == null) return;
            if (m_selectedDots.Count == 1) return;

            m_selectedDots.Pop(); //remove the dot
            m_selectedTiles.Pop();
            
            m_line.UpdateLinePositions(m_selectedTiles.ToArray());
        }

        
        //Clear the dots after a release event
        void ClearPieces()
        {
            
        }
        
        /// <summary>
        /// Utilities
        /// </summary>
        
        //Check if things are in range or not
        bool IsCoordInBoard(int x, int y)
        {
            return (x >= 0 && y >= 0 && x < width && y < height);
        }

        bool IsTileCardinalTo(Tile origin, Tile target)
        {
            if (Mathf.Abs(origin.xIndex - target.xIndex) == 1 && origin.yIndex == target.yIndex) //column aligned
                return true;
            if (Mathf.Abs(origin.yIndex - target.yIndex) == 1 && origin.xIndex == target.xIndex) //row aligned
                return true;
            return false;
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
            newTile.GetComponent<Tile>().Init(x,y, this,config.tileTypes[0]);
        
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