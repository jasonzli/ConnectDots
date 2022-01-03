using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Dots
{
    //TODO Do this with the edges that we're checking. Way easier next time
    [RequireComponent(typeof(SelectionSystem))]
    public class LineControl : MonoBehaviour
    {
        public GameObject lineDrawPrefab;
        public FloatParameter lineThickness;
        public int depth = 1;
        
        private LineRenderer m_mouseLine;
        private Color m_lineColor;
        private SelectionSystem m_selectionSystem;
        private List<GameObject> m_linePool;
        
        //draw a line from the latest point to the mouse
        private void Awake()
        {
            Reset();
        }

        void Reset()
        {
            m_mouseLine = Instantiate(lineDrawPrefab, Vector3.zero, Quaternion.identity, transform).GetComponent<LineRenderer>();
            m_mouseLine.gameObject.SetActive(false);
            m_selectionSystem = GetComponent<SelectionSystem>();
            m_linePool = new List<GameObject>();
        }

        void OnEnable()
        {
            SelectionSystem.DotSelected += DrawLines;
            SelectionSystem.ConnectionAdded += DrawLineBetweenTiles;
            SelectionSystem.SelectionReversed += RemoveLastLine;
            Tile.SelectionEnded += RemoveLines;
        }

        void OnDisable()
        {
            SelectionSystem.DotSelected -= DrawLines;
            SelectionSystem.ConnectionAdded -= DrawLineBetweenTiles;
            SelectionSystem.SelectionReversed -= RemoveLastLine;
            Tile.SelectionEnded -= RemoveLines;
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
        void DrawLines(Dot dot)
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

        void DrawLineBetweenTiles(Tile a, Tile b)
        {
            
            var line = Instantiate(lineDrawPrefab, a.transform.position, Quaternion.identity, transform)
                .GetComponent<LineRenderer>();
            line.SetPosition(0,
                new Vector3(a.xIndex, a.yIndex, depth));
            line.SetPosition(1,
                new Vector3(b.xIndex, b.yIndex, depth));
            line.material.SetColor("_Color",m_lineColor);
            ChangeMouseLinePositionTo(new Vector3(a.xIndex,a.yIndex,depth));
            m_linePool.Add(line.gameObject);
            
        }

        void RemoveLastLine()
        {
            var lineToRemove = m_linePool[m_linePool.Count - 1];
            var newMouseOrigin = m_linePool[m_linePool.Count - 2].transform.position;
            ChangeMouseLinePositionTo(newMouseOrigin);
            m_linePool.RemoveAt(m_linePool.Count-1);
            //get the line's position
            
            lineToRemove.SetActive(false);
        }

        void RemoveLines()
        {
            m_mouseLine.gameObject.SetActive(false);
            
            if (m_linePool.Count < 1) return;
            //deactivate all the objects
             foreach(GameObject line in m_linePool)
             {
                 line.SetActive(false);
             }
        }
        
        
        
    }
}
