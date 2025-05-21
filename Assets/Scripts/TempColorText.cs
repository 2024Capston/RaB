using UnityEngine;

public class TempColorText : MonoBehaviour
{
    [SerializeField] private ColorType _color;

    void Start()
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

    void UpdateColor()
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

        meshRenderer.material.SetObjectColor(_color);
        meshRenderer.material.SetPlayerColor(PlayerController.LocalPlayer.Color);
    }
}
