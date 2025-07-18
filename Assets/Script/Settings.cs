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

    // 新增：生命值系统
    [Header("生命值设置")]
    public int maxHealth = 3; // 最大生命值
    public Text healthText; // 用于显示生命值的UI Text
    public GameObject gameOverPanel; // 生命值为0时要显示的UI Panel
    
    private int currentHealth; // 当前生命值
    private float currentTime;
    private bool isTiming = false;

    void Awake()
    {
        // 移除单例相关逻辑
        // 初始化生命值
        currentHealth = maxHealth;
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
            // 新增：查找生命值UI Text
            if (healthText != null && t.gameObject.name == healthText.gameObject.name)
            {
                healthText = t;
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

        // 新增：查找游戏结束面板
        if (gameOverPanel != null)
        {
            GameObject newGameOverPanel = GameObject.Find(gameOverPanel.name);
            if(newGameOverPanel != null)
            {
                gameOverPanel = newGameOverPanel;
                gameOverPanel.SetActive(false); // Ensure it's hidden on load
            }
        }

        // Reset the timer when a scene is loaded/reloaded
        currentTime = countdownDuration;
        isTiming = true;
        
        // 重置生命值
        currentHealth = maxHealth;
        UpdateHealthDisplay();
    }

    void Start()
    {
        // The logic from Start is moved to OnSceneLoaded to handle scene reloads correctly.
        // We only need to ensure the panel is hidden on the very first start.
        if (timeoutPanel != null)
        {
            timeoutPanel.SetActive(false);
        }
        
        // 新增：确保游戏结束面板隐藏
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        // 初始化生命值显示
        UpdateHealthDisplay();
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
    }

    // 新增：更新生命值显示
    private void UpdateHealthDisplay()
    {
        if (healthText != null)
        {
            healthText.text = currentHealth.ToString();
        }
    }

    // 新增：减少生命值
    public void DecreaseHealth()
    {
        if (currentHealth > 0)
        {
            currentHealth--;
            UpdateHealthDisplay();
            
            Debug.Log("生命值减少，当前生命值: " + currentHealth);
            
            // 检查是否生命值为0
            if (currentHealth <= 0)
            {
                GameOver();
            }
        }
    }

    // 新增：游戏结束
    private void GameOver()
    {
        isTiming = false; // 停止倒计时
        
        // 显示游戏结束面板
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        
        // 游戏暂停
        PauseGame();
        Debug.Log("游戏结束！生命值为0");
    }

    // 调用此方法来重置场景
    public void ResetScene()
    {
        // 获取当前场景的名字
        string currentSceneName = SceneManager.GetActiveScene().name;
        // 使用场景管理器重新加载该场景
        SceneManager.LoadScene(currentSceneName);
        ResumeGame();
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