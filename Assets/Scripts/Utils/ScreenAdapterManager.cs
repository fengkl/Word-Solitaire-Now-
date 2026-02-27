using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 手机屏幕适配管理器
/// 主要处理不同分辨率手机的UI适配问题
/// </summary>
public class ScreenAdapterManager : MonoBehaviour
{
    public static ScreenAdapterManager Instance { get; private set; }
    
    [Header("设计分辨率")]
    public Vector2 designResolution = new Vector2(1080, 1920); // 竖屏设计分辨率
    
    [Header("适配策略")]
    public CanvasScaler.ScreenMatchMode screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
    [Range(0, 1)]
    public float matchWidthOrHeight = 0.5f; // 0=匹配宽度，1=匹配高度，0.5=平衡
    
    [Header("安全区域")]
    public bool enableSafeArea = true;
    public Rect safeAreaRect;
    
    private CanvasScaler canvasScaler;
    private Camera mainCamera;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        InitializeScreenAdaptation();
    }
    
    private void Start()
    {
        ApplyScreenAdaptation();
    }
    
    /// <summary>
    /// 初始化屏幕适配
    /// </summary>
    private void InitializeScreenAdaptation()
    {
        // 获取主Canvas上的CanvasScaler
        canvasScaler = FindObjectOfType<CanvasScaler>();
        if (canvasScaler == null)
        {
            Debug.LogError("未找到CanvasScaler组件，请确保场景中有带有CanvasScaler的Canvas！");
            return;
        }
        
        mainCamera = Camera.main;
        
        // 设置CanvasScaler参数
        SetupCanvasScaler();
        
        // 应用安全区域
        if (enableSafeArea)
        {
            ApplySafeArea();
        }
    }
    
    /// <summary>
    /// 设置CanvasScaler参数
    /// </summary>
    private void SetupCanvasScaler()
    {
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = designResolution;
        canvasScaler.screenMatchMode = screenMatchMode;
        canvasScaler.matchWidthOrHeight = matchWidthOrHeight;
        
        Debug.Log($"CanvasScaler已配置: 参考分辨率{designResolution}, 匹配模式{screenMatchMode}, 匹配值{matchWidthOrHeight}");
    }
    
    /// <summary>
    /// 应用屏幕适配
    /// </summary>
    public void ApplyScreenAdaptation()
    {
        Vector2 currentResolution = new Vector2(Screen.width, Screen.height);
        float currentAspect = currentResolution.x / currentResolution.y;
        float designAspect = designResolution.x / designResolution.y;
        
        Debug.Log($"当前分辨率: {currentResolution}, 宽高比: {currentAspect:F2}");
        Debug.Log($"设计分辨率: {designResolution}, 宽高比: {designAspect:F2}");
        
        // 根据不同设备类型调整适配策略
        AdjustForDeviceType(currentAspect, designAspect);
        
        // 更新安全区域
        if (enableSafeArea)
        {
            UpdateSafeArea();
        }
    }
    
    /// <summary>
    /// 根据设备类型调整适配策略
    /// </summary>
    private void AdjustForDeviceType(float currentAspect, float designAspect)
    {
        // 长屏手机适配（如iPhone X系列、安卓全面屏）
        if (currentAspect > designAspect * 1.1f)
        {
            // 更偏向匹配宽度，保持UI不过度拉伸
            canvasScaler.matchWidthOrHeight = 0.3f;
            Debug.Log("检测到长屏设备，调整匹配策略偏向宽度");
        }
        // 宽屏设备适配（如某些平板模式）
        else if (currentAspect < designAspect * 0.9f)
        {
            // 更偏向匹配高度
            canvasScaler.matchWidthOrHeight = 0.7f;
            Debug.Log("检测到宽屏设备，调整匹配策略偏向高度");
        }
        // 标准屏幕比例
        else
        {
            canvasScaler.matchWidthOrHeight = matchWidthOrHeight;
            Debug.Log("标准屏幕比例，使用默认匹配策略");
        }
    }
    
    /// <summary>
    /// 应用安全区域（刘海屏、水滴屏等）
    /// </summary>
    private void ApplySafeArea()
    {
        if (Application.platform == RuntimePlatform.Android || 
            Application.platform == RuntimePlatform.IPhonePlayer)
        {
            safeAreaRect = Screen.safeArea;
            Debug.Log($"安全区域: {safeAreaRect}");
        }
    }
    
    /// <summary>
    /// 更新安全区域
    /// </summary>
    private void UpdateSafeArea()
    {
        if (safeAreaRect != Screen.safeArea)
        {
            safeAreaRect = Screen.safeArea;
            Debug.Log($"安全区域更新: {safeAreaRect}");
            
            // 通知需要调整安全区域的UI组件
            SafeAreaAdjuster[] adjusters = FindObjectsOfType<SafeAreaAdjuster>();
            foreach (var adjuster in adjusters)
            {
                adjuster.ApplySafeArea(safeAreaRect);
            }
        }
    }
    
    /// <summary>
    /// 获取相对于设计分辨率的缩放因子
    /// </summary>
    public Vector2 GetScaleFactor()
    {
        Vector2 currentResolution = new Vector2(Screen.width, Screen.height);
        Vector2 scaleFactor = new Vector2(
            currentResolution.x / designResolution.x,
            currentResolution.y / designResolution.y
        );
        return scaleFactor;
    }
    
    /// <summary>
    /// 转换屏幕坐标到设计分辨率坐标系
    /// </summary>
    public Vector2 ScreenToDesignPosition(Vector2 screenPosition)
    {
        Vector2 scaleFactor = GetScaleFactor();
        return new Vector2(
            screenPosition.x / scaleFactor.x,
            screenPosition.y / scaleFactor.y
        );
    }
    
    /// <summary>
    /// 获取适配后的UI位置
    /// </summary>
    public Vector2 GetAdaptedPosition(RectTransform rectTransform)
    {
        Vector2 anchoredPosition = rectTransform.anchoredPosition;
        Vector2 scaleFactor = GetScaleFactor();
        
        return new Vector2(
            anchoredPosition.x * scaleFactor.x,
            anchoredPosition.y * scaleFactor.y
        );
    }
}

