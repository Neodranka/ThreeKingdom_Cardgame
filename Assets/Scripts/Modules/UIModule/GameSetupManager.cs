using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using ThreeKingdoms.DatabaseModule;

namespace ThreeKingdoms.UI
{
    /// <summary>
    /// 游戏准备场景管理器
    /// 负责游戏配置和武将选择
    /// ⭐ 支持跨场景语言保持和完整本地化
    /// </summary>
    public class GameSetupManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform characterGrid;          // 武将网格容器
        [SerializeField] private GameObject characterButtonPrefab; // 武将按钮预制体
        [SerializeField] private Toggle identityModeToggle;        // 身份场开关
        [SerializeField] private Slider aiDifficultySlider;        // AI难度滑块
        [SerializeField] private TextMeshProUGUI aiDifficultyText; // AI难度显示文本
        [SerializeField] private TextMeshProUGUI selectedCharacterText; // 已选武将显示
        [SerializeField] private Button startGameButton;           // 开始游戏按钮
        [SerializeField] private Button backButton;                // 返回按钮

        [Header("Font Settings")]
        [Tooltip("默认字体资源（可选，不设置则使用TMP默认字体）")]
        [SerializeField] private TMP_FontAsset defaultFont;

        [Header("Data")]
        [SerializeField] private List<GeneralData> availableGenerals; // 可用武将列表

        private GeneralData selectedGeneral;

        private void Start()
        {
            Debug.Log("=== GameSetup场景初始化 ===");

            // ⭐ 第一步：监听语言切换事件
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;
                Debug.Log($"[GameSetup] 已监听语言切换事件，当前语言：{LocalizationManager.Instance.GetCurrentLanguage()}");
            }
            else
            {
                Debug.LogWarning("[GameSetup] LocalizationManager未找到！请确保MainMenu场景有LocalizationManager");
            }

            // 第二步：初始化UI
            InitializeUI();

            // 第三步：加载武将
            LoadCharacters();

            // 第四步：绑定事件
            BindEvents();

