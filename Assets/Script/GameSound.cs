using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dots
{
    /// <summary>
    /// A game sound that plays until it isn't so it disappears gracefully
    /// </summary>
    public class GameSound : PoolableObject
    {
        [SerializeField]
        private AudioSource audioClip;
        public AudioSource AudioClip { get; private set; }

        void Awake()
        {
            AudioClip = GetComponent<AudioSource>();
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
