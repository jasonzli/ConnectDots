using System;
using UnityEngine;

namespace Dots
{
    /// <summary>
    /// A tile where dots go. These are mostly placeholders for spaces on the board
    /// They allow us to know which dot we're selecting by choosing which position we've chosen
    /// Rationale: dots move to places on the board, and those places are tiles
    /// tiles themselves can have properties, separate from dots, but you select *the tile* that holds the dot
    /// TODO Move the mouse interaction code off of this tile and into a separate controller that uses raycasts and state
    /// </summary>
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

        //Mouse Up Propagation
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
