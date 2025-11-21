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

        private Dictionary<string, GeneralData> generalDictionary = new Dictionary<string, GeneralData>();

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

            BuildDictionary();
            ValidateData();
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

            allGenerals.AddRange(loadedGenerals);
            Debug.Log($"从Resources加载了 {loadedGenerals.Length} 个武将");
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
            if (generalDictionary.TryGetValue(generalId, out GeneralData data))
            {
                return data;
            }

            Debug.LogWarning($"未找到武将: {generalId}");
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
