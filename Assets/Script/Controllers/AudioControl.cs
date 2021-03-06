using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dots
{
    /// <summary>
    /// A class that plays a single tone and pitch shifts logarithmically to higher tones
    /// Uses a pool to generate and play sound objects
    /// A patch job because I only have one tone.
    /// TODO get a proper arpeggio of tones to play
    /// </summary>
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
        }

        float LogarithmicPitchAdjustment(int notes)
        {
            if (notes == 0) return 1f;
            
            return Mathf.Log(notes)*pitchIncrement.value + 1f;
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

        void DecrementPitch(Tile head)
        {
            m_notesPlayed = m_notesPlayed - 1; 
        }

        void DecrementSquarePitch(int squaresRemaining)
        {
            m_notesPlayed = m_notesPlayed - SQUARE_OCTAVE_NOTES;
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
