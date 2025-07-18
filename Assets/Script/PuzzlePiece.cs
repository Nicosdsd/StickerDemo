using UnityEngine;
using System.Collections;
using HighlightPlus;
/// 用来控制拼图块，包括拼图块的初始化、拼图块的拖拽、拼图块的放置、拼图块的动画、拼图块的邻居检测。


public class PuzzlePiece : MonoBehaviour
{
    public Vector3 _startPosition; // 拼图块的初始位置
    private bool _isDragging; // 标记是否正在拖拽
    public Transform targetArea; // 拼图块的目标区域
    public float snapDistance = 1.0f; // 自动吸附的距离阈值
    private Animator animator; // 动画控制器
    public float neighborDetectionRadius = 2.0f; // 检测邻近拼图块的半径
    // 缓动速度
    public float snapSpeed = 5.0f; // 修改：拼图块吸附到目标位置的速度
    public float returnSpeed = 8.0f; // 新增：拼图块返回初始位置的速度
    public float dragSmoothSpeed = 30.0f; // 新增：拖拽时的平滑速度

    // 新增：点击时固定的向上偏移量
    public Vector3 offsetOnClick = new Vector3(0f, 2f, 0f);
    // 新增：向上拖拽时，拼图与手指距离拉远的系数
    public float dragFactor = 0.5f;

    // 新增：Scale 参数
    public float StartScale = 1.0f;
    public float DragScale = 1.2f;
    public float scaleSmoothSpeed = 5.0f; // 新增：缩放过渡速度

    public float returnDelay = 0.5f; // 延迟返回初始位置的时间

    private Vector3 mouseWorldPosOnDragBegin; // 拖拽开始时鼠标的世界坐标（在拼图块深度）
    private Vector3 pieceWorldPosOnDragBegin; // 拖拽开始时拼图块的世界坐标
    private float mouseConversionZ; // 鼠标屏幕坐标到世界坐标转换时使用的Z深度

    public bool isLocked = false; // 新增：标记拼图块是否已锁定
    private Coroutine _moveCoroutine; // 新增：用于跟踪移动协程
    private Vector3 _currentApplyingOffset = Vector3.zero; // 新增：用于平滑处理offsetOnClick的当前偏移值

    // 新增：是否为道具拼图
    public bool isPropPiece = false;

    // 新增：公共 getter 用于检查拼图块是否已锁定
    public bool IsLocked => isLocked;

    private PuzzleGroup puzzleGroup; // 修改：PuzzleGroup实例字段
    private Settings settings; // 新增：Settings实例字段

    public int priority = 0; // 生成优先级，0为最高，依次递增

    // 新增：成功动画prefab
    public GameObject effectPrefab; // 成功动画Prefab

    // 新增：强制完成拼图块的方法
    public void ForceComplete()
    {
        if (isLocked) return;
        if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
        transform.position = targetArea.position;
        transform.localScale = Vector3.one * DragScale;
        PlaySuccessAnimation();
        transform.parent = targetArea;
        if (!isPropPiece)
        {
            puzzleGroup.DecreasePieceCount();
        }
        DetectNeighborsAndTriggerAnimation();
        isLocked = true;
        _isDragging = false;
        AudioManager.Instance.PlaySound("放下", transform.position);

    }

    // 初始化
    void Start()
    {
        _startPosition = transform.position;
        animator = GetComponent<Animator>();
        
        // 修改：查找PuzzleGroup实例
        puzzleGroup = FindObjectOfType<PuzzleGroup>();
        if (puzzleGroup == null)
        {
            Debug.LogError("未找到 PuzzleGroup 组件，请确保场景中有 PuzzleGroup 脚本。");
        }
        
        // 新增：查找Settings实例
        settings = FindObjectOfType<Settings>();
        if (settings == null)
        {
            Debug.LogError("未找到 Settings 组件，请确保场景中有 Settings 脚本。");
        }
    }

    // 每帧更新
    void Update()
    {
        
    }

