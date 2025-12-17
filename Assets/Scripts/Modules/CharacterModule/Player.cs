using System.Collections.Generic;
using UnityEngine;
using ThreeKingdoms.AI;
using ThreeKingdoms.DatabaseModule;

namespace ThreeKingdoms
{
    /// <summary>
    /// 玩家阵营
    /// </summary>
    public enum Faction
    {
        Wei,    // 魏
        Shu,    // 蜀
        Wu,     // 吴
        Qun     // 群
    }

    /// <summary>
    /// 玩家类
    /// </summary>
    public class Player : MonoBehaviour
    {
        [Header("AI设置")]
        public bool isAI = false;
        public AIPlayer aiController;

        [Header("武将数据")]
        public DatabaseModule.GeneralData generalData;  // ⭐ 武将数据引用

        [Header("基础信息")]
        public string playerName = "玩家";
        public string generalName = "武将";    // 武将名称
        public Faction faction;                 // 阵营

        [Header("属性")]
        public int maxHP = 4;                   // 最大体力
        public int currentHP = 4;               // 当前体力
        public int handCardLimit = 0;           // 手牌上限(0表示等于当前体力)

        [Header("手牌和装备")]
        public List<Card> handCards = new List<Card>();         // 手牌
        public List<Card> equipments = new List<Card>();        // 装备区
        public List<Card> judgeCards = new List<Card>();        // 判定区

        [Header("技能")]
        public List<DatabaseModule.ISkill> skills = new List<DatabaseModule.ISkill>();  // ⭐ 技能列表

        [Header("状态")]
        public bool isAlive = true;             // 是否存活
        public bool isDead = false;             // 是否死亡
        public bool isNearDeath = false;        // ⭐ 是否处于濒死状态
        public int attackRange = 1;             // 攻击范围

        [Header("回合状态")]
        public int slashUsedThisTurn = 0;       // 本回合已使用杀的次数
        public int maxSlashPerTurn = 1;         // 每回合最大杀次数（默认1，咆哮技能可修改）
        public int extraAttackRange = 0;        // 额外攻击范围（由武器提供）

        [Header("座位信息")]
        public int seatIndex = -1;              // 座位索引（用于距离计算）

        private void Awake()
        {
            currentHP = maxHP;
        }

        /// <summary>
        /// 从武将数据初始化
        /// </summary>
        public void InitializeFromGeneralData(DatabaseModule.GeneralData data)
        {
            if (data == null)
            {
                Debug.LogWarning($"{playerName} 没有武将数据!");
                return;
            }

            generalData = data;
            generalName = data.generalName;
            faction = data.faction;
            maxHP = data.maxHP;
            currentHP = data.maxHP;
            attackRange = data.attackRange;

            Debug.Log($"{playerName} 使用武将: {generalName} [{faction}] HP:{maxHP}");

            // 初始化技能
            InitializeSkills();
        }

        /// <summary>
        /// 初始化技能
        /// </summary>
        private void InitializeSkills()
        {
            // 清空现有技能
            foreach (var skill in skills)
            {
                skill?.Cleanup();
            }
            skills.Clear();

            if (generalData == null || generalData.skills == null)
            {
                Debug.LogWarning($"{generalName} 没有技能数据!");
                return;
            }

            // 为每个技能数据创建实例
            foreach (var skillData in generalData.skills)
            {
                if (skillData == null)
                {
                    Debug.LogWarning($"{generalName} 有空的技能数据!");
                    continue;
                }

                try
                {
                    var skill = skillData.CreateSkillInstance(this);
                    if (skill != null)
                    {
                        skills.Add(skill);
                        Debug.Log($"✓ {generalName} 学习了技能: {skillData.skillName}");
                    }
                    else
                    {
                        Debug.LogError($"✗ 无法创建技能实例: {skillData.skillName}");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"✗ 初始化技能 {skillData.skillName} 失败: {e.Message}");
                }
            }

            Debug.Log($"{generalName} 共有 {skills.Count} 个技能");
        }

        /// <summary>
        /// 检查技能触发
        /// </summary>
        public void CheckSkillTriggers()
        {
            foreach (var skill in skills)
            {
                if (skill != null && skill.CanTrigger())
                {
                    skill.Trigger();
                }
            }
        }

        /// <summary>
        /// 获取手牌上限
        /// </summary>
        public int GetHandCardLimit()
        {
            return handCardLimit > 0 ? handCardLimit : currentHP;
        }

        /// <summary>
        /// 受到伤害
        /// </summary>
        public void TakeDamage(int damage, Player source = null)
        {
            currentHP -= damage;
            Debug.Log($"{playerName} 受到 {damage} 点伤害,剩余体力:{currentHP}");

            if (currentHP <= 0)
            {
                // ⭐ 进入濒死状态，由BattleManager处理求桃流程
                EnterNearDeath(source);
            }

            NotifyUIUpdate();
        }

        /// <summary>
        /// ⭐ 进入濒死状态
        /// </summary>
        public void EnterNearDeath(Player killer = null)
        {
            if (isNearDeath || isDead) return;

            isNearDeath = true;
            Debug.Log($"{playerName} 进入濒死状态！需要 {1 - currentHP} 张桃救回");

            // 通知BattleManager处理濒死流程
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.StartCoroutine(
                    BattleManager.Instance.ProcessNearDeath(this, killer)
                );
            }
            else
            {
                // 如果没有BattleManager，直接死亡
                ExecuteDeath(killer);
            }
        }

