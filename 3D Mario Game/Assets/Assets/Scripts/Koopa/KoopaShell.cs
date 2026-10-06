using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KoopaShell : MonoBehaviour
{
    private Rigidbody rb;

    [HideInInspector]
    public Vector3 velocity = Vector3.zero;
    public bool moving = false;

    [HideInInspector]
    public bool heldByPlayer = false;
    [HideInInspector]
    public Transform holdPos;
    public AudioSource Hit_Sound;

    public ParticleSystem dustps;
    public ParticleSystem spark;
    private float particleDelay = 0f;

    public GameObject coin;
    private const float SHELL_SPEED = 24f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (moving && !heldByPlayer)
        {
            if (rb != null)
            {
                rb.isKinematic = false;
                SphereCollider sc = GetComponent<SphereCollider>();
                if (sc != null) sc.enabled = true;

                Vector3 currentVel = velocity.normalized * SHELL_SPEED;
                currentVel.y = rb.velocity.y;
                rb.velocity = currentVel;

                // Extra gravity for shell stability
                rb.AddForce(Vector3.down * 30f, ForceMode.Acceleration);

                if (particleDelay > 0.08f && dustps != null)
                {
                    dustps.Play();
                    particleDelay = 0f;
                }
            }
        }
        else if (heldByPlayer && holdPos != null)
        {
            transform.position = holdPos.position;
            transform.rotation = holdPos.rotation;
            if (rb != null) rb.isKinematic = true;
            SphereCollider sc = GetComponent<SphereCollider>();
            if (sc != null) sc.enabled = false;
        }

        particleDelay += Time.fixedDeltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!moving || heldByPlayer) return;

        ContactPoint contact = collision.contacts[0];

        // Wall or obstacle bounce
        if (!collision.gameObject.CompareTag("ground") && !collision.gameObject.CompareTag("Player") &&
            !collision.gameObject.CompareTag("Fireball") && !collision.gameObject.CompareTag("GoombaEnemy") &&
            contact.normal.y < 0.3f)
        {
            // Reflect velocity along collision normal
            Vector3 reflected = Vector3.Reflect(velocity.normalized, contact.normal);
            reflected.y = 0f;
            velocity = reflected.normalized * SHELL_SPEED;

            AudioSource aud = GetComponent<AudioSource>();
            if (aud != null) aud.Play();

            // Spawn bounce sparks without leaking memory
            if (spark != null)
            {
                ParticleSystem sparkClone = Instantiate(spark, contact.point, Quaternion.LookRotation(contact.normal));
                sparkClone.Play();
                Destroy(sparkClone.gameObject, 1.2f);
            }

            if (collision.gameObject.CompareTag("BrickBlock"))
            {
                AudioSource blockAud = collision.gameObject.GetComponent<AudioSource>();
                if (blockAud != null) blockAud.Play();

                for (int i = 0; i < collision.transform.childCount; i++)
                {
                    ParticleSystem ps = collision.transform.GetChild(i).GetComponent<ParticleSystem>();
                    if (ps != null) ps.Play();
                }
                MeshRenderer mr = collision.gameObject.GetComponent<MeshRenderer>();
                if (mr != null) mr.enabled = false;
                BoxCollider bc = collision.gameObject.GetComponent<BoxCollider>();
                if (bc != null) bc.enabled = false;
                Destroy(collision.gameObject, 1.5f);
            }

            if (collision.gameObject.CompareTag("Question") && contact.normal.y < 0.2f)
            {
                StartCoroutine(QuestionBlockHit(collision.gameObject));
            }
        }

        // Damage Player if moving
        if (collision.gameObject.CompareTag("Player") && moving)
        {
            Player player_script = collision.gameObject.GetComponent<Player>();
            if (player_script != null && !player_script.groundpound && player_script.koopashellInvincible > 0.5f)
            {
                if (Hit_Sound != null) Hit_Sound.Play();
                player_script.TakeDamage(1);
            }
        }

        // Hit Goomba
        if (collision.gameObject.CompareTag("GoombaEnemy") && moving)
        {
            GoombaChase goomba = collision.gameObject.GetComponent<GoombaChase>();
            if (goomba != null)
            {
                goomba.Stop();
                StartCoroutine(goomba.Dead());
            }
        }
    }

    public IEnumerator hop()
    {
        if (rb != null)
        {
            rb.AddForce(Vector3.up * 8f, ForceMode.Impulse);
        }
        yield return new WaitForSeconds(0.1f);
    }

    IEnumerator QuestionBlockHit(GameObject block)
    {
        AudioSource blockAud = block.transform.parent != null ? block.transform.parent.GetComponent<AudioSource>() : null;
        if (blockAud != null) blockAud.Play();

        Vector3 originalPos = block.transform.position;
        for (int i = 0; i < 3; i++)
        {
            block.transform.position += new Vector3(0, 0.2f, 0);
            yield return new WaitForSeconds(0.01f);
        }
        for (int i = 0; i < 3; i++)
        {
            block.transform.position -= new Vector3(0, 0.2f, 0);
            yield return new WaitForSeconds(0.01f);
        }
        block.transform.position = originalPos;

        BoxCollider bc = block.GetComponent<BoxCollider>();
        if (bc != null) bc.enabled = false;

        if (block.transform.childCount >= 3)
        {
            block.transform.GetChild(0).gameObject.SetActive(false);
            block.transform.GetChild(1).gameObject.SetActive(false);
            block.transform.GetChild(2).gameObject.SetActive(true);
        }

        QuestionBlockID qId = block.GetComponent<QuestionBlockID>();
        if (qId != null && qId.ITEM_ID == 0 && coin != null)
        {
            GameObject coinClone = Instantiate(coin, block.transform.position + Vector3.up * 0.5f, coin.transform.rotation);
            SphereCollider sc = coinClone.GetComponent<SphereCollider>();
            if (sc != null) sc.enabled = false;
            Rigidbody cRb = coinClone.GetComponent<Rigidbody>();
            if (cRb != null)
            {
                cRb.useGravity = true;
                cRb.AddForce(Vector3.up * 6f, ForceMode.Impulse);
            }
            yield return new WaitForSeconds(0.2f);
            if (coinClone != null)
            {
                if (cRb != null) cRb.useGravity = false;
                AudioSource aud = coinClone.GetComponent<AudioSource>();
                if (aud != null) aud.Play();
                ParticleSystem ps = coinClone.GetComponent<ParticleSystem>();
                if (ps != null) ps.Play();
                Destroy(coinClone, 0.8f);
            }
            CoinCollect.COIN_COUNT++;
        }
    }
}
