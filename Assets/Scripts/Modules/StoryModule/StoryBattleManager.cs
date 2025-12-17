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

        // ⭐ 规则运行时状态
        private Dictionary<string, int> damageReductionCharges = new Dictionary<string, int>(); // 伤害减免次数
        private Dictionary<string, bool> firstDamagePrevented = new Dictionary<string, bool>(); // 首次伤害是否已防止
        private Dictionary<string, bool> firstDamageDealt = new Dictionary<string, bool>(); // 是否已造成首次伤害
        private Dictionary<string, bool> dealtDamageThisTurn = new Dictionary<string, bool>(); // 本回合是否造成伤害
        private HashSet<string> noAttackTargets = new HashSet<string>(); // 禁止攻击的目标ID
        private bool noAllyPeachRule = false; // 禁止对盟友用桃
        private int extraSlashCount = 0; // 额外出杀次数
        private int extraSlashRound = 0; // 额外出杀生效回合

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

            // ⭐ 重置规则运行时状态
            damageReductionCharges.Clear();
            firstDamagePrevented.Clear();
            firstDamageDealt.Clear();
            dealtDamageThisTurn.Clear();
            noAttackTargets.Clear();
            noAllyPeachRule = false;
            extraSlashCount = 0;
            extraSlashRound = 0;

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
            Debug.Log($"[StoryBattle] 应用规则: {rule.nameKey} (类型:{rule.type})");

            switch (rule.type)
            {
                case RuleType.ModifyInitialCards:
                    // 修改初始手牌数
                    ApplyModifyInitialCards(rule);
                    break;

                case RuleType.ModifyMaxHP:
                    // 修改血量上限
                    ApplyModifyMaxHP(rule);
                    break;

                case RuleType.ModifyAttackRange:
                    // 修改攻击距离
                    ApplyModifyAttackRange(rule);
                    break;

                case RuleType.FireDamageBonus:
                    // 火焰伤害加成
                    markers["fire_damage_bonus"] = rule.value;
                    break;

                case RuleType.ModifyDrawCards:
                    // 修改摸牌数量 - 存储到markers供摸牌时使用
                    if (rule.targetId == "enemies" || rule.targetId == "allies" || string.IsNullOrEmpty(rule.targetId))
                    {
                        markers[$"draw_modifier_{rule.targetId ?? "all"}"] = rule.value;
                    }
                    else
                    {
                        markers[$"draw_modifier_{rule.targetId}"] = rule.value;
                    }
                    break;

                case RuleType.ModifyHandLimit:
                    // 修改手牌上限
                    ApplyModifyHandLimit(rule);
                    break;

                case RuleType.DamageReduction:
                    // 伤害减免（官渡要塞：首次两次伤害各-1）
                    string drTarget = rule.targetId ?? "allies";
                    damageReductionCharges[drTarget] = rule.value > 0 ? rule.value : 2; // 默认2次
                    Debug.Log($"[规则] 伤害减免生效，目标:{drTarget}，次数:{damageReductionCharges[drTarget]}");
                    break;

                case RuleType.RandomDiscard:
                    // 随机弃牌 - 存储概率到markers（30%）
                    markers["random_discard_chance"] = rule.value > 0 ? rule.value : 30;
                    break;

                case RuleType.FirstDamagePrevention:
                    // 首次伤害防止（强势主公）
                    string fdpTarget = rule.targetId ?? "player";
                    firstDamagePrevented[fdpTarget] = false; // false表示还未触发防止
                    Debug.Log($"[规则] 首次伤害防止生效，目标:{fdpTarget}");
                    break;

                case RuleType.DrawOnDamage:
                    // 受伤后摸牌（死战不退）
                    markers["draw_on_damage"] = rule.value > 0 ? rule.value : 1;
                    break;

                case RuleType.DrawOnNoDamage:
                    // 未造成伤害时摸牌（以逸待劳）
                    markers["draw_on_no_damage"] = rule.value > 0 ? rule.value : 1;
                    break;

                case RuleType.DrawOnFirstDamage:
                    // 首次造成伤害后摸牌（江东猛虎）
                    markers["draw_on_first_damage"] = rule.value > 0 ? rule.value : 1;
                    break;

                case RuleType.DamageToWounded:
                    // 对已受伤目标伤害加成（西凉精骑）
                    markers["damage_to_wounded_bonus"] = rule.value > 0 ? rule.value : 1;
                    break;

                case RuleType.DamageOverTime:
                    // 持续伤害 - 记录开始回合
                    markers["damage_over_time_start"] = rule.triggerTurn > 0 ? rule.triggerTurn : 2;
                    markers["damage_over_time_value"] = rule.value > 0 ? rule.value : 1;
                    markers["damage_over_time_target"] = rule.targetId == "all" ? 0 : (rule.targetId == "enemies" ? 1 : 2);
                    Debug.Log($"[规则] 持续伤害将在第{markers["damage_over_time_start"]}回合开始生效");
                    break;

                case RuleType.LowHandDamageBonus:
                    // 低手牌时伤害加成（背水一战）
                    markers["low_hand_damage_bonus"] = rule.value > 0 ? rule.value : 1;
                    markers["low_hand_threshold"] = 2; // 手牌<=2时生效
                    break;

                case RuleType.FirstDamageBonus:
                    // 首次伤害加成（降将助战）
                    string fdbTarget = rule.targetId ?? "player";
                    firstDamageDealt[fdbTarget] = false;
                    markers[$"first_damage_bonus_{fdbTarget}"] = rule.value > 0 ? rule.value : 1;
                    break;

                case RuleType.ForcedDiscard:
                    // 强制弃牌（袁营猜忌）
                    markers["forced_discard"] = rule.value > 0 ? rule.value : 1;
                    markers["forced_discard_target"] = rule.targetId != null ? 1 : 0;
                    break;

                case RuleType.BonusDraw:
                    // 额外摸牌（赤足相迎）
                    markers["bonus_draw"] = rule.value > 0 ? rule.value : 1;
                    markers["bonus_draw_trigger"] = 1; // 首次触发
                    break;

                case RuleType.ExtraSlash:
                    // 额外出杀次数（先声夺人）
                    extraSlashCount = rule.value > 0 ? rule.value : 1;
                    extraSlashRound = rule.triggerTurn > 0 ? rule.triggerTurn : 1;
                    Debug.Log($"[规则] 第{extraSlashRound}回合可额外出{extraSlashCount}张杀");
                    break;

                case RuleType.NoAttackTarget:
                    // 禁止攻击目标（名义联盟）
                    if (!string.IsNullOrEmpty(rule.targetId))
                    {
                        noAttackTargets.Add(rule.targetId.ToLower());
                    }
                    // 检查是否有多个目标（通过敌人列表）
                    if (currentBattle?.enemies != null)
                    {
                        foreach (var enemy in currentBattle.enemies)
                        {
                            // 标记为"名义盟友"的敌人（袁绍、袁术）
                            if (enemy.characterId.ToLower().Contains("yuanshao") ||
                                enemy.characterId.ToLower().Contains("yuanshu"))
                            {
                                noAttackTargets.Add(enemy.characterId.ToLower());
                            }
                        }
                    }
                    Debug.Log($"[规则] 禁止攻击目标: {string.Join(", ", noAttackTargets)}");
                    break;

                case RuleType.NoAllyPeach:
                    // 禁止对盟友用桃（各自为战）
                    noAllyPeachRule = true;
                    Debug.Log("[规则] 禁止对盟友使用桃");
                    break;

                default:
                    Debug.Log($"[StoryBattle] 未处理的规则类型: {rule.type}");
                    break;
            }
        }

        #region 规则应用辅助方法

        private void ApplyModifyInitialCards(SpecialRule rule)
        {
            if (string.IsNullOrEmpty(rule.targetId)) return;

            List<Player> targets = GetTargetPlayers(rule.targetId);
            foreach (var player in targets)
            {
                if (rule.value > 0)
                {
                    // 增加手牌
                    for (int i = 0; i < rule.value; i++)
                    {
                        var card = DeckManager.Instance?.DrawCard();
                        if (card != null) player.DrawCard(card);
                    }
                }
                else
                {
                    // 减少手牌
                    for (int i = 0; i < -rule.value && player.handCards.Count > 0; i++)
                    {
                        player.DiscardCard(player.handCards[0]);
                    }
                }
            }
        }

        private void ApplyModifyMaxHP(SpecialRule rule)
        {
            List<Player> targets = GetTargetPlayers(rule.targetId);
            foreach (var player in targets)
            {
                player.maxHP += rule.value;
                if (rule.value > 0) player.currentHP += rule.value;
            }
        }

        private void ApplyModifyAttackRange(SpecialRule rule)
        {
            List<Player> targets = GetTargetPlayers(rule.targetId);
            foreach (var player in targets)
            {
                player.attackRange += rule.value;
            }
        }

        private void ApplyModifyHandLimit(SpecialRule rule)
        {
            List<Player> targets = GetTargetPlayers(rule.targetId);
            foreach (var player in targets)
            {
                player.handCardLimit += rule.value;
            }
        }

        /// <summary>
        /// 根据targetId获取目标玩家列表
        /// </summary>
        private List<Player> GetTargetPlayers(string targetId)
        {
            List<Player> targets = new List<Player>();
            if (string.IsNullOrEmpty(targetId)) return targets;

            if (targetId.ToLower() == "allies")
            {
                targets.AddRange(GetAllyPlayers());
            }
            else if (targetId.ToLower() == "enemies")
            {
                targets.AddRange(GetEnemyPlayers());
            }
            else if (targetId.ToLower() == "all")
            {
                if (BattleManager.Instance != null)
                    targets.AddRange(BattleManager.Instance.players);
            }
            else
            {
                var player = FindPlayer(targetId);
                if (player != null) targets.Add(player);
            }
            return targets;
        }

        #endregion

        #region 公共规则查询API

        /// <summary>
        /// ⭐ 检查目标是否可以被攻击（供AI和玩家使用）
        /// </summary>
        public bool IsTargetAttackable(Player attacker, Player target)
        {
            if (attacker == null || target == null) return false;
            if (attacker == target) return false;
            if (!target.isAlive) return false;

            // 检查是否是盟友（故事模式中盟友不能互相攻击）
            if (IsAlly(attacker, target))
            {
                Debug.Log($"[规则检查] {attacker.generalName} 不能攻击盟友 {target.generalName}");
                return false;
            }

            // 检查禁止攻击目标规则
            string targetId = target.generalName?.ToLower().Replace("_story", "") ?? "";
            foreach (var noAttackId in noAttackTargets)
            {
                if (targetId.Contains(noAttackId) || noAttackId.Contains(targetId))
                {
                    Debug.Log($"[规则检查] {target.generalName} 被规则保护，不能被攻击");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// ⭐ 检查两个角色是否是盟友
        /// </summary>
        public bool IsAlly(Player p1, Player p2)
        {
            if (p1 == null || p2 == null) return false;
            if (currentBattle == null) return p1.faction == p2.faction;

            bool p1IsAlly = false;
            bool p2IsAlly = false;

            // 检查是否都在我方列表中
            if (currentBattle.allies != null)
            {
                foreach (var ally in currentBattle.allies)
                {
                    string allyId = ally.characterId.ToLower().Replace("_story", "");
                    string p1Id = p1.generalName?.ToLower().Replace("_story", "") ?? "";
                    string p2Id = p2.generalName?.ToLower().Replace("_story", "") ?? "";

                    if (allyId.Contains(p1Id) || p1Id.Contains(allyId)) p1IsAlly = true;
                    if (allyId.Contains(p2Id) || p2Id.Contains(allyId)) p2IsAlly = true;
                }
            }

            return p1IsAlly && p2IsAlly;
        }

        /// <summary>
        /// ⭐ 检查两个角色是否是敌人
        /// </summary>
        public bool IsEnemy(Player p1, Player p2)
        {
            if (p1 == null || p2 == null) return false;
            if (p1 == p2) return false;
            return !IsAlly(p1, p2);
        }

        /// <summary>
        /// ⭐ 检查是否可以对目标使用桃
        /// </summary>
        public bool CanUsePeachOn(Player source, Player target)
        {
            if (source == null || target == null) return false;
            if (source == target) return true; // 对自己总是可以

            // 检查禁止对盟友用桃规则
            if (noAllyPeachRule && IsAlly(source, target))
            {
                Debug.Log($"[规则检查] 规则禁止 {source.generalName} 对盟友 {target.generalName} 使用桃");
                return false;
            }

            return true;
        }

        /// <summary>
        /// ⭐ 获取我方玩家列表
        /// </summary>
        public List<Player> GetAllyPlayers()
        {
            List<Player> allies = new List<Player>();
            if (BattleManager.Instance == null) return allies;

            if (currentBattle?.allies != null)
            {
                foreach (var allyConfig in currentBattle.allies)
                {
                    Player player = FindPlayer(allyConfig.characterId);
                    if (player != null && player.isAlive)
                    {
                        allies.Add(player);
                    }
                }
            }

            // 如果没有定义，返回玩家阵营相同的
            if (allies.Count == 0 && playerCharacter != null)
            {
                foreach (var player in BattleManager.Instance.players)
                {
                    if (player.isAlive && player.faction == playerCharacter.faction)
                    {
                        allies.Add(player);
                    }
                }
            }

            return allies;
        }

        /// <summary>
        /// ⭐ 获取敌方玩家列表
        /// </summary>
        public List<Player> GetEnemyPlayers()
        {
            List<Player> enemies = new List<Player>();
            if (BattleManager.Instance == null) return enemies;

            if (currentBattle?.enemies != null)
            {
                foreach (var enemyConfig in currentBattle.enemies)
                {
                    Player player = FindPlayer(enemyConfig.characterId);
                    if (player != null && player.isAlive)
                    {
                        enemies.Add(player);
                    }
                }
            }

            // 如果没有定义，返回玩家阵营不同的
            if (enemies.Count == 0 && playerCharacter != null)
            {
                foreach (var player in BattleManager.Instance.players)
                {
                    if (player.isAlive && player.faction != playerCharacter.faction)
                    {
                        enemies.Add(player);
                    }
                }
            }

            return enemies;
        }

        /// <summary>
        /// ⭐ 获取可攻击的有效目标（综合考虑攻击范围和规则）
        /// </summary>
        public List<Player> GetValidAttackTargets(Player attacker)
        {
            List<Player> validTargets = new List<Player>();
            if (attacker == null || BattleManager.Instance == null) return validTargets;

            foreach (var player in BattleManager.Instance.players)
            {
                if (player.isAlive &&
                    attacker.IsInAttackRange(player) &&
                    IsTargetAttackable(attacker, player))
                {
                    validTargets.Add(player);
                }
            }

            return validTargets;
        }

        /// <summary>
        /// ⭐ 获取当前回合额外出杀次数
        /// </summary>
        public int GetExtraSlashCount()
        {
            if (currentRound == extraSlashRound)
            {
                return extraSlashCount;
            }
            return 0;
        }

        /// <summary>
        /// ⭐ 计算修正后的伤害值
        /// </summary>
        public int GetModifiedDamage(Player source, Player target, int baseDamage, bool isFireDamage = false)
        {
            int damage = baseDamage;

            // 火焰伤害加成
            if (isFireDamage && markers.TryGetValue("fire_damage_bonus", out int fireBonus))
            {
                damage += fireBonus;
                Debug.Log($"[伤害修正] 火焰加成 +{fireBonus}");
            }

            // 对已受伤目标伤害加成
            if (target.currentHP < target.maxHP && markers.TryGetValue("damage_to_wounded_bonus", out int woundedBonus))
            {
                damage += woundedBonus;
                Debug.Log($"[伤害修正] 对已受伤目标 +{woundedBonus}");
            }

            // 低手牌伤害加成
            if (source != null && markers.TryGetValue("low_hand_damage_bonus", out int lowHandBonus))
            {
                int threshold = markers.TryGetValue("low_hand_threshold", out int t) ? t : 2;
                if (source.handCards.Count <= threshold)
                {
                    damage += lowHandBonus;
                    Debug.Log($"[伤害修正] 低手牌加成 +{lowHandBonus}");
                }
            }

            // 首次伤害加成
            if (source != null)
            {
                string sourceId = source.generalName?.ToLower().Replace("_story", "") ?? "";
                foreach (var kvp in firstDamageDealt)
                {
                    if (!kvp.Value && (kvp.Key.Contains(sourceId) || sourceId.Contains(kvp.Key)))
                    {
                        if (markers.TryGetValue($"first_damage_bonus_{kvp.Key}", out int bonus))
                        {
                            damage += bonus;
                            Debug.Log($"[伤害修正] {source.generalName} 首次伤害加成 +{bonus}");
                        }
                    }
                }
            }

            return damage;
        }

        /// <summary>
        /// ⭐ 检查伤害是否应该被防止（首次伤害防止）
        /// </summary>
        public bool ShouldPreventDamage(Player target)
        {
            if (target == null) return false;

            string targetId = target.generalName?.ToLower().Replace("_story", "") ?? "";

            foreach (var kvp in firstDamagePrevented)
            {
                if (!kvp.Value && (kvp.Key.Contains(targetId) || targetId.Contains(kvp.Key) || kvp.Key == "player"))
                {
                    // 标记已触发
                    firstDamagePrevented[kvp.Key] = true;
                    Debug.Log($"[规则] {target.generalName} 的首次伤害被防止！");
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// ⭐ 检查是否有伤害减免
        /// </summary>
        public int GetDamageReduction(Player target)
        {
            if (target == null) return 0;

            // 检查是否是盟友
            bool isAlly = false;
            if (currentBattle?.allies != null)
            {
                string targetId = target.generalName?.ToLower().Replace("_story", "") ?? "";
                foreach (var ally in currentBattle.allies)
                {
                    string allyId = ally.characterId.ToLower().Replace("_story", "");
                    if (allyId.Contains(targetId) || targetId.Contains(allyId))
                    {
                        isAlly = true;
                        break;
                    }
                }
            }

            // 检查盟友伤害减免
            if (isAlly && damageReductionCharges.TryGetValue("allies", out int charges) && charges > 0)
            {
                damageReductionCharges["allies"] = charges - 1;
                Debug.Log($"[规则] 盟友伤害减免触发，剩余次数: {charges - 1}");
                return 1;
            }

            return 0;
        }

        /// <summary>
        /// ⭐ 标记角色本回合造成了伤害
        /// </summary>
        public void MarkDamageDealt(Player source)
        {
            if (source == null) return;

            string sourceId = source.generalName?.ToLower().Replace("_story", "") ?? "";
            dealtDamageThisTurn[sourceId] = true;

            // 检查首次伤害标记
            foreach (var key in new List<string>(firstDamageDealt.Keys))
            {
                if (!firstDamageDealt[key] && (key.Contains(sourceId) || sourceId.Contains(key)))
                {
                    firstDamageDealt[key] = true;
                    Debug.Log($"[规则] {source.generalName} 造成了首次伤害");
                }
            }
        }

        #endregion

        #endregion

        #region 事件处理

        private void OnTurnStart(Player player)
        {
            if (!isBattleActive) return;

            // ⭐ 重置本回合伤害追踪
            string playerId = player.generalName?.ToLower().Replace("_story", "") ?? "";
            dealtDamageThisTurn[playerId] = false;

            // ⭐ 处理随机弃牌规则（内部不和：30%概率弃牌）
            if (markers.TryGetValue("random_discard_chance", out int chance))
            {
                if (IsAllyPlayer(player) && Random.Range(0, 100) < chance && player.handCards.Count > 0)
                {
                    int randomIndex = Random.Range(0, player.handCards.Count);
                    Card discardedCard = player.handCards[randomIndex];
                    player.DiscardCard(discardedCard);
                    Debug.Log($"[规则] 内部不和触发，{player.generalName} 弃置了 {discardedCard.cardName}");
                    TriggerEvents(EventTrigger.OnCardPlayed, "discard");
                }
            }

            // ⭐ 应用额外出杀次数
            if (currentRound == extraSlashRound && IsAllyPlayer(player))
            {
                player.maxSlashPerTurn += extraSlashCount;
                Debug.Log($"[规则] {player.generalName} 本回合可额外出 {extraSlashCount} 张杀");
            }

            // 检查回合开始事件
            TriggerEvents(EventTrigger.OnTurnStart, player.generalName);

            // ⭐ 回合开始时检测（用于存活回合数条件）
            CheckVictoryCondition();
            CheckDefeatCondition();
        }

        /// <summary>
        /// 检查玩家是否是我方角色
        /// </summary>
        private bool IsAllyPlayer(Player player)
        {
            if (player == null || currentBattle?.allies == null) return false;

            string playerId = player.generalName?.ToLower().Replace("_story", "") ?? "";
            foreach (var ally in currentBattle.allies)
            {
                string allyId = ally.characterId.ToLower().Replace("_story", "");
                if (allyId.Contains(playerId) || playerId.Contains(allyId))
                {
                    return true;
                }
            }
            return false;
        }

        private void OnTurnEnd(Player player)
        {
            if (!isBattleActive) return;

            // ⭐ 处理以逸待劳规则（未造成伤害时摸牌）
            if (markers.TryGetValue("draw_on_no_damage", out int drawCount) && IsAllyPlayer(player))
            {
                string playerId = player.generalName?.ToLower().Replace("_story", "") ?? "";
                if (!dealtDamageThisTurn.TryGetValue(playerId, out bool dealt) || !dealt)
                {
                    for (int i = 0; i < drawCount; i++)
                    {
                        var card = DeckManager.Instance?.DrawCard();
                        if (card != null) player.DrawCard(card);
                    }
                    Debug.Log($"[规则] 以逸待劳触发，{player.generalName} 摸了 {drawCount} 张牌");
                }
            }

            // ⭐ 处理强制弃牌规则（袁营猜忌：袁绍每回合弃牌）
            if (markers.TryGetValue("forced_discard", out int discardCount))
            {
                // 检查是否是目标角色（通常是敌人）
                bool isTarget = !IsAllyPlayer(player);
                if (isTarget && player.handCards.Count > 0)
                {
                    for (int i = 0; i < discardCount && player.handCards.Count > 0; i++)
                    {
                        int randomIndex = Random.Range(0, player.handCards.Count);
                        Card discardedCard = player.handCards[randomIndex];
                        player.DiscardCard(discardedCard);
                        Debug.Log($"[规则] 强制弃牌触发，{player.generalName} 弃置了 {discardedCard.cardName}");
                    }
                }
            }

            // 触发回合结束事件
            TriggerEvents(EventTrigger.OnTurnEnd, player.generalName);

            // 如果是第一个玩家回合结束，增加回合数
            if (BattleManager.Instance != null && BattleManager.Instance.players.Count > 0)
            {
                if (player == BattleManager.Instance.players[0])
                {
                    currentRound++;
                    OnRoundStart();
                }
            }

            // ⭐ 回合结束时也检测
            CheckVictoryCondition();
            CheckDefeatCondition();
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

            // ⭐ 处理持续伤害规则（粮草焚毁/洛阳焦土）
            if (markers.TryGetValue("damage_over_time_start", out int startRound) && currentRound >= startRound)
            {
                int damageValue = markers.TryGetValue("damage_over_time_value", out int dv) ? dv : 1;
                int targetType = markers.TryGetValue("damage_over_time_target", out int tt) ? tt : 0;

                List<Player> targets = new List<Player>();

                switch (targetType)
                {
                    case 0: // all - 所有角色
                        if (BattleManager.Instance != null)
                            targets.AddRange(BattleManager.Instance.players.FindAll(p => p.isAlive));
                        break;
                    case 1: // enemies - 敌方角色
                        targets.AddRange(GetEnemyPlayers());
                        break;
                    case 2: // allies - 我方角色
                        targets.AddRange(GetAllyPlayers());
                        break;
                }

                foreach (var target in targets)
                {
                    if (target.isAlive)
                    {
                        target.TakeDamage(damageValue, null);
                        Debug.Log($"[规则] 持续伤害触发，{target.generalName} 失去 {damageValue} 点体力");
                    }
                }

                // 触发特殊事件
                if (currentRound == startRound)
                {
                    TriggerEvents(EventTrigger.OnRoundStart, currentRound.ToString());
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

            // ⭐ 标记伤害来源本回合造成了伤害
            if (source != null)
            {
                MarkDamageDealt(source);

                // ⭐ 检查首次造成伤害摸牌（江东猛虎）
                if (markers.TryGetValue("draw_on_first_damage", out int drawCount) && IsAllyPlayer(source))
                {
                    string sourceId = source.generalName?.ToLower().Replace("_story", "") ?? "";
                    string trackKey = $"first_damage_drawn_{sourceId}";

                    if (!markers.ContainsKey(trackKey))
                    {
                        markers[trackKey] = 1; // 标记已触发
                        for (int i = 0; i < drawCount; i++)
                        {
                            var drawCard = DeckManager.Instance?.DrawCard();
                            if (drawCard != null) source.DrawCard(drawCard);
                        }
                        Debug.Log($"[规则] 江东猛虎触发，{source.generalName} 摸了 {drawCount} 张牌");
                    }
                }
            }

            // ⭐ 处理受伤后摸牌规则（死战不退）
            if (markers.TryGetValue("draw_on_damage", out int drawOnDamage) && IsAllyPlayer(victim))
            {
                for (int i = 0; i < drawOnDamage; i++)
                {
                    var drawCard = DeckManager.Instance?.DrawCard();
                    if (drawCard != null) victim.DrawCard(drawCard);
                }
                Debug.Log($"[规则] 死战不退触发，{victim.generalName} 摸了 {drawOnDamage} 张牌");
            }

            TriggerEvents(EventTrigger.OnDamageTaken, victim.generalName);
            TriggerEvents(EventTrigger.OnDamageDealt, source?.generalName ?? "");

            // 检查血量低于阈值事件
            TriggerEvents(EventTrigger.OnHPBelow, victim.generalName, victim.currentHP.ToString());

            // 检查濒死
            if (victim.currentHP <= 0)
            {
                TriggerEvents(EventTrigger.OnNearDeath, victim.generalName);
            }

            // ⭐ 伤害后检测胜负条件（用于HP相关条件）
            CheckVictoryCondition();
            CheckDefeatCondition();
        }

        private void OnPlayerDeath(Player victim, Player killer)
        {
            if (!isBattleActive) return;

            Debug.Log($"[StoryBattle] 玩家死亡: {victim.generalName}, 击杀者: {killer?.generalName ?? "无"}");
            TriggerEvents(EventTrigger.OnDeath, victim.generalName);

            // ⭐ 延迟一帧检测，确保死亡状态已更新
            StartCoroutine(DelayedVictoryCheck());
        }

        /// <summary>
        /// ⭐ 延迟检测胜负条件（确保状态已更新）
        /// </summary>
        private IEnumerator DelayedVictoryCheck()
        {
            yield return null; // 等待一帧

            if (!isBattleActive) yield break;

            Debug.Log("[StoryBattle] 执行延迟胜负检测");
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

            // ⭐ 出牌后检测（某些条件可能与出牌相关）
            CheckVictoryCondition();
            CheckDefeatCondition();
        }

        private void OnSkillTriggered(Player player, string skillId)
        {
            if (!isBattleActive) return;

            TriggerEvents(EventTrigger.OnSkillActivate, skillId);

            // ⭐ 技能触发后检测
            CheckVictoryCondition();
            CheckDefeatCondition();
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
            }
            else if (eventType == "hand_viewed")
            {
                if (!eventCounts.ContainsKey("hand_viewed_huanggai"))
                    eventCounts["hand_viewed_huanggai"] = 0;
                eventCounts["hand_viewed_huanggai"]++;
                TriggerEvents(EventTrigger.OnHandCardViewed, param);
            }

            // ⭐ 所有故事事件后都检测胜负条件
            CheckVictoryCondition();
            CheckDefeatCondition();
        }

        /// <summary>
        /// 触发符合条件的事件
        /// </summary>
        private void TriggerEvents(EventTrigger trigger, string param, string param2 = "")
        {
            if (currentBattle?.events == null) return;

            foreach (var evt in currentBattle.events)
            {
                if (evt.trigger != trigger) continue;

                // ⭐ 使用更宽松的参数匹配
                bool paramMatch = string.IsNullOrEmpty(evt.triggerParam) ||
                                  MatchEventParam(evt.triggerParam, param);

                if (!paramMatch) continue;

                // 检查param2（如果有）
                if (!string.IsNullOrEmpty(evt.triggerParam2) && evt.triggerParam2 != param2)
                    continue;

                // 检查是否已触发（非重复事件）
                if (!evt.repeatable && evt.triggered)
                    continue;

                evt.triggered = true;
                Debug.Log($"[StoryBattle] 触发事件: {trigger} param={param} (匹配:{evt.triggerParam})");

                // 显示对白
                if (evt.dialogues != null && evt.dialogues.Count > 0)
                {
                    StartCoroutine(ShowDialogueSequence(evt.dialogues, null));
                }
            }
        }

        /// <summary>
        /// ⭐ 匹配事件参数（支持中文名、英文ID、部分匹配）
        /// </summary>
        private bool MatchEventParam(string eventParam, string actualParam)
        {
            if (string.IsNullOrEmpty(eventParam) || string.IsNullOrEmpty(actualParam))
                return false;

            // 直接匹配
            if (eventParam == actualParam)
                return true;

            // 忽略大小写匹配
            if (eventParam.ToLower() == actualParam.ToLower())
                return true;

            // 包含匹配（处理"孙权"与"孙权_story"等情况）
            string cleanEvent = eventParam.Replace("_story", "").Replace("_", "");
            string cleanActual = actualParam.Replace("_story", "").Replace("_", "");

            if (cleanEvent == cleanActual)
                return true;

            if (actualParam.Contains(eventParam) || eventParam.Contains(actualParam))
                return true;

            // 尝试通过角色ID映射匹配
            string eventChinese = GetChineseNameFromParam(eventParam);
            string actualChinese = GetChineseNameFromParam(actualParam);

            if (!string.IsNullOrEmpty(eventChinese) && !string.IsNullOrEmpty(actualChinese))
            {
                return eventChinese == actualChinese;
            }

            return false;
        }

        /// <summary>
        /// ⭐ 将参数转换为中文名（用于匹配）
        /// </summary>
        private string GetChineseNameFromParam(string param)
        {
            if (string.IsNullOrEmpty(param)) return param;

            // 已经是中文，直接返回
            if (ContainsChinese(param)) return param;

            // 英文ID到中文名映射
            var nameMap = new Dictionary<string, string>
            {
                {"sunquan", "孙权"}, {"sunquan_story", "孙权"},
                {"lusu", "鲁肃"},
                {"chengpu", "程普"},
                {"zhangzhao", "张昭"},
                {"zhugeliang", "诸葛亮"}, {"zhugeliang_story", "诸葛亮"},
                {"zhouyu", "周瑜"}, {"zhouyu_story", "周瑜"},
                {"lvmeng", "吕蒙"},
                {"zhaoyun", "赵云"}, {"zhaoyun_story", "赵云"},
                {"zhangfei", "张飞"}, {"zhangfei_story", "张飞"},
                {"huanggai", "黄盖"}, {"huanggai_story", "黄盖"},
                {"caocao", "曹操"}, {"caocao_story", "曹操"},
                {"xiahoujie", "夏侯杰"},
                {"jianggan", "蒋干"},
                {"xiahoudun", "夏侯惇"},
                {"xiahouyuan", "夏侯渊"},
                {"zhangliao", "张辽"},
                {"liubei", "刘备"}, {"liubei_story", "刘备"},
                {"guanyu", "关羽"}, {"guanyu_story", "关羽"},
                {"caojun_cavalry", "曹军骑兵"},
            };

            string cleanParam = param.ToLower().Trim();
            if (nameMap.TryGetValue(cleanParam, out string chineseName))
            {
                return chineseName;
            }

            return param;
        }

        /// <summary>
        /// ⭐ 检查字符串是否包含中文
        /// </summary>
        private bool ContainsChinese(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            foreach (char c in text)
            {
                if (c >= 0x4e00 && c <= 0x9fff)
                    return true;
            }
            return false;
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
                    if (victory) Debug.Log("[StoryBattle] 胜利条件达成: 击败所有敌人");
                    break;

                case VictoryType.DefeatTarget:
                    victory = IsTargetDefeated(condition.targetCharacterId);
                    if (victory) Debug.Log($"[StoryBattle] 胜利条件达成: 击败目标 {condition.targetCharacterId}");
                    break;

                case VictoryType.SurviveTurns:
                    victory = currentRound >= condition.targetTurn;
                    if (victory) Debug.Log($"[StoryBattle] 胜利条件达成: 存活 {condition.targetTurn} 回合");
                    break;

                case VictoryType.AccumulateMarks:
                    if (markers.TryGetValue("zhaxiang", out int count))
                    {
                        victory = count >= condition.targetCount;
                        if (victory) Debug.Log($"[StoryBattle] 胜利条件达成: 累积 {count} 个标记");
                    }
                    break;

                case VictoryType.ProtectAlly:
                    // 保护目标存活且所有敌人被击败
                    var ally = FindPlayer(condition.targetCharacterId);
                    bool allyAlive = ally != null && ally.isAlive;
                    bool enemiesDefeated = AreAllEnemiesDefeated();
                    victory = allyAlive && enemiesDefeated;
                    if (victory) Debug.Log($"[StoryBattle] 胜利条件达成: 保护 {condition.targetCharacterId} 存活并击败所有敌人");
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
                    // ⭐ 使用我方列表中的第一个角色作为主角
                    if (playerCharacter == null && currentBattle?.allies != null && currentBattle.allies.Count > 0)
                    {
                        playerCharacter = FindPlayer(currentBattle.allies[0].characterId);
                    }
                    if (playerCharacter == null && BattleManager.Instance?.players.Count > 0)
                    {
                        playerCharacter = BattleManager.Instance.players[0];
                    }
                    defeat = playerCharacter != null && !playerCharacter.isAlive;
                    if (defeat) Debug.Log($"[StoryBattle] 失败条件达成: 主角 {playerCharacter?.generalName} 死亡");
                    break;

                case DefeatType.AllyDeath:
                    var ally = FindPlayer(condition.targetCharacterId);
                    defeat = ally != null && !ally.isAlive;
                    if (defeat) Debug.Log($"[StoryBattle] 失败条件达成: 盟友 {condition.targetCharacterId} 死亡");
                    break;

                case DefeatType.AllAlliesDeath:
                    defeat = AreAllAlliesDefeated();
                    if (defeat) Debug.Log("[StoryBattle] 失败条件达成: 我方全灭");
                    break;

                case DefeatType.ExceedCount:
                    // 检查特定计数（如蒋干连续3次查看黄盖手牌）
                    if (eventCounts.TryGetValue("hand_viewed_huanggai", out int count))
                    {
                        defeat = count >= condition.maxCount;
                        if (defeat) Debug.Log($"[StoryBattle] 失败条件达成: 计数达到 {count}");
                    }
                    break;

                case DefeatType.TurnLimitExceeded:
                    defeat = currentBattle.turnLimit > 0 && currentRound > currentBattle.turnLimit;
                    if (defeat) Debug.Log($"[StoryBattle] 失败条件达成: 超过回合限制 {currentBattle.turnLimit}");
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

            // ⭐ 使用 StoryBattle 中定义的敌人列表
            if (currentBattle?.enemies != null && currentBattle.enemies.Count > 0)
            {
                foreach (var enemyConfig in currentBattle.enemies)
                {
                    Player enemy = FindPlayer(enemyConfig.characterId);
                    if (enemy != null && enemy.isAlive)
                    {
                        return false;
                    }
                }
                return true;
            }

            // 回退逻辑：使用阵营判断
            if (playerCharacter == null)
            {
                // 尝试从我方列表获取第一个角色的阵营
                playerCharacter = BattleManager.Instance.players.Count > 0
                    ? BattleManager.Instance.players[0]
                    : null;
            }

            foreach (var player in BattleManager.Instance.players)
            {
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

            // ⭐ 使用 StoryBattle 中定义的盟友列表
            if (currentBattle?.allies != null && currentBattle.allies.Count > 0)
            {
                foreach (var allyConfig in currentBattle.allies)
                {
                    Player ally = FindPlayer(allyConfig.characterId);
                    if (ally != null && ally.isAlive)
                    {
                        return false;
                    }
                }
                return true;
            }

            // 回退逻辑：使用阵营判断
            if (playerCharacter == null)
            {
                playerCharacter = BattleManager.Instance.players.Count > 0
                    ? BattleManager.Instance.players[0]
                    : null;
            }

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
            bool defeated = target != null && !target.isAlive;
            Debug.Log($"[StoryBattle] 检查目标 {characterId} 是否被击败: {defeated} (target={target?.generalName}, alive={target?.isAlive})");
            return defeated;
        }

        private Player FindPlayer(string characterId)
        {
            if (BattleManager.Instance == null) return null;
            if (string.IsNullOrEmpty(characterId)) return null;

            string cleanId = characterId.ToLower().Replace("_story", "");

            foreach (var player in BattleManager.Instance.players)
            {
                string playerName = player.generalName?.ToLower() ?? "";
                string playerId = player.generalName?.ToLower().Replace("_story", "") ?? "";

                // 更宽松的匹配
                if (playerName.Contains(cleanId) || cleanId.Contains(playerName) ||
                    playerId.Contains(cleanId) || cleanId.Contains(playerId) ||
                    playerName == cleanId || playerId == cleanId)
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
