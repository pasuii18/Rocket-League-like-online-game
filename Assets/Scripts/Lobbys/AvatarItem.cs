using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UIElements;

public class AvatarItem : MonoBehaviourPunCallbacks
{
    public TMP_Text carName;

    public GameObject leftButton;
    public GameObject rightButton;

    public ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable();

    public UnityEngine.UI.Image playerAvatar;
    public Sprite[] avatars;

    Player player;

    public AudioSource buttonClick;

    private void Awake()
    {
        SetPlayerInfo(PhotonNetwork.LocalPlayer);
    }

    public void SetPlayerInfo(Player _player)
    {
        player = _player;
        UpdatePlayerItem(player);
        PhotonNetwork.SetPlayerCustomProperties(playerProperties);
        ChangeCarName((int)playerProperties["playerAvatar"]);
    }

    public void OnClickLeftArrow()
    {
        buttonClick.Play();
        if ((int)playerProperties["playerAvatar"] == 0)
        {
            playerProperties["playerAvatar"] = avatars.Length - 1;
        }
        else
        {
            playerProperties["playerAvatar"] = (int)playerProperties["playerAvatar"] - 1;
        }
        ChangeCarName((int)playerProperties["playerAvatar"]);
        PhotonNetwork.SetPlayerCustomProperties(playerProperties);
    }
    public void OnClickRightArrow()
    {
        buttonClick.Play();
        if ((int)playerProperties["playerAvatar"] == avatars.Length - 1)
        {
            playerProperties["playerAvatar"] = 0;
        }
        else
        {
            playerProperties["playerAvatar"] = (int)playerProperties["playerAvatar"] + 1;
        }
        ChangeCarName((int)playerProperties["playerAvatar"]);
        Debug.Log($"{player.NickName} - {playerProperties["playerAvatar"]}");
        PhotonNetwork.SetPlayerCustomProperties(playerProperties);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (player == targetPlayer)
        {
            UpdatePlayerItem(targetPlayer);
        }
    }

    public void UpdatePlayerItem(Player player)
    {
        if (player.CustomProperties.ContainsKey("playerAvatar"))
        {
            playerAvatar.sprite = avatars[(int)player.CustomProperties["playerAvatar"]];
            playerProperties["playerAvatar"] = (int)player.CustomProperties["playerAvatar"];
        }
        else
        {
            playerProperties["playerAvatar"] = 0;
        }
    }

    public void ChangeCarName(int num)
    {
        switch(num)
        {
            case 0:
                carName.text = "Octane";
                break;
            case 1:
                carName.text = "Fennec";
                break;
            case 2:
                carName.text = "Dominus";
                break;
            default:
                break;
        }
    }
}
