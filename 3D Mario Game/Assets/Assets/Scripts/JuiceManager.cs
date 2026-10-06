using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JuiceManager : MonoBehaviour
{
    public static JuiceManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Smooth squash and stretch tween for jumps, landings, and stomps.
    /// </summary>
    public static void SquashAndStretch(Transform target, Vector3 stretchScale, Vector3 normalScale, float duration)
    {
        if (Instance != null && target != null)
        {
            Instance.StartCoroutine(Instance.DoSquashAndStretch(target, stretchScale, normalScale, duration));
        }
    }

    private IEnumerator DoSquashAndStretch(Transform target, Vector3 stretchScale, Vector3 normalScale, float duration)
    {
        float elapsed = 0f;
        float halfDuration = duration * 0.5f;

        while (elapsed < halfDuration)
        {
            if (target == null) yield break;
            target.localScale = Vector3.Lerp(normalScale, stretchScale, elapsed / halfDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            if (target == null) yield break;
            target.localScale = Vector3.Lerp(stretchScale, normalScale, elapsed / halfDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (target != null) target.localScale = normalScale;
    }

    /// <summary>
    /// Hit-stop / micro-pause for crunchy impact feedback when stomping or smashing blocks.
    /// </summary>
    public static void HitStop(float duration = 0.05f)
    {
        if (Instance != null)
        {
            Instance.StartCoroutine(Instance.DoHitStop(duration));
        }
    }

    private IEnumerator DoHitStop(float duration)
    {
        float originalScale = Time.timeScale;
        Time.timeScale = 0.05f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = originalScale;
    }

    /// <summary>
    /// Smooth spring block bounce tween.
    /// </summary>
    public static void SpringBounce(Transform target, Vector3 direction, float distance, float duration)
    {
        if (Instance != null && target != null)
        {
            Instance.StartCoroutine(Instance.DoSpringBounce(target, direction, distance, duration));
        }
    }

    private IEnumerator DoSpringBounce(Transform target, Vector3 direction, float distance, float duration)
    {
        Vector3 origin = target.position;
        Vector3 peak = origin + direction.normalized * distance;
        float elapsed = 0f;
        float half = duration * 0.5f;

        while (elapsed < half)
        {
            if (target == null) yield break;
            target.position = Vector3.Lerp(origin, peak, Mathf.Sin((elapsed / half) * Mathf.PI * 0.5f));
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < half)
        {
            if (target == null) yield break;
            target.position = Vector3.Lerp(peak, origin, Mathf.Sin((elapsed / half) * Mathf.PI * 0.5f));
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (target != null) target.position = origin;
    }
}
