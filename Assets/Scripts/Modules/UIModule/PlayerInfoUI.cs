using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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

        private List<GameObject> hpIcons = new List<GameObject>();

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
        /// 更新手牌数量
        /// </summary>
        private void UpdateHandCount()
        {
            if (handCountText != null)
            {
                if (isLocalPlayer)
                {
                    handCountText.text = $"手牌: {playerData.handCards.Count}";
                }
                else
                {
                    // 其他玩家只显示数量
                    handCountText.text = $"手牌: {playerData.handCards.Count} 张";
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

            // 显示装备
            foreach (var equipment in playerData.equipments)
            {
                GameObject slot = CreateEquipmentSlot(equipment);
                slot.transform.SetParent(equipmentContainer, false);
            }
        }

        /// <summary>
        /// 创建装备槽
        /// </summary>
        private GameObject CreateEquipmentSlot(Card equipment)
        {
            if (equipmentSlotPrefab != null)
            {
                GameObject slot = Instantiate(equipmentSlotPrefab);
                // 设置装备信息
                TextMeshProUGUI text = slot.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = equipment.cardName;
                }
                return slot;
            }
            else
            {
                // 创建简单的装备显示
                GameObject slot = new GameObject("Equipment");
                Image img = slot.AddComponent<Image>();
                img.color = new Color(0.8f, 0.7f, 0.5f);

                RectTransform rt = slot.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(60, 40);

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
            if (avatarImage != null)
            {
                // 简易版:根据阵营设置颜色
                Color factionColor = GetFactionColor(playerData.faction);
                avatarImage.color = factionColor;

                // 在头像上显示武将名字首字
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

                TextMeshProUGUI avatarText = textTransform.GetComponent<TextMeshProUGUI>();
                if (avatarText != null && !string.IsNullOrEmpty(playerData.generalName))
                {
                    avatarText.text = playerData.generalName.Substring(0, 1);
                }
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
        /// 显示操作面板
        /// </summary>
        public void ShowActionPanel(bool show)
        {
            if (actionPanel != null)
            {
                actionPanel.SetActive(show);
            }
        }

        /// <summary>
        /// 播放受伤动画
        /// </summary>
        public void PlayDamageAnimation()
        {
            StartCoroutine(FlashColor(Color.red));
        }

        /// <summary>
        /// 播放回复动画
        /// </summary>
        public void PlayRecoverAnimation()
        {
            StartCoroutine(FlashColor(Color.green));
        }

        /// <summary>
        /// 颜色闪烁动画
        /// </summary>
        private System.Collections.IEnumerator FlashColor(Color flashColor)
        {
            Image bg = GetComponent<Image>();
            if (bg != null)
            {
                Color originalColor = bg.color;
                float duration = 0.2f;

                // 变色
                float elapsed = 0f;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    bg.color = Color.Lerp(originalColor, flashColor, elapsed / duration);
                    yield return null;
                }

                // 恢复
                elapsed = 0f;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    bg.color = Color.Lerp(flashColor, originalColor, elapsed / duration);
                    yield return null;
                }

                bg.color = originalColor;
            }
        }
    }
}