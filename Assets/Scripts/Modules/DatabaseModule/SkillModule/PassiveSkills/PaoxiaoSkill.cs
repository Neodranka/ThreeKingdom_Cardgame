using UnityEngine;

namespace ThreeKingdoms.DatabaseModule.Skills
{
    /// <summary>
    /// 咆哮（张飞）
    /// 锁定技，出牌阶段，你使用【杀】无次数限制。
    /// </summary>
    public class PaoxiaoSkill : SkillBase
    {
        protected override void RegisterEvents()
        {
            // 咆哮是锁定技，修改使用杀的次数限制
            if (EventManager.Instance != null)
            {
                EventManager.Instance.OnTurnStart += OnTurnStart;
                Debug.Log($"[咆哮] {Owner.generalName} 已注册回合开始监听");
            }
            else
            {
                Debug.LogError("[咆哮] EventManager不存在！");
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
            // 咆哮是锁定技，始终生效
            return true;
        }

        public override void Trigger()
        {
            if (!CanTrigger()) return;

            // 咆哮是被动技能，不需要主动触发
            // 效果在判断是否可以使用杀时检查
            Log($"{Owner.generalName} 的【咆哮】生效中（使用杀无次数限制）");
        }

        /// <summary>
        /// 检查是否可以使用杀（咆哮效果）
        /// </summary>
        public bool CanUseSlashUnlimited()
        {
            // 咆哮效果：可以无限使用杀
            return true;
        }

        /// <summary>
        /// 获取出杀次数限制
        /// </summary>
        public int GetSlashLimit()
        {
            // 咆哮：无限制，返回一个很大的数
            return 999;
        }

        private void OnTurnStart(Player player)
        {
            if (player != Owner) return;

            // 回合开始时提示咆哮生效
            Log($"{Owner.generalName} 的回合开始，【咆哮】生效（本回合使用杀无次数限制）");
        }

        public override string GetDescription()
        {
            return "锁定技，出牌阶段，你使用【杀】无次数限制。";
        }
    }
}