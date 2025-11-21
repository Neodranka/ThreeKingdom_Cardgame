using UnityEngine;

namespace ThreeKingdoms.DatabaseModule.Skills
{
    /// <summary>
    /// 仁德（刘备）
    /// 出牌阶段，你可以将任意数量的手牌交给其他角色，若你于此阶段以此法给出的牌首次达到两张或更多，你回复1点体力。
    /// </summary>
    public class RendeSkill : SkillBase
    {
        private int cardsGivenThisTurn = 0;
        private bool hasRecovered = false;

        protected override void RegisterEvents()
        {
            // TODO: 注册回合开始事件
            // EventManager.Instance.OnTurnStart += OnTurnStart;
        }

        protected override void UnregisterEvents()
        {
            // TODO: 取消注册事件
            // EventManager.Instance.OnTurnStart -= OnTurnStart;
        }

        protected override bool CheckTriggerCondition()
        {
            // 在出牌阶段，有手牌，且有其他存活玩家
            if (BattleManager.Instance == null) return false;
            if (BattleManager.Instance.currentPhase != TurnPhase.Play) return false;
            if (Owner.handCards.Count == 0) return false;
            
            return GetValidTargets().Length > 0;
        }

        public override void Trigger()
        {
            if (!CanTrigger()) return;

            Log($"{Owner.generalName} 发动了【仁德】");

            // TODO: UI选择要给出的牌和目标
            // 这里简化处理
        }

        /// <summary>
        /// 执行仁德技能（给牌）
        /// </summary>
        public void GiveCards(Player target, Card[] cards)
        {
            if (cards == null || cards.Length == 0) return;
            if (target == null || target == Owner) return;

            Log($"将 {cards.Length} 张牌交给 {target.generalName}");

            // 转移卡牌
            foreach (var card in cards)
            {
                if (Owner.handCards.Contains(card))
                {
                    Owner.handCards.Remove(card);
                    target.handCards.Add(card);
                }
            }

            cardsGivenThisTurn += cards.Length;

            // 如果本回合首次给出2张或以上，回复1点体力
            if (!hasRecovered && cardsGivenThisTurn >= 2)
            {
                if (Owner.currentHP < Owner.maxHP)
                {
                    Owner.Recover(1);
                    Log("回复了1点体力");
                }
                hasRecovered = true;
            }
        }

        public override Player[] GetValidTargets()
        {
            return GetAliveEnemies();
        }

        /// <summary>
        /// 回合开始时重置计数
        /// </summary>
        private void OnTurnStart(Player player)
        {
            if (player == Owner)
            {
                cardsGivenThisTurn = 0;
                hasRecovered = false;
            }
        }

        public override string GetDescription()
        {
            return "出牌阶段，你可以将任意数量的手牌交给其他角色，若你于此阶段以此法给出的牌首次达到两张或更多，你回复1点体力。";
        }
    }
}
