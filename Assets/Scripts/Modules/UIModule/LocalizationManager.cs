using UnityEngine;
using System.Collections.Generic;

namespace ThreeKingdoms
{
    /// <summary>
    /// 支持的语言
    /// </summary>
    public enum Language
    {
        Chinese,    // 简体中文
        English,    // 英文
        Korean      // 韩文
    }

    /// <summary>
    /// 本地化管理器
    /// 支持多语言切换，避免硬编码
    /// </summary>
    public class LocalizationManager : MonoBehaviour
    {
        public static LocalizationManager Instance { get; private set; }

        [Header("当前语言")]
        [SerializeField] private Language currentLanguage = Language.Chinese;

        // 本地化字典：key -> (语言 -> 翻译)
        private Dictionary<string, Dictionary<Language, string>> localizationData 
            = new Dictionary<string, Dictionary<Language, string>>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeLocalization();
                Debug.Log($"[本地化] 系统初始化完成，当前语言：{currentLanguage}");
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 初始化本地化数据
        /// </summary>
        private void InitializeLocalization()
        {
            // 卡牌名称
            AddLocalization("card_slash", "杀", "Slash", "살");
            AddLocalization("card_dodge", "闪", "Dodge", "섬");
            AddLocalization("card_peach", "桃", "Peach", "복숭아");
            AddLocalization("card_duel", "决斗", "Duel", "결투");
            AddLocalization("card_savage_assault", "南蛮入侵", "Savage Assault", "남만침입");
            AddLocalization("card_arrow_barrage", "万箭齐发", "Arrow Barrage", "만전제발");

            // 武将名称
            AddLocalization("general_caocao", "曹操", "Cao Cao", "조조");
            AddLocalization("general_guanyu", "关羽", "Guan Yu", "관우");
            AddLocalization("general_zhangfei", "张飞", "Zhang Fei", "장비");
            AddLocalization("general_sunquan", "孙权", "Sun Quan", "손권");
            AddLocalization("general_liubei", "刘备", "Liu Bei", "유비");
            AddLocalization("general_zhugeliang", "诸葛亮", "Zhuge Liang", "제갈량");

            // 武将称号
            AddLocalization("title_caocao", "魏武帝", "Emperor of Wei", "위무제");
            AddLocalization("title_guanyu", "美髯公", "Lord Guan", "미염공");
            AddLocalization("title_zhangfei", "万夫不当", "Unstoppable", "만부부당");
            AddLocalization("title_sunquan", "年轻的贤君", "Young Emperor", "젊은 현군");

            // 技能名称
            AddLocalization("skill_jianxiong", "奸雄", "Treachery", "간웅");
            AddLocalization("skill_wusheng", "武圣", "Warrior Saint", "무성");
            AddLocalization("skill_paoxiao", "咆哮", "Roar", "포효");
            AddLocalization("skill_zhiheng", "制衡", "Balance", "제형");

            // 技能描述
            AddLocalization("skill_desc_jianxiong", 
                "当你受到伤害后，你可以获得对你造成伤害的牌。", 
                "When you take damage, you may obtain the card that dealt the damage.",
                "피해를 받은 후, 피해를 입힌 카드를 획득할 수 있습니다.");
            
            AddLocalization("skill_desc_wusheng", 
                "你可以将一张红色牌当【杀】使用或打出。", 
                "You may treat a red card as Slash.",
                "빨간색 카드를 【살】로 사용하거나 낼 수 있습니다.");
            
            AddLocalization("skill_desc_paoxiao", 
                "锁定技，出牌阶段，你使用【杀】无次数限制。", 
                "Lock: During your Action Phase, you may use unlimited Slashes.",
                "고정기, 행동 단계에서 【살】을 무제한으로 사용할 수 있습니다.");
            
            AddLocalization("skill_desc_zhiheng", 
                "出牌阶段限一次，你可以弃置任意张牌，然后摸等量的牌。", 
                "Once per Action Phase, you may discard any cards then draw the same amount.",
                "행동 단계에서 한 번, 임의의 카드를 버린 후 같은 수만큼 뽑을 수 있습니다.");

            // 阵营
            AddLocalization("faction_wei", "魏", "Wei", "위");
            AddLocalization("faction_shu", "蜀", "Shu", "촉");
            AddLocalization("faction_wu", "吴", "Wu", "오");
            AddLocalization("faction_qun", "群", "Qun", "군");

            // UI文本
            AddLocalization("ui_battle_mode", "对战模式", "Battle Mode", "대전 모드");
            AddLocalization("ui_story_mode", "故事模式", "Story Mode", "스토리 모드");
            AddLocalization("ui_settings", "设置", "Settings", "설정");
            AddLocalization("ui_start_game", "开始游戏", "Start Game", "게임 시작");
            AddLocalization("ui_back", "返回", "Back", "돌아가기");
            AddLocalization("ui_select_general", "选择武将", "Select General", "무장 선택");
            AddLocalization("ui_identity_mode", "身份场", "Identity Mode", "신분전");
            AddLocalization("ui_ai_difficulty", "AI难度", "AI Difficulty", "AI 난이도");
            AddLocalization("ui_easy", "简单", "Easy", "쉬움");
            AddLocalization("ui_normal", "普通", "Normal", "보통");
            AddLocalization("ui_hard", "困难", "Hard", "어려움");

            // 游戏提示
            AddLocalization("msg_your_turn", "你的回合", "Your Turn", "당신의 턴");
            AddLocalization("msg_draw_phase", "摸牌阶段", "Draw Phase", "카드 뽑기 단계");
            AddLocalization("msg_play_phase", "出牌阶段", "Action Phase", "행동 단계");
            AddLocalization("msg_discard_phase", "弃牌阶段", "Discard Phase", "카드 버리기 단계");
            AddLocalization("msg_select_target", "选择目标", "Select Target", "목표 선택");
            AddLocalization("msg_game_over", "游戏结束", "Game Over", "게임 종료");

            Debug.Log($"[本地化] 加载了 {localizationData.Count} 个翻译条目");
        }

