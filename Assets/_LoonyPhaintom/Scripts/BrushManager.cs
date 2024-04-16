using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
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

    [SerializeField] private List<DynamicRenderTexture> cameras = new List<DynamicRenderTexture>();
    [SerializeField] private TextMeshProUGUI rateText;
    private Tweener DoRate { get; set; }

    private List<GameObject> Brushes { get; } = new List<GameObject>();
    private float Rate { get; set; }
    private float FromValue { get; set; }
    private float ToValue { get; set; }


    private void Awake()
    {
        DoRate = DOVirtual.Float(0f, 1f, 3f,
            value =>
            {
                var rate = Mathf.Lerp(FromValue, ToValue, value);
                rateText.text = rate.ToString("0") + "%";
            });
        DoRate.SetLink(gameObject);
        DoRate.Pause();
        DoRate.SetAutoKill(false);
        DoRate.SetEase(Ease.Linear);
    }

    private async UniTaskVoid Update()
    {
        if (cameras.Count <= 0) return;

        foreach (var cam in cameras)
        {
            cam.UpdateRender();
        }

        foreach (var brush in Brushes)
        {
            brush.SetActive(false);
        }

        if (!DoRate.IsPlaying() && cameras.All(b => !b.IsCalculatingRate))
        {
            //前回の計算結果を反映
            FromValue = ToValue;
            ToValue = (Rate / cameras.Count) * 100;
            DoRate.Restart();
            //初期化して計算
            Rate = 0f;
            foreach (var cam in cameras)
            {
                cam.GetFillRate(1024 * 8, //実機ならもっと減らす
                        color => Mathf.Min(1f, Mathf.Max(color.r, color.g, color.b) * 100f),
                        (result, bufferLength) => { Rate += result / bufferLength; })
                    .Forget();
            }
        }
    }


    public void Add(GameObject brushObject)
    {
        Brushes.Add(brushObject);
    }
}