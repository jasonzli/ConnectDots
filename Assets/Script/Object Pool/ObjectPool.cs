using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Dots{

    //An object pool based on patrykgalach's prefab approach
    //extended to have more standard Instiate-like behavior
    public class ObjectPool : MonoBehaviour
    {
        public PoolableObject prefab;

        private Stack<PoolableObject> objectPool = new Stack<PoolableObject>();

        public PoolableObject GetPrefabInstance(Transform parent = null)
        {
            PoolableObject instance;
            if (objectPool.Count > 0)
            {
                instance = objectPool.Pop();
                instance.gameObject.SetActive(true);
            }
            else
            {
                instance = Instantiate(prefab);
                instance.GetComponent<PoolableObject>().origin = this;
            }

            instance.transform.position = Vector3.zero;
            instance.transform.SetParent(parent);

            return instance;
        }
        public PoolableObject GetPrefabInstance(Vector3 position, Quaternion orientation)
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
            
            instance.transform.position = position;
            instance.transform.rotation = orientation;

            return instance;
        }
        public PoolableObject GetPrefabInstance(Vector3 position, Quaternion orientation,Transform parent)
        {
            PoolableObject instance;
            if (objectPool.Count > 0)
            {
                instance = objectPool.Pop();
                instance.gameObject.SetActive(true);
            }
            else
            {
                instance = Instantiate(prefab);
                instance.GetComponent<PoolableObject>().origin = this;
            }
            
            instance.transform.position = position;
            instance.transform.rotation = orientation;
            instance.transform.SetParent(parent);

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