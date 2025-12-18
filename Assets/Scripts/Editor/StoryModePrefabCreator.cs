using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using ThreeKingdoms.UI;

namespace ThreeKingdoms.Editor
{
    /// <summary>
    /// 故事模式Prefab创建工具
    /// 在Unity编辑器中创建所需的UI Prefab
    /// </summary>
    public class StoryModePrefabCreator : EditorWindow
    {
        [MenuItem("三国演义/创建故事模式Prefabs")]
        public static void ShowWindow()
        {
            GetWindow<StoryModePrefabCreator>("故事模式Prefab创建器");
        }

        private void OnGUI()
        {
            GUILayout.Label("故事模式UI Prefab创建工具", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("创建所有Prefabs", GUILayout.Height(40)))
            {
                CreateAllPrefabs();
            }

            GUILayout.Space(10);
            GUILayout.Label("单独创建：", EditorStyles.boldLabel);

            if (GUILayout.Button("创建战役按钮Prefab"))
            {
                CreateCampaignButtonPrefab();
            }

            if (GUILayout.Button("创建战斗按钮Prefab"))
            {
                CreateBattleButtonPrefab();
            }

            if (GUILayout.Button("创建故事模式场景UI"))
            {
                CreateStoryModeSceneUI();
            }
        }

        private static void CreateAllPrefabs()
        {
            // 确保文件夹存在
            EnsureFolderExists("Assets/Prefabs");
            EnsureFolderExists("Assets/Prefabs/UI");
            EnsureFolderExists("Assets/Prefabs/UI/StoryMode");

            CreateCampaignButtonPrefab();
            CreateBattleButtonPrefab();

            Debug.Log("[StoryModePrefabCreator] 所有Prefab创建完成！");
            EditorUtility.DisplayDialog("完成", "所有故事模式Prefab已创建！\n\n路径: Assets/Prefabs/UI/StoryMode/", "确定");
        }

        private static void CreateCampaignButtonPrefab()
        {
            EnsureFolderExists("Assets/Prefabs/UI/StoryMode");

            GameObject btnObj = CreateStoryButton("CampaignButton", new Color(0.2f, 0.3f, 0.5f, 1f));
            SetAllChineseFonts(btnObj);

            string path = "Assets/Prefabs/UI/StoryMode/CampaignButtonPrefab.prefab";
            PrefabUtility.SaveAsPrefabAsset(btnObj, path);
            DestroyImmediate(btnObj);

            Debug.Log($"[StoryModePrefabCreator] 战役按钮Prefab已创建: {path}");
        }

        private static void CreateBattleButtonPrefab()
        {
            EnsureFolderExists("Assets/Prefabs/UI/StoryMode");

            GameObject btnObj = CreateStoryButton("BattleButton", new Color(0.3f, 0.3f, 0.4f, 1f));
            SetAllChineseFonts(btnObj);

            string path = "Assets/Prefabs/UI/StoryMode/BattleButtonPrefab.prefab";
            PrefabUtility.SaveAsPrefabAsset(btnObj, path);
            DestroyImmediate(btnObj);

            Debug.Log($"[StoryModePrefabCreator] 战斗按钮Prefab已创建: {path}");
        }

        private static GameObject CreateStoryButton(string name, Color bgColor)
        {
            // 创建按钮对象
            GameObject btnObj = new GameObject(name);
            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(250, 45);

            // 添加背景图片
            Image bg = btnObj.AddComponent<Image>();
            bg.color = bgColor;

            // 添加Button组件
            Button btn = btnObj.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = bgColor;
            colors.highlightedColor = new Color(bgColor.r + 0.1f, bgColor.g + 0.1f, bgColor.b + 0.2f, 1f);
            colors.pressedColor = new Color(bgColor.r - 0.1f, bgColor.g - 0.1f, bgColor.b - 0.1f, 1f);
            colors.selectedColor = new Color(bgColor.r + 0.2f, bgColor.g + 0.2f, bgColor.b + 0.3f, 1f);
            colors.disabledColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            btn.colors = colors;

            // 创建文本
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);

            RectTransform textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(15, 5);
            textRt.offsetMax = new Vector2(-15, -5);

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = "按钮文本";
            text.fontSize = 18;
            text.alignment = TextAlignmentOptions.Left;
            text.color = Color.white;

            // 创建完成标记（星号）
            GameObject starObj = new GameObject("CompletedStar");
            starObj.transform.SetParent(btnObj.transform, false);

