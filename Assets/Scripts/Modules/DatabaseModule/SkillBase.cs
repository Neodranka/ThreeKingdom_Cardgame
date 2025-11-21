using UnityEngine;

namespace ThreeKingdoms.DatabaseModule
{
    /// <summary>
    /// 技能基类
    /// 提供技能的通用实现，子类只需重写特定方法
    /// </summary>
    public abstract class SkillBase : ISkill
    {
        public SkillData SkillData { get; protected set; }
        public Player Owner { get; protected set; }
        public bool IsEnabled { get; set; }

        protected SkillBase()
        {
            IsEnabled = true;
        }

        /// <summary>
        /// 初始化技能
        /// </summary>
        public virtual void Initialize(SkillData skillData, Player owner)
        {
            SkillData = skillData;
            Owner = owner;
            IsEnabled = true;

            // 注册技能事件监听
            RegisterEvents();
        }

        /// <summary>
        /// 注册事件监听（子类可重写）
        /// </summary>
        protected virtual void RegisterEvents()
        {
            // 子类根据技能类型注册相应的事件
        }

        /// <summary>
        /// 检查技能是否可以触发
        /// </summary>
        public virtual bool CanTrigger()
        {
            if (!IsEnabled) return false;
            if (Owner == null || !Owner.isAlive) return false;

            return CheckTriggerCondition();
        }

        /// <summary>
        /// 检查触发条件（子类必须实现）
        /// </summary>
        protected abstract bool CheckTriggerCondition();

        /// <summary>
        /// 触发技能（子类必须实现）
        /// </summary>
        public abstract void Trigger();

        /// <summary>
        /// 获取有效目标
        /// </summary>
        public virtual Player[] GetValidTargets()
        {
            // 默认返回空数组，子类根据需要重写
            return new Player[0];
        }

        /// <summary>
        /// 获取技能描述
        /// </summary>
        public virtual string GetDescription()
        {
            return SkillData != null ? SkillData.description : "未知技能";
        }

        /// <summary>
        /// 清理技能
        /// </summary>
        public virtual void Cleanup()
        {
            UnregisterEvents();
            IsEnabled = false;
        }

        /// <summary>
        /// 取消注册事件（子类可重写）
        /// </summary>
        protected virtual void UnregisterEvents()
        {
            // 子类取消注册事件
        }

        /// <summary>
        /// 记录日志
        /// </summary>
        protected void Log(string message)
        {
            Debug.Log($"[技能:{SkillData?.skillName}] {message}");
        }

        /// <summary>
        /// 获取所有存活的敌方玩家
        /// </summary>
        protected Player[] GetAliveEnemies()
        {
            if (BattleManager.Instance == null) return new Player[0];

            return System.Array.FindAll(
                BattleManager.Instance.players.ToArray(),
                p => p != Owner && p.isAlive
            );
        }

        /// <summary>
        /// 获取所有存活的友方玩家
        /// </summary>
        protected Player[] GetAliveAllies()
        {
            if (BattleManager.Instance == null) return new Player[0];

            return System.Array.FindAll(
                BattleManager.Instance.players.ToArray(),
                p => p.faction == Owner.faction && p.isAlive
            );
        }

        /// <summary>
        /// 获取所有存活的玩家
        /// </summary>
        protected Player[] GetAlivePlayers()
        {
            if (BattleManager.Instance == null) return new Player[0];

            return System.Array.FindAll(
                BattleManager.Instance.players.ToArray(),
                p => p.isAlive
            );
        }
    }
}