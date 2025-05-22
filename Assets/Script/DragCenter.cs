using UnityEngine;

public class DragCenter : MonoBehaviour
{
    public float mouseDownZDistance = 5.0f; // 鼠标按下时物体距离相机的深度
    public float mouseUpZDistance = 10.0f; // 鼠标松开时物体距离相机的深度
    public float zSmoothingSpeed = 5.0f; // Z轴缓动速度
    public Camera targetCamera; // 摄像机，可以手动赋值

    private float currentTargetZDistance;
    private float actualZDistance;

    void Start()
    {
        // 如果未设置摄像机，尝试使用主摄像机
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
        
        // 检查是否正确分配了摄像机
        if (targetCamera == null)
        {
            Debug.LogError("No camera found! Please assign one.");
            return; // 如果没有相机，则不继续执行
        }

        // 初始化Z轴距离
        actualZDistance = mouseUpZDistance;
        currentTargetZDistance = mouseUpZDistance;
    }

    void Update()
    {
        if (targetCamera == null)
            return;

        // 检测鼠标按键
        if (Input.GetMouseButtonDown(0)) // 0代表鼠标左键
        {
            currentTargetZDistance = mouseDownZDistance;
        }
        if (Input.GetMouseButtonUp(0)) // 0代表鼠标左键
        {
            currentTargetZDistance = mouseUpZDistance;
        }

        // 平滑过渡目标世界Z轴距离
        actualZDistance = Mathf.Lerp(actualZDistance, currentTargetZDistance, Time.deltaTime * zSmoothingSpeed);

        // 获取鼠标屏幕坐标
        Vector3 mouseScreenPosition = Input.mousePosition;

        // 限制鼠标坐标在屏幕范围内
        mouseScreenPosition.x = Mathf.Clamp(mouseScreenPosition.x, 0, Screen.width);
        mouseScreenPosition.y = Mathf.Clamp(mouseScreenPosition.y, 0, Screen.height);

        // 为了计算鼠标对应的世界X,Y坐标，我们需要一个Z深度值传给ScreenToWorldPoint。
        // 我们使用物体当前的Z深度（相对于相机），这样鼠标的X,Y映射就不会受到Z轴动画的影响。
        // transform.position.z 这里是上一帧的Z值，用它来确定当前鼠标交互的平面。
        float projectionDepth = targetCamera.WorldToScreenPoint(transform.position).z;
        mouseScreenPosition.z = projectionDepth;

        // 将屏幕坐标（使用当前物体深度）转换为世界坐标，得到鼠标在当前物体深度平面上的X,Y坐标
        Vector3 worldMouseXY = targetCamera.ScreenToWorldPoint(mouseScreenPosition);

        // 更新物体的位置：X,Y来自鼠标的投影，Z来自缓动的actualZDistance（作为世界Z坐标）
        transform.position = new Vector3(worldMouseXY.x, worldMouseXY.y, actualZDistance);
    }
}
