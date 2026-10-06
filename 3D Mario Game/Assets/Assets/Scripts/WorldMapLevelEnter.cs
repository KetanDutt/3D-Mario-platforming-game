using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMapLevelEnter : MonoBehaviour
{
    public GameObject transitionUI;

    [Header("LEVEL 1 STUFF")]
    public BoxCollider LevelWall;
    private GameObject Player;
    public GameObject level1;
    public Transform level1Camera;
    public GameObject Level1Particles;
    public GameObject Level1Flag;

    public static bool Level1Complete = false;
    public static bool Level2Complete = false;

    private sceneManage scene_manage;

    [Header("LEVEL 2 STUFF")]
    public GameObject level2;
    public GameObject Level2Particles;
    public Transform Level2Camera;
    public BoxCollider Level2Wall;
    public GameObject Level2Flag;

    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        GameObject smObj = GameObject.FindGameObjectWithTag("SceneManager");
        if (smObj != null)
        {
            scene_manage = smObj.GetComponent<sceneManage>();
        }
    }

    void Update()
    {
        if (Level1Flag != null) Level1Flag.SetActive(Level1Complete);
        if (Level2Flag != null) Level2Flag.SetActive(Level2Complete);
    }

    public IEnumerator EnterLevel(GameObject Particles)
    {
        if (LevelWall != null) LevelWall.enabled = false;
        yield return new WaitForSeconds(0.6f);

        if (Particles != null)
        {
            for (int i = 0; i < Particles.transform.childCount; i++)
            {
                ParticleSystem ps = Particles.transform.GetChild(i).GetComponent<ParticleSystem>();
                if (ps != null) ps.Play();
            }
        }

        AudioSource aud = GetComponent<AudioSource>();
        if (aud != null) aud.Play();

        if (Camera.main != null)
        {
            Animator camAnim = Camera.main.GetComponent<Animator>();
            if (camAnim != null)
            {
                camAnim.SetTrigger("Shake");
                yield return new WaitForSeconds(0.1f);
                camAnim.ResetTrigger("Shake");
            }
        }

        yield return new WaitForSeconds(1.5f);

        if (transitionUI != null)
        {
            Animator transAnim = transitionUI.GetComponent<Animator>();
            if (transAnim != null) transAnim.SetBool("TransitionOut", true);
        }

        // Smooth audio fade out (Fixed negative volume bug)
        AudioSource mapBgm = (Camera.main != null && Camera.main.transform.parent != null) ?
            Camera.main.transform.parent.GetComponent<AudioSource>() : null;

        if (mapBgm != null)
        {
            while (mapBgm.volume > 0.05f)
            {
                mapBgm.volume = Mathf.Max(0f, mapBgm.volume - 0.05f);
                yield return new WaitForSeconds(0.04f);
            }
            mapBgm.Stop();
        }

        yield return new WaitForSeconds(1f);
        if (scene_manage != null) scene_manage.level1();
    }

    public IEnumerator EnterLevel2(GameObject Particles)
    {
        if (Level2Wall != null) Level2Wall.enabled = false;
        yield return new WaitForSeconds(0.6f);

        if (Particles != null)
        {
            for (int i = 0; i < Particles.transform.childCount; i++)
            {
                ParticleSystem ps = Particles.transform.GetChild(i).GetComponent<ParticleSystem>();
                if (ps != null) ps.Play();
            }
        }

        AudioSource aud = GetComponent<AudioSource>();
        if (aud != null) aud.Play();

        if (Camera.main != null)
        {
            Animator camAnim = Camera.main.GetComponent<Animator>();
            if (camAnim != null)
            {
                camAnim.SetTrigger("Shake");
                yield return new WaitForSeconds(0.1f);
                camAnim.ResetTrigger("Shake");
            }
        }

        yield return new WaitForSeconds(1.5f);

        if (transitionUI != null)
        {
            Animator transAnim = transitionUI.GetComponent<Animator>();
            if (transAnim != null) transAnim.SetBool("TransitionOut", true);
        }

        AudioSource mapBgm = (Camera.main != null && Camera.main.transform.parent != null) ?
            Camera.main.transform.parent.GetComponent<AudioSource>() : null;

        if (mapBgm != null)
        {
            while (mapBgm.volume > 0.05f)
            {
                mapBgm.volume = Mathf.Max(0f, mapBgm.volume - 0.05f);
                yield return new WaitForSeconds(0.04f);
            }
            mapBgm.Stop();
        }

        yield return new WaitForSeconds(1f);
        if (scene_manage != null) scene_manage.level2();
    }
}
