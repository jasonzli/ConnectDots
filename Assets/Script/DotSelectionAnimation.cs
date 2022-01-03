using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Dots
{
    [RequireComponent(typeof(MeshRenderer))]
    public class DotSelectionAnimation : MonoBehaviour
    {
        public FloatParameter animationTime;
        public FloatParameter expansionSize;
        public FloatParameter startingAlpha;
        public CurveParameter expansionAnimationCurve;
        public CurveParameter fadeAnimationCurve;
        private float m_startingSize;
        private Color m_effectColor;
        private MeshRenderer m_renderer;
        private MaterialPropertyBlock m_materialProps;
        
        void Awake()
        {
            m_renderer = GetComponent<MeshRenderer>();
            m_materialProps = new MaterialPropertyBlock();
        }

        public void Init(DotType type)
        {
            m_startingSize = type.dotSize.value * .95f;//a little smaller
            m_effectColor = type.color;
            m_effectColor.a = startingAlpha.value; 
            
            m_materialProps.SetColor("_DotColor", m_effectColor);
            m_materialProps.SetFloat("_DotRadius", m_startingSize);
            UpdateShader();
        }

        //child of game object, this should be a coroutine
        public void Animate()
        {
            StartCoroutine(FadeAnimation());
        }

        IEnumerator FadeAnimation()
        {
            ResetEffect(); //in case something modified it
            //Do a shader animation
            float elapsedTime = 0f;
            float t = 0f,expansionT = 0f,fadeT = 0f,animatedDotSize = 0f,animatedFadeAmount = 0f;
            while (elapsedTime < animationTime.value)
            {
                t = Mathf.Clamp(elapsedTime / animationTime.value, 0, 1);

                expansionT = expansionAnimationCurve.Evaluate(t);
                fadeT = fadeAnimationCurve.Evaluate(t);

                animatedDotSize = expansionSize.value * expansionT;
                animatedFadeAmount = startingAlpha.value  * fadeT;

                m_effectColor.a = animatedFadeAmount;
                
                //set the material properties
                m_materialProps.SetColor("_DotColor", m_effectColor);
                m_materialProps.SetFloat("_DotRadius", m_startingSize + animatedDotSize);
                
                //update renderer
                UpdateShader();
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            ResetEffect(); // to clear it
        }

        void UpdateShader()
        {
            m_renderer.SetPropertyBlock(m_materialProps);
        }

        public void ResetEffect()
        {
            m_effectColor.a = startingAlpha.value;
            m_materialProps.SetColor("_DotColor", m_effectColor);
            m_materialProps.SetFloat("_DotRadius",m_startingSize);
            m_renderer.SetPropertyBlock(m_materialProps);
        }
        
        
    }
}