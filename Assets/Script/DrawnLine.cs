using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dots
{
    [RequireComponent(typeof(LineRenderer))]
    public class DrawnLine : PoolableObject
    {
        private LineRenderer m_renderer;

        void Awake()
        {
            m_renderer = GetComponent<LineRenderer>();
            m_renderer.positionCount = 2;
        }
        public void Init(Vector3 pos1, Vector3 pos2, float width, Color color)
        {
            m_renderer.SetPosition(0, pos1);
            m_renderer.SetPosition(1, pos2);
            m_renderer.startWidth = width;
            m_renderer.material.SetColor("_Color",color);
        }
        
    }
}