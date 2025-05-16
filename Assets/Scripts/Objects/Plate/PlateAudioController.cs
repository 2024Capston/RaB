using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 발판 오디오를 조작하는 Class
/// </summary>
public class PlateAudioController : MonoBehaviour
{
    [SerializeField] private AudioSource _pressAudioSource;
    [SerializeField] private AudioSource _unpressAudioSource;

    /// <summary>
    /// 발판이 눌리는 소리를 재생한다.
    /// </summary>
    public void PlayPressSound()
    {
        _pressAudioSource.PlayOneShot(_pressAudioSource.clip, _pressAudioSource.volume);
    }

    /// <summary>
    /// 발판이 올라가는 소리를 재생한다.
    /// </summary>
    public void PlayUnpressSound()
    {
        _unpressAudioSource.PlayOneShot(_unpressAudioSource.clip, _unpressAudioSource.volume);
    }
}
