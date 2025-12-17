using UnityEngine;
using System.Collections.Generic;
using ThreeKingdoms.DatabaseModule;

namespace ThreeKingdoms.Story
{
    /// <summary>
    /// 故事模式角色数据库
    /// 包含故事模式专属角色和配置
    /// </summary>
    public class StoryCharacterDatabase : MonoBehaviour
    {
        public static StoryCharacterDatabase Instance { get; private set; }

        // 故事模式角色数据缓存
        private Dictionary<string, StoryCharacterData> characters = new Dictionary<string, StoryCharacterData>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeCharacters();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeCharacters()
        {
            // ==================== 赤壁战役角色 ====================

            // 第一战：柴桑盟议
            AddCharacter(new StoryCharacterData
            {
                characterId = "sunquan_story",
                nameKey = "char_sunquan",
                faction = Faction.Wu,
                maxHP = 4,
                skills = new List<string> { "zhiheng" },
                description = "制衡 - 出牌阶段可弃任意张牌并摸等量牌"
            });

            AddCharacter(new StoryCharacterData
            {
                characterId = "lusu",
                nameKey = "char_lusu",
                faction = Faction.Wu,
                maxHP = 3,
                skills = new List<string> { "dimeng" },
                description = "缔盟 - 可令一名角色摸牌"
            });

            AddCharacter(new StoryCharacterData
            {
                characterId = "chengpu",
                nameKey = "char_chengpu",
                faction = Faction.Wu,
                maxHP = 4,
                skills = new List<string> { "laodang" },
                description = "老当 - 受到伤害后可摸一张牌"
            });

            AddCharacter(new StoryCharacterData
            {
                characterId = "zhangzhao",
                nameKey = "char_zhangzhao",
                faction = Faction.Wu,
                maxHP = 3,
                skills = new List<string> { "zhuhe" },
                description = "主和 - 回合开始时可令孙权弃一张牌",
                isStoryOnly = true
            });

            // 第二战：诸葛渡江
            AddCharacter(new StoryCharacterData
            {
                characterId = "zhugeliang_story",
                nameKey = "char_zhugeliang",
                faction = Faction.Shu,
                maxHP = 3,
                skills = new List<string> { "guanxing", "kongcheng" },
                description = "观星/空城 - 调整牌堆，无手牌时免疫杀和决斗"
            });

            AddCharacter(new StoryCharacterData
            {
                characterId = "zhouyu_story",
                nameKey = "char_zhouyu",
                faction = Faction.Wu,
                maxHP = 3,
                skills = new List<string> { "yingzi", "fanjian" },
                description = "英姿/反间 - 多摸牌，可令人猜花色"
            });

            AddCharacter(new StoryCharacterData
            {
                characterId = "lvmeng",
                nameKey = "char_lvmeng",
                faction = Faction.Wu,
                maxHP = 4,
                skills = new List<string> { "keji" },
                description = "克己 - 未使用杀则保留手牌"
            });

            // 第三战：长坂坡突围
            AddCharacter(new StoryCharacterData
            {
                characterId = "zhaoyun_story",
                nameKey = "char_zhaoyun",
                faction = Faction.Shu,
                maxHP = 4,
                skills = new List<string> { "longdan" },
                description = "龙胆 - 杀闪互换"
            });

            AddCharacter(new StoryCharacterData
            {
                characterId = "caojun_cavalry",
                nameKey = "char_caojun_cavalry",
                faction = Faction.Wei,
                maxHP = 2,
                skills = new List<string>(),
                description = "无特殊技能",
                isStoryOnly = true
            });

            // 第四战：张飞断桥
            AddCharacter(new StoryCharacterData
            {
                characterId = "zhangfei_story",
                nameKey = "char_zhangfei",
                faction = Faction.Shu,
                maxHP = 4,
                skills = new List<string> { "paoxiao" },
                description = "咆哮 - 可使用任意数量的杀"
            });

            AddCharacter(new StoryCharacterData
            {
                characterId = "xiahoujie",
                nameKey = "char_xiahoujie",
                faction = Faction.Wei,
                maxHP = 4,
                skills = new List<string> { "danlie" },
                description = "胆裂 - 体力降至2以下时不能出闪，伤害-1",
                isStoryOnly = true
            });

            // 第五战：黄盖诈降
            AddCharacter(new StoryCharacterData
            {
                characterId = "huanggai_story",
                nameKey = "char_huanggai",
                faction = Faction.Wu,
                maxHP = 4,
                skills = new List<string> { "kurou_zhaxiang" },
                description = "苦肉诈降 - 自损体力摸牌并累积诈降标记"
            });

            AddCharacter(new StoryCharacterData
            {
                characterId = "jianggan",
                nameKey = "char_jianggan",
                faction = Faction.Wei,
                maxHP = 3,
                skills = new List<string> { "daoshu" },
                description = "盗书 - 回合结束可查看一名角色手牌",
                isStoryOnly = true
            });

            // 第六战：赤壁火起
            AddCharacter(new StoryCharacterData
            {
                characterId = "liubei_story",
                nameKey = "char_liubei",
                faction = Faction.Shu,
                maxHP = 4,
                skills = new List<string> { "rende" },
                description = "仁德 - 可将手牌交给其他角色"
            });

            AddCharacter(new StoryCharacterData
            {
                characterId = "guanyu_story",
                nameKey = "char_guanyu",
                faction = Faction.Shu,
                maxHP = 4,
                skills = new List<string> { "wusheng" },
                description = "武圣 - 可将红色牌当杀使用"
            });

            AddCharacter(new StoryCharacterData
            {
                characterId = "caocao_story",
                nameKey = "char_caocao",
                faction = Faction.Wei,
                maxHP = 7, // 铁索连船加成后
                skills = new List<string> { "jianxiong" },
                description = "奸雄 - 受到伤害后可获得造成伤害的牌"
            });

            AddCharacter(new StoryCharacterData
            {
                characterId = "xiahoudun",
                nameKey = "char_xiahoudun",
                faction = Faction.Wei,
                maxHP = 4,
                skills = new List<string> { "ganglie" },
                description = "刚烈 - 受伤后判定，红色则来源弃牌或受伤"
            });

            AddCharacter(new StoryCharacterData
            {
                characterId = "xiahouyuan",
                nameKey = "char_xiahouyuan",
                faction = Faction.Wei,
                maxHP = 4,
                skills = new List<string> { "shenshu" },
                description = "神速 - 跳过阶段可视为使用杀"
            });

            AddCharacter(new StoryCharacterData
            {
                characterId = "zhangliao",
                nameKey = "char_zhangliao",
                faction = Faction.Wei,
                maxHP = 4,
                skills = new List<string> { "tuxi" },
                description = "突袭 - 摸牌阶段可改为获取他人手牌"
            });

            Debug.Log($"[StoryCharacterDatabase] 初始化完成，共 {characters.Count} 个角色");
        }

