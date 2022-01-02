using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Dots
{
    
    public class Tile : MonoBehaviour
    {
        public TileType type;
        public int xIndex;
        public int yIndex;

        public void Init(int x, int y, TileType tileType)
        {
            xIndex = x;
            yIndex = y;
            type = tileType;
        }

        //when clicked, start a line
        void OnMouseDown()
        {
            
        }
    }

}
