using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallPad : MonoBehaviour
{
    public GameObject sphere;
    public Color activeColor;
    public Color inactiveColor;

    private Renderer renderer;

    private CarControl carControlScript;

    private void Start()
    {
        //carControlScript = GameObject.FindGameObjectWithTag("Car").GetComponent<CarControlNew>();

        renderer = sphere.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = activeColor;
        }
    }

    private void Update()
    {
        if(GameObject.FindGameObjectWithTag("Car") != null)
        {
            carControlScript = GameObject.FindGameObjectWithTag("Car").GetComponent<CarControl>();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("CarBody") && carControlScript != null && renderer.material.color == activeColor)
        {
            if (carControlScript != null && other.attachedRigidbody == carControlScript.Rb)
            {
                carControlScript.AddBoost(12);
            }
            StartCoroutine(RespawnSmallPad());
        }
    }

    IEnumerator RespawnSmallPad()
    {
        if (renderer != null)
        {
            renderer.material.color = inactiveColor;
        }

        yield return new WaitForSeconds(5f);

        if (renderer != null)
        {
            renderer.material.color = activeColor;
        }
    }
}