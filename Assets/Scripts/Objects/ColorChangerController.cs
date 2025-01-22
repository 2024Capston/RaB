using UnityEngine;
using Unity.Netcode;
using System.Collections;
using Unity.VisualScripting;

public class ColorChangerController : NetworkBehaviour
{
    [SerializeField] private float _changeTime;

    [SerializeField] private GameObject _colorChangerUtilPrefab;

    private CubeController _cubeOnChanger;
    private NetworkInterpolator _cubeInterpolator;
    private ColorChangerUtil _utilObject;

    void Update()
    {
        if (!IsServer) {
            return;
        }

        if (_cubeOnChanger && _cubeOnChanger.IsTaken) {
            RevertChangingColorClientRpc();
        }
    }

    private void OnTriggerStay(Collider other) {
        if (!IsServer) {
            return;
        }
        if (other.gameObject.TryGetComponent<CubeController>(out CubeController cubeController) &&
            cubeController.GetComponent<NetworkInterpolator>().VisualReference.GetComponentInChildren<ColorChangerUtil>() == null) {
            if (!_cubeOnChanger) {
                StartChangingColorClientRpc(cubeController.gameObject);

                ColorType newColor = 3 - cubeController.CubeColor;
                cubeController.ChangeColor(newColor);
            }
        }
    }

    [ClientRpc]
    private void StartChangingColorClientRpc(NetworkObjectReference cube) {
        if (cube.TryGet(out NetworkObject networkObject)) {
            _cubeOnChanger = networkObject.GetComponent<CubeController>();
            _cubeInterpolator = networkObject.GetComponent<NetworkInterpolator>();
            _cubeOnChanger.ForceStopInteraction();

            _utilObject = Instantiate(_colorChangerUtilPrefab).GetComponent<ColorChangerUtil>();

            _utilObject.transform.SetParent(_cubeInterpolator.VisualReference.transform);
            _utilObject.transform.localPosition = Vector3.zero;
            _utilObject.transform.localRotation = Quaternion.identity;
            _utilObject.transform.localScale = new Vector3(1f / _cubeOnChanger.transform.localScale.x, 1f / _cubeOnChanger.transform.localScale.y, 1f / _cubeOnChanger.transform.localScale.z);

            _utilObject.Initialize(_cubeOnChanger, _changeTime);
        }
    }

    [ClientRpc]
    private void RevertChangingColorClientRpc() {
        _utilObject.StartTimer();

        _utilObject = null;
        _cubeOnChanger = null;
    }
}
