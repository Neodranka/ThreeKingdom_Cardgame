using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ThreeKingdoms.UI
{
    /// <summary>
    /// UI自动生成工具
    /// 在编辑器下运行,自动创建完整的战斗UI
    /// </summary>
    public class UIGenerator : MonoBehaviour
    {
#if UNITY_EDITOR
        [MenuItem("三国杀/生成战斗UI")]
        public static void GenerateBattleUI()
        {
            // 查找或创建Canvas
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("BattleCanvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
                
                canvasObj.AddComponent<GraphicRaycaster>();
                
                Debug.Log("创建了Canvas");
            }

            // 查找或创建EventSystem
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                
                Debug.Log("创建了EventSystem");
            }

            // 创建主UI根节点
            GameObject root = new GameObject("BattleUIRoot");
            root.transform.SetParent(canvas.transform, false);
            RectTransform rootRT = root.AddComponent<RectTransform>();
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.sizeDelta = Vector2.zero;

            // 添加BattleUI组件
            BattleUI battleUI = root.AddComponent<BattleUI>();

            // 创建各个面板
            CreateTopPanel(root, battleUI);
            CreateBottomPanel(root, battleUI);
            CreateLeftPanel(root);
            CreateRightPanel(root);
            CreateDeckArea(root, battleUI);
            CreateLogPanel(root, battleUI);

            // 创建预制体
            CreateCardPrefab(battleUI);
            CreatePlayerInfoPrefab(battleUI);

            Debug.Log("战斗UI生成完成!");
            EditorUtility.DisplayDialog("完成", "战斗UI已生成完毕!\n请检查BattleUIRoot对象", "确定");
        }

        private static void CreateTopPanel(GameObject parent, BattleUI battleUI)
        {
            GameObject panel = CreatePanel("TopPanel", parent);
            RectTransform rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.sizeDelta = new Vector2(0, 80);
            
            Image img = panel.GetComponent<Image>();
            img.color = new Color(0, 0, 0, 0.4f);

            // 回合信息
            GameObject turnInfo = CreateText("TurnInfoText", panel, new Vector2(-700, -40), new Vector2(200, 60));
            TextMeshProUGUI turnText = turnInfo.GetComponent<TextMeshProUGUI>();
            turnText.text = "第1回合\n玩家1";
            turnText.fontSize = 20;
            turnText.alignment = TextAlignmentOptions.Center;
            battleUI.turnInfoText = turnText;

            // 阶段信息
            GameObject phaseInfo = CreateText("PhaseText", panel, new Vector2(0, -40), new Vector2(200, 60));
            TextMeshProUGUI phaseText = phaseInfo.GetComponent<TextMeshProUGUI>();
            phaseText.text = "准备阶段";
            phaseText.fontSize = 24;
            phaseText.alignment = TextAlignmentOptions.Center;
            phaseText.color = Color.yellow;
            battleUI.phaseText = phaseText;

            // 牌堆信息
            GameObject deckInfo = CreateText("DeckInfoText", panel, new Vector2(700, -40), new Vector2(200, 60));
            TextMeshProUGUI deckText = deckInfo.GetComponent<TextMeshProUGUI>();
            deckText.text = "牌堆: 70";
            deckText.fontSize = 18;
            deckText.alignment = TextAlignmentOptions.Center;
            battleUI.deckInfoText = deckText;
        }

        private static void CreateBottomPanel(GameObject parent, BattleUI battleUI)
        {
            GameObject panel = CreatePanel("BottomPanel", parent);
            RectTransform rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.sizeDelta = new Vector2(0, 350);
            
            Image img = panel.GetComponent<Image>();
            img.color = new Color(0, 0, 0, 0.6f);

            // 本地玩家信息容器
            GameObject localPlayerContainer = new GameObject("LocalPlayerInfoContainer");
            localPlayerContainer.transform.SetParent(panel.transform, false);
            RectTransform localRT = localPlayerContainer.AddComponent<RectTransform>();
            localRT.anchorMin = new Vector2(0, 0.5f);
            localRT.anchorMax = new Vector2(0, 0.5f);
            localRT.pivot = new Vector2(0, 0.5f);
            localRT.anchoredPosition = new Vector2(20, 0);
            localRT.sizeDelta = new Vector2(220, 280);
            battleUI.localPlayerInfoContainer = localPlayerContainer.transform;

            // 手牌区
            GameObject handCardArea = CreateScrollView("HandCardScrollView", panel, 
                new Vector2(0, 0.6f), new Vector2(1400, 180));
            battleUI.handCardScrollRect = handCardArea.GetComponent<ScrollRect>();
            battleUI.handCardContainer = handCardArea.transform.Find("Viewport/Content");

            // 操作按钮
            CreateActionButtons(panel, battleUI);

            // 消息文本
            GameObject message = CreateText("MessageText", panel, new Vector2(0, 20), new Vector2(400, 40));
            TextMeshProUGUI msgText = message.GetComponent<TextMeshProUGUI>();
            msgText.fontSize = 20;
            msgText.alignment = TextAlignmentOptions.Center;
            msgText.color = Color.yellow;
            battleUI.messageText = msgText;
        }

        private static void CreateActionButtons(GameObject parent, BattleUI battleUI)
        {
            // 结束阶段按钮
            GameObject endBtn = CreateButton("EndPhaseButton", parent, new Vector2(500, 20), 
                new Vector2(150, 50), "结束出牌", new Color(0.2f, 0.8f, 0.2f));
            battleUI.endPhaseButton = endBtn.GetComponent<Button>();

            // 使用卡牌按钮
            GameObject useBtn = CreateButton("UseCardButton", parent, new Vector2(300, 20), 
                new Vector2(150, 50), "使用卡牌", new Color(0.2f, 0.2f, 0.8f));
            battleUI.useCardButton = useBtn.GetComponent<Button>();

            // 取消按钮
            GameObject cancelBtn = CreateButton("CancelButton", parent, new Vector2(100, 20), 
                new Vector2(150, 50), "取消", new Color(0.8f, 0.2f, 0.2f));
            battleUI.cancelButton = cancelBtn.GetComponent<Button>();
        }

        private static void CreateLeftPanel(GameObject parent)
        {
            GameObject panel = CreatePanel("LeftPanel", parent);
            RectTransform rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 0.5f);
            rt.sizeDelta = new Vector2(250, 0);
            rt.anchoredPosition = new Vector2(0, 0);
            
            Image img = panel.GetComponent<Image>();
            img.color = new Color(0, 0, 0, 0.3f);

            // 其他玩家容器
            GameObject container = new GameObject("OtherPlayersContainer");
            container.transform.SetParent(panel.transform, false);
            RectTransform containerRT = container.AddComponent<RectTransform>();
            containerRT.anchorMin = Vector2.zero;
            containerRT.anchorMax = Vector2.one;
            containerRT.sizeDelta = Vector2.zero;
            
            VerticalLayoutGroup vlg = container.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 10;
            vlg.padding = new RectOffset(10, 10, 10, 10);
            vlg.childAlignment = TextAnchor.UpperCenter;
        }

        private static void CreateRightPanel(GameObject parent)
        {
            GameObject panel = CreatePanel("RightPanel", parent);
            RectTransform rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(1, 0.5f);
            rt.sizeDelta = new Vector2(250, 0);
            rt.anchoredPosition = new Vector2(0, 0);
            
            Image img = panel.GetComponent<Image>();
            img.color = new Color(0, 0, 0, 0.3f);

            // 其他玩家容器
            GameObject container = new GameObject("OtherPlayersContainer");
            container.transform.SetParent(panel.transform, false);
            RectTransform containerRT = container.AddComponent<RectTransform>();
            containerRT.anchorMin = Vector2.zero;
            containerRT.anchorMax = Vector2.one;
            containerRT.sizeDelta = Vector2.zero;
            
            VerticalLayoutGroup vlg = container.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 10;
            vlg.padding = new RectOffset(10, 10, 10, 10);
            vlg.childAlignment = TextAnchor.UpperCenter;
        }

        private static void CreateDeckArea(GameObject parent, BattleUI battleUI)
        {
            // 摸牌堆
            GameObject drawPile = CreateButton("DrawPile", parent, new Vector2(700, 0), 
                new Vector2(100, 140), "牌堆\n70", new Color(0.1f, 0.1f, 0.4f));
            battleUI.drawPileText = drawPile.GetComponentInChildren<TextMeshProUGUI>();

            // 弃牌堆
            GameObject discardPile = CreateButton("DiscardPile", parent, new Vector2(820, 0), 
                new Vector2(100, 140), "弃牌堆\n0", new Color(0.4f, 0.1f, 0.1f));
            battleUI.discardPileText = discardPile.GetComponentInChildren<TextMeshProUGUI>();
        }

        private static void CreateLogPanel(GameObject parent, BattleUI battleUI)
        {
            GameObject panel = CreatePanel("LogPanel", parent);
            RectTransform rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(1, 0);
            rt.anchoredPosition = new Vector2(-10, 360);
            rt.sizeDelta = new Vector2(400, 200);
            
            Image img = panel.GetComponent<Image>();
            img.color = new Color(0, 0, 0, 0.7f);

            // 滚动视图
            GameObject scrollView = CreateScrollView("LogScrollView", panel, Vector2.one * 0.5f, 
                new Vector2(380, 180), false, true);
            Transform content = scrollView.transform.Find("Viewport/Content");
            battleUI.logContainer = content;

            // 创建日志文本预制体
            GameObject logTextObj = new GameObject("LogTextPrefab");
            logTextObj.transform.SetParent(content, false);
            RectTransform logRT = logTextObj.AddComponent<RectTransform>();
            logRT.sizeDelta = new Vector2(360, 25);
            
            TextMeshProUGUI logText = logTextObj.AddComponent<TextMeshProUGUI>();
            logText.fontSize = 14;
            logText.alignment = TextAlignmentOptions.Left;
            
            // 保存为临时引用
            battleUI.logPrefab = logText;
            
            // 从父级移除,作为预制体使用
            logTextObj.transform.SetParent(null);
            logTextObj.SetActive(false);
        }

        // 辅助方法
        private static GameObject CreatePanel(string name, GameObject parent)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent.transform, false);
            panel.AddComponent<RectTransform>();
            Image img = panel.AddComponent<Image>();
            img.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
            return panel;
        }

        private static GameObject CreateText(string name, GameObject parent, Vector2 pos, Vector2 size)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent.transform, false);
            RectTransform rt = textObj.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.color = Color.white;
            return textObj;
        }

        private static GameObject CreateButton(string name, GameObject parent, Vector2 pos, 
            Vector2 size, string text, Color color)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent.transform, false);
            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            
            Image img = btnObj.AddComponent<Image>();
            img.color = color;
            btnObj.AddComponent<Button>();
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;
            
            TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
            tmpText.text = text;
            tmpText.fontSize = 18;
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.color = Color.white;
            
            return btnObj;
        }

        private static GameObject CreateScrollView(string name, GameObject parent, Vector2 anchorPos, 
            Vector2 size, bool horizontal = true, bool vertical = false)
        {
            GameObject scrollView = new GameObject(name);
            scrollView.transform.SetParent(parent.transform, false);
            RectTransform rt = scrollView.AddComponent<RectTransform>();
            rt.anchorMin = anchorPos;
            rt.anchorMax = anchorPos;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            
            Image img = scrollView.AddComponent<Image>();
            img.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
            
            ScrollRect scroll = scrollView.AddComponent<ScrollRect>();
            scroll.horizontal = horizontal;
            scroll.vertical = vertical;
            
            // Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollView.transform, false);
            RectTransform vpRT = viewport.AddComponent<RectTransform>();
            vpRT.anchorMin = Vector2.zero;
            vpRT.anchorMax = Vector2.one;
            vpRT.sizeDelta = Vector2.zero;
            viewport.AddComponent<Image>();
            viewport.AddComponent<Mask>().showMaskGraphic = false;
            scroll.viewport = vpRT;
            
            // Content
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            RectTransform contentRT = content.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(0, 1);
            contentRT.pivot = new Vector2(0, 1);
            contentRT.sizeDelta = new Vector2(size.x, size.y);
            
            if (horizontal)
            {
                HorizontalLayoutGroup hlg = content.AddComponent<HorizontalLayoutGroup>();
                hlg.spacing = 10;
                hlg.childAlignment = TextAnchor.MiddleLeft;
                ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
                csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
            else
            {
                VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
                vlg.spacing = 5;
                vlg.childAlignment = TextAnchor.UpperLeft;
                ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
                csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
            
            scroll.content = contentRT;
            
            return scrollView;
        }

        private static void CreateCardPrefab(BattleUI battleUI)
        {
            Debug.Log("请手动创建CardUI预制体,参考UI_SETUP_GUIDE.md");
        }

        private static void CreatePlayerInfoPrefab(BattleUI battleUI)
        {
            Debug.Log("请手动创建PlayerInfoUI预制体,参考UI_SETUP_GUIDE.md");
        }
#endif
    }
}