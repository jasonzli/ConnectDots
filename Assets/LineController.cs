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
        }
        public void SetLineColor(DotType type)
        {
            //Has to actually set the material
            m_renderer.material.SetColor("_Color", type.color);
            m_renderer.startColor = type.color;
            m_renderer.endColor = type.color;
        }
        public void UpdateLinePositions(Tile[] tiles)
        {
            m_renderer.positionCount = tiles.Length + 1;
            
            var points = new Vector3[tiles.Length+1];//plus one for the mouse position

            //first positoin is reserved for the mouse
            for (int i = 1; i < tiles.Length+1; i++)
            {
                points[i] = new Vector3( tiles[i-1].xIndex, tiles[i-1].yIndex, 1f);//a little forward to be under the dots
            }

            m_renderer.SetPositions(points);
            m_active = true;
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