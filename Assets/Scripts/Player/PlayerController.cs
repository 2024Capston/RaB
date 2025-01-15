using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PlayerController : NetworkBehaviour
{
    [SerializeField]
    private float _walkSpeed = 10.0f;

    [SerializeField]
    private float _rotateSpeed = 2f;

    [SerializeField] private PostProcessResources _postProcessResources;
    [SerializeField] private PostProcessProfile _playerPostProcessProfile;

    private GameObject _mainCamera;

    private float _pitchAngle;

    private Rigidbody _rigidbody;
    
    private ColorType _playerColor;
    public ColorType PlayerColor
    {
        get => _playerColor;
        set => _playerColor = value; 
    }
    
    [SerializeField]
    private IInteractable _interactableInHand = null;
    public IInteractable InteractableInHand
    {
        get => _interactableInHand;
        set => _interactableInHand = value;
    }


    public override void OnNetworkSpawn()
    {
        _rigidbody = GetComponent<Rigidbody>();
 
        if (IsOwner)
        {
            _mainCamera = new GameObject("Main Camera");
            _mainCamera.transform.parent = transform;
            _mainCamera.transform.localPosition = new Vector3(0, 20, 0);
            _mainCamera.AddComponent<Camera>().cullingMask ^= 1 << LayerMask.NameToLayer("UI");
            _mainCamera.AddComponent<AudioListener>();
            _mainCamera.tag = "MainCamera";
            
            // PostProcess Setting
            _mainCamera.layer = LayerMask.NameToLayer("Camera");
            PostProcessLayer postProcessLayer = _mainCamera.AddComponent<PostProcessLayer>();
            postProcessLayer.Init(_postProcessResources);
            postProcessLayer.volumeTrigger = _mainCamera.transform;
            postProcessLayer.volumeLayer = 1 << LayerMask.NameToLayer("Camera");
            
            PostProcessVolume postProcessVolume = _mainCamera.AddComponent<PostProcessVolume>();
            postProcessVolume.isGlobal = true;
            postProcessVolume.profile = _playerPostProcessProfile;
            
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void FixedUpdate()
    {
        if (!IsOwner)
        {
            return;
        }

        if (Cursor.lockState != CursorLockMode.Locked)
        {
            return;
        }

        CameraHandler();
        MoveHandler();
        
        IInteractable it = null;
        if (InteractableInHand == null)
        {
            it = FindInteractableObject();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (it != null)
            {
                Logger.Log($"hit by {it}");
                it.StartInteraction(this);
            }
        }
        
        //InputHandler();
    }

    private void CameraHandler()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = -Input.GetAxis("Mouse Y");

        _pitchAngle = Mathf.Clamp(_pitchAngle + mouseY * _rotateSpeed, -90, 90);

        transform.Rotate(new Vector3(0, mouseX * _rotateSpeed, 0));
        Vector3 cameraRot = _mainCamera.transform.rotation.eulerAngles;
        _mainCamera.transform.rotation = Quaternion.Euler(_pitchAngle, cameraRot.y, cameraRot.z);
    }

    private void MoveHandler()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 moveDir = (v * transform.forward + h * transform.right).normalized * _walkSpeed;
        _rigidbody.velocity = new Vector3(moveDir.x, _rigidbody.velocity.y, moveDir.z);
    }

    private IInteractable FindInteractableObject()
    {
        if (_interactableInHand != null)
        {
            return null;
        }
        
        Physics.Raycast(_mainCamera.transform.position, _mainCamera.transform.forward, out RaycastHit hit);
        
        if (hit.collider == null)
        {
            return null;
        }
        IInteractable interactable = hit.collider.gameObject.GetComponent<IInteractable>();

        if (interactable == null)
        {
            return null;
        }

        return interactable;
    }

    private void InputHandler()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            ConfirmUIData confirmUIData = new ConfirmUIData()
            {
                ConfirmType = ConfirmType.OK_Cancel,
                TitleText = "돌아가기",
                DescText = $"홈 화면으로 돌아가시겠습니까?",
                OKButtonText = "확인",
                CancelButtonText = "취소",
                OnClickOKButton = () =>
                {
                    RaB.Connection.ConnectionManager.Instance.RequestShutdown();
                },
                OnClickCancelButton = () =>
                {
                    Cursor.lockState = CursorLockMode.Locked;
                }
            };
        
            UIManager.Instance.OpenUI<ConfirmUI>(confirmUIData);
        }
    }
}
