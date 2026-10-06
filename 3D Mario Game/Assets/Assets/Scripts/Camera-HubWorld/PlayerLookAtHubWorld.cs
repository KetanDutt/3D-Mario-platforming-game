using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLookAtHubWorld : MonoBehaviour
{
    public Vector3 offset;
    private Transform player;
    private Vector3 mypos;
    private float smoothYVelocity;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player == null) return;
        mypos = player.position + offset;

        float targetY = Mathf.SmoothDamp(transform.position.y, mypos.y, ref smoothYVelocity, 0.15f);
        transform.position = new Vector3(mypos.x, targetY, mypos.z);
    }
}
