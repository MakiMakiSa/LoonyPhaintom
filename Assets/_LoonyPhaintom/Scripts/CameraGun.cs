using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class CameraGun : MonoBehaviour
{
    [SerializeField] public Bullet prefab; // Specify your prefab with a Rigidbody here
    [SerializeField] private float shotSpeed = 100f;
    [SerializeField] private int shotgunCount = 5; // the number of bullets to be scattered
    [SerializeField] private float scatterAngle = 15.0f; // scatter angle for "shotgun" effect

    private List<Bullet> PrefabPool { get; } = new List<Bullet>();


    private Bullet CreatePrefab()
    {
        var instance = Instantiate(prefab);
        PrefabPool.Add(instance);
        return instance;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left Click
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out var hit))
            {
                var instance = GetPrefabFromPool();
                if (instance == null) instance = CreatePrefab();

                var emitPoint = transform.position + new Vector3(0f, -1f, 0f);
                instance.transform.position = emitPoint; 
                instance.SetActive(true); 

                var direction = hit.point - emitPoint; 
                instance.Velocity = direction.normalized * shotSpeed;
            }
        }
        else if (Input.GetMouseButtonDown(1)) // Right Click
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out var hit))
            {
                var emitPoint = transform.position + new Vector3(0f, -1f, 0f);
                var direction = hit.point - emitPoint;
                for (int i = 0; i < shotgunCount; i++)
                {
                    var instance = GetPrefabFromPool();
                    if (instance == null) instance = CreatePrefab();

                    instance.transform.position = emitPoint; 
                    instance.SetActive(true); 

                    var rotatedDirection = Quaternion.Euler(0, -scatterAngle / 2 + scatterAngle / (shotgunCount - 1) * i, 0) * direction;
                    instance.Velocity = rotatedDirection.normalized * shotSpeed;
                }
            }
        }
    }

    private Bullet GetPrefabFromPool()
    {
        return PrefabPool.FirstOrDefault(b => !b.gameObject.activeInHierarchy);
    }
}