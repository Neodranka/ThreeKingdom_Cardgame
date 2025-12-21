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
        [SerializeField] private Button exitButton;  // ⭐ 退出游戏按钮
        [SerializeField] private TextMeshProUGUI titleText; // ⭐ 游戏标题文本

        // ⭐ 按钮文本引用
        private TextMeshProUGUI battleModeText;
        private TextMeshProUGUI storyModeText;
        private TextMeshProUGUI exitModeText;

        private void Start()
        {
            // 获取按钮文本组件
            GetButtonTextComponents();

            // 绑定按钮事件
            if (battleModeButton != null)
                battleModeButton.onClick.AddListener(OnBattleModeClicked);

            if (storyModeButton != null)
                storyModeButton.onClick.AddListener(OnStoryModeClicked);

            if (exitButton != null)
                exitButton.onClick.AddListener(OnExitClicked);

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

            if (exitButton != null)
                exitModeText = exitButton.GetComponentInChildren<TextMeshProUGUI>();
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

            // ⭐ 更新标题文本
            if (titleText != null)
            {
                titleText.text = ThreeKingdoms.LocalizationManager.Instance.GetText("menu_title");
                ThreeKingdoms.UI.TMPFontHelper.SetFontByLanguage(titleText);
                Debug.Log($"[MainMenu] 更新标题 -> \"{titleText.text}\"");
            }

            // 更新按钮文本
            UpdateButtonText(battleModeButton, battleModeText, "ui_battle_mode");
            UpdateButtonText(storyModeButton, storyModeText, "ui_story_mode");
            UpdateButtonText(exitButton, exitModeText, "ui_exit_game");

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

            // ⭐ 清理故事模式的PlayerPrefs数据，防止误判
            ClearStoryModePlayerPrefs();

            // 加载游戏准备场景
            SceneManager.LoadScene("GameSetup");
        }

        /// <summary>
        /// ⭐ 清理故事模式相关的PlayerPrefs
        /// </summary>
        private void ClearStoryModePlayerPrefs()
        {
            PlayerPrefs.DeleteKey("StoryBattleId");
            PlayerPrefs.DeleteKey("StoryPlayerGeneral");
            PlayerPrefs.DeleteKey("StoryDifficulty");
            PlayerPrefs.Save();
            Debug.Log("[MainMenu] 已清理故事模式PlayerPrefs数据");
        }

        /// <summary>
        /// 进入故事模式
        /// </summary>
        private void OnStoryModeClicked()
        {
            Debug.Log("进入故事模式");
            // 加载故事模式场景
            SceneManager.LoadScene("StoryMode");
        }

        /// <summary>
        /// ⭐ 退出游戏
        /// </summary>
        private void OnExitClicked()
        {
            Debug.Log("退出游戏");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnDestroy()
        {
            // 清理事件监听，防止内存泄漏
            if (battleModeButton != null)
                battleModeButton.onClick.RemoveListener(OnBattleModeClicked);

            if (storyModeButton != null)
                storyModeButton.onClick.RemoveListener(OnStoryModeClicked);

            if (exitButton != null)
                exitButton.onClick.RemoveListener(OnExitClicked);

            // ⭐ 取消监听语言切换
            if (ThreeKingdoms.LocalizationManager.Instance != null)
            {
                ThreeKingdoms.LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;
                Debug.Log("[MainMenu] 已取消监听语言切换事件");
            }
        }
    }
}