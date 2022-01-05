using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions.Must;
using Random = System.Random;

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

        private Tile[,] m_allTiles;
        private Dot[,] m_allDots;
        public Dot[,] AllDots
        {
            get => m_allDots;
        }

        public FloatParameter rowDropDelay;
        public FloatParameter dotDropTime;

        private SelectionSystem m_selectionSystem;

        public SelectionSystem SelectionSystem
        {
            get { return m_selectionSystem; }
        }
        private Stack<LineRenderer> m_drawnLines; //stack of lines drawn

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

        private void OnEnable()
        {
            Tile.SelectionEnded += ClearPieces;
            GameControl.BoardConfigChanged += Reset;
            GameControl.ShuffleRequest += ShuffleDots;

        }

        private void OnDisable()
        {
            Tile.SelectionEnded -= ClearPieces;
            GameControl.BoardConfigChanged -= Reset;
            GameControl.ShuffleRequest -= ShuffleDots;
        }
        
        #region Initial Setup
        /// <summary>
        /// Setup
        /// </summary>
        
        //Get local versions of the variables--and possibly reset the board at some point.
        void Setup(BoardConfiguration boardConfig)
        {
            //get local versions of the width and height
            width = boardConfig.width;
            height = boardConfig.height;
            m_allDots = new Dot[width, height];
            m_allTiles = new Tile[width, height];
            dotPrefab = boardConfig.dotPrefab;
            tilePrefab = boardConfig.tilePrefab;
            SetupCamera();
            m_allTiles = SetupTiles(boardConfig);
            m_allDots = SetupDots(dotDropTime.value,rowDropDelay.value);
            m_drawnLines = new Stack<LineRenderer>();
            m_selecting = false;
        }

      
        
        void Reset()
        {
            //destroy all existing tiles and dots first and then setup again
            foreach (Dot dot in m_allDots)
            {
                Destroy(dot.gameObject);
            }

            foreach (Tile tile in m_allTiles)
            {
                Destroy(tile.gameObject);
            }

            Setup(config);
        }
        #endregion
        
        #region Dot and Tile Creation
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
        //Runs through the whole board and fills spaces without dots, Optional bottomRow is used to calculate delay-basis
        Dot[,] SetupDots(float dropTime = .5f , float rowDelay = .1f, int bottomRow = 0)
        {
            var newDots = new Dot[width, height];
            
            //fill tiles in the board
            for (int j = 0; j < height; j++)
            {
                var DistanceFromBottomRow = Math.Max(0,j-bottomRow);
                for (int i = 0; i < width; i++)
                {
                    if (m_allDots[i, j] != null) continue;
                    //a bit of creative code to make the drops consistent, and a 1 for a magic number to delay the animations
                    newDots[i, j] = CreateRandomDotAt(i, j,
                        height * 2f - j ,dropTime, (1+DistanceFromBottomRow)*rowDelay);
                    PlaceDotInBoard(newDots[i,j],i,j);
                }
            }
            return newDots;
        }

        void PlaceDotInBoard(Dot dot, int x, int y)
        {
            if (!IsCoordInBoard(x, y) && dot != null) return;
            
            m_allDots[x, y] = dot;
            dot.xIndex = x;
            dot.yIndex = y;
        }

        List<Dot> CreateDotsInTiles(List<Tile> tiles)
        {
            List<Dot> newDots = new List<Dot>();
            foreach (Tile tile in tiles)
            {
                var dot = CreateRandomDotAt(tile.xIndex, tile.yIndex, height * 1.3f - tile.yIndex, dotDropTime.value, 0);
                PlaceDotInBoard(dot,tile.xIndex,tile.yIndex);
            }

            return newDots;
        }
        #endregion

        #region Collapsing Column
        //This is an approach from Wilmer Lin's course on Match 3
        //The alternative would be to put this on the Dot class and have them search the board *down*--I prefer the board handle the Connect4ness of it all
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
                        
                        //Triggers asynchronous movement function
                        m_allDots[column, j]
                            .DropToPosition(targetPosition, m_allDots[column,j].transform.position, collapseTime, 0f);

                        m_allDots[column, i] = m_allDots[column, j]; //replace dot in the array
                        m_allDots[column, i].SetCoord(column,i);

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
        #endregion
        
        #region Clearing

        /// <summary>
        /// Clear pieces from tiles 
        /// </summary>


        //Clear the dots after a release event
        public static Action ClearFinished;
        public async void ClearPieces()
        { 
            //Destroy line immediately
            while (m_drawnLines.Count != 0)
            {
                GameObject.Destroy(m_drawnLines.Pop().gameObject);
            }

            var selectedTiles = m_selectionSystem.selectedTiles;
            
            if (selectedTiles.Count >= 2)//minimum two to clear
            {
                if (m_selectionSystem.IsSquare)
                {
                    //add all tiles that have matching dot types to the list
                    selectedTiles = selectedTiles.Union(FindAllTilesWithDotType(m_selectionSystem.selectionType)).ToList();
                }
                await ClearDotsFromTiles(selectedTiles);
            }
            
            //collapse columns
            CollapseColumn(selectedTiles);

            //get the empty tiles
            var emptyTiles = AllEmptyTiles();
            
            SetupDots(dotDropTime.value,rowDropDelay.value,LowestRowInTileSet(emptyTiles));

            await Task.Delay(500);//wait for the drop ins
            
            //Check for matches
            if (NoMatchesPossibleInBoard())
            {
                ShuffleDots();
            }
            
            //Empty selections and reset mouse line
            selectedTiles.Clear();
            m_selectionSystem.Reset();

            if (ClearFinished != null)
            {
                ClearFinished();
            }
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
        #endregion
        
        #region Shuffling
        
        
        //Shuffling creates a set of destinations and shuffles those, then tells all the dots to go to their new homes
        //The altnerative, a single shuffle with the animation mixed in, would reject animation commands because a dot
        //could be told to animate more than once because the algorithm would repeat over them.
        void ShuffleDots()
        {
            //Create array of swap directions
            Vector2Int[,] destinations = new Vector2Int[width, height];

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    destinations[i, j] = new Vector2Int(i, j);
                }
            }
            //Shuffle that array
            for (int i = width - 1; i > 0; i--)
            {
                for (int j = height - 1; j > 0; j--)
                {
                    var swapIndex = new Vector2Int(
                        Mathf.FloorToInt(UnityEngine.Random.value * (i + 1)),
                        Mathf.FloorToInt(UnityEngine.Random.value * (j + 1))
                    );
                    var temp = destinations[i, j];
                    destinations[i, j] = destinations[swapIndex.x, swapIndex.y];
                    destinations[swapIndex.x, swapIndex.y] = temp;

                }
            }
            
            //apply destinations to the board of dots
            Dot[,] shuffledDots = new Dot[width, height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    var dot = m_allDots[i, j];
                    var destination = destinations[i, j];
                    var targetPosition = new Vector3(destination.x, destination.y, 0);
                    dot.SetCoord(destination.x,destination.y);
                    dot.MoveToPosition(targetPosition, .2f);
                    shuffledDots[destination.x, destination.y] = dot;
                }
            }

            m_allDots = shuffledDots;
        }
        
        //Go through the whole board and determine if matches are possible or not.
        bool NoMatchesPossibleInBoard()
        {
            if (m_allDots == null) return true; //no dots, obviously no matches possible
            
            //Go through every dot, if there's a match, return false 
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (DotHasMatchesInBoard(m_allDots[i, j])) return false;
                    
                }
            }
            return true;
        }

        //Check a dot's cardinal neighbors for matching types
        bool DotHasMatchesInBoard(Dot dot)
        {
            //check all directions 
            int x = dot.xIndex;
            int y = dot.yIndex;

            Vector2Int[] locationsToCheck = new Vector2Int[]
            {
                new Vector2Int(x + 1, y),
                new Vector2Int(x - 1, y),
                new Vector2Int(x, y + 1),
                new Vector2Int(x, y -1)
            };
            //check the locations
            foreach (Vector2Int location in locationsToCheck)
            {
                if (!IsCoordInBoard(location.x, location.y)) continue; //ignore if out of bounds

                var neighborDot = DotInTile(m_allTiles[location.x, location.y]);
                
                if (neighborDot == null) continue; //ignore if for some reason the dot doesn't exist in a valid tile

                if (dot.type == neighborDot.type) return true; //types match, make a dot
            }
            return false; //didn't find any matches
        }
        
        #endregion
        
        #region Utilities
        
        //Return a tile's dot
        public Dot DotInTile(Tile tile)
        {
            if (tile == null) return null;
            return m_allDots[tile.xIndex, tile.yIndex];
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
        //Finds all Tiles without corresponding Dots
        List<Tile> AllEmptyTiles()
        {
            List<Tile> emptyTiles = new List<Tile>();
            foreach (Tile tile in m_allTiles)
            {
                if(m_allDots[tile.xIndex,tile.yIndex] == null) 
                    emptyTiles.Add(tile);
            }

            return emptyTiles;
        }
        
        //Finds the row with the lowest value in the set
        int LowestRowInTileSet(List<Tile> tiles)
        {
            int lowestRow = Int32.MaxValue;
            foreach (Tile tile in tiles)
            {
                if (tile.yIndex < lowestRow) lowestRow = tile.yIndex;
            }

            return lowestRow;
        }

        //Find all matching tiles of a type
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
        
        //Check if a coordinate is in range or not
        bool IsCoordInBoard(int x, int y)
        {
            return (x >= 0 && y >= 0 && x < width && y < height);
        }

        //Check if tile matches cardinal direction
        bool IsTileCardinalTo(Tile origin, Tile target)
        {
            if (Mathf.Abs(origin.xIndex - target.xIndex) == 1 && origin.yIndex == target.yIndex) //column aligned
                return true;
            if (Mathf.Abs(origin.yIndex - target.yIndex) == 1 && origin.xIndex == target.xIndex) //row aligned
                return true;
            return false;
        }
        #endregion
        
        #region Factories
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

            Vector3 finalPosition = new Vector3(x, y, 0);
            Vector3 startingPosition = new Vector3(x, y + yOffset, 0);
            var newDot = Instantiate(dotPrefab, startingPosition, Quaternion.identity);
            newDot.name = config.dotTypes[randomIndex].name;
            newDot.GetComponent<Dot>().Init(x,y, config.dotTypes[randomIndex]);
            
            //animate if needed
            if (yOffset != 0)
            {
                newDot.GetComponent<Dot>().DropToPosition(finalPosition, startingPosition, dropTime, delayTime);
            }

            return newDot.GetComponent<Dot>();
        }
        #endregion
    }
}