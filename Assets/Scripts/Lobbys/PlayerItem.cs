using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UIElements;

public class PlayerItem : MonoBehaviourPunCallbacks
{
    public TMP_Text playerName;

    private UnityEngine.UI.Image backgroundImage;
    public Color highlightColor;

    public Player player;

    private void Awake()
    {
        backgroundImage = GetComponent<UnityEngine.UI.Image>();
    }

    public void SetPlayerInfo(Player _player)
    {
        playerName.text = _player.NickName;
        player = _player;
    }

    public void LocalChanges()
    {
        backgroundImage.color = highlightColor;
    }
}
