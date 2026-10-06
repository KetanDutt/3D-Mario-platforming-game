using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubWorldPlayer : MonoBehaviour
{
    public static bool FireMario = false;

    [Header("Suit")]
    public GameObject[] mario_suit;
    public Material[] RegMaterial;
    public Material[] FireMaterial;

    private Rigidbody rb;
    private Animator player_anim;

    [Header("Movement Settings")]
    public float moveSpeed = 12f;
    private bool Grounded = true;
    private bool jump_bool = false;
    public float JumpForce = 600f;
    public float gravityForce = 25f;

    [Header("Level Entry")]
    public bool entering_level = false;
    public bool entering_level2 = false;
    private GameObject LevelColliderWithScript;
    private GameObject whichLevelToJump;

    private float inputX;
    private float inputZ;
    private bool jumpRequested = false;

    void Start()
    {
        FireMario = Player.FireMario;
        rb = GetComponent<Rigidbody>();
        player_anim = GetComponent<Animator>();
        UpdateSuit();
    }

    void Update()
    {
        if (entering_level || entering_level2) return;

        inputX = Input.GetAxis("Horizontal");
        inputZ = Input.GetAxis("Vertical");

        if (Input.GetKeyDown(KeyCode.Space) && Grounded)
        {
            jumpRequested = true;
        }

        bool isMoving = (Mathf.Abs(inputX) > 0.05f || Mathf.Abs(inputZ) > 0.05f);
        if (player_anim != null)
        {
            player_anim.SetBool("Moving", isMoving);
        }
    }

    void FixedUpdate()
    {
        if (entering_level && whichLevelToJump != null)
        {
            JumpInLevel(whichLevelToJump);
            return;
        }
        else if (entering_level2 && whichLevelToJump != null)
        {
            JumpInLevel2(whichLevelToJump);
            return;
        }

        Vector3 moveDir = new Vector3(-inputX, 0, -inputZ);
        if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

        if (rb != null)
        {
            rb.velocity = new Vector3(moveDir.x * moveSpeed, rb.velocity.y, moveDir.z * moveSpeed);
            rb.AddForce(Vector3.down * gravityForce, ForceMode.Acceleration);
        }

        if (moveDir != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDir), 15f * Time.fixedDeltaTime);
        }

        if (jumpRequested)
        {
            jumpRequested = false;
            ExecuteJump();
        }
    }

    void ExecuteJump()
    {
        if (Grounded && rb != null)
        {
            jump_bool = true;
            Grounded = false;
            if (player_anim != null)
            {
                player_anim.SetBool("Jump", true);
                player_anim.SetBool("Moving", false);
            }
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            rb.AddForce(Vector3.up * JumpForce, ForceMode.Impulse);
        }
    }

    void UpdateSuit()
    {
        if (mario_suit == null || mario_suit.Length < 2) return;

        if (FireMario && FireMaterial != null && FireMaterial.Length >= 2)
        {
            mario_suit[0].GetComponent<Renderer>().sharedMaterial = FireMaterial[0];
            mario_suit[1].GetComponent<Renderer>().sharedMaterial = FireMaterial[1];
        }
        else if (RegMaterial != null && RegMaterial.Length >= 2)
        {
            mario_suit[0].GetComponent<Renderer>().sharedMaterial = RegMaterial[0];
            mario_suit[1].GetComponent<Renderer>().sharedMaterial = RegMaterial[1];
        }
    }

    void JumpInLevel(GameObject whichLevel)
    {
        if (rb != null) rb.velocity = new Vector3(0, rb.velocity.y, 0);
        Vector3 targetPos = new Vector3(whichLevel.transform.position.x, transform.position.y, whichLevel.transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPos, 3f * Time.fixedDeltaTime);
        transform.LookAt(targetPos);

        if (Camera.main != null && Camera.main.transform.parent != null && LevelColliderWithScript != null)
        {
            WorldMapLevelEnter enterScript = LevelColliderWithScript.GetComponent<WorldMapLevelEnter>();
            if (enterScript != null && enterScript.level1Camera != null)
            {
                Camera.main.transform.parent.position = Vector3.Lerp(Camera.main.transform.parent.position, enterScript.level1Camera.position, 3f * Time.fixedDeltaTime);
            }
        }
        if (player_anim != null) player_anim.SetBool("LevelIn", true);
    }

    void JumpInLevel2(GameObject whichLevel)
    {
        if (rb != null) rb.velocity = new Vector3(0, rb.velocity.y, 0);
        Vector3 targetPos = new Vector3(whichLevel.transform.position.x, transform.position.y, whichLevel.transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPos, 3f * Time.fixedDeltaTime);
        transform.LookAt(targetPos);

        if (Camera.main != null && Camera.main.transform.parent != null && LevelColliderWithScript != null)
        {
            WorldMapLevelEnter enterScript = LevelColliderWithScript.GetComponent<WorldMapLevelEnter>();
            if (enterScript != null && enterScript.Level2Camera != null)
            {
                Camera.main.transform.parent.position = Vector3.Lerp(Camera.main.transform.parent.position, enterScript.Level2Camera.position, 3f * Time.fixedDeltaTime);
            }
        }
        if (player_anim != null) player_anim.SetBool("LevelIn", true);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("ground"))
        {
            Grounded = true;
            jump_bool = false;
            if (player_anim != null) player_anim.SetBool("Jump", false);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("ground") && !jump_bool)
        {
            Grounded = true;
            if (player_anim != null) player_anim.SetBool("Jump", false);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("ground"))
        {
            Grounded = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (Input.GetKeyDown(KeyCode.Space) && other.CompareTag("LEVEL1") && !entering_level)
        {
            entering_level = true;
            LevelColliderWithScript = other.gameObject;
            WorldMapLevelEnter enterScript = other.GetComponent<WorldMapLevelEnter>();
            if (enterScript != null)
            {
                whichLevelToJump = enterScript.level1;
                StartCoroutine(enterScript.EnterLevel(enterScript.Level1Particles));
            }
            if (Camera.main != null && Camera.main.transform.parent != null)
            {
                HubWorldCamera hCam = Camera.main.transform.parent.GetComponent<HubWorldCamera>();
                if (hCam != null) hCam.following_Player = false;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Space) && other.CompareTag("LEVEL2") && !entering_level2)
        {
            entering_level2 = true;
            LevelColliderWithScript = other.gameObject;
            WorldMapLevelEnter enterScript = other.GetComponent<WorldMapLevelEnter>();
            if (enterScript != null)
            {
                whichLevelToJump = enterScript.level2;
                StartCoroutine(enterScript.EnterLevel2(enterScript.Level2Particles));
            }
            if (Camera.main != null && Camera.main.transform.parent != null)
            {
                HubWorldCamera hCam = Camera.main.transform.parent.GetComponent<HubWorldCamera>();
                if (hCam != null) hCam.following_Player = false;
            }
        }
    }
}
