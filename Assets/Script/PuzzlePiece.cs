using UnityEngine;
using System.Collections;
using UnityEngine.Playables;
using Lattice;
using HighlightPlus;

public class PuzzlePiece : MonoBehaviour
{
    public Vector3 _startPosition; // 拼图块的初始位置
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

    // 新增：成功吸附后要播放动画的Animator
    public Animator successAnimator;

    private Vector3 mouseWorldPosOnDragBegin; // 拖拽开始时鼠标的世界坐标（在拼图块深度）
    private Vector3 pieceWorldPosOnDragBegin; // 拖拽开始时拼图块的世界坐标
    private float mouseConversionZ; // 鼠标屏幕坐标到世界坐标转换时使用的Z深度


    public bool isLocked = false; // 新增：标记拼图块是否已锁定
    private Coroutine _moveCoroutine; // 新增：用于跟踪移动协程
    private float _currentApplyingOffsetY = 0f; // 新增：用于平滑处理offsetOnClick的当前Y偏移值
    private DragCenter dragCenter;
    private LatticeModifier _latticeModifier; // 新增：LatticeModifier组件的引用

    private AudioSource currentLoopingSound; // 当前正在播放的循环音效

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
        transform.localScale = new Vector3(DragScale, DragScale, DragScale); // 确保Scale设置为DragScale

        // 新增：为子物体的MeshRenderer添加Light Layer2
        //AddLightLayer2ToChildMeshRenderers();
        
        // 新增：播放成功吸附动画
        PlaySuccessAnimation();



        transform.parent = targetArea; // 设置父对象


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

       
        // 播放完成音效
        AudioManager.Instance.PlaySound("放下", transform.position); // 使用"放下"音效或一个专门的"完成"音效



        // 新增：成功放下后显示LatticeModifier组件
        if (_latticeModifier != null)
        {
            _latticeModifier.enabled = true;
        }
    }

    // 初始化
    void Start()
    {
        _startPosition = transform.position;
        transform.localScale = new Vector3(StartScale, StartScale, StartScale); // 设置初始Scale
        animator = GetComponent<Animator>();
       

        // 新增：获取LatticeModifier组件并默认隐藏
        _latticeModifier = GetComponentInChildren<LatticeModifier>(true); // true表示也查找非激活的子对象
        if (_latticeModifier != null)
        {
            _latticeModifier.enabled = false;
        }

        dragCenter = FindObjectOfType<DragCenter>();

    }

    // 每帧更新
    void Update()
    {

        //TargetVisibility();
      
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

        }
    }

    // 鼠标按下时触发
    void OnMouseDown()
    {
        UpdateBaseStartPosition(); // 拖拽前更新初始位置记录


        if (isLocked) // 新增：如果已锁定，则不允许拖动
        {
            //AudioManager.Instance.PlaySound("按压",transform.position);
             // 播放循环音效
            // if (AudioManager.Instance != null && !string.IsNullOrEmpty("按压"))
            // {
            //     if (currentLoopingSound != null) // 如果已有音效在播放，先停止
            //     {
            //         AudioManager.Instance.StopLoopingSound(currentLoopingSound);
            //     }
            //     currentLoopingSound = AudioManager.Instance.PlayLoopingSound("按压", transform.position); 
            // }

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

        print("拖拽");
        AudioManager.Instance.PlaySound("抓起",transform.position);
        //GetComponent<Collider>().enabled = false;
        //GetComponent<HighlightEffect>().enabled = true;
        //dragCenter.enabled = false;

    }

    // 鼠标抬起时触发
    void OnMouseUp()
    {

        // 停止循环音效
        // if (currentLoopingSound != null && AudioManager.Instance != null)
        // {
        //     AudioManager.Instance.StopLoopingSound(currentLoopingSound);
        //     currentLoopingSound = null;
        // }

        //dragCenter.enabled = true;

        //GetComponent<HighlightEffect>().enabled = false;
       
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
           //成功吸附
           print("成功吸附");

            // 新增：为子物体的MeshRenderer添加Light Layer2
            AddLightLayer2ToChildMeshRenderers();
            
            // 新增：播放成功吸附动画
            PlaySuccessAnimation();

            transform.parent = targetArea; // 移动到这里：只有成功吸附才设置父物体
            // 根据目标点的SpriteRenderer设置当前拼图块的Order in Layer
          

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
        
            isLocked = true; // 修改：标记为已锁定

            // 播放完成音效
            AudioManager.Instance.PlaySound("放下",transform.position);

            GetComponent<Collider>().enabled = true;

            FindObjectOfType<PuzzleGroup2>().currentCount++;
            FindObjectOfType<PuzzleGroup2>().currentScore += 1;

            //_latticeModifier.enabled = true;
           
        }

        else
        {
            GetComponent<Collider>().enabled = true;
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
        // 获取所有子物体的MeshRenderer组件
        MeshRenderer[] childMeshRenderers = GetComponentsInChildren<MeshRenderer>();
        
        foreach (MeshRenderer meshRenderer in childMeshRenderers)
        {
            // Light Layer2 对应第2位 (从0开始计数)，即 1 << 2 = 4
            int lightLayer2Mask = 1 << 2;
            
            // 将Light Layer2添加到现有的Rendering Layer Mask中
            meshRenderer.renderingLayerMask |= (uint)lightLayer2Mask;

        }
    }

    // 新增：为子物体的MeshRenderer移除Light Layer2
    private void RemoveLightLayer2FromChildMeshRenderers()
    {
        // 获取所有子物体的MeshRenderer组件
        MeshRenderer[] childMeshRenderers = GetComponentsInChildren<MeshRenderer>();
        
        foreach (MeshRenderer meshRenderer in childMeshRenderers)
        {
            // Light Layer2 对应第2位 (从0开始计数)，即 1 << 2 = 4
            int lightLayer2Mask = 1 << 2;
            
            // 从现有的Rendering Layer Mask中移除Light Layer2
            meshRenderer.renderingLayerMask &= ~(uint)lightLayer2Mask;
        }
    }

    // 新增：播放成功吸附动画
    public void PlaySuccessAnimation()
    {
         AddLightLayer2ToChildMeshRenderers();
        if (successAnimator != null)
        {
            print("播放成功动画");
            successAnimator.SetTrigger("Success");
             
             // 启动协程，0.5秒后移除Light Layer2
             StartCoroutine(RemoveLightLayerAfterDelay(1f));
        }
       
    }

    // 新增：协程，延迟指定时间后移除Light Layer2
    private IEnumerator RemoveLightLayerAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        RemoveLightLayer2FromChildMeshRenderers();
    }

    // 
    // private void TargetVisibility()
    // {
      
    //      // 判断当前物体是否在目标区域的吸附范围内
    //     bool isInRange = Vector3.Distance(transform.position, targetArea.position) <= snapDistance;

    //     if (isInRange && !isLocked && _isDragging)
    //     {
    //         print("在范围内");
    //        // targetArea.GetComponent<HighlightEffect>().enabled = true;
    //     }
    //     else
    //     {
    //         //targetArea.GetComponent<HighlightEffect>().enabled = false;
    //     }
    // }

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