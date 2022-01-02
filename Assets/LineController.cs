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

        public void SetLineColor(DotType type)
        {
            //Has to actually set the material
            m_renderer.material.SetColor("_Color", type.color);
            m_renderer.startColor = type.color;
            m_renderer.endColor = type.color;
        }
        public void UpdateLinePositions(Tile[] tiles)
        {
            m_renderer.positionCount = tiles.Length;
            
            var points = new Vector3[tiles.Length];//plus one for the mouse position

            for (int i = 0; i < tiles.Length; i++)
            {
                points[i] = new Vector3( tiles[i].xIndex, tiles[i].yIndex, 1f);//a little forward to be under the dots
            }

            m_renderer.SetPositions(points);
        }

        void Update()
        {
            if (m_active)
            {
                m_renderer.SetPosition(m_renderer.positionCount - 1,
                    Camera.main.WorldToScreenPoint(Input.mousePosition));
            }
        }

        private void OnEnable()
        {
        }
    }
}