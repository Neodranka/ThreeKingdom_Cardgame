using System.Collections.Generic;
using ThreeKingdoms.AI;
using ThreeKingdoms.DatabaseModule;  // ⭐ 引入数据库模块
using ThreeKingdoms.UI;
using ThreeKingdoms.Story;  // ⭐ 引入故事模式
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

            // ⭐ 检查是否为故事模式战斗
            string storyBattleId = PlayerPrefs.GetString("StoryBattleId", "");
            if (!string.IsNullOrEmpty(storyBattleId))
            {
                Debug.Log($"[GameInitializer] 检测到故事模式战斗: {storyBattleId}");
                InitializeStoryBattle(storyBattleId);
                return;
            }

            // ⭐ 从GameConfig读取设置
            ApplyGameConfig();

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

            // ⭐ 创建出牌动画管理器
            EnsurePlayedCardDisplayManager();

            // 延迟开始游戏
            Invoke(nameof(StartBattle), 1f);
        }

        /// <summary>
        /// ⭐ 从GameConfig应用设置
        /// </summary>
        private void ApplyGameConfig()
        {
            if (GameConfig.Instance != null)
            {
                Debug.Log("========== 从GameConfig读取设置 ==========");

                // 读取玩家数量
                if (GameConfig.Instance.playerCount > 0)
                {
                    playerCount = GameConfig.Instance.playerCount;
                    Debug.Log($"玩家数量: {playerCount}");
                }

                // 读取AI难度
                defaultAILevel = GameConfig.Instance.aiDifficulty;
                Debug.Log($"AI难度: {GameConfig.Instance.GetAIDifficultyName()} (等级 {defaultAILevel})");

                // 读取选择的武将
                if (GameConfig.Instance.selectedGeneral != null)
                {
                    Debug.Log($"玩家选择的武将: {GameConfig.Instance.selectedGeneral.generalName}");
                }
                else
                {
                    Debug.LogWarning("未选择武将，将随机分配");
                }

                Debug.Log("==========================================");
            }
            else
            {
                Debug.LogWarning("GameConfig未找到，使用默认设置");
            }
        }

        /// <summary>
        /// 创建玩家 - 完整版(支持武将数据和AI)
        /// </summary>
        private void CreatePlayers()
        {
            List<Player> players = new List<Player>();

            // ⭐ 获取玩家在GameSetup选择的武将
            GeneralData playerSelectedGeneral = GameConfig.Instance?.selectedGeneral;

            // ⭐ 获取其他AI玩家的武将数据
            List<DatabaseModule.GeneralData> aiGenerals = null;
            if (useGeneralData && DatabaseModule.GeneralDatabase.Instance != null)
            {
                int availableCount = GeneralDatabase.Instance.GetGeneralCount();
                if (availableCount > 0)
                {
                    // 获取除玩家选择武将外的其他武将给AI使用
                    aiGenerals = GeneralDatabase.Instance.GetRandomGenerals(playerCount - 1, false);

                    // 如果玩家选择了武将，确保AI不会重复选到
                    if (playerSelectedGeneral != null && aiGenerals != null)
                    {
                        aiGenerals.RemoveAll(g => g.generalId == playerSelectedGeneral.generalId);
                    }

                    Debug.Log($"为AI选择了 {aiGenerals?.Count ?? 0} 个武将");
                }
                else
                {
                    Debug.LogWarning("武将数据库为空，将使用默认配置");
                }
            }

            int aiGeneralIndex = 0;

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
                    player.playerName = (i == 0) ? "你" : $"AI{i}";

                    // ⭐ 第一个玩家使用GameConfig中选择的武将
                    if (i == 0)
                    {
                        if (playerSelectedGeneral != null)
                        {
                            player.InitializeFromGeneralData(playerSelectedGeneral);
                            Debug.Log($"✓ {player.playerName} 使用选择的武将: {player.generalName} [{player.faction}] HP:{player.maxHP}");
                        }
                        else
                        {
                            // 没有选择武将，随机分配一个
                            if (aiGenerals != null && aiGenerals.Count > 0)
                            {
                                player.InitializeFromGeneralData(aiGenerals[0]);
                                aiGenerals.RemoveAt(0);
                                Debug.Log($"✓ {player.playerName} 随机分配武将: {player.generalName} [{player.faction}] HP:{player.maxHP}");
                            }
                            else
                            {
                                player.generalName = GetRandomGeneralName(i);
                                player.faction = (Faction)(i % 4);
                                player.maxHP = Random.Range(3, 5);
                                player.currentHP = player.maxHP;
                                Debug.LogWarning($"⚠ {player.playerName} 使用默认配置: {player.generalName}");
                            }
                        }

                        // 第一个玩家是人类
                        player.isAI = false;
                        Debug.Log($"创建人类玩家: {player.playerName} ({player.generalName}) - HP:{player.maxHP}");
                    }
                    else
                    {
                        // ⭐ AI玩家使用随机武将
                        if (aiGenerals != null && aiGeneralIndex < aiGenerals.Count && aiGenerals[aiGeneralIndex] != null)
                        {
                            player.InitializeFromGeneralData(aiGenerals[aiGeneralIndex]);
                            aiGeneralIndex++;
                            Debug.Log($"✓ {player.playerName} 使用武将: {player.generalName} [{player.faction}] HP:{player.maxHP}");
                        }
                        else
                        {
                            player.generalName = GetRandomGeneralName(i);
                            player.faction = (Faction)(i % 4);
                            player.maxHP = Random.Range(3, 5);
                            player.currentHP = player.maxHP;
                            Debug.LogWarning($"⚠ {player.playerName} 使用默认配置: {player.generalName}");
                        }

                        // 设置为AI玩家
                        player.isAI = true;

                        // 添加AI控制器
                        AIPlayer aiController = playerObj.AddComponent<AIPlayer>();
                        aiController.controlledPlayer = player;

                        // ⭐ AI配置 - 使用从GameConfig读取的难度
                        aiController.aiLevel = defaultAILevel;
                        aiController.thinkingTime = aiThinkingTime;
                        aiController.attackWeight = 1.0f;
                        aiController.healWeight = 1.5f;
                        aiController.saveWeight = 0.8f;

                        // 关联AI控制器
                        player.aiController = aiController;

                        Debug.Log($"创建AI玩家: {player.playerName} ({player.generalName}) - HP:{player.maxHP} [AI等级:{aiController.aiLevel}]");
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
        /// ⭐ 确保PlayedCardDisplayManager存在
        /// </summary>
        private void EnsurePlayedCardDisplayManager()
        {
            if (PlayedCardDisplayManager.Instance == null)
            {
                // 创建PlayedCardDisplayManager
                GameObject displayManagerObj = new GameObject("PlayedCardDisplayManager");

                // 如果有Canvas，作为Canvas的子对象
                Canvas canvas = FindObjectOfType<Canvas>();
                if (canvas != null)
                {
                    displayManagerObj.transform.SetParent(canvas.transform, false);
                }

                displayManagerObj.AddComponent<PlayedCardDisplayManager>();
                Debug.Log("[GameInitializer] 创建了 PlayedCardDisplayManager");
            }
            else
            {
                Debug.Log("[GameInitializer] PlayedCardDisplayManager 已存在");
            }
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

        #region 故事模式战斗初始化

        /// <summary>
        /// ⭐ 初始化故事模式战斗
        /// </summary>
        private void InitializeStoryBattle(string battleId)
        {
            Debug.Log($"========== 初始化故事模式战斗: {battleId} ==========");

            // 获取战斗数据
            BattleData battleData = FindStoryBattle(battleId);
            if (battleData == null)
            {
                Debug.LogError($"[GameInitializer] 找不到故事战斗数据: {battleId}");
                // 清除故事模式标记，回退到普通模式
                PlayerPrefs.DeleteKey("StoryBattleId");
                ApplyGameConfig();
                CreatePlayers();
                FinishInitialization();
                return;
            }

            // 创建故事模式玩家
            CreateStoryPlayers(battleData);

            // 清除故事模式标记（避免下次重复）
            PlayerPrefs.DeleteKey("StoryBattleId");
            PlayerPrefs.DeleteKey("StoryPlayerGeneral");
            PlayerPrefs.DeleteKey("StoryDifficulty");
            PlayerPrefs.Save();

            // 完成初始化
            FinishInitialization();

            // ⭐ 确保 StoryBattleManager 存在
            if (StoryBattleManager.Instance == null)
            {
                GameObject sbmObj = new GameObject("StoryBattleManager");
                sbmObj.AddComponent<StoryBattleManager>();
                Debug.Log("[GameInitializer] 创建了 StoryBattleManager");
            }

            // 启动故事战斗管理器
            if (StoryBattleManager.Instance != null && StoryModeManager.Instance != null)
            {
                // ⭐ 从所有战役中搜索对应的 StoryBattle（不只是当前选中的战役）
                StoryBattle storyBattle = FindStoryBattleFromAllCampaigns(battleId);

                if (storyBattle != null)
                {
                    Debug.Log($"[GameInitializer] 找到 StoryBattle: {battleId}, 开场对话数: {storyBattle.openingDialogue?.Count ?? 0}");
                    StoryBattleManager.Instance.StartBattle(storyBattle);
                }
                else
                {
                    Debug.LogWarning($"[GameInitializer] 未找到 StoryBattle: {battleId}");
                }
            }
        }

        /// <summary>
        /// ⭐ 从所有战役中查找 StoryBattle
        /// </summary>
        private StoryBattle FindStoryBattleFromAllCampaigns(string battleId)
        {
            if (StoryModeManager.Instance == null) return null;

            var campaigns = StoryModeManager.Instance.GetAllCampaigns();
            foreach (var campaign in campaigns)
            {
                if (campaign.storyBattles != null)
                {
                    foreach (var storyBattle in campaign.storyBattles)
                    {
                        if (storyBattle.battleId == battleId)
                        {
                            return storyBattle;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// ⭐ 查找故事战斗数据
        /// </summary>
        private BattleData FindStoryBattle(string battleId)
        {
            if (StoryModeManager.Instance == null)
            {
                Debug.LogWarning("[GameInitializer] StoryModeManager 不存在");
                return null;
            }

            var campaigns = StoryModeManager.Instance.GetAllCampaigns();
            foreach (var campaign in campaigns)
            {
                if (campaign.battles != null)
                {
                    foreach (var battle in campaign.battles)
                    {
                        if (battle.battleId == battleId)
                        {
                            return battle;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// ⭐ 创建故事模式玩家
        /// </summary>
        private void CreateStoryPlayers(BattleData battleData)
        {
            List<Player> players = new List<Player>();

            // 创建玩家角色
            Player playerCharacter = CreateStoryPlayer(
                battleData.playerGeneralId,
                "你",
                false,  // 不是AI
                Faction.Shu  // 默认蜀国，后面会根据武将数据覆盖
            );
            if (playerCharacter != null)
            {
                players.Add(playerCharacter);
            }

            // 创建敌方角色
            int enemyIndex = 1;
            foreach (var enemyId in battleData.enemyGeneralIds)
            {
                Player enemy = CreateStoryPlayer(
                    enemyId,
                    $"敌方{enemyIndex}",
                    true,  // 是AI
                    Faction.Wei  // 默认魏国
                );
                if (enemy != null)
                {
                    players.Add(enemy);
                    enemyIndex++;
                }
            }

            // 设置到BattleManager
            BattleManager.Instance.players = players;

            Debug.Log($"[GameInitializer] 故事模式创建了 {players.Count} 个角色");
        }

        /// <summary>
        /// ⭐ 创建故事模式单个玩家
        /// </summary>
        private Player CreateStoryPlayer(string generalId, string playerName, bool isAI, Faction defaultFaction)
        {
            GameObject playerObj = new GameObject($"Player_{playerName}");
            if (playerPrefab != null)
            {
                Destroy(playerObj);
                playerObj = Instantiate(playerPrefab);
                playerObj.name = $"Player_{playerName}";
            }
            else
            {
                playerObj.AddComponent<Player>();
            }

            Player player = playerObj.GetComponent<Player>();
            if (player == null)
            {
                player = playerObj.AddComponent<Player>();
            }

            player.playerName = playerName;
            player.isAI = isAI;

            // 尝试从数据库加载武将数据
            GeneralData generalData = null;
            if (GeneralDatabase.Instance != null)
            {
                generalData = GeneralDatabase.Instance.GetGeneralById(generalId);
                if (generalData == null)
                {
                    // 尝试通过名称查找
                    generalData = GeneralDatabase.Instance.GetGeneralByName(generalId);
                }
            }

            if (generalData != null)
            {
                player.InitializeFromGeneralData(generalData);
                Debug.Log($"✓ 故事模式 {playerName}: {player.generalName} [{player.faction}] HP:{player.maxHP}");
            }
            else
            {
                // 使用默认配置
                player.generalName = GetChineseGeneralName(generalId);
                player.faction = defaultFaction;
                player.maxHP = 4;
                player.currentHP = player.maxHP;
                Debug.LogWarning($"⚠ 故事模式 {playerName}: 未找到武将数据 '{generalId}'，使用默认配置");
            }

            // 如果是AI，添加AI控制器
            if (isAI)
            {
                AIPlayer aiController = playerObj.AddComponent<AIPlayer>();
                aiController.controlledPlayer = player;
                aiController.aiLevel = defaultAILevel;
                aiController.thinkingTime = aiThinkingTime;
                player.aiController = aiController;
            }

            return player;
        }

        /// <summary>
        /// ⭐ 获取武将中文名
        /// </summary>
        private string GetChineseGeneralName(string generalId)
        {
            // 映射表
            Dictionary<string, string> nameMap = new Dictionary<string, string>
            {
                // 蜀国
                {"liubei", "刘备"}, {"guanyu", "关羽"}, {"zhangfei", "张飞"},
                {"zhugeliang", "诸葛亮"}, {"zhaoyun", "赵云"}, {"huangzhong", "黄忠"},
                // 魏国
                {"caocao", "曹操"}, {"xiahoudun", "夏侯惇"}, {"xiahouyuan", "夏侯渊"},
                {"zhangliao", "张辽"}, {"xuzhu", "许褚"}, {"simayi", "司马懿"},
                // 吴国
                {"sunquan", "孙权"}, {"zhouyu", "周瑜"}, {"lvmeng", "吕蒙"},
                {"huanggai", "黄盖"}, {"ganning", "甘宁"}, {"luxun", "陆逊"},
                // 群雄
                {"lvbu", "吕布"}, {"dongzhuo", "董卓"}, {"huaxiong", "华雄"},
                {"yuanshao", "袁绍"}, {"yanliang", "颜良"}, {"wenchou", "文丑"},
                {"diaochan", "貂蝉"}, {"huatuo", "华佗"}
            };

            string lowerKey = generalId.ToLower().Replace("_story", "").Replace("_", "");
            if (nameMap.TryGetValue(lowerKey, out string name))
            {
                return name;
            }

            return generalId;
        }

        /// <summary>
        /// ⭐ 完成初始化（公共部分）
        /// </summary>
        private void FinishInitialization()
        {
            // 初始化牌堆
            if (DeckManager.Instance != null)
            {
                DeckManager.Instance.InitializeDeck();
            }

            // 创建UI
            if (BattleUI.Instance != null && BattleManager.Instance.players.Count > 0)
            {
                Player localPlayer = BattleManager.Instance.players[0];
                BattleUI.Instance.InitializePlayers(BattleManager.Instance.players, localPlayer);
            }

            // 确保PlayedCardDisplayManager存在
            EnsurePlayedCardDisplayManager();

            // 延迟开始游戏
            Invoke(nameof(StartBattle), 1f);
        }

        #endregion
    }
}