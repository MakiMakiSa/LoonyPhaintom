using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPS : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // Set target FPS to 60
        Application.targetFrameRate = 60;

        // Enable VSync
        QualitySettings.vSyncCount = 1;
    }
}