using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreenStarID : MonoBehaviour
{
    public int ID;
    public ParticleSystem my_material;
    public Color my_colour = Color.green;

    [Header("Visual Polish")]
    public float rotationSpeed = 90f;
    public float floatAmplitude = 0.25f;
    public float floatFrequency = 2f;

    private Vector3 initialPosition;

    void Start()
    {
        initialPosition = transform.position;

        if (my_material != null)
        {
            ParticleSystem.MainModule colour = my_material.main;
            colour.startColor = my_colour;
        }
    }

    void Update()
    {
        // Smooth rotation
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

        // Smooth floating bob
        float newY = initialPosition.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}
