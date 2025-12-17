using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using ThreeKingdoms.DatabaseModule;

namespace ThreeKingdoms.UI
{
    /// <summary>
    /// 玩家信息UI控制器
    /// </summary>
    public class PlayerInfoUI : MonoBehaviour
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

        [Header("技能按钮")]
        public Transform skillButtonContainer;
        public GameObject skillButtonPrefab;

        private List<GameObject> hpIcons = new List<GameObject>();
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

            // 更新头像(简易版用文字)
            UpdateAvatar();

            // ⭐ 更新技能按钮
            UpdateSkillButtons();
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
    }
}