using UnityEngine;
using System.Collections.Generic;
using ThreeKingdoms.DatabaseModule;

namespace ThreeKingdoms.DatabaseModule.Skills.Story
{
    // ==================== 故事模式专属技能 ====================

    /// <summary>
    /// 主和（张昭）
    /// 回合开始时，可令孙权弃一张牌
    /// </summary>
    public class ZhuheSkill : SkillBase
    {
        protected override void RegisterEvents()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.OnTurnStart += OnTurnStart;
            }
        }

        protected override void UnregisterEvents()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.OnTurnStart -= OnTurnStart;
            }
        }

        protected override bool CheckTriggerCondition()
        {
            return Owner != null && Owner.isAlive;
        }

        public override void Trigger()
        {
            if (!CanTrigger()) return;

            // 找到孙权
            Player sunquan = FindPlayerByName("孙权");
            if (sunquan == null || !sunquan.isAlive || sunquan.handCards.Count == 0)
                return;

            Log($"{Owner.generalName} 发动了【主和】");

            // 让孙权弃一张牌
            if (sunquan.isAI)
            {
                // AI随机弃牌
                if (sunquan.handCards.Count > 0)
                {
                    int index = Random.Range(0, sunquan.handCards.Count);
                    Card card = sunquan.handCards[index];
                    sunquan.DiscardCard(card);
                    Log($"孙权被迫弃置了 {card.cardName}");
                }
            }
            else
            {
                // 人类玩家 - 简化处理，随机弃牌
                if (sunquan.handCards.Count > 0)
                {
                    int index = Random.Range(0, sunquan.handCards.Count);
                    Card card = sunquan.handCards[index];
                    sunquan.DiscardCard(card);
                    Log($"孙权被迫弃置了 {card.cardName}");
                }
            }

            // 触发事件
            EventManager.Instance?.TriggerStoryEvent("skill_zhuhe", Owner.generalName);
        }

        private void OnTurnStart(Player player)
        {
            if (player == Owner)
            {
                Trigger();
            }
        }

        private Player FindPlayerByName(string name)
        {
            if (BattleManager.Instance == null) return null;
            foreach (var p in BattleManager.Instance.players)
            {
                if (p.generalName == name || p.generalName.Contains(name))
                    return p;
            }
            return null;
        }

        public override string GetDescription()
        {
            return "回合开始时，可令孙权弃一张牌。";
        }
    }

    /// <summary>
    /// 缔盟（鲁肃）
    /// 出牌阶段，可令一名角色摸一张牌
    /// </summary>
    public class DimengSkill : SkillBase
    {
        private bool usedThisTurn = false;

        protected override void RegisterEvents()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.OnTurnStart += OnTurnStart;
            }
        }

        protected override void UnregisterEvents()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.OnTurnStart -= OnTurnStart;
            }
        }

        protected override bool CheckTriggerCondition()
        {
            return !usedThisTurn && Owner != null && Owner.isAlive;
        }

        public override void Trigger()
        {
            if (!CanTrigger()) return;

            // AI自动选择目标：优先给血少的友军或主角
            Player target = SelectTarget();
            if (target == null) return;

            Log($"{Owner.generalName} 发动了【缔盟】");
            var cards = DeckManager.Instance?.DrawCards(1);
            if (cards != null && cards.Count > 0)
            {
                target.DrawCards(cards);
            }
            Log($"{target.generalName} 摸了一张牌");

            usedThisTurn = true;

            EventManager.Instance?.TriggerStoryEvent("skill_dimeng", Owner.generalName);
        }

        private Player SelectTarget()
        {
            if (BattleManager.Instance == null) return null;

            Player bestTarget = null;
            int lowestHP = int.MaxValue;

            foreach (var p in BattleManager.Instance.players)
            {
                if (p.isAlive && p.faction == Owner.faction)
                {
                    // 优先给血最少的友军
                    if (p.currentHP < lowestHP)
                    {
                        lowestHP = p.currentHP;
                        bestTarget = p;
                    }
                }
            }

            return bestTarget;
        }

        private void OnTurnStart(Player player)
        {
            if (player == Owner)
            {
                usedThisTurn = false;
            }
        }

        public override Player[] GetValidTargets()
        {
            return GetAlivePlayers();
        }

        public override string GetDescription()
        {
            return "出牌阶段限一次，你可以令一名角色摸一张牌。";
        }
    }

    /// <summary>
    /// 老当（程普）
    /// 当你受到伤害后，可摸一张牌
    /// </summary>
    public class LaodangSkill : SkillBase
    {
        protected override void RegisterEvents()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.OnPlayerDamaged += OnPlayerDamaged;
            }
        }

        protected override void UnregisterEvents()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.OnPlayerDamaged -= OnPlayerDamaged;
            }
        }

        protected override bool CheckTriggerCondition()
        {
            return Owner != null && Owner.isAlive;
        }

        public override void Trigger()
        {
            if (!CanTrigger()) return;

            Log($"{Owner.generalName} 发动了【老当】");
            var cards = DeckManager.Instance?.DrawCards(1);
            if (cards != null && cards.Count > 0)
            {
                Owner.DrawCards(cards);
            }
            Log($"摸了一张牌");
        }

        private void OnPlayerDamaged(Player victim, Player source, int damage, Card card)
        {
            if (victim == Owner && Owner.isAlive)
            {
                Trigger();
            }
        }

        public override string GetDescription()
        {
            return "当你受到伤害后，你可以摸一张牌。";
        }
    }

    /// <summary>
    /// 胆裂（夏侯杰）- 锁定技
    /// 当体力首次降至2或以下时，获得"胆裂"状态：
    /// 不能使用或打出【闪】，造成的伤害-1
    /// </summary>
    public class DanlieSkill : SkillBase
    {
        public bool isDanlieTriggered = false;

        protected override void RegisterEvents()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.OnPlayerDamaged += OnPlayerDamaged;
            }
        }

        protected override void UnregisterEvents()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.OnPlayerDamaged -= OnPlayerDamaged;
            }
        }

        protected override bool CheckTriggerCondition()
        {
            return !isDanlieTriggered && Owner != null && Owner.isAlive && Owner.currentHP <= 2;
        }

        public override void Trigger()
        {
            if (isDanlieTriggered) return;

            Log($"{Owner.generalName} 触发了【胆裂】！");
            isDanlieTriggered = true;

            // 胆裂状态通过isDanlieTriggered标记来跟踪
            // 实际效果需要在战斗系统中检查IsDanlieActive()

            EventManager.Instance?.TriggerStoryEvent("skill_danlie", Owner.generalName);
        }

        private void OnPlayerDamaged(Player victim, Player source, int damage, Card card)
        {
            if (victim == Owner && !isDanlieTriggered && Owner.currentHP <= 2)
            {
                Trigger();
            }
        }

        /// <summary>
        /// 检查是否处于胆裂状态（供战斗系统调用）
        /// </summary>
        public bool IsDanlieActive()
        {
            return isDanlieTriggered;
        }

        public override string GetDescription()
        {
            return "锁定技，当你的体力值首次降至2或以下时，你不能使用或打出【闪】，你造成的伤害-1。";
        }
    }

    /// <summary>
    /// 苦肉诈降（黄盖故事模式专属）
    /// 出牌阶段，你可以失去1点体力，然后摸2张牌并获得1个"诈降"标记
    /// </summary>
    public class KurouZhaxiangSkill : SkillBase
    {
        public int surrenderMarks = 0;

        protected override bool CheckTriggerCondition()
        {
            return Owner != null && Owner.isAlive && Owner.currentHP > 1;
        }

        public override void Trigger()
        {
            if (!CanTrigger()) return;

            Log($"{Owner.generalName} 发动了【苦肉诈降】");

            // 失去1点体力（通过TakeDamage实现，无伤害来源）
            Owner.TakeDamage(1, null);

            // 摸2张牌
            var cards = DeckManager.Instance?.DrawCards(2);
            if (cards != null && cards.Count > 0)
            {
                Owner.DrawCards(cards);
            }

            // 获得诈降标记
            surrenderMarks++;
            Log($"获得诈降标记，当前: {surrenderMarks}");

            EventManager.Instance?.TriggerStoryEvent("skill_kurou_zhaxiang", Owner.generalName);
            EventManager.Instance?.TriggerStoryEvent("marker_zhaxiang", surrenderMarks.ToString());
        }

        /// <summary>
        /// 获取当前诈降标记数
        /// </summary>
        public int GetSurrenderMarks()
        {
            return surrenderMarks;
        }

        public override string GetDescription()
        {
            return "出牌阶段，你可以失去1点体力，然后摸两张牌并获得1个[诈降]标记。";
        }
    }

    /// <summary>
    /// 盗书（蒋干）
    /// 回合结束时，可查看一名角色的手牌
    /// </summary>
    public class DaoshuSkill : SkillBase
    {
        public int viewCount = 0; // 连续查看黄盖手牌的次数

        protected override void RegisterEvents()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.OnTurnEnd += OnTurnEnd;
            }
        }

        protected override void UnregisterEvents()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.OnTurnEnd -= OnTurnEnd;
            }
        }

        protected override bool CheckTriggerCondition()
        {
            return Owner != null && Owner.isAlive;
        }

        public override void Trigger()
        {
            if (!CanTrigger()) return;

            // AI随机选择目标查看
            Player target = SelectTarget();
            if (target == null) return;

            Log($"{Owner.generalName} 发动了【盗书】，查看了 {target.generalName} 的手牌");

            // 记录是否查看了黄盖
            if (target.generalName.Contains("黄盖"))
            {
                viewCount++;
                EventManager.Instance?.TriggerStoryEvent("hand_viewed", "黄盖");
            }
            else
            {
                viewCount = 0; // 重置连续计数
            }

            EventManager.Instance?.TriggerStoryEvent("skill_daoshu", Owner.generalName);
        }

        private Player SelectTarget()
        {
            var enemies = GetAliveEnemies();
            if (enemies.Length == 0) return null;

            // 50%概率选择黄盖（如果存在）
            foreach (var e in enemies)
            {
                if (e.generalName.Contains("黄盖") && Random.value < 0.5f)
                    return e;
            }

            return enemies[Random.Range(0, enemies.Length)];
        }

        private void OnTurnEnd(Player player)
        {
            if (player == Owner)
            {
                Trigger();
            }
        }

        public override string GetDescription()
        {
            return "回合结束时，你可以查看一名角色的手牌。";
        }
    }

    /// <summary>
    /// 龙胆（赵云）
    /// 可将【杀】当【闪】使用或打出，或将【闪】当【杀】使用或打出
    /// </summary>
    public class LongdanSkill : SkillBase
    {
        protected override bool CheckTriggerCondition()
        {
            return Owner != null && Owner.isAlive;
        }

        public override void Trigger()
        {
            // 龙胆是转换技能，需要在出牌逻辑中处理
            Log($"{Owner.generalName} 可以使用【龙胆】");
        }

        /// <summary>
        /// 检查是否可以将杀当闪
        /// </summary>
        public bool CanUseKillAsDodge()
        {
            if (Owner == null) return false;
            foreach (var card in Owner.handCards)
            {
                if (CardNameHelper.IsSlash(card))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 检查是否可以将闪当杀
        /// </summary>
        public bool CanUseDodgeAsKill()
        {
            if (Owner == null) return false;
            foreach (var card in Owner.handCards)
            {
                if (CardNameHelper.IsDodge(card))
                    return true;
            }
            return false;
        }

        public override string GetDescription()
        {
            return "你可以将【杀】当【闪】使用或打出，或将【闪】当【杀】使用或打出。";
        }
    }

    /// <summary>
    /// 观星（诸葛亮）
    /// 回合开始时，可观看牌堆顶的5张牌，并以任意顺序置于牌堆顶或牌堆底
    /// </summary>
    public class GuanxingSkill : SkillBase
    {
        protected override void RegisterEvents()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.OnTurnStart += OnTurnStart;
            }
        }

        protected override void UnregisterEvents()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.OnTurnStart -= OnTurnStart;
            }
        }

        protected override bool CheckTriggerCondition()
        {
            return Owner != null && Owner.isAlive;
        }

        public override void Trigger()
        {
            if (!CanTrigger()) return;

            Log($"{Owner.generalName} 发动了【观星】");

            // 简化版：AI自动将好牌放顶部
            if (DeckManager.Instance != null)
            {
                // 观看5张牌（简化处理：只是记录日志）
                // DeckManager没有PeekTopCards，简化实现
                Log($"观看了牌堆顶的牌");
            }

            EventManager.Instance?.TriggerStoryEvent("skill_guanxing", Owner.generalName);
        }

        private void OnTurnStart(Player player)
        {
            if (player == Owner)
            {
                Trigger();
            }
        }

        public override string GetDescription()
        {
            return "回合开始时，你可以观看牌堆顶的五张牌，然后以任意顺序置于牌堆顶或牌堆底。";
        }
    }

    /// <summary>
    /// 空城（诸葛亮）- 锁定技
    /// 当你没有手牌时，你不能成为【杀】或【决斗】的目标
    /// </summary>
    public class KongchengSkill : SkillBase
    {
        protected override bool CheckTriggerCondition()
        {
            return Owner != null && Owner.isAlive && Owner.handCards.Count == 0;
        }

        public override void Trigger()
        {
            // 空城是锁定技，被动生效
        }

        /// <summary>
        /// 检查是否处于空城状态
        /// </summary>
        public bool IsKongchengActive()
        {
            return Owner != null && Owner.isAlive && Owner.handCards.Count == 0;
        }

        public override string GetDescription()
        {
            return "锁定技，当你没有手牌时，你不能成为【杀】或【决斗】的目标。";
        }
    }

    /// <summary>
    /// 英姿（周瑜）
    /// 摸牌阶段，你可以多摸一张牌
    /// </summary>
    public class YingziSkill : SkillBase
    {
        protected override void RegisterEvents()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.OnDrawPhase += OnDrawPhase;
            }
        }

        protected override void UnregisterEvents()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.OnDrawPhase -= OnDrawPhase;
            }
        }

        protected override bool CheckTriggerCondition()
        {
            return Owner != null && Owner.isAlive;
        }

        public override void Trigger()
        {
            if (!CanTrigger()) return;

            Log($"{Owner.generalName} 发动了【英姿】");
            var cards = DeckManager.Instance?.DrawCards(1);
            if (cards != null && cards.Count > 0)
            {
                Owner.DrawCards(cards);
            }
            Log($"多摸了一张牌");
        }

        private void OnDrawPhase(Player player)
        {
            if (player == Owner)
            {
                Trigger();
            }
        }

        public override string GetDescription()
        {
            return "摸牌阶段，你可以多摸一张牌。";
        }
    }

    /// <summary>
    /// 反间（周瑜）
    /// 出牌阶段限一次，你可以令一名角色猜测一种花色，然后展示一张手牌
    /// 若猜错，该角色受到1点伤害
    /// </summary>
    public class FanjianSkill : SkillBase
    {
        private bool usedThisTurn = false;

        protected override void RegisterEvents()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.OnTurnStart += OnTurnStart;
            }
        }

        protected override void UnregisterEvents()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.OnTurnStart -= OnTurnStart;
            }
        }

        protected override bool CheckTriggerCondition()
        {
            return !usedThisTurn && Owner != null && Owner.isAlive && Owner.handCards.Count > 0;
        }

        public override void Trigger()
        {
            if (!CanTrigger()) return;

            var targets = GetAliveEnemies();
            if (targets.Length == 0) return;

            Player target = targets[Random.Range(0, targets.Length)];

            Log($"{Owner.generalName} 对 {target.generalName} 发动了【反间】");

            // AI展示一张牌
            Card showCard = Owner.handCards[Random.Range(0, Owner.handCards.Count)];
            CardSuit actualSuit = showCard.suit;

            // 目标猜测（AI随机猜）
            CardSuit guess = (CardSuit)Random.Range(0, 4);

            Log($"{target.generalName} 猜测: {guess}，实际: {actualSuit}");

            if (guess != actualSuit)
            {
                target.TakeDamage(1, Owner);
                Log($"{target.generalName} 猜错了，受到1点伤害");
            }
            else
            {
                Log($"{target.generalName} 猜对了！");
            }

            usedThisTurn = true;

            EventManager.Instance?.TriggerStoryEvent("skill_fanjian", Owner.generalName);
        }

        private void OnTurnStart(Player player)
        {
            if (player == Owner)
            {
                usedThisTurn = false;
            }
        }

        public override Player[] GetValidTargets()
        {
            return GetAliveEnemies();
        }

        public override string GetDescription()
        {
            return "出牌阶段限一次，你可以令一名角色猜测一种花色，然后展示一张手牌，若猜错则该角色受到1点伤害。";
        }
    }

    /// <summary>
    /// 克己（吕蒙）
    /// 若你于出牌阶段未使用【杀】，你可以跳过弃牌阶段
    /// </summary>
    public class KejiSkill : SkillBase
    {
        public bool usedKillThisTurn = false;

        protected override void RegisterEvents()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.OnTurnStart += OnTurnStart;
                EventManager.Instance.OnCardPlayed += OnCardPlayed;
            }
        }

        protected override void UnregisterEvents()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.OnTurnStart -= OnTurnStart;
                EventManager.Instance.OnCardPlayed -= OnCardPlayed;
            }
        }

        protected override bool CheckTriggerCondition()
        {
            return Owner != null && Owner.isAlive && !usedKillThisTurn;
        }

        public override void Trigger()
        {
            if (!CanTrigger()) return;

            Log($"{Owner.generalName} 发动了【克己】，跳过弃牌阶段");
            // 实际跳过弃牌阶段需要在回合系统中处理
        }

        /// <summary>
        /// 检查是否可以跳过弃牌阶段
        /// </summary>
        public bool CanSkipDiscardPhase()
        {
            return !usedKillThisTurn;
        }

        private void OnTurnStart(Player player)
        {
            if (player == Owner)
            {
                usedKillThisTurn = false;
            }
        }

        private void OnCardPlayed(Player player, Card card)
        {
            if (player == Owner && CardNameHelper.IsSlash(card))
            {
                usedKillThisTurn = true;
            }
        }

        public override string GetDescription()
        {
            return "若你于出牌阶段未使用【杀】，你可以跳过弃牌阶段。";
        }
    }

    /// <summary>
    /// 刚烈（夏侯惇）
    /// 当你受到伤害后，你可以进行判定，若结果为红色，则伤害来源需选择：弃置两张手牌或受到1点伤害
    /// </summary>
    public class GanglieSkill : SkillBase
    {
        private Player damageSource;

        protected override void RegisterEvents()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.OnPlayerDamaged += OnPlayerDamaged;
            }
        }

        protected override void UnregisterEvents()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.OnPlayerDamaged -= OnPlayerDamaged;
            }
        }

        protected override bool CheckTriggerCondition()
        {
            return Owner != null && Owner.isAlive && damageSource != null;
        }

        public override void Trigger()
        {
            if (!CanTrigger()) return;

            Log($"{Owner.generalName} 发动了【刚烈】");

            // 进行判定
            Card judgeCard = DeckManager.Instance?.DrawCard();
            if (judgeCard == null) return;

            bool isRed = judgeCard.IsRed();
            Log($"判定结果: {judgeCard.cardName} ({(isRed ? "红色" : "黑色")})");

            if (isRed && damageSource != null && damageSource.isAlive)
            {
                // AI选择：如果手牌少于2张就受伤，否则弃牌
                if (damageSource.handCards.Count >= 2)
                {
                    // 弃两张牌
                    for (int i = 0; i < 2 && damageSource.handCards.Count > 0; i++)
                    {
                        Card card = damageSource.handCards[0];
                        damageSource.DiscardCard(card);
                    }
                    Log($"{damageSource.generalName} 选择弃置两张牌");
                }
                else
                {
                    damageSource.TakeDamage(1, Owner);
                    Log($"{damageSource.generalName} 选择受到1点伤害");
                }
            }

            damageSource = null;
        }

        private void OnPlayerDamaged(Player victim, Player source, int damage, Card card)
        {
            if (victim == Owner && source != null)
            {
                damageSource = source;
                Trigger();
            }
        }

        public override string GetDescription()
        {
            return "当你受到伤害后，你可以判定，若为红色，伤害来源须选择弃两张手牌或受到1点伤害。";
        }
    }

    /// <summary>
    /// 神速（夏侯渊）
    /// 你可以跳过判定阶段和/或摸牌阶段，每跳过一个阶段，视为对一名角色使用【杀】
    /// </summary>
    public class ShenshuSkill : SkillBase
    {
        public bool skipJudgePhase = false;
        public bool skipDrawPhase = false;

        protected override void RegisterEvents()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.OnTurnStart += OnTurnStart;
            }
        }

        protected override void UnregisterEvents()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.OnTurnStart -= OnTurnStart;
            }
        }

        protected override bool CheckTriggerCondition()
        {
            return Owner != null && Owner.isAlive;
        }

        public override void Trigger()
        {
            if (!CanTrigger()) return;

            var enemies = GetAliveEnemies();
            if (enemies.Length == 0) return;

            // AI策略：如果有多个敌人且血量健康，使用神速
            if (Owner.currentHP >= 3 && enemies.Length > 0)
            {
                // 跳过判定阶段，对一名敌人使用杀
                skipJudgePhase = true;
                Player target = enemies[Random.Range(0, enemies.Length)];
                Log($"{Owner.generalName} 发动【神速】跳过判定阶段，视为对 {target.generalName} 使用【杀】");

                // 模拟使用杀（简化：检查目标是否有闪）
                bool hasDodge = false;
                foreach (var card in target.handCards)
                {
                    if (CardNameHelper.IsDodge(card))
                    {
                        hasDodge = true;
                        target.DiscardCard(card);
                        Log($"{target.generalName} 打出了【闪】");
                        break;
                    }
                }

                if (!hasDodge)
                {
                    target.TakeDamage(1, Owner);
                }
            }
        }

        private void OnTurnStart(Player player)
        {
            if (player == Owner)
            {
                skipJudgePhase = false;
                skipDrawPhase = false;
                Trigger();
            }
        }

        public override Player[] GetValidTargets()
        {
            return GetAliveEnemies();
        }

        public override string GetDescription()
        {
            return "你可以跳过判定阶段和/或摸牌阶段，每跳过一个阶段，视为对一名角色使用【杀】。";
        }
    }

    /// <summary>
    /// 突袭（张辽）
    /// 摸牌阶段，你可以改为获取至多两名角色各一张手牌
    /// </summary>
    public class TuxiSkill : SkillBase
    {
        public bool useTuxiThisTurn = false;

        protected override void RegisterEvents()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.OnDrawPhase += OnDrawPhase;
                EventManager.Instance.OnTurnStart += OnTurnStart;
            }
        }

        protected override void UnregisterEvents()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.OnDrawPhase -= OnDrawPhase;
                EventManager.Instance.OnTurnStart -= OnTurnStart;
            }
        }

        protected override bool CheckTriggerCondition()
        {
            return Owner != null && Owner.isAlive;
        }

        public override void Trigger()
        {
            if (!CanTrigger()) return;

            var enemies = GetAliveEnemies();
            var targets = new List<Player>();

            // 选择至多两名有手牌的敌人
            foreach (var e in enemies)
            {
                if (e.handCards.Count > 0 && targets.Count < 2)
                {
                    targets.Add(e);
                }
            }

            if (targets.Count == 0)
            {
                useTuxiThisTurn = false;
                return;
            }

            Log($"{Owner.generalName} 发动了【突袭】");
            useTuxiThisTurn = true;

            foreach (var target in targets)
            {
                if (target.handCards.Count > 0)
                {
                    int index = Random.Range(0, target.handCards.Count);
                    Card card = target.handCards[index];
                    target.handCards.RemoveAt(index);
                    Owner.handCards.Add(card);
                    Log($"从 {target.generalName} 获取了一张手牌");
                }
            }

            EventManager.Instance?.TriggerStoryEvent("skill_tuxi", Owner.generalName);
        }

        private void OnDrawPhase(Player player)
        {
            if (player == Owner)
            {
                // 有50%概率使用突袭（AI策略）
                if (Random.value < 0.5f)
                {
                    Trigger();
                }
            }
        }

        private void OnTurnStart(Player player)
        {
            if (player == Owner)
            {
                useTuxiThisTurn = false;
            }
        }

        public override Player[] GetValidTargets()
        {
            return GetAliveEnemies();
        }

        public override string GetDescription()
        {
            return "摸牌阶段，你可以改为获取至多两名角色各一张手牌。";
        }
    }
}
