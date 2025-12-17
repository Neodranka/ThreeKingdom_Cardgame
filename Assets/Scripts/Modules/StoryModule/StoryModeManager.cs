using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using ThreeKingdoms.Story.Campaigns;

namespace ThreeKingdoms.Story
{
    /// <summary>
    /// 故事模式管理器
    /// 负责管理战役数据、进度保存和战斗启动
    /// </summary>
    public class StoryModeManager : MonoBehaviour
    {
        public static StoryModeManager Instance { get; private set; }

        [Header("战役数据")]
        [SerializeField] private List<CampaignData> campaigns;

        // 当前选择
        private int selectedCampaignIndex = 0;
        private int selectedBattleIndex = 0;

        // 存档key
        private const string SAVE_KEY = "StoryProgress";
        private const int DATA_VERSION = 4; // ⭐ 数据版本号，修改战役数据时增加此值（添加官渡/讨董详细数据）
        private const string VERSION_KEY = "StoryDataVersion";

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeCampaigns();
                LoadProgress();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 初始化战役数据
        /// </summary>
        private void InitializeCampaigns()
        {
            // ⭐ 检查数据版本，如果版本不匹配则强制重新创建
            int savedVersion = PlayerPrefs.GetInt(VERSION_KEY, 0);
            bool needsRebuild = campaigns == null || campaigns.Count == 0 || savedVersion < DATA_VERSION;

            if (needsRebuild)
            {
                Debug.Log($"[StoryMode] 重建战役数据 (旧版本:{savedVersion}, 新版本:{DATA_VERSION})");
                campaigns = CreateDefaultCampaigns();
                PlayerPrefs.SetInt(VERSION_KEY, DATA_VERSION);
                PlayerPrefs.Save();
            }

            // ⭐ 测试模式：解锁全部战役和战斗
            UnlockAllForTesting();
        }

        /// <summary>
        /// ⭐ 测试用：解锁全部战役和战斗
        /// </summary>
        private void UnlockAllForTesting()
        {
            foreach (var campaign in campaigns)
            {
                campaign.isUnlocked = true;
                foreach (var battle in campaign.battles)
                {
                    battle.isUnlocked = true;
                }
                if (campaign.storyBattles != null)
                {
                    foreach (var storyBattle in campaign.storyBattles)
                    {
                        storyBattle.isUnlocked = true;
                    }
                }
            }
            Debug.Log("[StoryMode] 测试模式：已解锁全部战役和战斗");
        }

        /// <summary>
        /// 创建默认战役数据（三大战役）
        /// </summary>
        private List<CampaignData> CreateDefaultCampaigns()
        {
            List<CampaignData> defaultCampaigns = new List<CampaignData>();

            // 战役1：讨董之战（使用详细的Taodong数据）
            CampaignData campaign1 = TaodongCampaignData.CreateCampaign();
            campaign1.isUnlocked = true; // 第一个战役默认解锁
            defaultCampaigns.Add(campaign1);

            // 战役2：官渡之战（使用详细的Guandu数据）
            CampaignData campaign2 = GuanduCampaignData.CreateCampaign();
            defaultCampaigns.Add(campaign2);

            // 战役3：赤壁之战（使用详细的ChiBi数据）
            CampaignData campaign3 = ChiBiCampaignData.CreateCampaign();
            defaultCampaigns.Add(campaign3);

            return defaultCampaigns;
        }

        #region 公共接口

        /// <summary>
        /// ⭐ 确保战役数据是最新的（场景加载时调用）
        /// </summary>
        public void EnsureCampaignsUpToDate()
        {
            int savedVersion = PlayerPrefs.GetInt(VERSION_KEY, 0);
            if (savedVersion < DATA_VERSION)
            {
                Debug.Log($"[StoryMode] 检测到数据需要更新 (当前:{savedVersion}, 需要:{DATA_VERSION})");
                campaigns = CreateDefaultCampaigns();
                PlayerPrefs.SetInt(VERSION_KEY, DATA_VERSION);
                PlayerPrefs.Save();

                // 重新加载进度到新数据
                LoadProgress();

                // 确保第一个战役解锁
                if (campaigns.Count > 0)
                {
                    campaigns[0].isUnlocked = true;
                    if (campaigns[0].battles.Count > 0)
                    {
                        campaigns[0].battles[0].isUnlocked = true;
                    }
                }
            }
        }

        /// <summary>
        /// 获取所有战役
        /// </summary>
        public List<CampaignData> GetAllCampaigns()
        {
            return campaigns;
        }

        /// <summary>
        /// 获取指定战役
        /// </summary>
        public CampaignData GetCampaign(int index)
        {
            if (index >= 0 && index < campaigns.Count)
            {
                return campaigns[index];
            }
            return null;
        }

        /// <summary>
        /// 获取当前选择的战役
        /// </summary>
        public CampaignData GetSelectedCampaign()
        {
            return GetCampaign(selectedCampaignIndex);
        }

        /// <summary>
        /// 获取当前选择的战斗
        /// </summary>
        public BattleData GetSelectedBattle()
        {
            var campaign = GetSelectedCampaign();
            if (campaign != null && selectedBattleIndex >= 0 && selectedBattleIndex < campaign.battles.Count)
            {
                return campaign.battles[selectedBattleIndex];
            }
            return null;
        }

        /// <summary>
        /// 选择战役
        /// </summary>
        public void SelectCampaign(int index)
        {
            if (index >= 0 && index < campaigns.Count)
            {
                selectedCampaignIndex = index;
                selectedBattleIndex = 0; // 重置战斗选择
                Debug.Log($"[StoryMode] 选择战役: {campaigns[index].nameKey}");
            }
        }

