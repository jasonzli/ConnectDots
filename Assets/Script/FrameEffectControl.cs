using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Dots
{
    //To control the fill of the frame we have this script
    //We can control this with a single value, the fill amount
    public class FrameEffectControl : MonoBehaviour
    {
        public Material FrameEffectMaterial;
        public IntParameter SelectionsToFillFrame;
        public CurveParameter FillInterpolationCurve;
        public FloatParameter FillTime;
        private int m_selections;
        private Color m_frameColor;
        private bool isAnimating;

        void Awake()
        {
            Reset();
        }
        private void OnEnable()
        {
            SelectionSystem.DotSelected += IncrementFrame;
            SelectionSystem.SquareFound += FillFrame;
            SelectionSystem.SquareRemoved += RemoveSquareAdditions;
            SelectionSystem.SelectionReversed += DecrementFrame;
            Tile.SelectionEnded += Reset;
        }

        private void OnDisable()
        {
            SelectionSystem.DotSelected -= IncrementFrame;
            SelectionSystem.SquareFound -= FillFrame;
            SelectionSystem.SquareRemoved -= RemoveSquareAdditions;
            SelectionSystem.SelectionReversed -= DecrementFrame;
        }

        void Reset()
        {
            m_frameColor = Color.clear;
            m_selections = 0;
            isAnimating = false;
            FrameEffectMaterial.SetColor("_Color", m_frameColor);
            FrameEffectMaterial.SetFloat("_FillAmount", FillAmount());
        }
        
        //Just use a coroutine because the things that call it aren't async

        private void AnimateFill()
        {
            StartCoroutine(AnimateFrameFilling());
        }
        
        IEnumerator AnimateFrameFilling()
        {
            if (!isAnimating)
            {
                isAnimating = true;
                var currentFill = FrameEffectMaterial.GetFloat("_FillAmount");
                var newFill = FillAmount();
                var elapsedTime = 0f;
                var t = 0f;
                while (elapsedTime < FillTime.value)
                {
                    newFill = FillAmount(); //update just in case of more lines
                    
                    //Map t
                    t = elapsedTime / FillTime.value;
                    t = FillInterpolationCurve.Evaluate(t);
                    
                    FrameEffectMaterial.SetFloat("_FillAmount", Mathf.Lerp(currentFill,newFill,t));
                    elapsedTime += Time.deltaTime;
                
                    yield return null;
                }
                FrameEffectMaterial.SetFloat("_FillAmount", FillAmount());
                isAnimating = false;
            }

        }
        
        
        void IncrementFrame(Dot dot)
        {
            m_selections++;
            if (m_frameColor != dot.type.color)
            {
                m_frameColor = dot.type.color;
                FrameEffectMaterial.SetColor("_Color", m_frameColor);
            }
            AnimateFill();
        }

        void DecrementFrame()
        {
            m_selections -= 2; //has to be doubled
            AnimateFill();
        }

        void FillFrame(Dot squareDot)
        {
            m_selections += 10; //to automatically fill the frame
            AnimateFill();
        }

        void RemoveSquareAdditions(int squaresFound)
        {
            m_selections -= 10;
            AnimateFill();

        }

        float FillAmount()
        {
            return (float) m_selections / SelectionsToFillFrame.value;
        }
    }
}
