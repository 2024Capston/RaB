using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 색깔 변환기를 조작하는 Class
/// </summary>
public class ColorChangerUtil : NetworkBehaviour
{
    private CubeController _cubeController;
    private Image _timerImage;

    private float _changeCooldown = 0f;
    private float _timer;
    private bool _timerStarted = false;

    private void Update()
    {
        if (!_timerStarted)
        {
            return;
        }

        transform.LookAt(Camera.main.transform.position);

        // 일정 시간이 지나면 색깔을 되돌린다.
        _timer += Time.deltaTime;
        _timerImage.fillAmount = _timer / _changeCooldown;

        if (_timer > _changeCooldown)
        {
            if (IsServer)
            {
                ColorType newColor;

                if (_cubeController.CubeColor == ColorType.Blue)
                {
                    newColor = ColorType.Red;
                }
                else
                {
                    newColor = ColorType.Blue;
                }

                _cubeController.ChangeColor(newColor, true);
                GetComponent<NetworkObject>().Despawn();
            }
        }
    }

    [ServerRpc]
    private void InitializeServerRpc(float changeTime)
    {
        InitializeClientRpc(changeTime);
    }

    [ClientRpc]
    private void InitializeClientRpc(float changeTime)
    {
        _changeCooldown = changeTime;
        _timer = 0f;
        _timerStarted = false;

        if (IsServer)
        {
            _cubeController = transform.parent.gameObject.GetComponent<CubeController>();

            ColorType newColor;

            if (_cubeController.CubeColor == ColorType.Blue)
            {
                newColor = ColorType.Red;
            }
            else
            {
                newColor = ColorType.Blue;
            }

            _cubeController.ChangeColor(newColor, false);
        }
    }

    [ServerRpc]
    private void StartTimerServerRpc()
    {
        StartTimerClientRpc();
    }

    [ClientRpc]
    private void StartTimerClientRpc()
    {
        _timerStarted = true;
        _timerImage = GetComponentInChildren<Image>();
        _timerImage.enabled = true;
    }

    /// <summary>
    /// 색깔 변환 메커니즘을 시작한다.
    /// </summary>
    /// <param name="changeTime">색깔 변환 지속시간</param>
    public void Initialize(float changeTime)
    {
        InitializeServerRpc(changeTime);
    }

    public void StartTimer()
    {
        StartTimerServerRpc();
    }
}
