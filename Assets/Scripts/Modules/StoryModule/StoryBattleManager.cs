using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using ThreeKingdoms.DatabaseModule;
using static ThreeKingdoms.DatabaseModule.CharacterPinyinHelper;

namespace ThreeKingdoms.Story
{
    /// <summary>
    /// 故事战斗管理器
    /// 管理故事模式战斗的事件、胜负条件等
    /// </summary>
    public class StoryBattleManager : MonoBehaviour
    {
        public static StoryBattleManager Instance { get; private set; }

        [Header("当前战斗")]
        public StoryBattle currentBattle;
        public int currentRound = 0;

        [Header("UI引用")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private Image speakerPortrait;
        [SerializeField] private TextMeshProUGUI speakerNameText;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private Button continueButton;

        [Header("胜利条件UI")]
        [SerializeField] private GameObject victoryConditionPanel;
        [SerializeField] private TextMeshProUGUI victoryConditionText;
        [SerializeField] private TextMeshProUGUI defeatConditionText;
        [SerializeField] private TextMeshProUGUI battleNameText;

        [Header("状态")]
        public bool isBattleActive = false;
        public bool isDialogueShowing = false;

        // ⭐ 开场对话状态
        private bool openingDialogueShown = false;
        private List<Dialogue> pendingOpeningDialogue = null;

        // 战斗状态追踪
        private Dictionary<string, int> markers = new Dictionary<string, int>(); // 标记计数
        private Dictionary<string, int> eventCounts = new Dictionary<string, int>(); // 事件计数
        private Player playerCharacter;
        private List<BattleEvent> pendingEvents = new List<BattleEvent>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // 注册事件监听
            RegisterEventListeners();

            // ⭐ 确保对话框UI被找到并隐藏（防止阻挡卡牌操作）
            EnsureDialoguePanelHidden();

            if (continueButton != null)
                continueButton.onClick.AddListener(OnContinueDialogue);
        }

        /// <summary>
        /// ⭐ 确保对话框面板被找到并隐藏
        /// </summary>
        private void EnsureDialoguePanelHidden()
        {
            Debug.Log($"[StoryBattleManager] EnsureDialoguePanelHidden 开始, dialoguePanel已有引用: {dialoguePanel != null}");

            // 如果已经有引用（在Inspector中赋值），直接隐藏
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
                BindDialogueUIReferences();
                Debug.Log("[StoryBattleManager] 使用Inspector中已有的dialoguePanel引用");
                return;
            }

            // 尝试查找场景中的DialoguePanel并隐藏
            // 注意：GameObject.Find 只能找到 active 的对象，所以要在隐藏之前找到并保存引用
            GameObject existingPanel = GameObject.Find("DialoguePanel");
            Debug.Log($"[StoryBattleManager] GameObject.Find(DialoguePanel) 结果: {existingPanel != null}");

            if (existingPanel == null)
            {
                // 也尝试在Canvas下查找（Transform.Find 可以找到 inactive 的子对象）
                Canvas[] allCanvases = FindObjectsOfType<Canvas>();
                Debug.Log($"[StoryBattleManager] 场景中Canvas数量: {allCanvases.Length}");

                foreach (Canvas canvas in allCanvases)
                {
                    Transform found = canvas.transform.Find("DialoguePanel");
                    if (found != null)
                    {
                        existingPanel = found.gameObject;
                        Debug.Log($"[StoryBattleManager] 在Canvas '{canvas.name}' 下找到DialoguePanel");
                        break;
                    }
                }
            }

            if (existingPanel != null)
            {
                // ⭐ 关键：保存引用到 dialoguePanel 字段，这样后续不需要再查找
                dialoguePanel = existingPanel;
                dialoguePanel.SetActive(false);

                // ⭐ 同时绑定子元素引用
                BindDialogueUIReferences();

                Debug.Log("[StoryBattleManager] Start时找到并隐藏了DialoguePanel");
            }
            else
            {
                Debug.LogWarning("[StoryBattleManager] Start时未找到DialoguePanel，将在需要时动态创建");
            }
        }

        /// <summary>
        /// ⭐ 绑定对话UI的子元素引用
        /// </summary>
        private void BindDialogueUIReferences()
        {
            if (dialoguePanel == null) return;

            // 说话人名称
            if (speakerNameText == null)
            {
                Transform speakerNameTrans = FindChildRecursive(dialoguePanel.transform, "SpeakerName");
                if (speakerNameTrans != null)
                    speakerNameText = speakerNameTrans.GetComponent<TextMeshProUGUI>();
            }

            // 对话文本
            if (dialogueText == null)
            {
                Transform dialogueTextTrans = FindChildRecursive(dialoguePanel.transform, "DialogueText");
                if (dialogueTextTrans != null)
                    dialogueText = dialogueTextTrans.GetComponent<TextMeshProUGUI>();
            }

            // 左侧画像
            if (leftPortrait == null)
            {
                Transform leftTrans = FindChildRecursive(dialoguePanel.transform, "LeftPortrait");
                if (leftTrans == null)
                    leftTrans = FindChildRecursive(dialoguePanel.transform, "LeftPortraitFrame");
                if (leftTrans != null)
                {
                    leftPortraitFrame = leftTrans.gameObject;
                    Transform portraitTrans = leftTrans.Find("Portrait");
                    if (portraitTrans != null)
                        leftPortrait = portraitTrans.GetComponent<Image>();
                    else
                        leftPortrait = leftTrans.GetComponent<Image>();
                }
            }

            // 右侧画像
            if (rightPortrait == null)
            {
                Transform rightTrans = FindChildRecursive(dialoguePanel.transform, "RightPortrait");
                if (rightTrans == null)
                    rightTrans = FindChildRecursive(dialoguePanel.transform, "RightPortraitFrame");
                if (rightTrans != null)
                {
                    rightPortraitFrame = rightTrans.gameObject;
                    Transform portraitTrans = rightTrans.Find("Portrait");
                    if (portraitTrans != null)
                        rightPortrait = portraitTrans.GetComponent<Image>();
                    else
                        rightPortrait = rightTrans.GetComponent<Image>();
                }
            }

            // ⭐ 添加点击事件
            Button panelButton = dialoguePanel.GetComponent<Button>();
            if (panelButton == null)
            {
                panelButton = dialoguePanel.AddComponent<Button>();
            }
            panelButton.onClick.RemoveAllListeners();
            panelButton.onClick.AddListener(OnContinueDialogue);

            // 确保有Image组件（Button需要）
            Image panelImage = dialoguePanel.GetComponent<Image>();
            if (panelImage == null)
            {
                panelImage = dialoguePanel.AddComponent<Image>();
                panelImage.color = new Color(0, 0, 0, 0.01f);
            }

            Debug.Log($"[StoryBattleManager] 绑定UI引用 - SpeakerName:{speakerNameText != null}, DialogueText:{dialogueText != null}");
        }

