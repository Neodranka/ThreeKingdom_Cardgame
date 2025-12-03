using UnityEngine;

namespace ThreeKingdoms.DatabaseModule.Skills
{
    /// <summary>
    /// 武圣（关羽）
    /// 你可以将一张红色牌当【杀】使用或打出。
    /// </summary>
    public class WushengSkill : SkillBase
    {
        protected override void RegisterEvents()
        {
            // 武圣是转化技能，在玩家使用卡牌时检查
            if (EventManager.Instance != null)
            {
                EventManager.Instance.OnCardUsed += OnCardUsed;
                Debug.Log($"[武圣] {Owner.generalName} 已注册卡牌使用监听");
            }
            else
            {
                Debug.LogError("[武圣] EventManager不存在！");
            }
        }

        protected override void UnregisterEvents()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.OnCardUsed -= OnCardUsed;
            }
        }

        protected override bool CheckTriggerCondition()
        {
            // 武圣是转化技能，在需要使用杀时可以转化红色牌
            return true;
        }

        public override void Trigger()
        {
            if (!CanTrigger()) return;

            Log($"{Owner.generalName} 发动了【武圣】");

            // TODO: 实现红色牌转化为杀的逻辑
            // 这需要在卡牌使用系统中集成
        }

        /// <summary>
        /// 检查一张牌是否可以被武圣转化为杀
        /// </summary>
        public bool CanConvertToSlash(Card card)
        {
            if (card == null) return false;

            // 检查是否是红色牌（红桃或方块）
            return card.suit == CardSuit.Heart || card.suit == CardSuit.Diamond;
        }

        /// <summary>
        /// 将红色牌转化为杀
        /// </summary>
        public Card ConvertToSlash(Card redCard)
        {
            if (!CanConvertToSlash(redCard))
            {
                Debug.LogWarning($"[武圣] {redCard.cardName} 不是红色牌，无法转化");
                return null;
            }

            Log($"{Owner.generalName} 将 {redCard.cardName} 转化为【杀】");

            // 创建一个临时的杀
            // 保留原牌的花色和点数，但类型变为杀
            // 在实际使用时，这个转化需要在卡牌系统中处理

            return redCard; // 暂时返回原牌，实际需要创建转化牌
        }

        private void OnCardUsed(Player player, Card card, Player target)
        {
            // 当玩家使用卡牌时，检查是否可以发动武圣
            if (player != Owner) return;

            // 这里可以添加UI提示，询问玩家是否发动武圣
            // 实际的转化逻辑需要在卡牌系统中集成
        }

        public override string GetDescription()
        {
            return "你可以将一张红色牌当【杀】使用或打出。";
        }
    }
}