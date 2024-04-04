using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Game
{
    internal class GateTrigger : MonoBehaviour
    {
        GameStart gameStartScript;
        public AudioSource ballPop;
        private void Start()
        {
            gameStartScript = GameObject.FindGameObjectWithTag("GameStart").GetComponent<GameStart>();
        }

        void Update()
        {
            if(gameStartScript == null)
            {
                gameStartScript = GameObject.FindGameObjectWithTag("GameStart").GetComponent<GameStart>();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if(PhotonNetwork.IsMasterClient)
            {
                if (other.CompareTag("Ball"))
                {
                    float ballZ = other.transform.position.z;

                    if (ballZ < -25f)
                    {
                        gameStartScript.ScoreGoal(1);
                    }
                    else if (ballZ > 150f)
                    {
                        gameStartScript.ScoreGoal(0);
                    }
                    ballPop.Play();
                }
            }
        }
    }
}