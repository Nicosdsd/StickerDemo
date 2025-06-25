using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Serialization;

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
    public Transform propGroup;    // 道具组
    public float bezierCurveHeight = 2f; // 贝塞尔曲线弧度高度，可在Inspector面板调节
    public float flyDuration = 1f; // 飞行动画持续时间，可在Inspector面板调节
    public Vector3 endScale = Vector3.one; // 飞行结束时的缩放，可在Inspector面板调节
    public AnimationCurve flyCurve = AnimationCurve.Linear(0, 0, 1, 1); // 飞行进度曲线，可在Inspector面板自定义

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

    private IEnumerator FlyToSlot(GameObject propObject, Vector3 startWorldPos, Transform slot, float duration)
    {
        Vector3 endWorldPos = slot.position;
        Vector3 dir = (endWorldPos - startWorldPos).normalized;
        Vector3 perp = Vector3.Cross(dir, Vector3.forward).normalized;

        // 判断当前道具是否属于后一半
        int propIndex = propItems.FindIndex(item => item.propObject == propObject);
        float curveHeight = bezierCurveHeight;
        if (propIndex >= propItems.Count / 2)
        {
            curveHeight = -bezierCurveHeight;
        }
        Vector3 controlPoint = (startWorldPos + endWorldPos) / 2f + perp * curveHeight;

        Vector3 startScale = propObject.transform.localScale;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = flyCurve.Evaluate(elapsed / duration);
            Vector3 pos = Mathf.Pow(1 - t, 2) * startWorldPos
                        + 2 * (1 - t) * t * controlPoint
                        + Mathf.Pow(t, 2) * endWorldPos;
            propObject.transform.position = pos;
            propObject.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        propObject.transform.position = endWorldPos;
        propObject.transform.SetParent(slot, false);
        propObject.transform.localPosition = Vector3.zero;
        //propObject.transform.localRotation = Quaternion.identity;
        propObject.transform.localScale = endScale;
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

                // 查找第一个空槽位
                if (propGroup != null && item.propObject != null)
                {
                    bool slotted = false;
                    for (int i = 0; i < propGroup.childCount; i++)
                    {
                        Transform slot = propGroup.GetChild(i);
                        if (slot.childCount == 0)
                        {
                            // 计算进度点在进度条上的UI世界坐标
                            Vector3 startPos = barStart.position;
                            Vector3 endPos = barEnd.position;
                            Vector3 uiPos = Vector3.Lerp(startPos, endPos, item.progressPoint);
                            Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(canvasCamera, uiPos);
                            screenPos.z = 10f;
                            Vector3 worldPos = canvasCamera.ScreenToWorldPoint(screenPos);
                            worldPos.z = 0f;

                            // 先把道具放到进度条上的位置
                            item.propObject.transform.SetParent(null);
                            item.propObject.transform.position = worldPos;

                            // 启动飞行动画
                            StartCoroutine(FlyToSlot(item.propObject, worldPos, slot, flyDuration));
                            slotted = true;
                            break;
                        }
                    }
                    if (!slotted)
                    {
                        Debug.LogWarning("没有空槽位可用！");
                    }
                }
            }
        }
    }
}
