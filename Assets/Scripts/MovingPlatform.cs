using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 이동 플랫폼 테스트용 Class
/// </summary>
public class MovingPlatform : NetworkBehaviour
{
    private Rigidbody _rigidbody;

    private float _timer;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        _timer += Time.fixedDeltaTime;

        // 일정 시점마다 서버 측에서 동기화 RPC를 전송
        if (IsServer && _timer > Mathf.PI * 2f)
        {
            _timer -= Mathf.PI * 2f;
            UpdatePositionClientRpc(_timer, _rigidbody.position);
        }

        Vector3 newPosition = transform.position;
        //newPosition.y = (Mathf.PingPong(_timer, Mathf.PI) * 5f) - 3f;
        newPosition.y = (Mathf.Sin(_timer) * 35f) + 40f;

        _rigidbody.MovePosition(Vector3.Lerp(_rigidbody.position, newPosition, Time.fixedDeltaTime * 20f));
    }

    [ClientRpc]
    void UpdatePositionClientRpc(float timer, Vector3 position)
    {
        _timer = timer;
        GetComponent<Rigidbody>().MovePosition(position);
    }
}
