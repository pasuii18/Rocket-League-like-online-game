using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.Timeline;

public class CarControl : MonoBehaviour
{
    public Rigidbody Rb { get; private set; }
    private float Speed = 8000;
    private float MaxLinearVelocity = 25;
    private float MaxAngularVelocity = 2;
    private float TurnSpeed = 4000;
    private float JumpForce = 250000;
    private float DoubleJumpForce = 200000;
    private float RollRotationSpeed = 200;
    private float YawpitchRotationSpeed = 200;
    private float BoostForce = 17000;

    public float BoostCapacity = 34;
    public bool UnlimitedBoost = false;

    private float JumpCheckDistance = 0.6f;
    private float ControlSwitchDistance = 0.6f;
    private float RoofCheckDistance = 1;

    public bool isJumpGrounded = false;
    public bool isControlGrounded = false;
    public bool IsRoofGrounded = false;
    public bool DoubleJump = false;
    public bool KickoffFreeze = false;

    private TMP_Text boostIndicatorText;
    private Image boostIndicatorImage;

    public List<AxleInfo> axleInfos;

    PhotonView view;

    GameObject playerNicknameLabel;
    public TMP_Text NicknameLabel;

    public AudioSource boostFillSound;
    public AudioSource jumpSound;

    private void Awake()
    {
        view = GetComponent<PhotonView>();
        if (view.IsMine)
        {
            Rb = GetComponent<Rigidbody>();

            Rb.maxLinearVelocity = MaxLinearVelocity;
            Rb.maxAngularVelocity = MaxAngularVelocity;

            GameObject boostText = GameObject.FindGameObjectWithTag("BoostText");
            GameObject boostImage = GameObject.FindGameObjectWithTag("BoostImage");
            boostIndicatorText = boostText.GetComponent<TMP_Text>();
            boostIndicatorImage = boostImage.GetComponent<Image>();

            boostIndicatorImage.fillAmount = BoostCapacity / 100;
            boostIndicatorText.text = Mathf.Round(BoostCapacity).ToString();

            HideNicknameLabel();
        }
    }

    public void FixedUpdate()
    {
        if (view.IsMine && !KickoffFreeze)
        {
            foreach (AxleInfo axleInfo in axleInfos)
            {
                axleInfo.ApplyLocalPositionToVisuals(axleInfo.leftWheel);
                axleInfo.ApplyLocalPositionToVisuals(axleInfo.rightWheel);
            }

            BoostingControl();
            CarControlSwitcher();
        }
    }

    private void Update()
    {
        if (view.IsMine && !KickoffFreeze)
        {
            Jump();
        }
    }

    private void HideNicknameLabel()
    {
        playerNicknameLabel = GameObject.FindGameObjectWithTag("NicknameLabel");
        playerNicknameLabel.SetActive(false);   
    }

