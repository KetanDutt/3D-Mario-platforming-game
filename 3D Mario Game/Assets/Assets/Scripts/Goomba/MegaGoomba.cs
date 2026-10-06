using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MegaGoomba : MonoBehaviour
{
    public enum MegaState { Idle, Surprise, Chase, Dead }
    public MegaState currentState = MegaState.Idle;

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

    public GameObject MegaMushroom;
    private AudioSource Surprise;
    private AudioSource Hit_Sound;
    private ParticleSystem ChasePS;

    public float speed = 300f;
    public int health = 5;
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
            case MegaState.Idle:
                if (dist < 20f)
                {
                    currentState = MegaState.Surprise;
                    stateTimer = 0.8f;
                    if (goomba_anim != null)
                    {
                        goomba_anim.SetBool("Surprise", true);
                        goomba_anim.SetBool("Idle", false);
                    }
                    if (Surprise != null) Surprise.Play();
                }
                break;

            case MegaState.Surprise:
                stateTimer -= Time.deltaTime;
                Vector3 toPlayer = player.transform.position - transform.position;
                toPlayer.y = 0f;
                if (toPlayer != Vector3.zero)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(toPlayer), 6f * Time.deltaTime);
                }

                if (stateTimer <= 0f)
                {
                    currentState = MegaState.Chase;
                    if (goomba_anim != null)
                    {
                        goomba_anim.SetBool("Surprise", false);
                        goomba_anim.SetBool("Chase", true);
                    }
                    if (bodyRenderer != null && mad_face != null)
                    {
                        bodyRenderer.sharedMaterial = mad_face;
                    }
                    if (ChasePS != null) ChasePS.Play();
                }
                break;

            case MegaState.Chase:
                if (dist > 25f)
                {
                    currentState = MegaState.Idle;
                    if (goomba_anim != null)
                    {
                        goomba_anim.SetBool("Chase", false);
                        goomba_anim.SetBool("Idle", true);
                    }
                    if (bodyRenderer != null && idle_face != null)
                    {
                        bodyRenderer.sharedMaterial = idle_face;
                    }
                    if (ChasePS != null) ChasePS.Stop();
                    if (rb != null) rb.velocity = new Vector3(0, rb.velocity.y, 0);
                }
                else
                {
                    Vector3 moveDir = player.transform.position - transform.position;
                    moveDir.y = 0f;
                    if (moveDir != Vector3.zero)
                    {
                        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDir), 4f * Time.deltaTime);
                    }

                    float moveSpeed = dist <= 2f ? 1.5f : 4f;
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
        currentState = MegaState.Dead;

        CapsuleCollider cc = GetComponent<CapsuleCollider>();
        if (cc != null) cc.enabled = false;

        if (transform.childCount > 8) transform.GetChild(8).gameObject.SetActive(false);
        if (ChasePS != null) ChasePS.Stop();

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
            GameObject deathEffect = Instantiate(DestroyPS, transform.position + Vector3.up * 1.5f, transform.rotation);
            deathEffect.transform.localScale = Vector3.one * 3f;
        }

        if (MegaMushroom != null)
        {
            Instantiate(MegaMushroom, transform.position + Vector3.up * 1f, Quaternion.identity);
        }

        if (Vanish != null) Vanish.Play();
        Destroy(gameObject);
    }

    public void Stop()
    {
        dead = true;
        currentState = MegaState.Dead;
        if (bodyRenderer != null && dead_face != null) bodyRenderer.sharedMaterial = dead_face;
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

            if (p != null && (p.groundpound || (pRb != null && pRb.velocity.y < -0.5f)))
            {
                health--;
                Play_Stomp_Sound();

                if (health <= 0)
                {
                    StartCoroutine(Dead());
                }
            }
            else if (p != null)
            {
                if (Hit_Sound != null) Hit_Sound.Play();
                p.TakeDamage(1);
            }
        }
    }

    public void Play_Stomp_Sound()
    {
        if (transform.childCount > 4)
        {
            AudioSource stompAud = transform.GetChild(4).GetComponent<AudioSource>();
            if (stompAud != null) stompAud.Play();
        }
    }
}
