using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoombaChase : MonoBehaviour
{
    public enum GoombaState { Idle, Surprise, Chase, Dead }
    public GoombaState currentState = GoombaState.Idle;

    private GameObject player;
    private Player playerScript;
    private Animator goomba_anim;
    private Rigidbody rb;
    public AudioSource Vanish;

    private SkinnedMeshRenderer bodyRenderer;
    public Material idle_face;
    public Material mad_face;
    public Material dead_face;
    private GameObject body;

    private AudioSource Surprise;
    private AudioSource Running;
    private AudioSource Hit_Sound;
    private ParticleSystem ChasePS;

    public float speed = 300f;
    public float closeDistance = 1.2f;
    public GameObject DestroyPS;

    private float stateTimer = 0f;
    private bool dead = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerScript = player.GetComponent<Player>();

        goomba_anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        Surprise = GetComponent<AudioSource>();

        if (transform.childCount > 4) Running = transform.GetChild(4).GetComponent<AudioSource>();
        if (transform.childCount > 3)
        {
            body = transform.GetChild(3).gameObject;
            bodyRenderer = body.GetComponent<SkinnedMeshRenderer>();
        }
        if (transform.childCount > 6) ChasePS = transform.GetChild(6).GetComponent<ParticleSystem>();
        if (transform.childCount > 7) Hit_Sound = transform.GetChild(7).GetComponent<AudioSource>();

        if (DestroyPS == null)
        {
            DestroyPS = GameObject.FindGameObjectWithTag("DestroyParticleSystem");
        }
    }

    void Update()
    {
        if (dead || player == null) return;

        float dist = Vector3.Distance(transform.position, player.transform.position);

        switch (currentState)
        {
            case GoombaState.Idle:
                if (dist < 15f)
                {
                    currentState = GoombaState.Surprise;
                    stateTimer = 0.5f;

                    if (goomba_anim != null)
                    {
                        goomba_anim.SetBool("Surprise", true);
                        goomba_anim.SetBool("Idle", false);
                    }
                    if (Surprise != null) Surprise.Play();
                }
                break;

            case GoombaState.Surprise:
                stateTimer -= Time.deltaTime;
                Vector3 lookDir = player.transform.position - transform.position;
                lookDir.y = 0f;
                if (lookDir != Vector3.zero)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), 10f * Time.deltaTime);
                }

                if (stateTimer <= 0f)
                {
                    currentState = GoombaState.Chase;
                    if (goomba_anim != null)
                    {
                        goomba_anim.SetBool("Surprise", false);
                        goomba_anim.SetBool("Chase", true);
                    }
                    if (bodyRenderer != null && mad_face != null)
                    {
                        bodyRenderer.sharedMaterial = mad_face;
                    }
                    if (Running != null && !Running.isPlaying) Running.Play();
                    if (ChasePS != null) ChasePS.Play();
                }
                break;

            case GoombaState.Chase:
                if (dist > 18f)
                {
                    // Player escaped, return to idle
                    currentState = GoombaState.Idle;
                    if (goomba_anim != null)
                    {
                        goomba_anim.SetBool("Chase", false);
                        goomba_anim.SetBool("Idle", true);
                    }
                    if (bodyRenderer != null && idle_face != null)
                    {
                        bodyRenderer.sharedMaterial = idle_face;
                    }
                    if (Running != null) Running.Stop();
                    if (ChasePS != null) ChasePS.Stop();
                    if (rb != null) rb.velocity = new Vector3(0, rb.velocity.y, 0);
                }
                else
                {
                    Vector3 toPlayer = player.transform.position - transform.position;
                    toPlayer.y = 0f;
                    if (toPlayer != Vector3.zero)
                    {
                        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(toPlayer), 8f * Time.deltaTime);
                    }

                    float moveSpeed = dist <= closeDistance ? 1.5f : 3.8f;
                    Vector3 moveVel = transform.forward * moveSpeed;
                    if (rb != null)
                    {
                        rb.velocity = new Vector3(moveVel.x, rb.velocity.y, moveVel.z);
                    }
                }
                break;
        }
    }

    public IEnumerator Dead()
    {
        if (dead) yield break;
        dead = true;
        currentState = GoombaState.Dead;

        CapsuleCollider cc = GetComponent<CapsuleCollider>();
        if (cc != null) cc.enabled = false;

        if (transform.childCount > 3)
        {
            AudioSource stompAud = transform.GetChild(3).GetComponent<AudioSource>();
            if (stompAud != null) stompAud.Play();
        }

        if (transform.childCount > 8)
        {
            transform.GetChild(8).gameObject.SetActive(false);
        }

        if (ChasePS != null) ChasePS.Stop();
        if (Running != null) Running.Stop();

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.isKinematic = true;
        }

        if (bodyRenderer != null && dead_face != null)
        {
            bodyRenderer.sharedMaterial = dead_face;
        }

        if (goomba_anim != null)
        {
            goomba_anim.SetBool("Dead", true);
        }

        yield return new WaitForSeconds(0.8f);

        if (DestroyPS != null)
        {
            Instantiate(DestroyPS, transform.position + Vector3.up * 0.5f, transform.rotation);
        }
        if (Vanish != null) Vanish.Play();

        Destroy(gameObject);
    }

    public void Stop()
    {
        dead = true;
        currentState = GoombaState.Dead;
        if (bodyRenderer != null && dead_face != null) bodyRenderer.sharedMaterial = dead_face;
        if (Running != null) Running.Stop();
        if (ChasePS != null) ChasePS.Stop();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.isKinematic = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (dead) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            Player p = collision.gameObject.GetComponent<Player>();
            Rigidbody pRb = collision.gameObject.GetComponent<Rigidbody>();

            // If player landed on Goomba or is ground pounding, Goomba dies
            if (p != null && (p.groundpound || (pRb != null && pRb.velocity.y < -0.5f)))
            {
                StartCoroutine(Dead());
            }
            else if (p != null)
            {
                // Goomba damaged player from side
                if (Hit_Sound != null) Hit_Sound.Play();
                p.TakeDamage(1);
            }
        }
    }
}