        private void OnDestroy()
        {
            UnregisterEventListeners();
        }

        private void RegisterEventListeners()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.OnTurnStart += OnTurnStart;
                EventManager.Instance.OnTurnEnd += OnTurnEnd;
                EventManager.Instance.OnPlayerDamaged += OnPlayerDamaged;
                EventManager.Instance.OnPlayerDeath += OnPlayerDeath;
                EventManager.Instance.OnCardPlayed += OnCardPlayed;
                EventManager.Instance.OnSkillTriggered += OnSkillTriggered;
                EventManager.Instance.OnStoryEventTriggered += OnStoryEventTriggered;
            }
        }

        private void UnregisterEventListeners()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.OnTurnStart -= OnTurnStart;
                EventManager.Instance.OnTurnEnd -= OnTurnEnd;
                EventManager.Instance.OnPlayerDamaged -= OnPlayerDamaged;
                EventManager.Instance.OnPlayerDeath -= OnPlayerDeath;
                EventManager.Instance.OnCardPlayed -= OnCardPlayed;
                EventManager.Instance.OnSkillTriggered -= OnSkillTriggered;
                EventManager.Instance.OnStoryEventTriggered -= OnStoryEventTriggered;
            }
        }

        #region 战斗初始化

        /// <summary>
        /// 开始故事战斗
        /// </summary>
        public void StartBattle(StoryBattle battle)
        {
            currentBattle = battle;
            currentRound = 0;
            isBattleActive = true;
            markers.Clear();
            eventCounts.Clear();

            // ⭐ 重置开场对话状态
            openingDialogueShown = false;
            pendingOpeningDialogue = null;

            // 重置事件触发状态
            foreach (var evt in battle.events)
            {
                evt.triggered = false;
            }

            Debug.Log($"[StoryBattle] 开始战斗: {battle.battleId}");

            // ⭐ 显示胜利条件UI
            ShowVictoryConditionUI();

            // 应用特殊规则
            ApplySpecialRules();

            // ⭐ 存储开场对白，等待出牌阶段再显示
            if (battle.openingDialogue != null && battle.openingDialogue.Count > 0)
            {
                pendingOpeningDialogue = battle.openingDialogue;
                Debug.Log($"[StoryBattle] 开场对话已存储，等待出牌阶段显示 ({pendingOpeningDialogue.Count} 句)");
            }

            // 触发战斗开始事件（不等对话）
            TriggerEvents(EventTrigger.OnBattleStart, "");
        }

        /// <summary>
        /// 应用特殊规则
        /// </summary>
        private void ApplySpecialRules()
        {
            if (currentBattle?.specialRules == null) return;

            foreach (var rule in currentBattle.specialRules)
            {
                if (rule.triggerTurn == 0) // 战斗开始时生效
                {
                    ApplyRule(rule);
                }
            }
        }

        /// <summary>
        /// 应用单个规则
        /// </summary>
        private void ApplyRule(SpecialRule rule)
        {
            Debug.Log($"[StoryBattle] 应用规则: {rule.nameKey}");

            switch (rule.type)
            {
                case RuleType.ModifyInitialCards:
                    // 修改初始手牌数
                    if (!string.IsNullOrEmpty(rule.targetId))
                    {
                        var player = FindPlayer(rule.targetId);
                        if (player != null)
                        {
                            // 减少手牌
                            for (int i = 0; i < -rule.value && player.handCards.Count > 0; i++)
                            {
                                player.DiscardCard(player.handCards[0]);
                            }
                        }
                    }
                    break;

                case RuleType.ModifyMaxHP:
                    // 修改血量上限
                    if (!string.IsNullOrEmpty(rule.targetId))
                    {
                        var player = FindPlayer(rule.targetId);
                        if (player != null)
                        {
                            player.maxHP += rule.value;
                            player.currentHP += rule.value;
                        }
                    }
                    break;

                case RuleType.ModifyAttackRange:
                    // 修改攻击距离
                    if (!string.IsNullOrEmpty(rule.targetId))
                    {
                        var player = FindPlayer(rule.targetId);
                        if (player != null)
                        {
                            player.attackRange += rule.value;
                        }
                    }
                    break;

                case RuleType.FireDamageBonus:
                    // 火焰伤害加成 - 存储到markers
                    markers["fire_damage_bonus"] = rule.value;
                    break;

                default:
                    break;
            }
        }

        #endregion

        #region 事件处理

        private void OnTurnStart(Player player)
        {
            if (!isBattleActive) return;

            // 检查回合开始事件
            TriggerEvents(EventTrigger.OnTurnStart, player.generalName);
        }

        private void OnTurnEnd(Player player)
        {
            if (!isBattleActive) return;

            // 如果是第一个玩家回合结束，增加回合数
            if (BattleManager.Instance != null && BattleManager.Instance.players.Count > 0)
            {
                if (player == BattleManager.Instance.players[0])
                {
                    currentRound++;
                    OnRoundStart();
                }
            }
        }

        private void OnRoundStart()
        {
            Debug.Log($"[StoryBattle] 第 {currentRound} 回合开始");

            // 检查回合规则
            if (currentBattle?.specialRules != null)
            {
                foreach (var rule in currentBattle.specialRules)
                {
                    if (rule.triggerTurn == currentRound)
                    {
                        ApplyRule(rule);
                    }
                }
            }

            // 触发回合开始事件
            TriggerEvents(EventTrigger.OnRoundStart, currentRound.ToString());

            // 检查胜负条件
            CheckVictoryCondition();
            CheckDefeatCondition();
        }

        private void OnPlayerDamaged(Player victim, Player source, int damage, Card card)
        {
            if (!isBattleActive) return;

            TriggerEvents(EventTrigger.OnDamageTaken, victim.generalName);

            // 检查血量低于阈值事件
            TriggerEvents(EventTrigger.OnHPBelow, victim.generalName, victim.currentHP.ToString());

            // 检查濒死
            if (victim.currentHP <= 0)
            {
                TriggerEvents(EventTrigger.OnNearDeath, victim.generalName);
            }
        }

        private void OnPlayerDeath(Player victim, Player killer)
        {
            if (!isBattleActive) return;

            TriggerEvents(EventTrigger.OnDeath, victim.generalName);

            // 检查胜负条件
            CheckVictoryCondition();
            CheckDefeatCondition();
        }

        private void OnCardPlayed(Player player, Card card)
        {
            if (!isBattleActive) return;

            // 检查首次出杀
            if (CardNameHelper.IsSlash(card))
            {
                if (!eventCounts.ContainsKey($"kill_{player.generalName}"))
                {
                    eventCounts[$"kill_{player.generalName}"] = 1;
                    TriggerEvents(EventTrigger.OnFirstKill, player.generalName);
                }
            }

            TriggerEvents(EventTrigger.OnCardPlayed, card.cardName);
        }

        private void OnSkillTriggered(Player player, string skillId)
        {
            if (!isBattleActive) return;

            TriggerEvents(EventTrigger.OnSkillActivate, skillId);
        }

        private void OnStoryEventTriggered(string eventType, string param)
        {
            if (!isBattleActive) return;

            // 处理特殊事件
            if (eventType == "marker_zhaxiang")
            {
                int count = int.Parse(param);
                markers["zhaxiang"] = count;
                TriggerEvents(EventTrigger.OnMarkerGained, "zhaxiang");
                CheckVictoryCondition();
            }
            else if (eventType == "hand_viewed")
            {
                if (!eventCounts.ContainsKey("hand_viewed_huanggai"))
                    eventCounts["hand_viewed_huanggai"] = 0;
                eventCounts["hand_viewed_huanggai"]++;
                TriggerEvents(EventTrigger.OnHandCardViewed, param);
                CheckDefeatCondition();
            }
        }

        /// <summary>
        /// 触发符合条件的事件
        /// </summary>
        private void TriggerEvents(EventTrigger trigger, string param, string param2 = "")
        {
            if (currentBattle?.events == null) return;

            foreach (var evt in currentBattle.events)
            {
                if (evt.trigger == trigger &&
                    (string.IsNullOrEmpty(evt.triggerParam) || evt.triggerParam == param))
                {
                    // 检查param2（如果有）
                    if (!string.IsNullOrEmpty(evt.triggerParam2) && evt.triggerParam2 != param2)
                        continue;

                    // 检查是否已触发（非重复事件）
                    if (!evt.repeatable && evt.triggered)
                        continue;

                    evt.triggered = true;

                    // 显示对白
                    if (evt.dialogues != null && evt.dialogues.Count > 0)
                    {
                        StartCoroutine(ShowDialogueSequence(evt.dialogues, null));
                    }
                }
            }
        }

        #endregion

        #region 胜负条件检测

        /// <summary>
        /// 检查胜利条件
        /// </summary>
        public void CheckVictoryCondition()
        {
            if (!isBattleActive || currentBattle?.victoryCondition == null) return;

            bool victory = false;
            var condition = currentBattle.victoryCondition;

            switch (condition.type)
            {
                case VictoryType.DefeatAllEnemies:
                    victory = AreAllEnemiesDefeated();
                    break;

                case VictoryType.DefeatTarget:
                    victory = IsTargetDefeated(condition.targetCharacterId);
                    break;

                case VictoryType.SurviveTurns:
                    victory = currentRound >= condition.targetTurn;
                    break;

                case VictoryType.AccumulateMarks:
                    if (markers.TryGetValue("zhaxiang", out int count))
                    {
                        victory = count >= condition.targetCount;
                    }
                    break;

                case VictoryType.ProtectAlly:
                    // 保护目标存活且所有敌人被击败
                    var ally = FindPlayer(condition.targetCharacterId);
                    victory = ally != null && ally.isAlive && AreAllEnemiesDefeated();
                    break;

                default:
                    break;
            }

            if (victory)
            {
                OnVictory();
            }
        }

        /// <summary>
        /// 检查失败条件
        /// </summary>
        public void CheckDefeatCondition()
        {
            if (!isBattleActive || currentBattle?.defeatCondition == null) return;

            bool defeat = false;
            var condition = currentBattle.defeatCondition;

            switch (condition.type)
            {
                case DefeatType.PlayerDeath:
                    defeat = playerCharacter != null && !playerCharacter.isAlive;
                    break;

                case DefeatType.AllyDeath:
                    var ally = FindPlayer(condition.targetCharacterId);
                    defeat = ally != null && !ally.isAlive;
                    break;

                case DefeatType.AllAlliesDeath:
                    defeat = AreAllAlliesDefeated();
                    break;

                case DefeatType.ExceedCount:
                    // 检查特定计数（如蒋干连续3次查看黄盖手牌）
                    if (eventCounts.TryGetValue("hand_viewed_huanggai", out int count))
                    {
                        defeat = count >= condition.maxCount;
                    }
                    break;

                case DefeatType.TurnLimitExceeded:
                    defeat = currentBattle.turnLimit > 0 && currentRound > currentBattle.turnLimit;
                    break;

                default:
                    break;
            }

            if (defeat)
            {
                OnDefeat();
            }
        }

        private bool AreAllEnemiesDefeated()
        {
            if (BattleManager.Instance == null) return false;

            foreach (var player in BattleManager.Instance.players)
            {
                // 假设敌人是非我方阵营
                if (player.isAlive && player.faction != playerCharacter?.faction)
                {
                    return false;
                }
            }
            return true;
        }

        private bool AreAllAlliesDefeated()
        {
            if (BattleManager.Instance == null) return true;

            foreach (var player in BattleManager.Instance.players)
            {
                if (player.isAlive && player.faction == playerCharacter?.faction)
                {
                    return false;
                }
            }
            return true;
        }

        private bool IsTargetDefeated(string characterId)
        {
            var target = FindPlayer(characterId);
            return target != null && !target.isAlive;
        }

        private Player FindPlayer(string characterId)
        {
            if (BattleManager.Instance == null) return null;

            foreach (var player in BattleManager.Instance.players)
            {
                if (player.generalName.Contains(characterId) ||
                    characterId.Contains(player.generalName))
                {
                    return player;
                }
            }
            return null;
        }

        #endregion

        #region 胜负处理

        private void OnVictory()
        {
            Debug.Log("[StoryBattle] 胜利！");
            isBattleActive = false;

            // 显示胜利对白
            if (currentBattle?.victoryDialogue != null && currentBattle.victoryDialogue.Count > 0)
            {
                StartCoroutine(ShowDialogueSequence(currentBattle.victoryDialogue, () =>
                {
                    // 标记战斗完成
                    MarkBattleCompleted();
                }));
            }
            else
            {
                MarkBattleCompleted();
            }
        }

        private void OnDefeat()
        {
            Debug.Log("[StoryBattle] 失败！");
            isBattleActive = false;

            // 显示失败对白
            if (currentBattle?.defeatDialogue != null && currentBattle.defeatDialogue.Count > 0)
            {
                StartCoroutine(ShowDialogueSequence(currentBattle.defeatDialogue, () =>
                {
                    // 返回故事模式界面
                    ReturnToStoryMode();
                }));
            }
            else
            {
                ReturnToStoryMode();
            }
        }

        private void MarkBattleCompleted()
        {
            if (currentBattle != null)
            {
                currentBattle.isCompleted = true;

                // 保存进度
                if (StoryModeManager.Instance != null)
                {
                    StoryModeManager.Instance.SaveProgress();
                }
            }

            // 返回故事模式界面
            ReturnToStoryMode();
        }

        private void ReturnToStoryMode()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("StoryMode");
        }

        #endregion

        #region 对话系统

        private Queue<Dialogue> dialogueQueue = new Queue<Dialogue>();
        private System.Action onDialogueComplete;
        private bool waitingForClick = false;  // ⭐ 等待点击
        private float clickCooldown = 0f;       // ⭐ 点击冷却时间
        private const float CLICK_COOLDOWN_TIME = 0.3f;  // ⭐ 冷却时间（秒）

        // ⭐ 角色画像UI
        private Image leftPortrait;
        private Image rightPortrait;
        private GameObject leftPortraitFrame;
        private GameObject rightPortraitFrame;
        private bool leftPortraitLoaded = false;   // ⭐ 左边是否已加载过图片
        private bool rightPortraitLoaded = false;  // ⭐ 右边是否已加载过图片

        /// <summary>
        /// ⭐ 显示对白序列 - 点击进入下一句
        /// </summary>
        public IEnumerator ShowDialogueSequence(List<Dialogue> dialogues, System.Action onComplete)
        {
            Debug.Log($"[StoryBattle] ShowDialogueSequence 开始，共 {dialogues?.Count ?? 0} 句对话");

            if (dialogues == null || dialogues.Count == 0)
            {
                Debug.LogWarning("[StoryBattle] 对话列表为空!");
                onComplete?.Invoke();
                yield break;
            }

            isDialogueShowing = true;
            onDialogueComplete = onComplete;

            // ⭐ 重置头像加载状态
            leftPortraitLoaded = false;
            rightPortraitLoaded = false;

            // 确保对话框UI存在
            if (dialoguePanel == null)
            {
                Debug.Log("[StoryBattle] dialoguePanel 为空，尝试创建/查找...");
                CreateDialogueUI();
            }

            if (dialoguePanel == null)
            {
                Debug.LogError("[StoryBattle] 创建对话UI失败! dialoguePanel 仍然为空");
                isDialogueShowing = false;
                onComplete?.Invoke();
                yield break;
            }

            Debug.Log($"[StoryBattle] 激活 dialoguePanel: {dialoguePanel.name}");
            dialoguePanel.SetActive(true);

            // ⭐ 检查父级Canvas是否激活
            Canvas parentCanvas = dialoguePanel.GetComponentInParent<Canvas>();
            if (parentCanvas != null)
            {
                Debug.Log($"[StoryBattle] 父级Canvas: {parentCanvas.name}, enabled={parentCanvas.enabled}, gameObject.active={parentCanvas.gameObject.activeInHierarchy}");
            }
            else
            {
                Debug.LogWarning("[StoryBattle] 对话面板没有找到父级Canvas!");
            }

            // ⭐ 检查关键UI组件
            Debug.Log($"[StoryBattle] UI组件状态 - dialogueText:{dialogueText != null}, speakerNameText:{speakerNameText != null}");

            // ⭐ 初始化头像为透明（等待首次加载）
            InitializePortraitsTransparent();

            // ⭐ 等待一帧确保UI完全初始化
            yield return null;
            yield return new WaitForSeconds(0.1f);

            Debug.Log("[StoryBattle] 开始显示对话内容...");

            foreach (var dialogue in dialogues)
            {
                ShowDialogue(dialogue);

                // ⭐ 等待玩家点击（带冷却时间）
                waitingForClick = true;
                clickCooldown = CLICK_COOLDOWN_TIME;  // 重置冷却
                yield return new WaitUntil(() => !waitingForClick);

                // ⭐ 等待一帧确保输入被清除
                yield return null;
            }

            Debug.Log("[StoryBattle] 对话序列完成，隐藏面板");
            dialoguePanel.SetActive(false);
            isDialogueShowing = false;
            onDialogueComplete?.Invoke();
        }

        private void ShowDialogue(Dialogue dialogue)
        {
            if (dialoguePanel == null)
            {
                Debug.LogError("[StoryBattleManager] dialoguePanel 为空!");
                return;
            }

            // 获取本地化文本
            string speaker = string.IsNullOrEmpty(dialogue.speakerKey)
                ? ""
                : (LocalizationManager.Instance?.GetText(dialogue.speakerKey) ?? dialogue.speakerKey);

            // ⭐ 如果本地化失败，尝试从映射表获取
            if (speaker == dialogue.speakerKey || string.IsNullOrEmpty(speaker))
            {
                speaker = GetCharacterName(dialogue.speakerKey);
            }

            string content = LocalizationManager.Instance?.GetText(dialogue.contentKey) ?? dialogue.contentKey;

            // ⭐ 清理全角标点（解决字体缺字问题）
            speaker = ThreeKingdoms.UI.TMPFontHelper.SanitizeFullWidthPunctuation(speaker);
            content = ThreeKingdoms.UI.TMPFontHelper.SanitizeFullWidthPunctuation(content);

            Debug.Log($"[对话显示] 说话人:{speaker}, 内容:{content.Substring(0, Mathf.Min(20, content.Length))}...");
            Debug.Log($"[对话显示] Panel active:{dialoguePanel.activeInHierarchy}, speakerText:{speakerNameText != null}, dialogueText:{dialogueText != null}");

            // 更新UI
            if (speakerNameText != null)
            {
                speakerNameText.text = speaker;
                speakerNameText.gameObject.SetActive(!string.IsNullOrEmpty(speaker));
                // ⭐ 设置字体
                ThreeKingdoms.UI.TMPFontHelper.SetFontByLanguage(speakerNameText);
            }
            else
            {
                Debug.LogWarning("[StoryBattleManager] speakerNameText 为空!");
            }

            if (dialogueText != null)
            {
                dialogueText.text = content;
                // ⭐ 设置字体
                ThreeKingdoms.UI.TMPFontHelper.SetFontByLanguage(dialogueText);
            }
            else
            {
                Debug.LogError("[StoryBattleManager] dialogueText 为空! 尝试重新查找...");
                // ⭐ 尝试重新查找 dialogueText
                Transform dialogueTextTrans = FindChildRecursive(dialoguePanel.transform, "DialogueText");
                if (dialogueTextTrans != null)
                {
                    dialogueText = dialogueTextTrans.GetComponent<TextMeshProUGUI>();
                    if (dialogueText != null)
                    {
                        dialogueText.text = content;
                        ThreeKingdoms.UI.TMPFontHelper.SetFontByLanguage(dialogueText);
                        Debug.Log("[StoryBattleManager] 重新找到 dialogueText");
                    }
                }
            }

            // ⭐ 更新角色画像 - 谁说话谁亮
            UpdatePortraits(dialogue.speakerKey);

            Debug.Log($"[对话] {speaker}: {content}");
        }

        /// <summary>
        /// ⭐ 更新角色画像显示
        /// </summary>
        private void UpdatePortraits(string currentSpeaker)
        {
            if (currentBattle == null) return;

            // ⭐ 判断说话人是我方还是敌方
            bool isAlly = false;
            bool isEnemy = false;
            string speakerId = currentSpeaker?.Replace("char_", "").ToLower() ?? "";

            // 检查是否是我方角色
            if (currentBattle.allies != null)
            {
                foreach (var ally in currentBattle.allies)
                {
                    if (ally.characterId.ToLower().Contains(speakerId) || speakerId.Contains(ally.characterId.ToLower()))
                    {
                        isAlly = true;
                        break;
                    }
                }
            }

            // 检查是否是敌方角色
            if (!isAlly && currentBattle.enemies != null)
            {
                foreach (var enemy in currentBattle.enemies)
                {
                    if (enemy.characterId.ToLower().Contains(speakerId) || speakerId.Contains(enemy.characterId.ToLower()))
                    {
                        isEnemy = true;
                        break;
                    }
                }
            }

            // ⭐ 加载并显示说话人头像
            if (!string.IsNullOrEmpty(speakerId))
            {
                Sprite speakerSprite = LoadCharacterPortrait(speakerId);
                if (speakerSprite != null)
                {
                    if (isAlly && leftPortrait != null)
                    {
                        leftPortrait.sprite = speakerSprite;
                        leftPortraitLoaded = true;  // ⭐ 标记左边已加载
                    }
                    else if (isEnemy && rightPortrait != null)
                    {
                        rightPortrait.sprite = speakerSprite;
                        rightPortraitLoaded = true;  // ⭐ 标记右边已加载
                    }
                    else if (!isAlly && !isEnemy)
                    {
                        // ⭐ 非战斗参与者（如旁白中的其他角色）
                        // 根据角色ID推测阵营：蜀国角色显示左边，魏国显示右边
                        bool isShuCharacter = IsShuCharacter(speakerId);
                        if (isShuCharacter && leftPortrait != null)
                        {
                            leftPortrait.sprite = speakerSprite;
                            leftPortraitLoaded = true;
                            isAlly = true; // 用于后续亮度设置
                        }
                        else if (rightPortrait != null)
                        {
                            rightPortrait.sprite = speakerSprite;
                            rightPortraitLoaded = true;
                            isEnemy = true;
                        }
                    }
                }
            }

            // ⭐ 更新画像亮度（只有加载过的那一边才显示）
            if (leftPortraitFrame != null)
            {
                Image frameImg = leftPortraitFrame.GetComponent<Image>();
                if (leftPortraitLoaded)
                {
                    // 左边已加载 - 说话时亮，否则暗
                    if (frameImg != null)
                        frameImg.color = isAlly ? new Color(1f, 1f, 1f, 1f) : new Color(0.4f, 0.4f, 0.4f, 1f);
                    if (leftPortrait != null)
                        leftPortrait.color = isAlly ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
                }
                // 未加载过则保持透明（已在初始化时设置）
            }

            if (rightPortraitFrame != null)
            {
                Image frameImg = rightPortraitFrame.GetComponent<Image>();
                if (rightPortraitLoaded)
                {
                    // 右边已加载 - 说话时亮，否则暗
                    if (frameImg != null)
                        frameImg.color = isEnemy ? new Color(1f, 1f, 1f, 1f) : new Color(0.4f, 0.4f, 0.4f, 1f);
                    if (rightPortrait != null)
                        rightPortrait.color = isEnemy ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
                }
                // 未加载过则保持透明
            }

            // ⭐ 旁白时：已加载的两边都暗
            if (string.IsNullOrEmpty(currentSpeaker))
            {
                if (leftPortrait != null && leftPortraitLoaded)
                    leftPortrait.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                if (rightPortrait != null && rightPortraitLoaded)
                    rightPortrait.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            }
        }

        /// <summary>
        /// ⭐ 初始化头像为透明（等待首次说话）
        /// </summary>
        private void InitializePortraitsTransparent()
        {
            if (leftPortrait != null)
            {
                leftPortrait.color = new Color(1f, 1f, 1f, 0f); // 完全透明
            }
            if (rightPortrait != null)
            {
                rightPortrait.color = new Color(1f, 1f, 1f, 0f); // 完全透明
            }
            if (leftPortraitFrame != null)
            {
                var img = leftPortraitFrame.GetComponent<Image>();
                if (img != null) img.color = new Color(1f, 1f, 1f, 0f);
            }
            if (rightPortraitFrame != null)
            {
                var img = rightPortraitFrame.GetComponent<Image>();
                if (img != null) img.color = new Color(1f, 1f, 1f, 0f);
            }
        }

        /// <summary>
        /// ⭐ 判断角色是否属于蜀国（用于对话位置判断）
        /// </summary>
        private bool IsShuCharacter(string characterId)
        {
            if (string.IsNullOrEmpty(characterId)) return false;

            string id = characterId.ToLower().Replace("char_", "");

            // 蜀国主要角色列表
            string[] shuCharacters = {
                "liubei", "guanyu", "zhangfei", "zhugeliang", "zhaoyun",
                "huangzhong", "machao", "jiangwei", "mifang", "mifuren",
                "liushan", "guanping", "zhangbao", "guansuo"
            };

            foreach (var shuChar in shuCharacters)
            {
                if (id.Contains(shuChar) || shuChar.Contains(id))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// ⭐ 加载角色头像（使用共享的 CharacterPinyinHelper）
        /// </summary>
        private Sprite LoadCharacterPortrait(string characterId)
        {
            if (string.IsNullOrEmpty(characterId)) return null;

            // 清理ID
            string cleanId = characterId.Replace("char_", "").Replace("_story", "").ToLower();

            // ⭐ 尝试所有势力的路径
            Faction[] factions = { Faction.Shu, Faction.Wei, Faction.Wu, Faction.Qun };

            foreach (var faction in factions)
            {
                Sprite sprite = CharacterPinyinHelper.LoadCharacterSprite(cleanId, faction);
                if (sprite != null)
                {
                    Debug.Log($"[StoryBattleManager] 加载头像成功: {cleanId}");
                    return sprite;
                }
            }

            Debug.LogWarning($"[StoryBattleManager] 未找到头像: {characterId}");
            return null;
        }

        /// <summary>
        /// ⭐ 点击继续对话（按钮回调）
        /// </summary>
        private void OnContinueDialogue()
        {
            // ⭐ 检查冷却时间
            if (waitingForClick && clickCooldown <= 0)
            {
                waitingForClick = false;
            }
        }

        /// <summary>
        /// ⭐ Update中检测点击和出牌阶段
        /// </summary>
        private void Update()
        {
            // ⭐ 更新冷却时间
            if (clickCooldown > 0)
            {
                clickCooldown -= Time.deltaTime;
            }

            // 对话中点击任意位置继续（需要冷却结束后才能点击）
            if (isDialogueShowing && waitingForClick && clickCooldown <= 0)
            {
                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                {
                    waitingForClick = false;
                }
            }

            // ⭐ 检测出牌阶段，显示待处理的开场对话
            CheckAndShowOpeningDialogue();
        }

        /// <summary>
        /// ⭐ 检测出牌阶段并显示开场对话
        /// </summary>
        private void CheckAndShowOpeningDialogue()
        {
            // 如果已经显示过或没有待显示的对话，跳过
            if (openingDialogueShown || pendingOpeningDialogue == null || pendingOpeningDialogue.Count == 0)
                return;

            // 如果正在显示对话，跳过
            if (isDialogueShowing)
                return;

            // 检查是否进入出牌阶段
            if (BattleManager.Instance != null && BattleManager.Instance.currentPhase == TurnPhase.Play)
            {
                Debug.Log("[StoryBattle] 检测到出牌阶段，开始显示开场对话");
                openingDialogueShown = true;

                StartCoroutine(ShowDialogueSequence(pendingOpeningDialogue, () =>
                {
                    Debug.Log("[StoryBattle] 开场对话显示完成");
                    pendingOpeningDialogue = null;
                }));
            }
        }

        /// <summary>
        /// ⭐ 尝试查找场景中已有的对话UI
        /// </summary>
        private bool TryFindExistingDialogueUI()
        {
            // 查找DialoguePanel
            GameObject existingPanel = GameObject.Find("DialoguePanel");
            if (existingPanel == null)
            {
                // 也尝试在Canvas下查找
                Canvas canvas = FindObjectOfType<Canvas>();
                if (canvas != null)
                {
                    Transform found = canvas.transform.Find("DialoguePanel");
                    if (found != null)
                        existingPanel = found.gameObject;
                }
            }

            if (existingPanel == null)
                return false;

            dialoguePanel = existingPanel;

            // ⭐ 找到面板后先隐藏，等 ShowDialogueSequence 时再激活
            // 这样可以避免阻挡卡牌操作
            dialoguePanel.SetActive(false);
            Debug.Log("[StoryBattleManager] 找到已有的DialoguePanel (已隐藏)");

            // ⭐ 查找子元素并绑定引用
            // 说话人名称
            Transform speakerNameTrans = FindChildRecursive(dialoguePanel.transform, "SpeakerName");
            if (speakerNameTrans != null)
                speakerNameText = speakerNameTrans.GetComponent<TextMeshProUGUI>();

            // 对话文本
            Transform dialogueTextTrans = FindChildRecursive(dialoguePanel.transform, "DialogueText");
            if (dialogueTextTrans != null)
                dialogueText = dialogueTextTrans.GetComponent<TextMeshProUGUI>();

            // 左侧画像
            Transform leftTrans = FindChildRecursive(dialoguePanel.transform, "LeftPortrait");
            if (leftTrans == null)
                leftTrans = FindChildRecursive(dialoguePanel.transform, "LeftPortraitFrame");
            if (leftTrans != null)
            {
                leftPortraitFrame = leftTrans.gameObject;
                Transform portraitTrans = leftTrans.Find("Portrait");
                if (portraitTrans != null)
                    leftPortrait = portraitTrans.GetComponent<Image>();
                else
                    leftPortrait = leftTrans.GetComponent<Image>();
            }

            // 右侧画像
            Transform rightTrans = FindChildRecursive(dialoguePanel.transform, "RightPortrait");
            if (rightTrans == null)
                rightTrans = FindChildRecursive(dialoguePanel.transform, "RightPortraitFrame");
            if (rightTrans != null)
            {
                rightPortraitFrame = rightTrans.gameObject;
                Transform portraitTrans = rightTrans.Find("Portrait");
                if (portraitTrans != null)
                    rightPortrait = portraitTrans.GetComponent<Image>();
                else
                    rightPortrait = rightTrans.GetComponent<Image>();
            }

            // ⭐ 添加点击事件（如果没有Button组件则添加）
            Button panelButton = dialoguePanel.GetComponent<Button>();
            if (panelButton == null)
            {
                panelButton = dialoguePanel.AddComponent<Button>();
            }
            panelButton.onClick.RemoveAllListeners();
            panelButton.onClick.AddListener(OnContinueDialogue);

            // 确保有Image组件（Button需要）
            Image panelImage = dialoguePanel.GetComponent<Image>();
            if (panelImage == null)
            {
                panelImage = dialoguePanel.AddComponent<Image>();
                panelImage.color = new Color(0, 0, 0, 0.01f);
            }

            Debug.Log($"[StoryBattleManager] 绑定完成 - SpeakerName:{speakerNameText != null}, DialogueText:{dialogueText != null}, LeftPortrait:{leftPortrait != null}, RightPortrait:{rightPortrait != null}");

            return true;
        }

        /// <summary>
        /// ⭐ 递归查找子物体
        /// </summary>
        private Transform FindChildRecursive(Transform parent, string name)
        {
            // 直接子物体
            Transform child = parent.Find(name);
            if (child != null)
                return child;

            // 递归查找
            foreach (Transform t in parent)
            {
                child = FindChildRecursive(t, name);
                if (child != null)
                    return child;
            }

            return null;
        }

        /// <summary>
        /// ⭐ 动态创建对话UI - 带角色画像（备用）
        /// </summary>
        private void CreateDialogueUI()
        {
            // ⭐ 优先查找场景中已有的对话UI
            if (TryFindExistingDialogueUI())
            {
                Debug.Log("[StoryBattleManager] 使用场景中已有的对话UI");
                return;
            }

            Debug.Log("[StoryBattleManager] 场景中未找到对话UI，动态创建");

            // 查找Canvas
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("DialogueCanvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            // ⭐ 创建主对话面板（全屏点击区域）
            dialoguePanel = new GameObject("DialoguePanel");
            dialoguePanel.transform.SetParent(canvas.transform, false);

            RectTransform panelRect = dialoguePanel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // ⭐ 添加按钮组件使整个面板可点击
            Button panelButton = dialoguePanel.AddComponent<Button>();
            panelButton.onClick.AddListener(OnContinueDialogue);

            // 透明背景
            Image panelBg = dialoguePanel.AddComponent<Image>();
            panelBg.color = new Color(0, 0, 0, 0.01f); // 几乎透明但可点击

            // ⭐ 创建左侧角色画像框（我方）
            leftPortraitFrame = CreatePortraitFrame(dialoguePanel.transform, true);
            leftPortrait = leftPortraitFrame.transform.Find("Portrait")?.GetComponent<Image>();

            // ⭐ 创建右侧角色画像框（敌方）
            rightPortraitFrame = CreatePortraitFrame(dialoguePanel.transform, false);
            rightPortrait = rightPortraitFrame.transform.Find("Portrait")?.GetComponent<Image>();

            // ⭐ 创建文本框
            GameObject textBox = new GameObject("TextBox");
            textBox.transform.SetParent(dialoguePanel.transform, false);

            RectTransform textBoxRect = textBox.AddComponent<RectTransform>();
            textBoxRect.anchorMin = new Vector2(0.15f, 0);
            textBoxRect.anchorMax = new Vector2(0.85f, 0.28f);
            textBoxRect.offsetMin = new Vector2(0, 20);
            textBoxRect.offsetMax = new Vector2(0, 0);

            Image textBoxBg = textBox.AddComponent<Image>();
            textBoxBg.color = new Color(0, 0, 0, 0.85f);

            // ⭐ 创建说话人名称
            GameObject nameObj = new GameObject("SpeakerName");
            nameObj.transform.SetParent(textBox.transform, false);
            speakerNameText = nameObj.AddComponent<TextMeshProUGUI>();
            speakerNameText.fontSize = 26;
            speakerNameText.fontStyle = FontStyles.Bold;
            speakerNameText.color = new Color(1f, 0.85f, 0.4f);
            speakerNameText.alignment = TextAlignmentOptions.Left;

            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0.75f);
            nameRect.anchorMax = new Vector2(0.4f, 1);
            nameRect.offsetMin = new Vector2(20, 5);
            nameRect.offsetMax = new Vector2(-10, -5);

            // ⭐ 创建对话文本
            GameObject textObj = new GameObject("DialogueText");
            textObj.transform.SetParent(textBox.transform, false);
            dialogueText = textObj.AddComponent<TextMeshProUGUI>();
            dialogueText.fontSize = 22;
            dialogueText.color = Color.white;
            dialogueText.enableWordWrapping = true;
            dialogueText.alignment = TextAlignmentOptions.TopLeft;

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0);
            textRect.anchorMax = new Vector2(1, 0.75f);
            textRect.offsetMin = new Vector2(20, 15);
            textRect.offsetMax = new Vector2(-20, -5);

            // ⭐ 创建点击提示
            GameObject hintObj = new GameObject("ClickHint");
            hintObj.transform.SetParent(textBox.transform, false);
            TextMeshProUGUI hintText = hintObj.AddComponent<TextMeshProUGUI>();
            hintText.text = "▼ 点击继续";
            hintText.fontSize = 16;
            hintText.color = new Color(0.7f, 0.7f, 0.7f, 0.8f);
            hintText.alignment = TextAlignmentOptions.Right;

            RectTransform hintRect = hintObj.GetComponent<RectTransform>();
            hintRect.anchorMin = new Vector2(0.7f, 0);
            hintRect.anchorMax = new Vector2(1, 0.2f);
            hintRect.offsetMin = new Vector2(0, 5);
            hintRect.offsetMax = new Vector2(-15, 0);

            // ⭐ 设置中文字体
            SetChineseFont(speakerNameText);
            SetChineseFont(dialogueText);
            SetChineseFont(hintText);

            dialoguePanel.SetActive(false);
        }

        /// <summary>
        /// ⭐ 创建角色画像框
        /// </summary>
        private GameObject CreatePortraitFrame(Transform parent, bool isLeft)
        {
            GameObject frame = new GameObject(isLeft ? "LeftPortraitFrame" : "RightPortraitFrame");
            frame.transform.SetParent(parent, false);

            RectTransform frameRect = frame.AddComponent<RectTransform>();
            if (isLeft)
            {
                frameRect.anchorMin = new Vector2(0, 0);
                frameRect.anchorMax = new Vector2(0.15f, 0.5f);
                frameRect.offsetMin = new Vector2(20, 30);
                frameRect.offsetMax = new Vector2(0, -10);
            }
            else
            {
                frameRect.anchorMin = new Vector2(0.85f, 0);
                frameRect.anchorMax = new Vector2(1, 0.5f);
                frameRect.offsetMin = new Vector2(0, 30);
                frameRect.offsetMax = new Vector2(-20, -10);
            }

            Image frameBg = frame.AddComponent<Image>();
            frameBg.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

            // 创建角色画像
            GameObject portraitObj = new GameObject("Portrait");
            portraitObj.transform.SetParent(frame.transform, false);

            RectTransform portraitRect = portraitObj.AddComponent<RectTransform>();
            portraitRect.anchorMin = new Vector2(0.05f, 0.05f);
            portraitRect.anchorMax = new Vector2(0.95f, 0.95f);
            portraitRect.offsetMin = Vector2.zero;
            portraitRect.offsetMax = Vector2.zero;

            Image portrait = portraitObj.AddComponent<Image>();
            portrait.color = new Color(0.5f, 0.5f, 0.5f, 1f); // 默认暗

            // ⭐ 尝试加载默认头像
            Sprite defaultSprite = Resources.Load<Sprite>("Sprites/Characters/default");
            if (defaultSprite != null)
            {
                portrait.sprite = defaultSprite;
            }

            // 创建标签（我方/敌方）
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(frame.transform, false);

            RectTransform labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0.85f);
            labelRect.anchorMax = new Vector2(1, 1);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            Image labelBg = labelObj.AddComponent<Image>();
            labelBg.color = isLeft ? new Color(0.2f, 0.5f, 0.8f, 0.9f) : new Color(0.8f, 0.2f, 0.2f, 0.9f);

            GameObject labelTextObj = new GameObject("LabelText");
            labelTextObj.transform.SetParent(labelObj.transform, false);

            RectTransform labelTextRect = labelTextObj.AddComponent<RectTransform>();
            labelTextRect.anchorMin = Vector2.zero;
            labelTextRect.anchorMax = Vector2.one;
            labelTextRect.offsetMin = Vector2.zero;
            labelTextRect.offsetMax = Vector2.zero;

            TextMeshProUGUI labelText = labelTextObj.AddComponent<TextMeshProUGUI>();
            labelText.text = isLeft ? "我方" : "敌方";
            labelText.fontSize = 14;
            labelText.color = Color.white;
            labelText.alignment = TextAlignmentOptions.Center;
            SetChineseFont(labelText);

            return frame;
        }

        #endregion

        #region 胜利条件显示

        /// <summary>
        /// ⭐ 显示胜利条件UI
        /// </summary>
        private void ShowVictoryConditionUI()
        {
            if (currentBattle == null) return;

            // 确保UI存在
            if (victoryConditionPanel == null)
            {
                CreateVictoryConditionUI();
            }

            victoryConditionPanel.SetActive(true);

            // 显示战斗名称
            if (battleNameText != null)
            {
                string battleName = LocalizationManager.Instance?.GetText(currentBattle.nameKey) ?? currentBattle.nameKey;
                battleNameText.text = battleName;
            }

            // 显示胜利条件
            if (victoryConditionText != null)
            {
                victoryConditionText.text = "胜利: " + GetVictoryConditionDescription();
            }

            // 显示失败条件
            if (defeatConditionText != null)
            {
                defeatConditionText.text = "失败: " + GetDefeatConditionDescription();
            }
        }

        /// <summary>
        /// ⭐ 获取胜利条件描述
        /// </summary>
        private string GetVictoryConditionDescription()
        {
            if (currentBattle?.victoryCondition == null)
                return "击败所有敌人";

            var condition = currentBattle.victoryCondition;

            switch (condition.type)
            {
                case VictoryType.DefeatAllEnemies:
                    return "击败所有敌人";

                case VictoryType.DefeatTarget:
                    string targetName = GetCharacterName(condition.targetCharacterId);
                    return $"击败 {targetName}";

                case VictoryType.SurviveTurns:
                    return $"存活 {condition.targetTurn} 回合";

                case VictoryType.AccumulateMarks:
                    return $"累积 {condition.targetCount} 个标记";

                case VictoryType.ProtectAlly:
                    string allyName = GetCharacterName(condition.targetCharacterId);
                    return $"保护 {allyName} 存活并击败所有敌人";

                case VictoryType.Custom:
                    return LocalizationManager.Instance?.GetText(condition.customConditionKey) ?? condition.customConditionKey;

                default:
                    return "击败所有敌人";
            }
        }

        /// <summary>
        /// ⭐ 获取失败条件描述
        /// </summary>
        private string GetDefeatConditionDescription()
        {
            if (currentBattle?.defeatCondition == null)
                return "主角死亡";

            var condition = currentBattle.defeatCondition;

            switch (condition.type)
            {
                case DefeatType.PlayerDeath:
                    return "主角死亡";

                case DefeatType.AllyDeath:
                    string allyName = GetCharacterName(condition.targetCharacterId);
                    return $"{allyName} 死亡";

                case DefeatType.AllAlliesDeath:
                    return "我方全灭";

                case DefeatType.ExceedCount:
                    return $"特定事件发生 {condition.maxCount} 次";

                case DefeatType.TurnLimitExceeded:
                    return $"超过 {currentBattle.turnLimit} 回合";

                case DefeatType.Custom:
                    return LocalizationManager.Instance?.GetText(condition.customConditionKey) ?? condition.customConditionKey;

                default:
                    return "主角死亡";
            }
        }

        /// <summary>
        /// ⭐ 获取角色名称
        /// </summary>
        private string GetCharacterName(string characterId)
        {
            if (string.IsNullOrEmpty(characterId)) return "未知";

            // 尝试本地化
            string localizedName = LocalizationManager.Instance?.GetText($"char_{characterId}");
            if (!string.IsNullOrEmpty(localizedName) && localizedName != $"char_{characterId}")
            {
                return localizedName;
            }

            // 映射表
            Dictionary<string, string> nameMap = new Dictionary<string, string>
            {
                {"caocao", "曹操"}, {"liubei", "刘备"}, {"sunquan", "孙权"},
                {"guanyu", "关羽"}, {"zhangfei", "张飞"}, {"zhugeliang", "诸葛亮"},
                {"zhouyu", "周瑜"}, {"lvmeng", "吕蒙"}, {"huanggai", "黄盖"},
                {"zhaoyun", "赵云"}, {"xiahoudun", "夏侯惇"}, {"xiahouyuan", "夏侯渊"},
                {"zhangliao", "张辽"}, {"zhangzhao", "张昭"}, {"xiahoujie", "夏侯杰"},
                {"jianggan", "蒋干"}, {"lusu", "鲁肃"}, {"chengpu", "程普"}
            };

            string key = characterId.ToLower().Replace("_story", "");
            if (nameMap.TryGetValue(key, out string name))
            {
                return name;
            }

            return characterId;
        }

        /// <summary>
        /// ⭐ 创建胜利条件UI
        /// </summary>
        private void CreateVictoryConditionUI()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;

            // 创建面板
            victoryConditionPanel = new GameObject("VictoryConditionPanel");
            victoryConditionPanel.transform.SetParent(canvas.transform, false);

            RectTransform panelRect = victoryConditionPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 1);
            panelRect.anchorMax = new Vector2(0, 1);
            panelRect.pivot = new Vector2(0, 1);
            panelRect.anchoredPosition = new Vector2(20, -20);
            panelRect.sizeDelta = new Vector2(350, 120);

            Image panelBg = victoryConditionPanel.AddComponent<Image>();
            panelBg.color = new Color(0, 0, 0, 0.7f);

            // 添加垂直布局
            var layout = victoryConditionPanel.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(15, 15, 10, 10);
            layout.spacing = 5;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = true;
            layout.childControlHeight = true;

            // 战斗名称
            GameObject nameObj = new GameObject("BattleName");
            nameObj.transform.SetParent(victoryConditionPanel.transform, false);
            battleNameText = nameObj.AddComponent<TextMeshProUGUI>();
            battleNameText.fontSize = 22;
            battleNameText.fontStyle = FontStyles.Bold;
            battleNameText.color = new Color(1f, 0.85f, 0.4f);
            battleNameText.alignment = TextAlignmentOptions.Left;
            var nameLayout = nameObj.AddComponent<LayoutElement>();
            nameLayout.preferredHeight = 30;

            // 胜利条件
            GameObject victoryObj = new GameObject("VictoryCondition");
            victoryObj.transform.SetParent(victoryConditionPanel.transform, false);
            victoryConditionText = victoryObj.AddComponent<TextMeshProUGUI>();
            victoryConditionText.fontSize = 18;
            victoryConditionText.color = new Color(0.4f, 1f, 0.4f); // 绿色
            victoryConditionText.alignment = TextAlignmentOptions.Left;
            var victoryLayout = victoryObj.AddComponent<LayoutElement>();
            victoryLayout.preferredHeight = 25;

            // 失败条件
            GameObject defeatObj = new GameObject("DefeatCondition");
            defeatObj.transform.SetParent(victoryConditionPanel.transform, false);
            defeatConditionText = defeatObj.AddComponent<TextMeshProUGUI>();
            defeatConditionText.fontSize = 18;
            defeatConditionText.color = new Color(1f, 0.4f, 0.4f); // 红色
            defeatConditionText.alignment = TextAlignmentOptions.Left;
            var defeatLayout = defeatObj.AddComponent<LayoutElement>();
            defeatLayout.preferredHeight = 25;

            // 设置字体
            SetChineseFont(battleNameText);
            SetChineseFont(victoryConditionText);
            SetChineseFont(defeatConditionText);
        }

        /// <summary>
        /// 设置中文字体
        /// </summary>
        private void SetChineseFont(TextMeshProUGUI text)
        {
            if (text == null) return;
            var font = ThreeKingdoms.UI.TMPFontHelper.GetUniversalFont();
            if (font != null)
            {
                text.font = font;
            }
        }

        #endregion

        #region 标记系统

        /// <summary>
        /// 获取标记数量
        /// </summary>
        public int GetMarkerCount(string markerType)
        {
            return markers.TryGetValue(markerType, out int count) ? count : 0;
        }

        /// <summary>
        /// 添加标记
        /// </summary>
        public void AddMarker(string markerType, int amount = 1)
        {
            if (!markers.ContainsKey(markerType))
                markers[markerType] = 0;
            markers[markerType] += amount;
        }

        /// <summary>
        /// 获取火焰伤害加成
        /// </summary>
        public int GetFireDamageBonus()
        {
            return markers.TryGetValue("fire_damage_bonus", out int bonus) ? bonus : 0;
        }

        #endregion
    }
}
