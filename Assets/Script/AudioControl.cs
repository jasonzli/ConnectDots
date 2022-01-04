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
        private const int SQUARE_OCTAVE_NOTES = 8;
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
            
            return Mathf.Log(notes)*.1f + 1f;
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
            //ReturnAllObjects();
            m_notesPlayed++;
            
            var sound = m_audioSamplePool.GetPrefabInstance().GetComponent<GameSound>();
            sound.AudioClip.pitch = LogarithmicPitchAdjustment(m_notesPlayed);
            sound.AudioClip.volume = .6f;
            sound.AudioClip.Play();

            
            m_soundObjects.Add(sound);
        }

        void PlaySquareSound(Dot dot)
        {
            ReturnAllObjects();
            for (int i = 0; i < SQUARE_OCTAVE_NOTES; i++)
            {
                m_notesPlayed++;
                
                var gameSound = m_audioSamplePool.GetPrefabInstance().GetComponent<GameSound>();
                gameSound.AudioClip.pitch = LogarithmicPitchAdjustment(m_notesPlayed);
                gameSound.AudioClip.volume = .6f;

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
            m_notesPlayed = m_notesPlayed - SQUARE_OCTAVE_NOTES - 1; //4 for the square and 1 for the new selection
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
