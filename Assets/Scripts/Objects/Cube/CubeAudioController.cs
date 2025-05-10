using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 큐브 오디오를 조작하는 Class
/// </summary>
public class CubeAudioController : NetworkBehaviour
{
    [SerializeField] AudioSource _hitAudioSource;
    [SerializeField] AudioSource _colorChangeAudioSource;

    [SerializeField] AudioClip[] _hitClips;             // 큐브가 부딪힐 때 나는 소리
    [SerializeField] AudioClip[] _colorChangeClips;     // 큐브 색깔이 바뀔 때 나는 소리

    private float _hitAudioCooldown = 0.0f;

    private void Update()
    {
        if (_hitAudioCooldown > 0.0f)
        {
            _hitAudioCooldown -= Time.deltaTime;
        }
    }

    /// <summary>
    /// 색깔이 바뀌는 소리를 재생한다.
    /// </summary>
    public void PlayColorChangeSound()
    {
        _colorChangeAudioSource.PlayOneShot(_colorChangeClips[Random.Range(0, _colorChangeClips.Length)], _colorChangeAudioSource.volume);
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (!IsOwner)
        {
            return;
        }

        if (_hitAudioCooldown <= 0.0f && collision.impulse.magnitude > 1024.0f)
        {
            // Collision 충격 정도에 따라 볼륨을 조정한다.
            int clipIndex = Random.Range(0, _hitClips.Length);
            float volume = _hitAudioSource.volume * (collision.impulse.magnitude / 1600.0f);
            _hitAudioSource.PlayOneShot(_hitClips[clipIndex], volume);

            _hitAudioCooldown = 0.1f;

            // 상대 플레이어도 소리를 재생한다.
            if (IsServer)
            {
                PlayHitSoundClientRpc(clipIndex, volume);
            }
            else
            {
                PlayHitSoundServerRpc(clipIndex, volume);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayHitSoundServerRpc(int clipIndex, float volume)
    {
        _hitAudioSource.PlayOneShot(_hitClips[clipIndex], volume);
    }

    [ClientRpc(RequireOwnership = false)]
    private void PlayHitSoundClientRpc(int clipIndex, float volume)
    {
        if (IsServer)
        {
            return;
        }

        _hitAudioSource.PlayOneShot(_hitClips[clipIndex], volume);
    }
}
