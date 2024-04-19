using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereMove : MonoBehaviour
{
    [SerializeField] private Transform sphere;
    [SerializeField] private LayerMask mask;

    void Update()
    {
        if (!Input.GetMouseButton(0)) return;
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit, 100f,mask))
        {
            sphere.position = hit.point;
        }
    }
}