            RectTransform starRt = starObj.AddComponent<RectTransform>();
            starRt.anchorMin = new Vector2(1, 0.5f);
            starRt.anchorMax = new Vector2(1, 0.5f);
            starRt.pivot = new Vector2(1, 0.5f);
            starRt.anchoredPosition = new Vector2(-10, 0);
            starRt.sizeDelta = new Vector2(20, 20);

            TextMeshProUGUI starText = starObj.AddComponent<TextMeshProUGUI>();
            starText.text = "*";
            starText.fontSize = 24;
            starText.alignment = TextAlignmentOptions.Center;
            starText.color = new Color(1f, 0.8f, 0.2f);
            starObj.SetActive(false); // 默认隐藏

            return btnObj;
        }

        [MenuItem("三国演义/在当前场景创建故事模式UI")]
        public static void CreateStoryModeSceneUI()
        {
            // ⭐ 确保EventSystem存在（否则UI无法响应点击）
            EventSystem eventSystem = FindObjectOfType<EventSystem>();
            if (eventSystem == null)
            {
                GameObject eventObj = new GameObject("EventSystem");
                eventObj.AddComponent<EventSystem>();
                eventObj.AddComponent<StandaloneInputModule>();
                Debug.Log("[StoryModePrefabCreator] 创建EventSystem");
            }

            // 查找或创建Canvas
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);

