using Photon.Pun;
using System.Collections;
using TMPro;
using UnityEngine;

public class GameStart : MonoBehaviour
{
    public GameObject[] playerPrefabsPink;
    public GameObject[] playerPrefabsBlue;
    public GameObject ballPrefab;

    public Transform[] PinkSpawnPoints;
    public Transform[] BlueSpawnPoints;
    public Transform[] FinalSpawnPoints;
    public Transform ballPosition;

    public TMP_Text team1ScoreText;
    public TMP_Text team2ScoreText;
    public TMP_Text timerText;
    public TMP_Text mainText;
    public TMP_Text pingText;

    private float gameTime = 15.0f;
    private int team1Score = 0;
    private int team2Score = 0;
    private float gameTimer;
    private float kickoffTimer = 4f;

    public bool isMainTime = false;
    public bool isKickoff = false;
    public bool isExtraTime = false;

    GameObject playerCar;
    Rigidbody playerPhysics;
    CarControl carControlScript;

    GameObject ball;
    Rigidbody ballPhysics;
    Ball ballScript;

    GameObject finalCamera;
    GameObject mainCamera;

    GameObject playerSpawnPrefab;
    int teamToSpawn;
    public int winnersTeam { get; private set; }
    bool playersNicknames = true;

    PhotonView photonView;
    GameObject UI;

    public AudioSource[] music;
    public AudioSource victory;
    private int currentTrack = -1;
    public TMP_Text trackLabel;

    void Start()
    {
        StartCoroutine(PlayRandomTrack());

        photonView = GetComponent<PhotonView>();
        UI = GameObject.FindGameObjectWithTag("UI");

        PlayerSpawn();
        if (PhotonNetwork.IsMasterClient)
        {
            BallSpawn();
        }

        finalCamera = GameObject.FindGameObjectWithTag("FinalCamera");
        finalCamera.SetActive(false);
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");

        FirstKickoff();
        gameTimer = gameTime;

        //Time.timeScale = 0.9F;

        ScoreUpdate();
    }

    void Update()
    {
        pingText.text = $"Ping: {PhotonNetwork.GetPing()}";
        VariablesUpdate();
        GameSwitchers();

        PlayerPositionCheck();
        BallPositionCheck();

        if (playersNicknames)
        {
            SetPlayersNicknames();
        }
    }

