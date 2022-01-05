using System;
using UnityEngine;

namespace Dots
{
    
    //Just for Demo Purposes, control the board and settings
    public class GameControl : MonoBehaviour
    {

        public BoardConfiguration config;
        private bool m_needToResetBoard;
        public static Action BoardConfigChanged;
        public static Action ShuffleRequest;
        

        void Start()
        {
            Screen.SetResolution(720, 1280, false);
        }
        void Update()
        {
            //handle some inputs
            if (Input.GetKeyDown(KeyCode.Q))
            {
                config.height = Mathf.Clamp(config.height + 1, config.MinBoardDimension, config.MaxBoardDimension);
                m_needToResetBoard = true;
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                
                config.height = Mathf.Clamp(config.height - 1, config.MinBoardDimension, config.MaxBoardDimension);
                m_needToResetBoard = true;
            }

            if (Input.GetKeyDown(KeyCode.W))
            {
                
                config.width = Mathf.Clamp(config.width + 1, config.MinBoardDimension, config.MaxBoardDimension);
                m_needToResetBoard = true;
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                config.width = Mathf.Clamp(config.width - 1, config.MinBoardDimension, config.MaxBoardDimension);
                m_needToResetBoard = true;
            }
            if (Input.GetKeyDown(KeyCode.Space)) //Reset
            {
                m_needToResetBoard = true;
            }
            if (Input.GetKeyDown(KeyCode.G)) //shuffle!
            {
                if (ShuffleRequest != null)
                {
                    ShuffleRequest();
                }
            }

            if (m_needToResetBoard)
            {
                if (BoardConfigChanged != null)
                {
                    BoardConfigChanged();
                }

                m_needToResetBoard = false;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
        }
    }
}
