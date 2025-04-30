using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class InGameUIController : MonoBehaviour
{
    private void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
    }
}
