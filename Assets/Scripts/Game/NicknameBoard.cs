using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NicknameBoard : MonoBehaviour
{
    Transform mainCamera;
    Transform finalCamera;
    void Start()
    {
        mainCamera = Camera.main.transform;
    } 

    private void Update()
    {
        if (GameObject.FindGameObjectWithTag("FinalCamera") && finalCamera == null)
        {
            finalCamera = GameObject.FindGameObjectWithTag("FinalCamera").transform;
        }

        if (finalCamera == null)
        {
            transform.LookAt(transform.position + mainCamera.rotation * Vector3.forward, mainCamera.rotation * Vector3.up);
        }
        else if (finalCamera != null)
        {
            transform.LookAt(transform.position + finalCamera.rotation * Vector3.forward, finalCamera.rotation * Vector3.up);
        }
    }
}
