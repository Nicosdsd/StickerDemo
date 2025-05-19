using UnityEngine;

public class PuzzleGroup : MonoBehaviour
{
    // 固定的移动距离，可以在Inspector中调整
    public float moveDistance = 1.0f;

    // 控制移动速度
    public float moveTime = 1.0f;

    private Vector3 targetPosition;
    private Vector3 initialPosition; // 添加 initialPosition 用于 Lerp
    private bool isMoving = false;
    private float elapsedTime = 0f;
    public int dragObjects; //
    public int score;
    public int totalScore;
    public float finishTimel = 1;
    public Animator sphereMaskAni;

    private bool allPiecesArrivedTriggered = false; // 新增：标记是否已触发所有拼图块的Arrive动画

    void Start()
    {
        // 初始化目标位置为当前对象位置
        initialPosition = transform.position; // 初始化 initialPosition
        targetPosition = transform.position;
    }
   

    void Update()
    {
        // 如果 dragObjects 等于 3 并且当前没有在移动，则开始移动
        if (dragObjects == 3 && !isMoving)
        {
            initialPosition = transform.position; // 每次开始移动前，更新初始位置
            targetPosition = transform.position + Vector3.up * moveDistance;
            isMoving = true;
            elapsedTime = 0f; // 重置 elapsedTime
        }

        // 如果当前正在移动
        if (isMoving)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / moveTime);
            transform.position = Vector3.Lerp(initialPosition, targetPosition, progress); // 使用 initialPosition 作为 Lerp 的起点

            // 当移动完成时，停止移动
            if (progress >= 1f)
            {
                transform.position = targetPosition; //确保精确到达目标位置
                isMoving = false;
                elapsedTime = 0f;
                dragObjects = 0; // 移动完成后将 dragObjects 归0

                // 更新所有子拼图块的起始位置参考点
                PuzzlePiece[] pieces = FindObjectsOfType<PuzzlePiece>();
                foreach (PuzzlePiece piece in pieces)
                {
                    // 调用 PuzzlePiece 上的方法来更新其 _startPosition
                    // PuzzlePiece 内部会处理是否真的需要更新（例如，如果它正在被拖动或已锁定，则不更新）
                    piece.UpdateBaseStartPosition();
                }
            }
        }

        // 新增逻辑：当score等于totalScore时，触发所有PuzzlePiece的Arrive动画
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