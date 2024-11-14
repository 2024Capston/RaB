using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LobbyUIController : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _playerColorData;

    public void SetPlayerColorData(bool IsHost)
    {
        if (IsHost)
        {
            _playerColorData.text = "Your Color: <color=\"blue\">Blue</color>";
        }
        else
        {
            _playerColorData.text = "Your Color: <color=\"red\">Red</color>";
        }
    }
}
