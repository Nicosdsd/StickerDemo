using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Playables;

[System.Serializable]
public struct PropChanceStep
{
    public int refreshThreshold; // 从第几次刷新开始
    [Range(0, 1)]
    public float chance;       // 道具出现的概率
}

public class PuzzleGroup : MonoBehaviour
{
    public Transform target1;
    public Transform target2;
    public Transform target3;

    public Transform startGroup;
    public Transform endGroup;
    public Transform PropGroup;

    public GameObject targetClickObject;

    public List<PropChanceStep> propChanceProgression = new List<PropChanceStep>
    {
        new PropChanceStep { refreshThreshold = 1, chance = 1.0f },
        new PropChanceStep { refreshThreshold = 2, chance = 0.5f },
        new PropChanceStep { refreshThreshold = 8, chance = 0.1f }
    };

    public PlayableDirector timelineDirector;
    private bool hasTimelinePlayed = false;

    private List<Transform> propPiecePool = new List<Transform>();
    private int refreshCount = 0;

    public Settings settings;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (settings == null)
        {
            settings = FindObjectOfType<Settings>();
            if (settings == null)
            {
                Debug.LogError("未找到 Settings 组件，请在 Inspector 中赋值或确保场景中有 Settings 脚本。");
            }
        }
        
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

        if (PropGroup != null)
        {
            foreach (Transform prop in PropGroup)
            {
                propPiecePool.Add(prop);
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
       
        if (settings != null && settings.remainingPieces <= 0 && !hasTimelinePlayed && timelineDirector != null)
        {
            timelineDirector.Play();
            hasTimelinePlayed = true;
            //AudioManager.Instance.PlaySound("完成",transform.position);
            Debug.Log("全部拼图完成，播放 Timeline 动画。");
        }

        if (Input.GetMouseButtonDown(0) && targetClickObject != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.gameObject == targetClickObject)
                {
                    AudioManager.Instance.PlaySound("按压", transform.position);
                }
            }
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

    float GetCurrentPropChance()
    {
        // 对列表按刷新阈值降序排序
        propChanceProgression.Sort((a, b) => b.refreshThreshold.CompareTo(a.refreshThreshold));
        
        foreach (var step in propChanceProgression)
        {
            if (refreshCount >= step.refreshThreshold)
            {
                return step.chance;
            }
        }
        
        return 0f; // 默认概率为0
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
                if (propPiecePool.Contains(child))
                {
                    child.SetParent(PropGroup);
                }
                else
                {
                    child.SetParent(transform);
                }
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
