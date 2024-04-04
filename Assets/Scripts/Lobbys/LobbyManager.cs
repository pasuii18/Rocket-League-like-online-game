using System.Collections;

using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using static GameStart;
using System;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public TMP_InputField roomInputField;
    public GameObject lobbyPanel;
    public GameObject roomPanel;
    public TMP_Text roomName;

    List<RoomItem> roomItemsList = new List<RoomItem>();
    public RoomItem roomItemPrefab;
    public Transform contentObject;

    public float timeBetweenUpdates = 2f;
    float nextUpdateTime;
    float playerListUpdateTime;

    List<PlayerItem> PlayerItemsList = new List<PlayerItem>();
    Dictionary<string, int> playersTeams = new Dictionary<string, int>();
    public PlayerItem PlayerItemPrefab;

    public Transform PlayerItemsNoTeam;
    public Transform PlayerItemsPinkTeam;
    public Transform PlayerItemsBlueTeam;

    public TMP_Text gameButtonText;
    public GameObject gameButton;

    PhotonView photonView;

    private int playerTeam = -1;

    public ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable();

    [System.Serializable]
    public struct PlayerTeamInfo
    {
        public string playerNickname;
        public int playerTeam;
    }     

    public AudioSource buttonClick;
    public AudioSource keyClick;
    public AudioSource playerJoined;
    public AudioSource gameClick;
    public AudioSource music;

    private void Start()
    {
        if(PhotonNetwork.CurrentRoom != null)
        {
            OnJoinedRoom();
        }
        else
        {
            PhotonNetwork.JoinLobby();
            roomInputField.characterLimit = 10;
            roomInputField.onValueChanged.AddListener(OnInputValueChanged);
        }
    }

    private void OnInputValueChanged(string arg0)
    {
        keyClick.Play();
    }

    private void Update()
    {
        if (photonView == null)
        {
            photonView = GetComponent<PhotonView>();
        }

        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount >= 1 && PlayerItemsNoTeam.childCount == 0)
        {
            gameButton.SetActive(true);
        }
        else
        {
            gameButton.SetActive(false); 
        }

        if (Time.time >= playerListUpdateTime && roomPanel.active)
        {
            UpdatePlayerList();
            playerListUpdateTime = Time.time + 0.5f;
            SetPlayerCustomProperties();
        }
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    //lobby
    public void OnClickCreate()
    {
        buttonClick.Play();
        if (roomInputField.text.Length >= 1 && roomInputField.text.Length <= 10)
        {
            PhotonNetwork.CreateRoom(roomInputField.text, new RoomOptions() { MaxPlayers = 6, BroadcastPropsChangeToAll = true });
        }
    }
    public void OnClickLeaveLobbyToMenu()
    {
        buttonClick.Play();
        PhotonNetwork.LeaveLobby();
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene("ConnectTo");
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        if (Time.time >= nextUpdateTime)
        {
            UpdateRoomList(roomList);
            nextUpdateTime = Time.time + timeBetweenUpdates;
        }
    }
    void UpdateRoomList(List<RoomInfo> list)
    {
        foreach (RoomItem item in roomItemsList)
        {
            Destroy(item.gameObject);
        }
        roomItemsList.Clear();

        foreach (RoomInfo room in list)
        {
            RoomItem newRoom = Instantiate(roomItemPrefab, contentObject);
            newRoom.SetRoomName(room.Name);
            roomItemsList.Add(newRoom);
        }
    }

    //join room
    public override void OnJoinedRoom()
    {
        lobbyPanel.SetActive(false);
        roomPanel.SetActive(true);
        roomName.text = "Room Name: " + PhotonNetwork.CurrentRoom.Name;
        UpdatePlayerList();
    }
    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        playerJoined.Play();
        SendPlayerTeamInfo();
        UpdatePlayerList();
    }

    // left room
    public override void OnLeftRoom()
    {
        roomPanel.SetActive(false);
        lobbyPanel.SetActive(true);
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayerList();
    }
    public void OnClickLeaveRoom()
    {
        buttonClick.Play();
        PhotonNetwork.LeaveRoom();
    }

    // Change Teams Updates
    public void UpdatePlayerList()
    {
        if (PhotonNetwork.CurrentRoom == null)
        {
            return;
        }

        foreach (PlayerItem item in PlayerItemsList)
        {
            Destroy(item.gameObject);
        }
        PlayerItemsList.Clear();

        foreach (KeyValuePair<int, Player> playerPair in PhotonNetwork.CurrentRoom.Players)
        {
            PlayerItem newPlayerItem;
            string playerValue = playerPair.Value.NickName;


            if(playersTeams.ContainsKey(playerValue))
            {
                int playerTeam = (int)playersTeams[playerValue];

                newPlayerItem = playerTeam switch
                {
                    -1 => Instantiate(PlayerItemPrefab, PlayerItemsNoTeam),
                    0 => Instantiate(PlayerItemPrefab, PlayerItemsPinkTeam),
                    1 => Instantiate(PlayerItemPrefab, PlayerItemsBlueTeam),
                    _ => Instantiate(PlayerItemPrefab, PlayerItemsNoTeam),
                };
            }
            else
            {
                newPlayerItem = Instantiate(PlayerItemPrefab, PlayerItemsNoTeam);
            }
            newPlayerItem.SetPlayerInfo(playerPair.Value);

            if (playerPair.Value == PhotonNetwork.LocalPlayer)
            {
                newPlayerItem.LocalChanges();
            }

            PlayerItemsList.Add(newPlayerItem);
        }
    }
    public void ChangeTeamButtonClicked()
    {
        buttonClick.Play();
        playerTeam = playerTeam switch
        {
            -1 => 0,
            0 => 1,
            1 => -1,
        };

        SendPlayerTeamInfo();
    }

 
    public void OnClickGameButton()
    {
        gameClick.Play();
        gameButtonText.text = "DUMAEM...";
        PhotonNetwork.LoadLevel("Game");
    }

    // PROPERTIES
    private void SetPlayerCustomProperties()
    {
        playerProperties["playerTeam"] = playerTeam;
        PhotonNetwork.SetPlayerCustomProperties(playerProperties);
    }

    //RPC
    private void SendPlayerTeamInfo()
    {
        PlayerTeamInfo playerTeamInfo;

        playerTeamInfo.playerNickname = PhotonNetwork.LocalPlayer.NickName;
        playerTeamInfo.playerTeam = playerTeam;

        var playerTeamInfoJson = JsonUtility.ToJson(playerTeamInfo);
        photonView.RPC("GetPlayerTeamInfo", RpcTarget.All, playerTeamInfoJson);
    }
    [PunRPC]
    void GetPlayerTeamInfo(string infoJson)
    {
        PlayerTeamInfo info = JsonUtility.FromJson<PlayerTeamInfo>(infoJson);

        if (playersTeams.ContainsKey(info.playerNickname))
        {
            playersTeams[info.playerNickname] = info.playerTeam;
        }
        else
        {
            playersTeams.Add(info.playerNickname, info.playerTeam);
        }
    }
}