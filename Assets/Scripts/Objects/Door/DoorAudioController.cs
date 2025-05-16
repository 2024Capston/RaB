using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 문 오디오를 조작하는 Class
/// </summary>
public class DoorAudioController : MonoBehaviour
{
    [SerializeField] private AudioSource _openAudioSource;
    [SerializeField] private AudioSource _closeAudioSource;

    float _openVolume;
    float _closeVolume;

    float _openCooldown = 0.0f;     // 열리는 소리의 지속 시간
    float _closeCooldown = 0.0f;    // 닫히는 소리의 지속 시간

    private void Start()
    {
        _openVolume = _openAudioSource.volume;
        _closeVolume = _closeAudioSource.volume;
    }

    private void Update()
    {
        if (_openCooldown > 0.0f)
        {
            _openCooldown -= Time.deltaTime;

            if (_openCooldown < 0.0f)
            {
                _openCooldown = 0.0f;
            }

            // 남은 시간에 따라 볼륨을 조절한다.
            _openAudioSource.volume = _openVolume * _openCooldown * _openCooldown;
        }

        if (_closeCooldown > 0.0f)
        {
            _closeCooldown -= Time.deltaTime;

            if (_closeCooldown < 0.0f)
            {
                _closeCooldown = 0.0f;
            }

            // 남은 시간에 따라 볼륨을 조절한다.
            _closeAudioSource.volume = _closeVolume * _closeCooldown * _closeCooldown;
        }
    }

    /// <summary>
    /// 문이 열리는 소리를 재생한다.
    /// </summary>
    public void PlayOpenSound()
    {
        _openAudioSource.volume = _openVolume;
        _openAudioSource.pitch = Random.Range(2.5f, 3.0f);
        _openAudioSource.PlayOneShot(_openAudioSource.clip, _openAudioSource.volume);

        _openCooldown = 1.0f;
    }

    /// <summary>
    /// 문이 닫히는 소리를 재생한다.
    /// </summary>
    public void PlayCloseSound()
    {
        _closeAudioSource.volume = _closeVolume;
        _openAudioSource.pitch = Random.Range(1.0f, 1.5f);
        _closeAudioSource.PlayOneShot(_closeAudioSource.clip, _closeAudioSource.volume);

        _closeCooldown = 1.0f;
    }
}
