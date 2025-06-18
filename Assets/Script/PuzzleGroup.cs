using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Playables;

public class PuzzleGroup : MonoBehaviour
{
    public Transform target1;
    public Transform target2;
    public Transform target3;

    public Transform startGroup;
    public Transform endGroup;
    public Transform PropGroup;

    public GameObject targetClickObject;

    public int currentScore = 0;
    public int targetScore = 9;

    public PlayableDirector timelineDirector;
    private bool hasTimelinePlayed = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       
        
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

         if (PropGroup != null && startGroup != null)
        {
            List<Transform> children = new List<Transform>();
            foreach (Transform child in PropGroup)
            {
                children.Add(child);
            }
            foreach (Transform child in children)
            {
                child.SetParent(startGroup);
                child.localPosition = Vector3.zero; // 可选：重置位置
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
       
        if (currentScore == targetScore && !hasTimelinePlayed && timelineDirector != null)
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

   

    public void AssignRandomChildrenToTargets()
    {
        if (target1 == null || target2 == null || target3 == null)
        {
            Debug.LogError("请在 Inspector 中分配所有三个目标 Transform。");
            return;
        }

        List<Transform> children = new List<Transform>();
        foreach (Transform child in transform)
        {
            children.Add(child);
        }

        if (children.Count == 0)
        {
            //Debug.LogError("没有可分配的子物体。");
            return;
        }

        // 随机打乱子物体列表
        System.Random rng = new System.Random();
        List<Transform> shuffledChildren = children.OrderBy(a => rng.Next()).ToList();

        // 目标列表
        Transform[] targets = new Transform[] { target1, target2, target3 };

        // 分配
        for (int i = 0; i < Mathf.Min(3, shuffledChildren.Count); i++)
        {
            shuffledChildren[i].SetParent(targets[i]);
            shuffledChildren[i].localPosition = Vector3.zero;
        }

        Debug.Log($"已成功将{Mathf.Min(3, shuffledChildren.Count)}个随机子物体分配到目标位置。");
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
        AssignRandomChildrenToTargets();
    }
}
