using UnityEngine;
using System.Collections.Generic;

namespace ThreeKingdoms.DatabaseModule
{
    /// <summary>
    /// ⭐ 角色ID到拼音文件名映射工具
    /// </summary>
    public static class CharacterPinyinHelper
    {
        private static readonly Dictionary<string, string> pinyinMap = new Dictionary<string, string>
        {
            // 蜀国 (Shu)
            {"liubei", "LiuBei"},
            {"guanyu", "GuanYu"},
            {"zhangfei", "ZhangFei"},
            {"zhugeliang", "ZhuGeLiang"},
            {"zhaoyun", "ZhaoYun"},
            {"huangzhong", "HuangZhong"},
            {"mifang", "MiFang"},
            {"mifuren", "MiFuRen"},

            // 吴国 (Wu)
            {"sunquan", "SunQuan"},
            {"zhouyu", "ZhouYu"},
            {"lvmeng", "LvMeng"},
            {"huanggai", "HuangGai"},
            {"lusu", "LuSu"},
            {"chengpu", "ChengPu"},
            {"zhangzhao", "ZhangZhao"},

            // 魏国 (Wei)
            {"caocao", "CaoCao"},
            {"xiahoudun", "XiaHouDun"},
            {"xiahouyuan", "XiaHouYuan"},
            {"zhangliao", "ZhangLiao"},
            {"xiahoujie", "XiaHouJie"},
            {"jianggan", "JiangGan"},
            {"caojun_cavalry", "CaoJunQiBing"},
            {"caojunqibing", "CaoJunQiBing"},

            // 群雄 (Qun)
            {"lvbu", "LvBu"},
            {"dongzhuo", "DongZhuo"},
            {"huaxiong", "HuaXiong"},
            {"yuanshao", "YuanShao"},
            {"yanliang", "YanLiang"},
            {"wenchou", "WenChou"},
            {"diaochan", "DiaoChan"},
        };

        /// <summary>
        /// 将角色ID转换为拼音文件名格式
        /// </summary>
        public static string GetPinyinFileName(string generalId)
        {
            if (string.IsNullOrEmpty(generalId)) return generalId;

            string cleanId = generalId.ToLower().Replace("_story", "");
            if (pinyinMap.TryGetValue(cleanId, out string pinyinName))
            {
                return pinyinName;
            }

            // 如果映射表没有，返回首字母大写的版本
            return char.ToUpper(cleanId[0]) + cleanId.Substring(1);
        }

        /// <summary>
        /// 尝试多种路径加载角色头像
        /// </summary>
        public static Sprite LoadCharacterSprite(string generalId, Faction faction)
        {
            if (string.IsNullOrEmpty(generalId)) return null;

            string pinyinName = GetPinyinFileName(generalId);
            string factionName = faction.ToString();

            // 尝试多种路径
            string[] paths = new string[]
            {
                $"Sprites/Characters/{factionName}/{pinyinName}",   // 优先: Shu/LiuBei
                $"Sprites/Characters/{pinyinName}",                  // 根目录: LiuBei
                $"Sprites/Characters/{factionName}/{generalId}",    // 小写: Shu/liubei
                $"Sprites/Characters/{generalId}",                   // 根目录小写
            };

            foreach (string path in paths)
            {
                Sprite sprite = Resources.Load<Sprite>(path);
                if (sprite != null)
                {
                    return sprite;
                }
            }

            Debug.LogWarning($"[CharacterPinyinHelper] 未找到头像: {generalId} (尝试路径: {paths[0]})");
            return null;
        }
    }

    /// <summary>
    /// 武将数据（ScriptableObject）
    /// 用于在Unity编辑器中创建和编辑武将数据
    /// </summary>
    [CreateAssetMenu(fileName = "New General", menuName = "Three Kingdoms/General Data")]
    public class GeneralData : ScriptableObject
    {
        [Header("基础信息")]
        [Tooltip("武将ID，用于唯一标识")]
        public string generalId;

        [Tooltip("武将名称")]
        public string generalName;

        [Tooltip("武将称号")]
        public string title;

        [Tooltip("阵营")]
        public Faction faction;

        [Header("属性")]
        [Tooltip("体力上限")]
        [Range(1, 10)]
        public int maxHP = 4;

        [Tooltip("初始攻击范围")]
        [Range(1, 5)]
        public int attackRange = 1;

        [Header("技能")]
        [Tooltip("武将技能列表（通常1-2个）")]
        public List<SkillData> skills = new List<SkillData>();

        [Header("UI显示")]
        [Tooltip("武将头像（可选）")]
        public Sprite avatar;

        [Tooltip("头像路径（当avatar为空时使用），相对于Resources/Avatars/，例如: Wei/CaoCao")]
        public string avatarPath = "";

        [Tooltip("武将描述")]
        [TextArea(3, 5)]
        public string description;

        [Header("音效（可选）")]
        public AudioClip[] voiceLines;

        /// <summary>
        /// 获取武将头像
        /// </summary>
        public Sprite GetAvatar()
        {
            // 优先使用直接引用的avatar
            if (avatar != null)
            {
                return avatar;
            }

            // 如果有指定路径，尝试从Resources加载
            if (!string.IsNullOrEmpty(avatarPath))
            {
                Sprite loadedSprite = Resources.Load<Sprite>($"Sprites/Characters/{avatarPath}");
                if (loadedSprite != null)
                {
                    return loadedSprite;
                }
                else
                {
                    Debug.LogWarning($"[{generalName}] 无法从路径加载头像: Sprites/Characters/{avatarPath}");
                }
            }

            // ⭐ 使用共享的拼音映射工具加载
            Sprite autoSprite = CharacterPinyinHelper.LoadCharacterSprite(generalId, faction);
            return autoSprite;
        }

        /// <summary>
        /// 验证数据完整性
        /// </summary>
        public bool Validate()
        {
            if (string.IsNullOrEmpty(generalId))
            {
                Debug.LogError($"武将 {generalName} 缺少 generalId!");
                return false;
            }

            if (string.IsNullOrEmpty(generalName))
            {
                Debug.LogError($"武将 {generalId} 缺少 generalName!");
                return false;
            }

            if (maxHP <= 0)
            {
                Debug.LogError($"武将 {generalName} 的 maxHP 必须大于0!");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 创建武将实例
        /// </summary>
        public Player CreatePlayerInstance()
        {
            GameObject playerObj = new GameObject($"Player_{generalName}");
            Player player = playerObj.AddComponent<Player>();

            // 设置基础属性
            player.generalName = generalName;
            player.faction = faction;
            player.maxHP = maxHP;
            player.currentHP = maxHP;
            player.attackRange = attackRange;

            // TODO: 为玩家添加技能组件
            // foreach (var skillData in skills)
            // {
            //     var skillComponent = skillData.CreateSkillInstance(player);
            // }

            return player;
        }
    }
}