        private void AddCharacter(StoryCharacterData data)
        {
            if (!characters.ContainsKey(data.characterId))
            {
                characters[data.characterId] = data;
            }
        }

        /// <summary>
        /// 获取角色数据
        /// </summary>
        public StoryCharacterData GetCharacter(string characterId)
        {
            if (characters.TryGetValue(characterId, out var data))
            {
                return data;
            }

            // 尝试从通用武将库获取
            if (GeneralDatabase.Instance != null)
            {
                var general = GeneralDatabase.Instance.GetGeneralById(characterId);
                if (general != null)
                {
                    return new StoryCharacterData
                    {
                        characterId = general.generalId,
                        nameKey = general.generalName,
                        faction = general.faction,
                        maxHP = general.maxHP,
                        skills = new List<string>(),
                        description = general.description
                    };
                }
            }

            Debug.LogWarning($"[StoryCharacterDatabase] 未找到角色: {characterId}");
            return null;
        }

        /// <summary>
        /// 获取所有故事模式专属角色
        /// </summary>
        public List<StoryCharacterData> GetStoryOnlyCharacters()
        {
            var result = new List<StoryCharacterData>();
            foreach (var kvp in characters)
            {
                if (kvp.Value.isStoryOnly)
                {
                    result.Add(kvp.Value);
                }
            }
            return result;
        }

        /// <summary>
        /// 创建玩家实例
        /// </summary>
        public Player CreatePlayer(string characterId, bool isPlayerControlled)
        {
            var data = GetCharacter(characterId);
            if (data == null) return null;

            GameObject playerObj = new GameObject($"Player_{data.nameKey}");
            Player player = playerObj.AddComponent<Player>();

            player.generalName = LocalizationManager.Instance?.GetText(data.nameKey) ?? data.nameKey;
            player.faction = data.faction;
            player.maxHP = data.maxHP;
            player.currentHP = data.maxHP;
            player.isAI = !isPlayerControlled;

            // TODO: 添加技能组件

            return player;
        }
    }

    /// <summary>
    /// 故事模式角色数据
    /// </summary>
    [System.Serializable]
    public class StoryCharacterData
    {
        public string characterId;          // 角色ID
        public string nameKey;              // 名称本地化key
        public Faction faction;             // 阵营
        public int maxHP;                   // 体力上限
        public List<string> skills;         // 技能ID列表
        public string description;          // 描述
        public bool isStoryOnly = false;    // 是否为故事模式专属
        public string avatarPath;           // 头像路径

        public StoryCharacterData()
        {
            skills = new List<string>();
        }
    }
}
