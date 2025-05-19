using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [System.Serializable]
    public class Sound
    {
        public string name;                // 音效名称
        public AudioClip clip;             // 音效资源
        [Range(0f, 1f)]
        public float volume = 1f;          // 默认音量
        [Range(0f, 10f)]
        public float minInterval;     // 最小播放间隔（秒）
        public bool enableSpatialSound ;   // 是否开启立体声效果
        public float maxDistance = 50f;    // 声音最大可听距离（仅在立体声模式下生效）
        public float minDistance = 1f;     // 声音不衰减的最小距离（仅在立体声模式下生效）

        [HideInInspector]
        public float lastPlayedTime = -Mathf.Infinity; // 上次播放时间
    }

    public Sound[] sounds;

    private Dictionary<string, Sound> soundDictionary;

    private void Awake()
    {
        // Implement Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        soundDictionary = new Dictionary<string, Sound>();
        foreach (Sound sound in sounds)
        {
            soundDictionary[sound.name] = sound;
        }
    }

    /// <summary>
    /// 在指定的位置播放音效 （支持开启/关闭立体声效果）
    /// </summary>
    /// <param name="name">音效名称</param>
    /// <param name="position">世界坐标位置（仅在立体声模式下生效）</param>
    public void PlaySound(string name, Vector3 position)
    {
        if (soundDictionary.TryGetValue(name, out Sound sound))
        {
            float currentTime = Time.time;

            // 检查是否满足最小播放间隔
            if (currentTime - sound.lastPlayedTime >= sound.minInterval)
            {
                // 创建一个临时AudioSource用于播放音效
                GameObject tempAudioSourceObj = new GameObject("TempAudioSource_" + name);
                tempAudioSourceObj.transform.position = position;

                AudioSource audioSource = tempAudioSourceObj.AddComponent<AudioSource>();
                audioSource.clip = sound.clip;
                audioSource.volume = sound.volume;

                if (sound.enableSpatialSound)
                {
                    // 配置3D音效参数以开启立体声效果
                    audioSource.spatialBlend = 1f;                          // 完全使用3D音效
                    audioSource.minDistance = sound.minDistance;           // 最大音量的最小距离
                    audioSource.maxDistance = sound.maxDistance;           // 音效完全听不到的最大距离
                    audioSource.rolloffMode = AudioRolloffMode.Linear;     // 设置为线性衰减（也可以选择其他模式）
                }
                else
                {
                    // 关闭立体声效果，使用2D音效
                    audioSource.spatialBlend = 0f;                          // 完全使用2D音效
                }

                audioSource.Play();
                Destroy(tempAudioSourceObj, sound.clip.length);           // 播放结束销毁对象

                sound.lastPlayedTime = currentTime;                       // 更新上次播放时间
            }
            else
            {
                Debug.Log($"Sound '{name}' skipped due to min interval.");
            }
        }
        else
        {
            Debug.LogWarning("Sound not found: " + name);
        }
    }

    public void SetVolume(string name, float volume)
    {
        if (soundDictionary.TryGetValue(name, out Sound sound))
        {
            sound.volume = Mathf.Clamp01(volume);
        }
        else
        {
            Debug.LogWarning("Sound not found: " + name);
        }
    }

    public void ToggleSpatialSound(string name, bool enable)
    {
        if (soundDictionary.TryGetValue(name, out Sound sound))
        {
            sound.enableSpatialSound = enable;
            Debug.Log($"Sound '{name}' spatial sound set to: {enable}");
        }
        else
        {
            Debug.LogWarning("Sound not found: " + name);
        }
    }

    public void SetUISound(string soundName)
    {
        /*Time.timeScale = 1;
        Instance.PlaySound(soundName,transform.position);*/

    }
}