using UnityEngine;
using System.Collections;
using UnityEngine.Playables;

public class PuzzlePiece : MonoBehaviour
{
    private Vector3 _startPosition; // 拼图块的初始位置
    private bool _isDragging; // 标记是否正在拖拽
    public Transform targetArea; // 拼图块的目标区域
    public float snapDistance = 1.0f; // 自动吸附的距离阈值
    private Animator animator; // 动画控制器
    public float neighborDetectionRadius = 2.0f; // 检测邻近拼图块的半径
    public int score = 1;
    // 缓动速度
    public float snapSpeed = 5.0f; // 修改：拼图块吸附到目标位置的速度
    public float returnSpeed = 8.0f; // 新增：拼图块返回初始位置的速度
    public float dragSmoothSpeed = 30.0f; // 新增：拖拽时的平滑速度

    // 新增：点击时固定的向上偏移量
    public float offsetOnClick = 2f;
    // 新增：向上拖拽时，拼图与手指距离拉远的系数
    public float dragFactor = 0.5f;

    // 新增：Scale 参数
    public float StartScale = 1.0f;
    public float DragScale = 1.2f;
    public float scaleSmoothSpeed = 5.0f; // 新增：缩放过渡速度

    private Vector3 mouseWorldPosOnDragBegin; // 拖拽开始时鼠标的世界坐标（在拼图块深度）
    private Vector3 pieceWorldPosOnDragBegin; // 拖拽开始时拼图块的世界坐标
    private float mouseConversionZ; // 鼠标屏幕坐标到世界坐标转换时使用的Z深度

    private SpriteRenderer _targetAreaSprite; // 目标区域的SpriteRenderer组件，用于控制显隐
    public bool isLocked = false; // 新增：标记拼图块是否已锁定
    private Coroutine _moveCoroutine; // 新增：用于跟踪移动协程
    private float _currentApplyingOffsetY = 0f; // 新增：用于平滑处理offsetOnClick的当前Y偏移值

    private SphereMaskController sphereMaskController;
    // private PlayableDirector sphereMaskPlayableDirector;

    // 新增：公共 getter 用于检查拼图块是否已锁定
    public bool IsLocked
    {
        get { return isLocked; }
    }

    // 新增：强制完成拼图块的方法
    public void ForceComplete()
    {
        if (isLocked) // 如果已经锁定，则不执行任何操作
        {
            return;
        }

        // 停止任何正在进行的移动或缩放协程
        if (_moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
            _moveCoroutine = null;
        }
        // 假设还有一个 _scaleCoroutine，也应该停止（根据现有代码，ChangeScaleOverTime是启动的，但没有统一的引用变量）
        // 为了简单起见，这里暂时不显式停止缩放协程，但理想情况下应该管理好所有协程。
        // 最好是将所有协程都用一个变量引用，或者使用 StopAllCoroutines()，但这可能会停止其他不相关的协程。

        transform.position = targetArea.position;
        transform.localScale = Vector3.one; // 确保Scale设置为1

        if (animator != null)
        {
            animator.SetTrigger("SelectBlink"); // 触发完成动画
        }
        transform.parent = targetArea; // 设置父对象

        SpriteRenderer pieceSpriteRenderer = GetComponent<SpriteRenderer>();
        if (pieceSpriteRenderer != null && _targetAreaSprite != null)
        {
            pieceSpriteRenderer.sortingOrder = _targetAreaSprite.sortingOrder + 1; // 更新渲染顺序
        }

        PuzzleGroup puzzleGroupScript = FindObjectOfType<PuzzleGroup>();
        if (puzzleGroupScript != null)
        {
            puzzleGroupScript.dragObjects += score;
            puzzleGroupScript.score += score;  
        }
        else
        {
            Debug.LogWarning("PuzzleGroup script not found in the scene.");
        }
            
        DetectNeighborsAndTriggerAnimation(); // 检测邻居并触发动画

        isLocked = true; // 标记为已锁定
        _isDragging = false; // 确保拖拽状态也关闭

        // if (_targetAreaSprite != null)
        // {
        //     _targetAreaSprite.enabled = false; // 隐藏目标区域提示
        // }

        if (sphereMaskController != null)
        {
            sphereMaskController.transform.position = targetArea.position;
             // 如果 sphereMaskController 有完成动画，也可以在这里触发
            Animator sphereAnimator = sphereMaskController.GetComponent<Animator>();
            if (sphereAnimator != null)
            {
                sphereAnimator.SetTrigger("Arrive"); // 假设 "Arrive" 是合适的触发器
            }
        }
        else
        {
            Debug.LogWarning("SphereMaskController script not found in the scene.");
        }

        // 播放完成音效
        AudioManager.Instance.PlaySound("放下", transform.position); // 使用"放下"音效或一个专门的"完成"音效

        // 可以选择禁用脚本或Collider，但仅设置isLocked通常已足够阻止交互
        // this.enabled = false;
        // GetComponent<Collider>().enabled = false;
    }

