using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudPlatform : MonoBehaviour
{
    public int direction = 1;
    public float speed = 5f;
    public float lifeTime = 16f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(Vector3.up * Time.deltaTime * speed * direction, Space.World);
    }
}
