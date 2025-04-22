using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace TubeStage
{
    public class TextRenderMonitorController : NetworkBehaviour
    {
        [SerializeField] private MeshRenderer _screenMeshRenderer;
        [SerializeField] private TMP_Text _screenText;
        [SerializeField] private Texture _screenTexture;
        [SerializeField] private MonitorType _monitorType;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _screenMeshRenderer.material.SetInt("_MonitorType", (int)_monitorType);
            _screenMeshRenderer.material.SetTexture("_ScreenTexture", _screenTexture);
        }
    
        [ClientRpc]
        private void UpdateMonitorTypeClientRpc(MonitorType newType)
        {
            _monitorType = newType;
            _screenMeshRenderer.material.SetFloat("_MonitorType", (int)newType);
        }

        [ServerRpc(RequireOwnership = false)]
        public void UpdateMonitorTypeServerRpc(MonitorType newType)
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
    }
}


