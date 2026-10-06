using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudPlatformSpawner : MonoBehaviour
{
    public GameObject CloudPlatform;
    public int direction = 1;
    public float spawnInterval = 2.0f;

    void Start()
    {
        InvokeRepeating(nameof(SpawnCloud), 0.5f, spawnInterval);
    }

    void SpawnCloud()
    {
        if (CloudPlatform != null)
        {
            GameObject clone = Instantiate(CloudPlatform, transform.position, CloudPlatform.transform.rotation);
            CloudPlatform cp = clone.GetComponent<CloudPlatform>();
            if (cp != null)
            {
                cp.direction = direction;
            }
        }
    }
}
