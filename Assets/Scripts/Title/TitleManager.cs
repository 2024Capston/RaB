using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class TitleManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _title;

    [SerializeField]
    private TMP_Text _progressText;

    private AsyncOperation _asyncOperation;

    private void Awake()
    {
        _title.SetActive(true);
    }

    private void Start()
    {
        // TODO : User의 Data를 불러오는 작업이 필요하다.

        StartCoroutine(CoLoadHome());
    }

    /// <summary>
    /// Home Scene을 Load하기 위한 Coroutine
    /// </summary>
    /// <returns></returns>
    private IEnumerator CoLoadHome()
    {
        Logger.Log($"{GetType()}::CoLoadHome");

        _progressText.text = "0%";

        // NetworkManager Instance가 생성될 때까지 대기한다.
        yield return new WaitUntil(() => NetworkManager.Singleton != null);

        _progressText.text = "10%";

        // Home Scene을 비동기적으로 불러오기 위해 시도한다.
        _asyncOperation = SceneLoader.Instance.LoadSceneAsync(SceneType.Home);

        if (_asyncOperation == null)
        {
            Logger.LogError("Lobby async loading error");
            yield break;
        }

        // asyncOperation이 완료되었을 때 progress를 0.9에서 정지시킨다.
        _asyncOperation.allowSceneActivation = false;

        // asyncOperation이 완료될 때까지 반복한다.
        while (!_asyncOperation.isDone)
        {
            _progressText.text = $"{(int)((_asyncOperation.progress < 0.5f ? 0.5f : _asyncOperation.progress) * 100)}%";

            // asyncOperation이 완료되었다면 
            if (_asyncOperation.progress >= 0.9f)
            {

                // 약간 대기 한 뒤 Home Scene으로 넘어간다.
                _progressText.text = "100%";
                yield return new WaitForSecondsRealtime(0.5f);

                _asyncOperation.allowSceneActivation = true;
                yield break;
            }

            yield return null;
        }
    }
}