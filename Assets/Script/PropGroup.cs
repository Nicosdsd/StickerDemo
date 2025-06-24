using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PropItem
{
    [Range(0, 1)]
    public float progressPoint; // 进度点（0~1）
    public GameObject propObject; // 场景中的3D物体
}

public class PropGroup : MonoBehaviour
{
    public List<PropItem> propItems = new List<PropItem>();
    [Range(0, 1)]
    public float progress = 0f;
    public RectTransform barStart; // 进度条起点
    public RectTransform barEnd;   // 进度条终点
    public Camera canvasCamera;    // UI相机

    private HashSet<float> shownProgress = new HashSet<float>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (var item in propItems)
        {
            if (item.propObject != null && barStart != null && barEnd != null)
            {
                // 计算进度点在进度条上的UI世界坐标
                Vector3 startPos = barStart.position;
                Vector3 endPos = barEnd.position;
                Vector3 uiPos = Vector3.Lerp(startPos, endPos, item.progressPoint);

                // 转换为屏幕坐标
                Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(canvasCamera, uiPos);
                // 设定z值（距离相机多远），比如10
                screenPos.z = 10f;
                // 转换为3D世界坐标
                Vector3 worldPos = canvasCamera.ScreenToWorldPoint(screenPos);

                // 保证Z轴为0
                worldPos.z = 0f;
                item.propObject.transform.position = worldPos;
                // 你可以根据需要设置缩放
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var item in propItems)
        {
            if (progress >= item.progressPoint && !shownProgress.Contains(item.progressPoint))
            {
                Debug.Log($"到达进度{item.progressPoint}，显示物体：{item.propObject.name}");
                shownProgress.Add(item.progressPoint);
            }
        }
    }
}
