using Cinemachine;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewCam : MonoBehaviour
{
    private GameObject ball;
    private GameObject car;
    public CinemachineVirtualCamera cam;
    public bool CamOnBall = false;
    public bool CameraReadyToSwitch = true;

    private void Start()
    {
        cam = GetComponent<CinemachineVirtualCamera>();
    }

    private void Update()
    {
        if (car == null)
        {
            car = GameObject.FindGameObjectWithTag("Car");
            if (car != null)
            {
                cam.Follow = car.transform;
                cam.LookAt = car.transform;
            }
        }
        if (ball == null)
        {
            ball = GameObject.FindGameObjectWithTag("Ball");
        }

        if (CameraReadyToSwitch)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                if (CamOnBall)
                {
                    //SetBindingMode(CinemachineTransposer.BindingMode.LockToTargetWithWorldUp);
                    cam.LookAt = car.transform;
                    CamOnBall = false;
                }
                else
                {
                    //SetBindingMode(CinemachineTransposer.BindingMode.LockToTargetOnAssign);
                    cam.LookAt = ball.transform;
                    CamOnBall = true;
                }
                StartCoroutine(WaitForCamera());
            }
        }
    }

    void SetBindingMode(CinemachineTransposer.BindingMode bindingMode)
    {
        CinemachineTransposer transposer = cam.GetCinemachineComponent<CinemachineTransposer>();
        if (transposer != null)
        {
            transposer.m_BindingMode = bindingMode;
        }
        else
        {
            Debug.LogError("CinemachineTransposer not found.");
        }
    }

    IEnumerator WaitForCamera()
    {
        CameraReadyToSwitch = false;
        yield return new WaitForSeconds(0.5f);
        CameraReadyToSwitch = true;
    }
}
