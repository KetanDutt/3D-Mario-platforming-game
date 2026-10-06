using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class sceneManage : MonoBehaviour
{
    public GameObject TransitionUI;

    public IEnumerator levelToWorldMap()
    {
        if (TransitionUI != null)
        {
            Animator anim = TransitionUI.GetComponent<Animator>();
            if (anim != null) anim.SetBool("TransitionOut", true);
        }
        yield return new WaitForSeconds(3.5f);
        SceneManager.LoadScene("World");
    }

    public void level1()
    {
        if (Application.CanStreamedLevelBeLoaded("Level1"))
        {
            SceneManager.LoadScene("Level1");
        }
        else
        {
            Debug.LogWarning("[sceneManage] Level1 scene not found in Build Settings! Loading Level2-Cave as fallback.");
            SceneManager.LoadScene("Level2-Cave");
        }
    }

    public void level2()
    {
        if (Application.CanStreamedLevelBeLoaded("Level2-Cave"))
        {
            SceneManager.LoadScene("Level2-Cave");
        }
        else
        {
            Debug.LogError("[sceneManage] Level2-Cave scene not found!");
        }
    }

    public void RestartCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
