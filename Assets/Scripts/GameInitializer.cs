using UnityEngine;
using System.Collections.Generic;
using ThreeKingdoms.UI;
using ThreeKingdoms.AI;  // ⭐ AI支持

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
        /// 创建玩家 - 完整版(支持AI)
        /// </summary>
        private void CreatePlayers()
        {
            List<Player> players = new List<Player>();

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
                    player.generalName = GetRandomGeneralName(i);
                    player.faction = (Faction)(i % 4);
                    player.maxHP = Random.Range(3, 5);
                    player.currentHP = player.maxHP;

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

            Debug.Log($"========== 玩家创建完成 ==========");
            Debug.Log($"共 {players.Count} 个玩家 (1个人类, {players.Count - 1}个AI)");
        }

        /// <summary>
        /// 获取随机武将名
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

        // ============ 测试方法 (Context Menu) ============

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
        /// 测试AI行动
        /// </summary>
        [ContextMenu("Test AI Action")]
        public void TestAIAction()
        {
            if (BattleManager.Instance.players.Count >= 2)
            {
                Player aiPlayer = BattleManager.Instance.players[1];
                if (aiPlayer.isAI && aiPlayer.aiController != null)
                {
                    StartCoroutine(aiPlayer.aiController.ExecuteAITurn());
                }
            }
        }

        /// <summary>
        /// 给所有玩家添加测试卡牌
        /// </summary>
        [ContextMenu("Give Test Cards")]
        public void GiveTestCards()
        {
            foreach (var player in BattleManager.Instance.players)
            {
                // 给每个玩家添加一些测试卡牌
                player.handCards.Add(new Card("杀", CardType.Basic, CardSuit.Spade, 7));
                player.handCards.Add(new Card("桃", CardType.Basic, CardSuit.Heart, 3));
                player.handCards.Add(new Card("闪", CardType.Basic, CardSuit.Diamond, 6));

                Debug.Log($"{player.playerName} 获得了测试卡牌");
            }

            // 更新UI
            if (BattleUI.Instance != null)
            {
                Player currentPlayer = BattleManager.Instance.GetCurrentPlayer();
                BattleUI.Instance.UpdateHandCards(currentPlayer.handCards);
            }
        }

        /// <summary>
        /// 切换AI等级
        /// </summary>
        [ContextMenu("Increase AI Level")]
        public void IncreaseAILevel()
        {
            foreach (var player in BattleManager.Instance.players)
            {
                if (player.isAI && player.aiController != null)
                {
                    player.aiController.aiLevel = (player.aiController.aiLevel + 1) % 4;
                    Debug.Log($"{player.playerName} AI等级调整为: {player.aiController.aiLevel}");
                }
            }
        }
    }
}