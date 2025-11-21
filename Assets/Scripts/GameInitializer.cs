using System.Collections.Generic;
using ThreeKingdoms.AI;
using ThreeKingdoms.DatabaseModule;  // ⭐ 引入数据库模块
using ThreeKingdoms.UI;
using UnityEngine;

namespace ThreeKingdoms
{
    /// <summary>
    /// 游戏初始化器 - 用于Demo测试
    /// </summary>
    public class GameInitializer : MonoBehaviour
    {
        [Header("游戏设置")]
        public int playerCount = 3;  // 推荐2-4人
        public bool autoStart = true;

        [Header("玩家预制体")]
        public GameObject playerPrefab;

        [Header("AI设置")]
        [Range(0, 3)]
        public int defaultAILevel = 1;  // 默认AI等级
        public float aiThinkingTime = 1.5f;  // AI思考时间

        [Header("武将数据设置")]
        public bool useGeneralData = true;  // 是否使用武将数据系统

        private void Start()
        {
            if (autoStart)
            {
                InitializeGame();
            }
        }

        /// <summary>
        /// 初始化游戏
        /// </summary>
        public void InitializeGame()
        {
            Debug.Log("========== 初始化游戏 ==========");

            // 创建玩家
            CreatePlayers();

            // 初始化牌堆
            if (DeckManager.Instance != null)
            {
                DeckManager.Instance.InitializeDeck();
            }

            // 创建UI
            if (BattleUI.Instance != null && BattleManager.Instance.players.Count > 0)
            {
                Player localPlayer = BattleManager.Instance.players[0]; // 第一个玩家作为本地玩家
                BattleUI.Instance.InitializePlayers(BattleManager.Instance.players, localPlayer);
            }

            // 延迟开始游戏
            Invoke(nameof(StartBattle), 1f);
        }

        /// <summary>
        /// 创建玩家 - 完整版(支持武将数据和AI)
        /// </summary>
        private void CreatePlayers()
        {
            List<Player> players = new List<Player>();

            // ⭐ 尝试获取武将数据
            List<DatabaseModule.GeneralData> selectedGenerals = null;
            if (useGeneralData && DatabaseModule.GeneralDatabase.Instance != null)
            {
                int availableCount = GeneralDatabase.Instance.GetGeneralCount();
                if (availableCount > 0)
                {
                    selectedGenerals = GeneralDatabase.Instance.GetRandomGenerals(playerCount, false);
                    Debug.Log($"从数据库选择了 {selectedGenerals.Count} 个武将");
                }
                else
                {
                    Debug.LogWarning("武将数据库为空，将使用默认配置");
                }
            }

            for (int i = 0; i < playerCount; i++)
            {
                // 创建玩家GameObject
                GameObject playerObj;
                if (playerPrefab != null)
                {
                    playerObj = Instantiate(playerPrefab);
                }
                else
                {
                    playerObj = new GameObject($"Player_{i + 1}");
                    playerObj.AddComponent<Player>();
                }

                // 获取Player组件
                Player player = playerObj.GetComponent<Player>();
                if (player != null)
                {
                    // 设置基础信息
                    player.playerName = $"玩家{i + 1}";

                    // ⭐ 使用武将数据初始化（如果有）
                    if (selectedGenerals != null && i < selectedGenerals.Count && selectedGenerals[i] != null)
                    {
                        // 使用武将数据
                        player.InitializeFromGeneralData(selectedGenerals[i]);
                        Debug.Log($"✓ {player.playerName} 使用武将: {player.generalName} [{player.faction}] HP:{player.maxHP}");
                    }
                    else
                    {
                        // 使用默认配置
                        player.generalName = GetRandomGeneralName(i);
                        player.faction = (Faction)(i % 4);
                        player.maxHP = Random.Range(3, 5);
                        player.currentHP = player.maxHP;
                        Debug.LogWarning($"⚠ {player.playerName} 使用默认配置: {player.generalName}");
                    }

                    // ⭐ AI设置: 第一个玩家是人类,其他都是AI
                    if (i > 0)
                    {
                        // 设置为AI玩家
                        player.isAI = true;

                        // 添加AI控制器
                        AIPlayer aiController = playerObj.AddComponent<AIPlayer>();
                        aiController.controlledPlayer = player;

                        // AI配置
                        aiController.aiLevel = defaultAILevel;      // 使用Inspector设置的AI等级
                        aiController.thinkingTime = aiThinkingTime; // 使用Inspector设置的思考时间
                        aiController.attackWeight = 1.0f;           // 攻击倾向
                        aiController.healWeight = 1.5f;             // 治疗倾向
                        aiController.saveWeight = 0.8f;             // 保牌倾向

                        // 关联AI控制器
                        player.aiController = aiController;

                        Debug.Log($"创建AI玩家: {player.playerName} ({player.generalName}) - HP:{player.maxHP} [AI等级:{aiController.aiLevel}]");
                    }
                    else
                    {
                        // 第一个玩家是人类
                        player.isAI = false;
                        Debug.Log($"创建人类玩家: {player.playerName} ({player.generalName}) - HP:{player.maxHP}");
                    }

                    players.Add(player);
                }
            }

            // 设置到BattleManager
            BattleManager.Instance.players = players;

            Debug.Log($"========== 创建了 {players.Count} 个玩家 ==========");
        }

