using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace ThreeKingdoms.Story
{
    /// <summary>
    /// 故事模式场景自动生成器
    /// 在空场景中自动创建所有必需的UI元素
    /// </summary>
    public class StoryModeSceneGenerator : MonoBehaviour
    {
        // 生成的UI引用
        private Canvas mainCanvas;
        private StoryModeUI storyModeUI;
        private StoryModeManager storyModeManager;

        // UI元素引用
        private Transform leftPanel;
        private Transform rightPanel;
        private Transform campaignListContainer;
        private Transform battleListContainer;
        private Image detailImage;
        private TextMeshProUGUI titleText;
        private TextMeshProUGUI descriptionText;
        private TextMeshProUGUI difficultyText;
        private TextMeshProUGUI specialRuleText;
        private GameObject completedBadge;
        private Button startBattleButton;
        private Button backButton;

        private void Awake()
        {
            GenerateScene();
        }

        /// <summary>
        /// 生成整个场景
        /// </summary>
        public void GenerateScene()
        {
            Debug.Log("[StoryModeGenerator] 开始生成故事模式场景...");

            // 1. 创建EventSystem（如果没有）
            CreateEventSystem();

            // 2. 创建StoryModeManager（如果没有）
            CreateStoryModeManager();

            // 3. 创建主Canvas
            CreateMainCanvas();

            // 4. 创建左侧面板（战役/战斗列表）
            CreateLeftPanel();

            // 5. 创建右侧面板（详情）
            CreateRightPanel();

            // 6. 创建返回按钮
            CreateBackButton();

            // 7. 创建StoryModeUI组件并绑定引用
            SetupStoryModeUI();

            Debug.Log("[StoryModeGenerator] 故事模式场景生成完成！");
        }

        /// <summary>
        /// 创建EventSystem
        /// </summary>
        private void CreateEventSystem()
        {
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }

        /// <summary>
        /// 创建StoryModeManager
        /// </summary>
        private void CreateStoryModeManager()
        {
            if (StoryModeManager.Instance == null)
            {
                GameObject managerObj = new GameObject("StoryModeManager");
                storyModeManager = managerObj.AddComponent<StoryModeManager>();
            }
            else
            {
                storyModeManager = StoryModeManager.Instance;
            }
        }

        /// <summary>
        /// 创建主Canvas
        /// </summary>
        private void CreateMainCanvas()
        {
            GameObject canvasObj = new GameObject("Canvas");
            mainCanvas = canvasObj.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            // 添加背景
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(canvasObj.transform, false);
            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.15f);
            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
        }

        /// <summary>
        /// 创建左侧面板
        /// </summary>
        private void CreateLeftPanel()
        {
            // 左侧面板容器
            GameObject leftPanelObj = new GameObject("LeftPanel");
            leftPanelObj.transform.SetParent(mainCanvas.transform, false);
            leftPanel = leftPanelObj.transform;

            RectTransform leftRect = leftPanelObj.AddComponent<RectTransform>();
            leftRect.anchorMin = new Vector2(0, 0);
            leftRect.anchorMax = new Vector2(0.3f, 1);
            leftRect.offsetMin = new Vector2(10, 10);
            leftRect.offsetMax = new Vector2(-5, -10);

            Image leftBg = leftPanelObj.AddComponent<Image>();
            leftBg.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);

            // 标题
            CreatePanelTitle(leftPanelObj.transform, "story_mode_title", new Vector2(0, -10), new Vector2(0, -60));

            // 战役列表标题
            CreateSectionTitle(leftPanelObj.transform, "ui_campaigns", new Vector2(10, -70), 24);

            // 战役列表容器
            campaignListContainer = CreateScrollableList(leftPanelObj.transform, "CampaignList",
                new Vector2(0, 0.55f), new Vector2(1, 0.85f));

            // 战斗列表标题
            CreateSectionTitle(leftPanelObj.transform, "ui_battles", new Vector2(10, -350), 24);

            // 战斗列表容器
            battleListContainer = CreateScrollableList(leftPanelObj.transform, "BattleList",
                new Vector2(0, 0.05f), new Vector2(1, 0.45f));
        }

        /// <summary>
        /// 创建可滚动列表
        /// </summary>
        private Transform CreateScrollableList(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            // ScrollView
            GameObject scrollObj = new GameObject(name + "ScrollView");
            scrollObj.transform.SetParent(parent, false);

            RectTransform scrollRect = scrollObj.AddComponent<RectTransform>();
            scrollRect.anchorMin = anchorMin;
            scrollRect.anchorMax = anchorMax;
            scrollRect.offsetMin = new Vector2(10, 10);
            scrollRect.offsetMax = new Vector2(-10, -10);

            ScrollRect scroll = scrollObj.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;

            Image scrollBg = scrollObj.AddComponent<Image>();
            scrollBg.color = new Color(0.1f, 0.1f, 0.15f, 0.5f);

            // Viewport
            GameObject viewportObj = new GameObject("Viewport");
            viewportObj.transform.SetParent(scrollObj.transform, false);

            RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;

            viewportObj.AddComponent<Image>().color = Color.clear;
            viewportObj.AddComponent<Mask>().showMaskGraphic = false;

            // Content
            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(viewportObj.transform, false);

            RectTransform contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.offsetMin = new Vector2(0, 0);
            contentRect.offsetMax = new Vector2(0, 0);

            VerticalLayoutGroup layout = contentObj.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 5;
            layout.padding = new RectOffset(5, 5, 5, 5);
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            ContentSizeFitter fitter = contentObj.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = viewportRect;
            scroll.content = contentRect;

            return contentObj.transform;
        }

        /// <summary>
        /// 创建右侧面板
        /// </summary>
        private void CreateRightPanel()
        {
            // 右侧面板容器
            GameObject rightPanelObj = new GameObject("RightPanel");
            rightPanelObj.transform.SetParent(mainCanvas.transform, false);
            rightPanel = rightPanelObj.transform;

            RectTransform rightRect = rightPanelObj.AddComponent<RectTransform>();
            rightRect.anchorMin = new Vector2(0.3f, 0);
            rightRect.anchorMax = new Vector2(1, 1);
            rightRect.offsetMin = new Vector2(5, 10);
            rightRect.offsetMax = new Vector2(-10, -10);

            Image rightBg = rightPanelObj.AddComponent<Image>();
            rightBg.color = new Color(0.12f, 0.12f, 0.18f, 0.95f);

            // 详情图片
            GameObject imageObj = new GameObject("DetailImage");
            imageObj.transform.SetParent(rightPanelObj.transform, false);

            RectTransform imageRect = imageObj.AddComponent<RectTransform>();
            imageRect.anchorMin = new Vector2(0.05f, 0.45f);
            imageRect.anchorMax = new Vector2(0.95f, 0.95f);
            imageRect.offsetMin = Vector2.zero;
            imageRect.offsetMax = Vector2.zero;

            detailImage = imageObj.AddComponent<Image>();
            detailImage.color = new Color(0.2f, 0.2f, 0.25f);
            detailImage.preserveAspect = true;

            // 标题文本
            titleText = CreateText(rightPanelObj.transform, "TitleText",
                new Vector2(0.05f, 0.35f), new Vector2(0.95f, 0.43f), 32, TextAlignmentOptions.Left);

            // 描述文本
            descriptionText = CreateText(rightPanelObj.transform, "DescriptionText",
                new Vector2(0.05f, 0.15f), new Vector2(0.95f, 0.34f), 20, TextAlignmentOptions.TopLeft);
            descriptionText.enableWordWrapping = true;

            // 难度文本
            difficultyText = CreateText(rightPanelObj.transform, "DifficultyText",
                new Vector2(0.05f, 0.08f), new Vector2(0.4f, 0.14f), 18, TextAlignmentOptions.Left);
            difficultyText.color = new Color(1f, 0.8f, 0.2f);

            // 特殊规则文本
            specialRuleText = CreateText(rightPanelObj.transform, "SpecialRuleText",
                new Vector2(0.4f, 0.08f), new Vector2(0.95f, 0.14f), 18, TextAlignmentOptions.Left);
            specialRuleText.color = new Color(0.8f, 0.6f, 1f);

            // 完成徽章
            completedBadge = new GameObject("CompletedBadge");
            completedBadge.transform.SetParent(rightPanelObj.transform, false);

            RectTransform badgeRect = completedBadge.AddComponent<RectTransform>();
            badgeRect.anchorMin = new Vector2(0.85f, 0.85f);
            badgeRect.anchorMax = new Vector2(0.95f, 0.95f);
            badgeRect.offsetMin = Vector2.zero;
            badgeRect.offsetMax = Vector2.zero;

            Image badgeImg = completedBadge.AddComponent<Image>();
            badgeImg.color = new Color(0.2f, 0.8f, 0.2f);

            TextMeshProUGUI badgeText = CreateText(completedBadge.transform, "BadgeText",
                Vector2.zero, Vector2.one, 14, TextAlignmentOptions.Center);
            badgeText.text = "*";
            badgeText.color = Color.white;

            completedBadge.SetActive(false);

            // 开始战斗按钮
            startBattleButton = CreateButton(rightPanelObj.transform, "StartBattleButton",
                new Vector2(0.3f, 0.02f), new Vector2(0.7f, 0.07f),
                GetLocalizedText("story_start_battle"), new Color(0.2f, 0.5f, 0.2f));
        }

        /// <summary>
        /// 创建返回按钮
        /// </summary>
        private void CreateBackButton()
        {
            backButton = CreateButton(mainCanvas.transform, "BackButton",
                new Vector2(0.01f, 0.92f), new Vector2(0.1f, 0.98f),
                GetLocalizedText("ui_back"), new Color(0.5f, 0.3f, 0.3f));

            backButton.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));
        }

        /// <summary>
        /// 设置StoryModeUI组件
        /// </summary>
        private void SetupStoryModeUI()
        {
            GameObject uiObj = new GameObject("StoryModeUI");
            uiObj.transform.SetParent(mainCanvas.transform, false);

            storyModeUI = uiObj.AddComponent<StoryModeUI>();

            // 使用反射设置私有字段（因为是SerializeField）
            System.Type type = typeof(StoryModeUI);

            SetPrivateField(type, storyModeUI, "campaignListContainer", campaignListContainer);
            SetPrivateField(type, storyModeUI, "battleListContainer", battleListContainer);
            SetPrivateField(type, storyModeUI, "detailImage", detailImage);
            SetPrivateField(type, storyModeUI, "titleText", titleText);
            SetPrivateField(type, storyModeUI, "descriptionText", descriptionText);
            SetPrivateField(type, storyModeUI, "difficultyText", difficultyText);
            SetPrivateField(type, storyModeUI, "specialRuleText", specialRuleText);
            SetPrivateField(type, storyModeUI, "completedBadge", completedBadge);
            SetPrivateField(type, storyModeUI, "startBattleButton", startBattleButton);
            SetPrivateField(type, storyModeUI, "backButton", backButton);

            // 手动调用刷新
            storyModeUI.RefreshCampaignList();
        }

        /// <summary>
        /// 设置私有字段
        /// </summary>
        private void SetPrivateField(System.Type type, object obj, string fieldName, object value)
        {
            var field = type.GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(obj, value);
            }
            else
            {
                Debug.LogWarning($"[StoryModeGenerator] 找不到字段: {fieldName}");
            }
        }

        /// <summary>
        /// 创建面板标题
        /// </summary>
        private void CreatePanelTitle(Transform parent, string textKey, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject titleObj = new GameObject("PanelTitle");
            titleObj.transform.SetParent(parent, false);

            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.offsetMin = new Vector2(offsetMin.x, offsetMax.y);
            titleRect.offsetMax = new Vector2(0, offsetMin.y);

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = GetLocalizedText(textKey);
            titleText.fontSize = 28;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = new Color(1f, 0.9f, 0.6f);

            UI.TMPFontHelper.SetFontByLanguage(titleText);
        }

        /// <summary>
        /// 创建区域标题
        /// </summary>
        private void CreateSectionTitle(Transform parent, string textKey, Vector2 position, int fontSize)
        {
            GameObject titleObj = new GameObject("SectionTitle_" + textKey);
            titleObj.transform.SetParent(parent, false);

            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.anchoredPosition = position;
            titleRect.sizeDelta = new Vector2(-20, 30);

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = GetLocalizedText(textKey);
            titleText.fontSize = fontSize;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Left;
            titleText.color = new Color(0.7f, 0.8f, 1f);

            UI.TMPFontHelper.SetFontByLanguage(titleText);
        }

        /// <summary>
        /// 创建文本
        /// </summary>
        private TextMeshProUGUI CreateText(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, int fontSize, TextAlignmentOptions alignment)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = anchorMin;
            textRect.anchorMax = anchorMax;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;

            UI.TMPFontHelper.SetFontByLanguage(text);

            return text;
        }

        /// <summary>
        /// 创建按钮
        /// </summary>
        private Button CreateButton(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, string text, Color bgColor)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = anchorMin;
            btnRect.anchorMax = anchorMax;
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;

            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = bgColor;

            Button btn = btnObj.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.highlightedColor = new Color(bgColor.r + 0.2f, bgColor.g + 0.2f, bgColor.b + 0.2f);
            colors.pressedColor = new Color(bgColor.r - 0.1f, bgColor.g - 0.1f, bgColor.b - 0.1f);
            btn.colors = colors;

            // 按钮文本
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            TextMeshProUGUI btnText = textObj.AddComponent<TextMeshProUGUI>();
            btnText.text = text;
            btnText.fontSize = 20;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.color = Color.white;

            UI.TMPFontHelper.SetFontByLanguage(btnText);

            return btn;
        }

        /// <summary>
        /// 获取本地化文本
        /// </summary>
        private string GetLocalizedText(string key)
        {
            if (LocalizationManager.Instance != null)
            {
                return LocalizationManager.Instance.GetText(key);
            }
            return key;
        }
    }
}
