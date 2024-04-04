using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    public TMP_InputField usernameInput;
    public TMP_Text buttonText;
    public AudioSource buttonClick;
    public AudioSource keyClick;

    private void Start()
    {
        usernameInput.characterLimit = 11;
        usernameInput.onValueChanged.AddListener(OnInputValueChanged);
    }

    public void OnClickConnect()
    {
        buttonClick.Play();
        if (usernameInput.text.Length >= 1 && usernameInput.text.Length <= 11)
        {
            PhotonNetwork.NickName = usernameInput.text;
            buttonText.text = "BUYING CARS...";

            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.ConnectUsingSettings();
        }
    }
    public override void OnConnectedToMaster()
    {
        SceneManager.LoadScene("Lobby 1");
    }

    public void OnClickExit()
    {
        buttonClick.Play();
        Application.Quit();
    }

    private void OnInputValueChanged(string arg0)
    {
        keyClick.Play();
    }
}
