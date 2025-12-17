using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using ThreeKingdoms.DatabaseModule;

namespace ThreeKingdoms.UI
{
    /// <summary>
    /// 玩家信息UI控制器
    /// ⭐ 支持右键点击查看技能信息和使用主动技能
    /// </summary>
    public class PlayerInfoUI : MonoBehaviour, IPointerClickHandler
    {
        [Header("玩家数据")]
        public Player playerData;
        public bool isLocalPlayer = false;

        [Header("UI引用")]
        public TextMeshProUGUI playerNameText;
        public TextMeshProUGUI generalNameText;
        public TextMeshProUGUI factionText;  // ⭐ 阵营文本
        public Image avatarImage;
        public GameObject hpContainer;
        public GameObject hpIconPrefab;
        public TextMeshProUGUI handCountText;
        public Image currentPlayerIndicator;
        public GameObject actionPanel;

        [Header("状态指示")]
        public Image aliveIndicator;
        public Color aliveColor = Color.green;
        public Color deadColor = Color.red;

        [Header("装备区")]
        public Transform equipmentContainer;
        public GameObject equipmentSlotPrefab;

        [Header("判定区")]
        public Transform judgeAreaContainer;
        public GameObject judgeCardPrefab;

        [Header("技能按钮")]
        public Transform skillButtonContainer;
        public GameObject skillButtonPrefab;

        [Header("技能信息弹窗")]
        public GameObject skillInfoPanel;
        public TextMeshProUGUI skillInfoText;

        private List<GameObject> hpIcons = new List<GameObject>();
        private List<GameObject> judgeCardIcons = new List<GameObject>();
        private SkillButtonContainer skillContainer;

        /// <summary>
        /// 设置玩家数据
        /// </summary>
        public void SetPlayer(Player player, bool isLocal = false)
        {
            playerData = player;
            isLocalPlayer = isLocal;

            UpdateDisplay();
        }

        /// <summary>
        /// 更新显示
        /// </summary>
        public void UpdateDisplay()
        {
            if (playerData == null) return;

            // 更新玩家名称
            if (playerNameText != null)
            {
                playerNameText.text = playerData.playerName;
            }

            // 更新武将名称
            if (generalNameText != null)
            {
                generalNameText.text = playerData.generalName;
            }

            // ⭐ 更新阵营显示
            UpdateFactionDisplay();

            // 更新体力显示
            UpdateHP();

            // 更新手牌数量
            UpdateHandCount();

            // 更新存活状态
            UpdateAliveStatus();

            // 更新装备
            UpdateEquipments();

            // ⭐ 更新判定区
            UpdateJudgeArea();

            // 更新头像(简易版用文字)
            UpdateAvatar();

            // ⭐ 技能按钮已移至二级面板（右键点击玩家面板查看技能信息）
            // UpdateSkillButtons();
        }

        /// <summary>
        /// ⭐ 更新阵营显示（本地化）
        /// </summary>
        private void UpdateFactionDisplay()
        {
            if (factionText != null && playerData != null)
            {
                // ⭐ 使用本地化文本
                string factionKey = $"faction_{playerData.faction.ToString().ToLower()}";
                string factionName = LocalizationManager.Instance.GetText(factionKey);

                factionText.text = factionName;

                // 设置阵营颜色
                Color factionColor = GetFactionColor(playerData.faction);
                factionText.color = factionColor;

                // ⭐ 设置韩文字体
                if (LocalizationManager.Instance.GetCurrentLanguage() == Language.Korean)
                {
                    factionText.font = TMPFontHelper.GetKoreanFont();
                }
            }
        }

        /// <summary>
        /// ⭐ 获取阵营名称（本地化）
        /// </summary>
        private string GetFactionName(Faction faction)
        {
            if (LocalizationManager.Instance == null)
            {
                // fallback
                switch (faction)
                {
                    case Faction.Wei: return "魏";
                    case Faction.Shu: return "蜀";
                    case Faction.Wu: return "吴";
                    case Faction.Qun: return "群";
                    default: return "未知";
                }
            }

            switch (faction)
            {
                case Faction.Wei: return LocalizationManager.Instance.GetText("faction_wei");
                case Faction.Shu: return LocalizationManager.Instance.GetText("faction_shu");
                case Faction.Wu: return LocalizationManager.Instance.GetText("faction_wu");
                case Faction.Qun: return LocalizationManager.Instance.GetText("faction_qun");
                default: return LocalizationManager.Instance.GetText("faction_unknown");
            }
        }

