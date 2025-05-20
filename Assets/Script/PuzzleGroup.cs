using UnityEngine;
using System.Collections.Generic; // 新增：为了使用 List

[System.Serializable] // 新增：使该类可以在Inspector中编辑
public class ScoreCameraMapping
{
    public int scoreValue;
    public Vector3 cameraPosition;
}

public class PuzzleGroup : MonoBehaviour
{
    // 固定的移动距离，可以在Inspector中调整
    public float moveDistance = 1.0f;

    // 控制移动速度
    public float moveTime = 1.0f;

    private Vector3 puzzleGroupTargetPosition; // 重命名: targetPosition
    private Vector3 puzzleGroupInitialPosition; // 重命名: initialPosition
    private bool isPuzzleGroupMoving = false; // 重命名: isMoving
    private float puzzleGroupElapsedTime = 0f; // 重命名: elapsedTime
    private bool isCameraActuallyMoving = false; // 新增：跟踪摄像机是否因分数逻辑而实际移动

    public int dragObjects; //
    public int score;
    public int totalScore;
    public float finishTimel = 1;
    public Animator sphereMaskAni;
    public Transform cameraPos; // 用于控制的摄像机移动
    
    public List<ScoreCameraMapping> scoreCameraMappings; // 新增：存储分数与摄像机位置的映射
    public float cameraMoveSpeed = 5f; // 新增：控制摄像机移动速度

    private bool allPiecesArrivedTriggered = false; // 新增：标记是否已触发所有拼图块的Arrive动画

    void Start()
    {
        // 初始化目标位置为当前对象位置
        puzzleGroupInitialPosition = transform.position; // 重命名: initialPosition
        puzzleGroupTargetPosition = transform.position; // 重命名: targetPosition
    }
   

    void Update()
    {
        // 1. Camera Movement Logic
        isCameraActuallyMoving = false; // 每帧开始时，假定摄像机没有主动移动
        if (cameraPos != null && scoreCameraMappings != null && scoreCameraMappings.Count > 0)
        {
            bool foundScoreMappingThisFrame = false;
            Vector3 targetCameraPosForCurrentScore = cameraPos.position;

            foreach (var mapping in scoreCameraMappings)
            {
                if (score == mapping.scoreValue)
                {
                    targetCameraPosForCurrentScore = mapping.cameraPosition;
                    foundScoreMappingThisFrame = true;
                    break;
                }
            }

            if (foundScoreMappingThisFrame)
            {
                // 如果摄像机当前位置与目标位置有明显差异
                if (Vector3.Distance(cameraPos.position, targetCameraPosForCurrentScore) > 0.01f)
                {
                    cameraPos.position = Vector3.Lerp(cameraPos.position, targetCameraPosForCurrentScore, Time.deltaTime * cameraMoveSpeed);
                    isCameraActuallyMoving = true; // 标记摄像机正在主动移动
                }
                else
                {
                    // 已经很接近目标或已到达，确保精确位置并停止标记移动
                    if (Vector3.Distance(cameraPos.position, targetCameraPosForCurrentScore) != 0f)
                    {
                        cameraPos.position = targetCameraPosForCurrentScore; // 精确到达目标位置
                    }
                    // isCameraActuallyMoving 保持 false，因为已到达或无需移动
                }
            }
            // 如果没有找到当前分数对应的映射，isCameraActuallyMoving 保持 false
        }

        // 2. PuzzleGroup Movement Logic
        // 如果 dragObjects 等于 3, PuzzleGroup 当前没有在移动, 并且摄像机当前也没有在移动，则开始移动 PuzzleGroup
        if (dragObjects == 3 && !isPuzzleGroupMoving && !isCameraActuallyMoving)
        {
            puzzleGroupInitialPosition = transform.position; // 每次开始移动前，更新初始位置（相对于父物体 cameraPos）
            puzzleGroupTargetPosition = transform.position + Vector3.up * moveDistance;
            isPuzzleGroupMoving = true;
            puzzleGroupElapsedTime = 0f;
        }

        // 如果 PuzzleGroup 当前正在移动
        if (isPuzzleGroupMoving)
        {
            puzzleGroupElapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(puzzleGroupElapsedTime / moveTime);
            transform.position = Vector3.Lerp(puzzleGroupInitialPosition, puzzleGroupTargetPosition, progress);

            // 当移动完成时，停止移动
            if (progress >= 1f)
            {
                transform.position = puzzleGroupTargetPosition; //确保精确到达目标位置
                isPuzzleGroupMoving = false;
                puzzleGroupElapsedTime = 0f; // 重置 elapsedTime
                dragObjects = 0; // 移动完成后将 dragObjects 归0

                // 更新所有子拼图块的起始位置参考点
                PuzzlePiece[] pieces = FindObjectsOfType<PuzzlePiece>();
                foreach (PuzzlePiece piece in pieces)
                {
                    piece.UpdateBaseStartPosition();
                }
            }
        }

        // 3. Score/TotalScore animation trigger (Original - unchanged)
        // 当score等于totalScore时，触发所有PuzzlePiece的Arrive动画
        if (score == totalScore && !allPiecesArrivedTriggered && totalScore > 0) // 确保totalScore有效
        {
            Invoke("PlayAnimation", finishTimel);
            allPiecesArrivedTriggered = true; // 标记已触发，防止重复执行
            Debug.Log("完成了"); 
        }
    }


    void PlayAnimation()
    {
         AudioManager.Instance.PlaySound("完成", transform.position); 

        sphereMaskAni.transform.position = new Vector3(0, 0, 0);
        sphereMaskAni.SetTrigger("Finish");

        PuzzlePiece[] pieces = FindObjectsOfType<PuzzlePiece>();
        foreach (PuzzlePiece piece in pieces)
        {
            Animator pieceAnimator = piece.GetComponent<Animator>();
            print(pieceAnimator.name);
            if (pieceAnimator != null)
            {
                pieceAnimator.SetTrigger("SelectBlink");
            }
        }

    }
}