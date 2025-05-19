using UnityEngine;
using UnityEngine.UI;

public class FPSDisplay : MonoBehaviour
{
    public Text fpsText; // UI Text ���������ʾ֡��.
    public float updateInterval = 1f; // ����֡����ʾ�������λΪ�룩.

    private int frameCount = 0;
    private float elapsedTime = 0f;

    public int targetFPS = 60;

    private void Start()
    {
        if (fpsText == null)
        {
            Debug.LogError("No Text component assigned to display the FPS.");
            this.enabled = false;
            return;
        }
        Application.targetFrameRate = targetFPS;
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;
        frameCount++;

        if (elapsedTime >= updateInterval)
        {
            float fps = frameCount / elapsedTime;
            fpsText.text = string.Format("FPS: {0}", Mathf.RoundToInt(fps));

            elapsedTime = 0f;
            frameCount = 0;
        }
    }
}