using UnityEngine;

public class PassableColorWallController : MonoBehaviour
{
    [SerializeField] private ColorType _color;

    private void Start()
    {
        if (PlayerController.LocalPlayer)
        {
            UpdateColor();
        }
        else
        {
            PlayerController.LocalPlayerCreated += UpdateColor;
        }
    }

    private void UpdateColor()
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

        meshRenderer.material.SetObjectColor(_color);
        meshRenderer.material.SetPlayerColor(PlayerController.LocalPlayer.Color);

        if (_color == PlayerController.LocalPlayer.Color || _color == ColorType.Purple)
        {
            GetComponent<BoxCollider>().enabled = false;
        }
    }
}