            // ⭐ 最后：刷新UI文本（确保使用当前语言）
            RefreshUIText();
        }

        /// <summary>
        /// ⭐ 语言切换回调
        /// </summary>
        private void OnLanguageChanged(Language newLanguage)
        {
            Debug.Log($"[GameSetup] 检测到语言切换: {newLanguage}");
            RefreshUIText();
        }

        /// <summary>
        /// 初始化UI
        /// </summary>
        private void InitializeUI()
        {
            // 确保GameConfig存在
            if (GameConfig.Instance == null)
            {
                GameObject configObj = new GameObject("GameConfig");
                configObj.AddComponent<GameConfig>();
            }

            // 设置默认值
            if (identityModeToggle != null)
                identityModeToggle.isOn = true;

            if (aiDifficultySlider != null)
            {
                aiDifficultySlider.minValue = 0;
                aiDifficultySlider.maxValue = 2;
                aiDifficultySlider.wholeNumbers = true;
                aiDifficultySlider.value = 1;
            }

            // 默认禁用开始按钮（需要先选择武将）
            if (startGameButton != null)
                startGameButton.interactable = false;

            // ⭐ 设置字体
            SetupFonts();
        }

        /// <summary>
        /// ⭐ 刷新UI文本（完整版）
        /// </summary>
        private void RefreshUIText()
        {
            if (LocalizationManager.Instance == null)
            {
                Debug.LogWarning("[GameSetup] LocalizationManager为null，无法刷新UI");
                return;
            }

            Debug.Log($"[GameSetup] 刷新UI文本，当前语言：{LocalizationManager.Instance.GetCurrentLanguage()}");

            // 1. 更新按钮文本
            if (startGameButton != null)
            {
                UpdateButtonText(startGameButton, "ui_start_game");
            }

            if (backButton != null)
            {
                UpdateButtonText(backButton, "ui_back");
            }

            // 2. ⭐ 更新AI难度文本
            UpdateAIDifficultyText();

            // 3. ⭐ 更新已选武将文本
            UpdateSelectedCharacterText();

            // 4. ⭐ 重新加载武将按钮（更新武将名称）
            RefreshCharacterButtons();

            Debug.Log("[GameSetup] UI文本刷新完成");
        }

        /// <summary>
        /// ⭐ 更新按钮文本
        /// </summary>
        private void UpdateButtonText(Button button, string localizationKey)
        {
            if (button == null) return;

            var text = button.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = LocalizationManager.Instance.GetText(localizationKey);
                TMPFontHelper.SetFontByLanguage(text);
                Debug.Log($"[GameSetup] 更新按钮 [{localizationKey}] -> \"{text.text}\"");
            }
        }

        /// <summary>
        /// 设置所有TMP组件的字体
        /// </summary>
        private void SetupFonts()
        {
            // 如果Inspector中设置了字体，则应用到所有TMP组件
            if (defaultFont != null)
            {
                Debug.Log($"[GameSetup] 使用自定义字体: {defaultFont.name}");
                SetFontForAllTMP(transform, defaultFont);
            }
            else
            {
                Debug.Log($"[GameSetup] 未设置字体，将使用TMPFontHelper动态设置");
            }
        }

        /// <summary>
        /// 为所有TMP组件设置字体
        /// </summary>
        private void SetFontForAllTMP(Transform parent, TMP_FontAsset font)
        {
            if (parent == null || font == null) return;

            TextMeshProUGUI[] tmpComponents = parent.GetComponentsInChildren<TextMeshProUGUI>(true);

            foreach (var tmp in tmpComponents)
            {
                tmp.font = font;
            }

            Debug.Log($"[GameSetup] 为 {tmpComponents.Length} 个TMP组件设置了字体");
        }

        /// <summary>
        /// 加载可用武将
        /// </summary>
        private void LoadCharacters()
        {
            if (characterGrid == null)
            {
                Debug.LogWarning("CharacterGrid 未设置！");
                return;
            }

            // 从DatabaseModule加载所有武将
            availableGenerals = new List<GeneralData>(
                Resources.LoadAll<GeneralData>("Data/Generals")
            );

            // 如果没有找到武将数据，创建一些示例武将
            if (availableGenerals.Count == 0)
            {
                Debug.LogWarning("未找到武将数据，使用默认武将");
                CreateDefaultCharacters();
            }

            Debug.Log($"成功加载 {availableGenerals.Count} 个武将");

            // 为每个武将创建选择按钮
            foreach (var general in availableGenerals)
            {
                CreateCharacterButton(general);
            }

            // ⭐ 关键：加载完成后强制刷新布局
            StartCoroutine(RefreshLayoutAfterLoad());
        }

        /// <summary>
        /// ⭐ 刷新武将按钮（语言切换时调用）
        /// </summary>
        private void RefreshCharacterButtons()
        {
            if (characterGrid == null || availableGenerals == null || availableGenerals.Count == 0)
            {
                Debug.LogWarning("[GameSetup] 无法刷新武将按钮：数据为空");
                return;
            }

            Debug.Log("[GameSetup] 刷新武将按钮文本...");

            // 获取所有武将按钮
            Button[] buttons = characterGrid.GetComponentsInChildren<Button>();

            // 确保按钮数量和武将数量匹配
            if (buttons.Length != availableGenerals.Count)
            {
                Debug.LogWarning($"[GameSetup] 按钮数量({buttons.Length})和武将数量({availableGenerals.Count})不匹配，将按索引更新");
            }

            // 按照索引顺序更新（假设按钮创建顺序和武将列表顺序一致）
            for (int i = 0; i < buttons.Length && i < availableGenerals.Count; i++)
            {
                GeneralData general = availableGenerals[i];
                Button button = buttons[i];

                // ⭐ 只更新文本，不重新设置sprite（保留原有视觉效果）
                UpdateCharacterButtonText(button, general);
            }

            Debug.Log($"[GameSetup] 武将按钮文本刷新完成：更新了{Mathf.Min(buttons.Length, availableGenerals.Count)}个按钮");
        }

        /// <summary>
        /// ⭐ 更新单个武将按钮的文本
        /// </summary>
        private void UpdateCharacterButtonText(Button button, GeneralData general)
        {
            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null && LocalizationManager.Instance != null)
            {
                // 使用本地化的武将名称
                string generalKey = $"general_{general.generalId}";
                string localizedName = LocalizationManager.Instance.GetText(generalKey);

                buttonText.text = localizedName;
                TMPFontHelper.SetFontByLanguage(buttonText);

                Debug.Log($"[GameSetup] 更新武将按钮: {general.generalId} -> {localizedName}");
            }
        }

        /// <summary>
        /// 协程：延迟刷新布局
        /// </summary>
        private IEnumerator RefreshLayoutAfterLoad()
        {
            yield return null;

            RectTransform contentRect = characterGrid as RectTransform;
            if (contentRect != null)
            {
                Debug.Log("正在刷新武将列表布局...");
                ForceRebuildLayout(contentRect);

                ScrollRect scrollRect = contentRect.GetComponentInParent<ScrollRect>();
                if (scrollRect != null)
                {
                    FixScrollViewContent(scrollRect);
                }
            }

            yield return null;

            if (contentRect != null)
            {
                ForceRebuildLayout(contentRect);
            }

            Debug.Log("武将列表布局刷新完成！");
        }

        /// <summary>
        /// 强制重建布局
        /// </summary>
        private void ForceRebuildLayout(RectTransform target)
        {
            if (target == null) return;

            LayoutRebuilder.ForceRebuildLayoutImmediate(target);

            Transform parent = target.parent;
            while (parent != null)
            {
                LayoutGroup layoutGroup = parent.GetComponent<LayoutGroup>();
                if (layoutGroup != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(parent as RectTransform);
                }
                parent = parent.parent;
            }
        }

        /// <summary>
        /// 修复ScrollView内容
        /// </summary>
        private void FixScrollViewContent(ScrollRect scrollRect)
        {
            if (scrollRect == null || scrollRect.content == null) return;

            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 1f;

            Debug.Log($"ScrollView内容大小: {scrollRect.content.rect.size}");
        }

        /// <summary>
        /// 创建默认武将（示例）
        /// </summary>
        private void CreateDefaultCharacters()
        {
            // 这里可以创建一些默认武将用于测试
            Debug.Log("[GameSetup] 创建默认武将数据");
        }

        /// <summary>
        /// 创建武将选择按钮
        /// </summary>
        private void CreateCharacterButton(GeneralData general)
        {
            GameObject buttonObj;

            if (characterButtonPrefab != null)
            {
                buttonObj = Instantiate(characterButtonPrefab, characterGrid);
            }
            else
            {
                buttonObj = CreateDefaultCharacterButton();
                buttonObj.transform.SetParent(characterGrid, false);
            }

            // 设置按钮文本（使用本地化）
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null && LocalizationManager.Instance != null)
            {
                string generalKey = $"general_{general.generalId}";
                string localizedName = LocalizationManager.Instance.GetText(generalKey);

                buttonText.text = localizedName;
                TMPFontHelper.SetFontByLanguage(buttonText);

                Debug.Log($"[GameSetup] 创建武将按钮: {general.generalId} -> {localizedName}");
            }
            else if (buttonText != null)
            {
                // 如果没有本地化系统，使用原始名称
                buttonText.text = general.generalName;
            }

            // 绑定选择事件
            Button button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => OnCharacterSelected(general, button));
            }

            // ⭐ 设置头像或背景（使用原始方法名）
            SetCharacterButtonDisplay(buttonObj, general, buttonText);
        }

        /// <summary>
        /// 设置武将按钮的显示（头像或备用方案）
        /// </summary>
        private void SetCharacterButtonDisplay(GameObject buttonObj, GeneralData general, TextMeshProUGUI buttonText)
        {
            Image buttonImage = buttonObj.GetComponent<Image>();
            if (buttonImage == null) return;

            // ⭐ 尝试获取头像（使用general.GetAvatar()）
            Sprite avatar = general.GetAvatar();

            if (avatar != null)
            {
                // 方案1：有头像图片
                buttonImage.sprite = avatar;
                buttonImage.color = Color.white;

                // 文字显示在按钮底部
                if (buttonText != null)
                {
                    buttonText.alignment = TextAlignmentOptions.Bottom;
                    buttonText.fontSize = 20;
                    buttonText.fontStyle = FontStyles.Bold;
                    buttonText.color = Color.white;

                    // 添加黑色描边使文字更清晰
                    buttonText.outlineWidth = 0.2f;
                    buttonText.outlineColor = Color.black;
                }

                Debug.Log($"[武将按钮] {general.generalName} 使用头像显示");
            }
            else
            {
                // 方案2：没有头像，使用纯色背景 + 大号文字
                buttonImage.sprite = null;

                // 根据阵营设置背景颜色
                Color factionColor = GetFactionBackgroundColor(general.faction);
                buttonImage.color = factionColor;

                // 文字居中显示
                if (buttonText != null)
                {
                    buttonText.alignment = TextAlignmentOptions.Center;
                    buttonText.fontSize = 28;
                    buttonText.fontStyle = FontStyles.Bold;
                    buttonText.color = Color.white;

                    // 添加阴影效果
                    var shadow = buttonText.gameObject.GetComponent<UnityEngine.UI.Shadow>();
                    if (shadow == null)
                    {
                        shadow = buttonText.gameObject.AddComponent<UnityEngine.UI.Shadow>();
                    }
                    shadow.effectDistance = new Vector2(2, -2);
                    shadow.effectColor = new Color(0, 0, 0, 0.5f);
                }

                Debug.Log($"[武将按钮] {general.generalName} 使用纯色背景显示（阵营：{general.faction}）");
            }
        }

        /// <summary>
        /// 根据阵营获取背景颜色
        /// </summary>
        private Color GetFactionBackgroundColor(Faction faction)
        {
            switch (faction)
            {
                case Faction.Wei:
                    return new Color(0.2f, 0.3f, 0.6f);  // 蓝色系（魏）
                case Faction.Shu:
                    return new Color(0.6f, 0.2f, 0.2f);  // 红色系（蜀）
                case Faction.Wu:
                    return new Color(0.2f, 0.6f, 0.3f);  // 绿色系（吴）
                case Faction.Qun:
                    return new Color(0.5f, 0.5f, 0.2f);  // 黄色系（群）
                default:
                    return new Color(0.3f, 0.3f, 0.3f);  // 灰色
            }
        }

        /// <summary>
        /// 创建默认武将按钮（无预制体时使用）
        /// </summary>
        private GameObject CreateDefaultCharacterButton()
        {
            GameObject buttonObj = new GameObject("CharacterButton");
            RectTransform rect = buttonObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(150, 200);

            Image image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.3f, 0.3f, 0.3f);
            image.maskable = true;

            Button button = buttonObj.AddComponent<Button>();

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 24;
            tmp.color = Color.white;

            if (defaultFont != null)
            {
                tmp.font = defaultFont;
            }

            return buttonObj;
        }

        /// <summary>
        /// 武将选择事件
        /// </summary>
        private void OnCharacterSelected(GeneralData general, Button button)
        {
            selectedGeneral = general;
            Debug.Log($"选择了武将: {general.generalName}");

            // 更新UI显示
            UpdateSelectedCharacterText();

            // 启用开始按钮
            if (startGameButton != null)
                startGameButton.interactable = true;

            // 视觉反馈：高亮选中的按钮
            HighlightSelectedButton(button);
        }

        /// <summary>
        /// 高亮选中的按钮
        /// </summary>
        private void HighlightSelectedButton(Button selectedButton)
        {
            Button[] allButtons = characterGrid.GetComponentsInChildren<Button>();
            foreach (var btn in allButtons)
            {
                Image img = btn.GetComponent<Image>();
                if (img != null)
                {
                    img.color = new Color(0.3f, 0.3f, 0.3f);
                }
            }

            Image selectedImage = selectedButton.GetComponent<Image>();
            if (selectedImage != null)
            {
                selectedImage.color = new Color(0.5f, 0.7f, 0.3f);
            }
        }

        /// <summary>
        /// ⭐ 更新已选武将显示（支持本地化）
        /// </summary>
        private void UpdateSelectedCharacterText()
        {
            if (selectedCharacterText != null && LocalizationManager.Instance != null)
            {
                if (selectedGeneral != null)
                {
                    string selectedLabel = LocalizationManager.Instance.GetText("ui_selected");
                    string generalKey = $"general_{selectedGeneral.generalId}";
                    string localizedName = LocalizationManager.Instance.GetText(generalKey);

                    selectedCharacterText.text = $"{selectedLabel}: {localizedName}";
                    TMPFontHelper.SetFontByLanguage(selectedCharacterText);
                }
                else
                {
                    selectedCharacterText.text = LocalizationManager.Instance.GetText("ui_please_select_general");
                    TMPFontHelper.SetFontByLanguage(selectedCharacterText);
                }
            }
            else if (selectedCharacterText != null)
            {
                // 没有本地化系统时的fallback
                if (selectedGeneral != null)
                {
                    selectedCharacterText.text = $"已选择: {selectedGeneral.generalName}";
                }
                else
                {
                    selectedCharacterText.text = "请选择武将";
                }
            }
        }

        /// <summary>
        /// ⭐ 更新AI难度显示（支持本地化）
        /// </summary>
        private void UpdateAIDifficultyText()
        {
            if (aiDifficultyText != null && aiDifficultySlider != null && LocalizationManager.Instance != null)
            {
                int difficulty = (int)aiDifficultySlider.value;
                string difficultyKey = difficulty switch
                {
                    0 => "ui_easy",
                    1 => "ui_normal",
                    2 => "ui_hard",
                    _ => "ui_unknown"
                };

                string aiLabel = LocalizationManager.Instance.GetText("ui_ai_difficulty");
                string difficultyName = LocalizationManager.Instance.GetText(difficultyKey);

                aiDifficultyText.text = $"{aiLabel}: {difficultyName}";
                TMPFontHelper.SetFontByLanguage(aiDifficultyText);
            }
            else if (aiDifficultyText != null && aiDifficultySlider != null)
            {
                // 没有本地化系统时的fallback
                int difficulty = (int)aiDifficultySlider.value;
                string difficultyName = difficulty switch
                {
                    0 => "简单",
                    1 => "普通",
                    2 => "困难",
                    _ => "未知"
                };
                aiDifficultyText.text = $"AI难度: {difficultyName}";
            }
        }

        /// <summary>
        /// 绑定UI事件
        /// </summary>
        private void BindEvents()
        {
            if (aiDifficultySlider != null)
            {
                aiDifficultySlider.onValueChanged.AddListener(OnAIDifficultyChanged);
            }

            if (startGameButton != null)
            {
                startGameButton.onClick.AddListener(OnStartGameClicked);
            }

            if (backButton != null)
            {
                backButton.onClick.AddListener(OnBackClicked);
            }
        }

        /// <summary>
        /// AI难度改变事件
        /// </summary>
        private void OnAIDifficultyChanged(float value)
        {
            UpdateAIDifficultyText();
        }

        /// <summary>
        /// 开始游戏按钮点击事件
        /// </summary>
        private void OnStartGameClicked()
        {
            if (selectedGeneral == null)
            {
                Debug.LogWarning("请先选择武将！");
                return;
            }

            // 保存配置到GameConfig
            GameConfig config = GameConfig.Instance;
            if (config != null)
            {
                config.selectedGeneral = selectedGeneral;
                config.enableIdentityMode = identityModeToggle != null && identityModeToggle.isOn;
                config.aiDifficulty = aiDifficultySlider != null ? (int)aiDifficultySlider.value : 1;

                Debug.Log($"游戏配置: 武将={config.selectedGeneral.generalName}, " +
                         $"身份场={config.enableIdentityMode}, AI难度={config.GetAIDifficultyName()}");
            }

            // 加载游戏场景
            SceneManager.LoadScene("GameScene");
        }

        /// <summary>
        /// 返回主菜单
        /// </summary>
        private void OnBackClicked()
        {
            Debug.Log("[GameSetup] 返回主菜单");
            SceneManager.LoadScene("MainMenu");
        }

        private void OnDestroy()
        {
            // 清理事件监听
            if (aiDifficultySlider != null)
                aiDifficultySlider.onValueChanged.RemoveListener(OnAIDifficultyChanged);

            if (startGameButton != null)
                startGameButton.onClick.RemoveListener(OnStartGameClicked);

            if (backButton != null)
                backButton.onClick.RemoveListener(OnBackClicked);

            // ⭐ 取消监听语言切换
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;
                Debug.Log("[GameSetup] 已取消监听语言切换事件");
            }
        }
    }
}