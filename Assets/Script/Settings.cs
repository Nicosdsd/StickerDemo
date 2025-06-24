using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class Settings : MonoBehaviour
{
    // 移除单例
    //[Header("倒计时设置")]
    public Text countdownText; // 用于显示倒计时的UI Text
    public GameObject timeoutPanel; // 时间到时要显示的UI Panel
    public float countdownDuration = 60f; // 倒计时总时长（秒）

    [Header("分数设置")]
    public Text scoreText;
    public int currentScore = 0;
    public int targetScore = 9;

    private float currentTime;
    private bool isTiming = false;

    public Image fillImage;

    private float currentFillAmount = 0f;
    private float fillAmountVelocity = 0f; // 用于SmoothDamp

    void Awake()
    {
        // 移除单例相关逻辑
    }

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
        // Re-find UI elements in the newly loaded scene.
        // This is a simple way, assuming the UI Text components are active.
        Text[] texts = FindObjectsOfType<Text>(true);
        foreach (var t in texts)
        {
            if (t.gameObject.name == countdownText.gameObject.name)
            {
                countdownText = t;
            }
            else if (t.gameObject.name == scoreText.gameObject.name)
            {
                scoreText = t;
            }
        }

        // Also re-find the timeout panel.
        if (timeoutPanel != null)
        {
            // This is a simplified search. A more robust solution might use tags.
            GameObject newTimeoutPanel = GameObject.Find(timeoutPanel.name);
            if(newTimeoutPanel != null)
            {
                timeoutPanel = newTimeoutPanel;
                timeoutPanel.SetActive(false); // Ensure it's hidden on load
            }
        }

        // Reset the timer when a scene is loaded/reloaded
        currentTime = countdownDuration;
        isTiming = true;
    }

    void Start()
    {
        // The logic from Start is moved to OnSceneLoaded to handle scene reloads correctly.
        // We only need to ensure the panel is hidden on the very first start.
        if (timeoutPanel != null)
        {
            timeoutPanel.SetActive(false);
        }
    }

    void Update()
    {
        //倒计时
        if (isTiming)
        {
            currentTime -= Time.deltaTime;

            // 更新显示的文本
            if (countdownText != null)
            {
                // 向上取整以获得更自然的倒计时显示
                float secondsToDisplay = Mathf.Ceil(currentTime);
                if (secondsToDisplay < 0)
                {
                    secondsToDisplay = 0;
                }

                // 使用TimeSpan来格式化时间为 mm:ss
                TimeSpan timeSpan = TimeSpan.FromSeconds(secondsToDisplay);
                countdownText.text = "剩余时间:" + timeSpan.ToString(@"mm\:ss");
            }

            // 检查时间是否结束
            if (currentTime <= 0)
            {
                currentTime = 0;
                isTiming = false;
                
                // 显示超时面板
                if (timeoutPanel != null)
                {
                    timeoutPanel.SetActive(true);
                }

                // 游戏暂停
                PauseGame();
                Debug.Log("时间到！");
            }
        }

        if (scoreText != null)
        {
            scoreText.text = " " + currentScore; //分数
        }
        // 使用SmoothDamp平滑fillImage的fillAmount
        if (fillImage != null && targetScore > 0)
        {
            float targetFill = (float)currentScore / targetScore;
            currentFillAmount = Mathf.SmoothDamp(currentFillAmount, targetFill, ref fillAmountVelocity, 0.15f); // 0.15f为缓动时间，可调整
            fillImage.fillAmount = currentFillAmount;
        }
    }

    // 调用此方法来重置场景
    public void ResetScene()
    {
        // 获取当前场景的名字
        string currentSceneName = SceneManager.GetActiveScene().name;
        // 使用场景管理器重新加载该场景
        SceneManager.LoadScene(currentSceneName);
    }

    // 调用此方法来暂停游戏
    public void PauseGame()
    {
        Time.timeScale = 0f; // 将时间尺度设置为0来暂停游戏
    }

    // 调用此方法来继续游戏
    public void ResumeGame()
    {
        Time.timeScale = 1f; // 将时间尺度设置为1来继续游戏
    }

    // 新增：一键完成所有拼图的后门方法
    public void CompleteAllPuzzles()
    {
        PuzzlePiece[] allPieces = FindObjectsOfType<PuzzlePiece>();
        foreach (PuzzlePiece piece in allPieces)
        {
            if (piece != null && !piece.IsLocked && piece.tag == "Puzzle") // 确保piece不为null且未被锁定
            {
                piece.ForceComplete();
            }
        }
        Debug.Log("All puzzles completed via backdoor!");
    }
}