        /// <summary>
        /// 更新体力显示
        /// </summary>
        private void UpdateHP()
        {
            if (hpContainer == null) return;

            // 清除旧的体力图标
            foreach (var icon in hpIcons)
            {
                Destroy(icon);
            }
            hpIcons.Clear();

            // 创建新的体力图标
            for (int i = 0; i < playerData.maxHP; i++)
            {
                GameObject hpIcon;

                if (hpIconPrefab != null)
                {
                    hpIcon = Instantiate(hpIconPrefab, hpContainer.transform);
                }
                else
                {
                    // 创建简单的体力图标
                    hpIcon = new GameObject($"HP_{i}");
                    hpIcon.transform.SetParent(hpContainer.transform);
                    Image img = hpIcon.AddComponent<Image>();
                    img.color = i < playerData.currentHP ? Color.red : new Color(0.3f, 0.3f, 0.3f);

                    RectTransform rt = hpIcon.GetComponent<RectTransform>();
                    rt.sizeDelta = new Vector2(30, 30);

                    // 添加体力文本
                    GameObject textObj = new GameObject("Text");
                    textObj.transform.SetParent(hpIcon.transform);
                    TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
                    text.text = "♥";
                    text.fontSize = 20;
                    text.alignment = TextAlignmentOptions.Center;
                    text.color = Color.white;

                    RectTransform textRt = textObj.GetComponent<RectTransform>();
                    textRt.anchorMin = Vector2.zero;
                    textRt.anchorMax = Vector2.one;
                    textRt.sizeDelta = Vector2.zero;
                }

                hpIcons.Add(hpIcon);
            }
        }

        /// <summary>
        /// 更新手牌数量（本地化）
        /// </summary>
        private void UpdateHandCount()
        {
            if (handCountText != null)
            {
                string handLabel = LocalizationManager.Instance != null
                    ? LocalizationManager.Instance.GetText("ui_hand_cards")
                    : "手牌";
                string countUnit = LocalizationManager.Instance != null
                    ? LocalizationManager.Instance.GetText("ui_cards")
                    : "张";

                if (isLocalPlayer)
                {
                    handCountText.text = $"{handLabel}: {playerData.handCards.Count}";
                }
                else
                {
                    // 其他玩家只显示数量
                    handCountText.text = $"{handLabel}: {playerData.handCards.Count} {countUnit}";
                }
            }
        }

        /// <summary>
        /// 更新存活状态
        /// </summary>
        private void UpdateAliveStatus()
        {
            if (aliveIndicator != null)
            {
                aliveIndicator.color = playerData.isAlive ? aliveColor : deadColor;
            }

            // 如果死亡,整体降低透明度
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = playerData.isAlive ? 1f : 0.5f;
        }

        /// <summary>
        /// 更新装备
        /// </summary>
        private void UpdateEquipments()
        {
            if (equipmentContainer == null) return;

            // 清空现有装备显示
            foreach (Transform child in equipmentContainer)
            {
                Destroy(child.gameObject);
            }

            // 创建装备槽位
            foreach (var equipment in playerData.equipments)
            {
                GameObject slot = CreateEquipmentSlot(equipment);
                if (slot != null)
                {
                    slot.transform.SetParent(equipmentContainer);
                }
            }
        }

        /// <summary>
        /// 创建装备槽位
        /// </summary>
        private GameObject CreateEquipmentSlot(Card equipment)
        {
            if (equipmentSlotPrefab != null)
            {
                GameObject slot = Instantiate(equipmentSlotPrefab);
                // TODO: 设置装备信息
                return slot;
            }
            else
            {
                // 创建简单的装备槽
                GameObject slot = new GameObject($"Equipment_{equipment.cardName}");
                Image img = slot.AddComponent<Image>();
                img.color = new Color(0.8f, 0.6f, 0.2f);

                RectTransform rt = slot.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(60, 80);

                // 添加装备名称
                GameObject textObj = new GameObject("Text");
                textObj.transform.SetParent(slot.transform);
                TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
                text.text = equipment.cardName;
                text.fontSize = 14;
                text.alignment = TextAlignmentOptions.Center;
                text.color = Color.black;

                RectTransform textRt = textObj.GetComponent<RectTransform>();
                textRt.anchorMin = Vector2.zero;
                textRt.anchorMax = Vector2.one;
                textRt.sizeDelta = Vector2.zero;

                return slot;
            }
        }

