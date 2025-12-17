#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using ThreeKingdoms.UI;
using ThreeKingdoms.Story;

namespace ThreeKingdoms.Editor
{
    /// <summary>
    /// 故事模式场景创建器（编辑器工具）
    /// 在Unity编辑器中直接生成StoryMode场景
    /// </summary>
    public class StoryModeSceneCreator
    {
        // 缓存生成的UI元素引用
        private static Transform campaignListContainer;
        private static Transform battleListContainer;
        private static Image detailImage;
        private static TextMeshProUGUI titleText;
        private static TextMeshProUGUI descriptionText;
        private static TextMeshProUGUI difficultyText;
        private static TextMeshProUGUI specialRuleText;
        private static GameObject completedBadge;
        private static Button startBattleButton;
        private static Button backButton;

        [MenuItem("Tools/Three Kingdoms/Create Story Mode Scene")]
        public static void CreateStoryModeScene()
        {
            // 创建新场景
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // 生成所有内容
            CreateSceneContent();

            // 保存场景
            string scenePath = "Assets/Scenes/StoryMode.unity";

            // 确保Scenes文件夹存在
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }

            EditorSceneManager.SaveScene(newScene, scenePath);

            // 添加到Build Settings
            AddSceneToBuildSettings(scenePath);

            Debug.Log("[StoryModeCreator] StoryMode场景已创建并保存到: " + scenePath);
            EditorUtility.DisplayDialog("完成", "StoryMode场景已创建！\n\n路径: " + scenePath + "\n\n已自动添加到Build Settings\n已自动绑定所有引用", "确定");
        }

