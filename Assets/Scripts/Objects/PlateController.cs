using UnityEngine;
using Unity.Netcode;

using UnityEngine.Events;

/// <summary>
/// 발판을 조작하는 Class
/// </summary>
public class PlateController : NetworkBehaviour
{
    /// <summary>
    /// 발판 옆에 표시될 빛 색깔
    /// </summary>
    [SerializeField] ColorType _color;
    
    /// <summary>
    /// 발판에 물체가 들어오면 호출할 이벤트
    /// </summary>
    [SerializeField] UnityEvent<PlateController, Collider> _eventsOnEnter;

    /// <summary>
    /// 발판에서 물체가 나가면 호출할 이벤트
    /// </summary>
    [SerializeField] UnityEvent<PlateController, Collider> _eventsOnExit;

    /// <summary>
    /// 발판 옆의 빛에 대한 레퍼런스
    /// </summary>
    [SerializeField] MeshRenderer _lightMeshRenderer;

    private Animator _animator;

    private int counter = 0;

    public override void OnNetworkSpawn()
    {
        _animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if  (!IsServer)
        {
            return;
        }

        _eventsOnEnter.Invoke(this, other);

        // 발판에 물체가 하나 이상 있으면 누르기
        if (counter++ == 0)
        {
            SetPressStateClientRpc(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer)
        {
            return;
        }

        _eventsOnExit.Invoke(this, other);

        // 발판에 물체가 남아 있지 않으면 올리기
        if (--counter == 0)
        {
            SetPressStateClientRpc(false);
        }
    }

    [ClientRpc]
    private void SetPressStateClientRpc(bool isPressed) {
        if (isPressed)
        {
            _animator.SetBool("IsPressed", true);
        }
        else
        {
            _animator.SetBool("IsPressed", false);
        }
    }
}
