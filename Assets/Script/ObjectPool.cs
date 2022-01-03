using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Dots{

    //It's exactly what you think it is, based on patrykgalach's approach

    public class ObjectPool : MonoBehaviour
    {
        public PoolableObject prefab;

        private Stack<PoolableObject> objectPool = new Stack<PoolableObject>();

        public PoolableObject GetPrefabInstance()
        {
            PoolableObject instance;
            if (objectPool.Count > 0)
            {
                instance = objectPool.Pop();
                instance.transform.SetParent(null);
                instance.gameObject.SetActive(true);
            }
            else
            {
                instance = Instantiate(prefab);
                instance.GetComponent<PoolableObject>().origin = this;
            }

            return instance;
        }

        public void ReturnToPool(PoolableObject instance)
        {
            instance.gameObject.SetActive(false);
            instance.transform.SetParent(transform);
            
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localScale = Vector3.one;
            instance.transform.localRotation = Quaternion.identity;
            
            objectPool.Push(instance);
        }
        
    }

    
}