        private static void CreateSceneContent()
        {
            // 1. 创建Canvas
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            // 2. 创建背景
            GameObject bgObj = CreateUIElement("Background", canvasObj.transform);
            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0.08f, 0.08f, 0.12f);
            SetAnchors(bgObj, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // 3. 创建左侧面板
            GameObject leftPanel = CreateLeftPanel(canvasObj.transform);

            // 4. 创建右侧面板
            GameObject rightPanel = CreateRightPanel(canvasObj.transform);

            // 5. 创建返回按钮
            backButton = CreateBackButton(canvasObj.transform);

            // 6. 创建StoryModeManager并添加组件
            GameObject managerObj = new GameObject("StoryModeManager");
            managerObj.AddComponent<StoryModeManager>();

            // 7. 创建StoryModeUI并绑定引用
            GameObject uiObj = new GameObject("StoryModeUI");
            uiObj.transform.SetParent(canvasObj.transform, false);
            StoryModeUI storyModeUI = uiObj.AddComponent<StoryModeUI>();

            // 使用SerializedObject绑定引用
            BindStoryModeUIReferences(storyModeUI);

            // 8. 创建EventSystem（如果没有）
            if (GameObject.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            Debug.Log("[StoryModeCreator] 场景内容已生成，引用已自动绑定");
        }

        /// <summary>
        /// 绑定StoryModeUI的引用
        /// </summary>
        private static void BindStoryModeUIReferences(StoryModeUI storyModeUI)
        {
            SerializedObject serializedUI = new SerializedObject(storyModeUI);

            // 绑定各个引用
            serializedUI.FindProperty("campaignListContainer").objectReferenceValue = campaignListContainer;
            serializedUI.FindProperty("battleListContainer").objectReferenceValue = battleListContainer;
            serializedUI.FindProperty("detailImage").objectReferenceValue = detailImage;
            serializedUI.FindProperty("titleText").objectReferenceValue = titleText;
            serializedUI.FindProperty("descriptionText").objectReferenceValue = descriptionText;
            serializedUI.FindProperty("difficultyText").objectReferenceValue = difficultyText;
            serializedUI.FindProperty("specialRuleText").objectReferenceValue = specialRuleText;
            serializedUI.FindProperty("completedBadge").objectReferenceValue = completedBadge;
            serializedUI.FindProperty("startBattleButton").objectReferenceValue = startBattleButton;
            serializedUI.FindProperty("backButton").objectReferenceValue = backButton;

            serializedUI.ApplyModifiedPropertiesWithoutUndo();

            Debug.Log("[StoryModeCreator] StoryModeUI引用绑定完成");
        }

        private static GameObject CreateLeftPanel(Transform parent)
        {
            // 左侧面板
            GameObject leftPanel = CreateUIElement("LeftPanel", parent);
            Image leftBg = leftPanel.AddComponent<Image>();
            leftBg.color = new Color(0.12f, 0.12f, 0.18f, 0.95f);
            SetAnchors(leftPanel, new Vector2(0, 0), new Vector2(0.28f, 1), new Vector2(15, 15), new Vector2(-10, -15));

            // 标题
            GameObject title = CreateTextElement("Title", leftPanel.transform, "故事模式", 32, FontStyles.Bold);
            SetAnchors(title, new Vector2(0, 0.92f), new Vector2(1, 1), new Vector2(10, 5), new Vector2(-10, -10));
            title.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
            title.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.4f);

            // 分隔线1
            GameObject divider1 = CreateUIElement("Divider1", leftPanel.transform);
            Image div1Img = divider1.AddComponent<Image>();
            div1Img.color = new Color(0.4f, 0.4f, 0.5f);
            SetAnchors(divider1, new Vector2(0.05f, 0.91f), new Vector2(0.95f, 0.915f), Vector2.zero, Vector2.zero);

            // 战役标签
            GameObject campaignLabel = CreateTextElement("CampaignLabel", leftPanel.transform, "[ 战役 ]", 22, FontStyles.Bold);
            SetAnchors(campaignLabel, new Vector2(0, 0.85f), new Vector2(1, 0.9f), new Vector2(15, 0), new Vector2(-10, 0));
            campaignLabel.GetComponent<TextMeshProUGUI>().color = new Color(0.6f, 0.8f, 1f);

            // 战役列表容器 - 缓存到静态变量
            GameObject campaignListObj = CreateScrollView("CampaignListContainer", leftPanel.transform,
                new Vector2(0.02f, 0.5f), new Vector2(0.98f, 0.84f));
            campaignListContainer = campaignListObj.transform;

            // 分隔线2
            GameObject divider2 = CreateUIElement("Divider2", leftPanel.transform);
            Image div2Img = divider2.AddComponent<Image>();
            div2Img.color = new Color(0.4f, 0.4f, 0.5f);
            SetAnchors(divider2, new Vector2(0.05f, 0.485f), new Vector2(0.95f, 0.49f), Vector2.zero, Vector2.zero);

            // 战斗标签
            GameObject battleLabel = CreateTextElement("BattleLabel", leftPanel.transform, "[ 战斗 ]", 22, FontStyles.Bold);
            SetAnchors(battleLabel, new Vector2(0, 0.43f), new Vector2(1, 0.48f), new Vector2(15, 0), new Vector2(-10, 0));
            battleLabel.GetComponent<TextMeshProUGUI>().color = new Color(0.6f, 0.8f, 1f);

            // 战斗列表容器 - 缓存到静态变量
            GameObject battleListObj = CreateScrollView("BattleListContainer", leftPanel.transform,
                new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.42f));
            battleListContainer = battleListObj.transform;

