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

            // 赤壁之战v2新增
            {"jienan", typeof(Skills.Story.JienanSkill)},               // 诘难（虞翻）
            {"shuizhan", typeof(Skills.Story.ShuizhanSkill)},           // 水战（蔡瑁）
            {"beiren", typeof(Skills.Story.BeirenSkill)},               // 北人（曹军水兵）
            {"paoxiao_story", typeof(Skills.Story.PaoxiaoSkill)},       // 咆哮（张飞-故事版）
            {"yijue", typeof(Skills.Story.YijueSkill)},                 // 义绝（关羽）

            // ==================== 官渡之战/讨董之战 v2 新增 ====================
            {"weiwu", typeof(Skills.Story.WeiWuSkill)},                 // 威武（颜良）
            {"qiangjian", typeof(Skills.Story.QiangJianSkill)},         // 强健（文丑）
            {"duobian", typeof(Skills.Story.DuoBianSkill)},             // 多变（张郃）
            {"mashu", typeof(Skills.Story.MaShuSkill)},                 // 马术（高览/袁军骑将）
            {"duanliang", typeof(Skills.Story.DuanLiangSkill)},         // 断粮（徐晃）
            {"yingming", typeof(Skills.Story.YingMingSkill)},           // 英明（淳于琼）
            {"qiji", typeof(Skills.Story.QiJiSkill)},                   // 齐击（袁绍）
            {"yaowu_v2", typeof(Skills.Story.YaoWuV2Skill)},            // 耀武v2（华雄-讨董版）
            {"wushuang", typeof(Skills.Story.WuShuangSkill)},           // 无双（吕布）
            {"yinghun", typeof(Skills.Story.YingHunSkill)},             // 英魂（孙坚）
            {"xueyi", typeof(Skills.Story.XueYiSkill)},                 // 血裔（袁绍-讨董版）
            {"jiuchiroulin", typeof(Skills.Story.JiuChiRouLinSkill)},   // 酒池肉林（董卓）
            {"jielve", typeof(Skills.Story.JieLveSkill)},               // 劫掠（李傕）
            {"xiongbao", typeof(Skills.Story.XiongBaoSkill)},           // 凶暴（郭汜）
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

            // 赤壁之战v2新增
            {"jienan", "诘难"}, {"shuizhan", "水战"}, {"beiren", "北人"},
            {"paoxiao_story", "咆哮"}, {"yijue", "义绝"},

            // 官渡之战/讨董之战 v2 新增
            {"weiwu", "威武"}, {"qiangjian", "强健"}, {"duobian", "多变"},
            {"mashu", "马术"}, {"duanliang", "断粮"}, {"yingming", "英明"},
            {"qiji", "齐击"}, {"yaowu_v2", "耀武"}, {"wushuang", "无双"},
            {"yinghun", "英魂"}, {"xueyi", "血裔"}, {"jiuchiroulin", "酒池肉林"},
            {"jielve", "劫掠"}, {"xiongbao", "凶暴"},
        };

        // ⭐ 技能ID到技能类型的映射（主动/被动/触发）
        private static readonly Dictionary<string, SkillType> skillTypeClassMap = new Dictionary<string, SkillType>
        {
            // 主动技能 - 出牌阶段可主动发动
            {"rende", SkillType.Active},        // 仁德
            {"zhiheng", SkillType.Active},      // 制衡
            {"fanjian", SkillType.Active},      // 反间
            {"kurou", SkillType.Active},        // 苦肉
            {"kurou_zhaxiang", SkillType.Active}, // 苦肉诈降
            {"dimeng", SkillType.Active},       // 缔盟
            {"guanxing", SkillType.Active},     // 观星
            {"tuxi", SkillType.Active},         // 突袭
            {"shenshu", SkillType.Active},      // 神速

            // 转化技能 - 需要玩家主动使用
            {"wusheng", SkillType.Active},      // 武圣（红色牌当杀）

            // 被动/锁定技能 - 条件触发，无需玩家操作
            {"paoxiao", SkillType.Passive},     // 咆哮
            {"paoxiao_story", SkillType.Passive},
            {"yingzi", SkillType.Passive},      // 英姿
            {"kongcheng", SkillType.Passive},   // 空城
            {"keji", SkillType.Passive},        // 克己
            {"danlie", SkillType.Passive},      // 胆裂
            {"beiren", SkillType.Passive},      // 北人
            {"shuizhan", SkillType.Passive},    // 水战
            {"xiaoji_passive", SkillType.Passive}, // 消极
            {"laodang", SkillType.Passive},     // 老当

            // 触发技能 - 特定时机触发，玩家可选择
            {"jianxiong", SkillType.Trigger},   // 奸雄
            {"ganglie", SkillType.Trigger},     // 刚烈
            {"longdan", SkillType.Trigger},     // 龙胆
            {"yijue", SkillType.Trigger},       // 义绝
            {"zhuhe", SkillType.Trigger},       // 主和
            {"jienan", SkillType.Trigger},      // 诘难
            {"daoshu", SkillType.Trigger},      // 盗书

            // ==================== 官渡之战/讨董之战 v2 新增 ====================
            // 主动技能
            {"duanliang", SkillType.Active},    // 断粮
            {"qiji", SkillType.Active},         // 齐击
            {"yinghun", SkillType.Active},      // 英魂
            {"jiuchiroulin", SkillType.Active}, // 酒池肉林
            {"weiwu", SkillType.Active},        // 威武（主动转化+被动少摸）
            {"qiangjian", SkillType.Active},    // 强健

            // 被动技能
            {"duobian", SkillType.Passive},     // 多变
            {"mashu", SkillType.Passive},       // 马术
            {"yingming", SkillType.Passive},    // 英明
            {"wushuang", SkillType.Passive},    // 无双

            // 触发技能
            {"yaowu_v2", SkillType.Trigger},    // 耀武v2
            {"xueyi", SkillType.Trigger},       // 血裔
            {"jielve", SkillType.Trigger},      // 劫掠
            {"xiongbao", SkillType.Trigger},    // 凶暴
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
                // ⭐ 设置技能类型
                tempData.skillType = GetSkillType(normalizedId);

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
        /// 获取技能名称（本地化）
        /// </summary>
        public static string GetSkillName(string skillId)
        {
            if (string.IsNullOrEmpty(skillId)) return "未知技能";

            string normalizedId = skillId.ToLower().Trim();

            // 优先使用本地化
            if (ThreeKingdoms.LocalizationManager.Instance != null)
            {
                string localizedName = ThreeKingdoms.LocalizationManager.Instance.GetText($"skill_{normalizedId}");
                // 如果本地化成功（不是返回key本身），使用本地化名称
                if (!string.IsNullOrEmpty(localizedName) && localizedName != $"skill_{normalizedId}")
                {
                    return localizedName;
                }
            }

            // 备用：使用硬编码映射
            if (skillNameMap.TryGetValue(normalizedId, out string name))
            {
                return name;
            }
            return skillId;
        }

        /// <summary>
        /// 获取技能描述（本地化）
        /// </summary>
        public static string GetSkillDescription(string skillId)
        {
            if (string.IsNullOrEmpty(skillId)) return "";

            string normalizedId = skillId.ToLower().Trim();

            // 使用本地化
            if (ThreeKingdoms.LocalizationManager.Instance != null)
            {
                string localizedDesc = ThreeKingdoms.LocalizationManager.Instance.GetText($"skill_{normalizedId}_desc");
                // 如果本地化成功（不是返回key本身），使用本地化描述
                if (!string.IsNullOrEmpty(localizedDesc) && localizedDesc != $"skill_{normalizedId}_desc")
                {
                    return localizedDesc;
                }
            }

            return "";
        }

        /// <summary>
        /// ⭐ 获取技能类型（主动/被动/触发）
        /// </summary>
        public static SkillType GetSkillType(string skillId)
        {
            if (string.IsNullOrEmpty(skillId)) return SkillType.Passive;

            string normalizedId = skillId.ToLower().Trim();
            if (skillTypeClassMap.TryGetValue(normalizedId, out SkillType type))
            {
                return type;
            }
            // 默认为被动技能
            return SkillType.Passive;
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
