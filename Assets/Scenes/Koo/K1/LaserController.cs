using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class LaserController : NetworkBehaviour
{
    private Vector3 _startPoint;
    private Vector3 _endPoint;
    private float _moveDuration;
    private float _waitAtEndTime;
    private float _waitAtStartTime;
    private ColorType _laserColor;
    private bool _laserVisible;
    private Rigidbody _rigidbody;

    [SerializeField] private MeshRenderer _lazer;
    
    private bool _movingToEnd = true;

    public void Initialize(Vector3 startPoint, Vector3 endPoint, float moveDuration, float waitAtStartTime, float waitAtEndTime, bool movingToEnd, ColorType laserColor, bool laserVisible)
    {
        initializeClientRpc(laserColor, laserVisible);
        
        _startPoint = startPoint;
        _endPoint = endPoint;
        _moveDuration = moveDuration;
        _waitAtStartTime = waitAtStartTime;
        _waitAtEndTime = waitAtEndTime;
        
        _movingToEnd = movingToEnd;
        
        StartCoroutine(MoveRazer());
    }

    private IEnumerator MoveRazer()
    {
        while (true)
        {
            Vector3 endPosition = _movingToEnd ? _endPoint : _startPoint;
            Vector3 startPosition = _movingToEnd ? _startPoint : _endPoint;
            
            float elapsedTime = 0f;

            while (elapsedTime < _moveDuration)
            {
                transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / _moveDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            transform.position = endPosition;
            _movingToEnd = !_movingToEnd;
            
            yield return new WaitForSeconds(_movingToEnd ? _waitAtStartTime : _waitAtEndTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out PlayerController playerController) && playerController.Color == _laserColor && IsServer)
        {
            if (playerController.IsOwner)
            {
                _rigidbody = playerController.GetComponent<Rigidbody>();
                _rigidbody.MovePosition(GameObject.FindWithTag("Blue Spawn Point").transform.position);
            }
            else
            {
                RespawnClientRpc();
            }
        }
    }

    [ClientRpc(RequireOwnership = false)]
    private void initializeClientRpc(ColorType lazerColor, bool lazerVisible)
    {
        _laserColor = lazerColor;
        _laserVisible = lazerVisible;
        
        PlayerController.LocalPlayerCreated += () =>
        {
            if (PlayerController.LocalPlayer.Color != _laserColor && !_laserVisible)
            {
                _lazer.enabled = false;
            }
            ;
        };
    }

    [ClientRpc(RequireOwnership = false)]
    private void RespawnClientRpc()
    {
        if (IsServer) return;
        _rigidbody = PlayerController.LocalPlayer.GetComponent<Rigidbody>();
        _rigidbody.MovePosition(GameObject.FindWithTag("Red Spawn Point").transform.position);
    }
}