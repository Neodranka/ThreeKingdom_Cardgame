using UnityEngine;
using UnityEngine.SceneManagement;

public class GameAudioManager : MonoBehaviour
{
    public static GameAudioManager Instance;

    private AudioSource bgmSource;
    private AudioSource sfxSource;

    public AudioClip mainMenuBGM;
    public AudioClip gameBGM;
    public string mainMenuSceneName = "MainMenu";

    public AudioClip cardDrawSFX;
    public AudioClip cardPlaySFX;
    public AudioClip attackSFX;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.volume = 0.5f;

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.volume = 0.7f;

            SceneManager.sceneLoaded += OnSceneLoaded;

            // 自动添加 Audio Listener
            EnsureAudioListener();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 场景加载后检查 Audio Listener
        EnsureAudioListener();

        if (scene.name == mainMenuSceneName && mainMenuBGM != null)
        {
            bgmSource.clip = mainMenuBGM;
            bgmSource.Play();
        }
        else if (gameBGM != null)
        {
            bgmSource.clip = gameBGM;
            bgmSource.Play();
        }
    }

    /// <summary>
    /// 确保场景中有 Audio Listener
    /// </summary>
    void EnsureAudioListener()
    {
        // 检查场景中是否已有 Audio Listener
        AudioListener listener = FindObjectOfType<AudioListener>();

        if (listener == null)
        {
            // 如果没有，尝试添加到 Main Camera
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.gameObject.AddComponent<AudioListener>();
                Debug.Log("[GameAudioManager] 已自动添加 Audio Listener 到 Main Camera");
            }
            else
            {
                // 如果没有相机，添加到 AudioManager 自己身上
                gameObject.AddComponent<AudioListener>();
                Debug.Log("[GameAudioManager] 已自动添加 Audio Listener 到 AudioManager");
            }
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}