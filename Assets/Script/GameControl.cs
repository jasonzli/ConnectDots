using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

namespace Dots
{
    
    //Just for Demo Purposes, control the board and settings
    public class GameControl : MonoBehaviour
    {

        public BoardConfiguration config;
        private bool m_needToResetBoard;
        public static Action BoardConfigChanged;
        
        void Update()
        {
            //handle some inputs
            if (Input.GetKeyDown(KeyCode.Q))
            {
                config.height += 1;
                m_needToResetBoard = true;
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                config.height -= 1;
                m_needToResetBoard = true;
            }

            if (Input.GetKeyDown(KeyCode.W))
            {
                config.width += 1;
                m_needToResetBoard = true;
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                config.width -= 1;
                m_needToResetBoard = true;
            }
            if (Input.GetKeyDown(KeyCode.Space)) //Reset
            {
                m_needToResetBoard = true;
            }

            if (m_needToResetBoard)
            {
                if (BoardConfigChanged != null)
                {
                    BoardConfigChanged();
                }

                m_needToResetBoard = false;
            }
        }
    }
}
