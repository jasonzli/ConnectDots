using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Dots
{
    [RequireComponent(typeof(MeshRenderer))]
    public class Dot : MonoBehaviour
    {
        public int xIndex;
        public int yIndex;
        public DotType type;
        public bool isAnimating = false;
        
        //Used for shader animation
        private MaterialPropertyBlock m_materialProps;
        private MeshRenderer m_renderer;

        public void Init(int x, int y, DotType dotType)
        {
            SetCoord(x, y);
            type = dotType;
            isAnimating = false;
            m_materialProps = new MaterialPropertyBlock();
            m_materialProps.SetColor("_DotColor", type.color);
            m_materialProps.SetFloat("_DotRadius", type.dotSize.value);
            m_renderer = GetComponent<MeshRenderer>();
            m_renderer.SetPropertyBlock(m_materialProps);
        }

        //For reinsertion into the board
        public void SetCoord(int x, int y)
        {
            xIndex = x;
            yIndex = y;
        }

        //Disappear over time, can be set
        public async Task Clear(float clearTime = 0.15f)
        {
            if (isAnimating) return;

            isAnimating = true;
            
            var elapsedTime = 0.0f;
            while (elapsedTime < clearTime)
            {
                float t = Mathf.Clamp(elapsedTime / clearTime, 0f, 1f);

                //Map t value
                if (type.clearAnimation)
                {
                    t = type.clearAnimation.Evaluate(t);
                }
                else
                {
                    t = 1f - t;
                }
                
                //Animate the shader
                m_materialProps.SetFloat("_DotRadius", type.dotSize.value * t);
                
                m_renderer.SetPropertyBlock(m_materialProps);
                
                elapsedTime += Time.deltaTime;
                
                await Task.Yield();
            }
            
            isAnimating = false;
        }

        //Called externally to trigger the animation *does not affect the actual grid coordinate, which is the xyIndex
        public async Task DropToPosition(Vector3 targetPosition, Vector3 offsetPosition, float dropInTime = .5f, float delayTime = 0f)
        {
            if (isAnimating) return;

            isAnimating = true;
            
            transform.position = offsetPosition; //needed to prevent dot from staying at the target position
            await Task.Delay( (int) (1000*delayTime) ); //wait for the delay
            await DropTo(targetPosition, offsetPosition, dropInTime); //animate
            
            isAnimating = false;
        }
        
        //Drop into place using Drop Curve
        private async Task DropTo(Vector3 targetPosition, Vector3 offsetPosition, float dropInTime = .5f)
        {
            var elapsedTime = 0.0f;
            while (elapsedTime < dropInTime)
            {
                float t = Mathf.Clamp(elapsedTime / dropInTime, 0f, 1f);
                
                //Map t value
                if (type.dropAnimation)
                {
                    t = type.dropAnimation.Evaluate(t);
                }
                else //Linear
                {
                    t = t;
                }
                
                //Move
                transform.position = Vector3.Lerp(offsetPosition, targetPosition, t);
                
                elapsedTime += Time.deltaTime;
                await Task.Yield();
            }

            transform.position = targetPosition;
        }
        
        
    }
}