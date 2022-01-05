using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Vector3 = UnityEngine.Vector3;

namespace Dots
{
    [RequireComponent(typeof(MeshRenderer))]
    public class Dot : MonoBehaviour
    {
        public int xIndex;
        public int yIndex;
        public DotType type;
        public bool isAnimating = false;
        public GameObject selectionAnimationObject;
        
        //Used for shader animation
        private MaterialPropertyBlock m_materialProps;
        private MeshRenderer m_renderer;
        private DotSelectionAnimation m_selectionEffect;

        void OnEnable()
        {
            SelectionSystem.DotSelected += OnSelected;
            SelectionSystem.SquareFound += OnSquare;
        }

        void OnDisable()
        {
            SelectionSystem.DotSelected -= OnSelected;
            SelectionSystem.SquareFound -= OnSquare;
        }
        
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
            m_selectionEffect = selectionAnimationObject.GetComponent<DotSelectionAnimation>();
            m_selectionEffect.Init(type);
        }

        //For reinsertion into the board
        public void SetCoord(int x, int y)
        {
            xIndex = x;
            yIndex = y;
        }

        //Selection behavior
        void OnSelected(Dot dot)
        {
            if (gameObject != dot.gameObject) return; //not the same dot, skip

            m_selectionEffect.Animate();
        }
        
        //If square found
        void OnSquare(Dot dot)
        {
            if (type != dot.type) return;

            m_selectionEffect.Animate();
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

        public async Task MoveToPosition(Vector3 targetPosition, float moveInTime = .5f)
        {
            if (isAnimating) return;

            isAnimating = true;

            await MoveTo(targetPosition, moveInTime);

            isAnimating = false;
        }
        
        private async Task MoveTo(Vector3 targetPosition, float moveInTime = .5f)
        {
            var origin = transform.position;
            await AnimateUsingCurve(type.moveAnimation, origin, targetPosition, moveInTime);
        }

        
        //Drop into place using Drop Curve
        private async Task DropTo(Vector3 targetPosition, Vector3 offsetPosition, float dropInTime = .5f)
        {
            await AnimateUsingCurve(type.dropAnimation, offsetPosition, targetPosition, dropInTime);
        }

        private async Task AnimateUsingCurve(CurveParameter animationCurve, Vector3 origin, Vector3 target, float animationTime = .5f)
        {
            var elapsedTime = 0f;
            var t = 0f;
            while (elapsedTime < animationTime)
            {
                t = Mathf.Clamp(elapsedTime / animationTime, 0f, 1f);
                
                //remap if curve exists
                if (animationCurve)
                {
                    t = animationCurve.Evaluate(t);
                }
                
                //Move
                transform.position = Vector3.Lerp(origin, target, t);

                elapsedTime += Time.deltaTime;
                await Task.Yield();
            }

            transform.position = target;
        }
        
        
    }
}