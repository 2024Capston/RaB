using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class LaserController1 : NetworkBehaviour
{
    
    [SerializeField] private ColorType _laserColor;
    [SerializeField] private bool _laserVisible;
     private Rigidbody _rigidbody;

    [SerializeField] private MeshRenderer _laser;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        Initialize(ColorType.Purple, true);
    }

    public void Initialize(ColorType laserColor, bool laserVisible)
    {
        initializeClientRpc(laserColor, laserVisible);

        if (IsHost)
        {
            StartCoroutine(MoveLaser());
        }
    }

    private IEnumerator MoveLaser()
    {
        while (true)
        {
            transform.rotation = Quaternion.Euler(new Vector3(0, 0.5f, 0)) * transform.rotation;
            yield return null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out PlayerController playerController) && IsServer)
        {
            if (playerController.IsOwner)
            {
                _rigidbody = playerController.GetComponent<Rigidbody>();
                _rigidbody.MovePosition(GameObject.FindWithTag("Blue Save Point 2").transform.position);
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
                _laser.enabled = false;
            }
            ;
        };
    }

    [ClientRpc(RequireOwnership = false)]
    private void RespawnClientRpc()
    {
        if (IsServer) return;
        _rigidbody = PlayerController.LocalPlayer.GetComponent<Rigidbody>();
        _rigidbody.MovePosition(GameObject.FindWithTag("Red Save Point 2").transform.position);
    }
}