    // 初始化
    void Start()
    {
        sphereMaskController = FindObjectOfType<SphereMaskController>();
       
        _startPosition = transform.position;
        transform.localScale = new Vector3(StartScale, StartScale, StartScale); // 设置初始Scale
        animator = GetComponent<Animator>();
        if (targetArea != null)
        {
            _targetAreaSprite = targetArea.GetComponent<SpriteRenderer>();
        }
        //RefreshTargetSpriteVisibility(); // 设置初始可见性
    }

    // 每帧更新
    void Update()
    {
        // RefreshTargetSpriteVisibility(); // 持续更新可见性 // 已被移出
    }

    // 新增：固定更新，用于物理相关的更新，比如拖拽
    void FixedUpdate()
    {
        if (_isDragging)
        {
            Vector3 currentMouse_ScreenPos = Input.mousePosition;
            Vector3 currentMouse_WorldPos_AtPieceDepth = Camera.main.ScreenToWorldPoint(new Vector3(currentMouse_ScreenPos.x, currentMouse_ScreenPos.y, mouseConversionZ));

            Vector3 mouseDelta_World = currentMouse_WorldPos_AtPieceDepth - mouseWorldPosOnDragBegin;

            float targetBaseX = pieceWorldPosOnDragBegin.x + mouseDelta_World.x;
            float targetBaseZ = pieceWorldPosOnDragBegin.z + mouseDelta_World.z;
            // Y的初始点是 pieceWorldPosOnDragBegin.y 加上（平滑过渡的）偏移，然后跟随鼠标Y的改变量
            float targetBaseY = (pieceWorldPosOnDragBegin.y + _currentApplyingOffsetY) + mouseDelta_World.y;

            float additionalUpwardOffset = 0f;
            if (mouseDelta_World.y > 0) // 如果鼠标向上移动
            {
                additionalUpwardOffset = mouseDelta_World.y * dragFactor;
            }

            //Lerp移动
            Vector3 targetDragPosition = new Vector3(targetBaseX, targetBaseY + additionalUpwardOffset, targetBaseZ);
            transform.position = Vector3.Lerp(transform.position, targetDragPosition, Time.fixedDeltaTime * dragSmoothSpeed);
            //直接移动
            // transform.position = new Vector3(targetBaseX, targetBaseY + additionalUpwardOffset, targetBaseZ); 
            RefreshTargetSpriteVisibility(); // 拖拽时也需要刷新，确保吸附提示正确显示
        }
    }

    // 鼠标按下时触发
    void OnMouseDown()
    {
        if (isLocked) // 新增：如果已锁定，则不允许拖动
        {
            return;
        }

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

        // 移除立即应用的固定向上偏移，改为平滑过渡
        // transform.position = new Vector3(transform.position.x, pieceWorldPosOnDragBegin.y + offsetOnClick, transform.position.z);
        _currentApplyingOffsetY = 0f; // 重置当前Y偏移
        StartCoroutine(AnimateOffsetYValue(0f, offsetOnClick, scaleSmoothSpeed)); // 开始平滑Y偏移过渡

        // transform.localScale = new Vector3(DragScale, DragScale, DragScale); // 设置拖拽时的Scale -> 改为协程
        StartCoroutine(ChangeScaleOverTime(new Vector3(DragScale, DragScale, DragScale), scaleSmoothSpeed));
        RefreshTargetSpriteVisibility(); // 开始拖拽时刷新
        print("拖拽");
        AudioManager.Instance.PlaySound("抓起",transform.position);
        //sphereMaskController.transform.position = transform.position;
       
    }

    // 鼠标抬起时触发
    void OnMouseUp()
    {
        if (!_isDragging) // 新增：如果不是正在拖拽状态，则直接返回
        {
            return;
        }
        RefreshTargetSpriteVisibility(); // 拖拽结束时刷新

        _isDragging = false;

        // 新增：在启动新的移动协程前，也确保停止任何可能存在的旧协程（以防万一）
        if (_moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
            _moveCoroutine = null;
        }

        if (Vector3.Distance(transform.position, targetArea.position) <= snapDistance)
        {
            _moveCoroutine = StartCoroutine(MoveToPosition(targetArea.position, true)); // 移动到目标位置并吸附
        }
        else
        {
            AudioManager.Instance.PlaySound("返回", transform.position); // 在开始返回时播放音效
            _moveCoroutine = StartCoroutine(MoveToPosition(_startPosition, false)); // 移动回初始位置
            StartCoroutine(ChangeScaleOverTime(new Vector3(StartScale, StartScale, StartScale), scaleSmoothSpeed)); // 立刻开始向StartScale过渡
        }
    }

