using UnityEngine;
using System.Collections.Generic;

namespace ThreeKingdoms.DatabaseModule
{
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

        [Tooltip("武将描述")]
        [TextArea(3, 5)]
        public string description;

        [Header("音效（可选）")]
        public AudioClip[] voiceLines;

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