        /// <summary>
        /// 获取随机武将名（备用方案）
        /// </summary>
        private string GetRandomGeneralName(int index)
        {
            string[] generalNames = new string[]
            {
                "刘备", "关羽", "张飞", "诸葛亮",
                "曹操", "夏侯惇", "司马懿", "许褚",
                "孙权", "周瑜", "甘宁", "吕蒙",
                "吕布", "貂蝉", "华佗", "张角"
            };

            return generalNames[index % generalNames.Length];
        }

        /// <summary>
        /// 开始战斗
        /// </summary>
        private void StartBattle()
        {
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.StartGame();
            }
        }

        /// <summary>
        /// 测试使用【杀】
        /// </summary>
        [ContextMenu("Test Use Slash")]
        public void TestUseSlash()
        {
            if (BattleManager.Instance.players.Count >= 2)
            {
                Player attacker = BattleManager.Instance.players[0];
                Player target = BattleManager.Instance.players[1];

                // 给攻击者一张【杀】
                Card slash = new Card("杀", CardType.Basic, CardSuit.Spade, 7);
                attacker.handCards.Add(slash);

                // 使用【杀】
                BattleManager.Instance.UseSlash(attacker, target, slash);
            }
        }

        /// <summary>
        /// 测试使用【桃】
        /// </summary>
        [ContextMenu("Test Use Peach")]
        public void TestUsePeach()
        {
            if (BattleManager.Instance.players.Count >= 1)
            {
                Player player = BattleManager.Instance.players[0];

                // 先受到伤害
                player.TakeDamage(1);

                // 给玩家一张【桃】
                Card peach = new Card("桃", CardType.Basic, CardSuit.Heart, 3);
                player.handCards.Add(peach);

                // 使用【桃】
                BattleManager.Instance.UsePeach(player, peach);
            }
        }

        /// <summary>
        /// 测试技能系统
        /// </summary>
        [ContextMenu("Test Skill System")]
        public void TestSkillSystem()
        {
            Debug.Log("========== 测试技能系统 ==========");

            if (BattleManager.Instance.players.Count == 0)
            {
                Debug.LogError("没有玩家!");
                return;
            }

            foreach (var player in BattleManager.Instance.players)
            {
                Debug.Log($"--- {player.playerName} ({player.generalName}) ---");
                Debug.Log($"  阵营: {player.faction}");
                Debug.Log($"  体力: {player.currentHP}/{player.maxHP}");
                Debug.Log($"  技能数量: {player.skills.Count}");

                foreach (var skill in player.skills)
                {
                    if (skill != null)
                    {
                        Debug.Log($"  ✓ 技能: {skill.SkillData.skillName} - {skill.GetDescription()}");
                    }
                }

                if (player.skills.Count == 0)
                {
                    Debug.LogWarning($"  ⚠ {player.generalName} 没有技能!");
                }
            }

            Debug.Log("========================================");
        }
    }
}