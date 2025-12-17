using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThreeKingdoms.DatabaseModule
{
    /// <summary>
    /// 技能工厂
    /// 负责从技能ID字符串创建技能实例
    /// </summary>
    public static class SkillFactory
    {
        // 技能ID到技能类型的映射表
        private static readonly Dictionary<string, Type> skillTypeMap = new Dictionary<string, Type>
        {
            // ==================== 基础技能 ====================
            // 蜀国
            {"rende", typeof(Skills.RendeSkill)},               // 仁德（刘备）
            {"wusheng", typeof(Skills.WushengSkill)},           // 武圣（关羽）
            {"paoxiao", typeof(Skills.PaoxiaoSkill)},           // 咆哮（张飞）

            // 魏国
            {"jianxiong", typeof(Skills.JianxiongSkill)},       // 奸雄（曹操）

            // 吴国
            {"zhiheng", typeof(Skills.ZhihengSkill)},           // 制衡（孙权）

            // ==================== 故事模式技能 ====================
            // 赤壁之战
            {"zhuhe", typeof(Skills.Story.ZhuheSkill)},                 // 主和（张昭）
            {"dimeng", typeof(Skills.Story.DimengSkill)},               // 缔盟（鲁肃）
            {"laodang", typeof(Skills.Story.LaodangSkill)},             // 老当（程普）
            {"guanxing", typeof(Skills.Story.GuanxingSkill)},           // 观星（诸葛亮）
            {"kongcheng", typeof(Skills.Story.KongchengSkill)},         // 空城（诸葛亮）
            {"yingzi", typeof(Skills.Story.YingziSkill)},               // 英姿（周瑜）
            {"fanjian", typeof(Skills.Story.FanjianSkill)},             // 反间（周瑜）
            {"keji", typeof(Skills.Story.KejiSkill)},                   // 克己（吕蒙）
            {"longdan", typeof(Skills.Story.LongdanSkill)},             // 龙胆（赵云）
            {"danlie", typeof(Skills.Story.DanlieSkill)},               // 胆裂（夏侯杰）
            {"kurou_zhaxiang", typeof(Skills.Story.KurouZhaxiangSkill)},// 苦肉诈降（黄盖-诈降版）
            {"kurou", typeof(Skills.Story.KurouSkill)},                 // 苦肉（黄盖-普通版）
            {"daoshu", typeof(Skills.Story.DaoshuSkill)},               // 盗书（蒋干）
            {"ganglie", typeof(Skills.Story.GanglieSkill)},             // 刚烈（夏侯惇）
            {"shenshu", typeof(Skills.Story.ShenshuSkill)},             // 神速（夏侯渊）
            {"tuxi", typeof(Skills.Story.TuxiSkill)},                   // 突袭（张辽）

            // 官渡之战/讨董之战
            {"dongyao", typeof(Skills.Story.DongyaoSkill)},             // 动摇
            {"xiaohao", typeof(Skills.Story.XiaohaoSkill)},             // 消耗
            {"xiancelue", typeof(Skills.Story.XianceSkill)},            // 献策
            {"yuanhu", typeof(Skills.Story.YuanhuSkill)},               // 援护
            {"shijiu", typeof(Skills.Story.ShijiuSkill)},               // 嗜酒
            {"shouliang", typeof(Skills.Story.ShouliangSkill)},         // 守粮
            {"xiaoji_passive", typeof(Skills.Story.XiaojiPassiveSkill)},// 消极
            {"fuji", typeof(Skills.Story.FujiSkill)},                   // 伏击
            {"chongzhen", typeof(Skills.Story.ChongzhenSkill)},         // 冲阵
            {"yaowu", typeof(Skills.Story.YaowuSkill)},                 // 耀武
            {"qiezhan", typeof(Skills.Story.QiezhanSkill)},             // 怯战
            {"fenlie", typeof(Skills.Story.FenlieSkill)},               // 分裂
        };

        // 技能ID到技能名称的映射
        private static readonly Dictionary<string, string> skillNameMap = new Dictionary<string, string>
        {
            // 基础技能
            {"rende", "仁德"}, {"wusheng", "武圣"}, {"paoxiao", "咆哮"},
            {"jianxiong", "奸雄"}, {"zhiheng", "制衡"},

            // 故事模式技能
            {"zhuhe", "主和"}, {"dimeng", "缔盟"}, {"laodang", "老当"},
            {"guanxing", "观星"}, {"kongcheng", "空城"},
            {"yingzi", "英姿"}, {"fanjian", "反间"}, {"keji", "克己"},
            {"longdan", "龙胆"}, {"danlie", "胆裂"},
            {"kurou_zhaxiang", "苦肉诈降"}, {"kurou", "苦肉"}, {"daoshu", "盗书"},
            {"ganglie", "刚烈"}, {"shenshu", "神速"}, {"tuxi", "突袭"},
            {"dongyao", "动摇"}, {"xiaohao", "消耗"}, {"xiancelue", "献策"},
            {"yuanhu", "援护"}, {"shijiu", "嗜酒"}, {"shouliang", "守粮"},
            {"xiaoji_passive", "消极"}, {"fuji", "伏击"}, {"chongzhen", "冲阵"},
            {"yaowu", "耀武"}, {"qiezhan", "怯战"}, {"fenlie", "分裂"},
        };

        /// <summary>
        /// 从技能ID创建技能实例
        /// </summary>
        /// <param name="skillId">技能ID</param>
        /// <param name="owner">技能拥有者</param>
        /// <returns>技能实例，如果创建失败返回null</returns>
        public static ISkill CreateSkill(string skillId, Player owner)
        {
            if (string.IsNullOrEmpty(skillId))
            {
                Debug.LogWarning("[SkillFactory] 技能ID为空");
                return null;
            }

            string normalizedId = skillId.ToLower().Trim();

            if (!skillTypeMap.TryGetValue(normalizedId, out Type skillType))
            {
                Debug.LogWarning($"[SkillFactory] 未找到技能ID映射: {skillId}");
                return null;
            }

            try
            {
                // 创建技能实例
                ISkill skill = Activator.CreateInstance(skillType) as ISkill;
                if (skill == null)
                {
                    Debug.LogError($"[SkillFactory] 无法创建技能实例: {skillId} (类型: {skillType.Name})");
                    return null;
                }

                // 创建一个临时的SkillData用于初始化
                SkillData tempData = ScriptableObject.CreateInstance<SkillData>();
                tempData.skillId = normalizedId;
                tempData.skillName = GetSkillName(normalizedId);
                tempData.skillClassName = skillType.FullName;

                // 初始化技能
                skill.Initialize(tempData, owner);

                Debug.Log($"[SkillFactory] ✓ 创建技能: {tempData.skillName} (ID: {skillId}) 给 {owner?.generalName}");
                return skill;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SkillFactory] 创建技能失败: {skillId}, 错误: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 为玩家创建并添加多个技能
        /// </summary>
        /// <param name="skillIds">技能ID列表</param>
        /// <param name="owner">技能拥有者</param>
        /// <returns>成功创建的技能列表</returns>
        public static List<ISkill> CreateSkills(List<string> skillIds, Player owner)
        {
            var skills = new List<ISkill>();

            if (skillIds == null || owner == null)
            {
                return skills;
            }

            foreach (var skillId in skillIds)
            {
                var skill = CreateSkill(skillId, owner);
                if (skill != null)
                {
                    skills.Add(skill);
                }
            }

            return skills;
        }

        /// <summary>
        /// 获取技能名称
        /// </summary>
        public static string GetSkillName(string skillId)
        {
            if (string.IsNullOrEmpty(skillId)) return "未知技能";

            string normalizedId = skillId.ToLower().Trim();
            if (skillNameMap.TryGetValue(normalizedId, out string name))
            {
                return name;
            }
            return skillId;
        }

        /// <summary>
        /// 检查技能是否已注册
        /// </summary>
        public static bool IsSkillRegistered(string skillId)
        {
            if (string.IsNullOrEmpty(skillId)) return false;
            return skillTypeMap.ContainsKey(skillId.ToLower().Trim());
        }

        /// <summary>
        /// 获取所有已注册的技能ID
        /// </summary>
        public static IEnumerable<string> GetAllRegisteredSkillIds()
        {
            return skillTypeMap.Keys;
        }

        /// <summary>
        /// 注册自定义技能（用于扩展）
        /// </summary>
        public static void RegisterSkill(string skillId, Type skillType, string displayName = null)
        {
            if (string.IsNullOrEmpty(skillId) || skillType == null)
            {
                Debug.LogWarning("[SkillFactory] 注册技能失败：参数无效");
                return;
            }

            if (!typeof(ISkill).IsAssignableFrom(skillType))
            {
                Debug.LogWarning($"[SkillFactory] 注册技能失败：{skillType.Name} 不是 ISkill 类型");
                return;
            }

            string normalizedId = skillId.ToLower().Trim();
            skillTypeMap[normalizedId] = skillType;

            if (!string.IsNullOrEmpty(displayName))
            {
                skillNameMap[normalizedId] = displayName;
            }

            Debug.Log($"[SkillFactory] 注册技能: {skillId} -> {skillType.Name}");
        }
    }
}
