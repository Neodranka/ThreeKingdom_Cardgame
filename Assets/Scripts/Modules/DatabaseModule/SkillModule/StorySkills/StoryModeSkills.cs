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

            // ⭐ 赤壁之战：中计规则 - 蒋干查看周瑜手牌时检查是否有黑牌
            string targetId = target.generalName?.ToLower().Replace("_story", "") ?? "";
            if (targetId.Contains("zhouyu") || targetId.Contains("周瑜"))
            {
                if (ThreeKingdoms.Story.StoryBattleManager.Instance != null)
                {
                    bool isFakeInfo = ThreeKingdoms.Story.StoryBattleManager.Instance.CheckTrickRule(target);
                    if (isFakeInfo)
                    {
                        Log($"{Owner.generalName} 获得了假情报！");
                    }
                    else
                    {
                        Log($"{Owner.generalName} 获得了真实情报！");
                    }
                    // 触发盗书事件（用于显示对白）
                    EventManager.Instance?.TriggerStoryEvent("hand_viewed_zhouyu", isFakeInfo ? "fake" : "real");
                }
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

                // ⭐ 赤壁之战：伪造书信规则 - 反间成功获得标记
                if (ThreeKingdoms.Story.StoryBattleManager.Instance != null)
                {
                    ThreeKingdoms.Story.StoryBattleManager.Instance.OnFanjianSuccess(Owner);
                }
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

    // ==================== 官渡之战/讨董之战 专属技能 ====================

    /// <summary>
    /// 动摇（军心动摇）- 锁定技
    /// 回合开始时，令所有盟友随机弃置一张手牌
    /// </summary>
    public class DongyaoSkill : SkillBase
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

            Log($"{Owner.generalName} 的【动摇】发动");

            // 让所有友方（包括自己除外）随机弃牌
            var allies = GetAliveAllies();
            foreach (var ally in allies)
            {
                if (ally != Owner && ally.handCards.Count > 0)
                {
                    int index = Random.Range(0, ally.handCards.Count);
                    Card card = ally.handCards[index];
                    ally.DiscardCard(card);
                    Log($"{ally.generalName} 被迫弃置了 {card.cardName}");
                }
            }
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
            return "锁定技，回合开始时，令所有盟友随机弃置一张手牌。";
        }
    }

    /// <summary>
    /// 消耗（粮草隐患）- 锁定技
    /// 回合结束时，所有盟友手牌上限-1（本回合）
    /// </summary>
    public class XiaohaoSkill : SkillBase
    {
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

            Log($"{Owner.generalName} 的【消耗】发动");

            // 令所有盟友多弃一张牌
            var allies = GetAliveAllies();
            foreach (var ally in allies)
            {
                if (ally != Owner && ally.handCards.Count > ally.currentHP)
                {
                    int index = Random.Range(0, ally.handCards.Count);
                    Card card = ally.handCards[index];
                    ally.DiscardCard(card);
                    Log($"{ally.generalName} 因粮草消耗额外弃置了 {card.cardName}");
                }
            }
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
            return "锁定技，回合结束时，所有盟友须额外弃一张牌。";
        }
    }

    /// <summary>
    /// 献策（许攸）
    /// 出牌阶段限一次，可弃一张牌令一名角色摸两张牌
    /// </summary>
    public class XianceSkill : SkillBase
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

            // 选择目标：优先给友方
            Player target = SelectTarget();
            if (target == null) return;

            // 弃一张牌
            if (Owner.handCards.Count > 0)
            {
                int index = Random.Range(0, Owner.handCards.Count);
                Card discard = Owner.handCards[index];
                Owner.DiscardCard(discard);
            }

            Log($"{Owner.generalName} 发动了【献策】");

            // 目标摸两张牌
            var cards = DeckManager.Instance?.DrawCards(2);
            if (cards != null && cards.Count > 0)
            {
                target.DrawCards(cards);
            }
            Log($"{target.generalName} 摸了两张牌");

            usedThisTurn = true;
            EventManager.Instance?.TriggerStoryEvent("skill_xiancelue", Owner.generalName);
        }

        private Player SelectTarget()
        {
            var allies = GetAliveAllies();
            if (allies.Length > 0)
            {
                // 优先给血最少的友军
                Player best = allies[0];
                foreach (var ally in allies)
                {
                    if (ally.currentHP < best.currentHP)
                        best = ally;
                }
                return best;
            }
            return Owner;
        }

        private void OnTurnStart(Player player)
        {
            if (player == Owner)
            {
                usedThisTurn = false;
            }
        }

        public override string GetDescription()
        {
            return "出牌阶段限一次，你可以弃一张牌，令一名角色摸两张牌。";
        }
    }

    /// <summary>
    /// 援护（曹洪）
    /// 当一名友方角色受到伤害时，你可以弃一张牌令伤害-1
    /// </summary>
    public class YuanhuSkill : SkillBase
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
            return Owner != null && Owner.isAlive && Owner.handCards.Count > 0;
        }

        public override void Trigger()
        {
            // 触发在OnPlayerDamaged中处理
        }

        private void OnPlayerDamaged(Player victim, Player source, int damage, Card card)
        {
            if (!CanTrigger()) return;

            // 检查受伤者是否是友军
            if (victim != Owner && victim.faction == Owner.faction && victim.isAlive)
            {
                // 50%概率发动
                if (Random.value < 0.5f && Owner.handCards.Count > 0)
                {
                    int index = Random.Range(0, Owner.handCards.Count);
                    Card discard = Owner.handCards[index];
                    Owner.DiscardCard(discard);

                    Log($"{Owner.generalName} 发动了【援护】，弃置 {discard.cardName}");

                    // 回复1点体力代替减伤（简化处理）
                    if (victim.isAlive)
                    {
                        victim.Recover(1);
                        Log($"{victim.generalName} 减免了1点伤害");
                    }
                }
            }
        }

        public override string GetDescription()
        {
            return "当一名友方角色受到伤害时，你可以弃一张牌令伤害-1。";
        }
    }

    /// <summary>
    /// 嗜酒（淳于琼）- 锁定技
    /// 你的手牌上限-1，当你受到火焰伤害时，伤害+1
    /// </summary>
    public class ShijiuSkill : SkillBase
    {
        protected override void RegisterEvents()
        {
            // 手牌上限减少在初始化时处理
            if (Owner != null)
            {
                Owner.handCardLimit = Mathf.Max(0, Owner.currentHP - 1);
            }
        }

        protected override void UnregisterEvents()
        {
            // 恢复手牌上限（如果需要）
        }

        protected override bool CheckTriggerCondition()
        {
            return Owner != null && Owner.isAlive;
        }

        public override void Trigger()
        {
            // 嗜酒是锁定技，效果在其他地方处理
        }

        public override string GetDescription()
        {
            return "锁定技，你的手牌上限-1。当你受到火焰伤害时，伤害+1。";
        }
    }

    /// <summary>
    /// 守粮（袁军守将）
    /// 你不能成为【杀】以外的牌的目标
    /// </summary>
    public class ShouliangSkill : SkillBase
    {
        protected override bool CheckTriggerCondition()
        {
            return Owner != null && Owner.isAlive;
        }

        public override void Trigger()
        {
            // 守粮是锁定技，效果在目标选择时检查
        }

        /// <summary>
        /// 检查是否可以成为某牌的目标
        /// </summary>
        public bool CanBeTargetOf(Card card)
        {
            if (card == null) return true;
            return CardNameHelper.IsSlash(card);
        }

        public override string GetDescription()
        {
            return "锁定技，你不能成为【杀】以外的牌的目标。";
        }
    }

    /// <summary>
    /// 消极（诸侯观望）- 锁定技
    /// 你不能主动使用牌
    /// </summary>
    public class XiaojiPassiveSkill : SkillBase
    {
        protected override bool CheckTriggerCondition()
        {
            return Owner != null && Owner.isAlive;
        }

        public override void Trigger()
        {
            // 消极是锁定技，效果是不能主动出牌
        }

        /// <summary>
        /// 检查是否可以使用牌
        /// </summary>
        public bool CanPlayCard()
        {
            return false;
        }

        public override string GetDescription()
        {
            return "锁定技，你不能主动使用牌。";
        }
    }

    /// <summary>
    /// 伏击（徐荣）
    /// 当你对一名角色造成伤害时，若其未受伤，伤害+1
    /// </summary>
    public class FujiSkill : SkillBase
    {
        protected override bool CheckTriggerCondition()
        {
            return Owner != null && Owner.isAlive;
        }

        public override void Trigger()
        {
            // 伏击效果在伤害计算时处理
        }

        /// <summary>
        /// 计算伏击伤害加成
        /// </summary>
        public int GetDamageBonus(Player target)
        {
            if (target != null && target.currentHP == target.maxHP)
            {
                Log($"{Owner.generalName} 的【伏击】发动，对未受伤目标伤害+1");
                return 1;
            }
            return 0;
        }

        public override string GetDescription()
        {
            return "当你对一名未受伤的角色造成伤害时，伤害+1。";
        }
    }

    /// <summary>
    /// 冲阵（西凉骑兵）
    /// 当你对一名已受伤的角色造成伤害时，伤害+1
    /// </summary>
    public class ChongzhenSkill : SkillBase
    {
        protected override bool CheckTriggerCondition()
        {
            return Owner != null && Owner.isAlive;
        }

        public override void Trigger()
        {
            // 冲阵效果在伤害计算时处理
        }

        /// <summary>
        /// 计算冲阵伤害加成
        /// </summary>
        public int GetDamageBonus(Player target)
        {
            if (target != null && target.currentHP < target.maxHP)
            {
                Log($"{Owner.generalName} 的【冲阵】发动，对已受伤目标伤害+1");
                return 1;
            }
            return 0;
        }

        public override string GetDescription()
        {
            return "当你对一名已受伤的角色造成伤害时，伤害+1。";
        }
    }

    /// <summary>
    /// 耀武（华雄）
    /// 出牌阶段，你使用【杀】无次数限制
    /// 当你使用【杀】造成伤害后，可摸一张牌
    /// </summary>
    public class YaowuSkill : SkillBase
    {
        protected override void RegisterEvents()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.OnTurnStart += OnTurnStart;
                EventManager.Instance.OnPlayerDamaged += OnPlayerDamaged;
            }
        }

        protected override void UnregisterEvents()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.OnTurnStart -= OnTurnStart;
                EventManager.Instance.OnPlayerDamaged -= OnPlayerDamaged;
            }
        }

        protected override bool CheckTriggerCondition()
        {
            return Owner != null && Owner.isAlive;
        }

        public override void Trigger()
        {
            // 设置无限出杀
            if (Owner != null)
            {
                Owner.maxSlashPerTurn = 999;
            }
        }

        private void OnTurnStart(Player player)
        {
            if (player == Owner)
            {
                Trigger();
            }
        }

        private void OnPlayerDamaged(Player victim, Player source, int damage, Card card)
        {
            if (source == Owner && card != null && CardNameHelper.IsSlash(card))
            {
                // 造成伤害后摸牌
                var cards = DeckManager.Instance?.DrawCards(1);
                if (cards != null && cards.Count > 0)
                {
                    Owner.DrawCards(cards);
                    Log($"{Owner.generalName} 【耀武】发动，摸了一张牌");
                }
            }
        }

        public override string GetDescription()
        {
            return "你使用【杀】无次数限制。当你使用【杀】造成伤害后，可摸一张牌。";
        }
    }

    /// <summary>
    /// 怯战（胡轸）- 锁定技
    /// 当你的体力值小于等于2时，你不能使用【杀】
    /// </summary>
    public class QiezhanSkill : SkillBase
    {
        protected override bool CheckTriggerCondition()
        {
            return Owner != null && Owner.isAlive;
        }

        public override void Trigger()
        {
            // 怯战是锁定技，效果在出牌时检查
        }

        /// <summary>
        /// 检查是否可以使用杀
        /// </summary>
        public bool CanUseSlash()
        {
            if (Owner != null && Owner.currentHP <= 2)
            {
                return false;
            }
            return true;
        }

        public override string GetDescription()
        {
            return "锁定技，当你的体力值小于等于2时，你不能使用【杀】。";
        }
    }

    /// <summary>
    /// 分裂（联军内斗）- 锁定技
    /// 回合开始时，所有角色（包括自己）失去1点体力
    /// </summary>
    public class FenlieSkill : SkillBase
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

            Log($"{Owner.generalName} 的【分裂】发动");

            // 所有角色失去1点体力
            var allPlayers = GetAlivePlayers();
            foreach (var player in allPlayers)
            {
                if (player.isAlive)
                {
                    player.TakeDamage(1, null);
                    Log($"{player.generalName} 因联军内斗失去1点体力");
                }
            }
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
            return "锁定技，回合开始时，所有角色失去1点体力。";
        }
    }

    /// <summary>
    /// 苦肉（黄盖 - 普通版）
    /// 出牌阶段，你可以失去1点体力，然后摸2张牌
    /// </summary>
    public class KurouSkill : SkillBase
    {
        protected override bool CheckTriggerCondition()
        {
            return Owner != null && Owner.isAlive && Owner.currentHP > 1;
        }

        public override void Trigger()
        {
            if (!CanTrigger()) return;

            Log($"{Owner.generalName} 发动了【苦肉】");

            // 失去1点体力
            Owner.TakeDamage(1, null);

            // 摸2张牌
            var cards = DeckManager.Instance?.DrawCards(2);
            if (cards != null && cards.Count > 0)
            {
                Owner.DrawCards(cards);
            }
            Log($"摸了2张牌");

            EventManager.Instance?.TriggerStoryEvent("skill_kurou", Owner.generalName);
        }

        public override string GetDescription()
        {
            return "出牌阶段，你可以失去1点体力，然后摸两张牌。";
        }
    }

    // ==================== 赤壁之战v2 新增技能 ====================

    /// <summary>
    /// 诘难（虞翻）
    /// 回合开始时，可令一名敌方角色弃置一张手牌
    /// 用于舌战群儒关卡
    /// </summary>
    public class JienanSkill : SkillBase
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

            // 选择一名有手牌的敌方角色
            var enemies = GetAliveEnemies();
            Player target = null;
            foreach (var enemy in enemies)
            {
                if (enemy.handCards.Count > 0)
                {
                    target = enemy;
                    break;
                }
            }

            if (target == null) return;

            Log($"{Owner.generalName} 发动了【诘难】");

            // 令目标弃置一张手牌
            if (target.handCards.Count > 0)
            {
                int index = Random.Range(0, target.handCards.Count);
                Card card = target.handCards[index];
                target.DiscardCard(card);
                Log($"{target.generalName} 被迫弃置了 {card.cardName}");
            }

            EventManager.Instance?.TriggerStoryEvent("skill_jienan", Owner.generalName);
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
            return "回合开始时，可令一名敌方角色弃置一张手牌。";
        }
    }

    /// <summary>
    /// 水战（蔡瑁）
    /// 锁定技，你使用【杀】造成的伤害+1，
    /// 你的手牌上限+1
    /// 用于蒋干盗书关卡
    /// </summary>
    public class ShuizhanSkill : SkillBase
    {
        protected override void RegisterEvents()
        {
            // 手牌上限增加在初始化时处理
            if (Owner != null)
            {
                Owner.handCardLimit = Owner.currentHP + 1;
            }
        }

        protected override void UnregisterEvents()
        {
            // 清理
        }

        protected override bool CheckTriggerCondition()
        {
            return Owner != null && Owner.isAlive;
        }

        public override void Trigger()
        {
            // 锁定技，被动生效
        }

        /// <summary>
        /// 获取杀的伤害加成
        /// </summary>
        public int GetSlashDamageBonus()
        {
            return 1;
        }

        public override string GetDescription()
        {
            return "锁定技，你使用【杀】造成的伤害+1，手牌上限+1。";
        }
    }

    /// <summary>
    /// 北人（曹军水兵）- 锁定技
    /// 你的手牌上限-1，你使用【杀】时需弃置一张牌
    /// 代表北方士兵不习水战的弱点
    /// 用于江上对峙关卡
    /// </summary>
    public class BeirenSkill : SkillBase
    {
        protected override void RegisterEvents()
        {
            // 手牌上限减少
            if (Owner != null)
            {
                Owner.handCardLimit = Mathf.Max(1, Owner.currentHP - 1);
            }

            if (EventManager.Instance != null)
            {
                EventManager.Instance.OnCardPlayed += OnCardPlayed;
            }
        }

        protected override void UnregisterEvents()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.OnCardPlayed -= OnCardPlayed;
            }
        }

        protected override bool CheckTriggerCondition()
        {
            return Owner != null && Owner.isAlive;
        }

        public override void Trigger()
        {
            // 锁定技，被动生效
        }

        private void OnCardPlayed(Player player, Card card)
        {
            if (player == Owner && CardNameHelper.IsSlash(card))
            {
                // 使用杀时额外弃一张牌
                if (Owner.handCards.Count > 0)
                {
                    int index = Random.Range(0, Owner.handCards.Count);
                    Card discard = Owner.handCards[index];
                    Owner.DiscardCard(discard);
                    Log($"{Owner.generalName} 因【北人】弃置了 {discard.cardName}");
                }
            }
        }

        public override string GetDescription()
        {
            return "锁定技，你的手牌上限-1，你使用【杀】时需额外弃置一张牌。";
        }
    }

    /// <summary>
    /// 咆哮（张飞）
    /// 出牌阶段，你使用【杀】无次数限制
    /// </summary>
    public class PaoxiaoSkill : SkillBase
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

            // 设置无限出杀
            if (Owner != null)
            {
                Owner.maxSlashPerTurn = 999;
                Log($"{Owner.generalName} 的【咆哮】生效，本回合出杀无限制");
            }
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
            return "出牌阶段，你使用【杀】无次数限制。";
        }
    }

    /// <summary>
    /// 义绝（关羽）
    /// 当你使用【杀】对目标角色造成伤害时，可令其弃置一张牌
    /// </summary>
    public class YijueSkill : SkillBase
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
            // 效果在OnPlayerDamaged中处理
        }

        private void OnPlayerDamaged(Player victim, Player source, int damage, Card card)
        {
            if (source == Owner && card != null && CardNameHelper.IsSlash(card))
            {
                if (victim.isAlive && victim.handCards.Count > 0)
                {
                    // 令目标弃置一张牌
                    int index = Random.Range(0, victim.handCards.Count);
                    Card discard = victim.handCards[index];
                    victim.DiscardCard(discard);
                    Log($"{Owner.generalName} 发动【义绝】，{victim.generalName} 弃置了 {discard.cardName}");
                }
            }
        }

        public override string GetDescription()
        {
            return "当你使用【杀】对目标角色造成伤害时，可令其弃置一张牌。";
        }
    }

    // ==================== 官渡之战/讨董之战 v2 新增技能 ====================

    /// <summary>
    /// 威武（颜良）
    /// 摸牌阶段少摸1张牌，你可以将黑色牌当【决斗】使用
    /// </summary>
    public class WeiWuSkill : SkillBase
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
            // 转化技能，在出牌时处理
        }

        private void OnDrawPhase(Player player)
        {
            if (player == Owner)
            {
                // 少摸1张牌 - 通过弃置1张刚摸的牌实现
                if (Owner.handCards.Count > 0)
                {
                    Card lastCard = Owner.handCards[Owner.handCards.Count - 1];
                    Owner.DiscardCard(lastCard);
                    Log($"{Owner.generalName} 【威武】发动，少摸1张牌");
                }
            }
        }

        /// <summary>
        /// 检查是否可以将黑色牌转化为决斗
        /// </summary>
        public bool CanConvertToDuel(Card card)
        {
            if (card == null) return false;
            return card.IsBlack();
        }

        public override string GetDescription()
        {
            return "摸牌阶段少摸1张牌，你可以将黑色牌当【决斗】使用。";
        }
    }

    /// <summary>
    /// 强健（文丑）
    /// 摸牌阶段少摸1张牌，你可以将黑色牌当【决斗】使用
    /// 与威武效果相同
    /// </summary>
    public class QiangJianSkill : WeiWuSkill
    {
        public override string GetDescription()
        {
            return "摸牌阶段少摸1张牌，你可以将黑色牌当【决斗】使用。";
        }
    }

    /// <summary>
    /// 多变（张郃）
    /// 出牌阶段，你可以额外使用一张【杀】
    /// </summary>
    public class DuoBianSkill : SkillBase
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

            // 增加出杀次数
            if (Owner != null)
            {
                Owner.maxSlashPerTurn = 2;
                Log($"{Owner.generalName} 的【多变】生效，本回合可出2张杀");
            }
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
            return "出牌阶段，你可以额外使用一张【杀】。";
        }
    }

    /// <summary>
    /// 马术（高览/袁军骑将）
    /// 你计算与其他角色的距离时-1
    /// </summary>
    public class MaShuSkill : SkillBase
    {
        protected override void RegisterEvents()
        {
            // 距离修正在Player.GetDistanceTo()中处理
            if (Owner != null)
            {
                Owner.offensiveHorseBonus = 1; // 使用进攻马加成来实现
                Log($"{Owner.generalName} 的【马术】生效，攻击距离-1");
            }
        }

        protected override void UnregisterEvents()
        {
            if (Owner != null)
            {
                Owner.offensiveHorseBonus = 0;
            }
        }

        protected override bool CheckTriggerCondition()
        {
            return Owner != null && Owner.isAlive;
        }

        public override void Trigger()
        {
            // 锁定技，被动生效
        }

        public override string GetDescription()
        {
            return "你计算与其他角色的距离时-1。";
        }
    }

    /// <summary>
    /// 断粮（徐晃）
    /// 你可以将黑色牌当【兵粮寸断】使用
    /// </summary>
    public class DuanLiangSkill : SkillBase
    {
        protected override bool CheckTriggerCondition()
        {
            return Owner != null && Owner.isAlive;
        }

        public override void Trigger()
        {
            // 转化技能，在出牌时处理
            Log($"{Owner.generalName} 可以使用【断粮】");
        }

        /// <summary>
        /// 检查是否可以将黑色牌转化为兵粮寸断
        /// </summary>
        public bool CanConvertToSupplyShortage(Card card)
        {
            if (card == null) return false;
            return card.IsBlack();
        }

        public override string GetDescription()
        {
            return "你可以将黑色牌当【兵粮寸断】使用。";
        }
    }

    /// <summary>
    /// 英明（淳于琼）
    /// 摸牌阶段，你可以多摸一张牌
    /// </summary>
    public class YingMingSkill : SkillBase
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

            Log($"{Owner.generalName} 发动了【英明】");
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
    /// 齐击（袁绍）
    /// 你可以将两张不同花色的牌当【万箭齐发】使用
    /// </summary>
    public class QiJiSkill : SkillBase
    {
        protected override bool CheckTriggerCondition()
        {
            if (Owner == null || !Owner.isAlive || Owner.handCards.Count < 2) return false;

            // 检查是否有两张不同花色的牌
            var suits = new System.Collections.Generic.HashSet<CardSuit>();
            foreach (var card in Owner.handCards)
            {
                suits.Add(card.suit);
                if (suits.Count >= 2) return true;
            }
            return false;
        }

        public override void Trigger()
        {
            if (!CanTrigger()) return;

            Log($"{Owner.generalName} 发动了【齐击】");

            // AI自动选择两张不同花色的牌
            Card card1 = null;
            Card card2 = null;
            foreach (var card in Owner.handCards)
            {
                if (card1 == null)
                {
                    card1 = card;
                }
                else if (card.suit != card1.suit)
                {
                    card2 = card;
                    break;
                }
            }

            if (card1 != null && card2 != null)
            {
                Owner.DiscardCard(card1);
                Owner.DiscardCard(card2);
                Log($"弃置 {card1.cardName} 和 {card2.cardName}，视为使用【万箭齐发】");

                // 对所有敌人造成效果
                var enemies = GetAliveEnemies();
                foreach (var enemy in enemies)
                {
                    // 检查是否有闪
                    bool hasDodge = false;
                    foreach (var c in enemy.handCards)
                    {
                        if (CardNameHelper.IsDodge(c))
                        {
                            hasDodge = true;
                            enemy.DiscardCard(c);
                            Log($"{enemy.generalName} 打出了【闪】");
                            break;
                        }
                    }
                    if (!hasDodge)
                    {
                        enemy.TakeDamage(1, Owner);
                        Log($"{enemy.generalName} 受到1点伤害");
                    }
                }
            }

            EventManager.Instance?.TriggerStoryEvent("skill_qiji", Owner.generalName);
        }

        /// <summary>
        /// 检查两张牌是否可以用于齐击
        /// </summary>
        public bool CanUseQiJi(Card card1, Card card2)
        {
            if (card1 == null || card2 == null) return false;
            return card1.suit != card2.suit;
        }

        public override string GetDescription()
        {
            return "你可以将两张不同花色的牌当【万箭齐发】使用。";
        }
    }

    /// <summary>
    /// 耀武v2（华雄）- 讨董之战版本
    /// 使用红色【杀】造成伤害后，你可以弃一张牌，获得目标一张牌
    /// </summary>
    public class YaoWuV2Skill : SkillBase
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
            return Owner != null && Owner.isAlive && Owner.handCards.Count > 0;
        }

        public override void Trigger()
        {
            // 效果在OnPlayerDamaged中处理
        }

        private void OnPlayerDamaged(Player victim, Player source, int damage, Card card)
        {
            if (source == Owner && card != null && CardNameHelper.IsSlash(card) && card.IsRed())
            {
                if (Owner.handCards.Count > 0 && victim.isAlive && victim.handCards.Count > 0)
                {
                    // 弃一张牌
                    int discardIndex = Random.Range(0, Owner.handCards.Count);
                    Card discard = Owner.handCards[discardIndex];
                    Owner.DiscardCard(discard);

                    // 获得目标一张牌
                    int stealIndex = Random.Range(0, victim.handCards.Count);
                    Card stolen = victim.handCards[stealIndex];
                    victim.handCards.RemoveAt(stealIndex);
                    Owner.handCards.Add(stolen);

                    Log($"{Owner.generalName} 发动【耀武】，弃置 {discard.cardName}，获得 {victim.generalName} 的一张牌");
                }
            }
        }

        public override string GetDescription()
        {
            return "使用红色【杀】造成伤害后，你可以弃一张牌，获得目标一张牌。";
        }
    }

    /// <summary>
    /// 无双（吕布）
    /// 你使用【杀】需要两张【闪】才能抵消；
    /// 你与其他角色进行【决斗】时，对方需打出两张【杀】
    /// </summary>
    public class WuShuangSkill : SkillBase
    {
        protected override bool CheckTriggerCondition()
        {
            return Owner != null && Owner.isAlive;
        }

        public override void Trigger()
        {
            // 锁定技，效果在战斗系统中检查
        }

        /// <summary>
        /// 检查是否需要双倍闪来抵消杀
        /// </summary>
        public bool RequiresDoubleDodge()
        {
            return true;
        }

        /// <summary>
        /// 检查决斗时是否需要双倍杀
        /// </summary>
        public bool RequiresDoubleSlashInDuel()
        {
            return true;
        }

        public override string GetDescription()
        {
            return "锁定技，你使用【杀】需两张【闪】响应；你的【决斗】中对方需打出两张【杀】。";
        }
    }

    /// <summary>
    /// 英魂（孙坚）
    /// 回合开始时，若你已受伤，你可以令一名角色摸X张牌然后弃X张牌（X为你已损失的体力值）
    /// </summary>
    public class YingHunSkill : SkillBase
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
            return Owner != null && Owner.isAlive && Owner.currentHP < Owner.maxHP;
        }

        public override void Trigger()
        {
            if (!CanTrigger()) return;

            int lostHP = Owner.maxHP - Owner.currentHP;
            if (lostHP <= 0) return;

            // 选择目标：优先选择敌人（让敌人先摸后弃，可能亏牌）
            var enemies = GetAliveEnemies();
            Player target = enemies.Length > 0 ? enemies[Random.Range(0, enemies.Length)] : Owner;

            Log($"{Owner.generalName} 发动了【英魂】，X={lostHP}");

            // 目标摸X张牌
            var cards = DeckManager.Instance?.DrawCards(lostHP);
            if (cards != null && cards.Count > 0)
            {
                target.DrawCards(cards);
                Log($"{target.generalName} 摸了 {lostHP} 张牌");
            }

            // 目标弃X张牌
            for (int i = 0; i < lostHP && target.handCards.Count > 0; i++)
            {
                int index = Random.Range(0, target.handCards.Count);
                Card discard = target.handCards[index];
                target.DiscardCard(discard);
            }
            Log($"{target.generalName} 弃置了 {lostHP} 张牌");

            EventManager.Instance?.TriggerStoryEvent("skill_yinghun", Owner.generalName);
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
            return "回合开始时，若你已受伤，可令一名角色摸X张牌然后弃X张牌（X为你已损失的体力值）。";
        }
    }

    /// <summary>
    /// 血裔（袁绍-讨董版）
    /// 摸牌阶段，你可以额外摸X张牌（X为场上已受伤的角色数）
    /// </summary>
    public class XueYiSkill : SkillBase
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

            // 计算已受伤角色数
            int woundedCount = 0;
            var allPlayers = GetAlivePlayers();
            foreach (var player in allPlayers)
            {
                if (player.currentHP < player.maxHP)
                {
                    woundedCount++;
                }
            }

            if (woundedCount > 0)
            {
                Log($"{Owner.generalName} 发动了【血裔】，额外摸 {woundedCount} 张牌");
                var cards = DeckManager.Instance?.DrawCards(woundedCount);
                if (cards != null && cards.Count > 0)
                {
                    Owner.DrawCards(cards);
                }
            }
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
            return "摸牌阶段，你可以额外摸X张牌（X为场上已受伤的角色数）。";
        }
    }

    /// <summary>
    /// 酒池肉林（董卓）
    /// 你可以将一张【杀】当【酒】使用；
    /// 你可以将一张【酒】当【杀】使用
    /// </summary>
    public class JiuChiRouLinSkill : SkillBase
    {
        protected override bool CheckTriggerCondition()
        {
            return Owner != null && Owner.isAlive;
        }

        public override void Trigger()
        {
            // 转化技能，在出牌时处理
        }

        /// <summary>
        /// 检查是否可以将杀当酒
        /// </summary>
        public bool CanConvertSlashToWine(Card card)
        {
            if (card == null) return false;
            return CardNameHelper.IsSlash(card);
        }

        /// <summary>
        /// 检查是否可以将酒当杀
        /// </summary>
        public bool CanConvertWineToSlash(Card card)
        {
            if (card == null) return false;
            return card.cardName == "酒";
        }

        public override string GetDescription()
        {
            return "你可以将一张【杀】当【酒】使用；你可以将一张【酒】当【杀】使用。";
        }
    }

    /// <summary>
    /// 劫掠（李傕）
    /// 当你造成伤害后，你可以弃一张牌，获得目标一张牌
    /// </summary>
    public class JieLveSkill : SkillBase
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
            return Owner != null && Owner.isAlive && Owner.handCards.Count > 0;
        }

        public override void Trigger()
        {
            // 效果在OnPlayerDamaged中处理
        }

        private void OnPlayerDamaged(Player victim, Player source, int damage, Card card)
        {
            if (source == Owner && Owner.handCards.Count > 0 && victim.isAlive && victim.handCards.Count > 0)
            {
                // 50%概率发动
                if (Random.value < 0.5f)
                {
                    // 弃一张牌
                    int discardIndex = Random.Range(0, Owner.handCards.Count);
                    Card discard = Owner.handCards[discardIndex];
                    Owner.DiscardCard(discard);

                    // 获得目标一张牌
                    int stealIndex = Random.Range(0, victim.handCards.Count);
                    Card stolen = victim.handCards[stealIndex];
                    victim.handCards.RemoveAt(stealIndex);
                    Owner.handCards.Add(stolen);

                    Log($"{Owner.generalName} 发动【劫掠】，弃置 {discard.cardName}，获得 {victim.generalName} 的一张牌");
                }
            }
        }

        public override string GetDescription()
        {
            return "当你造成伤害后，你可以弃一张牌，获得目标一张牌。";
        }
    }

    /// <summary>
    /// 凶暴（郭汜）
    /// 每当你造成1点伤害后，你可以摸一张牌
    /// </summary>
    public class XiongBaoSkill : SkillBase
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
            // 效果在OnPlayerDamaged中处理
        }

        private void OnPlayerDamaged(Player victim, Player source, int damage, Card card)
        {
            if (source == Owner)
            {
                // 每点伤害摸一张牌
                for (int i = 0; i < damage; i++)
                {
                    var cards = DeckManager.Instance?.DrawCards(1);
                    if (cards != null && cards.Count > 0)
                    {
                        Owner.DrawCards(cards);
                    }
                }
                Log($"{Owner.generalName} 发动【凶暴】，造成 {damage} 点伤害，摸 {damage} 张牌");
            }
        }

        public override string GetDescription()
        {
            return "每当你造成1点伤害后，你可以摸一张牌。";
        }
    }
}
