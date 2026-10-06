using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveLookAt : MonoBehaviour
{
    public Vector3 offset;
    public Transform player;
    private Vector3 mypos;

    public float smoothness_speed_x = 0.15f;
    public float smoothness_speed_y = 0.1f;
    public float smoothness_speed_z = 0.15f;

    private bool entered_cave = false;
    private bool follow = true;

    public GameObject surfaceLight;
    public GameObject lanterns;

    private float velX, velY, velZ;

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
        if (player == null || !follow) return;

        mypos = player.position - offset;

        float posX = Mathf.SmoothDamp(transform.position.x, mypos.x, ref velX, smoothness_speed_x);
        float posY = Mathf.SmoothDamp(transform.position.y, mypos.y, ref velY, smoothness_speed_y);
        float posZ = Mathf.SmoothDamp(transform.position.z, mypos.z, ref velZ, smoothness_speed_z);

        transform.position = new Vector3(posX, posY, posZ);
    }

    private IEnumerator OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "CameraRotateCollider0")
        {
            transform.eulerAngles = new Vector3(0, 180, 0);
            yield return new WaitForSeconds(0.3f);
            if (!entered_cave) entered_cave = true;
        }
        else if (other.gameObject.name == "CameraRotateCollider1")
        {
            transform.eulerAngles = new Vector3(25, 90, 0);
            transform.position = new Vector3(288.5f, transform.position.y, transform.position.z);
            offset = new Vector3(offset.x, offset.y, 0);
            if (lanterns != null) lanterns.SetActive(true);
        }
        else if (other.gameObject.name == "CameraRotateCollider2")
        {
            if (Camera.main != null && Camera.main.transform.parent != null)
            {
                CameraFollow cf = Camera.main.transform.parent.GetComponent<CameraFollow>();
                if (cf != null) { cf.cloudPlatforms = true; cf.restOfCaveLevel = false; }
            }
        }
        else if (other.gameObject.name == "CameraRotateCollider3")
        {
            if (Camera.main != null && Camera.main.transform.parent != null)
            {
                CameraFollow cf = Camera.main.transform.parent.GetComponent<CameraFollow>();
                if (cf != null) { cf.startLvel = true; cf.restOfCaveLevel = false; }
            }
        }
        else if (other.gameObject.name == "CameraRotateCollider4")
        {
            if (Camera.main != null && Camera.main.transform.parent != null)
            {
                CameraFollow cf = Camera.main.transform.parent.GetComponent<CameraFollow>();
                if (cf != null) { cf.endOfCaveLevel = true; cf.restOfCaveLevel = false; }
            }
            offset = new Vector3(offset.x, offset.y, -3);
        }
        else if (other.gameObject.name == "CameraRotateCollider5")
        {
            transform.eulerAngles = new Vector3(0, 180, 0);
            offset = new Vector3(offset.x, -5, offset.z);
            if (lanterns != null) lanterns.SetActive(false);
            yield return null;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "CameraRotateCollider2")
        {
            if (Camera.main != null && Camera.main.transform.parent != null)
            {
                CameraFollow cf = Camera.main.transform.parent.GetComponent<CameraFollow>();
                if (cf != null) { cf.cloudPlatforms = false; cf.restOfCaveLevel = true; }
            }
        }
        else if (other.gameObject.name == "CameraRotateCollider3")
        {
            if (Camera.main != null && Camera.main.transform.parent != null)
            {
                CameraFollow cf = Camera.main.transform.parent.GetComponent<CameraFollow>();
                if (cf != null) { cf.startLvel = false; cf.restOfCaveLevel = true; }
            }
        }
    }
}
