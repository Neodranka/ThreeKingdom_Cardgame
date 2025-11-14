using UnityEngine;

/// <summary>
/// 游戏主管理器
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameManager 初始化成功！");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Debug.Log("三国杀游戏启动！");
    }
}