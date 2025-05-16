using Cinemachine;
using Possessable;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 플레이어 오디오를 조작하는 Class
/// </summary>
public class PlayerAudioController : MonoBehaviour
{
    [SerializeField] AudioSource _footstepAudioSource;
    [SerializeField] AudioSource _jumpAudioSource;
    [SerializeField] AudioSource _landAudioSource;
    [SerializeField] AudioSource _hitAudioSource;

    [SerializeField] AudioClip[] _footstepClips;
    [SerializeField] AudioClip[] _hitClips;

    private PlayerController _playerController;
    private SkinnedMeshRenderer _meshRenderer;
    private Animator _animator;

    private AudioClip _jumpClip;

    private float _footStepAudioCooldown;
    private float _hitAudioCooldown;

    private void Start()
    {
        _meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        _animator = GetComponent<Animator>();

        _jumpClip = _jumpAudioSource.clip;
    }

    private void Update()
    {
        if (_footStepAudioCooldown > 0f)
        {
            _footStepAudioCooldown -= Time.deltaTime;
        }

        if (_hitAudioCooldown > 0f)
        {
            _hitAudioCooldown -= Time.deltaTime;
        }
    }

    public void SetPlayerController(PlayerController playerController)
    {
        _playerController = playerController;
    }

    /// <summary>
    /// 발걸음 소리를 재생한다.
    /// </summary>
    public void PlayFootstepSound()
    {
        if (_meshRenderer.enabled && _footStepAudioCooldown <= 0f)
        {
            // Transition 과정에서 소리가 중복 재생되는 것을 방지.
            if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Land Blend Tree") && _animator.GetFloat("Velocity") < 0.1f)
            {
                return;
            }

            _footstepAudioSource.PlayOneShot(_footstepClips[Random.Range(0, _footstepClips.Length)], _footstepAudioSource.volume);

            _footStepAudioCooldown = 0.1f;
        }
    }

    /// <summary>
    /// 점프 소리를 재생한다.
    /// </summary>
    public void PlayJumpSound()
    {
        _jumpAudioSource.PlayOneShot(_jumpClip, _jumpAudioSource.volume);
    }

    /// <summary>
    /// 착지 소리를 재생한다.
    /// </summary>
    public void PlayLandSound()
    {
        if (!(_playerController.InteractableInHand is PossessableController))
        {
            _landAudioSource.PlayOneShot(_landAudioSource.clip, _landAudioSource.volume);
        }
    }

    /// <summary>
    /// 부딪히는 소리를 재생한다.
    /// </summary>
    /// <param name="impulse">부딪힘 강도</param>
    public void PlayHitSound(float impulse)
    {
        if (_hitAudioCooldown > 0f)
        {
            return;
        }

        float threshold = 96.0f;

        // 플레이어가 빙의한 상태일 때는 threshold를 낮게 잡는다.
        if (_playerController.InteractableInHand is PossessableController)
        {
            threshold = 48.0f;
        }
        
        if (impulse < threshold)
        {
            return;
        }

        int clipIndex = Random.Range(0, _hitClips.Length);
        float volume = _hitAudioSource.volume * (impulse / threshold);
        _hitAudioSource.PlayOneShot(_hitClips[clipIndex], volume);

        _hitAudioCooldown = 0.1f;
    }
}