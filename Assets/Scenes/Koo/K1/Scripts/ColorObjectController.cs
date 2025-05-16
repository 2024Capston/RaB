using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorObjectController : MonoBehaviour
{
    [SerializeField]
    private ColorType _color;
    
    // Start is called before the first frame update
    void Start()
    {
        if (PlayerController.LocalPlayer == null)
        {
            PlayerController.LocalPlayerCreated += () => {
                if (PlayerController.LocalPlayer.Color != _color)
                {
                    GetComponent<MeshCollider>().enabled = false;
                    GetComponent<MeshRenderer>().enabled = false;
                }
            };  
        }
        else
        {
            if (PlayerController.LocalPlayer.Color != _color)
            {
                GetComponent<MeshCollider>().enabled = false;
                GetComponent<MeshRenderer>().enabled = false;
            }
        }
        
    }
}
