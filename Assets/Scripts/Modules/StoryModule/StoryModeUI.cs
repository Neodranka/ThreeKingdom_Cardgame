using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

namespace ThreeKingdoms.Story
{
    /// <summary>
    /// 故事模式UI管理器
    /// 负责显示战役列表、战斗列表和详情
    /// </summary>
    public class StoryModeUI : MonoBehaviour
    {
        [Header("左侧 - 战役/战斗列表")]
        [SerializeField] private Transform campaignListContainer;
        [SerializeField] private Transform battleListContainer;
        [SerializeField] private GameObject campaignButtonPrefab;
        [SerializeField] private GameObject battleButtonPrefab;

        [Header("右侧 - 详情面板")]
        [SerializeField] private Image detailImage;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI difficultyText;
        [SerializeField] private TextMeshProUGUI specialRuleText;
        [SerializeField] private GameObject completedBadge;

        [Header("按钮")]
        [SerializeField] private Button startBattleButton;
        [SerializeField] private Button backButton;

        [Header("默认图片")]
        [SerializeField] private Sprite defaultCampaignImage;
        [SerializeField] private Sprite defaultBattleImage;
        [SerializeField] private Sprite lockedImage;

        // UI对象缓存
        private List<GameObject> campaignButtons = new List<GameObject>();
        private List<GameObject> battleButtons = new List<GameObject>();
        private int selectedCampaignIndex = 0;
        private int selectedBattleIndex = 0;

        private void Start()
        {
            // 确保StoryModeManager存在
            if (StoryModeManager.Instance == null)
            {
                GameObject managerObj = new GameObject("StoryModeManager");
                managerObj.AddComponent<StoryModeManager>();
            }
            else
            {
                // ⭐ 确保战役数据是最新版本
                StoryModeManager.Instance.EnsureCampaignsUpToDate();
            }

            // ⭐ 确保UI引用存在
            EnsureUIReferences();

            SetupButtons();
            RefreshCampaignList();

            // 监听语言切换
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;
            }
        }

