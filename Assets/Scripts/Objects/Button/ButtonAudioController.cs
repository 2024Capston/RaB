using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 버튼 오디오를 조작하는 Class
/// </summary>
public class ButtonAudioController : MonoBehaviour
{
    [SerializeField] private AudioSource _pressAudioSource;
    [SerializeField] private AudioSource _unpressAudioSource;

    private bool _playPressSound = true;

    /// <summary>
    /// 버튼이 내려가는 소리를 재생한다.
    /// </summary>
    public void PlayPressSound()
    {
        _pressAudioSource.PlayOneShot(_pressAudioSource.clip, _pressAudioSource.volume);
    }

    /// <summary>
    /// 버튼이 올라가는 소리를 재생한다.
    /// </summary>
    public void PlayUnpressSound()
    {
        _unpressAudioSource.PlayOneShot(_unpressAudioSource.clip, _unpressAudioSource.volume);
    }

    /// <summary>
    /// 버튼이 올라가는 소리와 내려가는 소리를 토글 형식으로 재생한다.
    /// </summary>
    public void PlayToggleSound()
    {
        if (_playPressSound)
        {
            _pressAudioSource.PlayOneShot(_pressAudioSource.clip, _pressAudioSource.volume);
        }
        else
        {
            _unpressAudioSource.PlayOneShot(_unpressAudioSource.clip, _unpressAudioSource.volume);
        }

        _playPressSound = !_playPressSound;
    }
}
