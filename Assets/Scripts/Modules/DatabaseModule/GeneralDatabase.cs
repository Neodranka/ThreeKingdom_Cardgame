using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace ThreeKingdoms.DatabaseModule
{
    /// <summary>
    /// 武将数据库
    /// 单例模式，管理所有武将数据
    /// </summary>
    public class GeneralDatabase : MonoBehaviour
    {
        public static GeneralDatabase Instance { get; private set; }

        [Header("武将数据")]
        [Tooltip("所有武将数据的列表")]
        public List<GeneralData> allGenerals = new List<GeneralData>();

        [Header("自动加载")]
        [Tooltip("是否在启动时自动从Resources文件夹加载")]
        public bool autoLoadFromResources = true;

        [Tooltip("Resources中的武将数据路径")]
        public string resourcePath = "Data/Generals";

        [Header("扩展角色")]
        [Tooltip("是否加载有技能的故事模式角色到对战池")]
        public bool loadStoryCharacters = true;

        private Dictionary<string, GeneralData> generalDictionary = new Dictionary<string, GeneralData>();

        // ⭐ 运行时创建的GeneralData需要保持引用，避免被GC
        private List<GeneralData> runtimeGenerals = new List<GeneralData>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 初始化数据库
        /// </summary>
        private void Initialize()
        {
            if (autoLoadFromResources)
            {
                LoadGeneralsFromResources();
            }

            // ⭐ 加载有技能的故事模式角色
            if (loadStoryCharacters)
            {
                LoadStoryCharactersWithSkills();
            }

            BuildDictionary();
            ValidateData();
        }

        // ⭐ 对战模式排除列表（仅在故事模式使用的角色）- 全部小写
        private static readonly HashSet<string> excludedFromBattle = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
        {
            // === 故事模式专属敌人 ===
            "caojun_cavalry",       // 曹军骑兵 - 故事模式敌人
            "caojuncavalry",        // 曹军骑兵（无下划线版本）
            "caojun_navy",          // 曹军水兵 - 故事模式敌人
            "caojunnavy",           // 曹军水兵（无下划线版本）
            "dongzhuo",             // 董卓(7/8血版) - 仅故事模式BOSS

            // === 赤壁之战专属角色 ===
            "xiahoujie",            // 夏侯杰 - 胆裂技能，专为赤壁之战设计
            "jianggan",             // 蒋干 - 盗书技能，专为蒋干盗书关卡设计

            // === 通用敌人单位 ===
            "xiliang_soldier",      // 西凉兵
            "xiliang_soldier_1",    // 西凉兵1
            "xiliang_soldier_2",    // 西凉兵2
            "xiliang_soldier_3",    // 西凉兵3
            "xiliangsoldier",       // 西凉兵（无下划线版本）
        };

        /// <summary>
        /// ⭐ 检查角色是否应该被排除（不区分大小写，支持多种ID格式）
        /// </summary>
        private static bool ShouldExcludeFromBattle(string generalId)
        {
            if (string.IsNullOrEmpty(generalId)) return false;

            string cleanId = generalId.ToLower().Replace("_story", "").Replace(" ", "_");
            return excludedFromBattle.Contains(cleanId) || excludedFromBattle.Contains(generalId);
        }

        /// <summary>
        /// 从Resources文件夹加载武将数据
        /// </summary>
        private void LoadGeneralsFromResources()
        {
            GeneralData[] loadedGenerals = Resources.LoadAll<GeneralData>(resourcePath);

            if (loadedGenerals.Length == 0)
            {
                Debug.LogWarning($"未在 Resources/{resourcePath} 中找到武将数据!");
                return;
            }

            // ⭐ 过滤掉仅故事模式使用的角色
            int excludedCount = 0;
            foreach (var general in loadedGenerals)
            {
                if (ShouldExcludeFromBattle(general.generalId))
                {
                    excludedCount++;
                    Debug.Log($"[GeneralDatabase] 排除故事模式角色: {general.generalName} (ID: {general.generalId})");
                    continue;
                }
                allGenerals.Add(general);
            }

            Debug.Log($"从Resources加载了 {loadedGenerals.Length - excludedCount} 个武将（排除 {excludedCount} 个故事模式角色）");
        }

        /// <summary>
        /// ⭐ 加载有独立技能的故事模式角色到对战池
        /// 这些角色会在运行时动态创建，使用SkillFactory来初始化技能
        /// </summary>
        private void LoadStoryCharactersWithSkills()
        {
            Debug.Log("[GeneralDatabase] 开始加载有技能的故事角色...");

            // ⭐ 定义有独立技能的角色列表
            var storyCharactersData = new List<(string id, string name, Faction faction, int hp, string[] skills)>
            {
                // === 魏国 (Wei) ===
                ("caocao", "曹操", Faction.Wei, 4, new[] { "jianxiong" }),
                ("xiahoudun", "夏侯惇", Faction.Wei, 4, new[] { "ganglie" }),
                ("xiahouyuan", "夏侯渊", Faction.Wei, 4, new[] { "shensu" }),
                ("zhangliao", "张辽", Faction.Wei, 4, new[] { "tuxi" }),
                ("xuhuang", "徐晃", Faction.Wei, 4, new[] { "duanliang" }),

                // === 蜀国 (Shu) ===
                ("liubei", "刘备", Faction.Shu, 4, new[] { "rende" }),
                ("guanyu", "关羽", Faction.Shu, 4, new[] { "wusheng" }),
                ("zhangfei", "张飞", Faction.Shu, 4, new[] { "paoxiao" }),
                ("zhugeliang", "诸葛亮", Faction.Shu, 3, new[] { "guanxing", "kongcheng" }),
                ("zhaoyun", "赵云", Faction.Shu, 4, new[] { "longdan" }),
                ("huangzhong", "黄忠", Faction.Shu, 4, new[] { "liegong" }),

                // === 吴国 (Wu) ===
                ("sunquan", "孙权", Faction.Wu, 4, new[] { "zhiheng" }),
                ("zhouyu", "周瑜", Faction.Wu, 3, new[] { "yingzi", "fanjian" }),
                ("lvmeng", "吕蒙", Faction.Wu, 4, new[] { "keji" }),
                ("huanggai", "黄盖", Faction.Wu, 4, new[] { "kurou" }),
                ("sunjian", "孙坚", Faction.Wu, 4, new[] { "yinghun" }),

                // === 群雄 (Qun) ===
                ("lvbu", "吕布", Faction.Qun, 4, new[] { "wushuang" }),
                ("diaochan", "貂蝉", Faction.Qun, 3, new[] { "lijian", "biyue" }),
                ("huatuo", "华佗", Faction.Qun, 3, new[] { "qingnang", "jijiu" }),
                // 董卓(8血)仅在故事模式中出现，不加入对战池
                ("yuanshao", "袁绍", Faction.Qun, 4, new[] { "qiji" }),
                ("huaxiong", "华雄", Faction.Qun, 6, new[] { "yaowu_v2" }),
                ("yanliang", "颜良", Faction.Qun, 4, new[] { "weiwu" }),
                ("wenchou", "文丑", Faction.Qun, 4, new[] { "qiangjian" }),
                ("zhanghe", "张郃", Faction.Qun, 4, new[] { "duobian" }),

                // === 官渡/讨董扩展角色 ===
                ("gaolan", "高览", Faction.Qun, 4, new[] { "mashu" }),
                ("chunyuqiong", "淳于琼", Faction.Qun, 4, new[] { "yingming" }),
                ("lijue", "李傕", Faction.Qun, 4, new[] { "jielve" }),
                ("guosi", "郭汜", Faction.Qun, 4, new[] { "xiongbao" }),
            };

            int addedCount = 0;
            foreach (var (id, name, faction, hp, skills) in storyCharactersData)
            {
                // ⭐ 检查是否在排除列表中
                if (ShouldExcludeFromBattle(id))
                {
                    Debug.Log($"[GeneralDatabase] 跳过排除角色: {name} ({id})");
                    continue;
                }

                // ⭐ 跳过已存在的角色（避免重复）
                if (generalDictionary.ContainsKey(id) || allGenerals.Any(g => g.generalId == id))
                {
                    continue;
                }

                // 创建运行时GeneralData
                GeneralData generalData = ScriptableObject.CreateInstance<GeneralData>();
                generalData.generalId = id;
                generalData.generalName = name;
                generalData.faction = faction;
                generalData.maxHP = hp;
                generalData.attackRange = 1;
                generalData.avatarPath = $"{faction}/{CharacterPinyinHelper.GetPinyinFileName(id)}";

                // ⭐ 创建技能数据
                generalData.skills = new List<SkillData>();
                foreach (var skillId in skills)
                {
                    SkillData skillData = CreateRuntimeSkillData(skillId);
                    if (skillData != null)
                    {
                        generalData.skills.Add(skillData);
                    }
                }

                // 添加到列表
                allGenerals.Add(generalData);
                runtimeGenerals.Add(generalData);  // 保持引用
                addedCount++;

                Debug.Log($"[GeneralDatabase] ✓ 添加角色: {name} [{faction}] 技能: [{string.Join(", ", skills)}]");
            }

            Debug.Log($"[GeneralDatabase] 完成！添加了 {addedCount} 个有技能的故事角色到对战池");
        }

        /// <summary>
        /// ⭐ 创建运行时技能数据
        /// </summary>
        private SkillData CreateRuntimeSkillData(string skillId)
        {
            // 技能ID到类名的映射
            var skillClassMap = new Dictionary<string, (string className, SkillType type, string displayName)>
            {
                // 基础技能
                {"jianxiong", ("ThreeKingdoms.DatabaseModule.JianXiongSkill", SkillType.Trigger, "奸雄")},
                {"ganglie", ("ThreeKingdoms.DatabaseModule.GangLieSkill", SkillType.Trigger, "刚烈")},
                {"tuxi", ("ThreeKingdoms.DatabaseModule.TuXiSkill", SkillType.Trigger, "突袭")},
                {"shensu", ("ThreeKingdoms.DatabaseModule.ShenSuSkill", SkillType.Active, "神速")},
                {"rende", ("ThreeKingdoms.DatabaseModule.RenDeSkill", SkillType.Active, "仁德")},
                {"wusheng", ("ThreeKingdoms.DatabaseModule.WuShengSkill", SkillType.Active, "武圣")},
                {"paoxiao", ("ThreeKingdoms.DatabaseModule.PaoXiaoSkill", SkillType.Passive, "咆哮")},
                {"guanxing", ("ThreeKingdoms.DatabaseModule.GuanXingSkill", SkillType.Trigger, "观星")},
                {"kongcheng", ("ThreeKingdoms.DatabaseModule.KongChengSkill", SkillType.Passive, "空城")},
                {"longdan", ("ThreeKingdoms.DatabaseModule.LongDanSkill", SkillType.Active, "龙胆")},
                {"liegong", ("ThreeKingdoms.DatabaseModule.LieGongSkill", SkillType.Trigger, "烈弓")},
                {"zhiheng", ("ThreeKingdoms.DatabaseModule.ZhiHengSkill", SkillType.Active, "制衡")},
                {"yingzi", ("ThreeKingdoms.DatabaseModule.YingZiSkill", SkillType.Trigger, "英姿")},
                {"fanjian", ("ThreeKingdoms.DatabaseModule.FanJianSkill", SkillType.Active, "反间")},
                {"keji", ("ThreeKingdoms.DatabaseModule.KeJiSkill", SkillType.Trigger, "克己")},
                {"kurou", ("ThreeKingdoms.DatabaseModule.KuRouSkill", SkillType.Active, "苦肉")},
                {"lijian", ("ThreeKingdoms.DatabaseModule.LiJianSkill", SkillType.Active, "离间")},
                {"biyue", ("ThreeKingdoms.DatabaseModule.BiYueSkill", SkillType.Trigger, "闭月")},
                {"qingnang", ("ThreeKingdoms.DatabaseModule.QingNangSkill", SkillType.Active, "青囊")},
                {"jijiu", ("ThreeKingdoms.DatabaseModule.JiJiuSkill", SkillType.Active, "急救")},
                {"jiuchi", ("ThreeKingdoms.DatabaseModule.JiuChiSkill", SkillType.Active, "酒池")},
                {"roulin", ("ThreeKingdoms.DatabaseModule.RouLinSkill", SkillType.Passive, "肉林")},
                {"benghuai", ("ThreeKingdoms.DatabaseModule.BengHuaiSkill", SkillType.Trigger, "崩坏")},

                // 官渡/讨董技能
                {"weiwu", ("ThreeKingdoms.DatabaseModule.WeiWuSkill", SkillType.Active, "威武")},
                {"qiangjian", ("ThreeKingdoms.DatabaseModule.QiangJianSkill", SkillType.Active, "强健")},
                {"duobian", ("ThreeKingdoms.DatabaseModule.DuoBianSkill", SkillType.Passive, "多变")},
                {"mashu", ("ThreeKingdoms.DatabaseModule.MaShuSkill", SkillType.Passive, "马术")},
                {"duanliang", ("ThreeKingdoms.DatabaseModule.DuanLiangSkill", SkillType.Active, "断粮")},
                {"yingming", ("ThreeKingdoms.DatabaseModule.YingMingSkill", SkillType.Trigger, "英明")},
                {"qiji", ("ThreeKingdoms.DatabaseModule.QiJiSkill", SkillType.Active, "齐击")},
                {"yaowu_v2", ("ThreeKingdoms.DatabaseModule.YaoWuV2Skill", SkillType.Trigger, "耀武")},
                {"wushuang", ("ThreeKingdoms.DatabaseModule.WuShuangSkill", SkillType.Passive, "无双")},
                {"yinghun", ("ThreeKingdoms.DatabaseModule.YingHunSkill", SkillType.Active, "英魂")},
                {"xueyi", ("ThreeKingdoms.DatabaseModule.XueYiSkill", SkillType.Trigger, "血裔")},
                {"jiuchiroulin", ("ThreeKingdoms.DatabaseModule.JiuChiRouLinSkill", SkillType.Active, "酒池肉林")},
                {"jielve", ("ThreeKingdoms.DatabaseModule.JieLveSkill", SkillType.Trigger, "劫掠")},
                {"xiongbao", ("ThreeKingdoms.DatabaseModule.XiongBaoSkill", SkillType.Trigger, "凶暴")},
            };

            if (!skillClassMap.TryGetValue(skillId, out var skillInfo))
            {
                Debug.LogWarning($"[GeneralDatabase] 未知技能ID: {skillId}");
                return null;
            }

            SkillData skillData = ScriptableObject.CreateInstance<SkillData>();
            skillData.skillId = skillId;
            skillData.skillName = skillInfo.displayName;
            skillData.skillType = skillInfo.type;
            skillData.skillClassName = skillInfo.className;

            return skillData;
        }

        /// <summary>
        /// 构建字典以便快速查找
        /// </summary>
        private void BuildDictionary()
        {
            generalDictionary.Clear();
            
            foreach (var general in allGenerals)
            {
                if (general == null) continue;
                
                if (generalDictionary.ContainsKey(general.generalId))
                {
                    Debug.LogError($"重复的武将ID: {general.generalId}");
                    continue;
                }
                
                generalDictionary[general.generalId] = general;
            }
        }

        /// <summary>
        /// 验证所有数据
        /// </summary>
        private void ValidateData()
        {
            int validCount = 0;
            int invalidCount = 0;

            foreach (var general in allGenerals)
            {
                if (general == null)
                {
                    invalidCount++;
                    continue;
                }

                if (general.Validate())
                {
                    validCount++;
                }
                else
                {
                    invalidCount++;
                }
            }

            Debug.Log($"武将数据验证完成: {validCount} 个有效, {invalidCount} 个无效");
        }

        /// <summary>
        /// 通过ID获取武将数据
        /// </summary>
        public GeneralData GetGeneralById(string generalId)
        {
            if (string.IsNullOrEmpty(generalId)) return null;

            // ⭐ 清理ID：移除 _story 等后缀
            string cleanId = generalId.Replace("_story", "").ToLower();

            // 先尝试原始ID
            if (generalDictionary.TryGetValue(generalId, out GeneralData data))
            {
                return data;
            }

            // ⭐ 再尝试清理后的ID
            if (generalDictionary.TryGetValue(cleanId, out data))
            {
                return data;
            }

            // ⭐ 尝试不区分大小写查找
            foreach (var kvp in generalDictionary)
            {
                if (kvp.Key.ToLower() == cleanId)
                {
                    return kvp.Value;
                }
            }

            Debug.LogWarning($"未找到武将: {generalId} (清理后: {cleanId})");
            return null;
        }

        /// <summary>
        /// 通过名称获取武将数据
        /// </summary>
        public GeneralData GetGeneralByName(string generalName)
        {
            return allGenerals.FirstOrDefault(g => g.generalName == generalName);
        }

        /// <summary>
        /// 获取指定阵营的所有武将
        /// </summary>
        public List<GeneralData> GetGeneralsByFaction(Faction faction)
        {
            return allGenerals.Where(g => g.faction == faction).ToList();
        }

        /// <summary>
        /// 随机获取武将
        /// </summary>
        public GeneralData GetRandomGeneral()
        {
            if (allGenerals.Count == 0) return null;
            return allGenerals[Random.Range(0, allGenerals.Count)];
        }

        /// <summary>
        /// 随机获取指定数量的武将
        /// </summary>
        public List<GeneralData> GetRandomGenerals(int count, bool allowDuplicates = false)
        {
            List<GeneralData> result = new List<GeneralData>();
            List<GeneralData> available = new List<GeneralData>(allGenerals);

            for (int i = 0; i < count && available.Count > 0; i++)
            {
                int index = Random.Range(0, available.Count);
                result.Add(available[index]);

                if (!allowDuplicates)
                {
                    available.RemoveAt(index);
                }
            }

            return result;
        }

        /// <summary>
        /// 获取所有武将数量
        /// </summary>
        public int GetGeneralCount()
        {
            return allGenerals.Count;
        }

        /// <summary>
        /// 重新加载数据（编辑器用）
        /// </summary>
        [ContextMenu("Reload All Generals")]
        public void ReloadAllGenerals()
        {
            allGenerals.Clear();
            generalDictionary.Clear();
            Initialize();
        }
    }
}
