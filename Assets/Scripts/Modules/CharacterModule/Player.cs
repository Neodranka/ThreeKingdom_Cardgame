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
        public int attackRange = 1;             // 攻击范围

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
                Die(source);
            }

            NotifyUIUpdate();
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
        /// </summary>
        public int GetDistanceTo(Player target)
        {
            // 简化版距离计算,实际应该基于座位顺序
            return 1;
        }

        /// <summary>
        /// 判断是否在攻击范围内
        /// </summary>
        public bool IsInAttackRange(Player target)
        {
            return GetDistanceTo(target) <= attackRange;
        }
    }
}