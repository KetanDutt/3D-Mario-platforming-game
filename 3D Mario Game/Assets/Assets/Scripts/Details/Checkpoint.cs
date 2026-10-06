using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public Transform flagTransform;
    public ParticleSystem activateParticles;
    public AudioSource checkpointAudio;
    private bool activated = false;

    void OnTriggerEnter(Collider other)
    {
        if (activated) return;

        if (other.CompareTag("Player"))
        {
            activated = true;
            Player p = other.GetComponent<Player>();
            if (p != null)
            {
                p.SetCheckpoint(transform.position);
            }

            if (checkpointAudio != null) checkpointAudio.Play();
            if (activateParticles != null) activateParticles.Play();

            StartCoroutine(RaiseFlag());
        }
    }

    IEnumerator RaiseFlag()
    {
        if (flagTransform == null) yield break;

        Vector3 startPos = flagTransform.localPosition;
        Vector3 targetPos = startPos + Vector3.up * 2f;
        float elapsed = 0f;
        float duration = 0.8f;

        while (elapsed < duration)
        {
            flagTransform.localPosition = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        flagTransform.localPosition = targetPos;
    }
}
