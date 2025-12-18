using UnityEngine;
using UnityEditor;
using ThreeKingdoms;
using ThreeKingdoms.DatabaseModule;
using System.Collections.Generic;

namespace ThreeKingdoms.Editor
{
    /// <summary>
    /// 故事模式武将创建器
    /// 批量创建故事模式所需的武将数据
    /// </summary>
    public class StoryModeGeneralCreator : EditorWindow
    {
        [MenuItem("三国演义/创建故事模式武将/赤壁之战武将")]
        public static void CreateChiBiGenerals()
        {
            // 确保文件夹存在
            EnsureFolderExists("Assets/Resources/Data/Generals/Wei");
            EnsureFolderExists("Assets/Resources/Data/Generals/Shu");
            EnsureFolderExists("Assets/Resources/Data/Generals/Wu");
            EnsureFolderExists("Assets/Resources/Data/Generals/Qun");

            int created = 0;

            // ========== 赤壁之战相关武将 ==========

            // 蜀国（刘备、关羽、张飞已存在）
            created += CreateGeneralIfNotExists("zhugeliang", "诸葛亮", Faction.Shu, 3, "卧龙先生，足智多谋");
            created += CreateGeneralIfNotExists("zhaoyun", "赵云", Faction.Shu, 4, "常山赵子龙，浑身是胆");

            // 吴国（孙权已存在）
            created += CreateGeneralIfNotExists("zhouyu", "周瑜", Faction.Wu, 3, "美周郎，江东都督");
            created += CreateGeneralIfNotExists("lvmeng", "吕蒙", Faction.Wu, 4, "士别三日，刮目相看");
            created += CreateGeneralIfNotExists("huanggai", "黄盖", Faction.Wu, 4, "苦肉计，诈降曹操");
            created += CreateGeneralIfNotExists("lusu", "鲁肃", Faction.Wu, 3, "忠厚长者，孙刘联盟推动者");
            created += CreateGeneralIfNotExists("chengpu", "程普", Faction.Wu, 4, "三朝元老，东吴老将");

            // 魏国（曹操已存在）
            created += CreateGeneralIfNotExists("xiahoudun", "夏侯惇", Faction.Wei, 4, "独眼将军，拔矢啖睛");
            created += CreateGeneralIfNotExists("xiahouyuan", "夏侯渊", Faction.Wei, 4, "妙才将军，神速用兵");
            created += CreateGeneralIfNotExists("zhangliao", "张辽", Faction.Wei, 4, "威震逍遥津");
            created += CreateGeneralIfNotExists("jianggan", "蒋干", Faction.Wei, 3, "曹操说客，盗书误事");
            created += CreateGeneralIfNotExists("xiahoujie", "夏侯杰", Faction.Wei, 3, "曹操护卫，闻声丧胆");

            // 群雄/特殊
            created += CreateGeneralIfNotExists("zhangzhao", "张昭", Faction.Wu, 3, "东吴重臣，主和派领袖");
            created += CreateGeneralIfNotExists("caojun_cavalry", "曹军骑兵", Faction.Wei, 2, "曹操麾下骑兵");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[StoryModeGeneralCreator] 赤壁之战武将创建完成，新创建 {created} 个");
            EditorUtility.DisplayDialog("完成", $"赤壁之战武将创建完成！\n新创建: {created} 个武将\n\n包含：诸葛亮、赵云、周瑜、吕蒙、黄盖、鲁肃、程普、夏侯惇、夏侯渊、张辽、蒋干、夏侯杰、张昭等", "确定");
        }

        [MenuItem("三国演义/创建故事模式武将/董卓讨伐战武将")]
        public static void CreateDongZhuoGenerals()
        {
            EnsureFolderExists("Assets/Resources/Data/Generals/Qun");

            int created = 0;

            // 董卓讨伐战相关武将（暂不创建，之后再加）
            // created += CreateGeneralIfNotExists("dongzhuo", "董卓", Faction.Qun, 6, "暴虐的凉州军阀");
            // created += CreateGeneralIfNotExists("lvbu", "吕布", Faction.Qun, 5, "无双战神");
            // created += CreateGeneralIfNotExists("huaxiong", "华雄", Faction.Qun, 4, "虎牢关守将");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("提示", "董卓讨伐战武将暂未添加，之后再加", "确定");
        }

        [MenuItem("三国演义/创建故事模式武将/官渡之战武将")]
        public static void CreateGuanDuGenerals()
        {
            EnsureFolderExists("Assets/Resources/Data/Generals/Qun");

            int created = 0;

            // 官渡之战相关武将（暂不创建，之后再加）
            // created += CreateGeneralIfNotExists("yuanshao", "袁绍", Faction.Qun, 4, "四世三公");
            // created += CreateGeneralIfNotExists("yanliang", "颜良", Faction.Qun, 4, "河北名将");
            // created += CreateGeneralIfNotExists("wenchou", "文丑", Faction.Qun, 4, "河北名将");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("提示", "官渡之战武将暂未添加，之后再加", "确定");
        }

        private static int CreateGeneralIfNotExists(string id, string name, Faction faction, int maxHP, string description)
        {
            string factionFolder = GetFactionFolder(faction);
            string path = $"Assets/Resources/Data/Generals/{factionFolder}/{id}.asset";

            // 检查是否已存在
            if (AssetDatabase.LoadAssetAtPath<GeneralData>(path) != null)
            {
                Debug.Log($"[StoryModeGeneralCreator] 武将已存在: {name} ({id})");
                return 0;
            }

            // 创建新武将
            GeneralData general = ScriptableObject.CreateInstance<GeneralData>();
            general.generalId = id;
            general.generalName = name;
            general.faction = faction;
            general.maxHP = maxHP;
            general.attackRange = 1;
            general.description = description;
            general.skills = new List<SkillData>();

            AssetDatabase.CreateAsset(general, path);
            Debug.Log($"[StoryModeGeneralCreator] 创建武将: {name} ({id}) -> {path}");

            return 1;
        }

        private static string GetFactionFolder(Faction faction)
        {
            switch (faction)
            {
                case Faction.Wei: return "Wei";
                case Faction.Shu: return "Shu";
                case Faction.Wu: return "Wu";
                case Faction.Qun: return "Qun";
                default: return "Other";
            }
        }

        private static void EnsureFolderExists(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string[] folders = path.Split('/');
                string currentPath = folders[0];

                for (int i = 1; i < folders.Length; i++)
                {
                    string newPath = currentPath + "/" + folders[i];
                    if (!AssetDatabase.IsValidFolder(newPath))
                    {
                        AssetDatabase.CreateFolder(currentPath, folders[i]);
                    }
                    currentPath = newPath;
                }
            }
        }

        [MenuItem("三国演义/列出所有武将")]
        public static void ListAllGenerals()
        {
            GeneralData[] allGenerals = Resources.LoadAll<GeneralData>("Data/Generals");

            Debug.Log($"========== 武将列表 ({allGenerals.Length}个) ==========");

            foreach (var general in allGenerals)
            {
                Debug.Log($"  [{general.faction}] {general.generalName} (ID: {general.generalId}) HP:{general.maxHP}");
            }

            Debug.Log("==========================================");
        }
    }
}
