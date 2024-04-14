using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrushManager : MonoBehaviour
{
    private static BrushManager _instance;

    public static BrushManager Instance
    {
        get
        {
            if (!_instance)
            {
                _instance = FindObjectOfType<BrushManager>(true);
            }

            return _instance;
        }
    }

    [SerializeField] private List<Camera> cameras = new List<Camera>();

    private List<GameObject> Brushes { get; } = new List<GameObject>();

    private void Awake()
    {
        foreach (var cam in cameras)
        {
            cam.enabled = false;
        }
    }

    private void Update()
    {
        foreach (var cam in cameras)
        {
            cam.Render();
        }

        foreach (var brush in Brushes)
        {
            brush.SetActive(false);
        }
    }

    public void Add(GameObject brushObject)
    {
        Brushes.Add(brushObject);
    }
}