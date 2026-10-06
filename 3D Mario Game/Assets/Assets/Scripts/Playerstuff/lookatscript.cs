using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lookatscript : MonoBehaviour
{
    public Vector3 offset;
    public Transform player;
    public float smoothness_speed = 0.15f;

    private Vector3 mypos;
    private Vector3 smoothVelocity;

    void Start()
    {
        if (player == null)
        {
            GameObject pObj = GameObject.FindGameObjectWithTag("Player");
            if (pObj != null) player = pObj.transform;
        }

        if (player != null)
        {
            offset = player.position - transform.position;
        }
    }

    void FixedUpdate()
    {
        if (player == null) return;

        mypos = player.position - offset;
        transform.position = Vector3.SmoothDamp(transform.position, mypos, ref smoothVelocity, smoothness_speed);
    }
}
