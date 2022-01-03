using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Dots
{
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

        public static Action<Dot> DotSelected;

        void StartSelection(Tile tile)
        {
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

        //If the tile is valid, we add it and add it as an edge
        public static Action<Dot> SquareFound;
        public static Action<Tile, Tile> ConnectionAdded;
        public static Action SelectionReversed;
        public static Action<int> SquareRemoved;
        void HandleNewDotAtTile(Tile tile)
        {
            if (!hasSelection || tile == null) return;
            
            //A few possible outcomes
            var lastTile = m_selectedTiles[m_selectedTiles.Count - 1];
            var chosenDot = tile.Dot();

            //quick rejections
            if (chosenDot == null) return;
            if (chosenDot.type != selectionType) return; // type doesn't match
            if (!IsTileCardinalTo(tile, lastTile)) return; //not cardinal
            
            //check if we're going backwards
            if (m_selectedTiles.Count > 1)//we need more than 1 to go backwards
            {
                
                var reverseTile = m_selectedTiles[m_selectedTiles.Count - 2];
                if (tile == reverseTile)
                {
                    Tile removedTile = lastTile;
                    m_selectedTiles.RemoveAt(m_selectedTiles.Count - 1);

                    if (m_squareTiles.Contains(lastTile))
                    {
                        m_squareTiles.Remove(lastTile);
                        m_squaresFound--;
                        if (SquareRemoved != null)
                        {
                            SquareRemoved(m_squaresFound);
                        }
                    }
                    
                    m_paths.RemoveAt(m_paths.Count-1);//remove last connection;
                    
                    if (DotSelected != null) //yes it fires another event but come on...
                    {
                        DotSelected(chosenDot);
                    }
                    //This updates the lines
                    if (SelectionReversed != null)
                    {
                        SelectionReversed();
                    }
                    return;
                }
            }
            
            //Reject if the candidate edge is already traversed
            TileConnection candidate = new TileConnection(lastTile, tile);
            if (ConnectionContainedWithinList(m_paths,candidate)) return;

            //everything else should be valid
            
            //You're making a square if you can connect to something inside the list *and* you aren't repeating
            if (m_selectedTiles.Contains(tile))
            {
                //Need to track how many squares are found for when we undo them.
                m_squareTiles.Add(tile);
                m_squaresFound++;
                if (SquareFound != null)
                {
                    SquareFound(chosenDot);
                }

            }

            //all other conditions passed, we're adding a tile finally

            //Simply add the tile
            m_selectedTiles.Add(tile);
            //and add the connection
            m_paths.Add(candidate);
            
            //Alert selections
            if (ConnectionAdded != null)
            {
                ConnectionAdded(tile,lastTile);
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
