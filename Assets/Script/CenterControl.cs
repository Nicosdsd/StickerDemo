using UnityEngine;

public class CenterControl : MonoBehaviour
{
    [Header("旋转设置")]
    public float rotationSpeed = 2f;  // 旋转速度
    public bool invertRotation = false;  // 是否反转旋转方向
    
    private bool isRotating = false;  // 是否正在旋转
    private Vector3 lastMousePosition;  // 上一帧鼠标位置
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        HandleMouseInput();
    }
    
    private void HandleMouseInput()
    {
        // 检测鼠标按下
        if (Input.GetMouseButtonDown(0))
        {
            isRotating = true;
            lastMousePosition = Input.mousePosition;
        }
        
        // 检测鼠标松开
        if (Input.GetMouseButtonUp(0))
        {
            isRotating = false;
        }
        
        // 如果正在旋转，计算鼠标移动并旋转物体
        if (isRotating)
        {
            Vector3 currentMousePosition = Input.mousePosition;
            Vector3 mouseDelta = currentMousePosition - lastMousePosition;
            
            // 计算Y轴旋转（左右滑动）
            float rotationY = mouseDelta.x * rotationSpeed;
            
            // 如果设置了反转，则反向旋转
            if (invertRotation)
            {
                rotationY = -rotationY;
            }
            
            // 应用旋转
            transform.Rotate(0, rotationY, 0, Space.World);
            
            // 更新上一帧鼠标位置
            lastMousePosition = currentMousePosition;
        }
    }
}