                canvasObj.AddComponent<GraphicRaycaster>();
            }

            // 创建主UI容器
            GameObject storyUI = new GameObject("StoryModeUI");
            storyUI.transform.SetParent(canvas.transform, false);
            RectTransform storyRt = storyUI.AddComponent<RectTransform>();
            storyRt.anchorMin = Vector2.zero;
            storyRt.anchorMax = Vector2.one;
            storyRt.offsetMin = Vector2.zero;
            storyRt.offsetMax = Vector2.zero;

            // 添加背景
            Image storyBg = storyUI.AddComponent<Image>();
            storyBg.color = new Color(0.05f, 0.05f, 0.1f, 1f);

            // 添加StoryModeUI组件
            var uiComponent = storyUI.AddComponent<ThreeKingdoms.Story.StoryModeUI>();

            // 创建标题
            GameObject titleObj = CreateTitleText(storyUI.transform);

            // 创建左侧面板（战役和战斗列表）
            GameObject leftPanel = CreateLeftPanel(storyUI.transform);

            // 创建右侧面板（详情）
            GameObject rightPanel = CreateRightPanel(storyUI.transform);

            // 创建底部按钮
            GameObject bottomPanel = CreateBottomPanel(storyUI.transform);

            // 设置引用
            SetupReferences(uiComponent, leftPanel, rightPanel, bottomPanel, titleObj);

            // 设置所有文本的中文字体
            SetAllChineseFonts(storyUI);

            // 选中创建的对象
            Selection.activeGameObject = storyUI;

            Debug.Log("[StoryModePrefabCreator] 故事模式场景UI已创建！");
        }

        private static GameObject CreateTitleText(Transform parent)
        {
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(parent, false);

            RectTransform rt = titleObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.92f);
            rt.anchorMax = new Vector2(1, 1f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image bg = titleObj.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.2f, 0.9f);

            // 标题文本
            GameObject textObj = new GameObject("TitleText");
            textObj.transform.SetParent(titleObj.transform, false);

            RectTransform textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = "故事模式";
            text.fontSize = 36;
            text.alignment = TextAlignmentOptions.Center;
            text.color = new Color(1f, 0.9f, 0.6f);

            return titleObj;
        }

        private static GameObject CreateLeftPanel(Transform parent)
        {
            GameObject leftPanel = new GameObject("LeftPanel");
            leftPanel.transform.SetParent(parent, false);

            RectTransform rt = leftPanel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.08f);
            rt.anchorMax = new Vector2(0.45f, 0.9f);
            rt.offsetMin = new Vector2(10, 0);
            rt.offsetMax = new Vector2(-5, 0);

            // 战役列表
            GameObject campaignList = CreateScrollList("CampaignList", leftPanel.transform,
                new Vector2(0, 0), new Vector2(0.48f, 1f), "战役");

            // 战斗列表
            GameObject battleList = CreateScrollList("BattleList", leftPanel.transform,
                new Vector2(0.52f, 0), new Vector2(1f, 1f), "战斗");

            return leftPanel;
        }

        private static GameObject CreateScrollList(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, string title)
        {
            GameObject listObj = new GameObject(name);
            listObj.transform.SetParent(parent, false);

            RectTransform rt = listObj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image bg = listObj.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);

            // 标题
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(listObj.transform, false);

            RectTransform titleRt = titleObj.AddComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0, 0.92f);
            titleRt.anchorMax = new Vector2(1, 1f);
            titleRt.offsetMin = Vector2.zero;
            titleRt.offsetMax = Vector2.zero;

            Image titleBg = titleObj.AddComponent<Image>();
            titleBg.color = new Color(0.15f, 0.15f, 0.25f, 1f);

            GameObject titleTextObj = new GameObject("Text");
            titleTextObj.transform.SetParent(titleObj.transform, false);

            RectTransform titleTextRt = titleTextObj.AddComponent<RectTransform>();
            titleTextRt.anchorMin = Vector2.zero;
            titleTextRt.anchorMax = Vector2.one;
            titleTextRt.offsetMin = Vector2.zero;
            titleTextRt.offsetMax = Vector2.zero;

            TextMeshProUGUI titleText = titleTextObj.AddComponent<TextMeshProUGUI>();
            titleText.text = title;
            titleText.fontSize = 20;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.white;

            // ScrollView
            GameObject scrollView = new GameObject("ScrollView");
            scrollView.transform.SetParent(listObj.transform, false);

            RectTransform scrollRt = scrollView.AddComponent<RectTransform>();
            scrollRt.anchorMin = new Vector2(0, 0);
            scrollRt.anchorMax = new Vector2(1, 0.9f);
            scrollRt.offsetMin = new Vector2(5, 5);
            scrollRt.offsetMax = new Vector2(-5, -5);

            ScrollRect scrollRect = scrollView.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            Image scrollBg = scrollView.AddComponent<Image>();
            scrollBg.color = new Color(0.08f, 0.08f, 0.12f, 1f);
            scrollView.AddComponent<Mask>();

            // Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollView.transform, false);

            RectTransform viewportRt = viewport.AddComponent<RectTransform>();
            viewportRt.anchorMin = Vector2.zero;
            viewportRt.anchorMax = Vector2.one;
            viewportRt.offsetMin = Vector2.zero;
            viewportRt.offsetMax = Vector2.zero;

            viewport.AddComponent<Image>().color = Color.white;
            viewport.AddComponent<Mask>().showMaskGraphic = false;

            // Content
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);

            RectTransform contentRt = content.AddComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 1);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0.5f, 1);
            contentRt.offsetMin = Vector2.zero;
            contentRt.offsetMax = Vector2.zero;

            VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 5;
            layout.padding = new RectOffset(5, 5, 5, 5);
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = viewportRt;
            scrollRect.content = contentRt;

            return listObj;
        }

        private static GameObject CreateRightPanel(Transform parent)
        {
            GameObject rightPanel = new GameObject("RightPanel");
            rightPanel.transform.SetParent(parent, false);

            RectTransform rt = rightPanel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.47f, 0.08f);
            rt.anchorMax = new Vector2(0.99f, 0.9f);
            rt.offsetMin = new Vector2(5, 0);
            rt.offsetMax = new Vector2(-10, 0);

            Image bg = rightPanel.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);

            // 详情图片
            GameObject imageObj = new GameObject("DetailImage");
            imageObj.transform.SetParent(rightPanel.transform, false);

            RectTransform imageRt = imageObj.AddComponent<RectTransform>();
            imageRt.anchorMin = new Vector2(0.05f, 0.5f);
            imageRt.anchorMax = new Vector2(0.95f, 0.95f);
            imageRt.offsetMin = Vector2.zero;
            imageRt.offsetMax = Vector2.zero;

            Image detailImage = imageObj.AddComponent<Image>();
            detailImage.color = new Color(0.2f, 0.2f, 0.25f, 1f);

            // 标题文本
            GameObject titleTextObj = new GameObject("TitleText");
            titleTextObj.transform.SetParent(rightPanel.transform, false);

            RectTransform titleRt = titleTextObj.AddComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0.05f, 0.4f);
            titleRt.anchorMax = new Vector2(0.95f, 0.48f);
            titleRt.offsetMin = Vector2.zero;
            titleRt.offsetMax = Vector2.zero;

            TextMeshProUGUI titleText = titleTextObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "选择一个战役";
            titleText.fontSize = 28;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = new Color(1f, 0.9f, 0.6f);

            // 描述文本
            GameObject descObj = new GameObject("DescriptionText");
            descObj.transform.SetParent(rightPanel.transform, false);

            RectTransform descRt = descObj.AddComponent<RectTransform>();
            descRt.anchorMin = new Vector2(0.05f, 0.15f);
            descRt.anchorMax = new Vector2(0.95f, 0.38f);
            descRt.offsetMin = Vector2.zero;
            descRt.offsetMax = Vector2.zero;

            TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
            descText.text = "战役描述将显示在这里...";
            descText.fontSize = 18;
            descText.alignment = TextAlignmentOptions.TopLeft;
            descText.color = Color.white;

            // 难度文本
            GameObject diffObj = new GameObject("DifficultyText");
            diffObj.transform.SetParent(rightPanel.transform, false);

            RectTransform diffRt = diffObj.AddComponent<RectTransform>();
            diffRt.anchorMin = new Vector2(0.05f, 0.08f);
            diffRt.anchorMax = new Vector2(0.5f, 0.14f);
            diffRt.offsetMin = Vector2.zero;
            diffRt.offsetMax = Vector2.zero;

            TextMeshProUGUI diffText = diffObj.AddComponent<TextMeshProUGUI>();
            diffText.text = "难度: *";
            diffText.fontSize = 18;
            diffText.alignment = TextAlignmentOptions.Left;
            diffText.color = new Color(0.8f, 0.8f, 0.8f);

            // 完成标记
            GameObject completedObj = new GameObject("CompletedBadge");
            completedObj.transform.SetParent(rightPanel.transform, false);

            RectTransform completedRt = completedObj.AddComponent<RectTransform>();
            completedRt.anchorMin = new Vector2(0.8f, 0.08f);
            completedRt.anchorMax = new Vector2(0.95f, 0.14f);
            completedRt.offsetMin = Vector2.zero;
            completedRt.offsetMax = Vector2.zero;

            Image completedBg = completedObj.AddComponent<Image>();
            completedBg.color = new Color(0.2f, 0.6f, 0.2f, 1f);

            GameObject completedTextObj = new GameObject("Text");
            completedTextObj.transform.SetParent(completedObj.transform, false);

            RectTransform completedTextRt = completedTextObj.AddComponent<RectTransform>();
            completedTextRt.anchorMin = Vector2.zero;
            completedTextRt.anchorMax = Vector2.one;
            completedTextRt.offsetMin = Vector2.zero;
            completedTextRt.offsetMax = Vector2.zero;

            TextMeshProUGUI completedText = completedTextObj.AddComponent<TextMeshProUGUI>();
            completedText.text = "已完成";
            completedText.fontSize = 16;
            completedText.alignment = TextAlignmentOptions.Center;
            completedText.color = Color.white;

            completedObj.SetActive(false);

            return rightPanel;
        }

        private static GameObject CreateBottomPanel(Transform parent)
        {
            GameObject bottomPanel = new GameObject("BottomPanel");
            bottomPanel.transform.SetParent(parent, false);

            RectTransform rt = bottomPanel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0.07f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image bg = bottomPanel.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);

            // 返回按钮
            GameObject backBtn = CreateUIButton("BackButton", bottomPanel.transform,
                new Vector2(0.02f, 0.1f), new Vector2(0.15f, 0.9f),
                "返回", new Color(0.5f, 0.3f, 0.3f));

            // 开始战斗按钮
            GameObject startBtn = CreateUIButton("StartBattleButton", bottomPanel.transform,
                new Vector2(0.85f, 0.1f), new Vector2(0.98f, 0.9f),
                "开始战斗", new Color(0.3f, 0.5f, 0.3f));

            return bottomPanel;
        }

        private static GameObject CreateUIButton(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, string text, Color color)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image bg = btnObj.AddComponent<Image>();
            bg.color = color;

            Button btn = btnObj.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = color;
            colors.highlightedColor = new Color(color.r + 0.1f, color.g + 0.1f, color.b + 0.1f, 1f);
            colors.pressedColor = new Color(color.r - 0.1f, color.g - 0.1f, color.b - 0.1f, 1f);
            btn.colors = colors;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);

            RectTransform textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            TextMeshProUGUI btnText = textObj.AddComponent<TextMeshProUGUI>();
            btnText.text = text;
            btnText.fontSize = 20;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.color = Color.white;

            return btnObj;
        }

        private static void SetupReferences(ThreeKingdoms.Story.StoryModeUI uiComponent,
            GameObject leftPanel, GameObject rightPanel, GameObject bottomPanel, GameObject titleObj)
        {
            // 使用SerializedObject设置引用
            SerializedObject so = new SerializedObject(uiComponent);

            // 战役列表容器
            Transform campaignContent = leftPanel.transform.Find("CampaignList/ScrollView/Viewport/Content");
            if (campaignContent != null)
            {
                so.FindProperty("campaignListContainer").objectReferenceValue = campaignContent;
            }

            // 战斗列表容器
            Transform battleContent = leftPanel.transform.Find("BattleList/ScrollView/Viewport/Content");
            if (battleContent != null)
            {
                so.FindProperty("battleListContainer").objectReferenceValue = battleContent;
            }

            // 详情面板引用
            Image detailImage = rightPanel.transform.Find("DetailImage")?.GetComponent<Image>();
            if (detailImage != null)
            {
                so.FindProperty("detailImage").objectReferenceValue = detailImage;
            }

            TextMeshProUGUI titleText = rightPanel.transform.Find("TitleText")?.GetComponent<TextMeshProUGUI>();
            if (titleText != null)
            {
                so.FindProperty("titleText").objectReferenceValue = titleText;
            }

            TextMeshProUGUI descText = rightPanel.transform.Find("DescriptionText")?.GetComponent<TextMeshProUGUI>();
            if (descText != null)
            {
                so.FindProperty("descriptionText").objectReferenceValue = descText;
            }

            TextMeshProUGUI diffText = rightPanel.transform.Find("DifficultyText")?.GetComponent<TextMeshProUGUI>();
            if (diffText != null)
            {
                so.FindProperty("difficultyText").objectReferenceValue = diffText;
            }

            GameObject completedBadge = rightPanel.transform.Find("CompletedBadge")?.gameObject;
            if (completedBadge != null)
            {
                so.FindProperty("completedBadge").objectReferenceValue = completedBadge;
            }

            // 按钮引用
            Button startBtn = bottomPanel.transform.Find("StartBattleButton")?.GetComponent<Button>();
            if (startBtn != null)
            {
                so.FindProperty("startBattleButton").objectReferenceValue = startBtn;
            }

            Button backBtn = bottomPanel.transform.Find("BackButton")?.GetComponent<Button>();
            if (backBtn != null)
            {
                so.FindProperty("backButton").objectReferenceValue = backBtn;
            }

            so.ApplyModifiedProperties();
        }

        private static void EnsureFolderExists(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string[] folders = path.Split('/');
                string currentPath = folders[0];

                for (int i = 1; i < folders.Length; i++)
                {
                    string newPath = currentPath + "/" + folders[i];
                    if (!AssetDatabase.IsValidFolder(newPath))
                    {
                        AssetDatabase.CreateFolder(currentPath, folders[i]);
                    }
                    currentPath = newPath;
                }
            }
        }

        /// <summary>
        /// 设置中文字体 - 使用TMPFontHelper的通用字体
        /// </summary>
        private static void SetChineseFont(TextMeshProUGUI text)
        {
            if (text == null) return;

            // ⭐ 直接使用TMPFontHelper的通用字体（已设置好回退）
            TMP_FontAsset font = TMPFontHelper.GetUniversalFont();

            if (font != null)
            {
                text.font = font;
                Debug.Log($"[StoryModePrefabCreator] 使用通用字体: {font.name}");
            }
            else
            {
                // 备用方案：直接从AssetDatabase加载
                font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Resources/TextMesh Pro/Fonts/MSYH SDF_1.asset");
                if (font != null)
                {
                    text.font = font;
                    Debug.Log("[StoryModePrefabCreator] 使用MSYH字体");
                }
                else
                {
                    Debug.LogWarning("[StoryModePrefabCreator] 未找到中文字体！");
                }
            }
        }

        /// <summary>
        /// 为所有子对象的TextMeshProUGUI设置中文字体
        /// </summary>
        private static void SetAllChineseFonts(GameObject root)
        {
            TextMeshProUGUI[] allTexts = root.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var text in allTexts)
            {
                SetChineseFont(text);
            }
            Debug.Log($"[StoryModePrefabCreator] 已为 {allTexts.Length} 个文本组件设置字体");
        }
    }
}
