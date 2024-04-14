using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Bullet : MonoBehaviour
{
    [SerializeField] private GameObject renderer;
    [SerializeField] private GameObject brushObject;
    [SerializeField] private Rigidbody rb;


    public Vector3 Velocity
    {
        get => rb.velocity;
        set => rb.velocity = value;
    }


    private void Awake()
    {
        BrushManager.Instance.Add(brushObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        brushObject.transform.parent = collision.transform;
        brushObject.transform.position = collision.contacts[0].point;
        brushObject.SetActive(true);
        
        gameObject.SetActive(false);
    }



    public void SetActive(bool flag)
    {
        var scale =
            new Vector3(Random.Range(0.9f, 1.1f), Random.Range(0.9f, 1.1f), Random.Range(0.9f, 1.1f))*2f;

        renderer.transform.localScale = scale;
        brushObject.transform.localScale = scale;

        renderer.SetActive(flag);
        gameObject.SetActive(flag);
        brushObject.SetActive(false);
    }
}