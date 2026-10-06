using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarioWalkSounds : MonoBehaviour
{
    public Player player;
    public CameraFollow cam_follow;
    public AudioSource StompSound;
    public AudioSource BrickWalkSound;

    void Start()
    {
        if (player == null) player = GetComponentInParent<Player>();
        if (cam_follow == null && Camera.main != null && Camera.main.transform.parent != null)
        {
            cam_follow = Camera.main.transform.parent.GetComponent<CameraFollow>();
        }
    }

    void Play_Stomp_Sound()
    {
        if (player == null) return;

        if (player.MEGAMUSHROOM && StompSound != null)
        {
            StompSound.Play();
        }
        else if (cam_follow != null && cam_follow.Underground && !player.MEGAMUSHROOM && BrickWalkSound != null)
        {
            BrickWalkSound.Play();
        }
    }
}
