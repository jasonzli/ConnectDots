using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Dots
{
    /// <summary>
    /// The Control handler for the game. This selects tiles off the board and creates paths
    /// TODO clean up the connection added/dot selected events they frequently cause confusion in listeners
    /// </summary>
    [RequireComponent(typeof(Board))]
    public class SelectionSystem : MonoBehaviour
    {

        private List<Tile> m_selectedTiles;     
        public List<Tile> selectedTiles { get => m_selectedTiles; }
        
        private List<TileConnection> m_paths;
        private class TileConnection //it's an edge, surprise
        {
            public TileConnection(Tile _a, Tile _b)
            {
                nodes = new HashSet<int>();
                nodes.Add(_a.gameObject.GetInstanceID());
                nodes.Add(_b.gameObject.GetInstanceID());
            }

            public bool IsEqualTo(TileConnection connection)
            {
                return this.nodes.SetEquals(connection.nodes);
            }
            
            public HashSet<int> nodes { get; private set; }
        }
   
        public bool hasSelection { get; private set; }
        public DotType selectionType { get; private set; }
        
        private int m_squaresFound;
        public bool IsSquare { get { return (m_squaresFound > 0); } }

        private List<Tile> m_squareTiles; //to hold all the square making points
        
        void Awake()
        {
            Reset();
        }
        
        public void Reset()
        {
            m_selectedTiles = new List<Tile>();
            m_squareTiles = new List<Tile>();
            m_paths = new List<TileConnection>();
            m_squaresFound = 0;
            hasSelection = false;
        }

        void OnEnable()
        {
            Tile.SelectionEntered += HandleNewDotAtTile;
            Tile.SelectionStarted += StartSelection;
        }

        void OnDisable()
        {
            Tile.SelectionEntered -= HandleNewDotAtTile;
            Tile.SelectionStarted -= StartSelection;
        }

        /// <summary>
        /// Starts a selection, gets the type and fires the event with the dot
        /// </summary>
        public static Action<Dot> DotSelected;
        void StartSelection(Tile tile)
        {
            if (hasSelection) return;
            if (tile == null) return;
            
            hasSelection = true;

            //find the dot in the tile
            var chosenDot = tile.Dot();
            if (chosenDot == null) //dot is empty so far
            {
                hasSelection = false;
                return;
            }

            //set the values
            m_selectedTiles.Add(tile);
            
            selectionType = chosenDot.type;
            m_squaresFound = 0;;
            
            if (DotSelected != null) //yes it fires another event but come on...
            {
                DotSelected(chosenDot);
            }
            
        }

        /// <summary>
        /// The meat
        ///
        /// The logic for handling new dots is this:
        /// 1. If it's the origin dot, we go backwards
        /// 2. Reject if this forms an existing edge
        /// 3. Compute if this is a square
        /// 4. Add the tile and the edge
        /// 5. fire event on edge added
        /// TODO clean up the connection added/dot selected events they frequently cause confusion in listeners
        /// </summary>
        //If the tile is valid, we add it and add it as an edge
        public static Action<Dot> SquareFound;
        public static Action<Tile, Tile> ConnectionAdded;
        public static Action<Tile> SelectionReversed;
        public static Action<int> SquareRemoved;
        void HandleNewDotAtTile(Tile candidateTile)
        {
            if (!hasSelection || candidateTile == null) return;
            
            //Grab some reference
            var currentTile = m_selectedTiles[m_selectedTiles.Count - 1]; //the current tile we're on
            var chosenDot = candidateTile.Dot();

            //quick rejections
            if (chosenDot == null) return; //no dot there!
            if (chosenDot.type != selectionType) return; // type doesn't match
            if (!IsTileCardinalTo(candidateTile, currentTile)) return; //not cardinal
            
            // 1. If it's the origin dot, we go backwards
            if (m_selectedTiles.Count > 1)//we need more than 1 to go backwards
            {
                //This is the tile before the current tile
                var reverseTile = m_selectedTiles[m_selectedTiles.Count - 2];
                if (candidateTile == reverseTile)
                {
                    m_selectedTiles.RemoveAt(m_selectedTiles.Count - 1);

                    if (m_squareTiles.Contains(currentTile)) //need to leave the square to count
                    {
                        m_squareTiles.Remove(currentTile);
                        m_squaresFound--;
                        if (SquareRemoved != null)
                        {
                            SquareRemoved(m_squaresFound);
                        }
                    }
                    
                    m_paths.RemoveAt(m_paths.Count-1);//remove last connection;
                    
                    if (SelectionReversed != null)
                    {
                        SelectionReversed(currentTile);
                    }
                    
                    if (DotSelected != null) //yes it fires another event but come on...
                    {
                        DotSelected(chosenDot);
                    }
                    
                    return;
                }
            }
            
            // 2. Reject if this forms an existing edge
            TileConnection candidateEdge = new TileConnection(currentTile, candidateTile);
            if (ConnectionContainedWithinList(m_paths,candidateEdge)) return;

            //everything else should be valid
            
            // 3. Compute if this is a square
            // We're a square if we can add something we've already added and it isn't an existing edge
            if (m_selectedTiles.Contains(candidateTile))
            {
                //Need to track how many squares are found for when we undo them.
                m_squareTiles.Add(candidateTile);
                m_squaresFound++;
                if (SquareFound != null)
                {
                    SquareFound(chosenDot);
                }

            }

            // 4. Add the tile and the edge //all other conditions passed, we're adding a tile finally

            //Simply add the tile
            m_selectedTiles.Add(candidateTile);
            //and add the connection
            m_paths.Add(candidateEdge);
            
            // 5. fire events
            if (ConnectionAdded != null)
            {
                ConnectionAdded(candidateTile,currentTile);
            }
            if (DotSelected != null) //yes it fires another event but come on...
            {
                DotSelected(chosenDot);
            }
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

        //Compare the connections against the existing ones
        bool ConnectionContainedWithinList(List<TileConnection> connections, TileConnection candidate)
        {
            foreach (TileConnection connection in connections)
            {
                if (connection.IsEqualTo(candidate)) return true;
            }
            return false;
        }
        
    }

}