        private void OnDestroy()
        {
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;
            }
        }

        private void OnLanguageChanged(Language newLanguage)
        {
            RefreshCampaignList();
            RefreshBattleList();
            UpdateDetailPanel();
        }

        /// <summary>
        /// ⭐ 确保UI引用存在，如果不存在则尝试查找或创建
        /// </summary>
        private void EnsureUIReferences()
        {
            // ⭐ 确保EventSystem存在（否则UI按钮无法响应）
            EventSystem eventSystem = FindObjectOfType<EventSystem>();
            if (eventSystem == null)
            {
                Debug.LogWarning("[StoryModeUI] 未找到EventSystem，创建一个新的");
                GameObject eventObj = new GameObject("EventSystem");
                eventObj.AddComponent<EventSystem>();
                eventObj.AddComponent<StandaloneInputModule>();
            }

            // 查找Canvas
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogWarning("[StoryModeUI] 未找到Canvas，创建一个新的");
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }

            // 确保战役列表容器存在
            if (campaignListContainer == null)
            {
                campaignListContainer = transform.Find("LeftPanel/CampaignList/Viewport/Content");
                if (campaignListContainer == null)
                {
                    Debug.Log("[StoryModeUI] 创建战役列表容器");
                    campaignListContainer = CreateListContainer("CampaignListContainer", canvas.transform, false);
                }
            }

            // 确保战斗列表容器存在
            if (battleListContainer == null)
            {
                battleListContainer = transform.Find("LeftPanel/BattleList/Viewport/Content");
                if (battleListContainer == null)
                {
                    Debug.Log("[StoryModeUI] 创建战斗列表容器");
                    battleListContainer = CreateListContainer("BattleListContainer", canvas.transform, true);
                }
            }

            // 确保标题文本存在
            if (titleText == null)
            {
                titleText = transform.GetComponentInChildren<TextMeshProUGUI>();
                if (titleText == null)
                {
                    Debug.Log("[StoryModeUI] 创建标题文本");
                    GameObject titleObj = new GameObject("TitleText");
                    titleObj.transform.SetParent(canvas.transform, false);
                    titleText = titleObj.AddComponent<TextMeshProUGUI>();
                    titleText.fontSize = 24;
                    titleText.alignment = TextAlignmentOptions.Center;
                    RectTransform rt = titleObj.GetComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0.5f, 0.9f);
                    rt.anchorMax = new Vector2(0.5f, 0.95f);
                    rt.sizeDelta = new Vector2(400, 50);
                }
            }

            Debug.Log($"[StoryModeUI] UI引用检查完成 - 战役容器:{campaignListContainer != null}, 战斗容器:{battleListContainer != null}");
        }

        /// <summary>
        /// ⭐ 创建列表容器
        /// </summary>
        private Transform CreateListContainer(string name, Transform parent, bool isSecondColumn = false)
        {
            GameObject containerObj = new GameObject(name);
            containerObj.transform.SetParent(parent, false);

            RectTransform rt = containerObj.AddComponent<RectTransform>();
            if (isSecondColumn)
            {
                // 战斗列表 - 第二列
                rt.anchorMin = new Vector2(0.22f, 0.1f);
                rt.anchorMax = new Vector2(0.42f, 0.9f);
            }
            else
            {
                // 战役列表 - 第一列
                rt.anchorMin = new Vector2(0.02f, 0.1f);
                rt.anchorMax = new Vector2(0.2f, 0.9f);
            }
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // 添加垂直布局组
            var layout = containerObj.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
            layout.spacing = 5;
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            // 添加背景
            var bg = containerObj.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);

            // 添加内容大小适配器
            var fitter = containerObj.AddComponent<UnityEngine.UI.ContentSizeFitter>();
            fitter.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;

            return containerObj.transform;
        }

        /// <summary>
        /// 设置按钮事件
        /// </summary>
        private void SetupButtons()
        {
            Debug.Log($"[StoryModeUI] SetupButtons - startBattleButton:{startBattleButton != null}, backButton:{backButton != null}");

            if (startBattleButton != null)
            {
                startBattleButton.onClick.RemoveAllListeners();
                startBattleButton.onClick.AddListener(OnStartBattleClicked);
                Debug.Log("[StoryModeUI] 已绑定开始战斗按钮");
            }
            else
            {
                Debug.LogWarning("[StoryModeUI] startBattleButton 引用为空！");
            }

            if (backButton != null)
            {
                backButton.onClick.RemoveAllListeners();
                backButton.onClick.AddListener(OnBackClicked);
                Debug.Log("[StoryModeUI] 已绑定返回按钮");
            }
            else
            {
                Debug.LogWarning("[StoryModeUI] backButton 引用为空！");
            }
        }

        /// <summary>
        /// 刷新战役列表
        /// </summary>
        public void RefreshCampaignList()
        {
            // 清除现有按钮
            foreach (var btn in campaignButtons)
            {
                Destroy(btn);
            }
            campaignButtons.Clear();

            if (StoryModeManager.Instance == null) return;

            var campaigns = StoryModeManager.Instance.GetAllCampaigns();

            for (int i = 0; i < campaigns.Count; i++)
            {
                CreateCampaignButton(campaigns[i], i);
            }

            // 默认选择第一个战役
            if (campaigns.Count > 0)
            {
                SelectCampaign(0);
            }
        }

        /// <summary>
        /// 创建战役按钮
        /// </summary>
        private void CreateCampaignButton(CampaignData campaign, int index)
        {
            GameObject btnObj;

            if (campaignButtonPrefab != null)
            {
                btnObj = Instantiate(campaignButtonPrefab, campaignListContainer);
            }
            else
            {
                btnObj = CreateDefaultButton(campaignListContainer);
            }

            campaignButtons.Add(btnObj);

            // 设置按钮文本
            TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                string campaignName = GetLocalizedText(campaign.nameKey);
                btnText.text = campaign.isUnlocked ? campaignName : "???";

                if (campaign.isCompleted)
                {
                    btnText.text += " *";
                }

                UI.TMPFontHelper.SetFontByLanguage(btnText);
            }

            // 设置按钮交互
            Button btn = btnObj.GetComponent<Button>();
            if (btn != null)
            {
                int capturedIndex = index;
                btn.onClick.AddListener(() => SelectCampaign(capturedIndex));
                btn.interactable = campaign.isUnlocked;
            }

            // 设置按钮颜色
            Image btnImage = btnObj.GetComponent<Image>();
            if (btnImage != null)
            {
                if (!campaign.isUnlocked)
                {
                    btnImage.color = new Color(0.3f, 0.3f, 0.3f);
                }
                else if (campaign.isCompleted)
                {
                    btnImage.color = new Color(0.2f, 0.6f, 0.2f);
                }
                else
                {
                    btnImage.color = new Color(0.3f, 0.3f, 0.5f);
                }
            }
        }

        /// <summary>
        /// 刷新战斗列表
        /// </summary>
        public void RefreshBattleList()
        {
            // 清除现有按钮
            foreach (var btn in battleButtons)
            {
                Destroy(btn);
            }
            battleButtons.Clear();

            var campaign = StoryModeManager.Instance?.GetSelectedCampaign();
            if (campaign == null) return;

            for (int i = 0; i < campaign.battles.Count; i++)
            {
                CreateBattleButton(campaign.battles[i], i);
            }

            // 默认选择第一个战斗
            if (campaign.battles.Count > 0)
            {
                SelectBattle(0);
            }
        }

        /// <summary>
        /// 创建战斗按钮
        /// </summary>
        private void CreateBattleButton(BattleData battle, int index)
        {
            GameObject btnObj;

            if (battleButtonPrefab != null)
            {
                btnObj = Instantiate(battleButtonPrefab, battleListContainer);
            }
            else
            {
                btnObj = CreateDefaultButton(battleListContainer);
            }

            battleButtons.Add(btnObj);

            // 设置按钮文本
            TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                string battleName = GetLocalizedText(battle.nameKey);
                string prefix = $"{index + 1}. ";
                btnText.text = battle.isUnlocked ? (prefix + battleName) : (prefix + "???");

                if (battle.isCompleted)
                {
                    btnText.text += " *";
                }

                UI.TMPFontHelper.SetFontByLanguage(btnText);
            }

            // 设置按钮交互
            Button btn = btnObj.GetComponent<Button>();
            if (btn != null)
            {
                int capturedIndex = index;
                btn.onClick.AddListener(() => SelectBattle(capturedIndex));
                btn.interactable = battle.isUnlocked;
            }

            // 设置按钮颜色
            Image btnImage = btnObj.GetComponent<Image>();
            if (btnImage != null)
            {
                if (!battle.isUnlocked)
                {
                    btnImage.color = new Color(0.3f, 0.3f, 0.3f);
                }
                else if (battle.isCompleted)
                {
                    btnImage.color = new Color(0.2f, 0.5f, 0.2f);
                }
                else
                {
                    btnImage.color = new Color(0.4f, 0.4f, 0.5f);
                }
            }
        }

        /// <summary>
        /// 选择战役
        /// </summary>
        public void SelectCampaign(int index)
        {
            selectedCampaignIndex = index;
            StoryModeManager.Instance?.SelectCampaign(index);

            // ⭐ 动态加载战役图片
            LoadCampaignImage(index);

            // 更新战役按钮高亮
            UpdateCampaignButtonHighlights();

            // 刷新战斗列表
            RefreshBattleList();
        }

        /// <summary>
        /// ⭐ 动态加载战役图片
        /// </summary>
        private void LoadCampaignImage(int index)
        {
            var campaign = StoryModeManager.Instance?.GetSelectedCampaign();
            if (campaign == null || detailImage == null) return;

            // 如果已有图片则直接显示
            if (campaign.campaignImage != null)
            {
                detailImage.sprite = campaign.campaignImage;
                detailImage.color = Color.white;  // ⭐ 确保原色显示
                return;
            }

            // 根据campaignId加载图片
            string imageName = GetCampaignImageName(campaign.campaignId);
            if (!string.IsNullOrEmpty(imageName))
            {
                // 尝试多种路径
                string[] paths = new string[]
                {
                    $"Sprites/Campaigns/{imageName}",
                    $"Sprites/Campaign/{imageName}",
                    $"Sprites/Story/{imageName}",
                    $"Images/Campaigns/{imageName}",
                };

                foreach (string path in paths)
                {
                    Sprite sprite = Resources.Load<Sprite>(path);
                    if (sprite != null)
                    {
                        campaign.campaignImage = sprite;
                        detailImage.sprite = sprite;  // ⭐ 立即显示图片
                        detailImage.color = Color.white;  // ⭐ 确保原色显示
                        Debug.Log($"[StoryModeUI] 加载战役图片成功: {path}");
                        break;
                    }
                }

                if (campaign.campaignImage == null)
                {
                    Debug.LogWarning($"[StoryModeUI] 未找到战役图片: {imageName}");
                }
            }
        }

        /// <summary>
        /// ⭐ 获取战役图片名称
        /// </summary>
        private string GetCampaignImageName(string campaignId)
        {
            switch (campaignId?.ToLower())
            {
                case "chibi": return "ChiBi";
                case "guandu": return "GuanDu";
                case "taodong": return "TaoDong";
                default: return campaignId;
            }
        }

        /// <summary>
        /// 选择战斗
        /// </summary>
        public void SelectBattle(int index)
        {
            selectedBattleIndex = index;
            StoryModeManager.Instance?.SelectBattle(index);

            // 更新战斗按钮高亮
            UpdateBattleButtonHighlights();

            // 更新详情面板
            UpdateDetailPanel();
        }

        /// <summary>
        /// 更新战役按钮高亮
        /// </summary>
        private void UpdateCampaignButtonHighlights()
        {
            var campaigns = StoryModeManager.Instance?.GetAllCampaigns();
            if (campaigns == null) return;

            for (int i = 0; i < campaignButtons.Count && i < campaigns.Count; i++)
            {
                Image btnImage = campaignButtons[i].GetComponent<Image>();
                if (btnImage != null)
                {
                    bool isSelected = (i == selectedCampaignIndex);
                    var campaign = campaigns[i];

                    if (!campaign.isUnlocked)
                    {
                        btnImage.color = new Color(0.3f, 0.3f, 0.3f);
                    }
                    else if (isSelected)
                    {
                        btnImage.color = new Color(0.5f, 0.5f, 0.8f); // 选中高亮
                    }
                    else if (campaign.isCompleted)
                    {
                        btnImage.color = new Color(0.2f, 0.6f, 0.2f);
                    }
                    else
                    {
                        btnImage.color = new Color(0.3f, 0.3f, 0.5f);
                    }
                }
            }
        }

        /// <summary>
        /// 更新战斗按钮高亮
        /// </summary>
        private void UpdateBattleButtonHighlights()
        {
            var campaign = StoryModeManager.Instance?.GetSelectedCampaign();
            if (campaign == null) return;

            for (int i = 0; i < battleButtons.Count && i < campaign.battles.Count; i++)
            {
                Image btnImage = battleButtons[i].GetComponent<Image>();
                if (btnImage != null)
                {
                    bool isSelected = (i == selectedBattleIndex);
                    var battle = campaign.battles[i];

                    if (!battle.isUnlocked)
                    {
                        btnImage.color = new Color(0.3f, 0.3f, 0.3f);
                    }
                    else if (isSelected)
                    {
                        btnImage.color = new Color(0.6f, 0.6f, 0.9f); // 选中高亮
                    }
                    else if (battle.isCompleted)
                    {
                        btnImage.color = new Color(0.2f, 0.5f, 0.2f);
                    }
                    else
                    {
                        btnImage.color = new Color(0.4f, 0.4f, 0.5f);
                    }
                }
            }
        }

        /// <summary>
        /// 更新详情面板
        /// </summary>
        private void UpdateDetailPanel()
        {
            var battle = StoryModeManager.Instance?.GetSelectedBattle();
            var campaign = StoryModeManager.Instance?.GetSelectedCampaign();

            if (battle == null || campaign == null)
            {
                // 显示战役信息
                if (campaign != null)
                {
                    ShowCampaignDetail(campaign);
                }
                return;
            }

            ShowBattleDetail(battle);
        }

        /// <summary>
        /// 显示战役详情
        /// </summary>
        private void ShowCampaignDetail(CampaignData campaign)
        {
            if (titleText != null)
            {
                titleText.text = campaign.isUnlocked ? GetLocalizedText(campaign.nameKey) : "???";
                UI.TMPFontHelper.SetFontByLanguage(titleText);
            }

            if (descriptionText != null)
            {
                descriptionText.text = campaign.isUnlocked ? GetLocalizedText(campaign.descriptionKey) : GetLocalizedText("story_locked");
                UI.TMPFontHelper.SetFontByLanguage(descriptionText);
            }

            if (detailImage != null)
            {
                if (campaign.isUnlocked && campaign.campaignImage != null)
                {
                    detailImage.sprite = campaign.campaignImage;
                }
                else if (!campaign.isUnlocked && lockedImage != null)
                {
                    detailImage.sprite = lockedImage;
                }
                else if (defaultCampaignImage != null)
                {
                    detailImage.sprite = defaultCampaignImage;
                }
            }

            if (difficultyText != null)
            {
                difficultyText.gameObject.SetActive(false);
            }

            if (specialRuleText != null)
            {
                specialRuleText.gameObject.SetActive(false);
            }

            if (completedBadge != null)
            {
                completedBadge.SetActive(campaign.isCompleted);
            }

            if (startBattleButton != null)
            {
                startBattleButton.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 显示战斗详情
        /// </summary>
        private void ShowBattleDetail(BattleData battle)
        {
            if (titleText != null)
            {
                titleText.text = battle.isUnlocked ? GetLocalizedText(battle.nameKey) : "???";
                UI.TMPFontHelper.SetFontByLanguage(titleText);
            }

            if (descriptionText != null)
            {
                if (battle.isUnlocked)
                {
                    string desc = GetLocalizedText(battle.descriptionKey);
                    string brief = GetLocalizedText(battle.briefingKey);
                    descriptionText.text = $"{desc}\n\n{brief}";
                }
                else
                {
                    descriptionText.text = GetLocalizedText("story_locked");
                }
                UI.TMPFontHelper.SetFontByLanguage(descriptionText);
            }

            if (detailImage != null)
            {
                if (battle.isUnlocked && battle.battleImage != null)
                {
                    detailImage.sprite = battle.battleImage;
                }
                else if (!battle.isUnlocked && lockedImage != null)
                {
                    detailImage.sprite = lockedImage;
                }
                else if (defaultBattleImage != null)
                {
                    detailImage.sprite = defaultBattleImage;
                }
            }

            if (difficultyText != null)
            {
                difficultyText.gameObject.SetActive(battle.isUnlocked);
                if (battle.isUnlocked)
                {
                    string diffLabel = GetLocalizedText("story_difficulty");
                    string stars = new string('*', battle.difficulty);
                    difficultyText.text = $"{diffLabel}: {stars}";
                    UI.TMPFontHelper.SetFontByLanguage(difficultyText);
                }
            }

            if (specialRuleText != null)
            {
                bool hasRule = battle.isUnlocked && !string.IsNullOrEmpty(battle.specialRuleKey);
                specialRuleText.gameObject.SetActive(hasRule);
                if (hasRule)
                {
                    string ruleLabel = GetLocalizedText("story_special_rule");
                    string rule = GetLocalizedText(battle.specialRuleKey);
                    specialRuleText.text = $"{ruleLabel}: {rule}";
                    UI.TMPFontHelper.SetFontByLanguage(specialRuleText);
                }
            }

            if (completedBadge != null)
            {
                completedBadge.SetActive(battle.isCompleted);
            }

            if (startBattleButton != null)
            {
                startBattleButton.gameObject.SetActive(true);
                startBattleButton.interactable = battle.isUnlocked;

                TextMeshProUGUI btnText = startBattleButton.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null)
                {
                    btnText.text = GetLocalizedText("story_start_battle");
                    UI.TMPFontHelper.SetFontByLanguage(btnText);
                }
            }
        }

        /// <summary>
        /// 开始战斗按钮点击
        /// </summary>
        private void OnStartBattleClicked()
        {
            StoryModeManager.Instance?.StartBattle();
        }

        /// <summary>
        /// 返回按钮点击
        /// </summary>
        private void OnBackClicked()
        {
            SceneManager.LoadScene("MainMenu");
        }

        /// <summary>
        /// 创建默认按钮
        /// </summary>
        private GameObject CreateDefaultButton(Transform parent)
        {
            GameObject btnObj = new GameObject("StoryButton");
            btnObj.transform.SetParent(parent, false);

            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(280, 50);

            Image img = btnObj.AddComponent<Image>();
            img.color = new Color(0.3f, 0.3f, 0.5f);

            Button btn = btnObj.AddComponent<Button>();

            // 添加文本
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);

            RectTransform textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(10, 5);
            textRt.offsetMax = new Vector2(-10, -5);

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.fontSize = 18;
            text.alignment = TextAlignmentOptions.Left;
            text.color = Color.white;

            // 设置字体
            UI.TMPFontHelper.SetFontByLanguage(text);

            return btnObj;
        }

        /// <summary>
        /// 获取本地化文本
        /// </summary>
        private string GetLocalizedText(string key)
        {
            if (string.IsNullOrEmpty(key)) return "";

            if (LocalizationManager.Instance != null)
            {
                return LocalizationManager.Instance.GetText(key);
            }
            return key;
        }
    }
}
