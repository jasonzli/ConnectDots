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
        private int m_notesPlayed;
        
        void Awake()
        {
            m_audioSamplePool = Instantiate(audioPoolPrefab, Vector3.zero, Quaternion.identity, transform).GetComponent<ObjectPool>();
            m_audioSamplePool.name = "Audio Effects Pool";
            m_soundObjects = new List<GameSound>();
            Reset();
        }

        void Reset()
        {
            m_notesPlayed = 0;
            //ReturnAllObjects();
            //m_soundObjects.Clear();
        }

        float LogarithmicPitchAdjustment(int notes)
        {
            if (notes == 0) return 1f;
            
            return Mathf.Log(notes)*.2f + 1f;
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
            m_notesPlayed++;
            
            var sound = m_audioSamplePool.GetPrefabInstance();
            sound.transform.position = transform.position;
            var gameSound = sound.gameObject.GetComponent<GameSound>();
            gameSound.AudioClip.pitch = LogarithmicPitchAdjustment(m_notesPlayed);
            gameSound.AudioClip.volume = .5f;
            gameSound.AudioClip.Play();

            
            m_soundObjects.Add(gameSound);
        }

        void PlaySquareSound(Dot dot)
        {
            int chordPitch = m_notesPlayed;
            for (int i = 0; i < 4; i++)
            {
                m_notesPlayed++;
                var sound = m_audioSamplePool.GetPrefabInstance();
                sound.transform.position = transform.position;
                var gameSound = sound.gameObject.GetComponent<GameSound>();
                gameSound.AudioClip.pitch = LogarithmicPitchAdjustment(m_notesPlayed);
                gameSound.AudioClip.volume = 1.5f;//total volume is 1.2f

                gameSound.AudioClip.Play();
                
                m_soundObjects.Add(gameSound);
                
            }
        }

        void DecrementPitch()
        {
            m_notesPlayed = m_notesPlayed - 1 - 1; //one for the removal and one for the new selection
        }
        
        void DecrementSquarePitch(int squaresRemaining)
        {
            m_notesPlayed = m_notesPlayed - 4 - 1; //4 for the square and 1 for the new selection
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
