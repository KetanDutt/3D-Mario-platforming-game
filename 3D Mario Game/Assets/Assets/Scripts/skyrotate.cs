using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class skyrotate : MonoBehaviour
{
    public float rotatespeed = 1.0f;

    void Update()
    {
        if (RenderSettings.skybox != null)
        {
            RenderSettings.skybox.SetFloat("_Rotation", Time.time * rotatespeed);
        }
    }
}
