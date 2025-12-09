using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;
    public AudioSource bgmSource;

    [Header("默认音乐")]
    public AudioClip defaultBGM;

    [Header("战斗音乐")]
    public AudioClip battleBGM;  // 👈 新增:在Inspector中拖拽战斗音乐

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            bgmSource = GetComponent<AudioSource>();

            if (defaultBGM != null)
            {
                PlayBGM(defaultBGM);
            }

            // 👇 监听场景切换
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    // 👇 场景加载时切换音乐
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"场景加载: {scene.name}");

        // 如果是GameScene,播放战斗音乐
        if (scene.name == "GameScene" && battleBGM != null)
        {
            PlayBGM(battleBGM);
        }
        // 回到主菜单,播放默认音乐
        else if (scene.name == "MainMenu" && defaultBGM != null)
        {
            PlayBGM(defaultBGM);
        }
    }

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("音频片段为空!");
            return;
        }

        // 如果正在播放相同音乐,不重复播放
        if (bgmSource.clip == clip && bgmSource.isPlaying)
        {
            Debug.Log($"已在播放 {clip.name}");
            return;
        }

        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();

        Debug.Log($"开始播放音乐: {clip.name}");
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}