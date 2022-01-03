using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dots
{
    [RequireComponent(typeof(LineRenderer))]
    public class LineController : MonoBehaviour
    {
        
        private bool m_active;
        private LineRenderer m_renderer;
        void Awake()
        {
            m_renderer = GetComponent<LineRenderer>();
            m_active = false;
        }
        public void Reset()
        {
            m_renderer.positionCount = 0;
            m_active = false;
        }
        public void SetLineColor(DotType type)
        {
            //Has to actually set the material
            m_renderer.material.SetColor("_Color", type.color);
            m_renderer.startColor = type.color;
            m_renderer.endColor = type.color;
        }
        

        public void FollowMouseFromTile(Tile tile)
        {
            if (tile == null) return;
            
            m_renderer.positionCount = 2;
            m_renderer.SetPosition(1, new Vector3(tile.xIndex,tile.yIndex,1));
            m_active = true;
        }
        
        public void SetLineBetweenTiles(Tile start, Tile end)
        {
            if (start == null || end == null) return;
            
            m_renderer.positionCount = 2;
            m_renderer.SetPosition(0, new Vector3(start.xIndex,start.yIndex,1));
            m_renderer.SetPosition(1, new Vector3(end.xIndex,end.yIndex,1));
            
        }
        
        void Update()
        {
            if (m_active)
            {
                var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                m_renderer.SetPosition(0,
                    new Vector3(mousePosition.x, mousePosition.y, 1f));
            }
        }



        private void OnEnable()
        {
        }
    }
}