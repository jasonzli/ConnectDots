using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dots
{
    //A simple pick up piece audio sample
    public class AudioControl : MonoBehaviour
    {
        public GameObject audioPoolPrefab;
        public FloatParameter pitchIncrement;
        
        private ObjectPool m_audioSamplePool;
        private List<GameSound> m_soundObjects;
        private float m_pitch;
        
        void Awake()
        {
            m_audioSamplePool = Instantiate(audioPoolPrefab, Vector3.zero, Quaternion.identity, transform).GetComponent<ObjectPool>();
            m_audioSamplePool.name = "Audio Effects Pool";
            m_soundObjects = new List<GameSound>();
            Reset();
        }

        void Reset()
        {
            m_pitch = 1f;
            //ReturnAllObjects();
            //m_soundObjects.Clear();
        }

        void ReturnAllObjects()
        {
            foreach (GameSound soundObject in m_soundObjects)
            {
                soundObject.StopAudio();
            }
            m_soundObjects.Clear();
        }
        void PlaySelectionSound(Dot dot)
        {
            ReturnAllObjects();
            
            var sound = m_audioSamplePool.GetPrefabInstance();
            sound.transform.position = transform.position;
            var gameSound = sound.gameObject.GetComponent<GameSound>();
            gameSound.AudioClip.pitch = SamplePitch();
            gameSound.AudioClip.volume = .5f;
            gameSound.AudioClip.Play();
            m_pitch += pitchIncrement.value;
            m_soundObjects.Add(gameSound);
        }

        //Pitch only goes to 3 anyway, just repeat at that number
        float SamplePitch()
        {
            return Mathf.Clamp(m_pitch, 1f, 5f);
        }

        void PlaySquareSound(Dot dot)
        {
            float[] chordPitches = new float[4];
            
            if (m_pitch > 5f)
            {
                for (int i = 0; i < 4; i++)
                {
                    chordPitches[i] = m_pitch- (float) i * 2 * pitchIncrement.value;
                }
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    chordPitches[i] = m_pitch;
                    m_pitch += pitchIncrement.value;
                }
            }
            
            for (int i = 0; i < 4; i++)
            {
                var sound = m_audioSamplePool.GetPrefabInstance();
                sound.transform.position = transform.position;
                var gameSound = sound.gameObject.GetComponent<GameSound>();
                gameSound.AudioClip.pitch = chordPitches[i];
                gameSound.AudioClip.volume = .25f;
                gameSound.AudioClip.Play();
                m_soundObjects.Add(gameSound);
                
            }
        }

        void DecrementPitch()
        {
            m_pitch -= 2*pitchIncrement.value;
        }
        
        void DecrementSquarePitch(int squaresRemaining)
        {
            m_pitch -= 4 * pitchIncrement.value;
        }
        void OnEnable()
        {
            SelectionSystem.DotSelected += PlaySelectionSound;
            SelectionSystem.SquareFound += PlaySquareSound;
            SelectionSystem.SelectionReversed += DecrementPitch;
            SelectionSystem.SquareRemoved += DecrementSquarePitch;
            Tile.SelectionEnded += Reset;
        }

        void OnDisable()
        {
            SelectionSystem.DotSelected -= PlaySelectionSound;
            SelectionSystem.SquareFound -= PlaySquareSound;
            SelectionSystem.SelectionReversed -= DecrementPitch;
            SelectionSystem.SquareRemoved -= DecrementSquarePitch;
            Tile.SelectionEnded -= Reset;
        }
        
    }
}