        /// <summary>
        /// 添加本地化条目
        /// </summary>
        private void AddLocalization(string key, string chinese, string english, string korean)
        {
            if (!localizationData.ContainsKey(key))
            {
                localizationData[key] = new Dictionary<Language, string>();
            }

            localizationData[key][Language.Chinese] = chinese;
            localizationData[key][Language.English] = english;
            localizationData[key][Language.Korean] = korean;
        }

        /// <summary>
        /// 获取本地化文本
        /// </summary>
        public string GetText(string key)
        {
            if (localizationData.ContainsKey(key))
            {
                if (localizationData[key].ContainsKey(currentLanguage))
                {
                    return localizationData[key][currentLanguage];
                }
                else
                {
                    // 如果当前语言没有翻译，返回中文
                    Debug.LogWarning($"[本地化] 键 '{key}' 没有 {currentLanguage} 翻译");
                    return localizationData[key][Language.Chinese];
                }
            }
            else
            {
                Debug.LogWarning($"[本地化] 找不到键: {key}");
                return $"[{key}]"; // 返回键名，方便调试
            }
        }

        /// <summary>
        /// 切换语言
        /// </summary>
        public void SetLanguage(Language language)
        {
            currentLanguage = language;
            Debug.Log($"[本地化] 语言已切换到：{currentLanguage}");
            
            // 触发UI刷新事件
            OnLanguageChanged?.Invoke(currentLanguage);
        }

        /// <summary>
        /// 获取当前语言
        /// </summary>
        public Language GetCurrentLanguage()
        {
            return currentLanguage;
        }

        /// <summary>
        /// 语言切换事件
        /// </summary>
        public event System.Action<Language> OnLanguageChanged;

        /// <summary>
        /// 获取卡牌名称（本地化）
        /// </summary>
        public string GetCardName(string cardName)
        {
            // 根据中文卡牌名映射到key
            Dictionary<string, string> cardNameToKey = new Dictionary<string, string>()
            {
                {"杀", "card_slash"},
                {"闪", "card_dodge"},
                {"桃", "card_peach"},
                {"决斗", "card_duel"},
                {"南蛮入侵", "card_savage_assault"},
                {"万箭齐发", "card_arrow_barrage"}
            };

            if (cardNameToKey.ContainsKey(cardName))
            {
                return GetText(cardNameToKey[cardName]);
            }

            // 如果找不到映射，直接返回原名称
            return cardName;
        }

        /// <summary>
        /// 从本地化的卡牌名获取内部key
        /// 用于反向查询（例如AI判断卡牌类型）
        /// </summary>
        public string GetCardNameKey(string localizedName)
        {
            // 遍历所有card_开头的key
            foreach (var kvp in localizationData)
            {
                if (kvp.Key.StartsWith("card_"))
                {
                    foreach (var translation in kvp.Value)
                    {
                        if (translation.Value == localizedName)
                        {
                            return kvp.Key;
                        }
                    }
                }
            }

            return null;
        }
    }
}
