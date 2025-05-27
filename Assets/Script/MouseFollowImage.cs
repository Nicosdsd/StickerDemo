using UnityEngine;
using UnityEngine.UI; // 需要引用UI命名空间
using UnityEngine.EventSystems; // (可选) 如果需要更复杂的UI事件处理

public class MouseFollowImage : MonoBehaviour
{
    [Tooltip("鼠标按下时显示的图片")]
    public Sprite pressedSprite; // 拖拽到Inspector，设置鼠标按下时的图片

    private Image uiImage;        // Image组件的引用
    private Sprite originalSprite; // 存储原始图片
    private RectTransform rectTransform; // RectTransform组件的引用，用于设置位置
    private AudioSource currentLoopingSound; // 当前正在播放的循环音效

    void Awake()
    {
        uiImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();

        if (uiImage == null)
        {
            Debug.LogError("MouseFollowImage: 无法在GameObject上找到Image组件！请添加Image组件。");
            enabled = false; // 禁用此脚本，因为它无法正常工作
            return;
        }

        if (rectTransform == null)
        {
            Debug.LogError("MouseFollowImage: 无法在GameObject上找到RectTransform组件！UI元素应该有此组件。");
            enabled = false;
            return;
        }

        originalSprite = uiImage.sprite; // 存储初始的图片
    }

    void Update()
    {
        if (uiImage == null || rectTransform == null) return;

        // 1. 使UI图片跟随鼠标
        // 注意: Input.mousePosition 返回的是屏幕坐标。
        // 如果Canvas的Render Mode是 "Screen Space - Overlay"，直接赋值给rectTransform.position即可。
        // 如果是 "Screen Space - Camera" 或 "World Space"，则需要使用 RectTransformUtility.ScreenPointToLocalPointInRectangle 进行坐标转换。
        // 此处假设为 "Screen Space - Overlay"
        rectTransform.position = Input.mousePosition;

        // 2. 处理鼠标按下和松开事件来切换图片
        if (Input.GetMouseButtonDown(0)) // 0代表鼠标左键
        {
            if (pressedSprite != null)
            {
                uiImage.sprite = pressedSprite;
            }
            else
            {
                Debug.LogWarning("MouseFollowImage: pressedSprite未设置！");
            }

            // // 播放循环音效
            // if (AudioManager.Instance != null && !string.IsNullOrEmpty("按压"))
            // {
            //     if (currentLoopingSound != null) // 如果已有音效在播放，先停止
            //     {
            //         AudioManager.Instance.StopLoopingSound(currentLoopingSound);
            //     }
            //     currentLoopingSound = AudioManager.Instance.PlayLoopingSound("按压", transform.position); 
            // }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            // 恢复原始图片
            // 即使originalSprite在开始时为null（Image组件上没有初始图片），这也将正确地将其设置回null
            uiImage.sprite = originalSprite;

            // // 停止循环音效
            // if (currentLoopingSound != null && AudioManager.Instance != null)
            // {
            //     AudioManager.Instance.StopLoopingSound(currentLoopingSound);
            //     currentLoopingSound = null;
            // }
        }
    }
} 