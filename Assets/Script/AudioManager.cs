using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public float masterVolume = 1f; // 总音量控制

    [System.Serializable]
    public class Sound
    {
        public string name;                // 音效名称
        public AudioClip clip;             // 音效资源
        [Range(0f, 5f)]
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
                audioSource.volume = sound.volume * masterVolume; // 应用总音量

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

    /// <summary>
    /// 播放循环音效，并返回创建的 AudioSource
    /// </summary>
    /// <param name="name">音效名称</param>
    /// <param name="position">世界坐标位置（仅在立体声模式下生效）</param>
    /// <returns>创建的 AudioSource，如果音效未找到或无法播放则返回 null</returns>
    public AudioSource PlayLoopingSound(string name, Vector3 position)
    {
        if (soundDictionary.TryGetValue(name, out Sound sound))
        {
            // 检查是否满足最小播放间隔 (对于循环音效，通常第一次播放时检查)
            // 如果不希望循环音效受最小间隔限制，可以移除或调整此逻辑
            float currentTime = Time.time;
            if (currentTime - sound.lastPlayedTime < sound.minInterval)
            {
                Debug.Log($"Looping sound '{name}' start skipped due to min interval. Playing immediately.");
                // 或者选择不跳过，直接播放
            }

            GameObject tempAudioSourceObj = new GameObject("LoopingAudioSource_" + name);
            tempAudioSourceObj.transform.position = position;

            AudioSource audioSource = tempAudioSourceObj.AddComponent<AudioSource>();
            audioSource.clip = sound.clip;
            audioSource.volume = sound.volume * masterVolume;
            audioSource.loop = true; // 设置为循环播放

            if (sound.enableSpatialSound)
            {
                audioSource.spatialBlend = 1f;
                audioSource.minDistance = sound.minDistance;
                audioSource.maxDistance = sound.maxDistance;
                audioSource.rolloffMode = AudioRolloffMode.Linear;
            }
            else
            {
                audioSource.spatialBlend = 0f;
            }

            audioSource.Play();
            sound.lastPlayedTime = currentTime; // 更新上次播放时间（主要影响非循环音效的间隔）
            return audioSource;
        }
        else
        {
            Debug.LogWarning("Looping sound not found: " + name);
            return null;
        }
    }

    /// <summary>
    /// 停止指定的 AudioSource 播放并销毁其 GameObject
    /// </summary>
    /// <param name="audioSourceToStop">要停止的 AudioSource</param>
    public void StopLoopingSound(AudioSource audioSourceToStop)
    {
        if (audioSourceToStop != null)
        {
            audioSourceToStop.Stop();
            Destroy(audioSourceToStop.gameObject);
        }
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
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
        // 考虑是否需要为UI声音提供一个专用的非循环播放方法，
        // 或者 PlaySound 已经满足需求。
        // 如果是循环的背景UI音，可以使用PlayLoopingSound
        // 如果是点击等一次性音效，PlaySound更合适
        // 此处暂时保留注释，根据具体需求决定如何实现SetUISound
        PlaySound(soundName, Camera.main != null ? Camera.main.transform.position : Vector3.zero); // 播放UI音效，位置可以设为摄像机位置或者(0,0,0)
    }
}