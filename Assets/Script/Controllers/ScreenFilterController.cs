using System;
using System.Collections;
using System.Collections.Generic;
using Dots;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

namespace Dots
{
    /// <summary>
    /// A class to control the alpha layer that appears when a square is made
    /// Pass parameters to the shader
    /// </summary>
    public class ScreenFilterController : MonoBehaviour
    {
        public FloatParameter alphaIntensity;
        private Image m_imageRenderer;

        private void Awake()
        {
            m_imageRenderer = GetComponent<Image>();
        }

        private void OnEnable()
        {
            SelectionSystem.SquareFound += MatchSquareColor;
            SelectionSystem.SquareRemoved += OnSquareRemoved;
            Tile.SelectionEnded += ClearFilter;
        }

        private void OnDisable()
        {
            SelectionSystem.SquareFound -= MatchSquareColor;
            SelectionSystem.SquareRemoved -= OnSquareRemoved;
            Tile.SelectionEnded -= ClearFilter;
        }

        void MatchSquareColor(Dot dot)
        {
            var color = dot.type.color;
            color.a = alphaIntensity.value;
            m_imageRenderer.color = color;
        }

        void OnSquareRemoved(int squaresRemaining)
        {
            if (squaresRemaining > 0) return;

            ClearFilter();
        }

        void ClearFilter()
        {
            m_imageRenderer.color = Color.clear;
        }
    }
}

