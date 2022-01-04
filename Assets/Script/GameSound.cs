using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dots
{
    public class GameSound : PoolableObject
    {
        [SerializeField]
        private AudioSource audioClip;
        public AudioSource AudioClip { get; private set; }

        void Awake()
        {
            AudioClip = GetComponent<AudioSource>();
        }

        void OnEnable()
        {
            
        }
        void Update()
        {
            if (!AudioClip.isPlaying)
            {
                origin.ReturnToPool(this);
            }
        }

        public void StopAudio()
        {
            AudioClip.Stop();
            origin.ReturnToPool(this);
        }
        
        
    }
}
