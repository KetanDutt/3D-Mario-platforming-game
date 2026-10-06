using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crate : MonoBehaviour
{
    private bool destroyed = false;

    public IEnumerator Destroy_Punch()
    {
        if (destroyed) yield break;
        destroyed = true;

        BoxCollider bc = GetComponent<BoxCollider>();
        if (bc != null) bc.enabled = false;

        AudioSource aud = GetComponent<AudioSource>();
        if (aud != null) aud.Play();

        SkinnedMeshRenderer smr = GetComponent<SkinnedMeshRenderer>();
        if (smr != null) smr.enabled = false;
        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr != null) mr.enabled = false;

        // Play burst particles
        for (int i = 0; i < transform.childCount; i++)
        {
            ParticleSystem ps = transform.GetChild(i).GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();
        }

        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }

    public IEnumerator Destroy_GroundPound()
    {
        if (destroyed) yield break;
        destroyed = true;

        BoxCollider bc = GetComponent<BoxCollider>();
        if (bc != null) bc.enabled = false;

        AudioSource aud = GetComponent<AudioSource>();
        if (aud != null) aud.Play();

        SkinnedMeshRenderer smr = GetComponent<SkinnedMeshRenderer>();
        if (smr != null) smr.enabled = false;
        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr != null) mr.enabled = false;

        for (int i = 0; i < transform.childCount; i++)
        {
            ParticleSystem ps = transform.GetChild(i).GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();
        }

        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }

    public IEnumerator MegaMushroom()
    {
        return Destroy_GroundPound();
    }
}
