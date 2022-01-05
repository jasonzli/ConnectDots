using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Dots
{
    /// <summary>
    /// A line drawing class that creates instsances of the line renderer between tiles
    /// </summary>
    //TODO Do this with the edges that we're checking. Way easier next time
    [RequireComponent(typeof(SelectionSystem))]
    public class LineControl : MonoBehaviour
    {
        public GameObject lineDrawPrefab;
        public GameObject linePoolPrefab;
        public FloatParameter lineThickness;
        public int depth = 1;
        
        private LineRenderer m_mouseLine;
        private Color m_lineColor;
        private SelectionSystem m_selectionSystem;
        private ObjectPool m_linePool;
        private List<DrawnLine> m_activeLines;
        
        //draw a line from the latest point to the mouse
        private void Awake()
        {
            Reset();
        }

        void Reset()
        {
            m_mouseLine = Instantiate(lineDrawPrefab, Vector3.zero, Quaternion.identity, transform).GetComponent<LineRenderer>();
            m_mouseLine.name = "Mouse Line";
            m_mouseLine.gameObject.SetActive(false);
            m_selectionSystem = GetComponent<SelectionSystem>();
            m_activeLines = new List<DrawnLine>();
            m_linePool = Instantiate(linePoolPrefab, Vector3.zero, Quaternion.identity, transform).GetComponent<ObjectPool>();
            m_linePool.name = "Line Pool";
        }

        void OnEnable()
        {
            SelectionSystem.DotSelected += DrawMouseLine;
            SelectionSystem.ConnectionAdded += DrawLineBetweenTiles;
            SelectionSystem.SelectionReversed += RemoveLastLine;
            SelectionSystem.SelectionReversed += ChangeMouseLinePositionTo;
            Tile.SelectionEnded += RemoveAllLines;
        }

        void OnDisable()
        {
            SelectionSystem.DotSelected -= DrawMouseLine;
            SelectionSystem.ConnectionAdded -= DrawLineBetweenTiles;
            SelectionSystem.SelectionReversed -= RemoveLastLine;
            SelectionSystem.SelectionReversed -= ChangeMouseLinePositionTo;
            Tile.SelectionEnded -= RemoveAllLines;
        }

        void Update()
        {
            if (m_selectionSystem.hasSelection)
            {
                var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                m_mouseLine.SetPosition(0,
                    new Vector3(mousePosition.x, mousePosition.y, depth));
            }
        }

        //draw lines between dots;
        void DrawMouseLine(Dot dot)
        {
            m_lineColor = dot.type.color;
            if (!m_mouseLine.gameObject.active)
            {
                m_mouseLine.gameObject.SetActive(true);
                m_mouseLine.material.SetColor("_Color", m_lineColor);
            }
            ChangeMouseLinePositionTo(new Vector3(dot.xIndex,dot.yIndex,depth));
        }

        void ChangeMouseLinePositionTo(Vector3 location)
        {
            m_mouseLine.SetPosition(1, location);
        }
        
        void ChangeMouseLinePositionTo(Tile head)
        {
            var location = new Vector3(head.xIndex, head.yIndex, depth);
            m_mouseLine.SetPosition(1, location);
        }

        void DrawLineBetweenTiles(Tile a, Tile b)
        {
            var line = m_linePool.GetPrefabInstance();
            line.transform.position = a.transform.position;
            var startPosition = new Vector3(a.xIndex, a.yIndex, depth);
            var endPosition = new Vector3(b.xIndex, b.yIndex, depth);
            line.GetComponent<DrawnLine>().Init(startPosition, endPosition, lineThickness.value, m_lineColor);
            m_activeLines.Add(line.GetComponent<DrawnLine>());
            
            ChangeMouseLinePositionTo(new Vector3(a.xIndex,a.yIndex,depth));
        }

        void RemoveLastLine(Tile head)
        {
            var lineToRemove = m_activeLines[m_activeLines.Count - 1];
            
            lineToRemove.ReturnToPool();//return to pool before removing from active
            
            m_activeLines.Remove(lineToRemove);
            
        }

        void RemoveAllLines()
        {
            m_mouseLine.gameObject.SetActive(false);
            
            //deactivate all the objects
            foreach(DrawnLine line in m_activeLines)
            { 
                line.ReturnToPool();
            }

            m_activeLines.Clear();
        }
        
        
        
    }
}
