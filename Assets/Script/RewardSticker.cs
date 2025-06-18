using UnityEngine;
using UnityEngine.Playables;

public class RewardSticker : MonoBehaviour
{
    public int score = 0;
    public int targetScore = 5;
    private PlayableDirector rewardTimeline;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 自动获取当前物体上的PlayableDirector组件
        rewardTimeline = GetComponent<PlayableDirector>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddScore(int value)
    {
        score += value;
        if (score >= targetScore)
        {
            Debug.Log("RewardSticker 达到目标分数！");
            // 播放Timeline动画
            if (rewardTimeline != null)
            {
                rewardTimeline.Play();
            }
            else
            {
                Debug.LogWarning("未绑定PlayableDirector！");
            }
        }
    }
}
