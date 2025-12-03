using UnityEngine;
using ThreeKingdoms.DatabaseModule;

namespace ThreeKingdoms
{
    /// <summary>
    /// 游戏配置数据
    /// 使用单例模式在场景间传递游戏设置
    /// </summary>
    public class GameConfig : MonoBehaviour
    {
        public static GameConfig Instance { get; private set; }

        [Header("游戏设置")]
        public GeneralData selectedGeneral;     // 玩家选择的武将
        public bool enableIdentityMode = true;  // 是否启用身份场
        public int aiDifficulty = 1;            // AI难度 (0=简单, 1=普通, 2=困难)
        public int playerCount = 4;             // 玩家数量

        private void Awake()
        {
            // 单例模式：确保只有一个实例，且在场景切换时不销毁
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 重置为默认配置
        /// </summary>
        public void ResetToDefault()
        {
            selectedGeneral = null;
            enableIdentityMode = true;
            aiDifficulty = 1;
            playerCount = 4;
        }

        /// <summary>
        /// 获取AI难度名称
        /// </summary>
        public string GetAIDifficultyName()
        {
            switch (aiDifficulty)
            {
                case 0: return "简单";
                case 1: return "普通";
                case 2: return "困难";
                default: return "未知";
            }
        }
    }
}
