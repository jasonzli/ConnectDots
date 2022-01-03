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
        
        public List<Tile> selectedTiles
        {
            get => m_selectedTiles;
        }
        public bool hasSelection { get; private set; }
        public DotType selectionType { get; private set; }
        public bool isSquare { get; private set; }

        void Awake()
        {
            Reset();
        }
        
        public void Reset()
        {
            m_selectedTiles = new List<Tile>();
            m_paths = new List<TileConnection>();
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
            Debug.Log("started selection");
            if (tile == null) return;
            
            
            hasSelection = true;
            
            //find the dot in the tile
            var chosenDot = tile.Dot();
            
            //Update Line -later-
            
            
            //set the values
            m_selectedTiles.Add(tile);
            
            if (DotSelected != null) //yes it fires another event but come on...
            {
                DotSelected(chosenDot);
            }
            selectionType = chosenDot.type;
            isSquare = false;
            
        }

        //If the tile is valid, we add it and add it as an edge
        public static Action<Dot> SquareFound;
        void HandleNewDotAtTile(Tile tile)
        {
            if (!hasSelection || tile == null) return;
            
            //A few possible outcomes
            var lastTile = m_selectedTiles[m_selectedTiles.Count - 1];
            var chosenDot = tile.Dot();
            
            //quick rejections
            if (chosenDot.type != selectionType) return; // type doesn't match
            if (!IsTileCardinalTo(tile, lastTile)) return; //not cardinal
            
            //check if we're going backwards
            if (m_selectedTiles.Count > 2)//we need more than 1 to go backwards
            {
                var reverseTile = m_selectedTiles[m_selectedTiles.Count - 2];
                if (tile == reverseTile)
                {
                    
                    m_selectedTiles.RemoveAt(m_selectedTiles.Count - 1);
                    
                    //TODO Update lines
                    
                    m_paths.RemoveAt(m_paths.Count-1);//remove last connection;
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
                Debug.Log("SquareFound");
                if (SquareFound != null)
                {
                    SquareFound(chosenDot);
                }

                isSquare = true;
            }


            //Simply add the tile
            m_selectedTiles.Add(tile);
            //and add the connection
            m_paths.Add(candidate);
            
            //Alert selections
            if (DotSelected != null)
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
