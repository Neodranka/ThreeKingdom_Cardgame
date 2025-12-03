using UnityEngine;
using System.Collections.Generic;

namespace ThreeKingdoms.DatabaseModule.Skills
{
    /// <summary>
    /// 制衡（孙权）
    /// 出牌阶段限一次，你可以弃置任意张牌，然后摸等量的牌。
    /// </summary>
    public class ZhihengSkill : SkillBase
    {
        private bool usedThisTurn = false; // 本回合是否已使用

        protected override void RegisterEvents()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.OnTurnStart += OnTurnStart;
                EventManager.Instance.OnTurnEnd += OnTurnEnd;
                Debug.Log($"[制衡] {Owner.generalName} 已注册回合事件监听");
            }
            else
            {
                Debug.LogError("[制衡] EventManager不存在！");
            }
        }

        protected override void UnregisterEvents()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.OnTurnStart -= OnTurnStart;
                EventManager.Instance.OnTurnEnd -= OnTurnEnd;
            }
        }

        protected override bool CheckTriggerCondition()
        {
            // 检查是否在自己的出牌阶段且本回合未使用
            if (usedThisTurn)
            {
                Log($"{Owner.generalName} 的【制衡】本回合已使用");
                return false;
            }

            // 检查是否有手牌可以弃置
            if (Owner.handCards.Count == 0)
            {
                Log($"{Owner.generalName} 没有手牌，无法发动【制衡】");
                return false;
            }

            return true;
        }

        public override void Trigger()
        {
            if (!CanTrigger()) return;

            Log($"{Owner.generalName} 发动了【制衡】");

            // TODO: 实际实现需要：
            // 1. 显示UI让玩家选择要弃置的牌
            // 2. 弃置选中的牌
            // 3. 摸等量的牌
            // 4. 标记本回合已使用

            // 暂时的AI自动使用逻辑
            if (Owner.isAI)
            {
                UseZhiheng();
            }
        }

        /// <summary>
        /// 执行制衡（AI或玩家确认后）
        /// </summary>
        public void UseZhiheng()
        {
            if (!CanTrigger()) return;

            // AI简单策略：弃置所有非杀、非闪、非桃的牌
            List<Card> cardsToDiscard = new List<Card>();

            foreach (var card in Owner.handCards)
            {
                // 保留关键牌：杀、闪、桃（使用CardNameHelper避免硬编码）
                if (!CardNameHelper.IsSlash(card) &&
                    !CardNameHelper.IsDodge(card) &&
                    !CardNameHelper.IsPeach(card))
                {
                    cardsToDiscard.Add(card);
                }
            }

            // 如果没有要弃的牌，至少弃一张
            if (cardsToDiscard.Count == 0 && Owner.handCards.Count > 0)
            {
                cardsToDiscard.Add(Owner.handCards[0]);
            }

            int discardCount = cardsToDiscard.Count;

            if (discardCount == 0)
            {
                Log($"{Owner.generalName} 没有可以弃置的牌");
                return;
            }

            // 弃置牌
            Log($"{Owner.generalName} 弃置了 {discardCount} 张牌");
            foreach (var card in cardsToDiscard)
            {
                Owner.handCards.Remove(card);

                // 将牌加入弃牌堆
                if (DeckManager.Instance != null)
                {
                    DeckManager.Instance.DiscardCard(card);
                }
            }

            // 摸等量的牌
            for (int i = 0; i < discardCount; i++)
            {
                Card drawnCard = DeckManager.Instance.DrawCard();
                if (drawnCard != null)
                {
                    Owner.handCards.Add(drawnCard);
                }
            }

            Log($"{Owner.generalName} 摸了 {discardCount} 张牌");

            // 标记本回合已使用
            usedThisTurn = true;
        }

        /// <summary>
        /// 使用制衡弃置指定的牌
        /// </summary>
        public void UseZhihengWithCards(List<Card> cardsToDiscard)
        {
            if (!CanTrigger()) return;

            if (cardsToDiscard == null || cardsToDiscard.Count == 0)
            {
                Debug.LogWarning($"{Owner.generalName} 制衡至少需要弃置一张牌");
                return;
            }

            int discardCount = cardsToDiscard.Count;

            // 弃置牌
            Log($"{Owner.generalName} 弃置了 {discardCount} 张牌");
            foreach (var card in cardsToDiscard)
            {
                if (Owner.handCards.Contains(card))
                {
                    Owner.handCards.Remove(card);

                    if (DeckManager.Instance != null)
                    {
                        DeckManager.Instance.DiscardCard(card);
                    }
                }
            }

            // 摸等量的牌
            for (int i = 0; i < discardCount; i++)
            {
                Card drawnCard = DeckManager.Instance.DrawCard();
                if (drawnCard != null)
                {
                    Owner.handCards.Add(drawnCard);
                }
            }

            Log($"{Owner.generalName} 摸了 {discardCount} 张牌");

            // 标记本回合已使用
            usedThisTurn = true;
        }

        private void OnTurnStart(Player player)
        {
            if (player != Owner) return;

            // 回合开始时重置使用状态
            usedThisTurn = false;
            Log($"{Owner.generalName} 的回合开始，【制衡】可以使用");
        }

        private void OnTurnEnd(Player player)
        {
            if (player != Owner) return;

            // 回合结束时重置（双保险）
            usedThisTurn = false;
        }

        public override string GetDescription()
        {
            return "出牌阶段限一次，你可以弃置任意张牌，然后摸等量的牌。";
        }
    }
}