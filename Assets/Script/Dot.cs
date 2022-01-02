using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dots
{
    [RequireComponent(typeof(MeshRenderer))]
    public class Dot : MonoBehaviour
    {
        public int xIndex;
        public int yIndex;
        public DotType type;
        private MaterialPropertyBlock m_materialProps;

        public void Init(int x, int y, DotType dotType)
        {
            xIndex = x;
            yIndex = y;
            type = dotType;
            m_materialProps = new MaterialPropertyBlock();
            m_materialProps.SetColor("_DotColor", type.color);
            MeshRenderer renderer = GetComponent<MeshRenderer>();
            renderer.SetPropertyBlock(m_materialProps);
        }
    }
}