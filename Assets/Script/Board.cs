using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Dots
{
    [RequireComponent(typeof(SelectionSystem))]
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

        public FloatParameter rowDropDelay;
        public FloatParameter dotDropTime;

        private SelectionSystem m_selectionSystem;
        private List<Tile> m_selectedTiles; //tiles we've selected
        private Stack<LineRenderer> m_drawnLines; //stack of lines drawn
        private LineController m_line; //a tile to mouse line renderer

        private bool m_selecting; //The state variable
        private bool m_squareFound;
        private DotType m_selectedType;

        private void Awake()
        {
            m_selectionSystem = GetComponent<SelectionSystem>();
        }
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
            SetupCamera();
            m_allTiles = SetupTiles(boardConfig);
            m_allDots = SetupDots(dotDropTime.value,rowDropDelay.value);
            m_line = Instantiate(lineControllerPrefab, Vector3.zero, Quaternion.identity).GetComponent<LineController>();
            m_selectedTiles = new List<Tile>(); 
            m_drawnLines = new Stack<LineRenderer>();
            m_selecting = false;
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
        
        //Instantaneous
        Dot[,] SetupPieces(BoardConfiguration boardConfig)
        {
            var newDots = new Dot[width, height];
            //fill the tiles in the board
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (newDots[i, j] != null) continue;
                    newDots[i, j] = CreateRandomDotAt(i, j);
                }
            }
            return newDots;
        }
        
        //Animated
        Dot[,] SetupDots(float dropTime = .5f , float rowDelay = .1f)
        {
            var newDots = new Dot[width, height];
            List<Task> animationTasks = new List<Task>();
            //fill tiles in the board
            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    if (newDots[i, j] != null) continue;
                    //a bit of creative code to make the drops consistent, and a 1 for a magic number to delay the animations
                    newDots[i, j] = CreateRandomDotAt(i, j,
                        height * 1.1f - j ,dropTime, 1+(1+j)*rowDelay);
                }
            }
            return newDots;
        }
        
        /// <summary>
        /// Tile Selection and Line Drawing
        /// </summary>
        /// When picking a tile, we add the dot that it is associated with to a set of dots
        /// Moving the mouse over another tile adds that dot to the selection
        /// If we move away and then move back over, that unselects the dot at the location
        /// Selections add tile positions into the list of selections
        /// When we release the press, we submit the dots and clear them from the board

        //First selection adds a tile and starts the mouse renderer
        public void SelectDotAtTile(Tile tile)
        {
            if (tile == null) return;
            
            //Get the dot
            var chosenDot = m_allDots[tile.xIndex, tile.yIndex];
            
            //Set the tile to the previous Tile
            m_selectedTiles.Add(tile);
            
            //Update the line
            m_line.SetLineColor(chosenDot.type);
            m_line.FollowMouseFromTile(tile);

            m_selecting = true;
            m_squareFound = false;
            m_selectedType = chosenDot.type;
        }
        
        //Create a line Controller between two tiles
        GameObject CreateLineBetweenTiles(Tile start, Tile end)
        {
            if (start == null || end == null) return null;
            
            //Create the line and set it at the start and end
            var line = Instantiate(lineControllerPrefab, 
                new Vector3(start.xIndex, start.yIndex, 1f), Quaternion.identity);
            
            //Set the lines
            line.GetComponent<LineController>().SetLineBetweenTiles(start,end);
            //Add the line to the stack
            m_drawnLines.Push(line.GetComponent<LineRenderer>());

            return line;
        }
        
        //How to handle our new tile
        //There is a lot of logic here
        public void HandleNewDotAtTile(Tile tile)
        {
            if (!m_selecting) return; //ignore if we're not selecting tiles
            if (tile == null) return; //ignore if the tile is null
            
            //Find the last dot we added
            var lastTile = m_selectedTiles[m_selectedTiles.Count - 1];
            var lastDot = m_allDots[lastTile.xIndex,lastTile.yIndex];
            //if it's the first tile, then we only care if it's the same or not
            if (m_selectedTiles.Count == 1)
            {
                if (lastTile == tile) return;//exit early
            }
            
            var chosenDot = m_allDots[tile.xIndex, tile.yIndex]; //Find the dot at the tile
            if (chosenDot == null) return; //ignore if no dot
            
            //First do behavior checks for removal or backwards catching
            
            //If this is the last tile we added then we *remove* it
            if (tile == lastTile)
            {
                //Clear the last tile
                RemoveLastTileFromSelection();
                
                //Clean up line renderer
                if (m_drawnLines.Count > 0)
                {
                    GameObject.Destroy(m_drawnLines.Pop().gameObject);
                }
                //Find the last possible mouse point
                var pointingTile = m_selectedTiles[m_selectedTiles.Count - 1];
                m_line.FollowMouseFromTile(pointingTile);
                
                return; //finish handling
            }
            
            //Check if the tile is point toward our last selected tile ie going backwards
            if (m_selectedTiles.Count >= 2) //alternatively could have stored *direction*
            {
                var pointingTile = m_selectedTiles[m_selectedTiles.Count - 2];
                if (tile == pointingTile) return; //finish handling
            }
            
            //Then do pre-addition checks

            if (chosenDot.type != lastDot.type) return; //ignore if not a type match
            if (!IsTileCardinalTo(lastTile, tile)) return; //ignore if not cardinal
            
            //Create the line
            var newLine = CreateLineBetweenTiles(lastTile, tile);
            newLine.GetComponent<LineController>().SetLineColor(chosenDot.type);
            
            //Square Clearing behavior if we can connect to a dot that is in our line
            if (m_selectedTiles.Contains(tile))
            {
                //Square found
                //Illuminate squares
                m_squareFound = true;
                Debug.Log($"Square found");
                
                //TO DO, square found behavior should also be different
            }
            
            //Finally, just normally add the tile and update hte line renderer
            m_selectedTiles.Add(tile);
            m_line.FollowMouseFromTile(tile);
        }

        //Remove a tile dot from the list (except the first)
        void RemoveTileFromSelection(Tile tile)
        {
            if (tile == null) return;
            if (m_allDots[tile.xIndex, tile.yIndex] == null) return;

            m_selectedTiles.Remove(tile);
        }

        void RemoveLastTileFromSelection()
        {
            if (m_selectedTiles.Count < 1) return;
            m_selectedTiles.RemoveAt(m_selectedTiles.Count - 1);
        }
        
        
        //This is an approach I saw from Wilmer Lin's course on Match 3
        //The alternative would be to put this on the Dot class
        List<Dot> CollapseColumn(int column, float collapseTime = 0.1f)
        {
            List<Dot> fallingDots = new List<Dot>();

            for (int i = 0; i < height - 1; i++)
            {
                if (m_allDots[column, i] == null)//if there's an empty dot there because we detroyed it
                {
                    var targetPosition = new Vector3(column, i, 0);
                    //search upwards to find a new dot
                    for (int j = i + 1; j < height; j++)
                    {
                        if (m_allDots[column, j] == null) continue; // haven't found a dot yet

                        m_allDots[column, j]
                            .DropToPosition(targetPosition, m_allDots[column,j].transform.position, collapseTime, 0f);

                        m_allDots[column, i] = m_allDots[column, j]; //replace dot in the array
                        m_allDots[column,i].SetCoord(column,j);

                        if (!fallingDots.Contains(m_allDots[column, i]))
                        {
                            fallingDots.Add(m_allDots[column, i]);
                        }
                        
                        //create new empty spot
                        m_allDots[column, j] = null;
                        break;
                    }
                }
            }

            return fallingDots;
        }

        List<Dot> CollapseColumn(List<Tile> tiles)
        {
            List<Dot> fallingDots = new List<Dot>();
            List<int> columnsToCollapse = GetColumnsFromTiles(tiles);

            foreach (int column in columnsToCollapse)
            {
                fallingDots = fallingDots.Union(CollapseColumn(column)).ToList();
            }

            return fallingDots;
        }
        
        List<int> GetColumnsFromTiles(List<Tile> tiles)
        {
            List<int> columns = new List<int>();
            foreach (Tile tile in tiles)
            {
                if (columns.Contains(tile.xIndex)) continue;
                columns.Add(tile.xIndex);
            }

            return columns;
        }
        
        
        
        /// <summary>
        /// Clear pieces from tiles 
        /// </summary>
        
        
        //Clear the dots after a release event
        public async void ClearPieces()
        { 
            //Destroy line immediately
            while (m_drawnLines.Count != 0)
            {
                GameObject.Destroy(m_drawnLines.Pop().gameObject);
            }

            if (m_selectedTiles.Count >= 2)//minimum two to clear
            {
                if (m_squareFound)
                {
                    //add all tiles that have matching dot types to the list
                    m_selectedTiles = m_selectedTiles.Union(FindAllTilesWithDotType(m_selectedType)).ToList();
                }
                await ClearDotsFromTiles(m_selectedTiles);
            }
            
            //collapse columns
            CollapseColumn(m_selectedTiles);
            
            //Empty selections and reset mouse line
            m_selectedTiles.Clear();
            m_line.Reset();
            m_selecting = false;
            m_selectedType = null;
        }
        
        async Task ClearDotsFromTiles(List<Tile> tiles)
        {
            //get all the clearing tasks
            List<Task> clearingTasks = new List<Task>();
            //Destroy game objects from board
            foreach (Tile tile in tiles)
            {
                if (tile == null) continue;
                
                clearingTasks.Add(ClearDotFromTileAt(tile.xIndex,tile.yIndex));
            }

            await Task.WhenAll(clearingTasks); //might do animation here.
            
        }

        //Clear a dot within a tile, does some indirection with the m_allDots array
        async Task ClearDotFromTileAt(int x, int y)
        {
            Dot dotToClear = m_allDots[x, y];
            if (dotToClear == null) return;
            
            m_allDots[x, y] = null;
            
            //TO DO:: Do clearing animation()
            await dotToClear.Clear();
            
            Destroy(dotToClear.gameObject);
        }

        /// <summary>
        /// Utilities
        /// </summary>

        List<Tile> FindAllTilesWithDotType(DotType type)
        {
            List<Tile> foundTiles = new List<Tile>();
            foreach (Tile tile in m_allTiles)
            {
                if (tile == null) continue;
                var dot = m_allDots[tile.xIndex, tile.yIndex];
                if (dot == null || dot.type != type) continue;
                
                foundTiles.Add(tile);
            }
            return foundTiles;
        }
        
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
        Dot CreateRandomDotAt(int x, int y, float yOffset = 0f, float dropTime = .5f, float delayTime = 0f)
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

            Vector3 targetPosition = new Vector3(x, y, 0);
            Vector3 offsetPosition = new Vector3(x, y + yOffset, 0);
            var newDot = Instantiate(dotPrefab, offsetPosition, Quaternion.identity);
            newDot.name = config.dotTypes[randomIndex].name;
            newDot.GetComponent<Dot>().Init(x,y, config.dotTypes[randomIndex]);
            
            //animate if needed (
            if (yOffset != 0)
            {
                newDot.GetComponent<Dot>().DropToPosition(targetPosition, offsetPosition, dropTime, delayTime);
            }

            return newDot.GetComponent<Dot>();
        }
    }
}