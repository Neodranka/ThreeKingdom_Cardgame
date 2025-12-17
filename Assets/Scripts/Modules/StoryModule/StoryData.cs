using UnityEngine;
using System.Collections.Generic;

namespace ThreeKingdoms.Story
{
    /// <summary>
    /// 战役数据（一个大章节）
    /// </summary>
    [System.Serializable]
    public class CampaignData
    {
        public string campaignId;           // 战役ID
        public string nameKey;              // 本地化名称key
        public string descriptionKey;       // 本地化描述key
        public Sprite campaignImage;        // 战役图片
        public List<BattleData> battles;    // 包含的战斗列表（兼容旧格式）
        public List<StoryBattle> storyBattles;  // 故事战斗列表（新格式）
        public bool isUnlocked = false;     // 是否解锁
        public bool isCompleted = false;    // 是否完成

        public CampaignData()
        {
            battles = new List<BattleData>();
            storyBattles = new List<StoryBattle>();
        }
    }

    /// <summary>
    /// 故事战斗配置（单场战斗的完整数据）
    /// </summary>
    [System.Serializable]
    public class StoryBattle
    {
        public string battleId;             // 战斗ID
        public string nameKey;              // 本地化名称key
        public string subtitleKey;          // 副标题key
        public string descriptionKey;       // 本地化描述key
        public string briefingKey;          // 战前简报key
        public Sprite battleImage;          // 战斗图片

        [Header("战斗配置")]
        public int difficulty = 1;          // 难度 1-3
        public int turnLimit = 0;           // 回合限制（0表示无限制）

        [Header("角色配置")]
        public List<BattleCharacter> allies;    // 我方角色
        public List<BattleCharacter> enemies;   // 敌方角色

        [Header("胜利/失败条件")]
        public VictoryCondition victoryCondition;   // 胜利条件
        public DefeatCondition defeatCondition;     // 失败条件

        [Header("特殊规则")]
        public List<SpecialRule> specialRules;      // 特殊规则列表
        public string specialRuleKey;               // 特殊规则说明key

        [Header("局内事件")]
        public List<BattleEvent> events;            // 局内事件
        public List<Dialogue> openingDialogue;      // 开场对白
        public List<Dialogue> victoryDialogue;      // 胜利对白
        public List<Dialogue> defeatDialogue;       // 失败对白

        [Header("进度")]
        public bool isUnlocked = false;     // 是否解锁
        public bool isCompleted = false;    // 是否完成
        public int bestScore = 0;           // 最佳评分

        public StoryBattle()
        {
            allies = new List<BattleCharacter>();
            enemies = new List<BattleCharacter>();
            specialRules = new List<SpecialRule>();
            events = new List<BattleEvent>();
            openingDialogue = new List<Dialogue>();
            victoryDialogue = new List<Dialogue>();
            defeatDialogue = new List<Dialogue>();
            victoryCondition = new VictoryCondition();
            defeatCondition = new DefeatCondition();
        }
    }

    /// <summary>
    /// 战斗角色配置
    /// </summary>
    [System.Serializable]
    public class BattleCharacter
    {
        public string characterId;          // 角色ID
        public string nameKey;              // 名称本地化key
        public int maxHP;                   // 体力上限
        public bool isPlayer;               // 是否为玩家控制
        public List<string> skillIds;       // 技能ID列表
        public int initialHandCards = 4;    // 初始手牌数

        public BattleCharacter()
        {
            skillIds = new List<string>();
        }

        public BattleCharacter(string id, string name, int hp, bool player, params string[] skills)
        {
            characterId = id;
            nameKey = name;
            maxHP = hp;
            isPlayer = player;
            skillIds = new List<string>(skills);
            initialHandCards = 4;
        }
    }

    /// <summary>
    /// 胜利条件
    /// </summary>
    [System.Serializable]
    public class VictoryCondition
    {
        public VictoryType type;            // 胜利类型
        public string targetCharacterId;    // 目标角色ID（击败特定角色时使用）
        public int targetTurn;              // 目标回合数（存活N回合时使用）
        public int targetCount;             // 目标数量（累积标记等）
        public string customConditionKey;   // 自定义条件说明key

        public VictoryCondition()
        {
            type = VictoryType.DefeatAllEnemies;
        }
    }

    /// <summary>
    /// 胜利条件类型
    /// </summary>
    public enum VictoryType
    {
        DefeatAllEnemies,           // 击败所有敌人
        DefeatTarget,               // 击败特定目标
        SurviveTurns,               // 存活N回合
        AccumulateMarks,            // 累积特定标记
        ProtectAlly,                // 保护友军存活
        DefeatAllEnemiesOrSurvive,  // 击败所有敌人或存活N回合
        Custom                      // 自定义条件
    }

