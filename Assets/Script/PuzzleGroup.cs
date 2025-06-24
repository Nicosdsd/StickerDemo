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

    }

    // Update is called once per frame
    void Update()
    {
       
        if (settings != null && settings.currentScore >= settings.targetScore && !hasTimelinePlayed && timelineDirector != null)
        {
            timelineDirector.Play();
            hasTimelinePlayed = true;
            //AudioManager.Instance.PlaySound("完成",transform.position);
            Debug.Log("目标分数已达到，播放 Timeline 动画。");
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

        // 新增：判断三个目标下的子物体是否都为空
        if (target1 != null && target2 != null && target3 != null)
        {
            if (target1.childCount == 0 && target2.childCount == 0 && target3.childCount == 0)
            {
                AssignRandomChildrenToTargets();
            }
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

    public void AssignRandomChildrenToTargets()
    {
        if (target1 == null || target2 == null || target3 == null)
        {
            Debug.LogError("请在 Inspector 中分配所有三个目标 Transform。");
            return;
        }

        refreshCount++;

        List<Transform> regularPieces = new List<Transform>();
        foreach (Transform child in transform)
        {
            regularPieces.Add(child);
        }

        List<Transform> piecesToAssign = new List<Transform>();
        System.Random rng = new System.Random();

        float currentPropChance = GetCurrentPropChance();
        bool includeProp = UnityEngine.Random.value < currentPropChance && propPiecePool.Count > 0 && regularPieces.Count >= 2;

        if (includeProp)
        {
            // 选出1个道具块和2个普通块
            int propIndex = rng.Next(propPiecePool.Count);
            Transform prop = propPiecePool.Find(p => p.parent == PropGroup);
            if(prop != null)
            {
                piecesToAssign.Add(prop);

                var shuffledRegular = regularPieces.OrderBy(a => rng.Next()).ToList();
                for (int i = 0; i < 2; i++)
                {
                    piecesToAssign.Add(shuffledRegular[i]);
                }
            }
            else
            {
                includeProp = false; // 没有可用的道具块
            }
        }
        
        if(!includeProp)
        {
            // 选出3个普通块
            if (regularPieces.Count >= 3)
            {
                var shuffledRegular = regularPieces.OrderBy(a => rng.Next()).ToList();
                for (int i = 0; i < 3; i++)
                {
                    piecesToAssign.Add(shuffledRegular[i]);
                }
            }
            else
            {
                piecesToAssign.AddRange(regularPieces);
                Debug.LogWarning("普通拼图块不足3个。");
            }
        }


        if (piecesToAssign.Count == 0)
        {
            return;
        }

        // 随机打乱待选列表
        List<Transform> shuffledPieces = piecesToAssign.OrderBy(a => rng.Next()).ToList();

        // 目标列表
        Transform[] targets = new Transform[] { target1, target2, target3 };

        // 分配
        for (int i = 0; i < Mathf.Min(targets.Length, shuffledPieces.Count); i++)
        {
            shuffledPieces[i].SetParent(targets[i]);
            shuffledPieces[i].localPosition = Vector3.zero;
        }

        Debug.Log($"已成功将{Mathf.Min(targets.Length, shuffledPieces.Count)}个随机子物体分配到目标位置。");
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
        AssignRandomChildrenToTargets();
    }
}