        /// <summary>
        /// ⭐ 濒死被救（使用桃恢复1点体力）
        /// </summary>
        public void SaveFromNearDeath(Player savior)
        {
            if (!isNearDeath) return;

            currentHP = Mathf.Min(currentHP + 1, maxHP);
            Debug.Log($"{playerName} 被 {savior?.playerName ?? "某人"} 用【桃】救回，当前体力:{currentHP}");

            // 如果体力恢复到1或以上，脱离濒死
            if (currentHP > 0)
            {
                isNearDeath = false;
                Debug.Log($"{playerName} 脱离濒死状态");
            }

            NotifyUIUpdate();
        }

        /// <summary>
        /// ⭐ 执行死亡（濒死未被救时调用）
        /// </summary>
        public void ExecuteDeath(Player killer = null)
        {
            isNearDeath = false;
            Die(killer);
        }

        /// <summary>
        /// ⭐ 获取脱离濒死所需的桃数量
        /// </summary>
        public int GetPeachesNeeded()
        {
            return Mathf.Max(0, 1 - currentHP);
        }

        /// <summary>
        /// 回复体力
        /// </summary>
        public void Recover(int amount)
        {
            currentHP = Mathf.Min(currentHP + amount, maxHP);
            Debug.Log($"{playerName} 回复 {amount} 点体力,当前体力:{currentHP}");

            NotifyUIUpdate();
        }

        /// <summary>
        /// 摸牌
        /// </summary>
        public void DrawCard(Card card)
        {
            if (card != null)
            {
                handCards.Add(card);
                Debug.Log($"{playerName} 摸了一张 {card.cardName}");
            }
        }

        /// <summary>
        /// 摸多张牌
        /// </summary>
        public void DrawCards(List<Card> cards)
        {
            foreach (var card in cards)
            {
                DrawCard(card);
            }
        }

        /// <summary>
        /// 打出一张牌
        /// </summary>
        public bool PlayCard(Card card)
        {
            if (handCards.Contains(card))
            {
                handCards.Remove(card);
                Debug.Log($"{playerName} 打出了 {card.cardName}");
                return true;
            }
            return false;
        }

        /// <summary>
        /// 弃牌
        /// </summary>
        public void DiscardCard(Card card)
        {
            if (handCards.Contains(card))
            {
                handCards.Remove(card);
                Debug.Log($"{playerName} 弃置了 {card.cardName}");
            }
        }

        /// <summary>
        /// 死亡
        /// </summary>
        private void Die(Player killer = null)
        {
            isAlive = false;
            isDead = true;
            Debug.Log($"{playerName} 阵亡!");

            // 清空所有区域
            handCards.Clear();
            equipments.Clear();
            judgeCards.Clear();

            // 清理技能
            foreach (var skill in skills)
            {
                skill?.Cleanup();
            }
            skills.Clear();

            // ⭐ 触发死亡事件（用于胜负条件检测）
            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerPlayerDeath(this, killer);
            }

            NotifyUIUpdate();
        }

        /// <summary>
        /// 通知UI更新
        /// </summary>
        private void NotifyUIUpdate()
        {
            // 检查BattleUI是否存在并更新所有玩家信息
            if (UI.BattleUI.Instance != null)
            {
                UI.BattleUI.Instance.UpdateAllPlayerInfo();
            }
        }

        /// <summary>
        /// 计算与目标的距离
        /// 基于座位顺序，只计算存活玩家之间的距离
        /// </summary>
        public int GetDistanceTo(Player target)
        {
            if (target == null || target == this) return 0;
            if (BattleManager.Instance == null) return 1;

            // 获取所有存活玩家
            List<Player> alivePlayers = new List<Player>();
            foreach (var player in BattleManager.Instance.players)
            {
                if (player.isAlive)
                {
                    alivePlayers.Add(player);
                }
            }

            if (alivePlayers.Count <= 1) return 0;

            // 找到自己和目标在存活玩家中的索引
            int myIndex = alivePlayers.IndexOf(this);
            int targetIndex = alivePlayers.IndexOf(target);

            if (myIndex == -1 || targetIndex == -1) return 999; // 找不到

            // 计算双向距离（顺时针和逆时针），取较小值
            int count = alivePlayers.Count;
            int clockwiseDistance = (targetIndex - myIndex + count) % count;
            int counterClockwiseDistance = (myIndex - targetIndex + count) % count;

            return Mathf.Min(clockwiseDistance, counterClockwiseDistance);
        }

        /// <summary>
        /// 获取实际攻击范围（基础范围 + 武器范围）
        /// </summary>
        public int GetTotalAttackRange()
        {
            return attackRange + extraAttackRange;
        }

        /// <summary>
        /// 判断是否在攻击范围内
        /// </summary>
        public bool IsInAttackRange(Player target)
        {
            return GetDistanceTo(target) <= GetTotalAttackRange();
        }

        /// <summary>
        /// 检查是否还能使用杀
        /// </summary>
        public bool CanUseSlash()
        {
            return slashUsedThisTurn < maxSlashPerTurn;
        }

        /// <summary>
        /// 使用杀（增加计数）
        /// </summary>
        public void UseSlash()
        {
            slashUsedThisTurn++;
            Debug.Log($"{playerName} 本回合已使用 {slashUsedThisTurn}/{maxSlashPerTurn} 张杀");
        }

        /// <summary>
        /// 重置回合状态（回合开始时调用）
        /// </summary>
        public void ResetTurnState()
        {
            slashUsedThisTurn = 0;
            Debug.Log($"{playerName} 回合状态已重置");
        }

        /// <summary>
        /// 获取可攻击的目标列表
        /// </summary>
        public List<Player> GetValidSlashTargets()
        {
            List<Player> validTargets = new List<Player>();

            if (BattleManager.Instance == null) return validTargets;

            foreach (var player in BattleManager.Instance.players)
            {
                if (player != this && player.isAlive && IsInAttackRange(player))
                {
                    validTargets.Add(player);
                }
            }

            return validTargets;
        }
    }
}