    // 新增：固定更新，用于物理相关的更新，比如拖拽
    void FixedUpdate()
    {
        if (_isDragging)
        {
            Vector3 mousePos = Input.mousePosition;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, mouseConversionZ));
            Vector3 delta = worldPos - mouseWorldPosOnDragBegin;
            Vector3 targetPos = pieceWorldPosOnDragBegin + _currentApplyingOffset + delta;
            float upOffset = delta.z > 0 ? delta.z * dragFactor : 0f;
            targetPos += new Vector3(0f, 0f, upOffset);
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.fixedDeltaTime * dragSmoothSpeed);
        }
    }

    // 鼠标按下时触发
    void OnMouseDown()
    {
        UpdateBaseStartPosition(); // 拖拽前更新初始位置记录

        if (isLocked) return; 

        // 新增：如果当前有移动协程在运行，则停止它
        if (_moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
            _moveCoroutine = null;
        }

        _isDragging = true;
        pieceWorldPosOnDragBegin = transform.position;
        mouseConversionZ = Camera.main.WorldToScreenPoint(transform.position).z;
        mouseWorldPosOnDragBegin = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, mouseConversionZ));

        _currentApplyingOffset = Vector3.zero; // 重置当前偏移
        StartCoroutine(AnimateOffsetValue(Vector3.zero, offsetOnClick, scaleSmoothSpeed)); // 开始平滑偏移过渡

        StartCoroutine(ChangeScaleOverTime(Vector3.one * DragScale, scaleSmoothSpeed));

        print("拖拽");
        AudioManager.Instance.PlaySound("抓起",transform.position);
    }

    // 鼠标抬起时触发
    void OnMouseUp()
    {
        if (!_isDragging) // 新增：如果不是正在拖拽状态，则直接返回
        {
            return;
        }

        _isDragging = false;

        // 新增：在启动新的移动协程前，也确保停止任何可能存在的旧协程（以防万一）
        if (_moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
            _moveCoroutine = null;
        }

        // 新增：道具拼图吸附逻辑
        if (isPropPiece)
        {
            Transform nearest = FindNearestTargetArea();
            if (nearest != null && Vector3.Distance(transform.position, nearest.position) <= snapDistance)
            {
                targetArea = nearest;
                _moveCoroutine = StartCoroutine(MoveToPosition(targetArea.position, true));
            }
            else
            {
                AudioManager.Instance.PlaySound("返回", transform.position);
                _moveCoroutine = StartCoroutine(MoveToPosition(_startPosition, false));
                StartCoroutine(ChangeScaleOverTime(Vector3.one * StartScale, scaleSmoothSpeed));
            }
            return;
        }

        if (Vector3.Distance(transform.position, targetArea.position) <= snapDistance)
        {
            _moveCoroutine = StartCoroutine(MoveToPosition(targetArea.position, true)); // 移动到目标位置并吸附
        }
        else
        {
            //拖动失败飞回初始点
            AudioManager.Instance.PlaySound("返回", transform.position); // 在开始返回时播放音效
            animator.SetTrigger("Fault");
            
            // 新增：拼错时扣除生命值
            if (settings != null)
            {
                settings.DecreaseHealth();
            }
            
            StartCoroutine(DelayedReturnToStart()); // 使用延迟协程
        }
    }
    
    private IEnumerator DelayedReturnToStart()
    {
        yield return new WaitForSeconds(returnDelay); // 延迟0.5秒，确保动画播放完成
        _moveCoroutine = StartCoroutine(MoveToPosition(_startPosition, false)); // 移动回初始位置
        StartCoroutine(ChangeScaleOverTime(Vector3.one * StartScale, scaleSmoothSpeed)); // 立刻开始向StartScale过渡
    }

    /// <summary>
    /// 协程：移动拼图到指定位置。
    /// </summary>
    /// <param name="targetPosition">目标位置</param>
    /// <param name="isSnap">是否吸附到目标点（触发后续逻辑）</param>
    private IEnumerator MoveToPosition(Vector3 targetPosition, bool isSnap)
    {
        float speed = isSnap ? snapSpeed : returnSpeed;
        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, speed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPosition;
        if (isSnap)
        {
            AddLightLayer2ToChildMeshRenderers();
            PlaySuccessAnimation();
            transform.parent = targetArea;
            DetectNeighborsAndTriggerAnimation();
            isLocked = true;
            AudioManager.Instance.PlaySound("放下",transform.position);
            if (!isPropPiece) puzzleGroup.DecreasePieceCount();
        }
    }

    // 新增：协程，用于平滑改变Scale
    private IEnumerator ChangeScaleOverTime(Vector3 targetScale, float speed)
    {
        float journey = 0f;
        Vector3 initialScale = transform.localScale;

        while (journey <= 1.0f)
        {
            journey += Time.deltaTime * speed;
            transform.localScale = Vector3.Lerp(initialScale, targetScale, journey);
            yield return null;
        }
        transform.localScale = targetScale; // 确保最终设置为目标值
    }

    // 新增：协程，用于平滑改变当前的偏移值
    private IEnumerator AnimateOffsetValue(Vector3 startOffset, Vector3 endOffset, float speed)
    {
        float journey = 0f;
        while (journey <= 1.0f)
        {
            journey += Time.deltaTime * speed;
            _currentApplyingOffset = Vector3.Lerp(startOffset, endOffset, journey);
            yield return null;
        }
        _currentApplyingOffset = endOffset; //  确保最终设置为目标值
    }

    // 新增：当父对象移动后，调用此方法来更新拼图块的初始位置记录
    public void UpdateBaseStartPosition()
    {
        if (!isLocked && !_isDragging)
        {
            _startPosition = transform.position;
        }
    }

    // 检测邻近拼图块并触发它们的动画
    private void DetectNeighborsAndTriggerAnimation()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, neighborDetectionRadius);

        foreach (Collider collider in colliders)
        {
            PuzzlePiece neighborPiece = collider.GetComponent<PuzzlePiece>();

            // 确保是另一个已吸附的拼图块
            if (neighborPiece != null && neighborPiece != this &&
                Vector3.Distance(neighborPiece.transform.position, neighborPiece.targetArea.position) <= snapDistance)
            {
                //sphereMaskController.GetComponent<Animator>().SetTrigger("Arrive");
            }
        }
    }

    // 新增：为子物体的MeshRenderer添加Light Layer2
    private void AddLightLayer2ToChildMeshRenderers()
    {
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        int mask = 1 << 2;
        foreach (var r in renderers) r.renderingLayerMask |= (uint)mask;
    }

    // 新增：为子物体的MeshRenderer移除Light Layer2
    private void RemoveLightLayer2FromChildMeshRenderers()
    {
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        int mask = 1 << 2;
        foreach (var r in renderers) r.renderingLayerMask &= ~(uint)mask;
    }

    // 新增：播放成功吸附动画
    public void PlaySuccessAnimation()
    {
        print("播放成功");
        
        // 新增：实例化波纹特效
        if (effectPrefab != null)
        {
            GameObject successEffect = Instantiate(effectPrefab, transform.position, transform.rotation);
        }
        if(animator != null)
        {
            animator.SetTrigger("Success");
        }
    }

    // 新增：协程，延迟指定时间后移除Light Layer2
    private IEnumerator RemoveLightLayerAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        RemoveLightLayer2FromChildMeshRenderers();
    }

    // 新增：查找最近的TargetArea（仅用于道具拼图）
    private Transform FindNearestTargetArea()
    {
        PuzzleTarget[] allTargets = FindObjectsOfType<PuzzleTarget>();
        Transform nearest = null;
        float minDist = float.MaxValue;
        foreach (var t in allTargets)
        {
            float dist = Vector3.Distance(transform.position, t.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = t.transform;
            }
        }
        return nearest;
    }


     // 在编辑器中绘制辅助线，方便调试
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, neighborDetectionRadius); // 绘制邻居检测范围

        if (targetArea != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(targetArea.position, snapDistance); // 绘制目标区域的吸附范围
        }
    }
}