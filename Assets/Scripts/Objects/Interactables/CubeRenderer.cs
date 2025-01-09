using UnityEngine;

public class CubeRenderer : MonoBehaviour
{
    [SerializeField] Material[] _cubeMaterials;

    private CubeController _cubeController;

    void Start()
    {
        _cubeController = GetComponent<CubeController>();

        GetComponent<MeshRenderer>().material = _cubeMaterials[(int)_cubeController.CubeColor - 1];
    }
}