    private void CarControlSwitcher()
    {
        isControlGrounded = IsControlGrounded();
        IsRoofGrounded = IsCarOnRoofGrounded();

        if (isControlGrounded)
        {
            GroundCarControl();
        }
        else if (IsRoofGrounded)
        {
            RoofCarControl();
        }
        else
        {
            AerialCarControl();
        }
    }
    // control
    private void GroundCarControl()
    {
        Vector3 localVelocity = transform.InverseTransformDirection(Rb.velocity);
        localVelocity.x = 0;

        // Casual control
        if (Input.GetKey(KeyCode.W))
        {
            Rb.AddRelativeForce(Vector3.forward * Speed);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            Rb.AddRelativeForce(-Vector3.forward * Speed);
        }

        // Backward control
        if (Input.GetKey(KeyCode.S))
        {
            if (Input.GetKey(KeyCode.D))
            {
                Rb.AddTorque(Vector3.up * TurnSpeed);
            }
            else if (Input.GetKey(KeyCode.A))
            {
                Rb.AddTorque(-Vector3.up * TurnSpeed);
            }

            Rb.velocity = transform.TransformDirection(localVelocity);
        }

        // Drift Control
        if (Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.S))
        {
            if (Input.GetKey(KeyCode.D))
            {
                Rb.AddTorque(Vector3.up * TurnSpeed);
            }
            else if (Input.GetKey(KeyCode.A))
            {
                Rb.AddTorque(-Vector3.up * TurnSpeed);
            }
        }
        else if (!Input.GetKey(KeyCode.S))
        {
            if (Input.GetKey(KeyCode.D))
            {
                Rb.AddTorque(Vector3.up * TurnSpeed);
            }
            else if (Input.GetKey(KeyCode.A))
            {
                Rb.AddTorque(-Vector3.up * TurnSpeed);
            }

            Rb.velocity = transform.TransformDirection(localVelocity);
        }
    }
    private void AerialCarControl()
    {
        if (Input.GetKey(KeyCode.D) && Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.E))
        {
            transform.Rotate(Vector3.back, RollRotationSpeed * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.Q))
        {
            transform.Rotate(Vector3.forward, RollRotationSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.W))
        {
            transform.Rotate(Vector3.right, YawpitchRotationSpeed * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            transform.Rotate(Vector3.left, YawpitchRotationSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.LeftShift))
        {
            transform.Rotate(Vector3.up, YawpitchRotationSpeed * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.LeftShift))
        {
            transform.Rotate(Vector3.down, YawpitchRotationSpeed * Time.deltaTime);
        }
    }
    private void RoofCarControl()
    {
        if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(Vector3.up, YawpitchRotationSpeed * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(Vector3.down, YawpitchRotationSpeed * Time.deltaTime);
        }
        else if (Input.GetMouseButton(1))
        {
            Rb.AddForce(-transform.up * JumpForce);
        }
    }

    // boost
    private void BoostingControl()
    {
        if (Input.GetMouseButton(0))
        {
            if (BoostCapacity >= 0.4)
            {
                Rb.maxLinearVelocity = MaxLinearVelocity + 20;
                Rb.AddForce(transform.forward * BoostForce);
                if (!UnlimitedBoost) BoostCapacity -= Time.deltaTime * 40;
                boostIndicatorText.text = Mathf.Round(BoostCapacity).ToString();
                boostIndicatorImage.fillAmount = BoostCapacity / 100f;
            }
            if(BoostCapacity < 0.4)
            {
                BoostCapacity = 0;
            }
        }
    }
    public void AddBoost(int boost)
    {
        if (!UnlimitedBoost)
        {
            BoostCapacity += boost;
        }
        if (BoostCapacity > 100)
        {
            BoostCapacity = 100;
        }
        UpdateBoostText();
        boostFillSound.Play();
    }
    public void ResetBoost(int boost)
    {
        BoostCapacity = boost;
        UpdateBoostText();
    }
    private void UpdateBoostText()
    {
        boostIndicatorImage.fillAmount = BoostCapacity / 100f;
        boostIndicatorText.text = Mathf.Round(BoostCapacity).ToString();
    }

    private void Jump()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (IsJumpGrounded())
            {
                jumpSound.Play();
                DoubleJump = true;
                Rb.AddForce(transform.up * JumpForce);
            }
            else if (DoubleJump)
            {
                jumpSound.Play();
                DoubleJump = false;
                Rb.AddForce(transform.up * DoubleJumpForce);
            }
        }
    }

    // rays
    public bool IsJumpGrounded()
    {
        bool grounded;
        if (Physics.Raycast(transform.position, -transform.up, JumpCheckDistance))
        {
            grounded = true;
        }
        else
        {
            grounded = false;
        }

        Debug.DrawRay(transform.position, -transform.up.normalized * JumpCheckDistance, Color.blue);

        return grounded;
    }
    public bool IsControlGrounded()
    {
        bool grounded;
        if (Physics.Raycast(transform.position, -transform.up, ControlSwitchDistance))
        {
            grounded = true;
        }
        else
        {
            grounded = false;
        }

        Debug.DrawRay(transform.position, -transform.up.normalized * ControlSwitchDistance, Color.red);

        return grounded;
    }
    public bool IsCarOnRoofGrounded()
    {
        bool grounded;
        if (Physics.Raycast(transform.position, transform.up, RoofCheckDistance))
        {
            grounded = true;
        }
        else
        {
            grounded = false;
        }

        Debug.DrawRay(transform.position, transform.up.normalized * RoofCheckDistance, Color.yellow);

        return grounded;
    }
}