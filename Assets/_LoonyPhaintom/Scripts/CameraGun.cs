using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class CameraGun : MonoBehaviour
{
    [SerializeField] public Bullet prefab; // Specify your prefab with a Rigidbody here
    [SerializeField] private float shotSpeed = 100f;

    private List<Bullet> PrefabPool { get; } = new List<Bullet>();


    private Bullet CreatePrefab()
    {
        var instance = Instantiate(prefab);
        PrefabPool.Add(instance);
        return instance;
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out var hit)) return;
        var instance = GetPrefabFromPool();
        if (instance == null) instance = CreatePrefab();


        var emitPoint = transform.position + new Vector3(0f, -1f, 0f);
        instance.transform.position = emitPoint; // Start the prefab at the current transform position
        instance.SetActive(true); // Ensure the prefab is active

        var direction = hit.point - emitPoint; // Calculate the direction to the click
        instance.Velocity = direction.normalized * shotSpeed; // Set velocity towards clicked position
    }

    // Method for retrieving a prefab from the pool
    private Bullet GetPrefabFromPool()
    {
        return PrefabPool.FirstOrDefault(b => !b.gameObject.activeInHierarchy);

        // If no inactive prefabs are found and the pool is full, return null
    }
}