/// <summary>
/// 安全区域调整器组件
/// 用于自动调整UI元素避开刘海、状态栏等区域
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class SafeAreaAdjuster : MonoBehaviour
{
    [Header("调整方向")]
    public bool adjustTop = true;
    public bool adjustBottom = true;
    public bool adjustLeft = false;
    public bool adjustRight = false;
    
    [Header("偏移量")]
    public Vector2 offset = Vector2.zero;
    
    private RectTransform rectTransform;
    private Rect originalRect;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalRect = rectTransform.rect;
    }
    
    /// <summary>
    /// 应用安全区域调整
    /// </summary>
    public void ApplySafeArea(Rect safeArea)
    {
        if (rectTransform == null) return;
        
        // 计算安全区域相对于屏幕的比例
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        
        Rect safeAreaRelative = new Rect(
            safeArea.x / screenWidth,
            safeArea.y / screenHeight,
            safeArea.width / screenWidth,
            safeArea.height / screenHeight
        );
        
        // 获取Canvas
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;
        
        RectTransform canvasRectTransform = canvas.GetComponent<RectTransform>();
        Vector2 canvasSize = canvasRectTransform.rect.size;
        
        // 计算需要的偏移量
        Vector2 minAnchor = rectTransform.anchorMin;
        Vector2 maxAnchor = rectTransform.anchorMax;
        
        if (adjustTop)
        {
            float topInset = (1f - safeAreaRelative.yMax) * canvasSize.y;
            rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x, topInset + offset.y);
        }
        
        if (adjustBottom)
        {
            float bottomInset = safeAreaRelative.yMin * canvasSize.y;
            rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, -bottomInset - offset.y);
        }
        
        if (adjustLeft)
        {
            float leftInset = safeAreaRelative.xMin * canvasSize.x;
            rectTransform.offsetMin = new Vector2(leftInset + offset.x, rectTransform.offsetMin.y);
        }
        
        if (adjustRight)
        {
            float rightInset = (1f - safeAreaRelative.xMax) * canvasSize.x;
            rectTransform.offsetMax = new Vector2(-rightInset - offset.x, rectTransform.offsetMax.y);
        }
    }
}