    /// <summary>
    /// 协程：移动拼图到指定位置。
    /// </summary>
    /// <param name="targetPosition">目标位置</param>
    /// <param name="isSnap">是否吸附到目标点（触发后续逻辑）</param>
    private IEnumerator MoveToPosition(Vector3 targetPosition, bool isSnap)
    {
        float currentSpeed = isSnap ? snapSpeed : returnSpeed; // 根据是否吸附选择速度

        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, currentSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPosition;
        // transform.localScale = new Vector3(StartScale, StartScale, StartScale); // 协程结束，恢复Scale -> 移到 else 分支
        
        if (isSnap)
        {
            if (animator != null)
            {
                animator.SetTrigger("SelectBlink");
            }
            transform.parent = targetArea; // 移动到这里：只有成功吸附才设置父物体
            // 根据目标点的SpriteRenderer设置当前拼图块的Order in Layer
            SpriteRenderer pieceSpriteRenderer = GetComponent<SpriteRenderer>();
            if (pieceSpriteRenderer != null)
            {
                if (_targetAreaSprite != null)
                {
                    pieceSpriteRenderer.sortingOrder = _targetAreaSprite.sortingOrder + 1;
                }
               
            }

            // 新增逻辑：寻找 PuzzleGroup 实例并更新计数，然后禁用当前拼图
            PuzzleGroup puzzleGroupScript = FindObjectOfType<PuzzleGroup>();
            if (puzzleGroupScript != null)
            {
                puzzleGroupScript.dragObjects += score;
                puzzleGroupScript.score += score; 
            }
            else
            {
                Debug.LogWarning("PuzzleGroup script not found in the scene.");
            }
            
            DetectNeighborsAndTriggerAnimation(); // 检测并触发邻居的动画

            // 禁用当前拼图块脚本
            //this.enabled = false;
            //this.GetComponent<Collider>().enabled = false; // 旧的禁用 Collider 的代码
            isLocked = true; // 修改：标记为已锁定

            if (sphereMaskController != null)
            {
                sphereMaskController.transform.position = targetArea.position;
            }
            else
            {
                Debug.LogWarning("SphereMaskController script not found in the scene.");
            }

            //音效
            AudioManager.Instance.PlaySound("放下",transform.position);
        }

        else
        {
            if (animator != null)
            {
                //animator.SetTrigger("Reset");
                print("回弹");
            }
            // StartCoroutine(ChangeScaleOverTime(new Vector3(StartScale, StartScale, StartScale), scaleSmoothSpeed)); // 移至 OnMouseUp
            // AudioManager.Instance.PlaySound("返回", transform.position); // 从此处移除
        }
        _moveCoroutine = null; // 协程结束，清空引用
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

    // 新增：协程，用于平滑改变当前的Y轴偏移值
    private IEnumerator AnimateOffsetYValue(float startOffset, float endOffset, float speed)
    {
        float journey = 0f;
        while (journey <= 1.0f)
        {
            journey += Time.deltaTime * speed;
            _currentApplyingOffsetY = Mathf.Lerp(startOffset, endOffset, journey);
            yield return null;
        }
        _currentApplyingOffsetY = endOffset; //  确保最终设置为目标值
    }

    // 新增：当父对象移动后，调用此方法来更新拼图块的初始位置记录
    public void UpdateBaseStartPosition()
    {
        // 仅当拼图块未被锁定且未被拖拽时，才更新其起始位置
        // 这是为了确保拖拽中的拼图块如果未吸附，会返回到它被拿起时的位置，
        // 而不是父对象移动后的新位置。
        // 对于未被拖拽且未锁定的拼图块，它们的"家"位置确实改变了。
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
                sphereMaskController.GetComponent<Animator>().SetTrigger("Arrive");
            }
        }
    }

    // 刷新目标区域Sprite的可见性
    private void RefreshTargetSpriteVisibility()
    {
        if (targetArea != null && _targetAreaSprite != null)
        {
            // 判断当前物体是否在目标区域的吸附范围内
            bool isInRange = Vector3.Distance(transform.position, targetArea.position) <= snapDistance;

            // 仅当拖拽时或者拼图块未锁定时，才根据距离显示目标区域
            // 如果拼图块已锁定，则不应该再显示其目标区域，即使它在范围内（比如父物体移动导致）
            if ((_isDragging || !isLocked) && isInRange)
            {
                _targetAreaSprite.enabled = true;
            }
            else
            {
                _targetAreaSprite.enabled = false;
            }
        }
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