        /// <summary>
        /// ⭐ 更新判定区显示（延时锦囊牌）
        /// </summary>
        private void UpdateJudgeArea()
        {
            // 清除旧的判定牌图标
            foreach (var icon in judgeCardIcons)
            {
                if (icon != null) Destroy(icon);
            }
            judgeCardIcons.Clear();

            if (playerData == null || playerData.judgeCards == null || playerData.judgeCards.Count == 0)
                return;

            // 如果没有指定容器，创建一个
            if (judgeAreaContainer == null)
            {
                CreateJudgeAreaContainer();
            }

            // 创建判定牌图标
            foreach (var card in playerData.judgeCards)
            {
                GameObject judgeIcon = CreateJudgeCardIcon(card);
                if (judgeIcon != null)
                {
                    judgeIcon.transform.SetParent(judgeAreaContainer);
                    judgeIcon.transform.localScale = Vector3.one;
                    judgeCardIcons.Add(judgeIcon);
                }
            }
        }

        /// <summary>
        /// ⭐ 创建判定区容器
        /// </summary>
        private void CreateJudgeAreaContainer()
        {
            GameObject containerObj = new GameObject("JudgeAreaContainer");
            containerObj.transform.SetParent(transform, false);

            RectTransform rt = containerObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 0.5f);
            rt.anchorMax = new Vector2(1, 0.5f);
            rt.pivot = new Vector2(0, 0.5f);
            rt.anchoredPosition = new Vector2(5, 0);
            rt.sizeDelta = new Vector2(100, 30);

            // 添加水平布局
            HorizontalLayoutGroup hlg = containerObj.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 2;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            judgeAreaContainer = containerObj.transform;
        }