            return leftPanel;
        }

        private static GameObject CreateRightPanel(Transform parent)
        {
            // 右侧面板
            GameObject rightPanel = CreateUIElement("RightPanel", parent);
            Image rightBg = rightPanel.AddComponent<Image>();
            rightBg.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            SetAnchors(rightPanel, new Vector2(0.28f, 0), new Vector2(1, 1), new Vector2(10, 15), new Vector2(-15, -15));

            // 详情图片 - 缓存到静态变量
            GameObject detailImageObj = CreateUIElement("DetailImage", rightPanel.transform);
            detailImage = detailImageObj.AddComponent<Image>();
            detailImage.color = new Color(0.2f, 0.2f, 0.25f);
            detailImage.preserveAspect = true;
            SetAnchors(detailImageObj, new Vector2(0.03f, 0.4f), new Vector2(0.97f, 0.95f), Vector2.zero, Vector2.zero);

            // 图片边框
            GameObject imageBorder = CreateUIElement("ImageBorder", detailImageObj.transform);
            Image borderImg = imageBorder.AddComponent<Image>();
            borderImg.color = new Color(0.4f, 0.35f, 0.25f);
            Outline outline = imageBorder.AddComponent<Outline>();
            outline.effectColor = new Color(0.6f, 0.5f, 0.3f);
            outline.effectDistance = new Vector2(3, 3);
            SetAnchors(imageBorder, Vector2.zero, Vector2.one, new Vector2(-5, -5), new Vector2(5, 5));
            imageBorder.transform.SetAsFirstSibling();

            // 标题文本 - 缓存到静态变量
            GameObject titleTextObj = CreateTextElement("TitleText", rightPanel.transform, "战役/战斗名称", 36, FontStyles.Bold);
            SetAnchors(titleTextObj, new Vector2(0.03f, 0.32f), new Vector2(0.97f, 0.38f), Vector2.zero, Vector2.zero);
            titleText = titleTextObj.GetComponent<TextMeshProUGUI>();
            titleText.color = new Color(1f, 0.95f, 0.8f);

            // 描述文本 - 缓存到静态变量
            GameObject descTextObj = CreateTextElement("DescriptionText", rightPanel.transform,
                "这里显示战役或战斗的详细描述...\n\n战前简报会显示在这里。", 20, FontStyles.Normal);
            SetAnchors(descTextObj, new Vector2(0.03f, 0.14f), new Vector2(0.97f, 0.31f), Vector2.zero, Vector2.zero);
            descriptionText = descTextObj.GetComponent<TextMeshProUGUI>();
            descriptionText.alignment = TextAlignmentOptions.TopLeft;
            descriptionText.enableWordWrapping = true;
            descriptionText.color = new Color(0.85f, 0.85f, 0.9f);

            // 难度文本 - 缓存到静态变量
            GameObject diffTextObj = CreateTextElement("DifficultyText", rightPanel.transform, "难度: ***", 20, FontStyles.Normal);
            SetAnchors(diffTextObj, new Vector2(0.03f, 0.08f), new Vector2(0.3f, 0.13f), Vector2.zero, Vector2.zero);
            difficultyText = diffTextObj.GetComponent<TextMeshProUGUI>();
            difficultyText.color = new Color(1f, 0.8f, 0.3f);

            // 特殊规则文本 - 缓存到静态变量
            GameObject ruleTextObj = CreateTextElement("SpecialRuleText", rightPanel.transform, "特殊规则: 无", 18, FontStyles.Normal);
            SetAnchors(ruleTextObj, new Vector2(0.32f, 0.08f), new Vector2(0.97f, 0.13f), Vector2.zero, Vector2.zero);
            specialRuleText = ruleTextObj.GetComponent<TextMeshProUGUI>();
            specialRuleText.color = new Color(0.8f, 0.6f, 1f);

            // 完成徽章 - 缓存到静态变量
            completedBadge = CreateUIElement("CompletedBadge", rightPanel.transform);
            Image badgeImg = completedBadge.AddComponent<Image>();
            badgeImg.color = new Color(0.15f, 0.6f, 0.15f);
            SetAnchors(completedBadge, new Vector2(0.88f, 0.88f), new Vector2(0.96f, 0.94f), Vector2.zero, Vector2.zero);

            GameObject badgeText = CreateTextElement("BadgeText", completedBadge.transform, "完成", 16, FontStyles.Bold);
            SetAnchors(badgeText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            badgeText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

            completedBadge.SetActive(false); // 默认隐藏

            // 开始战斗按钮 - 缓存到静态变量
            GameObject startBtnObj = CreateButton("StartBattleButton", rightPanel.transform, "开始战斗",
                new Color(0.2f, 0.5f, 0.25f));
            SetAnchors(startBtnObj, new Vector2(0.35f, 0.015f), new Vector2(0.65f, 0.065f), Vector2.zero, Vector2.zero);
            startBattleButton = startBtnObj.GetComponent<Button>();

            return rightPanel;
        }

        private static Button CreateBackButton(Transform parent)
        {
            GameObject backBtn = CreateButton("BackButton", parent, "返回", new Color(0.5f, 0.25f, 0.25f));
            SetAnchors(backBtn, new Vector2(0.01f, 0.94f), new Vector2(0.08f, 0.985f), Vector2.zero, Vector2.zero);
            return backBtn.GetComponent<Button>();
        }

        private static GameObject CreateScrollView(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
        {
            // ScrollView容器
            GameObject scrollView = CreateUIElement(name + "_ScrollView", parent);
            Image scrollBg = scrollView.AddComponent<Image>();
            scrollBg.color = new Color(0.08f, 0.08f, 0.12f, 0.8f);
            SetAnchors(scrollView, anchorMin, anchorMax, Vector2.zero, Vector2.zero);

            ScrollRect scroll = scrollView.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 20;

            // Viewport
            GameObject viewport = CreateUIElement("Viewport", scrollView.transform);
            viewport.AddComponent<Image>().color = Color.clear;
            viewport.AddComponent<Mask>().showMaskGraphic = false;
            SetAnchors(viewport, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // Content (这是实际要绑定到StoryModeUI的容器)
            GameObject content = CreateUIElement(name, viewport.transform);
            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0, 0);

            VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8;
            layout.padding = new RectOffset(8, 8, 8, 8);
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = viewport.GetComponent<RectTransform>();
            scroll.content = contentRect;

            return content;
        }

        private static GameObject CreateUIElement(string name, Transform parent)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.AddComponent<RectTransform>();
            return obj;
        }

        private static GameObject CreateTextElement(string name, Transform parent, string text, int fontSize, FontStyles style)
        {
            GameObject obj = CreateUIElement(name, parent);
            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.color = Color.white;

            // 设置字体
            TMP_FontAsset font = TMPFontHelper.GetChineseFont();
            if (font != null)
            {
                tmp.font = font;
            }

            return obj;
        }

        private static GameObject CreateButton(string name, Transform parent, string text, Color bgColor)
        {
            GameObject btnObj = CreateUIElement(name, parent);
            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = bgColor;

            Button btn = btnObj.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.highlightedColor = new Color(bgColor.r + 0.15f, bgColor.g + 0.15f, bgColor.b + 0.15f);
            colors.pressedColor = new Color(bgColor.r - 0.1f, bgColor.g - 0.1f, bgColor.b - 0.1f);
            colors.selectedColor = bgColor;
            btn.colors = colors;

            GameObject textObj = CreateTextElement("Text", btnObj.transform, text, 22, FontStyles.Bold);
            SetAnchors(textObj, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            TextMeshProUGUI btnText = textObj.GetComponent<TextMeshProUGUI>();
            btnText.alignment = TextAlignmentOptions.Center;

            return btnObj;
        }

        private static void SetAnchors(GameObject obj, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

            // 检查是否已存在
            bool exists = false;
            foreach (var scene in scenes)
            {
                if (scene.path == scenePath)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                scenes.Add(new EditorBuildSettingsScene(scenePath, true));
                EditorBuildSettings.scenes = scenes.ToArray();
                Debug.Log("[StoryModeCreator] 已添加到Build Settings: " + scenePath);
            }
        }
    }
}
#endif
