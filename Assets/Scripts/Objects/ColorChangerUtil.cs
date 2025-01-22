using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 색깔 변환기를 조작하는 Class
/// </summary>
public class ColorChangerUtil : MonoBehaviour
{
    private CubeController _cubeController;
    private Image _timerImage;

    private bool _timerStarted = false;
    private float _changeCooldown = 0f;
    private float _timer;

    private void Update()
    {
        transform.LookAt(Camera.main.transform.position);

        if (!_timerStarted) {
            return;
        }

        // 일정 시간이 지나면 색깔을 되돌린다.
        _timer += Time.deltaTime;
        _timerImage.fillAmount = _timer / _changeCooldown;

        if (_timer > _changeCooldown)
        {
            if (NetworkManager.Singleton.IsServer) {
                ColorType newColor;

                if (_cubeController.CubeColor == ColorType.Blue)
                {
                    newColor = ColorType.Red;
                }
                else
                {
                    newColor = ColorType.Blue;
                }

                _cubeController.ChangeColor(newColor);
            }
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 색깔 변환 메커니즘을 시작한다.
    /// </summary>
    /// <param name="changeTime">색깔 변환 지속시간</param>
    public void Initialize(CubeController cubeController, float changeTime)
    {
        _changeCooldown = changeTime;
        _timerStarted = false;
        _timer = 0f;

        _cubeController = cubeController;
        _timerImage = GetComponentInChildren<Image>();
    }

    public void StartTimer() {
        _timerStarted = true;
    }
}