    public void VariablesUpdate()
    {
        if (photonView == null)
        {
            photonView = GetComponent<PhotonView>();
        }
        if (ball == null)
        {
            ball = GameObject.FindGameObjectWithTag("Ball");
        }
        else
        {
            if (ballScript == null)
            {
                ballScript = ball.GetComponent<Ball>();
            }

            if (ballPhysics == null)
            {
                ballPhysics = ball.GetComponent<Rigidbody>();
            }
        }

    }
    public void GameSwitchers()
    {
        if (isExtraTime && !isKickoff)
        {
            ExtraTimerUpdate();
        }
        else if (isMainTime)
        {
            TimerUpdate();
        }
        else if (isKickoff)
        {
            KickoffUpdate();
        }
    }
    private void SetPlayersNicknames()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Car");
        if (PhotonNetwork.CurrentRoom.PlayerCount == players.Length)
        {
            foreach (var item in players)
            {
                PhotonView playerView = item.GetComponent<PhotonView>();
                if (!playerView.IsMine)
                {
                    TMP_Text playerLabel = item.GetComponentInChildren<TMP_Text>();
                    if (playerLabel != null)
                    {
                        playerLabel.text = playerView.Owner.NickName;
                    }
                }
            }
            playersNicknames = false;
        }
    }

    // Score
    public void ScoreGoal(int team)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            SendInfoAbout(team);
        }

        if (isExtraTime)
        {
            _ = team switch
            {
                0 => team1Score++,
                1 => team2Score++,
            };
            ball.SetActive(false);

            EndGame();
        }
        else
        {
            if (ballScript.lastPlayerTouch != null)
            {
                mainText.text = team switch
                {
                    0 => $"{ballScript.lastPlayerTouch} SCORED!",
                    1 => $"{ballScript.lastPlayerTouch} SCORED!",
                };
            }
            else
            {
                mainText.text = team switch
                {
                    0 => $"PINK SCORED!",
                    1 => $"BLUE SCORED!",
                };
            }

            _ = team switch
            {
                0 => team1Score++,
                1 => team2Score++,
            };

            ball.SetActive(false);
            isMainTime = false;
            StartCoroutine(AfterGoalWait());
        }
        if (PhotonNetwork.IsMasterClient)
        {
            ScoreUpdate();
        }
    }
    private void ScoreUpdate()
    {
        team1ScoreText.text = team1Score.ToString();
        team2ScoreText.text = team2Score.ToString();
    }

    // TIMERS
    private void TimerUpdate()
    {
        gameTimer -= Time.deltaTime;

        int minutes = (int)(gameTimer / 60.0);
        int seconds = (int)(gameTimer % 60.0);
        string formattedTime = string.Format("{0:D2}:{1:D2}", minutes, seconds);

        timerText.text = formattedTime;

        if (gameTimer <= 1)
        {
            EndGame();
        }

    }
    private void ExtraTimerUpdate()
    {
        gameTimer += Time.deltaTime;

        int minutes = (int)(gameTimer / 60.0);
        int seconds = (int)(gameTimer % 60.0);
        string formattedTime = string.Format("+{0:D2}:{1:D2}", minutes, seconds);

        timerText.text = formattedTime;
    }

    // Player
    private void PlayerSpawn()
    {
        teamToSpawn = (int)PhotonNetwork.LocalPlayer.CustomProperties["playerTeam"];

        playerSpawnPrefab = teamToSpawn switch
        {
            0 => playerPrefabsPink[(int)PhotonNetwork.LocalPlayer.CustomProperties["playerAvatar"]],
            1 => playerPrefabsBlue[(int)PhotonNetwork.LocalPlayer.CustomProperties["playerAvatar"]],
        };

        Transform playerPos = PositionGenByTeam(teamToSpawn);

        PhotonNetwork.Instantiate(playerSpawnPrefab.name, playerPos.position, playerPos.rotation);

        playerCar = GameObject.FindGameObjectWithTag("Car");
        playerPhysics = GameObject.FindGameObjectWithTag("Car").GetComponent<Rigidbody>();
        carControlScript = GameObject.FindGameObjectWithTag("Car").GetComponent<CarControl>();
    }
    private void PlayerRespawn()
    {
        Transform playerPos;

        playerPos = teamToSpawn switch
        {
            0 => RandomPositionGen2(PinkSpawnPoints),
            1 => RandomPositionGen2(BlueSpawnPoints),
            _ => RandomPositionGen2(FinalSpawnPoints),
        };

        if (playerPhysics != null)
        {
            playerPhysics.velocity = Vector3.zero;
            playerPhysics.angularVelocity = Vector3.zero;
        }
        if (playerCar != null)
        {
            playerCar.transform.position = playerPos.position;
            playerCar.transform.rotation = playerPos.rotation;
        }
    }
    private void PlayerPositionCheck()
    {
        if (playerCar != null)
        {
            Vector3 playerCarPos = playerCar.transform.position;
            if (playerCarPos.x < -120 || playerCarPos.x > 120 || playerCarPos.y < -5 || playerCarPos.y > 100 || playerCarPos.z < -50 || playerCarPos.z > 180)
            {
                PlayerRespawn();
            }
        }
    }

    // Ball
    private void BallSpawn()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            ball = PhotonNetwork.Instantiate(ballPrefab.name, ballPosition.position, ballPosition.rotation);
        }

        if (ball != null)
        {
            ballScript = ball.GetComponent<Ball>();
            ballPhysics = ball.GetComponent<Rigidbody>();
        }
    }
    private void BallRespawn()
    {
        if(ball != null)
        {
            ball.transform.position = ballPosition.position;
            ball.transform.rotation = ballPosition.rotation;
        }
        if (ballPhysics != null)
        {
            ballPhysics.velocity = Vector3.zero;
            ballPhysics.angularVelocity = Vector3.zero;
        }
        ball.SetActive(true);
    }
    private void BallPositionCheck()
    {
        if(ball != null)
        {
            Vector3 ballPos = ball.transform.position;
            if (ballPos.x < -120 || ballPos.x > 120 || ballPos.y < -5 || ballPos.y > 100 || ballPos.z < -50 || ballPos.z > 180)
            {
                BallRespawn();
            }
        }
    }

    // Kickoff
    private void KickoffUpdate()
    {
        kickoffTimer -= Time.deltaTime;

        int seconds = (int)(kickoffTimer % 60.0);

        mainText.text = seconds.ToString();

        if (kickoffTimer <= 1)
        {
            if (!isExtraTime)
            {
                isMainTime = true;
            }
            isKickoff = false;
            carControlScript.KickoffFreeze = false;
            playerPhysics.maxLinearVelocity = 200;

            kickoffTimer = 5f;
            mainText.text = "GO!";
            StartCoroutine(MainTextWait());
        }
    }
    private void KickoffStart()
    {
        isKickoff = true;
        PlayerRespawn();
        carControlScript.ResetBoost(34);
        carControlScript.KickoffFreeze = true;
        playerPhysics.maxLinearVelocity = 0;
        BallRespawn();
    }
    private void FirstKickoff()
    {
        isKickoff = true;
        carControlScript.KickoffFreeze = true;
        playerPhysics.maxLinearVelocity = 0;
    }

    // PosGen
    private Transform PositionGenByTeam(int team)
    {
        Transform playerPos;

        playerPos = team switch
        {
            0 => RandomPositionGen2(PinkSpawnPoints),
            1 => RandomPositionGen2(BlueSpawnPoints),
            _ => RandomPositionGen2(FinalSpawnPoints),
        };

        return playerPos;
    }
    private Transform RandomPositionGen2(Transform[] spawnPoints)
    {
        int randomNumber;
        Collider[] colliders;

        do
        {
            randomNumber = UnityEngine.Random.Range(0, spawnPoints.Length);
            colliders = Physics.OverlapSphere(spawnPoints[randomNumber].position, 0.2f);

        } while (colliders.Length > 0);

        return spawnPoints[randomNumber];
    }

    // Events 
    private void EndGame()
    {
        if (isExtraTime)
        {
            mainText.text = (team1Score > team2Score) ? "PINK TEAM WIN!" : "BLUE TEAM WIN!";
            winnersTeam = (team1Score > team2Score) ? 0 : 1;

            isMainTime = false;
            isExtraTime = false;
            StartCoroutine(AfterWinWait());
        }
        else if (isMainTime)
        {
            if (team1Score != team2Score)
            {
                mainText.text = (team1Score > team2Score) ? "PINK TEAM WIN!" : "BLUE TEAM WIN!";
                winnersTeam = (team1Score > team2Score) ? 0 : 1;

                StartCoroutine(AfterWinWait());
            }
            else
            {
                mainText.text = "It's a tie!\nOvertime started.";

                isExtraTime = true;

                StartCoroutine(AfterGoalWait());
            }
            isMainTime = false;
        }
        ball.SetActive(false);
    }
    private void AfterWin()
    {
        music[currentTrack].Stop();
        victory.Play();
        UI.SetActive(false);

        finalCamera.SetActive(true);
        mainCamera.SetActive(false);
        mainText.text = "Winners!";

        if (teamToSpawn == winnersTeam)
        {
            Transform playerPos = PositionGenByTeam(2);

            if (playerPhysics != null)
            {
                playerPhysics.velocity = Vector3.zero;
                playerPhysics.angularVelocity = Vector3.zero;
            }

            playerCar.transform.position = playerPos.position;
            playerCar.transform.rotation = playerPos.rotation;

            carControlScript.UnlimitedBoost = true;
        }
        else
        {
            Destroy(playerCar);
        }
    }

    // music
    private IEnumerator PlayRandomTrack()
    {
        while (true)
        {
            int randomNumber = UnityEngine.Random.Range(0, music.Length);
            if(randomNumber != currentTrack)
            {
                music[randomNumber].Play();
                trackLabel.text = "Track: " + music[randomNumber].name;
                currentTrack = randomNumber;
                yield return new WaitForSeconds(music[randomNumber].clip.length);
                PlayRandomTrack();
            }
        }
    }

    // Waiters xdddd
    private IEnumerator AfterGoalWait()
    {
        yield return new WaitForSeconds(4f);
        KickoffStart();
    }
    private IEnumerator MainTextWait()
    {
        yield return new WaitForSeconds(2f);
        mainText.text = "";
    }
    private IEnumerator AfterWinWait()
    {
        yield return new WaitForSeconds(3f);
        AfterWin();
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(ReturnToTheRoom());
        }
    }
    private IEnumerator ReturnToTheRoom()
    {
        yield return new WaitForSeconds(9f);
        PhotonNetwork.LoadLevel("Lobby 1");
    }

    // RPC
    private void SendInfoAbout(int team)
    {
        photonView.RPC("GetInfoAbout", RpcTarget.Others, team);
    }
    [PunRPC]
    private void GetInfoAbout(int info)
    {
        ScoreGoal(info);
        ScoreUpdate();
    }
}