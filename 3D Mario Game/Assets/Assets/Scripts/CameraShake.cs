using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private Vector3 originalLocalPos;
    private Coroutine shakeCoroutine;

    void Awake()
    {
        originalLocalPos = transform.localPosition;
    }

    public void TriggerShake(float duration = 0.2f, float magnitude = 0.15f)
    {
        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
        shakeCoroutine = StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float percent = 1f - (elapsed / duration);
            float currentMag = magnitude * percent;

            float x = Random.Range(-1f, 1f) * currentMag;
            float y = Random.Range(-1f, 1f) * currentMag;

            transform.localPosition = originalLocalPos + new Vector3(x, y, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalLocalPos;
        shakeCoroutine = null;
    }
}
