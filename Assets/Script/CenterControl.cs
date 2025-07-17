using UnityEngine;

public class CenterControl : MonoBehaviour
{
    [Header("旋转设置")]
    public float rotationSpeed = 100f;  // 旋转速度
    public bool invertRotation = false;  // 是否反转旋转方向
    public float rotationSmoothness = 0.1f;  // 旋转平滑度 (0.1 = 很平滑, 1 = 无平滑)
    
    [Header("自转设置")]
    public float autoRotationSpeed = 10f;  // 自转速度（度/秒）
    public bool enableAutoRotation = true;  // 是否启用自转
    
    private bool isRotating = false;  // 是否正在旋转
    private Vector3 lastMousePosition;  // 上一帧鼠标位置
    private float currentRotationVelocity = 0f;  // 当前旋转速度
    private float targetRotationVelocity = 0f;  // 目标旋转速度
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        HandleMouseInput();
        HandleAutoRotation();
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
        
        // 如果正在旋转，计算鼠标移动并应用旋转
        if (isRotating)
        {
            Vector3 currentMousePosition = Input.mousePosition;
            Vector3 mouseDelta = currentMousePosition - lastMousePosition;
            
            // 计算Y轴旋转（左右滑动）
            float rotationInput = mouseDelta.x * rotationSpeed * Time.deltaTime;
            
            // 如果设置了反转，则反向旋转
            if (invertRotation)
            {
                rotationInput = -rotationInput;
            }
            
            // 设置目标旋转速度
            targetRotationVelocity = rotationInput;
            
            // 更新上一帧鼠标位置
            lastMousePosition = currentMousePosition;
        }
        else
        {
            // 如果没有拖拽，目标速度为0
            targetRotationVelocity = 0f;
        }
        
        // 平滑过渡到目标旋转速度
        currentRotationVelocity = Mathf.Lerp(currentRotationVelocity, targetRotationVelocity, rotationSmoothness);
        
        // 应用旋转
        if (Mathf.Abs(currentRotationVelocity) > 0.01f)
        {
            transform.Rotate(0, currentRotationVelocity, 0, Space.World);
        }
    }
    
    private void HandleAutoRotation()
    {
        // 如果启用自转且当前没有被鼠标控制，则执行自转
        if (enableAutoRotation && !isRotating)
        {
            float autoRotationY = autoRotationSpeed * Time.deltaTime;
            transform.Rotate(0, autoRotationY, 0, Space.World);
        }
    }
}
