using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Dots
{
    
    public class Tile : MonoBehaviour
    {
        public TileType type;
        public int xIndex;
        public int yIndex;

        private Board m_board;

        public void Init(int x, int y, Board board, TileType tileType)
        {
            xIndex = x;
            yIndex = y;
            m_board = board;
            type = tileType;
        }

        public Dot Dot()
        {
            return m_board.DotInTile(this);
        }

        //when clicked, start a line
        public static Action<Tile> SelectionStarted; 
        void OnMouseDown()
        {
            if (SelectionStarted != null)
            {
                SelectionStarted(this);
            }
        }

        //Send the tile to the board to be added 
        public static Action<Tile> SelectionEntered;
        void OnMouseEnter()
        {
            if (SelectionEntered != null)
            {
                SelectionEntered(this);
            }
        }

        public static Action SelectionEnded;
        void OnMouseUp()
        {
            if (SelectionEnded != null)
            {
                SelectionEnded();
            }
        }
    }

}
