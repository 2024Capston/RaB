using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ColorChangerController : NetworkBehaviour
{
    [SerializeField] private float _changeTime;

    [SerializeField] private GameObject ColorChangerUtilPrefab;

    private CubeController _cubeControllerOnChanger;
    private CubeRenderer _cubeRendererOnChanger;

    void Start()
    {
        
    }

    void Update()
    {
        if (_cubeControllerOnChanger && _cubeControllerOnChanger.IsOwner)
        {
            if (_cubeControllerOnChanger.IsTaken)
            {
                _cubeControllerOnChanger.GetComponentInChildren<ColorChangerUtil>().StartTimer();

                _cubeControllerOnChanger = null;
                _cubeRendererOnChanger = null;
            }
            else
            {
                _cubeControllerOnChanger.transform.position = transform.position;
                _cubeControllerOnChanger.transform.rotation = transform.rotation;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer)
        {
            return;
        }

        if (!_cubeControllerOnChanger && other.gameObject.TryGetComponent<CubeController>(out CubeController cubeController))
        {
            if (cubeController.IsTaken && cubeController.GetComponentInChildren<ColorChangerUtil>() == null)
            {
                Debug.Log("Entered");
                _cubeControllerOnChanger = cubeController;
                _cubeRendererOnChanger = other.gameObject.GetComponent<CubeRenderer>();

                _cubeControllerOnChanger.ForceStopInteraction();
                _cubeControllerOnChanger.SetActive(false);

                StartCoroutine("DoAnimation");

                GameObject colorChangerUtilObject = Instantiate(ColorChangerUtilPrefab);
                ColorChangerUtil colorChangerUtil = colorChangerUtilObject.GetComponent<ColorChangerUtil>();
                NetworkObject networkObject = colorChangerUtilObject.GetComponent<NetworkObject>();

                networkObject.Spawn();
                networkObject.TrySetParent(cubeController.gameObject);

                colorChangerUtilObject.transform.localPosition = Vector3.zero;
                colorChangerUtilObject.transform.localScale = Vector3.one;

                colorChangerUtil.Initialize(_changeTime);
            }
        }
    }

    private IEnumerator DoAnimation()
    {
        yield return new WaitForSeconds(3f);

        _cubeControllerOnChanger.SetActive(true);
        _cubeRendererOnChanger.UpdateColor();
    }
}
