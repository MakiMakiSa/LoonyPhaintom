using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(2500)]
public class DynamicRenderTexture : MonoBehaviour
{
    //レンダラー設定用のHubクラス
    [Serializable]
    private class TargetState
    {
        [SerializeField] public Renderer _renderer;
        [SerializeField] public string _targetProperty = "_BaseMap";


        /// <summary>
        /// _rendererの_targetPropertyにrenderTextureを指定
        /// </summary>
        /// <param name="renderTexture"></param>
        public void SetTexture(Texture renderTexture)
        {
            var block = new MaterialPropertyBlock();
            _renderer.GetPropertyBlock(block);
            block.SetTexture(_targetProperty, renderTexture);
            _renderer.SetPropertyBlock(block);
        }
    }

    //RenderTextureの初期化パラメーター、今は最低限のパラメーター指定しか用意していないけれど、将来的に全てのパラメーターを指定できる予定
    [SerializeField] private Vector2 _size = new Vector2(256, 256);
    [SerializeField] private int _depth = 16;
    [SerializeField] private RenderTextureFormat _format = RenderTextureFormat.ARGB32;

    //テクスチャを設定するターゲット群
    [SerializeField] private Camera _targetCamera;
    [SerializeField] private List<TargetState> _targetRenderers = new List<TargetState>();
    [SerializeField] private List<RawImage> _targetRaws = new List<RawImage>();

    [SerializeField] private bool isOneShot;

    //生成されたRenderTexture
    public RenderTexture InstanceRenderTexture { get; set; }


    /// <summary>
    /// RT生成、ターゲットのテクスチャをRTに設定
    /// </summary>
    private void Start()
    {
        //生成
        InstanceRenderTexture = new RenderTexture((int)_size.x, (int)_size.y, _depth, _format);
        InstanceRenderTexture.Create();

        //初期化

        //設定
        if (_targetCamera)
        {
            _targetCamera.targetTexture = InstanceRenderTexture;
        }

        foreach (var target in _targetRenderers)
        {
            target.SetTexture(InstanceRenderTexture);
        }

        foreach (var raw in _targetRaws)
        {
            raw.texture = InstanceRenderTexture;
        }


        if (isOneShot)
        {
            _targetCamera.Render();
            _targetCamera.gameObject.SetActive(false);
        }
    }


    /// <summary>
    /// 解放
    /// </summary>
    private void OnDestroy()
    {
        if (InstanceRenderTexture)
        {
            Destroy(InstanceRenderTexture);
            InstanceRenderTexture = null; //ここでNullを代入しないと、EngineのC++側で用意されたTextureが消されない
        }
    }


    /// <summary>
    /// このスクリプトは、１カメラにつき１つだけ付く想定なので、アタッチの際に自動で初期化されるようサポート
    /// しかし、上記意外の管理方法も想定して、RequireComponentは指定していない状態なので、Cameraにアタッチしなくても動くようにはしてある
    /// </summary>
    private void Reset()
    {
        var selfCamera = GetComponent<Camera>();
        if (!selfCamera)
        {
            return;
        }

        _targetCamera = selfCamera;
    }

    [ContextMenu("AutoInit")]
    public void AutoInit()
    {
        _targetRenderers.Clear();
        var mrs = GameObject.FindObjectsOfType<MeshRenderer>();
        foreach (var meshRenderer in mrs)
        {
            var target = new TargetState();
            target._renderer = meshRenderer;
            target._targetProperty = "_BaseMap";
            _targetRenderers.Add(target);
        }
    }
}