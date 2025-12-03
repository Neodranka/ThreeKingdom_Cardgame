using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace ThreeKingdomsKill.UI
{
    /// <summary>
    /// 主菜单管理器
    /// 负责处理主菜单的按钮事件和场景切换
    /// ⭐ 支持本地化和语言切换
    /// </summary>
    public class MainMenuManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button battleModeButton;
        [SerializeField] private Button storyModeButton;
        [SerializeField] private Button settingsButton;

        // ⭐ 按钮文本引用
        private TextMeshProUGUI battleModeText;
        private TextMeshProUGUI storyModeText;
        private TextMeshProUGUI settingsText;

        private void Start()
        {
            // 获取按钮文本组件
            GetButtonTextComponents();

            // 绑定按钮事件
            if (battleModeButton != null)
                battleModeButton.onClick.AddListener(OnBattleModeClicked);

            if (storyModeButton != null)
                storyModeButton.onClick.AddListener(OnStoryModeClicked);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClicked);

            // ⭐ 初始化UI文本
            RefreshUIText();

            // ⭐ 监听语言切换事件
            if (ThreeKingdoms.LocalizationManager.Instance != null)
            {
                ThreeKingdoms.LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;
                Debug.Log("[MainMenu] 已监听语言切换事件");
            }
            else
            {
                Debug.LogWarning("[MainMenu] LocalizationManager未找到！请确保场景中有LocalizationManager对象");
            }

            Debug.Log("主菜单初始化完成");
        }

        /// <summary>
        /// ⭐ 获取按钮文本组件
        /// </summary>
        private void GetButtonTextComponents()
        {
            if (battleModeButton != null)
                battleModeText = battleModeButton.GetComponentInChildren<TextMeshProUGUI>();

            if (storyModeButton != null)
                storyModeText = storyModeButton.GetComponentInChildren<TextMeshProUGUI>();

            if (settingsButton != null)
                settingsText = settingsButton.GetComponentInChildren<TextMeshProUGUI>();
        }

        /// <summary>
        /// ⭐ 语言切换回调
        /// </summary>
        private void OnLanguageChanged(ThreeKingdoms.Language newLanguage)
        {
            Debug.Log($"[MainMenu] 检测到语言切换: {newLanguage}");
            RefreshUIText();
        }

        /// <summary>
        /// ⭐ 刷新UI文本
        /// </summary>
        private void RefreshUIText()
        {
            if (ThreeKingdoms.LocalizationManager.Instance == null)
            {
                Debug.LogWarning("[MainMenu] LocalizationManager为null，无法刷新UI");
                return;
            }

            Debug.Log("[MainMenu] 开始刷新UI文本...");

            // 更新按钮文本
            UpdateButtonText(battleModeButton, battleModeText, "ui_battle_mode");
            UpdateButtonText(storyModeButton, storyModeText, "ui_story_mode");
            UpdateButtonText(settingsButton, settingsText, "ui_settings");

            Debug.Log("[MainMenu] UI文本刷新完成");
        }

        /// <summary>
        /// ⭐ 更新按钮文本
        /// </summary>
        private void UpdateButtonText(Button button, TextMeshProUGUI text, string localizationKey)
        {
            if (button == null || text == null)
            {
                Debug.LogWarning($"[MainMenu] 按钮或文本为null: {localizationKey}");
                return;
            }

            // 获取本地化文本
            string localizedText = ThreeKingdoms.LocalizationManager.Instance.GetText(localizationKey);
            text.text = localizedText;

            // ⭐ 根据语言设置字体
            ThreeKingdoms.Language currentLang = ThreeKingdoms.LocalizationManager.Instance.GetCurrentLanguage();
            ThreeKingdoms.UI.TMPFontHelper.SetFontByLanguage(text);

            Debug.Log($"[MainMenu] 更新按钮 [{localizationKey}] -> \"{localizedText}\" (语言: {currentLang})");
        }

        /// <summary>
        /// 进入对战模式
        /// </summary>
        private void OnBattleModeClicked()
        {
            Debug.Log("进入对战模式");
            // 加载游戏准备场景
            SceneManager.LoadScene("GameSetup");
        }

        /// <summary>
        /// 进入故事模式（暂未实现）
        /// </summary>
        private void OnStoryModeClicked()
        {
            Debug.Log("故事模式开发中...");
            ShowComingSoonMessage("故事模式开发中，敬请期待！");
        }

        /// <summary>
        /// 打开设置界面（暂未实现）
        /// </summary>
        private void OnSettingsClicked()
        {
            Debug.Log("设置功能开发中...");
            ShowComingSoonMessage("设置功能开发中，敬请期待！");
        }

        /// <summary>
        /// 显示"开发中"提示信息
        /// </summary>
        private void ShowComingSoonMessage(string message)
        {
            Debug.Log(message);
            // TODO: 显示UI提示框
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

            // ⭐ 取消监听语言切换
            if (ThreeKingdoms.LocalizationManager.Instance != null)
            {
                ThreeKingdoms.LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;
                Debug.Log("[MainMenu] 已取消监听语言切换事件");
            }
        }
    }
}