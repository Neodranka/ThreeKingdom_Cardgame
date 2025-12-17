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
        private bool battleEndHandled = false;  // ⭐ 防止重复处理游戏结束

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

        // ⭐ 赤壁之战 Custom 规则状态
        private bool huweiRule = false;              // 虎威：张飞杀有30%令目标弃牌
        private bool noPeachRule = false;            // 单骑断桥：张飞不能用桃
        private bool debateRule = false;             // 舌战模式
        private bool persuadeRule = false;           // 以理服人：诸葛亮杀伤可改弃2牌
        private bool forgeLetterRule = false;        // 伪造书信：反间成功获得标记
        private bool trickRule = false;              // 中计：黑牌视为假情报
        private bool suspicionRule = false;          // 曹操猜忌：标记减蔡瑁HP
        private bool seasickRule = false;            // 水土不服：曹军30%掉血
        private bool guanyuPriorityRule = false;     // 关羽优先攻击最低血敌人
        private int fanjianMarkerCount = 0;          // 反间标记计数
        private int stealSuccessCount = 0;           // 蒋干盗书成功次数（连续）

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
            battleEndHandled = false;  // ⭐ 重置游戏结束处理标志

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

                // ==================== 讨董之战新增规则 ====================

                case RuleType.HealOnKill:
                    // 击杀后回复体力（救民）
                    markers["heal_on_kill"] = rule.value > 0 ? rule.value : 1;
                    Debug.Log($"[规则] 击杀后全体回复{markers["heal_on_kill"]}点体力");
                    break;

                case RuleType.FirstSlashUndodgeable:
                    // 首次杀不可闪避（酒尚温）
                    string fsuTarget = rule.targetId ?? "player";
                    markers[$"first_slash_undodgeable_{fsuTarget}"] = 1;
                    Debug.Log($"[规则] {fsuTarget}首次使用杀不可闪避");
                    break;

                case RuleType.TemporaryHPBonus:
                    // 临时体力加成（一骑当千）- 会在指定回合后消失
                    {
                        string thbTarget = rule.targetId ?? "enemies";
                        int bonusHP = rule.value > 0 ? rule.value : 3;
                        int removeRound = 3;
                        if (!string.IsNullOrEmpty(rule.extraInfo) && int.TryParse(rule.extraInfo, out int parsedRound))
                        {
                            removeRound = parsedRound;
                        }
                        markers[$"temp_hp_bonus_{thbTarget}"] = bonusHP;
                        markers[$"temp_hp_remove_round_{thbTarget}"] = removeRound;
                        // 立即应用HP加成
                        List<Player> thbTargets = GetTargetPlayers(thbTarget);
                        foreach (var player in thbTargets)
                        {
                            player.maxHP += bonusHP;
                            player.currentHP += bonusHP;
                        }
                        Debug.Log($"[规则] {thbTarget}临时+{bonusHP}体力，第{removeRound}回合后移除");
                    }
                    break;

                case RuleType.DoubleDamage:
                    // 伤害翻倍（绝世武力）
                    {
                        string ddTarget = rule.targetId ?? "enemies";
                        int ddRound = rule.triggerTurn > 0 ? rule.triggerTurn : 1;
                        markers[$"double_damage_{ddTarget}"] = ddRound;
                        Debug.Log($"[规则] 第{ddRound}回合{ddTarget}伤害翻倍");
                    }
                    break;

                case RuleType.ReinforcementOnRound:
                    // 指定回合增援（逐步增援）
                    markers["reinforcement_active"] = 1;
                    if (!string.IsNullOrEmpty(rule.extraInfo))
                    {
                        // 格式: "guanyu_story:2,liubei:3"
                        markers["reinforcement_data"] = 1; // 标记有增援数据
                        Debug.Log($"[规则] 逐步增援配置: {rule.extraInfo}");
                    }
                    break;

                case RuleType.AttackRangeBonus:
                    // 攻击距离加成（西凉铁骑）
                    {
                        string arbTarget = rule.targetId ?? "enemies";
                        int arbValue = rule.value > 0 ? rule.value : 1;
                        List<Player> arbTargets = GetTargetPlayers(arbTarget);
                        foreach (var player in arbTargets)
                        {
                            player.attackRange += arbValue;
                        }
                        Debug.Log($"[规则] {arbTarget}攻击距离+{arbValue}");
                    }
                    break;

                case RuleType.EscapeOnDeath:
                    // 死亡时逃跑（挟天子西遁）
                    {
                        string eodTarget = rule.targetId ?? "";
                        if (!string.IsNullOrEmpty(eodTarget))
                        {
                            markers[$"escape_on_death_{eodTarget}"] = 1;
                            Debug.Log($"[规则] {eodTarget}濒死时将逃跑而非死亡");
                        }
                    }
                    break;

                case RuleType.ReduceHandLimit:
                    // 减少手牌上限（军心崩溃）
                    {
                        string rhlTarget = rule.targetId ?? "enemies";
                        int rhlValue = rule.value != 0 ? rule.value : -1;
                        List<Player> rhlTargets = GetTargetPlayers(rhlTarget);
                        foreach (var player in rhlTargets)
                        {
                            player.handCardLimit += rhlValue;
                            if (player.handCardLimit < 0) player.handCardLimit = 0;
                        }
                        Debug.Log($"[规则] {rhlTarget}手牌上限{rhlValue}");
                    }
                    break;

                case RuleType.RandomDamage:
                    // 随机伤害（饥疲交迫）- 每回合结束50%概率失去1HP
                    markers["random_damage_chance"] = rule.value > 0 ? rule.value : 50;
                    markers["random_damage_target"] = rule.targetId == "enemies" ? 1 : 0;
                    Debug.Log($"[规则] 敌方每回合结束{markers["random_damage_chance"]}%概率失去1体力");
                    break;

                case RuleType.DamageIncrease:
                    // 伤害增加（士气低迷）- 概率额外受到1点伤害
                    markers["damage_increase_chance"] = rule.value > 0 ? rule.value : 30;
                    markers["damage_increase_target"] = rule.targetId == "allies" ? 1 : 0;
                    Debug.Log($"[规则] 我方受伤害{markers["damage_increase_chance"]}%概率+1");
                    break;

                case RuleType.DamageBonus:
                    // 特定目标伤害加成（复仇之战）
                    {
                        string dbAttacker = rule.targetId ?? "";
                        string dbTarget = rule.extraInfo ?? "";
                        if (!string.IsNullOrEmpty(dbAttacker) && !string.IsNullOrEmpty(dbTarget))
                        {
                            markers[$"damage_bonus_{dbAttacker}_to_{dbTarget}"] = rule.value > 0 ? rule.value : 1;
                            Debug.Log($"[规则] {dbAttacker}对{dbTarget}伤害+{rule.value}");
                        }
                    }
                    break;

                case RuleType.Custom:
                    // ⭐ 自定义规则处理
                    ApplyCustomRule(rule);
                    break;

                case RuleType.AllyAutoSupport:
                    // ⭐ 盟友自动支援（鲁肃斡旋）
                    markers["ally_auto_support"] = 1;
                    Debug.Log("[规则] 盟友自动支援生效");
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

        /// <summary>
        /// ⭐ 应用自定义规则（赤壁之战专用）
        /// </summary>
        private void ApplyCustomRule(SpecialRule rule)
        {
            string ruleId = rule.ruleId?.ToLower() ?? "";
            Debug.Log($"[Custom规则] 应用规则: {ruleId}");

            switch (ruleId)
            {
                case "tutorial":
                    // 新手教学 - 暂无特殊处理，仅标记
                    markers["tutorial"] = 1;
                    break;

                case "huwei":
                    // 虎威：张飞使用【杀】时，有30%概率令目标弃置1张手牌
                    huweiRule = true;
                    Debug.Log("[规则] 虎威生效：张飞使用杀有30%令目标弃牌");
                    break;

                case "no_peach":
                    // 单骑断桥：张飞不能使用【桃】
                    noPeachRule = true;
                    Debug.Log("[规则] 单骑断桥生效：张飞不能使用桃");
                    break;

                case "debate":
                    // 舌战模式：所有伤害代表"辩论失利"
                    debateRule = true;
                    Debug.Log("[规则] 舌战模式生效：伤害代表辩论失利");
                    break;

                case "persuade":
                    // 以理服人：诸葛亮使用【杀】造成伤害时，可选择改为令目标弃2张牌
                    persuadeRule = true;
                    Debug.Log("[规则] 以理服人生效：诸葛亮杀伤可改弃2牌");
                    break;

                case "forge_letter":
                    // 伪造书信：周瑜每次成功发动【反间】获得1个"反间"标记
                    forgeLetterRule = true;
                    fanjianMarkerCount = 0;
                    Debug.Log("[规则] 伪造书信生效：反间成功获得标记");
                    break;

                case "trick":
                    // 中计：当蒋干查看手牌时，若周瑜手牌中有黑色牌，视为"假情报"
                    trickRule = true;
                    Debug.Log("[规则] 中计生效：黑牌视为假情报");
                    break;

                case "suspicion":
                    // 曹操猜忌：每累积1个反间标记，蔡瑁体力上限-1
                    suspicionRule = true;
                    Debug.Log("[规则] 曹操猜忌生效：标记减蔡瑁HP");
                    break;

                case "seasick":
                    // 水土不服：曹军水兵每回合30%概率掉1血
                    seasickRule = true;
                    Debug.Log("[规则] 水土不服生效：曹军30%掉血");
                    break;

                case "guanyu_priority":
                    // 关羽优先攻击血量最低的敌人
                    guanyuPriorityRule = true;
                    Debug.Log("[规则] 关羽归来生效：优先攻击最低血敌人");
                    break;

                default:
                    Debug.Log($"[Custom规则] 未知规则ID: {ruleId}");
                    break;
            }
        }

        #endregion

        #region 赤壁之战 Custom 规则公共API

        /// <summary>
        /// ⭐ 检查角色是否可以使用桃（单骑断桥规则）
        /// </summary>
        public bool CanUsePeach(Player player)
        {
            if (!noPeachRule) return true;

            // 检查是否是张飞
            string playerId = player?.generalName?.ToLower().Replace("_story", "") ?? "";
            if (playerId.Contains("zhangfei") || playerId.Contains("张飞"))
            {
                Debug.Log("[规则] 单骑断桥：张飞不能使用桃");
                return false;
            }
            return true;
        }

        /// <summary>
        /// ⭐ 杀命中后触发虎威效果（30%概率令目标弃牌）
        /// </summary>
        public void TryTriggerHuwei(Player attacker, Player target)
        {
            if (!huweiRule) return;

            // 检查攻击者是否是张飞
            string attackerId = attacker?.generalName?.ToLower().Replace("_story", "") ?? "";
            if (!attackerId.Contains("zhangfei") && !attackerId.Contains("张飞")) return;

            // 30%概率
            if (Random.Range(0, 100) < 30)
            {
                if (target.handCards.Count > 0)
                {
                    // 随机弃置1张手牌
                    int randomIndex = Random.Range(0, target.handCards.Count);
                    Card discardCard = target.handCards[randomIndex];
                    target.DiscardCard(discardCard);
                    Debug.Log($"[虎威] {target.generalName} 被气势震慑，弃置了1张手牌");

                    // 显示提示
                    if (ThreeKingdoms.UI.BattleUI.Instance != null)
                    {
                        ThreeKingdoms.UI.BattleUI.Instance.ShowMessage($"【虎威】{target.generalName} 弃置1张手牌！");
                    }
                }
            }
        }

        /// <summary>
        /// ⭐ 检查是否触发以理服人（诸葛亮杀伤可改弃2牌）
        /// 返回true表示应该让玩家选择，false表示不适用此规则
        /// </summary>
        public bool ShouldOfferPersuade(Player attacker, Player target)
        {
            if (!persuadeRule) return false;

            // 检查攻击者是否是诸葛亮且是玩家控制
            string attackerId = attacker?.generalName?.ToLower().Replace("_story", "") ?? "";
            if (!attackerId.Contains("zhugeliang") && !attackerId.Contains("诸葛亮")) return false;

            // 检查目标手牌数是否足够
            if (target.handCards.Count < 2) return false;

            return !attacker.isAI; // 只对玩家提供选择
        }

        /// <summary>
        /// ⭐ 执行以理服人效果（目标弃2牌代替伤害）
        /// </summary>
        public void ExecutePersuade(Player target)
        {
            if (target.handCards.Count >= 2)
            {
                // 弃置2张手牌
                for (int i = 0; i < 2 && target.handCards.Count > 0; i++)
                {
                    int randomIndex = Random.Range(0, target.handCards.Count);
                    Card discardCard = target.handCards[randomIndex];
                    target.DiscardCard(discardCard);
                }
                Debug.Log($"[以理服人] {target.generalName} 被说服，弃置了2张手牌");

                if (ThreeKingdoms.UI.BattleUI.Instance != null)
                {
                    ThreeKingdoms.UI.BattleUI.Instance.ShowMessage($"【以理服人】{target.generalName} 弃置2张手牌");
                }
            }
        }

        /// <summary>
        /// ⭐ 周瑜反间成功时触发（伪造书信规则）
        /// </summary>
        public void OnFanjianSuccess(Player zhouyu)
        {
            if (!forgeLetterRule) return;

            fanjianMarkerCount++;
            Debug.Log($"[伪造书信] 周瑜获得第{fanjianMarkerCount}个反间标记");

            if (ThreeKingdoms.UI.BattleUI.Instance != null)
            {
                ThreeKingdoms.UI.BattleUI.Instance.ShowMessage($"【伪造书信】获得反间标记（{fanjianMarkerCount}/3）");
            }

            // 触发事件
            TriggerEvents(EventTrigger.OnMarkerGained, $"fanjian_{fanjianMarkerCount}");

            // 曹操猜忌：每个标记减蔡瑁HP
            if (suspicionRule)
            {
                ApplySuspicionEffect();
            }

            // 检查胜利条件
            CheckVictoryCondition();
        }

        /// <summary>
        /// ⭐ 应用曹操猜忌效果（减蔡瑁HP）
        /// </summary>
        private void ApplySuspicionEffect()
        {
            Player caimao = FindPlayer("caimao");
            if (caimao != null && caimao.isAlive)
            {
                caimao.maxHP--;
                if (caimao.currentHP > caimao.maxHP)
                {
                    caimao.currentHP = caimao.maxHP;
                }
                Debug.Log($"[曹操猜忌] 蔡瑁体力上限降至{caimao.maxHP}");

                if (ThreeKingdoms.UI.BattleUI.Instance != null)
                {
                    ThreeKingdoms.UI.BattleUI.Instance.ShowMessage($"【曹操猜忌】蔡瑁体力上限-1");
                    ThreeKingdoms.UI.BattleUI.Instance.UpdateAllPlayerInfo();
                }
            }
        }

        /// <summary>
        /// ⭐ 蒋干盗书时检查是否中计（周瑜手牌有黑牌=假情报）
        /// </summary>
        public bool CheckTrickRule(Player zhouyu)
        {
            if (!trickRule) return false;

            // 检查周瑜手牌是否有黑色牌
            foreach (var card in zhouyu.handCards)
            {
                if (card.suit == CardSuit.Spade || card.suit == CardSuit.Club)
                {
                    Debug.Log("[中计] 周瑜手中有黑色牌，蒋干获得假情报");
                    stealSuccessCount = 0; // 重置连续成功计数
                    return true; // 有黑牌 = 假情报
                }
            }

            // 没有黑牌 = 真实情报，计入失败条件
            stealSuccessCount++;
            Debug.Log($"[中计] 周瑜手中无黑色牌，蒋干获得真实情报（连续{stealSuccessCount}次）");

            // 检查失败条件
            if (stealSuccessCount >= 3)
            {
                Debug.Log("[中计] 蒋干连续3次获得真实情报，计谋败露！");
            }

            return false;
        }

        /// <summary>
        /// ⭐ 回合开始时触发水土不服（曹军30%掉血）
        /// </summary>
        public void TrySeasickEffect(Player player)
        {
            if (!seasickRule) return;

            // 检查是否是曹军水兵
            string playerId = player?.generalName?.ToLower().Replace("_story", "") ?? "";
            if (!playerId.Contains("caojun") && !playerId.Contains("曹军") && !playerId.Contains("sailor"))
                return;

            // 30%概率
            if (Random.Range(0, 100) < 30)
            {
                player.TakeDamage(1, null);
                Debug.Log($"[水土不服] {player.generalName} 晕船，失去1点体力");

                if (ThreeKingdoms.UI.BattleUI.Instance != null)
                {
                    ThreeKingdoms.UI.BattleUI.Instance.ShowMessage($"【水土不服】{player.generalName} 晕船-1HP");
                }

                // 触发事件
                TriggerEvents(EventTrigger.OnSkillActivate, "beiren");
            }
        }

        /// <summary>
        /// ⭐ 获取关羽应该优先攻击的目标（最低血敌人）
        /// </summary>
        public Player GetGuanyuPriorityTarget(Player guanyu)
        {
            if (!guanyuPriorityRule) return null;

            // 检查是否是关羽
            string playerId = guanyu?.generalName?.ToLower().Replace("_story", "") ?? "";
            if (!playerId.Contains("guanyu") && !playerId.Contains("关羽")) return null;

            // 获取所有存活的敌人
            List<Player> enemies = GetEnemyPlayers();
            Player lowestHPEnemy = null;
            int lowestHP = int.MaxValue;

            foreach (var enemy in enemies)
            {
                if (enemy.isAlive && enemy.currentHP < lowestHP)
                {
                    lowestHP = enemy.currentHP;
                    lowestHPEnemy = enemy;
                }
            }

            if (lowestHPEnemy != null)
            {
                Debug.Log($"[关羽归来] 优先攻击血量最低的敌人：{lowestHPEnemy.generalName}（{lowestHP}血）");
            }

            return lowestHPEnemy;
        }

        /// <summary>
        /// ⭐ 获取当前反间标记数
        /// </summary>
        public int GetFanjianMarkerCount()
        {
            return fanjianMarkerCount;
        }

        /// <summary>
        /// ⭐ 获取蒋干连续盗书成功次数
        /// </summary>
        public int GetStealSuccessCount()
        {
            return stealSuccessCount;
        }

        /// <summary>
        /// ⭐ 检查是否是舌战模式
        /// </summary>
        public bool IsDebateMode()
        {
            return debateRule;
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

            // ⭐ 赤壁之战：水土不服规则（曹军水兵30%掉血）
            TrySeasickEffect(player);

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

            // ⭐ 处理增援角色（虎牢关血战：关羽R2加入，刘备R3加入）
            ProcessReinforcements();

            // ⭐ 处理临时HP加成移除（一骑当千）
            ProcessTemporaryHPBonusRemoval();

            // 触发回合开始事件
            TriggerEvents(EventTrigger.OnRoundStart, currentRound.ToString());

            // 检查胜负条件
            CheckVictoryCondition();
            CheckDefeatCondition();
        }

        /// <summary>
        /// 处理增援角色加入
        /// </summary>
        private void ProcessReinforcements()
        {
            if (currentBattle?.reinforcements == null || currentBattle.reinforcements.Count == 0) return;

            foreach (var reinforcement in currentBattle.reinforcements)
            {
                if (reinforcement.roundNumber == currentRound && reinforcement.character != null)
                {
                    Debug.Log($"[增援] 第{currentRound}回合，{reinforcement.character.nameKey} 加入战斗！");

                    // 创建增援角色
                    if (BattleManager.Instance != null)
                    {
                        // 获取角色数据
                        var charData = StoryCharacterDatabase.Instance?.GetCharacter(reinforcement.character.characterId);
                        if (charData != null)
                        {
                            // 创建新玩家 GameObject 和 Player 组件
                            GameObject playerObj = new GameObject($"Player_{charData.nameKey}_Reinforcement");
                            Player newPlayer = playerObj.AddComponent<Player>();

                            newPlayer.generalName = LocalizationManager.Instance?.GetText(charData.nameKey) ?? charData.nameKey;
                            newPlayer.faction = charData.faction;
                            newPlayer.maxHP = charData.maxHP;
                            newPlayer.currentHP = charData.maxHP;
                            newPlayer.isAlive = true;
                            newPlayer.isAI = true;  // 增援默认为AI控制

                            // 添加技能
                            if (charData.skills != null && charData.skills.Count > 0)
                            {
                                newPlayer.skills = new List<DatabaseModule.ISkill>();
                                foreach (var skillId in charData.skills)
                                {
                                    var skill = SkillFactory.CreateSkill(skillId, newPlayer);
                                    if (skill != null)
                                    {
                                        newPlayer.skills.Add(skill);
                                    }
                                }
                            }

                            // 发初始手牌
                            for (int i = 0; i < 4; i++)
                            {
                                var card = DeckManager.Instance?.DrawCard();
                                if (card != null) newPlayer.DrawCard(card);
                            }

                            // 添加到战斗
                            BattleManager.Instance.players.Add(newPlayer);

                            // 触发增援事件
                            TriggerEvents(EventTrigger.OnReinforcementJoin, reinforcement.character.characterId);

                            Debug.Log($"[增援] {newPlayer.generalName} 已加入战斗，HP:{newPlayer.currentHP}/{newPlayer.maxHP}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 处理临时HP加成移除（一骑当千）
        /// </summary>
        private void ProcessTemporaryHPBonusRemoval()
        {
            List<string> keysToRemove = new List<string>();

            foreach (var kvp in markers)
            {
                if (kvp.Key.StartsWith("temp_hp_remove_round_"))
                {
                    string targetId = kvp.Key.Replace("temp_hp_remove_round_", "");
                    int removeRound = kvp.Value;

                    if (currentRound > removeRound)
                    {
                        // 需要移除临时HP加成
                        if (markers.TryGetValue($"temp_hp_bonus_{targetId}", out int bonusHP))
                        {
                            List<Player> targets = GetTargetPlayers(targetId);
                            foreach (var player in targets)
                            {
                                player.maxHP -= bonusHP;
                                if (player.currentHP > player.maxHP)
                                    player.currentHP = player.maxHP;
                                Debug.Log($"[规则] {player.generalName}的临时+{bonusHP}体力效果消失");
                            }

                            keysToRemove.Add(kvp.Key);
                            keysToRemove.Add($"temp_hp_bonus_{targetId}");
                        }
                    }
                }
            }

            // 清理已处理的标记
            foreach (var key in keysToRemove)
            {
                markers.Remove(key);
            }
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
            // ⭐ 防止重复检查 - 如果已处理过游戏结束或战斗不活跃，直接返回
            if (!isBattleActive || battleEndHandled || currentBattle?.victoryCondition == null) return;

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
                    // ⭐ 优先检查反间标记（第四章蒋干盗书）
                    int markCount = fanjianMarkerCount;
                    // 也检查通用markers字典
                    if (markers.TryGetValue("fanjian", out int fanjianCount))
                    {
                        markCount = System.Math.Max(markCount, fanjianCount);
                    }
                    if (markers.TryGetValue("zhaxiang", out int zhaxiangCount))
                    {
                        markCount = System.Math.Max(markCount, zhaxiangCount);
                    }
                    victory = markCount >= condition.targetCount;
                    if (victory) Debug.Log($"[StoryBattle] 胜利条件达成: 累积 {markCount} 个标记");
                    break;

                case VictoryType.ProtectAlly:
                    // 保护目标存活且所有敌人被击败
                    var ally = FindPlayer(condition.targetCharacterId);
                    bool allyAlive = ally != null && ally.isAlive;
                    bool enemiesDefeated = AreAllEnemiesDefeated();
                    victory = allyAlive && enemiesDefeated;
                    if (victory) Debug.Log($"[StoryBattle] 胜利条件达成: 保护 {condition.targetCharacterId} 存活并击败所有敌人");
                    break;

                case VictoryType.DefeatAllEnemiesOrSurvive:
                    // 击败所有敌人或存活N回合
                    bool allDefeated = AreAllEnemiesDefeated();
                    bool survivedTurns = currentRound >= condition.targetTurn;
                    victory = allDefeated || survivedTurns;
                    if (victory)
                    {
                        if (allDefeated)
                            Debug.Log("[StoryBattle] 胜利条件达成: 击败所有敌人");
                        else
                            Debug.Log($"[StoryBattle] 胜利条件达成: 存活 {condition.targetTurn} 回合");
                    }
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
            // ⭐ 防止重复检查 - 如果已处理过游戏结束或战斗不活跃，直接返回
            if (!isBattleActive || battleEndHandled || currentBattle?.defeatCondition == null) return;

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

                case DefeatType.PlayerDeathOrExceedCount:
                    // 玩家死亡或超过特定次数（如蒋干盗书）
                    if (playerCharacter == null && currentBattle?.allies != null && currentBattle.allies.Count > 0)
                    {
                        playerCharacter = FindPlayer(currentBattle.allies[0].characterId);
                    }
                    if (playerCharacter == null && BattleManager.Instance?.players.Count > 0)
                    {
                        playerCharacter = BattleManager.Instance.players[0];
                    }
                    bool playerDead = playerCharacter != null && !playerCharacter.isAlive;
                    bool exceedCount = false;
                    if (eventCounts.TryGetValue("hand_viewed_huanggai", out int viewCount))
                    {
                        exceedCount = viewCount >= condition.maxCount;
                    }
                    defeat = playerDead || exceedCount;
                    if (defeat)
                    {
                        if (playerDead)
                            Debug.Log($"[StoryBattle] 失败条件达成: 主角 {playerCharacter?.generalName} 死亡");
                        else
                            Debug.Log($"[StoryBattle] 失败条件达成: 计数达到 {viewCount}");
                    }
                    break;

                case DefeatType.PlayerDeathOrAllAlliesDeath:
                    // 玩家死亡或我方全灭
                    if (playerCharacter == null && currentBattle?.allies != null && currentBattle.allies.Count > 0)
                    {
                        playerCharacter = FindPlayer(currentBattle.allies[0].characterId);
                    }
                    if (playerCharacter == null && BattleManager.Instance?.players.Count > 0)
                    {
                        playerCharacter = BattleManager.Instance.players[0];
                    }
                    bool mainPlayerDead = playerCharacter != null && !playerCharacter.isAlive;
                    bool allAlliesDead = AreAllAlliesDefeated();
                    defeat = mainPlayerDead || allAlliesDead;
                    if (defeat)
                    {
                        if (mainPlayerDead)
                            Debug.Log($"[StoryBattle] 失败条件达成: 主角 {playerCharacter?.generalName} 死亡");
                        else
                            Debug.Log("[StoryBattle] 失败条件达成: 我方全灭");
                    }
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

            string cleanId = characterId.ToLower().Replace("_story", "").Replace("_", "");

            foreach (var player in BattleManager.Instance.players)
            {
                // ⭐ 检查 generalData.generalId
                if (player.generalData != null)
                {
                    string dataId = player.generalData.generalId?.ToLower().Replace("_story", "").Replace("_", "") ?? "";
                    if (dataId == cleanId || dataId.Contains(cleanId) || cleanId.Contains(dataId))
                    {
                        return player;
                    }
                }

                // 检查中文名匹配
                string playerName = player.generalName ?? "";
                string expectedChinese = GetChineseNameFromParam(characterId);

                if (!string.IsNullOrEmpty(expectedChinese) && playerName == expectedChinese)
                {
                    return player;
                }

                // 检查英文名匹配
                string playerNameLower = playerName.ToLower().Replace("_story", "").Replace("_", "");
                if (playerNameLower == cleanId || playerNameLower.Contains(cleanId) || cleanId.Contains(playerNameLower))
                {
                    return player;
                }
            }

            Debug.LogWarning($"[StoryBattle] FindPlayer 找不到角色: {characterId}");
            return null;
        }

        #endregion

        #region 胜负处理

        private void OnVictory()
        {
            // ⭐ 防止重复调用
            if (battleEndHandled)
            {
                Debug.Log("[StoryBattle] OnVictory: 已处理过游戏结束，跳过");
                return;
            }
            battleEndHandled = true;

            Debug.Log("[StoryBattle] 胜利！");
            isBattleActive = false;

            // ⭐ 使用协程处理胜利，以便等待任何正在进行的对话完成
            StartCoroutine(HandleVictorySequence());
        }

        /// <summary>
        /// ⭐ 处理胜利序列 - 等待现有对话完成后显示胜利对话
        /// </summary>
        private IEnumerator HandleVictorySequence()
        {
            // ⭐ 等待任何正在进行的对话完成
            if (isDialogueShowing)
            {
                Debug.Log("[StoryBattle] 等待现有对话完成...");
                yield return new WaitUntil(() => !isDialogueShowing);
                Debug.Log("[StoryBattle] 现有对话已完成");
                // 等待一帧确保状态稳定
                yield return null;
            }

            // 显示胜利对白
            if (currentBattle?.victoryDialogue != null && currentBattle.victoryDialogue.Count > 0)
            {
                Debug.Log($"[StoryBattle] 开始显示胜利对话，共 {currentBattle.victoryDialogue.Count} 句");
                yield return StartCoroutine(ShowDialogueSequence(currentBattle.victoryDialogue, null));
                Debug.Log("[StoryBattle] 胜利对话完成，准备标记战斗完成");
            }
            else
            {
                Debug.Log("[StoryBattle] 没有胜利对话，直接标记完成");
            }

            // 标记战斗完成
            MarkBattleCompleted();
        }

        private void OnDefeat()
        {
            // ⭐ 防止重复调用
            if (battleEndHandled)
            {
                Debug.Log("[StoryBattle] OnDefeat: 已处理过游戏结束，跳过");
                return;
            }
            battleEndHandled = true;

            Debug.Log("[StoryBattle] 失败！");
            isBattleActive = false;

            // ⭐ 使用协程处理失败，以便等待任何正在进行的对话完成
            StartCoroutine(HandleDefeatSequence());
        }

        /// <summary>
        /// ⭐ 处理失败序列 - 等待现有对话完成后显示失败对话
        /// </summary>
        private IEnumerator HandleDefeatSequence()
        {
            // ⭐ 等待任何正在进行的对话完成
            if (isDialogueShowing)
            {
                Debug.Log("[StoryBattle] 等待现有对话完成...");
                yield return new WaitUntil(() => !isDialogueShowing);
                Debug.Log("[StoryBattle] 现有对话已完成");
                // 等待一帧确保状态稳定
                yield return null;
            }

            // 显示失败对白
            if (currentBattle?.defeatDialogue != null && currentBattle.defeatDialogue.Count > 0)
            {
                Debug.Log($"[StoryBattle] 开始显示失败对话，共 {currentBattle.defeatDialogue.Count} 句");
                yield return StartCoroutine(ShowDialogueSequence(currentBattle.defeatDialogue, null));
                Debug.Log("[StoryBattle] 失败对话完成，准备返回故事模式");
            }
            else
            {
                Debug.Log("[StoryBattle] 没有失败对话，直接返回");
            }

            // 返回故事模式界面
            ReturnToStoryMode();
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
            Debug.Log($"[StoryBattle] ReturnToStoryMode 被调用! isDialogueShowing={isDialogueShowing}");
            Debug.Log($"[StoryBattle] 调用堆栈:\n{System.Environment.StackTrace}");
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

            // ⭐ 防止重复启动对话序列
            if (isDialogueShowing)
            {
                Debug.LogWarning("[StoryBattle] 对话序列已在运行中，跳过重复调用!");
                yield break;
            }

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

            int dialogueIndex = 0;
            foreach (var dialogue in dialogues)
            {
                dialogueIndex++;
                Debug.Log($"[StoryBattle] 显示第 {dialogueIndex}/{dialogues.Count} 句对话: {dialogue.contentKey}");

                ShowDialogue(dialogue);

                // ⭐ 等待玩家点击（带冷却时间）
                waitingForClick = true;
                clickCooldown = CLICK_COOLDOWN_TIME;  // 重置冷却
                Debug.Log($"[StoryBattle] 等待玩家点击... (cooldown={CLICK_COOLDOWN_TIME}s)");
                yield return new WaitUntil(() => !waitingForClick);
                Debug.Log($"[StoryBattle] 第 {dialogueIndex} 句对话点击确认");

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
