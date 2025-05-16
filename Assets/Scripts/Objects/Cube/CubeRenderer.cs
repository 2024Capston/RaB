using UnityEngine;
using Unity.Netcode;
using System.Collections;
using Unity.VisualScripting;
using System.Runtime.CompilerServices;

/// <summary>
/// 큐브를 렌더링하는 Class
/// </summary>
public class CubeRenderer : NetworkBehaviour
{
    [SerializeField] private int _viewMode;
    [SerializeField] private AudioClip _colorChangeClips;

    private ColorType _playerColor;
    
    private CubeController _cubeController;
    private CubeAudioController _cubeAudioController;
    private NetworkInterpolator _networkInterpolator;
    private Outline _outline;

    private Transform[] _piecesTransforms;
    private MeshRenderer[] _piecesMeshRenderers;

    private Material[][] _cubeMaterials;

    [ServerRpc(RequireOwnership = false)]
    private void UpdateColorServerRpc()
    {
        UpdateColorClientRpc();
    }

    [ClientRpc(RequireOwnership = false)]
    private void UpdateColorClientRpc()
    {
        bool originalOutline = false;

        // Outline 매터리얼까지 갱신되는 것을 막기 위해 잠시 비활성화
        if (_outline) {
            originalOutline = _outline.enabled;
            _outline.enabled = false;
        }

        int childCount = _piecesTransforms.Length;

        for (int i = 0; i < childCount; i++)
        {
            _cubeMaterials[i][0].SetObjectColor(_cubeController.Color);
            _cubeMaterials[i][1].SetObjectColor(_cubeController.Color);

            _piecesMeshRenderers[i].materials = _cubeMaterials[i];
        }

        if (_outline) {
            _outline.enabled = originalOutline;
        }
    }

    public void Initialize()
    {
        _cubeController = GetComponent<CubeController>();
        _cubeAudioController = GetComponent<CubeAudioController>();
        _networkInterpolator = GetComponent<NetworkInterpolator>();

        _playerColor = NetworkManager.Singleton.IsHost ? ColorType.Blue : ColorType.Red;

        _networkInterpolator.AddVisualReferenceDependantFunction(() =>
        {
            _outline = _networkInterpolator.VisualReference.GetComponent<Outline>();

            int childCount = _networkInterpolator.VisualReference.transform.childCount;

            _piecesTransforms = new Transform[childCount];
            _piecesMeshRenderers = new MeshRenderer[childCount];
            _cubeMaterials = new Material[childCount][];

            for (int i = 0; i < childCount; i++)
            {
                Transform child = _networkInterpolator.VisualReference.transform.GetChild(i);

                _piecesTransforms[i] = child.transform;
                _piecesMeshRenderers[i] = child.GetComponent<MeshRenderer>();

                Material[] materials = _piecesMeshRenderers[i].materials;
                materials[0].SetMaterial(_cubeController.Color, _playerColor, _viewMode);
                materials[1].SetMaterial(_cubeController.Color, _playerColor, _viewMode);
                _piecesMeshRenderers[i].materials = materials;

                _cubeMaterials[i] = materials;
            }
        });
    }

    /// <summary>
    /// 큐브의 렌더링 색깔을 갱신한다.
    /// </summary>
    public void UpdateColor()
    {
        UpdateColorServerRpc();
    }

    public void PlayTransitionAnimation(float transitionTime = 2f)
    {
        StartCoroutine("CoPlayTransitionAnimation", transitionTime);
    }

    private IEnumerator CoPlayTransitionAnimation(float transitionTime)
    {
        // Outline 매터리얼까지 갱신되는 것을 막기 위해 비활성화
        if (_outline) {
            _outline.enabled = false;
        }
        
        int targetColor = 3 - (int)_cubeController.Color;

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                _cubeMaterials[i][j].SetInterpolationFactor(0f);
                _cubeMaterials[i][j].SetTargetColor(targetColor);
            }
            
            _piecesMeshRenderers[i].materials = _cubeMaterials[i];
        }

        float timer = 0f;
        float lastTime = Time.realtimeSinceStartup;

        // 1차 회전
        _cubeAudioController.PlayColorChangeSound();

        while (timer < transitionTime / 3f)
        {
            for (int i = 0; i < 4; i++)
            {
                _piecesTransforms[i].localRotation = Quaternion.Lerp(Quaternion.identity, Quaternion.Euler(0, 90, 0), timer * 3f / transitionTime);

                _cubeMaterials[i][0].SetInterpolationFactor(timer * 3f / transitionTime);
                _cubeMaterials[i][1].SetInterpolationFactor(timer * 3f / transitionTime);
                _piecesMeshRenderers[i].materials = _cubeMaterials[i];
            }

            yield return new WaitForSeconds(0.01f);

            timer += Time.realtimeSinceStartup - lastTime;
            lastTime = Time.realtimeSinceStartup;
        }

        for (int i = 0; i < 4; i++) {
            _piecesTransforms[i].localRotation = Quaternion.identity;
        }

        timer -= transitionTime / 3f;

        // 2차 회전
        _cubeAudioController.PlayColorChangeSound();

        while (timer < transitionTime / 3f)
        {
            for (int i = 1; i < 8; i += 2)
            {
                _piecesTransforms[i].localRotation = Quaternion.Lerp(Quaternion.identity, Quaternion.Euler(90, 0, 0), timer * 3f / transitionTime);

                if (i == 5 || i == 7)
                {
                    _cubeMaterials[i][0].SetInterpolationFactor(timer * 3f / transitionTime);
                    _cubeMaterials[i][1].SetInterpolationFactor(timer * 3f / transitionTime);
                    _piecesMeshRenderers[i].materials = _cubeMaterials[i];
                }
            }

            yield return new WaitForSeconds(0.01f);

            timer += Time.realtimeSinceStartup - lastTime;
            lastTime = Time.realtimeSinceStartup;
        }

        for (int i = 1; i < 8; i += 2) {
            _piecesTransforms[i].localRotation = Quaternion.identity;
        }

        timer -= transitionTime / 3f;

        // 3차 회전
        _cubeAudioController.PlayColorChangeSound();

        while (timer < transitionTime / 3f)
        {
            for (int i = 4; i < 8; i++)
            {
                _piecesTransforms[i].localRotation = Quaternion.Lerp(Quaternion.identity, Quaternion.Euler(0, -90, 0), timer * 3f / transitionTime);

                if (i == 4 || i == 6)
                {
                    _cubeMaterials[i][0].SetInterpolationFactor(timer * 3f / transitionTime);
                    _cubeMaterials[i][1].SetInterpolationFactor(timer * 3f / transitionTime);   
                    _piecesMeshRenderers[i].materials = _cubeMaterials[i];
                }
            }

            yield return new WaitForSeconds(0.01f);
            
            timer += Time.realtimeSinceStartup - lastTime;
            lastTime = Time.realtimeSinceStartup;
        }

        for (int i = 4; i < 8; i++) {
            _piecesTransforms[i].localRotation = Quaternion.identity;
        }
    }
}
