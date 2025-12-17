using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ThreeKingdoms.DatabaseModule;

namespace ThreeKingdoms.UI
{
    /// <summary>
    /// 技能按钮UI
    /// 管理单个技能按钮的显示和交互
    /// </summary>
    public class SkillButtonUI : MonoBehaviour
    {
        [Header("UI组件")]
        public Button button;
        public TextMeshProUGUI skillNameText;
        public Image backgroundImage;
        public Image iconImage;

        [Header("颜色设置")]
        public Color enabledColor = new Color(0.2f, 0.5f, 0.3f);
        public Color disabledColor = new Color(0.3f, 0.3f, 0.3f);
        public Color pressedColor = new Color(0.1f, 0.4f, 0.2f);

        // 技能引用
        private ISkill skill;
        private Player owner;

        /// <summary>
        /// 初始化技能按钮
        /// </summary>
        public void Initialize(ISkill skill, Player owner)
        {
            this.skill = skill;
            this.owner = owner;

            // 设置技能名称
            if (skillNameText != null && skill.SkillData != null)
            {
                // 尝试获取本地化名称
                string skillName = skill.SkillData.skillName;
                if (LocalizationManager.Instance != null)
                {
                    string localizedName = LocalizationManager.Instance.GetText($"skill_{skill.SkillData.skillId}");
                    if (localizedName != $"skill_{skill.SkillData.skillId}")
                    {
                        skillName = localizedName;
                    }
                }
                skillNameText.text = skillName;
                TMPFontHelper.SetFontByLanguage(skillNameText);
            }

            // 设置技能图标
            if (iconImage != null && skill.SkillData != null && skill.SkillData.icon != null)
            {
                iconImage.sprite = skill.SkillData.icon;
                iconImage.gameObject.SetActive(true);
            }
            else if (iconImage != null)
            {
                iconImage.gameObject.SetActive(false);
            }

            // 设置按钮点击事件
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnButtonClicked);
            }