        /// <summary>
        /// 选择战斗
        /// </summary>
        public void SelectBattle(int index)
        {
            var campaign = GetSelectedCampaign();
            if (campaign != null && index >= 0 && index < campaign.battles.Count)
            {
                selectedBattleIndex = index;
                Debug.Log($"[StoryMode] 选择战斗: {campaign.battles[index].nameKey}");
            }
        }

        /// <summary>
        /// 开始战斗
        /// </summary>
        public void StartBattle()
        {
            var battle = GetSelectedBattle();
            if (battle == null)
            {
                Debug.LogError("[StoryMode] 没有选择战斗!");
                return;
            }

            if (!battle.isUnlocked)
            {
                Debug.LogWarning("[StoryMode] 该战斗尚未解锁!");
                return;
            }

            // 保存当前战斗信息供GameScene使用
            PlayerPrefs.SetString("StoryBattleId", battle.battleId);
            PlayerPrefs.SetString("StoryPlayerGeneral", battle.playerGeneralId);
            PlayerPrefs.SetInt("StoryDifficulty", battle.difficulty);
            PlayerPrefs.Save();

            Debug.Log($"[StoryMode] 开始战斗: {battle.battleId}");

            // 加载战斗场景
            SceneManager.LoadScene("GameScene");
        }

        /// <summary>
        /// 完成战斗
        /// </summary>
        public void CompleteBattle(string battleId, int score)
        {
            var campaign = GetSelectedCampaign();
            if (campaign == null) return;

            for (int i = 0; i < campaign.battles.Count; i++)
            {
                if (campaign.battles[i].battleId == battleId)
                {
                    campaign.battles[i].isCompleted = true;
                    if (score > campaign.battles[i].bestScore)
                    {
                        campaign.battles[i].bestScore = score;
                    }

                    // 解锁下一场战斗
                    if (i + 1 < campaign.battles.Count)
                    {
                        campaign.battles[i + 1].isUnlocked = true;
                    }
                    // 如果是最后一场，解锁下一个战役
                    else if (selectedCampaignIndex + 1 < campaigns.Count)
                    {
                        campaign.isCompleted = true;
                        campaigns[selectedCampaignIndex + 1].isUnlocked = true;
                        if (campaigns[selectedCampaignIndex + 1].battles.Count > 0)
                        {
                            campaigns[selectedCampaignIndex + 1].battles[0].isUnlocked = true;
                        }
                    }

                    SaveProgress();
                    break;
                }
            }
        }

        /// <summary>
        /// 返回主菜单
        /// </summary>
        public void ReturnToMainMenu()
        {
            SceneManager.LoadScene("MainMenu");
        }

        #endregion

        #region 存档系统

        /// <summary>
        /// 保存进度
        /// </summary>
        public void SaveProgress()
        {
            StoryProgressData progressData = new StoryProgressData
            {
                currentCampaignIndex = selectedCampaignIndex,
                currentBattleIndex = selectedBattleIndex,
                campaignProgresses = new List<CampaignProgress>()
            };

            foreach (var campaign in campaigns)
            {
                CampaignProgress cp = new CampaignProgress
                {
                    campaignId = campaign.campaignId,
                    isUnlocked = campaign.isUnlocked,
                    isCompleted = campaign.isCompleted,
                    battleProgresses = new List<BattleProgress>()
                };

                foreach (var battle in campaign.battles)
                {
                    cp.battleProgresses.Add(new BattleProgress
                    {
                        battleId = battle.battleId,
                        isUnlocked = battle.isUnlocked,
                        isCompleted = battle.isCompleted,
                        bestScore = battle.bestScore
                    });
                }

                progressData.campaignProgresses.Add(cp);
            }

            string json = JsonUtility.ToJson(progressData);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();

            Debug.Log("[StoryMode] 进度已保存");
        }

        /// <summary>
        /// 加载进度
        /// </summary>
        public void LoadProgress()
        {
            if (!PlayerPrefs.HasKey(SAVE_KEY))
            {
                Debug.Log("[StoryMode] 没有存档，使用默认进度");
                return;
            }

            string json = PlayerPrefs.GetString(SAVE_KEY);
            StoryProgressData progressData = JsonUtility.FromJson<StoryProgressData>(json);

            if (progressData == null) return;

            selectedCampaignIndex = progressData.currentCampaignIndex;
            selectedBattleIndex = progressData.currentBattleIndex;

            // 恢复战役进度
            foreach (var cp in progressData.campaignProgresses)
            {
                var campaign = campaigns.Find(c => c.campaignId == cp.campaignId);
                if (campaign != null)
                {
                    campaign.isUnlocked = cp.isUnlocked;
                    campaign.isCompleted = cp.isCompleted;

                    foreach (var bp in cp.battleProgresses)
                    {
                        var battle = campaign.battles.Find(b => b.battleId == bp.battleId);
                        if (battle != null)
                        {
                            battle.isUnlocked = bp.isUnlocked;
                            battle.isCompleted = bp.isCompleted;
                            battle.bestScore = bp.bestScore;
                        }
                    }
                }
            }

            Debug.Log("[StoryMode] 进度已加载");
        }

        /// <summary>
        /// 重置进度
        /// </summary>
        public void ResetProgress()
        {
            PlayerPrefs.DeleteKey(SAVE_KEY);
            campaigns = CreateDefaultCampaigns();
            selectedCampaignIndex = 0;
            selectedBattleIndex = 0;
            Debug.Log("[StoryMode] 进度已重置");
        }

        #endregion
    }
}