    /// <summary>
    /// 失败条件
    /// </summary>
    [System.Serializable]
    public class DefeatCondition
    {
        public DefeatType type;             // 失败类型
        public string targetCharacterId;    // 目标角色ID
        public int maxCount;                // 最大次数（蒋干查看手牌次数等）
        public string customConditionKey;   // 自定义条件说明key

        public DefeatCondition()
        {
            type = DefeatType.PlayerDeath;
        }
    }

    /// <summary>
    /// 失败条件类型
    /// </summary>
    public enum DefeatType
    {
        PlayerDeath,                // 玩家死亡
        AllyDeath,                  // 特定友军死亡
        AllAlliesDeath,             // 我方全灭
        ExceedCount,                // 超过特定次数
        TurnLimitExceeded,          // 超过回合限制
        PlayerDeathOrExceedCount,   // 玩家死亡或超过特定次数
        PlayerDeathOrAllAlliesDeath,// 玩家死亡或我方全灭
        Custom                      // 自定义条件
    }

    /// <summary>
    /// 特殊规则
    /// </summary>
    [System.Serializable]
    public class SpecialRule
    {
        public string ruleId;               // 规则ID
        public string nameKey;              // 规则名称key
        public string descriptionKey;       // 规则描述key
        public RuleType type;               // 规则类型
        public int triggerTurn;             // 触发回合（0表示战斗开始就生效）
        public int value;                   // 数值参数
        public string targetId;             // 目标ID

        public SpecialRule() { }

        public SpecialRule(string id, string name, RuleType ruleType, int turn = 0, int val = 0)
        {
            ruleId = id;
            nameKey = name;
            type = ruleType;
            triggerTurn = turn;
            value = val;
        }
    }

    /// <summary>
    /// 规则类型
    /// </summary>
    public enum RuleType
    {
        ModifyInitialCards,     // 修改初始手牌数
        ModifyMaxHP,            // 修改血量上限
        ModifyAttackRange,      // 修改攻击距离
        ModifyDamage,           // 修改伤害（火攻加成等）
        FireDamageBonus,        // 火焰伤害加成
        DisableSkill,           // 禁用某技能
        AddStatus,              // 添加状态
        EnemyNoAttack,          // 敌人不主动攻击
        AllyAutoSupport,        // 友军自动辅助

        // 官渡之战/讨董之战新增规则类型
        ModifyDrawCards,        // 修改摸牌数量
        ModifyHandLimit,        // 修改手牌上限
        DamageReduction,        // 伤害减免
        RandomDiscard,          // 随机弃牌
        FirstDamagePrevention,  // 首次伤害防止
        DrawOnDamage,           // 受伤后摸牌
        DrawOnNoDamage,         // 未造成伤害时摸牌
        DrawOnFirstDamage,      // 首次造成伤害后摸牌
        DamageToWounded,        // 对已受伤目标伤害加成
        DamageOverTime,         // 持续伤害（每回合失去体力）
        LowHandDamageBonus,     // 低手牌时伤害加成
        FirstDamageBonus,       // 首次伤害加成
        ForcedDiscard,          // 强制弃牌
        BonusDraw,              // 额外摸牌
        ExtraSlash,             // 额外出杀次数
        NoAttackTarget,         // 禁止攻击目标
        NoAllyPeach,            // 禁止对盟友用桃

        Custom                  // 自定义规则
    }

    /// <summary>
    /// 战斗事件
    /// </summary>
    [System.Serializable]
    public class BattleEvent
    {
        public EventTrigger trigger;        // 触发类型
        public string triggerParam;         // 触发参数（角色ID/回合数/技能ID等）
        public string triggerParam2;        // 第二触发参数
        public List<Dialogue> dialogues;    // 触发的对白
        public bool triggered = false;      // 是否已触发（一次性事件）
        public bool repeatable = false;     // 是否可重复触发

        public BattleEvent()
        {
            dialogues = new List<Dialogue>();
        }

        public BattleEvent(EventTrigger trig, string param, params Dialogue[] dlgs)
        {
            trigger = trig;
            triggerParam = param;
            dialogues = new List<Dialogue>(dlgs);
            triggered = false;
            repeatable = false;
        }
    }

