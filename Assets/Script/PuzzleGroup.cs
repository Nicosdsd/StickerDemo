using UnityEngine;
using System.Collections.Generic; // 新增：为了使用 List
using UnityEngine.Playables; // 新增：为了使用 PlayableDirector
using System.Collections; // 新增：为了使用 Coroutine

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

    public int dragObjects; //
    public int score;
    public int totalScore;
    public float finishTime = 1;
    
    public Transform cameraPos; // 用于控制的摄像机移动
    public PlayableDirector finishTimeline; // 新增：完成时播放的Timeline
    
    public List<ScoreCameraMapping> scoreCameraMappings; // 新增：存储分数与摄像机位置的映射
    public float cameraMoveSpeed = 5f; // 新增：控制摄像机移动速度

    private bool allPiecesArrivedTriggered = false; // 新增：标记是否已触发所有拼图块的Arrive动画

    // 新增：用于管理镜头移动协程的字段
    private Coroutine _cameraMovementCoroutine;
    private Vector3 _cameraTargetForCoroutine = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue); // 初始化一个不会匹配任何实际位置的值

    void Start()
    {
        // 初始化目标位置为当前对象位置
        puzzleGroupInitialPosition = transform.position; // 重命名: initialPosition
        puzzleGroupTargetPosition = transform.position; // 重命名: targetPosition
    }
   

    void Update()
    {
        // 1. Camera Movement Logic with Delay
        if (cameraPos != null && scoreCameraMappings != null && scoreCameraMappings.Count > 0)
        {
            Vector3 determinedTargetCameraPos = cameraPos.position; 
            bool mappingFoundForCurrentScore = false;

            foreach (var mapping in scoreCameraMappings)
            {
                if (score == mapping.scoreValue)
                {
                    determinedTargetCameraPos = mapping.cameraPosition;
                    mappingFoundForCurrentScore = true;
                    break;
                }
            }

            if (mappingFoundForCurrentScore)
            {
                bool triggerNewCoroutine = false;
                if (_cameraMovementCoroutine == null) // No coroutine currently running
                {
                    // If not at the target for the current score
                    if (Vector3.Distance(cameraPos.position, determinedTargetCameraPos) > 0.01f)
                    {
                        triggerNewCoroutine = true;
                    }
                }
                else // A coroutine is already running
                {
                    // If the running coroutine is for a DIFFERENT target than the current score's target
                    if (determinedTargetCameraPos != _cameraTargetForCoroutine)
                    {
                        triggerNewCoroutine = true;
                    }
                }

                if (triggerNewCoroutine)
                {
                    if (_cameraMovementCoroutine != null)
                    {
                        StopCoroutine(_cameraMovementCoroutine);
                    }
                    _cameraTargetForCoroutine = determinedTargetCameraPos; 
                    _cameraMovementCoroutine = StartCoroutine(MoveCameraWithDelayCoroutine(determinedTargetCameraPos, finishTime));
                }
            }
        }

        // 2. PuzzleGroup Movement Logic
        // PuzzleGroup should only move if dragObjects == 3, it's not already moving, AND the camera coroutine is NOT active.
        bool isCameraBusy = _cameraMovementCoroutine != null;
        if (dragObjects == 3 && !isPuzzleGroupMoving && !isCameraBusy)
        {
            puzzleGroupInitialPosition = transform.position; 
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
            PlayAnimation(); // 直接调用，移除了Invoke和finishTime
            allPiecesArrivedTriggered = true; // 标记已触发，防止重复执行
            Debug.Log("完成了"); 
        }
    }

    IEnumerator MoveCameraWithDelayCoroutine(Vector3 targetPosition, float delay)
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(delay);

        // Move the camera to the target position
        while (Vector3.Distance(cameraPos.position, targetPosition) > 0.01f)
        {
            cameraPos.position = Vector3.Lerp(cameraPos.position, targetPosition, Time.deltaTime * cameraMoveSpeed);
            yield return null; // Wait for the next frame
        }
        cameraPos.position = targetPosition; // Ensure it reaches the exact target position

        // Coroutine is finished
        _cameraMovementCoroutine = null;
    }

    void PlayAnimation()
    {
       
        Invoke("FinishBlink", 2.5f);
       

        // 新增：播放Timeline动画
        if (finishTimeline != null)
        {
            finishTimeline.Play();
        }
        else
        {
            Debug.LogWarning("finishTimeline is not assigned in the Inspector.");
        }

        // // 新增：修改摄像机投影大小
        // if (cameraPos != null)
        // {
        //     Camera mainCamera = cameraPos.GetComponent<Camera>();
        //     if (mainCamera != null && mainCamera.orthographic)
        //     {
        //         mainCamera.orthographicSize = 13f;
        //     }
        //     else if (mainCamera != null && !mainCamera.orthographic)
        //     {
        //         Debug.LogWarning("Camera is not orthographic. Projection size not changed for perspective camera.");
        //     }
        //     else
        //     {
        //         Debug.LogWarning("Camera component not found on cameraPos.");
        //     }
        // }
        // else
        // {
        //     Debug.LogWarning("cameraPos is not assigned in the Inspector.");
        // }


      
    }

    void FinishBlink()
    {
        AudioManager.Instance.PlaySound("完成", transform.position); 
        PuzzlePiece[] pieces = FindObjectsOfType<PuzzlePiece>();
        foreach (PuzzlePiece piece in pieces)
        {
            Animator pieceAnimator = piece.GetComponent<Animator>();
            print(pieceAnimator.name);
            if (pieceAnimator != null)
            {
                pieceAnimator.SetTrigger("Blink");
            }
        }
    }
}