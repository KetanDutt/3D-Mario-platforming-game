using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Level & Identification")]
    public string CURRENTLEVEL;
    public GameObject player;

    [Header("Movement Settings")]
    public Rigidbody rb;
    public float desired_move_speed = 12f;
    public float crouch_move_speed = 3.5f;
    private float movespeed;
    private Vector3 inputVector;
    private Vector3 targetRotationDir;

    [Header("Jump & Grounding")]
    public bool grounded;
    public float jumpForce = 1500f;
    public float extra_gravity_if_needed = 0f;
    public float coyoteDuration = 0.15f;
    private float coyoteTimer = 0f;
    public float jumpBufferDuration = 0.15f;
    private float jumpBufferTimer = 0f;
    private int jump_count = 1;
    private float lastJumpTime = 0f;

    [Header("Colliders")]
    public CapsuleCollider reg_coll;
    public CapsuleCollider crouch_col;

    [Header("Dust & Effects")]
    public WalkDustManager walkdustmanager;
    public ParticleSystem GroundPoundDust;
    public ParticleSystem WallJumpPS;
    public Animator cam_shake;
    private AudioSource groundpound_audio;
    private int particleCount = 0;

    [Header("Wall Jump")]
    public GameObject wallraydetector;
    public LayerMask ignoreWalls;
    private bool walljumpbool = false;

    [Header("Ground Pound")]
    public bool groundpound = false;
    private bool groundPoundQueued = false;

    [Header("Animations")]
    public Animator player_anim;
    public AnimatorOverrideController player_shell_anim;
    private RuntimeAnimatorController default_anim_controller;

    [Header("Items & Power-ups")]
    public GameObject[] Question_Block_Items;
    public Material[] fire_material;
    public Material[] reg_material;
    public GameObject[] mario_suit;
    public GameObject[] entire_mario_body;
    public static bool FireMario = false;
    public bool MEGAMUSHROOM = false;
    public Animator mega;
    public Vector3 DESIRED_SCALE = new Vector3(0.7f, 0.7f, 0.7f);

    [Header("Audio (Mario_Effects)")]
    // 0: powerup, 1: Jump1, 2: Jump2, 3: Jump3, 4: Hurt/Downgrade, 5: Fireball, 6: Item, 7: Grow, 8: MegaMusic, 9: Extra, 10: Flag
    public AudioSource[] Mario_Effects;

    [Header("Fireball Settings")]
    public GameObject Fireball;
    public Vector3 velocity = new Vector3(0, -16, 19);
    public Transform fireball_spawn_loc;
    private bool canShoot = true;
    private float shootCooldownTimer = 0f;
    private const float SHOOT_COOLDOWN = 0.35f;

    [Header("Combat & Interaction")]
    public GameObject PunchDetector;
    private float punch_time = 2f;
    public GameObject Coin;
    public GameObject Sprite_1up;
    public GameObject[] ui_GreenStars;

    [Header("Koopa Shell Interaction")]
    public GameObject koopaShellHoldPos;
    [HideInInspector]
    public bool holdingShell = false;
    private GameObject koopashell;
    [HideInInspector]
    public float koopashellInvincible = 0f;

    [Header("Pipe & State")]
    public bool PipeEntry = false;
    private bool isInvincible = false;
    private Vector3 respawnPosition;

    [Header("Level End Sequence")]
    [HideInInspector]
    public bool REACHED_GOAL = false;
    [HideInInspector]
    public bool flagpole_end = false;
    private bool move_down_pole = false;
    private GameObject flagpole = null;
    private Transform mario_level_end_position;
    private bool move_out_of_camera = false;
    private bool play_flag_sound = true;

    // Cached Input state
    private float inputH;
    private float inputV;
    private bool inputCrouch;

    void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (groundpound_audio == null) groundpound_audio = GetComponent<AudioSource>();
        if (player_anim != null) default_anim_controller = player_anim.runtimeAnimatorController;
    }

    void Start()
    {
        movespeed = desired_move_speed;
        grounded = false;
        respawnPosition = transform.position;

        GameObject endPosObj = GameObject.FindGameObjectWithTag("LevelEndPosition");
        if (endPosObj != null)
        {
            mario_level_end_position = endPosObj.transform;
        }

        FireMario = HubWorldPlayer.FireMario;
        if (FireMario)
        {
            CorrectSuitOnStart();
        }
    }

    void Update()
    {
        // Decrement timers
        if (coyoteTimer > 0f) coyoteTimer -= Time.deltaTime;
        if (jumpBufferTimer > 0f) jumpBufferTimer -= Time.deltaTime;
        if (shootCooldownTimer > 0f) shootCooldownTimer -= Time.deltaTime;
        koopashellInvincible += Time.deltaTime;
        punch_time += Time.deltaTime;

        // Fall into abyss / death boundary check
        if (transform.position.y < -25f && !REACHED_GOAL)
        {
            RespawnFromPit();
            return;
        }

        if (PipeEntry || REACHED_GOAL) return;

        // Sample directional inputs in Update for maximum responsiveness
        inputH = Input.GetAxis("Horizontal");
        inputV = Input.GetAxis("Vertical");
        inputCrouch = Input.GetKey(KeyCode.LeftShift);

        // Jump input buffered
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferTimer = jumpBufferDuration;
        }

        // Ground pound input
        if (Input.GetKeyDown(KeyCode.V) && !grounded && !holdingShell && !groundpound)
        {
            groundPoundQueued = true;
        }

        // Fireball input
        if (Input.GetMouseButtonDown(0) && FireMario && shootCooldownTimer <= 0f && canShoot)
        {
            StartCoroutine(Shoot_Fireball());
        }

        // Shell throw input
        if (Input.GetMouseButtonUp(1) && holdingShell)
        {
            ThrowShell();
        }

        // Coin counter check
        if (CoinCollect.COIN_COUNT > 99)
        {
            Lives.LIVES++;
            CoinCollect.COIN_COUNT = 0;
            if (Sprite_1up != null)
            {
                ParticleSystem ps = Sprite_1up.GetComponent<ParticleSystem>();
                if (ps != null) ps.Play();
                AudioSource aud = Sprite_1up.GetComponent<AudioSource>();
                if (aud != null) aud.Play();
            }
        }
    }

    void FixedUpdate()
    {
        // Execute queued groundpound
        if (groundPoundQueued)
        {
            groundPoundQueued = false;
            StartCoroutine(GroundPound());
        }

        // Execute queued jump via coyote time & jump buffer
        if (jumpBufferTimer > 0f && coyoteTimer > 0f && !groundpound && !PipeEntry && !REACHED_GOAL)
        {
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
            ExecuteJump();
        }

        // Handle Movement
        if (!groundpound && !walljumpbool && !PipeEntry && !REACHED_GOAL)
        {
            Camera mainCam = Camera.main;
            Vector3 camRight = mainCam != null ? mainCam.transform.right : Vector3.right;
            Vector3 camForward = mainCam != null ? mainCam.transform.forward : Vector3.forward;
            camRight.y = 0f;
            camForward.y = 0f;
            camRight.Normalize();
            camForward.Normalize();

            Vector3 moveDir = (camRight * inputH + camForward * inputV);
            if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

            float currentSpeed = inputCrouch ? crouch_move_speed : desired_move_speed;
            inputVector = moveDir * currentSpeed;

            // Apply movement velocity smoothly, preserving vertical velocity
            rb.velocity = new Vector3(inputVector.x, rb.velocity.y, inputVector.z);

            // Rotate Mario towards movement direction smoothly
            if (moveDir.sqrMagnitude > 0.001f)
            {
                targetRotationDir = moveDir;
                Quaternion targetRot = Quaternion.LookRotation(targetRotationDir, Vector3.up);
                player.transform.rotation = Quaternion.Slerp(player.transform.rotation, targetRot, 15f * Time.fixedDeltaTime);
            }
        }

        // Extra gravity if in air and not groundpounding
        if (!grounded && !groundpound && extra_gravity_if_needed > 0f)
        {
            rb.AddForce(Vector3.down * extra_gravity_if_needed, ForceMode.Acceleration);
        }

        // Wall jump check
        if (!REACHED_GOAL)
        {
            Walljump();
        }

        // Mega Mushroom scale tweening
        if (MEGAMUSHROOM)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(2.5f, 2.5f, 2.5f), 4f * Time.fixedDeltaTime);
        }
        else
        {
            transform.localScale = Vector3.Lerp(transform.localScale, DESIRED_SCALE, 4f * Time.fixedDeltaTime);
        }

        // Update Animation States
        UpdateMovementAnimations();

        // Level End flagpole sequence
        HandleLevelEndMovement();
    }

    void UpdateMovementAnimations()
    {
        if (player_anim == null || PipeEntry || REACHED_GOAL) return;

        bool hasMoveInput = (Mathf.Abs(inputH) > 0.05f || Mathf.Abs(inputV) > 0.05f);

        if (grounded)
        {
            if (inputCrouch && !holdingShell)
            {
                player_anim.SetBool("CrouchIdle", !hasMoveInput);
                player_anim.SetBool("CrouchMove", hasMoveInput);
                player_anim.SetBool("Moving", hasMoveInput);
                if (reg_coll) reg_coll.enabled = false;
                if (crouch_col) crouch_col.enabled = true;
            }
            else
            {
                player_anim.SetBool("CrouchIdle", false);
                player_anim.SetBool("CrouchMove", false);
                player_anim.SetBool("Moving", hasMoveInput);
                if (reg_coll) reg_coll.enabled = true;
                if (crouch_col) crouch_col.enabled = false;
            }

            if (MEGAMUSHROOM && cam_shake != null)
            {
                cam_shake.SetBool("Shake", hasMoveInput && rb.velocity.magnitude > 0.5f);
            }
        }
    }

    void ExecuteJump()
    {
        grounded = false;
        if (player_anim != null)
        {
            player_anim.SetBool("Jump", true);
            player_anim.SetBool("Moving", false);
        }

        // Triple jump chaining: jump 1, jump 2, jump 3 (with bonus force)
        float jumpBonus = 1f;
        if (Time.time - lastJumpTime < 1.2f)
        {
            jump_count++;
            if (jump_count > 3) jump_count = 1;
        }
        else
        {
            jump_count = 1;
        }
        lastJumpTime = Time.time;

        if (jump_count == 3) jumpBonus = 1.25f;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(Vector3.up * jumpForce * jumpBonus, ForceMode.Impulse);

        if (walkdustmanager != null && walkdustmanager.Walkdust != null)
        {
            walkdustmanager.Walkdust.Stop();
        }

        // Play Jump sound safely
        if (Mario_Effects != null && jump_count < Mario_Effects.Length && Mario_Effects[jump_count] != null)
        {
            Mario_Effects[jump_count].Play();
        }
    }

    void Walljump()
    {
        if (wallraydetector == null) return;

        Ray wallRay = new Ray(wallraydetector.transform.position, wallraydetector.transform.forward);
        RaycastHit hit;

        RaycastHit hitdown;
        Ray downRay = new Ray(transform.position, -Vector3.up);
        bool offground = Physics.Raycast(downRay, out hitdown) && hitdown.distance > 0.6f;

        particleCount++;
        if (particleCount > 4 && WallJumpPS != null)
        {
            WallJumpPS.Play();
            particleCount = 0;
        }

        if (Physics.Raycast(wallRay, out hit, 0.7f, ignoreWalls) && hit.normal.y < 0.1f && offground && rb.velocity.y <= 0f)
        {
            rb.drag = 5f;
            if (player_anim != null)
            {
                player_anim.SetBool("WallJumpLeft", true);
                player_anim.SetBool("Jump", false);
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                walljumpbool = true;
                rb.drag = 0f;
                rb.velocity = new Vector3(hit.normal.x * 9f, 18f, hit.normal.z * 9f);

                if (hit.normal != Vector3.zero)
                {
                    player.transform.rotation = Quaternion.LookRotation(hit.normal, Vector3.up);
                }

                if (player_anim != null)
                {
                    player_anim.SetBool("WallJumpLeft", false);
                    player_anim.SetBool("Jump", true);
                }

                if (Mario_Effects != null && jump_count < Mario_Effects.Length && Mario_Effects[jump_count] != null)
                {
                    Mario_Effects[jump_count].Play();
                }

                StartCoroutine(ResetWallJumpBool(0.2f));
            }
        }
        else
        {
            if (player_anim != null) player_anim.SetBool("WallJumpLeft", false);
            rb.drag = 0f;
            if (WallJumpPS != null) WallJumpPS.Stop();
        }
    }

    IEnumerator ResetWallJumpBool(float delay)
    {
        yield return new WaitForSeconds(delay);
        walljumpbool = false;
    }

    IEnumerator GroundPound()
    {
        Ray downRay = new Ray(transform.position, -Vector3.up);
        RaycastHit hit;

        if (Physics.Raycast(downRay, out hit) && hit.distance > 1.2f)
        {
            grounded = false;
            groundpound = true;

            if (wallraydetector != null) wallraydetector.SetActive(false);
            rb.drag = 0f;

            if (player_anim != null)
            {
                player_anim.SetBool("GroundPound", true);
                player_anim.SetBool("Jump", false);
            }

            rb.velocity = Vector3.zero;
            rb.useGravity = false;

            yield return new WaitForSeconds(0.35f);

            rb.velocity = new Vector3(0, -32f - extra_gravity_if_needed, 0);

            // Safe loop waiting for ground with timeout so we never lock up
            float timeout = 3.5f;
            while (!grounded && timeout > 0f)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }

            if (cam_shake != null) cam_shake.SetBool("Shake", true);
            if (GroundPoundDust != null) GroundPoundDust.Play();
            if (groundpound_audio != null) groundpound_audio.Play();

            yield return new WaitForSeconds(0.12f);
            if (cam_shake != null) cam_shake.SetBool("Shake", false);

            groundpound = false;
            rb.useGravity = true;
            rb.isKinematic = true;

            yield return new WaitForSeconds(0.3f);
            if (player_anim != null) player_anim.SetBool("GroundPound", false);

            yield return new WaitForSeconds(0.08f);
            rb.isKinematic = false;
            if (wallraydetector != null) wallraydetector.SetActive(true);
        }
    }

    IEnumerator Shoot_Fireball()
    {
        shootCooldownTimer = SHOOT_COOLDOWN;
        if (player_anim != null)
        {
            player_anim.SetTrigger("Shoot");
            player_anim.SetBool("Jump", false);
        }

        yield return new WaitForSeconds(0.08f);

        if (Mario_Effects != null && Mario_Effects.Length > 5 && Mario_Effects[5] != null)
        {
            Mario_Effects[5].Play();
        }

        if (Fireball != null && fireball_spawn_loc != null)
        {
            GameObject clone = Instantiate(Fireball, fireball_spawn_loc.position, transform.rotation);
            Fireball fbScript = clone.GetComponent<Fireball>();
            if (fbScript != null) fbScript.enabled = true;

            Rigidbody fbRb = clone.GetComponent<Rigidbody>();
            if (fbRb != null)
            {
                fbRb.velocity = transform.TransformDirection(velocity);
            }

            Destroy(clone, 6f);
        }
    }

    public void TakeDamage(int damage = 1)
    {
        if (isInvincible || MEGAMUSHROOM || REACHED_GOAL) return;

        if (FireMario)
        {
            StartCoroutine(Downgrade_FireSuit());
        }
        else
        {
            Lives.LIVES -= damage;
            if (Mario_Effects != null && Mario_Effects.Length > 4 && Mario_Effects[4] != null)
            {
                Mario_Effects[4].Play();
            }

            if (Lives.LIVES <= 0)
            {
                Die();
            }
            else
            {
                StartCoroutine(FlickerEffect());
            }
        }
    }

    public void RespawnFromPit()
    {
        Lives.LIVES--;
        if (Mario_Effects != null && Mario_Effects.Length > 4 && Mario_Effects[4] != null)
        {
            Mario_Effects[4].Play();
        }

        if (Lives.LIVES <= 0)
        {
            Die();
        }
        else
        {
            rb.velocity = Vector3.zero;
            transform.position = respawnPosition + Vector3.up * 1.5f;
            groundpound = false;
            StartCoroutine(FlickerEffect());
        }
    }

    public void SetCheckpoint(Vector3 newCheckpointPos)
    {
        respawnPosition = newCheckpointPos;
    }

    public void Die()
    {
        if (player_anim != null)
        {
            player_anim.SetTrigger("Dead");
        }

        // Return to World Map or reload scene after delay
        StartCoroutine(RestartAfterDeath());
    }

    IEnumerator RestartAfterDeath()
    {
        yield return new WaitForSeconds(2.5f);
        Lives.LIVES = 5;
        GameObject manager = GameObject.FindGameObjectWithTag("SceneManager");
        if (manager != null)
        {
            sceneManage manage_script = manager.GetComponent<sceneManage>();
            if (manage_script != null)
            {
                StartCoroutine(manage_script.levelToWorldMap());
                yield break;
            }
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public IEnumerator QuestionBlockHit(GameObject block, int direction)
    {
        AudioSource blockAudio = block.transform.parent != null ? block.transform.parent.GetComponent<AudioSource>() : null;
        if (blockAudio != null) blockAudio.Play();

        QuestionBlockID qId = block.GetComponent<QuestionBlockID>();
        int itemId = qId != null ? qId.ITEM_ID : 0;

        if (itemId == 1 && Question_Block_Items != null && Question_Block_Items.Length > 1)
        {
            if (Camera.main != null && Camera.main.transform.childCount > 1)
            {
                AudioSource itemAudio = Camera.main.transform.GetChild(1).GetComponent<AudioSource>();
                if (itemAudio != null) itemAudio.Play();
            }
            GameObject flowerClone = Instantiate(Question_Block_Items[1], block.transform.position + Vector3.up * 0.8f, block.transform.rotation);
            Collider col = flowerClone.GetComponentInChildren<Collider>();
            if (col != null) col.enabled = true;
        }

        // Block bounce animation
        Vector3 initialPos = block.transform.position;
        for (int i = 0; i < 4; i++)
        {
            block.transform.position += new Vector3(0, 0.12f * direction, 0);
            yield return new WaitForSeconds(0.01f);
        }
        for (int i = 0; i < 4; i++)
        {
            block.transform.position -= new Vector3(0, 0.12f * direction, 0);
            yield return new WaitForSeconds(0.01f);
        }
        block.transform.position = initialPos;

        // Switch to empty block visual
        BoxCollider boxCol = block.GetComponent<BoxCollider>();
        if (boxCol != null) boxCol.enabled = false;

        if (block.transform.childCount >= 3)
        {
            block.transform.GetChild(0).gameObject.SetActive(false);
            block.transform.GetChild(1).gameObject.SetActive(false);
            block.transform.GetChild(2).gameObject.SetActive(true);
        }

        // Dispense Coin
        if (itemId == 0 && Question_Block_Items != null && Question_Block_Items.Length > 0 && Question_Block_Items[0] != null)
        {
            GameObject coinClone = Instantiate(Question_Block_Items[0], block.transform.position + new Vector3(0, 0.5f, 0), Question_Block_Items[0].transform.rotation);
            SphereCollider sc = coinClone.GetComponent<SphereCollider>();
            if (sc != null) sc.enabled = false;

            Rigidbody coinRb = coinClone.GetComponent<Rigidbody>();
            if (coinRb != null)
            {
                coinRb.constraints = RigidbodyConstraints.None;
                coinRb.useGravity = true;
                coinRb.AddForce(Vector3.up * 7f, ForceMode.Impulse);
            }

            yield return new WaitForSeconds(0.2f);

            if (coinClone != null)
            {
                if (coinRb != null) coinRb.useGravity = false;
                AudioSource coinAud = coinClone.GetComponent<AudioSource>();
                if (coinAud != null) coinAud.Play();
                if (coinClone.transform.childCount > 0)
                {
                    MeshRenderer mr = coinClone.transform.GetChild(0).GetComponent<MeshRenderer>();
                    if (mr != null) mr.enabled = false;
                }
                ParticleSystem ps = coinClone.GetComponent<ParticleSystem>();
                if (ps != null) ps.Play();
                Destroy(coinClone, 0.8f);
            }
            CoinCollect.COIN_COUNT++;
        }
    }

    public void BrickBlockHit(GameObject brick)
    {
        AudioSource aud = brick.GetComponent<AudioSource>();
        if (aud != null) aud.Play();

        for (int i = 0; i < brick.transform.childCount; i++)
        {
            ParticleSystem ps = brick.transform.GetChild(i).GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();
        }

        MeshRenderer mr = brick.GetComponent<MeshRenderer>();
        if (mr != null) mr.enabled = false;
        BoxCollider bc = brick.GetComponent<BoxCollider>();
        if (bc != null) bc.enabled = false;

        Destroy(brick, 1.5f);
    }

    void OnCollisionEnter(Collision other)
    {
        walljumpbool = false;

        if (other.gameObject.CompareTag("ground") || other.gameObject.CompareTag("CurveGround") ||
            (other.contacts.Length > 0 && other.contacts[0].normal.y > 0.6f))
        {
            grounded = true;
            coyoteTimer = coyoteDuration;
            if (player_anim != null)
            {
                player_anim.SetBool("Jump", false);
                player_anim.SetBool("WallJumpLeft", false);
            }
            rb.drag = 0f;

            if (MEGAMUSHROOM && Camera.main != null)
            {
                Animator camAnim = Camera.main.GetComponent<Animator>();
                if (camAnim != null) camAnim.SetTrigger("Shake");
            }
        }

        if (other.gameObject.CompareTag("Question"))
        {
            if (rb.velocity.y > -0.2f && other.contacts.Length > 0 && other.contacts[0].normal.y < -0.7f)
            {
                StartCoroutine(QuestionBlockHit(other.gameObject, 1));
            }
            else if (groundpound && other.contacts.Length > 0 && other.contacts[0].normal.y >= 0.7f)
            {
                StartCoroutine(QuestionBlockHit(other.gameObject, -1));
            }
            else if (MEGAMUSHROOM)
            {
                Destroy(other.gameObject);
            }
        }

        if (other.gameObject.CompareTag("BrickBlock"))
        {
            if ((rb.velocity.y > -0.1f && other.contacts.Length > 0 && other.contacts[0].normal.y < -0.75f) || MEGAMUSHROOM)
            {
                BrickBlockHit(other.gameObject);
            }
        }

        if (other.gameObject.CompareTag("Crate") && MEGAMUSHROOM)
        {
            Crate crateScript = other.gameObject.GetComponent<Crate>();
            if (crateScript != null) StartCoroutine(crateScript.Destroy_GroundPound());
        }

        if (other.gameObject.CompareTag("GoombaEnemy") && MEGAMUSHROOM)
        {
            CrushEnemy(other.gameObject);
        }

        if (other.gameObject.CompareTag("MegaGoomba") && MEGAMUSHROOM)
        {
            CrushMegaGoomba(other.gameObject);
        }

        if (other.gameObject.CompareTag("Tree") && MEGAMUSHROOM)
        {
            Vector3 dir = other.contacts.Length > 0 ? other.contacts[0].point - transform.position : transform.forward;
            StartCoroutine(DestroyTree(other.gameObject, dir));
        }

        if (other.gameObject.CompareTag("KoopaShell"))
        {
            KoopaShell shell = other.gameObject.GetComponent<KoopaShell>();
            if (shell != null && !shell.moving && !Input.GetMouseButton(1))
            {
                koopashellInvincible = 0f;
                Vector3 dir = (other.transform.position - transform.position).normalized;
                dir.y = 0f;
                shell.velocity = dir * 25f;
                shell.moving = true;
                if (shell.transform.childCount > 0)
                {
                    Animator spinAnim = shell.transform.GetChild(0).GetComponent<Animator>();
                    if (spinAnim != null) spinAnim.SetBool("Spin", true);
                }
            }
            else if (shell != null && !shell.moving && Input.GetMouseButton(1) && koopaShellHoldPos != null)
            {
                shell.holdPos = koopaShellHoldPos.transform;
                shell.heldByPlayer = true;
                holdingShell = true;
                koopashell = other.gameObject;
                SphereCollider sc = koopaShellHoldPos.GetComponent<SphereCollider>();
                if (sc != null) sc.enabled = true;
                if (player_anim != null && player_shell_anim != null)
                {
                    player_anim.runtimeAnimatorController = player_shell_anim;
                }
            }
        }
    }

    void OnCollisionStay(Collision other)
    {
        if (other.gameObject.CompareTag("ground") || other.gameObject.CompareTag("CurveGround") ||
            (other.contacts.Length > 0 && other.contacts[0].normal.y > 0.6f))
        {
            grounded = true;
            coyoteTimer = coyoteDuration;
            if (player_anim != null && !Input.GetKey(KeyCode.Space))
            {
                player_anim.SetBool("Jump", false);
            }
            rb.drag = 0f;
        }
    }

    void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("ground") || other.gameObject.CompareTag("CurveGround"))
        {
            grounded = false;
        }
    }

    IEnumerator OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Fireflower"))
        {
            if (!FireMario) StartCoroutine(FireSuit(other.gameObject));
            else Destroy(other.gameObject);
            FireMario = true;
        }

        // Stomp Goomba
        if (other.gameObject.CompareTag("GoombaDeath") && rb.velocity.y < 0.5f)
        {
            GoombaChase goomba = other.gameObject.GetComponentInParent<GoombaChase>();
            if (goomba != null)
            {
                StartCoroutine(goomba.Dead());
            }

            // Clean single jump impulse bounce on stomp
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector3.up * (jumpForce * 0.75f), ForceMode.Impulse);

            if (player_anim != null) player_anim.Play("Jump", -1, 0f);

            // Spawn reward coin safely
            if (Coin != null)
            {
                GameObject clone = Instantiate(Coin, other.transform.position + Vector3.up * 0.5f, Coin.transform.rotation);
                SphereCollider sc = clone.GetComponent<SphereCollider>();
                if (sc != null) sc.enabled = false;
                Rigidbody coinRb = clone.GetComponent<Rigidbody>();
                if (coinRb != null)
                {
                    coinRb.useGravity = true;
                    coinRb.AddForce(Vector3.up * 5f, ForceMode.Impulse);
                }

                yield return new WaitForSeconds(0.2f);
                if (clone != null)
                {
                    if (coinRb != null) coinRb.useGravity = false;
                    AudioSource aud = clone.GetComponent<AudioSource>();
                    if (aud != null) aud.Play();
                    ParticleSystem ps = clone.GetComponent<ParticleSystem>();
                    if (ps != null) ps.Play();
                    Destroy(clone, 0.8f);
                }
                CoinCollect.COIN_COUNT++;
            }
        }

        if (other.gameObject.CompareTag("Crate") && groundpound)
        {
            GameObject crateObj = other.transform.parent != null ? other.transform.parent.gameObject : other.gameObject;
            Crate crateScript = crateObj.GetComponent<Crate>();
            if (crateScript != null) StartCoroutine(crateScript.Destroy_GroundPound());
        }

        if (other.gameObject.CompareTag("BrickBlock") && groundpound)
        {
            GameObject brickObj = other.transform.parent != null ? other.transform.parent.gameObject : other.gameObject;
            BrickBlockHit(brickObj);
            if (cam_shake != null) cam_shake.SetBool("Shake", true);
            rb.velocity = new Vector3(0, -30, 0);
        }

        if (other.gameObject.CompareTag("Coins"))
        {
            AudioSource aud = other.gameObject.GetComponent<AudioSource>();
            if (aud != null) aud.Play();
            SphereCollider sc = other.gameObject.GetComponent<SphereCollider>();
            if (sc != null) sc.enabled = false;
            if (other.transform.childCount > 0)
            {
                MeshRenderer mr = other.transform.GetChild(0).GetComponent<MeshRenderer>();
                if (mr != null) mr.enabled = false;
            }
            ParticleSystem ps = other.gameObject.GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();
            Destroy(other.gameObject, 0.8f);
            CoinCollect.COIN_COUNT++;
        }

        if (other.gameObject.CompareTag("MegaMushroom"))
        {
            StartCoroutine(ActivateMegaMushroom(other.gameObject));
        }

        if (other.gameObject.CompareTag("1UP Mushroom"))
        {
            if (Sprite_1up != null)
            {
                ParticleSystem ps = Sprite_1up.GetComponent<ParticleSystem>();
                if (ps != null) ps.Play();
                AudioSource aud = Sprite_1up.GetComponent<AudioSource>();
                if (aud != null) aud.Play();
            }
            Destroy(other.gameObject);
            Lives.LIVES++;
        }

        if (other.gameObject.CompareTag("GreenStar"))
        {
            GreenStarID greenstar = other.gameObject.GetComponent<GreenStarID>();
            if (greenstar != null && ui_GreenStars != null && greenstar.ID < ui_GreenStars.Length && ui_GreenStars[greenstar.ID] != null)
            {
                ui_GreenStars[greenstar.ID].SetActive(true);
            }
            Animator starAnim = other.gameObject.GetComponent<Animator>();
            if (starAnim != null) starAnim.SetBool("Collected", true);
            AudioSource aud = other.gameObject.GetComponent<AudioSource>();
            if (aud != null) aud.Play();
            SphereCollider sc = other.gameObject.GetComponent<SphereCollider>();
            if (sc != null) sc.enabled = false;
        }

        if (other.gameObject.CompareTag("Koopa"))
        {
            Koopa koopaScript = other.gameObject.GetComponent<Koopa>();
            if (koopaScript != null) koopaScript.die();
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector3.up * (jumpForce * 0.75f), ForceMode.Impulse);
        }

        // Flagpole Level End
        if (other.gameObject.CompareTag("Flagpole") && !REACHED_GOAL)
        {
            REACHED_GOAL = true;
            rb.isKinematic = true;
            flagpole = other.gameObject;
            move_down_pole = true;
            if (player_anim != null) player_anim.SetBool("Goal", true);

            AudioSource flagAudio = other.gameObject.GetComponent<AudioSource>();
            if (flagAudio != null) flagAudio.Play();

            if (Camera.main != null)
            {
                AudioSource camAudio = Camera.main.GetComponent<AudioSource>();
                if (camAudio != null) camAudio.Stop();
            }
        }

        if (other.gameObject.CompareTag("FlagpoleEnd"))
        {
            flagpole_end = true;
            if (flagpole != null)
            {
                CapsuleCollider cc = flagpole.GetComponent<CapsuleCollider>();
                if (cc != null) cc.enabled = false;
            }
        }
    }

    IEnumerator ActivateMegaMushroom(GameObject mushroomObj)
    {
        if (Mario_Effects != null)
        {
            if (Mario_Effects.Length > 7 && Mario_Effects[7] != null) Mario_Effects[7].Play();
            if (Mario_Effects.Length > 6 && Mario_Effects[6] != null) Mario_Effects[6].Play();
            if (Mario_Effects.Length > 8 && Mario_Effects[8] != null) Mario_Effects[8].Play();
        }

        if (mario_suit != null && mario_suit.Length >= 2 && reg_material != null && reg_material.Length >= 2)
        {
            mario_suit[0].GetComponent<Renderer>().sharedMaterial = reg_material[0];
            mario_suit[1].GetComponent<Renderer>().sharedMaterial = reg_material[1];
        }
        FireMario = false;
        MEGAMUSHROOM = true;

        float camVol = 1f;
        AudioSource camAudio = Camera.main != null ? Camera.main.GetComponent<AudioSource>() : null;
        if (camAudio != null)
        {
            camVol = camAudio.volume;
            camAudio.volume = 0f;
        }

        Destroy(mushroomObj);

        RuntimeAnimatorController normalController = player_anim != null ? player_anim.runtimeAnimatorController : null;
        if (mega != null && player_anim != null)
        {
            player_anim.runtimeAnimatorController = mega.runtimeAnimatorController;
        }

        yield return new WaitForSeconds(18f);

        if (Mario_Effects != null && Mario_Effects.Length > 8 && Mario_Effects[8] != null)
        {
            Mario_Effects[8].Stop();
        }
        if (camAudio != null) camAudio.volume = camVol;

        MEGAMUSHROOM = false;
        if (player_anim != null && normalController != null)
        {
            player_anim.runtimeAnimatorController = normalController;
        }
        if (cam_shake != null) cam_shake.SetBool("Shake", false);
    }

    void CrushEnemy(GameObject enemy)
    {
        GoombaChase g = enemy.GetComponent<GoombaChase>();
        if (g != null)
        {
            g.Stop();
            g.enabled = false;
            StartCoroutine(g.Dead());
        }
    }

    void CrushMegaGoomba(GameObject enemy)
    {
        MegaGoomba mg = enemy.GetComponent<MegaGoomba>();
        if (mg != null)
        {
            mg.Stop();
            mg.enabled = false;
            StartCoroutine(mg.Dead());
        }
    }

    void HandleLevelEndMovement()
    {
        if (!REACHED_GOAL) return;

        if (move_down_pole && !flagpole_end)
        {
            transform.Translate(0, -6f * Time.fixedDeltaTime, 0);
            if (play_flag_sound && Mario_Effects != null && Mario_Effects.Length > 10 && Mario_Effects[10] != null)
            {
                Mario_Effects[10].Play();
                play_flag_sound = false;
            }
        }
        else if (flagpole_end && !move_out_of_camera)
        {
            StartCoroutine(MarioFinale());
        }

        if (move_out_of_camera && mario_level_end_position != null)
        {
            Vector3 distance_to_move = new Vector3(mario_level_end_position.position.x - transform.position.x, 0, mario_level_end_position.position.z - transform.position.z);
            rb.velocity = distance_to_move.normalized * desired_move_speed * 0.8f;

            Vector3 lookAtPoint = new Vector3(mario_level_end_position.position.x, transform.position.y, mario_level_end_position.position.z);
            transform.LookAt(lookAtPoint);
        }
    }

    IEnumerator MarioFinale()
    {
        yield return new WaitForSeconds(0.5f);
        rb.isKinematic = false;
        if (player_anim != null)
        {
            player_anim.SetBool("Goal", false);
            player_anim.SetBool("EndLevel", true);
        }
        move_out_of_camera = true;

        yield return new WaitForSeconds(1.2f);
        if (Camera.main != null && Camera.main.transform.parent != null)
        {
            CameraFollow cf = Camera.main.transform.parent.GetComponent<CameraFollow>();
            if (cf != null) cf.CameraLevelEndMovement = true;
        }

        if (CURRENTLEVEL == "Level1") WorldMapLevelEnter.Level1Complete = true;
        else if (CURRENTLEVEL == "Level2") WorldMapLevelEnter.Level2Complete = true;

        yield return new WaitForSeconds(5f);

        GameObject manager = GameObject.FindGameObjectWithTag("SceneManager");
        if (manager != null)
        {
            sceneManage manage_script = manager.GetComponent<sceneManage>();
            if (manage_script != null)
            {
                StartCoroutine(manage_script.levelToWorldMap());
            }
        }
    }

    public void CorrectSuitOnStart()
    {
        if (mario_suit != null && mario_suit.Length >= 2 && fire_material != null && fire_material.Length >= 2)
        {
            Renderer suit1 = mario_suit[0].GetComponent<Renderer>();
            Renderer suit2 = mario_suit[1].GetComponent<Renderer>();
            if (suit1) suit1.sharedMaterial = fire_material[0];
            if (suit2) suit2.sharedMaterial = fire_material[1];
        }
    }

    public IEnumerator FireSuit(GameObject other)
    {
        if (Mario_Effects != null)
        {
            if (Mario_Effects.Length > 6 && Mario_Effects[6] != null) Mario_Effects[6].Play();
            if (Mario_Effects.Length > 0 && Mario_Effects[0] != null) Mario_Effects[0].Play();
        }
        Destroy(other);

        if (mario_suit == null || mario_suit.Length < 2) yield break;
        Renderer suit1 = mario_suit[0].GetComponent<Renderer>();
        Renderer suit2 = mario_suit[1].GetComponent<Renderer>();

        Time.timeScale = 0.1f;
        float pauseEndTime = Time.realtimeSinceStartup + 0.25f;

        while (Time.realtimeSinceStartup < pauseEndTime)
        {
            suit1.sharedMaterial = fire_material[0];
            suit2.sharedMaterial = fire_material[1];
            yield return new WaitForSecondsRealtime(0.04f);
            suit1.sharedMaterial = reg_material[0];
            suit2.sharedMaterial = reg_material[1];
            yield return new WaitForSecondsRealtime(0.04f);
        }

        suit1.sharedMaterial = fire_material[0];
        suit2.sharedMaterial = fire_material[1];
        Time.timeScale = 1f;
    }

    public IEnumerator Downgrade_FireSuit()
    {
        if (Mario_Effects != null && Mario_Effects.Length > 4 && Mario_Effects[4] != null)
        {
            Mario_Effects[4].Play();
        }
        FireMario = false;

        if (mario_suit == null || mario_suit.Length < 2) yield break;
        Renderer suit1 = mario_suit[0].GetComponent<Renderer>();
        Renderer suit2 = mario_suit[1].GetComponent<Renderer>();

        Time.timeScale = 0.1f;
        float pauseEndTime = Time.realtimeSinceStartup + 0.25f;

        while (Time.realtimeSinceStartup < pauseEndTime)
        {
            suit1.sharedMaterial = reg_material[0];
            suit2.sharedMaterial = reg_material[1];
            yield return new WaitForSecondsRealtime(0.04f);
            suit1.sharedMaterial = fire_material[0];
            suit2.sharedMaterial = fire_material[1];
            yield return new WaitForSecondsRealtime(0.04f);
        }

        suit1.sharedMaterial = reg_material[0];
        suit2.sharedMaterial = reg_material[1];
        Time.timeScale = 1f;
        StartCoroutine(FlickerEffect());
    }

    public IEnumerator FlickerEffect()
    {
        isInvincible = true;
        Physics.IgnoreLayerCollision(9, 13, true);
        Physics.IgnoreLayerCollision(9, 21, true);

        for (int i = 0; i < 12; i++)
        {
            SetMarioRenderersVisible(false);
            yield return new WaitForSeconds(0.06f);
            SetMarioRenderersVisible(true);
            yield return new WaitForSeconds(0.06f);
        }

        SetMarioRenderersVisible(true);
        Physics.IgnoreLayerCollision(9, 13, false);
        Physics.IgnoreLayerCollision(9, 21, false);
        isInvincible = false;
    }

    private void SetMarioRenderersVisible(bool visible)
    {
        if (entire_mario_body == null) return;
        foreach (GameObject part in entire_mario_body)
        {
            if (part != null)
            {
                Renderer r = part.GetComponent<Renderer>();
                if (r != null) r.enabled = visible;
            }
        }
    }

    void ThrowShell()
    {
        if (holdingShell && koopashell != null)
        {
            holdingShell = false;
            koopashellInvincible = 0f;

            KoopaShell shell = koopashell.GetComponent<KoopaShell>();
            if (shell != null)
            {
                shell.heldByPlayer = false;
                shell.moving = true;
                Vector3 throwDir = (transform.forward + Vector3.up * 0.15f).normalized;
                shell.velocity = throwDir * 25f;
            }

            Rigidbody shellRb = koopashell.GetComponent<Rigidbody>();
            if (shellRb != null) shellRb.isKinematic = false;

            SphereCollider shellCollider = koopashell.GetComponent<SphereCollider>();
            if (shellCollider != null) shellCollider.enabled = true;

            if (koopaShellHoldPos != null)
            {
                SphereCollider sc = koopaShellHoldPos.GetComponent<SphereCollider>();
                if (sc != null) sc.enabled = false;
            }

            if (player_anim != null && default_anim_controller != null)
            {
                player_anim.runtimeAnimatorController = default_anim_controller;
            }
        }
    }

    IEnumerator DestroyTree(GameObject tree, Vector3 direction)
    {
        Collider c = tree.GetComponent<Collider>();
        if (c != null) c.enabled = false;

        for (int i = 0; i < 15; i++)
        {
            tree.transform.Translate(direction.normalized * Time.deltaTime * 6f, Space.World);
            yield return null;
        }

        for (int i = 0; i < tree.transform.childCount; i++)
        {
            ParticleSystem ps = tree.transform.GetChild(i).GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();
            MeshRenderer mr = tree.transform.GetChild(i).GetComponent<MeshRenderer>();
            if (mr != null) mr.enabled = false;
        }
        Destroy(tree, 1f);
    }
}
