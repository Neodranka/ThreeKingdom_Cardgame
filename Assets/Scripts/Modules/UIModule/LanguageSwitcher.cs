using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace ThreeKingdoms.UI
{
    /// <summary>
    /// 语言切换器
    /// 提供简单的语言切换功能
    /// 支持下拉菜单和按钮两种方式
    /// </summary>
    public class LanguageSwitcher : MonoBehaviour
    {
        [Header("UI引用")]
        [SerializeField] private TMP_Dropdown languageDropdown;
        [SerializeField] private Button languageButton;
        [SerializeField] private TextMeshProUGUI currentLanguageText;

        [Header("选项")]
        [SerializeField] private bool useDropdown = true; // true=下拉菜单, false=循环按钮
        [SerializeField] private bool enableHotkey = true; // 是否启用快捷键
        [SerializeField] private KeyCode hotkeyCode = KeyCode.L; // 快捷键（默认L键）

        private void Start()
        {
            InitializeLanguageSwitcher();
        }

        private void OnEnable()
        {
            // ⭐ 监听LocalizationManager的语言切换事件
            // 先移除再添加，避免重复绑定
            if (ThreeKingdoms.LocalizationManager.Instance != null)
            {
                ThreeKingdoms.LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChangedFromManager;
                ThreeKingdoms.LocalizationManager.Instance.OnLanguageChanged += OnLanguageChangedFromManager;

                Debug.Log("[LanguageSwitcher] 已监听LocalizationManager事件");
            }
        }

        private void OnDisable()
        {
            // ⭐ 取消监听
            if (ThreeKingdoms.LocalizationManager.Instance != null)
            {
                ThreeKingdoms.LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChangedFromManager;
            }
        }

        /// <summary>
        /// ⭐ 初始化语言切换器
        /// </summary>
        private void InitializeLanguageSwitcher()
        {
            if (useDropdown && languageDropdown != null)
            {
                InitializeDropdown();
            }
            else if (!useDropdown && languageButton != null)
            {
                InitializeButton();
            }

            Debug.Log("[LanguageSwitcher] 语言切换器初始化完成");
        }

        /// <summary>
        /// ⭐ LocalizationManager语言切换回调（同步UI）
        /// </summary>
        private void OnLanguageChangedFromManager(ThreeKingdoms.Language newLanguage)
        {
            Debug.Log($"[LanguageSwitcher] 检测到语言切换: {newLanguage}");

            // 同步UI显示
            if (useDropdown && languageDropdown != null)
            {
                // 暂时移除监听，避免循环触发
                languageDropdown.onValueChanged.RemoveListener(OnLanguageChanged);
                languageDropdown.value = (int)newLanguage;
                languageDropdown.RefreshShownValue();
                languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
            }
            else if (!useDropdown && languageButton != null)
            {
                UpdateButtonText();
            }
        }

        private void Update()
        {
            // 快捷键切换语言
            if (enableHotkey && Input.GetKeyDown(hotkeyCode))
            {
                CycleToNextLanguage();
            }
        }

        /// <summary>
        /// 初始化下拉菜单方式
        /// </summary>
        private void InitializeDropdown()
        {
            if (languageDropdown == null)
            {
                Debug.LogWarning("[LanguageSwitcher] languageDropdown为null！");
                return;
            }

            // ⭐ 只移除OnLanguageChanged监听器，不用RemoveAllListeners()
            // RemoveAllListeners()会破坏dropdown的内部监听器
            languageDropdown.onValueChanged.RemoveListener(OnLanguageChanged);

            // 清空并添加语言选项
            languageDropdown.ClearOptions();

            List<string> options = new List<string>
            {
                "中文",
                "English",
                "한국어"
            };

            languageDropdown.AddOptions(options);

            // 设置当前语言
            if (ThreeKingdoms.LocalizationManager.Instance != null)
            {
                ThreeKingdoms.Language currentLang = ThreeKingdoms.LocalizationManager.Instance.GetCurrentLanguage();
                languageDropdown.value = (int)currentLang;
                languageDropdown.RefreshShownValue();

                Debug.Log($"[LanguageSwitcher] 设置Dropdown当前值: {currentLang} ({(int)currentLang})");
            }

            // ⭐ 添加新的监听器
            languageDropdown.onValueChanged.AddListener(OnLanguageChanged);

            Debug.Log("[LanguageSwitcher] 下拉菜单初始化完成");
        }

        /// <summary>
        /// 初始化按钮方式
        /// </summary>
        private void InitializeButton()
        {
            // 更新按钮文本
            UpdateButtonText();

            // 监听点击事件
            languageButton.onClick.AddListener(OnLanguageButtonClicked);

            Debug.Log("[LanguageSwitcher] 按钮初始化完成");
        }

        /// <summary>
        /// 下拉菜单切换语言
        /// </summary>
        private void OnLanguageChanged(int index)
        {
            ThreeKingdoms.Language selectedLanguage = (ThreeKingdoms.Language)index;

            if (ThreeKingdoms.LocalizationManager.Instance != null)
            {
                ThreeKingdoms.LocalizationManager.Instance.SetLanguage(selectedLanguage);
                Debug.Log($"[LanguageSwitcher] 语言已切换到：{selectedLanguage}");
            }
        }

        /// <summary>
        /// 按钮切换语言（循环切换）
        /// </summary>
        private void OnLanguageButtonClicked()
        {
            CycleToNextLanguage();
        }

        /// <summary>
        /// 循环切换到下一个语言
        /// </summary>
        private void CycleToNextLanguage()
        {
            if (ThreeKingdoms.LocalizationManager.Instance == null) return;

            // 获取当前语言
            ThreeKingdoms.Language currentLang = ThreeKingdoms.LocalizationManager.Instance.GetCurrentLanguage();

            // 循环切换到下一个语言
            ThreeKingdoms.Language nextLang;
            switch (currentLang)
            {
                case ThreeKingdoms.Language.Chinese:
                    nextLang = ThreeKingdoms.Language.English;
                    break;
                case ThreeKingdoms.Language.English:
                    nextLang = ThreeKingdoms.Language.Korean;
                    break;
                case ThreeKingdoms.Language.Korean:
                    nextLang = ThreeKingdoms.Language.Chinese;
                    break;
                default:
                    nextLang = ThreeKingdoms.Language.Chinese;
                    break;
            }

            // 切换语言
            ThreeKingdoms.LocalizationManager.Instance.SetLanguage(nextLang);

            // 如果是按钮模式，更新按钮文本
            if (!useDropdown && languageButton != null)
            {
                UpdateButtonText();
            }
            // 如果是下拉菜单模式，更新选中项
            else if (useDropdown && languageDropdown != null)
            {
                languageDropdown.value = (int)nextLang;
                languageDropdown.RefreshShownValue();
            }

            Debug.Log($"[LanguageSwitcher] 语言已切换到：{nextLang}");
        }

        /// <summary>
        /// 更新按钮文本
        /// </summary>
        private void UpdateButtonText()
        {
            if (currentLanguageText == null) return;
            if (ThreeKingdoms.LocalizationManager.Instance == null) return;

            ThreeKingdoms.Language currentLang = ThreeKingdoms.LocalizationManager.Instance.GetCurrentLanguage();

            switch (currentLang)
            {
                case ThreeKingdoms.Language.Chinese:
                    currentLanguageText.text = "中文";
                    break;
                case ThreeKingdoms.Language.English:
                    currentLanguageText.text = "EN";
                    break;
                case ThreeKingdoms.Language.Korean:
                    currentLanguageText.text = "한국어";
                    break;
            }

            // 设置字体
            TMPFontHelper.SetFontByLanguage(currentLanguageText);
        }

        /// <summary>
        /// 手动设置语言（供外部调用）
        /// </summary>
        public void SetLanguage(ThreeKingdoms.Language language)
        {
            if (ThreeKingdoms.LocalizationManager.Instance != null)
            {
                ThreeKingdoms.LocalizationManager.Instance.SetLanguage(language);

                // 更新UI
                if (useDropdown && languageDropdown != null)
                {
                    languageDropdown.value = (int)language;
                    languageDropdown.RefreshShownValue();
                }
                else if (!useDropdown && languageButton != null)
                {
                    UpdateButtonText();
                }
            }
        }

        /// <summary>
        /// 获取当前语言
        /// </summary>
        public ThreeKingdoms.Language GetCurrentLanguage()
        {
            if (ThreeKingdoms.LocalizationManager.Instance != null)
            {
                return ThreeKingdoms.LocalizationManager.Instance.GetCurrentLanguage();
            }
            return ThreeKingdoms.Language.Chinese;
        }

        private void OnDestroy()
        {
            // 清理Dropdown事件
            if (languageDropdown != null)
            {
                languageDropdown.onValueChanged.RemoveListener(OnLanguageChanged);
            }

            // 清理Button事件
            if (languageButton != null)
            {
                languageButton.onClick.RemoveListener(OnLanguageButtonClicked);
            }

            // ⭐ 清理LocalizationManager事件
            if (ThreeKingdoms.LocalizationManager.Instance != null)
            {
                ThreeKingdoms.LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChangedFromManager;
            }
        }
    }
}