    /// <summary>
    /// 事件触发类型
    /// </summary>
    public enum EventTrigger
    {
        OnBattleStart,          // 战斗开始
        OnRoundStart,           // 回合开始（参数为回合数）
        OnTurnStart,            // 角色回合开始（参数为角色ID）
        OnTurnEnd,              // 角色回合结束（参数为角色ID）
        OnFirstAttack,          // 首次攻击（参数为攻击者ID）
        OnFirstKill,            // 首次出杀（参数为使用者ID）
        OnSkillActivate,        // 技能发动（参数为技能ID）
        OnDamageDealt,          // 造成伤害（参数为受伤者ID）
        OnDamageTaken,          // 受到伤害（参数为受伤者ID）
        OnNearDeath,            // 濒死（参数为濒死者ID）
        OnDeath,                // 死亡（参数为死亡者ID）
        OnCardPlayed,           // 出牌（参数为牌类型）
        OnHPBelow,              // 血量低于（参数为角色ID，参数2为血量）
        OnMarkerGained,         // 获得标记（参数为标记类型）
        OnHandCardViewed,       // 手牌被查看（参数为被查看者ID）
        Custom                  // 自定义触发
    }

    /// <summary>
    /// 对白
    /// </summary>
    [System.Serializable]
    public class Dialogue
    {
        public string speakerKey;           // 说话人名称key（空表示旁白）
        public string contentKey;           // 对白内容key
        public string speakerPortrait;      // 说话人头像路径
        public float displayTime = 3f;      // 显示时间

        public Dialogue() { }

        public Dialogue(string speaker, string content, float time = 3f)
        {
            speakerKey = speaker;
            contentKey = content;
            displayTime = time;
        }

        /// <summary>
        /// 创建旁白
        /// </summary>
        public static Dialogue Narration(string content, float time = 3f)
        {
            return new Dialogue("", content, time);
        }
    }

    /// <summary>
    /// 故事模式存档数据
    /// </summary>
    [System.Serializable]
    public class StoryProgressData
    {
        public int currentCampaignIndex = 0;
        public int currentBattleIndex = 0;
        public List<CampaignProgress> campaignProgresses;

        public StoryProgressData()
        {
            campaignProgresses = new List<CampaignProgress>();
        }
    }

    /// <summary>
    /// 战役进度
    /// </summary>
    [System.Serializable]
    public class CampaignProgress
    {
        public string campaignId;
        public bool isUnlocked;
        public bool isCompleted;
        public List<BattleProgress> battleProgresses;

        public CampaignProgress()
        {
            battleProgresses = new List<BattleProgress>();
        }
    }

    /// <summary>
    /// 战斗进度
    /// </summary>
    [System.Serializable]
    public class BattleProgress
    {
        public string battleId;
        public bool isUnlocked;
        public bool isCompleted;
        public int bestScore;
        public int completionCount;
    }

    // ==================== 兼容旧数据结构 ====================

    /// <summary>
    /// 旧版战斗数据（兼容用）
    /// </summary>
    [System.Serializable]
    public class BattleData
    {
        public string battleId;
        public string nameKey;
        public string descriptionKey;
        public string briefingKey;
        public Sprite battleImage;
        public int playerCount = 4;
        public bool enableIdentityMode = true;
        public int difficulty = 1;
        public string playerGeneralId;
        public List<string> enemyGeneralIds;
        public string specialRuleKey;
        public int turnLimit = 0;
        public bool isUnlocked = false;
        public bool isCompleted = false;
        public int bestScore = 0;

        public BattleData()
        {
            enemyGeneralIds = new List<string>();
        }

        /// <summary>
        /// 转换为新版StoryBattle
        /// </summary>
        public StoryBattle ToStoryBattle()
        {
            var battle = new StoryBattle
            {
                battleId = battleId,
                nameKey = nameKey,
                descriptionKey = descriptionKey,
                briefingKey = briefingKey,
                battleImage = battleImage,
                difficulty = difficulty,
                turnLimit = turnLimit,
                specialRuleKey = specialRuleKey,
                isUnlocked = isUnlocked,
                isCompleted = isCompleted,
                bestScore = bestScore
            };

            // 添加玩家角色
            if (!string.IsNullOrEmpty(playerGeneralId))
            {
                battle.allies.Add(new BattleCharacter(playerGeneralId, playerGeneralId, 4, true));
            }

            // 添加敌方角色
            foreach (var enemyId in enemyGeneralIds)
            {
                battle.enemies.Add(new BattleCharacter(enemyId, enemyId, 4, false));
            }

            return battle;
        }
    }
}
