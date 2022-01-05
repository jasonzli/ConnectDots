using UnityEngine;

namespace Dots
{
    /// <summary>
    /// Objects that work in the pool
    /// </summary>
    public class PoolableObject : MonoBehaviour
    {
        public ObjectPool origin;

        public virtual void ReturnToPool()
        {
            origin.ReturnToPool(this);
        }
    }
}