namespace ThreeKingdoms.DatabaseModule
{
    /// <summary>
    /// 技能接口
    /// 所有技能必须实现此接口
    /// </summary>
    public interface ISkill
    {
        /// <summary>
        /// 技能所属的数据
        /// </summary>
        SkillData SkillData { get; }

        /// <summary>
        /// 技能拥有者
        /// </summary>
        Player Owner { get; }

        /// <summary>
        /// 技能是否可用
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// 初始化技能
        /// </summary>
        void Initialize(SkillData skillData, Player owner);

        /// <summary>
        /// 检查技能是否可以触发
        /// </summary>
        bool CanTrigger();

        /// <summary>
        /// 触发技能
        /// </summary>
        void Trigger();

        /// <summary>
        /// 获取技能目标（如果需要）
        /// </summary>
        Player[] GetValidTargets();

        /// <summary>
        /// 技能描述（用于UI显示）
        /// </summary>
        string GetDescription();

        /// <summary>
        /// 清理技能（当玩家死亡或技能失效时调用）
        /// </summary>
        void Cleanup();
    }
}