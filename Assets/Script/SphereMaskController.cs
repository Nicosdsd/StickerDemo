using UnityEngine;

public class SphereMaskController : MonoBehaviour
{
    public Material targetMaterial; // 需要赋值的材质
    public float maxRadius = 3.0f; // 鼠标按下时的目标半径
    public float currentRadius; // 当前半径
    public float aniRadius;//用于动画的半径
    public float hardness = 0.5f; // 硬度
    
    public float radiusTransitionSpeed = 85f; // 新增：半径过渡速度
    private bool isDragging = false;

  

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main; // 获取主摄像机
        currentRadius = 0f; // 初始化 currentRadius
    }
    

    void Update()
    {
       
        // 鼠标交互逻辑
        if (Input.GetMouseButtonDown(0)) // 检测鼠标左键按下
        {
            isDragging = true;
        }

        if (Input.GetMouseButtonUp(0)) // 检测鼠标左键抬起
        {
            isDragging = false;
        }

        if (isDragging)
        {
            currentRadius = Mathf.Lerp(currentRadius, maxRadius, Time.deltaTime * radiusTransitionSpeed);
        
        }
        else
        {
            currentRadius = Mathf.Lerp(currentRadius, 0f, Time.deltaTime * radiusTransitionSpeed);
        
        }   

       

        if (isDragging && mainCamera != null)
        {
            // 获取鼠标在屏幕上的位置
            Vector3 mousePosition = Input.mousePosition;
            // 计算物体当前在摄像机前方多远
            float distanceToCamera = Vector3.Dot(transform.position - mainCamera.transform.position, mainCamera.transform.forward);
            // 将屏幕 Z 坐标设置为物体与摄像机的距离，这样物体就会保持在那个深度
            mousePosition.z = distanceToCamera;
            // 将鼠标的屏幕坐标转换为世界坐标
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);
            // 更新物体的位置
            transform.position = worldPosition;
        }

        // 更新 Shader 属性的逻辑
        if (targetMaterial != null)
        {
            // 传递当前物体（球体）的世界坐标到Shader
            targetMaterial.SetVector("_SphereCenter", transform.position);
            // 传递半径和硬度到Shader
            targetMaterial.SetFloat("_Radius", currentRadius + aniRadius);
            targetMaterial.SetFloat("_Hardness", hardness);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, currentRadius + aniRadius);
    }
}