            // 更新按钮状态
            UpdateButtonState();
        }

        /// <summary>
        /// 更新按钮状态
        /// </summary>
        public void UpdateButtonState()
        {
            if (skill == null || button == null) return;

            bool canTrigger = skill.CanTrigger();

            // 更新按钮交互性
            button.interactable = canTrigger;

            // 更新背景颜色
            if (backgroundImage != null)
            {
                backgroundImage.color = canTrigger ? enabledColor : disabledColor;
            }

            // 更新文字颜色
            if (skillNameText != null)
            {
                skillNameText.color = canTrigger ? Color.white : new Color(0.6f, 0.6f, 0.6f);
            }
        }

        /// <summary>
        /// 按钮点击处理
        /// </summary>
        private void OnButtonClicked()
        {
            Debug.Log($"[SkillButtonUI] 按钮被点击");

            if (skill == null)
            {
                Debug.LogWarning("[SkillButtonUI] 技能为空!");
                return;
            }

            if (!skill.CanTrigger())
            {
                Debug.Log($"[SkillButtonUI] 技能 {skill.SkillData?.skillName} 当前无法触发");
                return;
            }

            Debug.Log($"[SkillButtonUI] 触发技能: {skill.SkillData?.skillName}");

            // 触发技能
            skill.Trigger();

            // 更新按钮状态
            UpdateButtonState();

            // 通知UI更新（延迟一帧，避免重复调用）
            StartCoroutine(DelayedUIUpdate());
        }

        /// <summary>
        /// 延迟UI更新
        /// </summary>
        private System.Collections.IEnumerator DelayedUIUpdate()
        {
            yield return null;
            if (BattleUI.Instance != null)
            {
                BattleUI.Instance.UpdateAllPlayerInfo();
            }
        }

        /// <summary>
        /// 获取技能数据
        /// </summary>
        public ISkill GetSkill()
        {
            return skill;
        }

        /// <summary>
        /// 设置按钮可见性
        /// </summary>
        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }
    }

    /// <summary>
    /// 技能按钮容器
    /// 管理一个玩家的所有技能按钮
    /// </summary>
    public class SkillButtonContainer : MonoBehaviour
    {
        [Header("设置")]
        public Transform buttonContainer;
        public GameObject skillButtonPrefab;

        [Header("布局")]
        public float buttonWidth = 80f;
        public float buttonHeight = 30f;
        public float buttonSpacing = 5f;

        private Player player;
        private System.Collections.Generic.List<SkillButtonUI> skillButtons = new System.Collections.Generic.List<SkillButtonUI>();
        private bool isInitialized = false;

        /// <summary>
        /// 初始化技能按钮容器
        /// </summary>
        public void Initialize(Player player)
        {
            // 防止重复初始化
            if (isInitialized && this.player == player && skillButtons.Count > 0)
            {
                // 已经初始化过，只更新状态
                UpdateAllButtonStates();
                return;
            }

            this.player = player;

            // 清除现有按钮（如果有的话）
            if (skillButtons.Count > 0)
            {
                ClearButtons();
            }

            if (player == null || player.skills == null) return;

            // 为每个主动技能创建按钮
            int createdCount = 0;
            foreach (var skill in player.skills)
            {
                if (skill == null || skill.SkillData == null) continue;

                // 只为主动技能创建按钮
                if (skill.SkillData.skillType == SkillType.Active)
                {
                    CreateSkillButton(skill);
                    createdCount++;
                }
            }

            isInitialized = true;

            // 只在首次创建时打印日志
            if (createdCount > 0)
            {
                Debug.Log($"[SkillButtonContainer] 为 {player.playerName} 创建了 {createdCount} 个技能按钮");
            }
        }

        /// <summary>
        /// 创建技能按钮
        /// </summary>
        private void CreateSkillButton(ISkill skill)
        {
            GameObject buttonObj;
            SkillButtonUI buttonUI;

            if (skillButtonPrefab != null)
            {
                buttonObj = Instantiate(skillButtonPrefab, buttonContainer ?? transform);
                buttonUI = buttonObj.GetComponent<SkillButtonUI>();
                if (buttonUI == null)
                {
                    buttonUI = buttonObj.AddComponent<SkillButtonUI>();
                }
            }
            else
            {
                // 动态创建按钮（已包含SkillButtonUI组件）
                buttonObj = CreateDefaultSkillButton(skill);
                buttonUI = buttonObj.GetComponent<SkillButtonUI>();
            }

            buttonUI.Initialize(skill, player);
            skillButtons.Add(buttonUI);
        }

        /// <summary>
        /// 创建默认技能按钮（无预制体时）
        /// </summary>
        private GameObject CreateDefaultSkillButton(ISkill skill)
        {
            GameObject buttonObj = new GameObject($"SkillButton_{skill.SkillData?.skillId}");
            buttonObj.transform.SetParent(buttonContainer ?? transform, false);

            // RectTransform
            RectTransform rt = buttonObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(buttonWidth, buttonHeight);

            // 背景图片
            Image bg = buttonObj.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.5f, 0.3f, 0.9f);

            // 按钮组件
            Button btn = buttonObj.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f);
            btn.colors = colors;

            // 技能名称文本
            GameObject textObj = new GameObject("SkillName");
            textObj.transform.SetParent(buttonObj.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = new Vector2(5, 2);
            textRT.offsetMax = new Vector2(-5, -2);

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.fontSize = 14;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;

            // 设置SkillButtonUI组件的引用
            SkillButtonUI skillButtonUI = buttonObj.AddComponent<SkillButtonUI>();
            skillButtonUI.button = btn;
            skillButtonUI.skillNameText = text;
            skillButtonUI.backgroundImage = bg;

            return buttonObj;
        }

        /// <summary>
        /// 清除所有按钮
        /// </summary>
        public void ClearButtons()
        {
            foreach (var btn in skillButtons)
            {
                if (btn != null)
                {
                    Destroy(btn.gameObject);
                }
            }
            skillButtons.Clear();
            isInitialized = false;
        }

        /// <summary>
        /// 更新所有按钮状态
        /// </summary>
        public void UpdateAllButtonStates()
        {
            foreach (var btn in skillButtons)
            {
                if (btn != null)
                {
                    btn.UpdateButtonState();
                }
            }
        }

        /// <summary>
        /// 设置容器可见性
        /// </summary>
        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        private void OnDestroy()
        {
            ClearButtons();
        }
    }
}
