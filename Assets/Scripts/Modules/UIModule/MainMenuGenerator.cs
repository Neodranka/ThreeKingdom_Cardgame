using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ThreeKingdomsKill.UI
{
    #if UNITY_EDITOR
    public class MainMenuGenerator : EditorWindow
    {
        [MenuItem("三国杀/生成主菜单UI")]
        public static void ShowWindow()
        {
            GetWindow<MainMenuGenerator>("主菜单生成器");
        }

        private void OnGUI()
        {
            GUILayout.Label("主菜单UI生成工具", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("生成主菜单UI", GUILayout.Height(40)))
            {
                GenerateMainMenuUI();
            }

            GUILayout.Space(10);
            GUILayout.Label("注意：", EditorStyles.boldLabel);
            GUILayout.Label("1. 请确保当前场景是MainMenu场景");
            GUILayout.Label("2. 会自动创建完整的UI层级结构");
            GUILayout.Label("3. 会自动添加MainMenuManager组件");
        }

        private static void GenerateMainMenuUI()
        {
            // 检查是否已有Canvas
            Canvas existingCanvas = FindObjectOfType<Canvas>();
            if (existingCanvas != null)
            {
                if (!EditorUtility.DisplayDialog("警告", 
                    "场景中已存在Canvas，是否删除重新生成？", "是", "取消"))
                {
                    return;
                }
                DestroyImmediate(existingCanvas.gameObject);
            }

            // 创建Canvas
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObj.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
            canvasObj.AddComponent<GraphicRaycaster>();

            // 创建EventSystem
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // 创建背景
            GameObject background = CreateImageObject("Background", canvasObj.transform);
            RectTransform bgRect = background.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            Image bgImage = background.GetComponent<Image>();
            bgImage.color = new Color(0.2f, 0.15f, 0.1f, 1f); // 深棕色背景

            // 创建标题面板
            GameObject titlePanel = new GameObject("TitlePanel");
            titlePanel.transform.SetParent(canvasObj.transform, false);
            RectTransform titleRect = titlePanel.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.7f);
            titleRect.anchorMax = new Vector2(0.5f, 0.9f);
            titleRect.sizeDelta = new Vector2(800, 200);

            // 创建游戏标题
            GameObject titleText = CreateTextObject("GameTitle", titlePanel.transform, "三国杀", 80);
            RectTransform titleTextRect = titleText.GetComponent<RectTransform>();
            titleTextRect.anchorMin = Vector2.zero;
            titleTextRect.anchorMax = Vector2.one;
            titleTextRect.sizeDelta = Vector2.zero;
            TextMeshProUGUI titleTMP = titleText.GetComponent<TextMeshProUGUI>();
            titleTMP.color = new Color(0.9f, 0.8f, 0.5f); // 金色
            titleTMP.fontStyle = FontStyles.Bold;

            // 创建主菜单面板
            GameObject mainMenuPanel = new GameObject("MainMenuPanel");
            mainMenuPanel.transform.SetParent(canvasObj.transform, false);
            RectTransform menuRect = mainMenuPanel.AddComponent<RectTransform>();
            menuRect.anchorMin = new Vector2(0.5f, 0.3f);
            menuRect.anchorMax = new Vector2(0.5f, 0.6f);
            menuRect.sizeDelta = new Vector2(400, 300);

            // 添加垂直布局组
            VerticalLayoutGroup vlg = mainMenuPanel.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 20;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // 创建按钮
            GameObject battleButton = CreateButton("BattleModeButton", mainMenuPanel.transform, "对战模式");
            GameObject storyButton = CreateButton("StoryModeButton", mainMenuPanel.transform, "故事模式");
            GameObject settingsButton = CreateButton("SettingsButton", mainMenuPanel.transform, "设置");

            // 设置按钮高度
            battleButton.AddComponent<LayoutElement>().preferredHeight = 80;
            storyButton.AddComponent<LayoutElement>().preferredHeight = 80;
            settingsButton.AddComponent<LayoutElement>().preferredHeight = 80;

            // 创建版本信息
            GameObject versionText = CreateTextObject("VersionText", canvasObj.transform, "v0.1.0", 20);
            RectTransform versionRect = versionText.GetComponent<RectTransform>();
            versionRect.anchorMin = new Vector2(0, 0);
            versionRect.anchorMax = new Vector2(0, 0);
            versionRect.anchoredPosition = new Vector2(100, 30);
            versionRect.sizeDelta = new Vector2(200, 40);
            TextMeshProUGUI versionTMP = versionText.GetComponent<TextMeshProUGUI>();
            versionTMP.color = new Color(0.7f, 0.7f, 0.7f);
            versionTMP.alignment = TextAlignmentOptions.Left;

            // 添加MainMenuManager组件
            GameObject managerObj = new GameObject("MainMenuManager");
            MainMenuManager manager = managerObj.AddComponent<MainMenuManager>();
            
            // 使用反射设置私有字段
            var type = typeof(MainMenuManager);
            var battleField = type.GetField("battleModeButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var storyField = type.GetField("storyModeButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var settingsField = type.GetField("settingsButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            battleField?.SetValue(manager, battleButton.GetComponent<Button>());
            storyField?.SetValue(manager, storyButton.GetComponent<Button>());
            settingsField?.SetValue(manager, settingsButton.GetComponent<Button>());

            // 标记场景为已修改
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            Debug.Log("主菜单UI生成完成！");
            EditorUtility.DisplayDialog("成功", "主菜单UI已生成完成！", "确定");
        }

        private static GameObject CreateImageObject(string name, Transform parent)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.AddComponent<RectTransform>();
            obj.AddComponent<Image>();
            return obj;
        }

        private static GameObject CreateTextObject(string name, Transform parent, string text, int fontSize)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.AddComponent<RectTransform>();
            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            return obj;
        }

        private static GameObject CreateButton(string name, Transform parent, string text)
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent, false);
            RectTransform rect = buttonObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(400, 80);

            // 添加Image组件
            Image image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.3f, 0.2f, 0.15f); // 深色按钮背景

            // 添加Button组件
            Button button = buttonObj.AddComponent<Button>();
            
            // 设置按钮颜色状态
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.3f, 0.2f, 0.15f);
            colors.highlightedColor = new Color(0.4f, 0.3f, 0.2f);
            colors.pressedColor = new Color(0.2f, 0.15f, 0.1f);
            colors.selectedColor = new Color(0.35f, 0.25f, 0.18f);
            button.colors = colors;

            // 创建按钮文本
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 36;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.9f, 0.85f, 0.7f); // 浅金色文字

            return buttonObj;
        }
    }
    #endif
}
