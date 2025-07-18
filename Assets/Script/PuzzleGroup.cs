using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Playables;

///用来控制拼图组，包括拼图的初始化、拼图的随机分配、拼图的复位和随机化。


public class PuzzleGroup : MonoBehaviour
{
    public Transform target1;
    public Transform target2;
    public Transform target3;

    public Transform startGroup;
    public Transform endGroup;

    public PlayableDirector timelineDirector;
    private bool hasTimelinePlayed = false;

    private int targetPieceCount;
    
    [Header("拼图数量设置")]
    public Text pieceCountText; // 用于显示拼图数量的UI Text
    private int remainingPieces; // 剩余拼图数量

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 重新查找UI元素
        Text[] texts = FindObjectsOfType<Text>(true);
        foreach (var t in texts)
        {
            if (pieceCountText != null && t.gameObject.name == pieceCountText.gameObject.name)
            {
                pieceCountText = t;
                break;
            }
        }
        
        // 重新初始化拼图数量
        targetPieceCount = transform.childCount;
        remainingPieces = targetPieceCount;
        
        Debug.Log($"场景重载后PuzzleGroup初始化：总拼图数量 = {targetPieceCount}");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 根据子物体数量自动设置targetPieceCount
        targetPieceCount = transform.childCount;
        remainingPieces = targetPieceCount;
        
        Debug.Log($"PuzzleGroup初始化：总拼图数量 = {targetPieceCount}");
        
        if (startGroup != null && endGroup != null)
        {
            if (startGroup.childCount != endGroup.childCount)
            {
                Debug.LogError("startGroup 和 endGroup 的子物体数量必须相同。");
                return;
            }

            for (int i = 0; i < startGroup.childCount; i++)
            {
                PuzzlePiece puzzlePiece = startGroup.GetChild(i).GetComponent<PuzzlePiece>();
                if (puzzlePiece != null)
                {
                    puzzlePiece.targetArea = endGroup.GetChild(i);
                }

                // 新增：为endGroup的TargetTrigger赋值puzzlePiece
                PuzzleTarget puzzleTarget = endGroup.GetChild(i).GetComponent<PuzzleTarget>();
                if (puzzleTarget != null)
                {
                    puzzleTarget.puzzlePiece = puzzlePiece;
                }
            }
        }

        // 保证每个目标初始都有拼图
        if (target1 != null && target1.childCount == 0) AssignRandomChildToTarget(target1);
        if (target2 != null && target2.childCount == 0) AssignRandomChildToTarget(target2);
        if (target3 != null && target3.childCount == 0) AssignRandomChildToTarget(target3);
    }

    // Update is called once per frame
    void Update()
    {
        // 更新拼图数量UI
        if (pieceCountText != null)
        {
            pieceCountText.text = "" + remainingPieces + "块";
        }
        
        // 新逻辑：哪个目标没拼图就补哪个，始终保证3个目标有拼图
        if (target1 != null && target1.childCount == 0)
        {
            AssignRandomChildToTarget(target1);
        }
        if (target2 != null && target2.childCount == 0)
        {
            AssignRandomChildToTarget(target2);
        }
        if (target3 != null && target3.childCount == 0)
        {
            AssignRandomChildToTarget(target3);
        }
    }

    // 新增：减少拼图数量的方法
    public void DecreasePieceCount()
    {
        remainingPieces--;
        if (remainingPieces <= 0)
        {
            remainingPieces = 0;
            // TODO: 通关逻辑，比如弹窗、暂停等
            Debug.Log("全部拼图完成！");
        }
    }

    public void AssignRandomChildToTarget(Transform target)
    {
        if (target == null) return;

        // 收集所有未分配的拼图块
        List<Transform> allPieces = new List<Transform>();
        foreach (Transform child in transform)
        {
            allPieces.Add(child);
        }

        // 找到当前最小的priority
        int minPriority = int.MaxValue;
        foreach (Transform t in allPieces)
        {
            PuzzlePiece piece = t.GetComponent<PuzzlePiece>();
            if (piece != null && piece.priority < minPriority)
            {
                minPriority = piece.priority;
            }
        }

        // 只选取priority等于minPriority的拼图块
        List<Transform> candidatePieces = new List<Transform>();
        foreach (Transform t in allPieces)
        {
            PuzzlePiece piece = t.GetComponent<PuzzlePiece>();
            if (piece != null && piece.priority == minPriority)
            {
                candidatePieces.Add(t);
            }
        }

        if (candidatePieces.Count > 0)
        {
            System.Random rng = new System.Random();
            var selected = candidatePieces[rng.Next(candidatePieces.Count)];
            selected.SetParent(target);
            selected.localPosition = Vector3.zero;
            Debug.Log($"已为目标{target.name}分配一个优先级为{minPriority}的拼图。");
        }
    }

    public void ResetAndRandomizeTargets()
    {
        System.Action<Transform> returnChildrenToPool = (target) =>
        {
            if (target == null) return;

            // Create a temporary list of children to move
            List<Transform> childrenToMove = new List<Transform>();
            foreach (Transform child in target)
            {
                childrenToMove.Add(child);
            }

            // Move each child back to the parent transform (the pool)
            foreach (Transform child in childrenToMove)
            {
                child.SetParent(transform);
                child.localPosition = Vector3.zero; // Optional: Reset position
            }
        };

        returnChildrenToPool(target1);
        returnChildrenToPool(target2);
        returnChildrenToPool(target3);

        // Now, with all pieces back in the pool, we can assign new random ones.
        AssignRandomChildToTarget(target1);
        AssignRandomChildToTarget(target2);
        AssignRandomChildToTarget(target3);
    }
}
