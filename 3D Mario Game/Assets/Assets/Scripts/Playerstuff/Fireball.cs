using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : MonoBehaviour
{
    public Rigidbody rb;
    public Vector3 velocity = new Vector3(0, -16, 19);

    private bool isDestroyed = false;
    private int bounceCount = 0;
    private const int MAX_BOUNCES = 5;

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            velocity = rb.velocity;
        }
        StartCoroutine(AutoDestroyTimer(5f));
    }

    void FixedUpdate()
    {
        if (isDestroyed || rb == null) return;

        // Custom gravity simulation for Mario fireball bounces
        rb.AddForce(Vector3.down * 35f, ForceMode.Acceleration);
    }

    void OnCollisionEnter(Collision col)
    {
        if (isDestroyed) return;

        // Hit Goomba
        if (col.gameObject.CompareTag("GoombaEnemy"))
        {
            GoombaChase goomba = col.gameObject.GetComponent<GoombaChase>();
            if (goomba != null)
            {
                goomba.Stop();
                StartCoroutine(goomba.Dead());
            }
            StartCoroutine(Destroy());
            return;
        }

        // Hit Mega Goomba
        if (col.gameObject.CompareTag("MegaGoomba"))
        {
            MegaGoomba mega = col.gameObject.GetComponent<MegaGoomba>();
            if (mega != null)
            {
                mega.health--;
                mega.Play_Stomp_Sound();
                if (mega.health <= 0)
                {
                    mega.Stop();
                    StartCoroutine(mega.Dead());
                }
            }
            StartCoroutine(Destroy());
            return;
        }

        // Hit Crate
        if (col.gameObject.CompareTag("Crate"))
        {
            Crate crate = col.gameObject.GetComponent<Crate>();
            if (crate != null) StartCoroutine(crate.Destroy_GroundPound());
            StartCoroutine(Destroy());
            return;
        }

        // Hit Koopa Shell
        if (col.gameObject.CompareTag("KoopaShell"))
        {
            KoopaShell shell = col.gameObject.GetComponent<KoopaShell>();
            if (shell != null)
            {
                shell.velocity = Vector3.zero;
                shell.moving = false;
                StartCoroutine(shell.hop());
            }
            StartCoroutine(Destroy());
            return;
        }

        // Bounce on floor / walls
        if (col.contacts.Length > 0)
        {
            ContactPoint cp = col.contacts[0];
            bounceCount++;

            if (bounceCount > MAX_BOUNCES)
            {
                StartCoroutine(Destroy());
                return;
            }

            if (cp.normal.y > 0.5f)
            {
                // Floor bounce
                if (rb != null)
                {
                    rb.velocity = new Vector3(rb.velocity.x, 8f, rb.velocity.z);
                }
            }
            else
            {
                // Wall bounce or destroy
                StartCoroutine(Destroy());
            }
        }
    }

    IEnumerator AutoDestroyTimer(float time)
    {
        yield return new WaitForSeconds(time);
        if (!isDestroyed) StartCoroutine(Destroy());
    }

    public IEnumerator Destroy()
    {
        if (isDestroyed) yield break;
        isDestroyed = true;

        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr != null) mr.enabled = false;
        SphereCollider sc = GetComponent<SphereCollider>();
        if (sc != null) sc.enabled = false;

        ParticleSystem ps = GetComponent<ParticleSystem>();
        if (ps != null) ps.Stop();

        if (transform.childCount > 0)
        {
            GameObject dissolve = transform.GetChild(0).gameObject;
            GameObject clone = Instantiate(dissolve, transform.position, dissolve.transform.rotation);
            ParticleSystem dPs = clone.GetComponent<ParticleSystem>();
            if (dPs != null) dPs.Play();
            Destroy(clone, 1.5f);
        }

        yield return new WaitForSeconds(0.2f);
        Destroy(gameObject);
    }
}
