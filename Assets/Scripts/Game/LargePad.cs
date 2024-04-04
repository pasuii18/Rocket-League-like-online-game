using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Game
{
    internal class LargePad : MonoBehaviour
    {
        public GameObject smallSphere1;
        public GameObject smallSphere2;
        public GameObject smallSphere3;
        public GameObject bigSphere;

        public Color activeColor;
        public Color inactiveColor;

        private Renderer renderer1;
        private Renderer renderer2;
        private Renderer renderer3;
        private Renderer renderer4;

        private CarControl carControlScript;

        private void Start()
        {
            renderer1 = smallSphere1.GetComponent<Renderer>();
            renderer2 = smallSphere2.GetComponent<Renderer>();
            renderer3 = smallSphere3.GetComponent<Renderer>();
            renderer4 = bigSphere.GetComponent<Renderer>();

            if (renderer1 != null && renderer2 != null && renderer3 != null && renderer4 != null)
            {
                renderer1.material.color = activeColor;
                renderer2.material.color = activeColor;
                renderer3.material.color = activeColor;
                renderer4.material.color = activeColor;
            }
        }

        private void Update()
        {
            if (GameObject.FindGameObjectWithTag("Car") != null)
            {
                carControlScript = GameObject.FindGameObjectWithTag("Car").GetComponent<CarControl>();
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("CarBody") && carControlScript != null && renderer1.material.color == activeColor)
            {
                if (other.attachedRigidbody == carControlScript.Rb)
                {
                    carControlScript.AddBoost(100);
                }
                StartCoroutine(RespawnLargePad());
            }
        }

        IEnumerator RespawnLargePad()
        {
            if (renderer1 != null && renderer2 != null && renderer3 != null && renderer4 != null)
            {
                renderer1.material.color = inactiveColor;
                renderer2.material.color = inactiveColor;
                renderer3.material.color = inactiveColor;
                renderer4.material.color = inactiveColor;
                bigSphere.SetActive(false);
            }

            yield return new WaitForSeconds(10f);

            if (renderer1 != null && renderer2 != null && renderer3 != null && renderer4 != null)
            {
                renderer1.material.color = activeColor;
                renderer2.material.color = activeColor;
                renderer3.material.color = activeColor;
                renderer4.material.color = activeColor;
                bigSphere.SetActive(true);
            }
        }
    }
}
