using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TubeStage
{
    public class TextRenderMonitorController : NetworkBehaviour
    {
        [SerializeField] private MeshRenderer _screenMeshRenderer;
        [SerializeField] private TMP_Text _screenText;
        [SerializeField] private MonitorType _monitorType;
        [SerializeField] private Camera _renderCamera;
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            _screenMeshRenderer.material.SetInt("_MonitorType", (int)_monitorType);
            
            _screenMeshRenderer.material.SetPlayerColor(IsHost ? ColorType.Blue : ColorType.Red);
            _screenMeshRenderer.material.SetViewType(1);
        }

        private void Start()
        {
            RenderTexture screenTexture = new RenderTexture(1512, 794, 24);
            screenTexture.Create();
            
            _renderCamera.targetTexture = screenTexture;
            _screenMeshRenderer.material.SetTexture("_TextRenderTexture", screenTexture);
        }

        [ClientRpc]
        private void UpdateMonitorTypeClientRpc(MonitorType newType)
        {
            _monitorType = newType;
            _screenMeshRenderer.material.SetFloat("_MonitorType", (int)newType);
        }

        
        public void UpdateMonitorType(MonitorType newType)
        {
            UpdateMonitorTypeClientRpc(newType);
        }

        [ClientRpc]
        private void UpdateMonitorTextClientRpc(string newText)
        {
            _screenText.text = newText;
        }

        [ServerRpc(RequireOwnership = false)]
        public void UpdateMonitorTextServerRpc(string newText)
        {
            UpdateMonitorTextClientRpc(newText);
        }

        public void UpdateMonitorText(string newText)
        {
            _screenText.text = newText;
        }

        [ClientRpc]
        private void UpdateMonitorColorClientRpc(ColorType colorType)
        {
            _screenMeshRenderer.material.SetObjectColor(colorType);
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void UpdateMonitorColorServerRpc(ColorType colorType)
        {
            UpdateMonitorColorClientRpc(colorType);
        }
    }
}


