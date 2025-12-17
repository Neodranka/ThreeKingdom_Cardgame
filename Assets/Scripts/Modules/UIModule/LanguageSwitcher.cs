using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
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
        [SerializeField] private TextMeshProUGUI languageLabelText; // ⭐ "语言"/"Language"标签

        [Header("选项")]
        [SerializeField] private bool useDropdown = true; // true=下拉菜单, false=循环按钮
        [SerializeField] private bool enableHotkey = true; // 是否启用快捷键
        [SerializeField] private KeyCode hotkeyCode = KeyCode.L; // 快捷键（默认L键）

        private bool isInitialized = false;
        private int frameCheckCount = 0; // ⭐ 用于限制LateUpdate检查次数

        private void Start()
        {
            // 使用协程延迟初始化，确保LocalizationManager已就绪
            StartCoroutine(DelayedInitialize());
        }

        private void OnEnable()
        {
            // ⭐ 重置帧检查计数器
            frameCheckCount = 0;

            // ⭐ 监听LocalizationManager的语言切换事件
            // 先移除再添加，避免重复绑定
            if (ThreeKingdoms.LocalizationManager.Instance != null)
            {
                ThreeKingdoms.LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChangedFromManager;
                ThreeKingdoms.LocalizationManager.Instance.OnLanguageChanged += OnLanguageChangedFromManager;

                Debug.Log("[LanguageSwitcher] 已监听LocalizationManager事件");

                // ⭐ 每次OnEnable都确保事件正确绑定
                if (useDropdown && languageDropdown != null)
                {
                    // ⭐ 检查选项是否正确
                    bool needsInit = languageDropdown.options.Count == 0 ||
                                    languageDropdown.options.Count != 3 ||
                                    (languageDropdown.options.Count > 0 && languageDropdown.options[0].text != "中文");

                    if (needsInit)
                    {
                        Debug.Log($"[LanguageSwitcher] OnEnable: Dropdown选项异常 (count={languageDropdown.options.Count}), 立即初始化");
                        InitializeDropdown();
                        isInitialized = true;
                    }
                    else
                    {
                        // ⭐ 即使选项正确，也要确保事件监听器已绑定
                        EnsureDropdownEventBound();
                    }
                }
            }
            else
            {
                Debug.Log("[LanguageSwitcher] OnEnable: LocalizationManager尚未就绪");
            }
        }

        /// <summary>
        /// ⭐ 确保Dropdown事件已绑定
        /// </summary>
        private void EnsureDropdownEventBound()
        {
            if (languageDropdown == null) return;

            // 移除再添加，确保只有一个监听器
            languageDropdown.onValueChanged.RemoveListener(OnLanguageChanged);
            languageDropdown.onValueChanged.AddListener(OnLanguageChanged);

            // 同步当前语言值
            if (ThreeKingdoms.LocalizationManager.Instance != null)
            {
                ThreeKingdoms.Language currentLang = ThreeKingdoms.LocalizationManager.Instance.GetCurrentLanguage();
                if (languageDropdown.value != (int)currentLang)
                {
                    languageDropdown.SetValueWithoutNotify((int)currentLang);
                    languageDropdown.RefreshShownValue();
                }
            }

            Debug.Log("[LanguageSwitcher] 已确保Dropdown事件绑定");
        }

        /// <summary>
        /// ⭐ 延迟初始化，等待LocalizationManager就绪
        /// </summary>
        private IEnumerator DelayedInitialize()
        {
            // 等待LocalizationManager实例可用
            float waitTime = 0f;
            while (ThreeKingdoms.LocalizationManager.Instance == null && waitTime < 2f)
            {
                yield return null;
                waitTime += Time.deltaTime;
            }

            if (ThreeKingdoms.LocalizationManager.Instance == null)
            {
                Debug.LogWarning("[LanguageSwitcher] LocalizationManager未找到，无法初始化语言切换器");
                yield break;
            }

            // 再等一帧确保LocalizationManager完全初始化
            yield return null;

            InitializeLanguageSwitcher();
            isInitialized = true;
        }

        /// <summary>
        /// ⭐ 刷新Dropdown状态（场景切换后调用）
        /// </summary>
        private void RefreshDropdown()
        {
            if (languageDropdown == null) return;
            if (ThreeKingdoms.LocalizationManager.Instance == null) return;

            // ⭐ 检查选项是否正确（检查第一个选项是否是"中文"）
            bool optionsValid = languageDropdown.options.Count == 3 &&
                               languageDropdown.options[0].text == "中文" &&
                               languageDropdown.options[1].text == "English" &&
                               languageDropdown.options[2].text == "한국어";

            if (!optionsValid)
            {
                Debug.Log($"[LanguageSwitcher] Dropdown选项异常 (count={languageDropdown.options.Count}), 重新初始化");
                InitializeDropdown();
            }
            else
            {
                // 只同步当前值
                ThreeKingdoms.Language currentLang = ThreeKingdoms.LocalizationManager.Instance.GetCurrentLanguage();
                languageDropdown.onValueChanged.RemoveAllListeners();
                languageDropdown.value = (int)currentLang;
                languageDropdown.RefreshShownValue();

                // ⭐ 更新Caption文本
                if (languageDropdown.captionText != null)
                {
                    languageDropdown.captionText.text = languageDropdown.options[(int)currentLang].text;
                }

                languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
                Debug.Log($"[LanguageSwitcher] Dropdown已同步: {currentLang}");
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
            // ⭐ 初始化语言标签
            UpdateLanguageLabel();

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

            // ⭐ 更新语言标签
            UpdateLanguageLabel();

            // 同步UI显示
            if (useDropdown && languageDropdown != null)
            {
                // ⭐ 确保选项正确
                if (languageDropdown.options.Count != 3 || languageDropdown.options[0].text != "中文")
                {
                    InitializeDropdown();
                    return;
                }

                // 暂时移除监听，避免循环触发
                languageDropdown.onValueChanged.RemoveAllListeners();
                languageDropdown.value = (int)newLanguage;
                languageDropdown.RefreshShownValue();

                // ⭐ 更新Caption文本
                if (languageDropdown.captionText != null && languageDropdown.options.Count > (int)newLanguage)
                {
                    languageDropdown.captionText.text = languageDropdown.options[(int)newLanguage].text;
                }

                languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
            }
            else if (!useDropdown && languageButton != null)
            {
                UpdateButtonText();
            }
        }

        /// <summary>
        /// ⭐ 更新语言标签文本
        /// </summary>
        private void UpdateLanguageLabel()
        {
            if (languageLabelText != null && ThreeKingdoms.LocalizationManager.Instance != null)
            {
                languageLabelText.text = ThreeKingdoms.LocalizationManager.Instance.GetText("menu_language");
                TMPFontHelper.SetFontByLanguage(languageLabelText);
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

        private void LateUpdate()
        {
            // ⭐ 只在前10帧检查，避免性能问题
            if (frameCheckCount >= 10) return;
            frameCheckCount++;

            // ⭐ 安全检查：确保dropdown选项始终正确
            if (useDropdown && languageDropdown != null && ThreeKingdoms.LocalizationManager.Instance != null)
            {
                // 如果选项不正确，重新初始化
                if (languageDropdown.options.Count != 3 ||
                    (languageDropdown.options.Count > 0 && languageDropdown.options[0].text != "中文"))
                {
                    Debug.LogWarning($"[LanguageSwitcher] LateUpdate(frame {frameCheckCount})检测到Dropdown选项异常，重新初始化");
                    InitializeDropdown();
                    isInitialized = true;
                }
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

            Debug.Log("[LanguageSwitcher] 开始初始化下拉菜单...");

            // ⭐ 移除所有监听器，稍后重新添加
            languageDropdown.onValueChanged.RemoveAllListeners();

            // ⭐ 先设置字体
            SetDropdownFont();

            // ⭐ 强制清空所有选项
            languageDropdown.options.Clear();
            languageDropdown.ClearOptions();

            // ⭐ 添加语言选项
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>
            {
                new TMP_Dropdown.OptionData("中文"),
                new TMP_Dropdown.OptionData("English"),
                new TMP_Dropdown.OptionData("한국어")
            };
            languageDropdown.options = options;

            // ⭐ 设置当前语言
            int langIndex = 0;
            if (ThreeKingdoms.LocalizationManager.Instance != null)
            {
                ThreeKingdoms.Language currentLang = ThreeKingdoms.LocalizationManager.Instance.GetCurrentLanguage();
                langIndex = (int)currentLang;
                Debug.Log($"[LanguageSwitcher] 当前语言: {currentLang} (index={langIndex})");
            }

            // ⭐ 设置值并强制刷新
            languageDropdown.value = langIndex;
            languageDropdown.RefreshShownValue();

            // ⭐ 更新Caption文本
            if (languageDropdown.captionText != null)
            {
                languageDropdown.captionText.text = options[langIndex].text;
                Debug.Log($"[LanguageSwitcher] Caption文本设置为: {options[langIndex].text}");
            }

            // ⭐ 添加新的监听器
            languageDropdown.onValueChanged.AddListener(OnLanguageChanged);

            Debug.Log($"[LanguageSwitcher] 下拉菜单初始化完成, 选项数: {languageDropdown.options.Count}");
        }

        /// <summary>
        /// ⭐ 设置下拉菜单的字体
        /// </summary>
        private void SetDropdownFont()
        {
            if (languageDropdown == null) return;

            // ⭐ 使用通用字体（支持中文、英文、韩文）
            TMP_FontAsset font = TMPFontHelper.GetUniversalFont();

            if (font == null)
            {
                Debug.LogWarning("[LanguageSwitcher] 无法获取通用字体");
                return;
            }

            Debug.Log($"[LanguageSwitcher] 使用通用字体: {font.name}");

            // 设置Caption Text（当前显示的文本）
            if (languageDropdown.captionText != null)
            {
                languageDropdown.captionText.font = font;
                Debug.Log("[LanguageSwitcher] 已设置Caption字体");
            }

            // 设置Item Text Template（下拉项模板）
            if (languageDropdown.itemText != null)
            {
                languageDropdown.itemText.font = font;
                Debug.Log("[LanguageSwitcher] 已设置Item模板字体");
            }

            // ⭐ 如果下拉菜单的Template已经存在，也需要设置
            Transform template = languageDropdown.transform.Find("Template");
            if (template != null)
            {
                TextMeshProUGUI[] allTexts = template.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var text in allTexts)
                {
                    text.font = font;
                }
                Debug.Log($"[LanguageSwitcher] 已设置Template中的 {allTexts.Length} 个文本组件字体");
            }
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