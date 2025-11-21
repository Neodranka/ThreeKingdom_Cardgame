using UnityEngine;

namespace ThreeKingdoms.DatabaseModule
{
    /// <summary>
    /// 技能类型
    /// </summary>
    public enum SkillType
    {
        Passive,    // 被动技能（锁定技）
        Active,     // 主动技能
        Trigger,    // 触发技能
        Limit       // 限定技
    }

    /// <summary>
    /// 技能触发时机
    /// </summary>
    public enum SkillTiming
    {
        None,               // 无特定时机
        OnTurnStart,        // 回合开始时
        OnTurnEnd,          // 回合结束时
        OnDrawPhase,        // 摸牌阶段
        OnPlayPhase,        // 出牌阶段
        OnDamaged,          // 受到伤害时
        OnDealDamage,       // 造成伤害时
        OnDying,            // 濒死时
        OnKill,             // 击杀时
        OnDeath,            // 死亡时
        OnCardPlayed,       // 出牌时
        OnCardUsed,         // 使用牌时
        OnCardDiscarded,    // 弃牌时
        BeforeJudge,        // 判定前
        AfterJudge          // 判定后
    }

    /// <summary>
    /// 技能数据（ScriptableObject）
    /// </summary>
    [CreateAssetMenu(fileName = "New Skill", menuName = "Three Kingdoms/Skill Data")]
    public class SkillData : ScriptableObject
    {
        [Header("基础信息")]
        [Tooltip("技能ID")]
        public string skillId;

        [Tooltip("技能名称")]
        public string skillName;

        [Tooltip("技能类型")]
        public SkillType skillType;

        [Tooltip("触发时机")]
        public SkillTiming timing;

        [Header("描述")]
        [Tooltip("技能描述")]
        [TextArea(2, 4)]
        public string description;

        [Header("实现")]
        [Tooltip("技能实现类的完整名称（例如：ThreeKingdoms.Skills.JianxiongSkill）")]
        public string skillClassName;

        [Header("配置参数")]
        [Tooltip("技能的配置参数（可选，JSON格式）")]
        [TextArea(2, 4)]
        public string configJson;

        [Header("UI")]
        [Tooltip("技能图标")]
        public Sprite icon;

        [Tooltip("技能音效")]
        public AudioClip soundEffect;

        /// <summary>
        /// 创建技能实例
        /// 使用反射根据 skillClassName 创建对应的技能类实例
        /// </summary>
        public ISkill CreateSkillInstance(Player owner)
        {
            if (string.IsNullOrEmpty(skillClassName))
            {
                Debug.LogError($"技能 {skillName} 没有指定 skillClassName!");
                return null;
            }

            try
            {
                // 使用反射创建技能实例
                System.Type skillType = System.Type.GetType(skillClassName);
                if (skillType == null)
                {
                    Debug.LogError($"找不到技能类: {skillClassName}");
                    return null;
                }

                // 创建实例
                ISkill skill = System.Activator.CreateInstance(skillType) as ISkill;
                if (skill == null)
                {
                    Debug.LogError($"无法创建技能实例: {skillClassName}");
                    return null;
                }

                // 初始化技能
                skill.Initialize(this, owner);

                return skill;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"创建技能 {skillName} 失败: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 验证数据
        /// </summary>
        public bool Validate()
        {
            if (string.IsNullOrEmpty(skillId))
            {
                Debug.LogError($"技能 {skillName} 缺少 skillId!");
                return false;
            }

            if (string.IsNullOrEmpty(skillName))
            {
                Debug.LogError($"技能 {skillId} 缺少 skillName!");
                return false;
            }

            if (string.IsNullOrEmpty(skillClassName))
            {
                Debug.LogWarning($"技能 {skillName} 缺少 skillClassName，无法实例化!");
                return false;
            }

            return true;
        }
    }
}