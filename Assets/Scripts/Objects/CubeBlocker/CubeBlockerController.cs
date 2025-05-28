using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class CubeBlockerController : NetworkBehaviour
{
    private ColorType _color;

    void Start()
    {
        
    }

    [ClientRpc(RequireOwnership = false)]
    private void InitializeClientRpc(ColorType color)
    {
        _color = color;
    }

    public void Initialize(ColorType color, EventType[] subscribeForBlock)
    {
        InitializeClientRpc(color);

        foreach (EventType eventType in subscribeForBlock)
        {
            EventBus.Instance.SubscribeEvent<UnityAction>(eventType, BlockCube);
        }
    }

    [ClientRpc(RequireOwnership = false)]
    private void BlockCubeClientRpc()
    {
        if (PlayerController.LocalPlayer.Color == _color)
        {
            GetComponent<Collider>().enabled = true;
        }
    }

    public void BlockCube()
    {
        BlockCubeClientRpc();
    }
}
