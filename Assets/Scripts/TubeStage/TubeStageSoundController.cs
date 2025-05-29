using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TubeStage
{
    public class TubeStageSoundController : MonoBehaviour
    {
        [SerializeField] private AudioSource _correctSFX;
        [SerializeField] private AudioSource _incorrectSFX;
        [SerializeField] private AudioSource _tictacSFX;
        
        public void PlayCorrectSFX()
        {
            _correctSFX.PlayOneShot(_correctSFX.clip, _correctSFX.volume);
        }

        public void PlayInCorrectSFX()
        {
            _incorrectSFX.PlayOneShot(_incorrectSFX.clip, _incorrectSFX.volume);
        }

        public void PlayTicTacSFX()
        {
            _tictacSFX.Play();
        }

        public void StopTicTacSFX()
        {
            _tictacSFX.Stop();
        }
    }
}

