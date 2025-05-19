using UnityEngine;
using UnityEngine.SceneManagement;

public class Settings : MonoBehaviour
{
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
            if (piece != null && !piece.IsLocked) // 确保piece不为null且未被锁定
            {
                piece.ForceComplete();
            }
        }
        Debug.Log("All puzzles completed via backdoor!");
    }
}