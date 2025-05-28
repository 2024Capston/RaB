using UnityEngine;

public class RaycastBlockerController : MonoBehaviour
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
        if (_color == PlayerController.LocalPlayer.Color || _color == ColorType.Purple)
        {
            GetComponent<Collider>().enabled = false;
        }
    }
}
