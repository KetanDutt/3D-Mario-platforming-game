using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkDustManager : MonoBehaviour
{
    public ParticleSystem Walkdust;
    public Player player;

    void Start()
    {
        if (player == null) player = GetComponentInParent<Player>();
        if (Walkdust == null) Walkdust = GetComponent<ParticleSystem>();
    }

    void Update()
    {
        if (player == null || Walkdust == null) return;

        bool isMovingOnGround = player.grounded && !player.PipeEntry && !player.groundpound &&
            (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f || Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f);

        if (isMovingOnGround)
        {
            if (!Walkdust.isPlaying)
            {
                Walkdust.Play();
            }
        }
        else
        {
            if (Walkdust.isPlaying)
            {
                Walkdust.Stop();
            }
        }
    }
}
