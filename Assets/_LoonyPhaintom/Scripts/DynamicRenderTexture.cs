using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
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
    [SerializeField] private UpdateMode updateMode;

    //
    enum UpdateMode
    {
        Default,
        OneShot,
        Manual
    }

    public delegate float CalculateAction(Color color);

    //非同期計算中フラグ
    public bool IsCalculatingRate { get; set; } = false;

    //レンダーテクスチャ
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


        switch (updateMode)
        {
            case UpdateMode.Default:
                break;
            case UpdateMode.OneShot:
                UpdateRender();
                _targetCamera.enabled = false;
                _targetCamera.gameObject.SetActive(false);
                break;
            case UpdateMode.Manual:
                _targetCamera.enabled = false;
                break;
        }
    }

    /// <summary>
    /// 現在のカラーバッファのコピーを取得するサンプル関数
    /// 毎フレーム取得は重いので非推奨
    /// </summary>
    private Color[] GetColorBuffer()
    {
        var texture2D = new Texture2D(InstanceRenderTexture.width, InstanceRenderTexture.height);
        RenderTexture.active = InstanceRenderTexture;
        texture2D.ReadPixels(new Rect(0, 0, InstanceRenderTexture.width, InstanceRenderTexture.height), 0, 0);
        texture2D.Apply();
        RenderTexture.active = null;
        var colorBuffer = texture2D.GetPixels(0, 0, InstanceRenderTexture.width, InstanceRenderTexture.height);
        Destroy(texture2D);

        return colorBuffer;
    }


    /// <summary>
    /// テクスチャの中身を解析する場合のサンプル関数
    /// </summary>
    /// <param name="loopForOneFrame">１フレームあたりどれくらい計算を回すか、時間基準で良い感じにできた方が良いかも</param>
    /// <param name="calculateAction">計算の内容</param>
    /// <param name="onFillRateCalculated">計算結果の受け取り</param>
    /// <returns></returns>
    public async UniTaskVoid GetFillRate(int loopForOneFrame, CalculateAction calculateAction,
        Action<float, int> onFillRateCalculated)
    {
        IsCalculatingRate = true;
        var colorBuffer = GetColorBuffer();
        var rate = 0f;
        for (var i = 0; i < colorBuffer.Length; i++)
        {
            rate += calculateAction(colorBuffer[i]);
            
            if (i % loopForOneFrame == loopForOneFrame - 1)
            {
                await UniTask.Yield(PlayerLoopTiming.Update);
            }
        }

        onFillRateCalculated(rate, colorBuffer.Length);
        IsCalculatingRate = false;
    }


    /// <summary>
    /// カメラの描画更新
    /// </summary>
    public void UpdateRender()
    {
        _targetCamera.Render();
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