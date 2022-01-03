using UnityEngine;

namespace Dots
{
    public class PoolableObject : MonoBehaviour
    {
        public ObjectPool origin;

        public virtual void ReturnToPool()
        {
            origin.ReturnToPool(this);
        }
    }
}