        /// <summary>
        /// ⭐ 创建判定牌图标
        /// </summary>
        private GameObject CreateJudgeCardIcon(Card card)
        {
            if (judgeCardPrefab != null)
            {
                GameObject icon = Instantiate(judgeCardPrefab);
                // 设置判定牌信息
                TextMeshProUGUI text = icon.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = GetJudgeCardShortName(card.cardName);
                }
                return icon;
            }
            else
            {
                // 创建简单的判定牌图标
                GameObject icon = new GameObject($"Judge_{card.cardName}");

                Image img = icon.AddComponent<Image>();
                img.color = GetJudgeCardColor(card.cardName);

                RectTransform rt = icon.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(28, 28);

                // 添加判定牌名称缩写
                GameObject textObj = new GameObject("Text");
                textObj.transform.SetParent(icon.transform, false);
                TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
                text.text = GetJudgeCardShortName(card.cardName);
                text.fontSize = 12;
                text.alignment = TextAlignmentOptions.Center;
                text.color = Color.white;
                text.fontStyle = FontStyles.Bold;

                RectTransform textRt = textObj.GetComponent<RectTransform>();
                textRt.anchorMin = Vector2.zero;
                textRt.anchorMax = Vector2.one;
                textRt.sizeDelta = Vector2.zero;
                textRt.anchoredPosition = Vector2.zero;

                // 添加ToolTip提示
                AddJudgeCardTooltip(icon, card);

                return icon;
            }
        }

        /// <summary>
        /// ⭐ 获取判定牌缩写名
        /// </summary>
        private string GetJudgeCardShortName(string cardName)
        {
            switch (cardName)
            {
                case "乐不思蜀": return "乐";
                case "闪电": return "电";
                case "兵粮寸断": return "粮";
                default: return cardName.Length > 0 ? cardName.Substring(0, 1) : "?";
            }
        }

        /// <summary>
        /// ⭐ 获取判定牌背景颜色
        /// </summary>
        private Color GetJudgeCardColor(string cardName)
        {
            switch (cardName)
            {
                case "乐不思蜀": return new Color(0.9f, 0.5f, 0.1f); // 橙色
                case "闪电": return new Color(0.3f, 0.5f, 0.9f);     // 蓝色
                case "兵粮寸断": return new Color(0.5f, 0.3f, 0.1f); // 棕色
                default: return new Color(0.5f, 0.5f, 0.5f);
            }
        }

        /// <summary>
        /// ⭐ 添加判定牌鼠标悬停提示
        /// </summary>
        private void AddJudgeCardTooltip(GameObject icon, Card card)
        {
            // 添加EventTrigger用于显示提示
            EventTrigger trigger = icon.AddComponent<EventTrigger>();

            // 鼠标进入
            EventTrigger.Entry enterEntry = new EventTrigger.Entry();
            enterEntry.eventID = EventTriggerType.PointerEnter;
            enterEntry.callback.AddListener((data) => {
                string tooltip = $"【{card.cardName}】\n{GetJudgeCardDescription(card.cardName)}";
                ShowTooltip(tooltip, icon.transform.position);
            });
            trigger.triggers.Add(enterEntry);

            // 鼠标离开
            EventTrigger.Entry exitEntry = new EventTrigger.Entry();
            exitEntry.eventID = EventTriggerType.PointerExit;
            exitEntry.callback.AddListener((data) => {
                HideTooltip();
            });
            trigger.triggers.Add(exitEntry);
        }

        /// <summary>
        /// ⭐ 获取判定牌效果描述
        /// </summary>
        private string GetJudgeCardDescription(string cardName)
        {
            switch (cardName)
            {
                case "乐不思蜀": return "判定非红桃则跳过出牌阶段";
                case "闪电": return "判定黑桃2-9则受到3点雷电伤害";
                case "兵粮寸断": return "判定非梅花则跳过摸牌阶段";
                default: return "";
            }
        }

        // ⭐ 提示框相关（简易实现）
        private static GameObject tooltipObject;
        private static TextMeshProUGUI tooltipText;

        private void ShowTooltip(string text, Vector3 position)
        {
            if (tooltipObject == null)
            {
                CreateTooltip();
            }
            if (tooltipObject != null)
            {
                tooltipObject.SetActive(true);
                tooltipText.text = text;
                tooltipObject.transform.position = position + new Vector3(50, 20, 0);
            }
        }

        private void HideTooltip()
        {
            if (tooltipObject != null)
            {
                tooltipObject.SetActive(false);
            }
        }

        private void CreateTooltip()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            tooltipObject = new GameObject("JudgeCardTooltip");
            tooltipObject.transform.SetParent(canvas.transform, false);

            Image bg = tooltipObject.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.85f);

            RectTransform rt = tooltipObject.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(180, 60);

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(tooltipObject.transform, false);
            tooltipText = textObj.AddComponent<TextMeshProUGUI>();
            tooltipText.fontSize = 14;
            tooltipText.alignment = TextAlignmentOptions.Center;
            tooltipText.color = Color.white;
            TMPFontHelper.SetFontByLanguage(tooltipText);  // ⭐ 设置字体

            RectTransform textRt = textObj.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.sizeDelta = new Vector2(-10, -10);
            textRt.anchoredPosition = Vector2.zero;

            tooltipObject.SetActive(false);
        }

        /// <summary>
        /// 更新头像
        /// </summary>
        private void UpdateAvatar()
        {
            if (avatarImage == null || playerData == null) return;

            // 方案1: 如果GeneralData中直接有avatar引用
            if (playerData.generalData != null && playerData.generalData.avatar != null)
            {
                avatarImage.sprite = playerData.generalData.avatar;
                avatarImage.color = Color.white;  // 使用原图颜色

                // 清除文字（因为已经有图片了）
                Transform textTransform = avatarImage.transform.Find("AvatarText");
                if (textTransform != null)
                {
                    textTransform.gameObject.SetActive(false);
                }
            }
            // 方案2: 如果使用路径动态加载
            else if (playerData.generalData != null && !string.IsNullOrEmpty(playerData.generalData.avatarPath))
            {
                Sprite loadedSprite = Resources.Load<Sprite>($"Sprites/Characters/{playerData.generalData.avatarPath}");

                if (loadedSprite != null)
                {
                    avatarImage.sprite = loadedSprite;
                    avatarImage.color = Color.white;

                    // 清除文字
                    Transform textTransform = avatarImage.transform.Find("AvatarText");
                    if (textTransform != null)
                    {
                        textTransform.gameObject.SetActive(false);
                    }
                }
                else
                {
                    Debug.LogWarning($"无法加载头像: Sprites/Characters/{playerData.generalData.avatarPath}");
                    UseFallbackAvatar();
                }
            }
            // 方案3: 根据generalId自动加载（使用拼音映射）
            else if (playerData.generalData != null)
            {
                // ⭐ 使用共享的拼音映射工具加载
                Sprite loadedSprite = CharacterPinyinHelper.LoadCharacterSprite(
                    playerData.generalData.generalId,
                    playerData.faction
                );

                if (loadedSprite != null)
                {
                    avatarImage.sprite = loadedSprite;
                    avatarImage.color = Color.white;

                    // 清除文字
                    Transform textTransform = avatarImage.transform.Find("AvatarText");
                    if (textTransform != null)
                    {
                        textTransform.gameObject.SetActive(false);
                    }
                }
                else
                {
                    Debug.LogWarning($"无法加载头像: {playerData.generalData.generalId}，使用备用方案");
                    UseFallbackAvatar();
                }
            }
            // 备用方案: 使用纯色 + 文字
            else
            {
                UseFallbackAvatar();
            }
        }

        /// <summary>
        /// 使用备用头像（纯色+文字）
        /// </summary>
        private void UseFallbackAvatar()
        {
            // 根据阵营设置颜色
            Color factionColor = GetFactionColor(playerData.faction);
            avatarImage.color = factionColor;
            avatarImage.sprite = null;  // 不使用图片

            // 显示武将名字首字
            Transform textTransform = avatarImage.transform.Find("AvatarText");
            if (textTransform == null)
            {
                GameObject textObj = new GameObject("AvatarText");
                textObj.transform.SetParent(avatarImage.transform);
                TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
                text.fontSize = 36;
                text.alignment = TextAlignmentOptions.Center;
                text.color = Color.white;
                text.fontStyle = FontStyles.Bold;

                RectTransform rt = textObj.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.sizeDelta = Vector2.zero;

                textTransform = textObj.transform;
            }

            textTransform.gameObject.SetActive(true);
            TextMeshProUGUI avatarText = textTransform.GetComponent<TextMeshProUGUI>();
            if (avatarText != null && !string.IsNullOrEmpty(playerData.generalName))
            {
                avatarText.text = playerData.generalName.Substring(0, 1);
            }
        }

        /// <summary>
        /// 获取阵营颜色
        /// </summary>
        private Color GetFactionColor(Faction faction)
        {
            switch (faction)
            {
                case Faction.Wei: return new Color(0.2f, 0.4f, 0.8f);  // 蓝色
                case Faction.Shu: return new Color(0.8f, 0.3f, 0.3f);  // 红色
                case Faction.Wu: return new Color(0.3f, 0.7f, 0.3f);   // 绿色
                case Faction.Qun: return new Color(0.6f, 0.6f, 0.6f);  // 灰色
                default: return Color.white;
            }
        }

        /// <summary>
        /// 设置当前玩家指示器
        /// </summary>
        public void SetCurrentPlayer(bool isCurrent)
        {
            if (currentPlayerIndicator != null)
            {
                currentPlayerIndicator.gameObject.SetActive(isCurrent);
            }

            // 添加简单的缩放动画效果
            if (isCurrent)
            {
                StartCoroutine(PulseAnimation());
            }
        }

        /// <summary>
        /// 简单的脉冲动画
        /// </summary>
        private System.Collections.IEnumerator PulseAnimation()
        {
            Vector3 originalScale = transform.localScale;
            Vector3 targetScale = originalScale * 1.05f;

            float duration = 0.25f;
            float elapsed = 0f;

            // 放大
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
                yield return null;
            }

            elapsed = 0f;
            // 缩小回去
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
                yield return null;
            }

            transform.localScale = originalScale;
        }

        /// <summary>
        /// 播放受伤动画
        /// </summary>
        public void PlayDamageAnimation()
        {
            StartCoroutine(DamageFlashAnimation());
        }

        /// <summary>
        /// 受伤闪烁动画
        /// </summary>
        private System.Collections.IEnumerator DamageFlashAnimation()
        {
            Image background = GetComponent<Image>();
            if (background == null) yield break;

            Color originalColor = background.color;
            Color damageColor = Color.red;

            // 闪红
            background.color = damageColor;
            yield return new WaitForSeconds(0.1f);

            // 恢复
            background.color = originalColor;
        }

        /// <summary>
        /// 播放回复动画
        /// </summary>
        public void PlayRecoverAnimation()
        {
            StartCoroutine(RecoverFlashAnimation());
        }

        /// <summary>
        /// 回复闪烁动画
        /// </summary>
        private System.Collections.IEnumerator RecoverFlashAnimation()
        {
            Image background = GetComponent<Image>();
            if (background == null) yield break;

            Color originalColor = background.color;
            Color healColor = Color.green;

            // 闪绿
            background.color = healColor;
            yield return new WaitForSeconds(0.1f);

            // 恢复
            background.color = originalColor;
        }

        /// <summary>
        /// ⭐ 更新技能按钮
        /// </summary>
        private void UpdateSkillButtons()
        {
            if (playerData == null) return;

            // 只为本地玩家显示技能按钮
            if (!isLocalPlayer) return;

            // 如果已经创建过技能按钮，只更新状态
            if (skillContainer != null)
            {
                skillContainer.UpdateAllButtonStates();
                return;
            }

            // 检查是否有主动技能
            bool hasActiveSkills = false;
            foreach (var skill in playerData.skills)
            {
                if (skill != null && skill.SkillData != null &&
                    skill.SkillData.skillType == DatabaseModule.SkillType.Active)
                {
                    hasActiveSkills = true;
                    break;
                }
            }

            if (!hasActiveSkills) return;

            // 首次创建技能按钮容器
            CreateSkillButtonContainer();

            // 初始化技能按钮
            if (skillContainer != null)
            {
                skillContainer.Initialize(playerData);
            }
        }

        /// <summary>
        /// ⭐ 创建技能按钮容器
        /// </summary>
        private void CreateSkillButtonContainer()
        {
            Transform containerParent = skillButtonContainer ?? transform;

            // 如果没有指定容器，创建一个
            if (skillButtonContainer == null)
            {
                GameObject containerObj = new GameObject("SkillButtonContainer");
                containerObj.transform.SetParent(transform, false);

                RectTransform rt = containerObj.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0, 0);
                rt.anchorMax = new Vector2(1, 0);
                rt.pivot = new Vector2(0.5f, 0);
                rt.anchoredPosition = new Vector2(0, -10);
                rt.sizeDelta = new Vector2(0, 40);

                // 添加水平布局
                HorizontalLayoutGroup hlg = containerObj.AddComponent<HorizontalLayoutGroup>();
                hlg.spacing = 5;
                hlg.childAlignment = TextAnchor.MiddleCenter;
                hlg.childControlWidth = false;
                hlg.childControlHeight = false;
                hlg.childForceExpandWidth = false;
                hlg.childForceExpandHeight = false;

                containerParent = containerObj.transform;
            }

            // 添加SkillButtonContainer组件
            skillContainer = containerParent.gameObject.GetComponent<SkillButtonContainer>();
            if (skillContainer == null)
            {
                skillContainer = containerParent.gameObject.AddComponent<SkillButtonContainer>();
            }

            skillContainer.buttonContainer = containerParent;
            skillContainer.skillButtonPrefab = skillButtonPrefab;
        }

        /// <summary>
        /// ⭐ 刷新技能按钮状态
        /// </summary>
        public void RefreshSkillButtonStates()
        {
            if (skillContainer != null)
            {
                skillContainer.UpdateAllButtonStates();
            }
        }

        #region ⭐ 右键点击功能 - 查看技能信息和使用主动技能

        /// <summary>
        /// ⭐ 处理鼠标点击事件
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            // 右键点击显示技能信息
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                ShowSkillInfoPanel();
            }
        }

        /// <summary>
        /// ⭐ 显示技能信息面板
        /// </summary>
        private void ShowSkillInfoPanel()
        {
            if (playerData == null) return;

            // 创建或显示技能信息面板
            if (skillInfoPanel == null)
            {
                CreateSkillInfoPanel();
            }

            if (skillInfoPanel != null)
            {
                skillInfoPanel.SetActive(true);
                UpdateSkillInfoContent();
            }
        }

        /// <summary>
        /// ⭐ 创建技能信息面板
        /// </summary>
        private void CreateSkillInfoPanel()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            // 创建面板背景
            skillInfoPanel = new GameObject("SkillInfoPanel");
            skillInfoPanel.transform.SetParent(canvas.transform, false);

            Image panelBg = skillInfoPanel.AddComponent<Image>();
            panelBg.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);

            RectTransform panelRt = skillInfoPanel.GetComponent<RectTransform>();
            panelRt.sizeDelta = new Vector2(320, 400);
            // ⭐ 靠左显示
            panelRt.anchorMin = new Vector2(0, 0.5f);
            panelRt.anchorMax = new Vector2(0, 0.5f);
            panelRt.pivot = new Vector2(0, 0.5f);
            panelRt.anchoredPosition = new Vector2(20, 0);

            // 添加标题
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(skillInfoPanel.transform, false);
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = $"【{playerData.generalName}】技能";
            titleText.fontSize = 20;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = new Color(1f, 0.85f, 0.4f);
            TMPFontHelper.SetFontByLanguage(titleText);  // ⭐ 设置字体

            RectTransform titleRt = titleObj.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0, 1);
            titleRt.anchorMax = new Vector2(1, 1);
            titleRt.pivot = new Vector2(0.5f, 1);
            titleRt.anchoredPosition = new Vector2(0, -10);
            titleRt.sizeDelta = new Vector2(0, 30);

            // 添加内容区域
            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(skillInfoPanel.transform, false);
            skillInfoText = contentObj.AddComponent<TextMeshProUGUI>();
            skillInfoText.fontSize = 14;
            skillInfoText.alignment = TextAlignmentOptions.TopLeft;
            skillInfoText.color = Color.white;
            skillInfoText.richText = true;  // ⭐ 启用富文本
            TMPFontHelper.SetFontByLanguage(skillInfoText);  // ⭐ 设置字体

            RectTransform contentRt = contentObj.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 0);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0.5f, 0.5f);
            contentRt.offsetMin = new Vector2(15, 60);
            contentRt.offsetMax = new Vector2(-15, -50);

            // 添加关闭按钮
            GameObject closeBtn = new GameObject("CloseButton");
            closeBtn.transform.SetParent(skillInfoPanel.transform, false);
            Image closeBtnImg = closeBtn.AddComponent<Image>();
            closeBtnImg.color = new Color(0.8f, 0.2f, 0.2f);

            RectTransform closeBtnRt = closeBtn.GetComponent<RectTransform>();
            closeBtnRt.anchorMin = new Vector2(1, 1);
            closeBtnRt.anchorMax = new Vector2(1, 1);
            closeBtnRt.pivot = new Vector2(1, 1);
            closeBtnRt.anchoredPosition = new Vector2(-5, -5);
            closeBtnRt.sizeDelta = new Vector2(30, 30);

            Button closeButton = closeBtn.AddComponent<Button>();
            closeButton.onClick.AddListener(() => skillInfoPanel.SetActive(false));

            GameObject closeTxtObj = new GameObject("X");
            closeTxtObj.transform.SetParent(closeBtn.transform, false);
            TextMeshProUGUI closeTxt = closeTxtObj.AddComponent<TextMeshProUGUI>();
            closeTxt.text = "X";
            closeTxt.fontSize = 18;
            closeTxt.fontStyle = FontStyles.Bold;
            closeTxt.alignment = TextAlignmentOptions.Center;
            closeTxt.color = Color.white;

            RectTransform closeTxtRt = closeTxtObj.GetComponent<RectTransform>();
            closeTxtRt.anchorMin = Vector2.zero;
            closeTxtRt.anchorMax = Vector2.one;
            closeTxtRt.sizeDelta = Vector2.zero;

            // 添加技能按钮容器
            CreateSkillActionButtons();
        }

        /// <summary>
        /// ⭐ 创建技能操作按钮（用于使用主动技能）
        /// </summary>
        private void CreateSkillActionButtons()
        {
            if (skillInfoPanel == null || playerData == null) return;

            GameObject buttonsContainer = new GameObject("SkillButtons");
            buttonsContainer.transform.SetParent(skillInfoPanel.transform, false);

            RectTransform containerRt = buttonsContainer.AddComponent<RectTransform>();
            containerRt.anchorMin = new Vector2(0, 0);
            containerRt.anchorMax = new Vector2(1, 0);
            containerRt.pivot = new Vector2(0.5f, 0);
            containerRt.anchoredPosition = new Vector2(0, 10);
            containerRt.sizeDelta = new Vector2(0, 40);

            HorizontalLayoutGroup hlg = buttonsContainer.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;

            // 为每个主动技能创建按钮
            foreach (var skill in playerData.skills)
            {
                if (skill == null || skill.SkillData == null) continue;
                if (skill.SkillData.skillType != DatabaseModule.SkillType.Active) continue;

                CreateSkillUseButton(buttonsContainer.transform, skill);
            }
        }

        /// <summary>
        /// ⭐ 创建单个技能使用按钮
        /// </summary>
        private void CreateSkillUseButton(Transform parent, DatabaseModule.ISkill skill)
        {
            GameObject btnObj = new GameObject($"Btn_{skill.SkillData.skillName}");
            btnObj.transform.SetParent(parent, false);

            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0.2f, 0.5f, 0.8f);
            btnImg.raycastTarget = true;  // ⭐ 确保可点击

            RectTransform btnRt = btnObj.GetComponent<RectTransform>();
            btnRt.sizeDelta = new Vector2(80, 35);

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnImg;  // ⭐ 设置按钮目标图形
            btn.onClick.AddListener(() => OnSkillButtonClicked(skill));

            // ⭐ 设置按钮颜色过渡
            ColorBlock colors = btn.colors;
            colors.normalColor = new Color(0.2f, 0.5f, 0.8f);
            colors.highlightedColor = new Color(0.3f, 0.6f, 0.9f);
            colors.pressedColor = new Color(0.15f, 0.4f, 0.7f);
            btn.colors = colors;

            GameObject txtObj = new GameObject("Text");
            txtObj.transform.SetParent(btnObj.transform, false);
            TextMeshProUGUI txt = txtObj.AddComponent<TextMeshProUGUI>();
            txt.text = skill.SkillData.skillName;
            txt.fontSize = 14;
            txt.alignment = TextAlignmentOptions.Center;
            txt.color = Color.white;
            txt.raycastTarget = false;  // ⭐ 文字不阻挡点击
            TMPFontHelper.SetFontByLanguage(txt);  // ⭐ 设置字体

            RectTransform txtRt = txtObj.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.sizeDelta = Vector2.zero;
        }

        /// <summary>
        /// ⭐ 技能按钮点击事件
        /// </summary>
        private void OnSkillButtonClicked(DatabaseModule.ISkill skill)
        {
            if (skill == null) return;

            // 关闭面板
            if (skillInfoPanel != null)
            {
                skillInfoPanel.SetActive(false);
            }

            // 检查是否可以使用技能
            if (!skill.CanTrigger())
            {
                BattleUI.Instance?.ShowMessage($"【{skill.SkillData.skillName}】当前无法使用");
                return;
            }

            // 触发技能
            Debug.Log($"[技能] 尝试使用主动技能: {skill.SkillData.skillName}");
            skill.Trigger();
        }

        /// <summary>
        /// ⭐ 更新技能信息内容
        /// </summary>
        private void UpdateSkillInfoContent()
        {
            if (skillInfoText == null || playerData == null) return;

            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            // 显示武将基本信息
            sb.AppendLine($"<color=#AAAAAA>体力: {playerData.currentHP}/{playerData.maxHP}</color>");
            sb.AppendLine($"<color=#AAAAAA>阵营: {GetFactionName(playerData.faction)}</color>");
            sb.AppendLine();

            // 显示技能信息
            if (playerData.skills == null || playerData.skills.Count == 0)
            {
                sb.AppendLine("<color=#888888>该武将没有技能</color>");
            }
            else
            {
                foreach (var skill in playerData.skills)
                {
                    if (skill == null || skill.SkillData == null) continue;

                    // 技能名称（根据类型着色）
                    string typeColor = GetSkillTypeColor(skill.SkillData.skillType);
                    string typeName = GetSkillTypeName(skill.SkillData.skillType);

                    sb.AppendLine($"<color={typeColor}>【{skill.SkillData.skillName}】</color> <size=12><color=#888888>[{typeName}]</color></size>");

                    // 技能描述 - 优先使用 GetDescription()，备选 SkillData.description
                    string desc = skill.GetDescription();
                    if (string.IsNullOrEmpty(desc))
                    {
                        desc = skill.SkillData.description;
                    }
                    if (string.IsNullOrEmpty(desc))
                    {
                        desc = "暂无描述";
                    }
                    sb.AppendLine($"<color=#CCCCCC>{desc}</color>");
                    sb.AppendLine();
                }
            }

            skillInfoText.text = sb.ToString();
        }

        /// <summary>
        /// ⭐ 获取技能类型颜色
        /// </summary>
        private string GetSkillTypeColor(DatabaseModule.SkillType skillType)
        {
            switch (skillType)
            {
                case DatabaseModule.SkillType.Active: return "#FFD700";    // 金色 - 主动技能
                case DatabaseModule.SkillType.Passive: return "#90EE90";   // 浅绿 - 被动技能
                case DatabaseModule.SkillType.Trigger: return "#87CEEB";   // 天蓝 - 触发技能
                case DatabaseModule.SkillType.Limit: return "#DDA0DD";     // 梅红 - 限定技能
                default: return "#FFFFFF";
            }
        }

        /// <summary>
        /// ⭐ 获取技能类型名称
        /// </summary>
        private string GetSkillTypeName(DatabaseModule.SkillType skillType)
        {
            switch (skillType)
            {
                case DatabaseModule.SkillType.Active: return "主动技";
                case DatabaseModule.SkillType.Passive: return "被动技";
                case DatabaseModule.SkillType.Trigger: return "触发技";
                case DatabaseModule.SkillType.Limit: return "限定技";
                default: return "未知";
            }
        }

        #endregion
    }
}