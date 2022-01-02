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

        //when clicked, start a line
        void OnMouseDown()
        {
            if (m_board == null) return;
            m_board.SelectDotAtTile(this);
        }

        //Send the tile to the board to be added 
        void OnMouseEnter()
        {
            if(m_board == null) return;
            m_board.AddDotAtTile(this);
        }

        
    }

}
