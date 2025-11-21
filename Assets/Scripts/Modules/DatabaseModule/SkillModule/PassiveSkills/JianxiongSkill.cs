using UnityEngine;

namespace ThreeKingdoms.DatabaseModule.Skills
{
    /// <summary>
    /// 奸雄（曹操）
    /// 当你受到伤害后，你可以获得对你造成伤害的牌。
    /// </summary>
    public class JianxiongSkill : SkillBase
    {
        private Card damageCard; // 造成伤害的牌

        protected override void RegisterEvents()
        {
            // ⭐ 注册伤害事件
            if (EventManager.Instance != null)
            {
                EventManager.Instance.OnPlayerDamaged += OnPlayerDamaged;
                Debug.Log($"[奸雄] {Owner.generalName} 已注册伤害监听");
            }
            else
            {
                Debug.LogError("[奸雄] EventManager不存在！请在场景中添加EventManager");
            }
        }

        protected override void UnregisterEvents()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.OnPlayerDamaged -= OnPlayerDamaged;
            }
        }

        /// <summary>
        /// 检查触发条件
        /// </summary>
        protected override bool CheckTriggerCondition()
        {
            // 当受到伤害且造成伤害的牌存在时可以触发
            return damageCard != null;
        }

        /// <summary>
        /// 触发技能
        /// </summary>
        public override void Trigger()
        {
            if (!CanTrigger()) return;

            Log($"{Owner.generalName} 发动了【奸雄】");

            // 将造成伤害的牌收入手牌
            if (damageCard != null)
            {
                Owner.DrawCard(damageCard);
                Log($"获得了 {damageCard.cardName}");

                damageCard = null;
            }
        }

        /// <summary>
        /// 当玩家受到伤害时的回调
        /// </summary>
        private void OnPlayerDamaged(Player victim, Player source, int damage, Card card)
        {
            // 只处理自己受伤的情况
            if (victim != Owner) return;

            Log($"{Owner.generalName} 受到伤害，触发条件检查");

            // 记录造成伤害的牌
            damageCard = card;

            // 询问玩家是否发动技能
            if (Owner.isAI)
            {
                // AI自动发动
                Trigger();
            }
            else
            {
                // 人类玩家也自动发动（简化版）
                Trigger();
                // TODO: 显示UI询问玩家是否发动
            }
        }

        public override string GetDescription()
        {
            return "当你受到伤害后，你可以获得对你造成伤害的牌。";
        }
    }
}