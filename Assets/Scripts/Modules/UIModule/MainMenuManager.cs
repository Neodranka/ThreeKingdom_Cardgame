using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ThreeKingdoms.UI
{
    /// <summary>
    /// 主菜单管理器
    /// 负责处理主菜单的按钮事件和场景切换
    /// </summary>
    public class MainMenuManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button battleModeButton;
        [SerializeField] private Button storyModeButton;
        [SerializeField] private Button settingsButton;

        private void Start()
        {
            // 绑定按钮事件
            if (battleModeButton != null)
                battleModeButton.onClick.AddListener(OnBattleModeClicked);

            if (storyModeButton != null)
                storyModeButton.onClick.AddListener(OnStoryModeClicked);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClicked);

            Debug.Log("主菜单初始化完成");
        }

        /// <summary>
        /// 进入对战模式（先进入游戏准备场景）
        /// </summary>
        private void OnBattleModeClicked()
        {
            Debug.Log("进入游戏准备");
            // 加载游戏准备场景
            SceneManager.LoadScene("GameSetup");
        }

        /// <summary>
        /// 进入故事模式（暂未实现）
        /// </summary>
        private void OnStoryModeClicked()
        {
            Debug.Log("故事模式开发中...");
            // TODO: 之后实现故事模式
            // SceneManager.LoadScene("StoryMode");

            // 暂时显示提示（可选）
            ShowComingSoonMessage("故事模式开发中，敬请期待！");
        }

        /// <summary>
        /// 打开设置界面（暂未实现）
        /// </summary>
        private void OnSettingsClicked()
        {
            Debug.Log("设置功能开发中...");
            // TODO: 之后实现设置面板
            // 可以打开一个设置UI面板，包括音量、画质等设置

            ShowComingSoonMessage("设置功能开发中，敬请期待！");
        }

        /// <summary>
        /// 显示"开发中"提示信息
        /// </summary>
        private void ShowComingSoonMessage(string message)
        {
            // 这里可以显示一个简单的提示UI
            // 暂时只在Console输出
            Debug.Log(message);
        }

        private void OnDestroy()
        {
            // 清理事件监听，防止内存泄漏
            if (battleModeButton != null)
                battleModeButton.onClick.RemoveListener(OnBattleModeClicked);

            if (storyModeButton != null)
                storyModeButton.onClick.RemoveListener(OnStoryModeClicked);

            if (settingsButton != null)
                settingsButton.onClick.RemoveListener(OnSettingsClicked);
        }
    }
}