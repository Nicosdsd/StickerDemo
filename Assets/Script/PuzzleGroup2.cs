using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Playables;

public class PuzzleGroup2 : MonoBehaviour
{
    public Transform target1;
    public Transform target2;
    public Transform target3;

    public GameObject targetClickObject;

    public int currentCount = 0;
    public int targetCount = 3;
    public int currentScore = 0;
    public int targetScore = 9;

    public PlayableDirector timelineDirector;
    private bool hasTimelinePlayed = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentCount = 3;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentCount >= targetCount)
        {
            AssignRandomChildrenToTargets();
            currentCount = 0;
        }

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

        if (children.Count < 3)
        {
            Debug.LogError("子物体数量不足3个，无法执行分配。");
            return;
        }

        // 随机打乱子物体列表
        System.Random rng = new System.Random();
        List<Transform> shuffledChildren = children.OrderBy(a => rng.Next()).ToList();

        // 选择前三个子物体并分配给目标
        shuffledChildren[0].SetParent(target1);
        shuffledChildren[0].localPosition = Vector3.zero; // 可选：重置局部位置

        shuffledChildren[1].SetParent(target2);
        shuffledChildren[1].localPosition = Vector3.zero; // 可选：重置局部位置

        shuffledChildren[2].SetParent(target3);
        shuffledChildren[2].localPosition = Vector3.zero; // 可选：重置局部位置
        
        Debug.Log("已成功将3个随机子物体分配到目标位置。");
    }

   
}
