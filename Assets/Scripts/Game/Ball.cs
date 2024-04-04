using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviourPunCallbacks
{
    private CarControl carControlScript;

    Rigidbody ballPhysics;

    public string lastPlayerTouch { get; private set; }
    PhotonView photonView;

    void Start()
    {
        photonView = GetComponent<PhotonView>();

        ballPhysics = GetComponent<Rigidbody>();
        ballPhysics.maxAngularVelocity = 100;
        ballPhysics.maxLinearVelocity = 100;
    }

    void Update()
    {
        if (photonView == null)
        {
            Debug.Log("photonView NULL ball");
            photonView = GetComponent<PhotonView>();
        }
        if (carControlScript == null)
        {
            if(GameObject.FindGameObjectWithTag("Car") != null)
            {
                carControlScript = GameObject.FindGameObjectWithTag("Car").GetComponent<CarControl>();
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("CarBody") && other.attachedRigidbody == carControlScript.Rb)
        {
            lastPlayerTouch = PhotonNetwork.LocalPlayer.NickName;
            photonView.RPC("GetLastPlayerTouch", RpcTarget.OthersBuffered, lastPlayerTouch);
        }
    }

    [PunRPC]
    void GetLastPlayerTouch(string info)
    {
        lastPlayerTouch = info;
    }

}
