using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dots
{
    [RequireComponent(typeof(Board))]
    public class SelectionSystem : MonoBehaviour
    {

        private List<Tile> m_selectedTiles;
        
        private HashSet<Connection> m_paths;
        struct Connection //it's an edge, surprise
        {
            public int a;
            public int b;
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
        void HandleNewDotAtTile(Tile tile)
        {
            if (!hasSelection || tile == null) return;
            
            //A few possible outcomes
            var lastTile = m_selectedTiles[m_selectedTiles.Count - 1];
            var chosenDot = tile.Dot();
            
            //quick rejections
            if (chosenDot.type != selectionType) return; // type doesn't match
            if (!IsTileCardinalTo(tile, lastTile)) return; //not cardinal
            //If we hovered over the last tile but it's only tile
            if (tile == lastTile && m_selectedTiles.Count == 1) return;
            
            //now we can check if we're going backwards
            var reverseTile = m_selectedTiles[m_selectedTiles.Count - 2];
            if (tile == reverseTile)
            {
                m_selectedTiles.RemoveAt(m_selectedTiles.Count - 1);
                
                //TODO Update lines

                return;
            }
            
            //if not backwards then...
           
            
            //Simply add the tile
            m_selectedTiles.Add(tile);
            
            DotSelected(chosenDot);
            //Square is weird: after square you have to reject anything inside

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

        
    }

}
