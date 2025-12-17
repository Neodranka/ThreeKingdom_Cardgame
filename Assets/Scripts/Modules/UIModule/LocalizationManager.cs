using UnityEngine;
using System.Collections.Generic;

namespace ThreeKingdoms
{
    /// <summary>
    /// 语言枚举
    /// </summary>
    public enum Language
    {
        Chinese = 0,
        English = 1,
        Korean = 2
    }

    /// <summary>
    /// 本地化管理器
    /// 单例模式，管理多语言翻译
    /// </summary>
    public class LocalizationManager : MonoBehaviour
    {
        public static LocalizationManager Instance { get; private set; }

        [Header("当前语言")]
        [SerializeField] private Language currentLanguage = Language.Chinese;

        // 翻译字典：Key -> 语言 -> 文本
        private Dictionary<string, Dictionary<Language, string>> translations = new Dictionary<string, Dictionary<Language, string>>();

        // 语言切换事件
        public event System.Action<Language> OnLanguageChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeLocalization();
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
            translations.Clear();

            // ==================== MainMenu & GameSetup ====================

            // 主菜单
            AddTranslation("menu_title", "三国杀", "Three Kingdoms Kill", "삼국살");
            AddTranslation("menu_battle_mode", "对战模式", "Battle Mode", "대전 모드");
            AddTranslation("menu_story_mode", "故事模式", "Story Mode", "스토리 모드");
            AddTranslation("menu_settings", "设置", "Settings", "설정");
            AddTranslation("menu_exit", "退出游戏", "Exit Game", "게임 종료");
            AddTranslation("menu_language", "语言", "Language", "언어");

            // 游戏准备场景
            AddTranslation("setup_title", "游戏准备", "Game Setup", "게임 준비");
            AddTranslation("setup_select_general", "选择武将", "Select General", "무장 선택");
            AddTranslation("setup_player_count", "玩家数量", "Player Count", "플레이어 수");
            AddTranslation("setup_ai_difficulty", "AI难度", "AI Difficulty", "AI 난이도");
            AddTranslation("setup_start_game", "开始游戏", "Start Game", "게임 시작");
            AddTranslation("setup_back", "返回", "Back", "돌아가기");
            AddTranslation("setup_confirm", "确认", "Confirm", "확인");
            AddTranslation("setup_cancel", "取消", "Cancel", "취소");
            AddTranslation("setup_identity_mode", "身份模式", "Identity Mode", "신분 모드");
            AddTranslation("setup_classic_mode", "经典模式", "Classic Mode", "클래식 모드");

            // AI难度
            AddTranslation("ai_easy", "简单", "Easy", "쉬움");
            AddTranslation("ai_normal", "普通", "Normal", "보통");
            AddTranslation("ai_hard", "困难", "Hard", "어려움");

            // ==================== 武将 (Generals) ====================

            AddTranslation("general_caocao", "曹操", "Cao Cao", "조조");
            AddTranslation("general_liubei", "刘备", "Liu Bei", "유비");
            AddTranslation("general_sunquan", "孙权", "Sun Quan", "손권");
            AddTranslation("general_guanyu", "关羽", "Guan Yu", "관우");
            AddTranslation("general_zhangfei", "张飞", "Zhang Fei", "장비");
            AddTranslation("general_zhugeliang", "诸葛亮", "Zhuge Liang", "제갈량");
            AddTranslation("general_zhaoyun", "赵云", "Zhao Yun", "조운");
            AddTranslation("general_machao", "马超", "Ma Chao", "마초");

            // 阵营
            AddTranslation("faction_wei", "魏", "Wei", "위");
            AddTranslation("faction_shu", "蜀", "Shu", "촉");
            AddTranslation("faction_wu", "吴", "Wu", "오");
            AddTranslation("faction_qun", "群", "Neutral", "군");

            // ==================== 卡牌名称 (Card Names) ====================

            // 基础牌
            AddTranslation("card_slash", "杀", "Slash", "공격");
            AddTranslation("card_dodge", "闪", "Dodge", "회피");
            AddTranslation("card_peach", "桃", "Heal", "회복");

            // 锦囊牌 - 即时锦囊
            AddTranslation("card_duel", "决斗", "Duel", "결투");
            AddTranslation("card_savage_assault", "南蛮入侵", "Savage Assault", "남만침입");
            AddTranslation("card_arrow_barrage", "万箭齐发", "Arrow Barrage", "일제사격");
            AddTranslation("card_peach_garden", "桃园结义", "Peach Garden", "도원결의");
            AddTranslation("card_nullification", "无懈可击", "Nullification", "무해가격");
            AddTranslation("card_snatch", "顺手牵羊", "Snatch", "순수견양");
            AddTranslation("card_dismantlement", "过河拆桥", "Dismantlement", "과하철교");
            AddTranslation("card_harvest", "五谷丰登", "Harvest", "오곡풍등");

            // 锦囊牌 - 延时锦囊
            AddTranslation("card_indulgence", "乐不思蜀", "Indulgence", "락불사촉");
            AddTranslation("card_lightning", "闪电", "Lightning", "번개");

            // 装备牌 - 武器
            AddTranslation("card_qinggang_sword", "青釭剑", "Qinggang Sword", "청강검");
            AddTranslation("card_zhangba_spear", "丈八蛇矛", "Zhangba Spear", "장팔사모");
            AddTranslation("card_qinglong_blade", "青龙偃月刀", "Qinglong Blade", "청룡언월도");
            AddTranslation("card_frost_sword", "寒冰剑", "Frost Sword", "한빙검");
            AddTranslation("card_crossbow", "诸葛连弩", "Crossbow", "제갈연노");

            // 装备牌 - 防具
            AddTranslation("card_eight_diagram", "八卦阵", "Eight Diagram", "팔괘진");
            AddTranslation("card_renwang_shield", "仁王盾", "Renwang Shield", "인왕방패");

            // 装备牌 - +1马
            AddTranslation("card_dilu", "的卢", "Dilu", "적로");
            AddTranslation("card_zixin", "紫骍", "Zixin", "자신");
            AddTranslation("card_jueying", "绝影", "Jueying", "절영");

            // 装备牌 - -1马
            AddTranslation("card_chitu", "赤兔", "Chitu", "적토");
            AddTranslation("card_dawan", "大宛", "Dawan", "대완");
            AddTranslation("card_zhuahuang", "爪黄飞电", "Zhuahuang Feidian", "조황비전");

            // 卡牌花色
            AddTranslation("suit_spade", "♠", "♠", "♠");
            AddTranslation("suit_heart", "♥", "♥", "♥");
            AddTranslation("suit_club", "♣", "♣", "♣");
            AddTranslation("suit_diamond", "♦", "♦", "♦");

            // 卡牌点数
            AddTranslation("point_a", "A", "A", "A");
            AddTranslation("point_j", "J", "J", "J");
            AddTranslation("point_q", "Q", "Q", "Q");
            AddTranslation("point_k", "K", "K", "K");

            // ==================== 回合阶段 (Turn Phases) ====================

            AddTranslation("phase_prepare", "准备阶段", "Prepare Phase", "준비 단계");
            AddTranslation("phase_judge", "判定阶段", "Judge Phase", "판정 단계");
            AddTranslation("phase_draw", "摸牌阶段", "Draw Phase", "드로우 단계");
            AddTranslation("phase_play", "出牌阶段", "Play Phase", "플레이 단계");
            AddTranslation("phase_discard", "弃牌阶段", "Discard Phase", "버리기 단계");
            AddTranslation("phase_end", "结束阶段", "End Phase", "종료 단계");

            // ==================== UI标签 (UI Labels) ====================

            AddTranslation("ui_turn", "第 {0} 回合", "Turn {0}", "{0} 턴");
            AddTranslation("ui_current_player", "当前玩家", "Current Player", "현재 플레이어");
            AddTranslation("ui_phase", "阶段", "Phase", "단계");
            AddTranslation("ui_draw_pile", "牌堆", "Draw Pile", "덱");
            AddTranslation("ui_discard_pile", "弃牌堆", "Discard Pile", "버린 카드");
            AddTranslation("ui_cards", "张牌", "cards", "장");
            AddTranslation("ui_hand_cards", "手牌", "Hand Cards", "패");
            AddTranslation("ui_hp", "体力", "HP", "체력");
            AddTranslation("ui_max_hp", "体力上限", "Max HP", "최대 체력");
            AddTranslation("ui_equipment", "装备", "Equipment", "장비");
            AddTranslation("ui_judge_area", "判定区", "Judge Area", "판정 구역");

            // UI按钮
            AddTranslation("ui_use_card", "使用卡牌", "Use Card", "카드 사용");
            AddTranslation("ui_cancel", "取消", "Cancel", "취소");
            AddTranslation("ui_end_phase", "结束出牌", "End Phase", "단계 종료");
            AddTranslation("ui_select_target", "选择目标", "Select Target", "대상 선택");
            AddTranslation("ui_confirm", "确认", "Confirm", "확인");
            AddTranslation("ui_skip", "跳过", "Skip", "건너뛰기");

            // 游戏状态
            AddTranslation("ui_game_over", "游戏结束", "Game Over", "게임 종료");
            AddTranslation("ui_winner", "获胜者", "Winner", "승자");
            AddTranslation("ui_waiting", "等待中...", "Waiting...", "대기 중...");
            AddTranslation("ui_your_turn", "你的回合", "Your Turn", "당신의 턴");
            AddTranslation("ui_ai_thinking", "AI思考中...", "AI Thinking...", "AI 생각 중...");

            // 玩家信息
            AddTranslation("ui_player", "玩家{0}", "Player {0}", "플레이어 {0}");
            AddTranslation("ui_ai_player", "AI玩家{0}", "AI Player {0}", "AI 플레이어 {0}");
            AddTranslation("ui_you", "你", "You", "당신");
            AddTranslation("ui_alive", "存活", "Alive", "생존");
            AddTranslation("ui_dead", "阵亡", "Dead", "사망");

            // ==================== 游戏消息 (Game Messages) ====================

            AddTranslation("msg_game_start", "游戏开始！", "Game Start!", "게임 시작!");
            AddTranslation("msg_select_target", "请选择目标!", "Please select target!", "대상을 선택하세요!");
            AddTranslation("msg_no_target", "没有可选目标", "No valid target", "유효한 대상 없음");
            AddTranslation("msg_invalid_target", "无效的目标", "Invalid target", "유효하지 않은 대상");

            // 卡牌使用消息（带格式化参数）
            AddTranslation("msg_used_card", "{0} 使用了【{1}】", "{0} used [{1}]", "{0}이(가) [{1}]을(를) 사용함");
            AddTranslation("msg_used_card_on_target", "{0} 对 {1} 使用了【{2}】", "{0} used [{2}] on {1}", "{0}이(가) {1}에게 [{2}]을(를) 사용함");
            AddTranslation("msg_drew_cards", "{0} 摸了 {1} 张牌", "{0} drew {1} card(s)", "{0}이(가) {1}장을 뽑음");
            AddTranslation("msg_damaged", "{0} 受到 {1} 点伤害", "{0} took {1} damage", "{0}이(가) {1} 데미지를 입음");
            AddTranslation("msg_recovered", "{0} 回复 {1} 点体力", "{0} recovered {1} HP", "{0}이(가) 체력 {1} 회복");
            AddTranslation("msg_discarded", "{0} 弃置了 {1} 张牌", "{0} discarded {1} card(s)", "{0}이(가) {1}장을 버림");
            AddTranslation("msg_died", "{0} 阵亡了", "{0} has died", "{0} 사망");

            // 响应消息
            AddTranslation("msg_dodge_required", "请出【闪】", "Play [Dodge]", "[섬]을 내세요");
            AddTranslation("msg_slash_required", "请出【杀】", "Play [Slash]", "[살]을 내세요");
            AddTranslation("msg_peach_required", "请出【桃】", "Play [Peach]", "[도]를 내세요");
            AddTranslation("msg_response_required", "请响应", "Please respond", "응답하세요");
            AddTranslation("msg_responded_dodge", "{0} 打出了【闪】", "{0} played [Dodge]", "{0}이(가) [섬]을 냄");
            AddTranslation("msg_responded_slash", "{0} 打出了【杀】", "{0} played [Slash]", "{0}이(가) [살]을 냄");
            AddTranslation("msg_no_dodge", "{0} 没有【闪】", "{0} has no [Dodge]", "{0}은(는) [섬]이 없음");
            AddTranslation("msg_no_slash", "{0} 没有【杀】", "{0} has no [Slash]", "{0}은(는) [살]이 없음");

            // 错误/警告消息
            AddTranslation("msg_cannot_use", "不能使用此牌", "Cannot use this card", "이 카드를 사용할 수 없음");
            AddTranslation("msg_dodge_only_response", "【闪】只能在响应【杀】时使用", "[Dodge] can only be used to respond to [Slash]", "[섬]은 [살]에 대응할 때만 사용 가능");
            AddTranslation("msg_out_of_range", "目标不在攻击范围内", "Target out of range", "대상이 공격 범위 밖");
            AddTranslation("msg_already_used_slash", "本回合已使用过【杀】", "Already used [Slash] this turn", "이번 턴에 이미 [살] 사용함");
            AddTranslation("msg_card_effect_unimplemented", "卡牌【{0}】的效果尚未实现", "Card [{0}] effect not implemented", "카드 [{0}] 효과 미구현");

            // 阶段切换消息
            AddTranslation("msg_phase_ended", "阶段结束", "Phase ended", "단계 종료");
            AddTranslation("msg_turn_start", "{0} 的回合开始", "{0}'s turn started", "{0}의 턴 시작");
            AddTranslation("msg_turn_end", "{0} 的回合结束", "{0}'s turn ended", "{0}의 턴 종료");

            // 特殊效果消息
            AddTranslation("msg_nullified", "【{0}】被【无懈可击】抵消", "[{0}] was nullified", "[{0}]이(가) 무효화됨");
            AddTranslation("msg_steal_card", "{0} 获得了 {1} 的一张手牌", "{0} stole a card from {1}", "{0}이(가) {1}의 카드 1장을 가져감");
            AddTranslation("msg_dismantle_card", "{0} 弃置了 {1} 的一张手牌", "{0} dismantled a card from {1}", "{0}이(가) {1}의 카드 1장을 파괴함");
            AddTranslation("msg_judge_success", "{0} 判定成功", "{0} judge succeeded", "{0} 판정 성공");
            AddTranslation("msg_judge_failed", "{0} 判定失败", "{0} judge failed", "{0} 판정 실패");
            AddTranslation("msg_skip_play_phase", "{0} 跳过出牌阶段", "{0} skipped play phase", "{0}이(가) 플레이 단계를 건너뜀");

            // 决斗相关
            AddTranslation("msg_duel_start", "【决斗】开始", "Duel started", "결투 시작");
            AddTranslation("msg_duel_lost", "{0} 决斗失败", "{0} lost the duel", "{0}이(가) 결투 패배");

            // AOE锦囊相关
            AddTranslation("msg_savage_assault_start", "【南蛮入侵】生效", "Savage Assault activated", "남만침입 발동");
            AddTranslation("msg_arrow_barrage_start", "【万箭齐发】生效", "Arrow Barrage activated", "만전제발 발동");
            AddTranslation("msg_all_recover", "所有角色回复1点体力", "All characters recovered 1 HP", "모든 캐릭터 체력 1 회복");



            // ==================== 手动修复缺失补充 ====================
            AddTranslation("ui_battle_mode", "对战模式", "Battle Mode", "대결모드");
            AddTranslation("ui_story_mode", "故事模式", "Story Mode", "스토리 모드");
            AddTranslation("ui_settings", "设置", "Setting", "설정");

            AddTranslation("ui_start_game", "开始游戏", "Start", "게임시작");
            AddTranslation("ui_back", "返回", "Back", "돌아가기");
            AddTranslation("ui_ai_difficulty", "ai难度", "AI Difficulty", "ai 난이도");
            AddTranslation("ui_normal", "普通", "normal", "보통");
            AddTranslation("ui_easy", "低", "easy", "쉬움");
            AddTranslation("ui_hard", "高", "hard", "어려움");
            AddTranslation("ui_selected", "已选择", "Selected", "선택됨");
            AddTranslation("ui_please_select_general", "请选择武将", "Please select general", "캐릭터를 선택하세요");

            // ==================== 杀的限制消息 ====================
            AddTranslation("msg_slash_limit_reached", "本回合已无法再使用杀", "Cannot use Slash anymore this turn", "이번 턴에 더 이상 [살]을 사용할 수 없습니다");
            AddTranslation("msg_target_out_of_range", "目标不在攻击范围内（距离:{0}, 范围:{1}）", "Target out of range (Distance:{0}, Range:{1})", "대상이 공격 범위 밖 (거리:{0}, 범위:{1})");

            // ==================== 技能名称 ====================
            AddTranslation("skill_rende", "仁德", "Benevolence", "인덕");
            AddTranslation("skill_wusheng", "武圣", "Warrior Saint", "무성");
            AddTranslation("skill_paoxiao", "咆哮", "Roar", "포효");
            AddTranslation("skill_jianxiong", "奸雄", "Villainous Hero", "간웅");
            AddTranslation("skill_zhiheng", "制衡", "Balance of Power", "제형");

            // 技能描述
            AddTranslation("skill_rende_desc", "出牌阶段，你可以将任意数量的手牌交给其他角色。", "During your play phase, you may give any number of hand cards to other characters.", "출패 단계에 패를 다른 캐릭터에게 줄 수 있습니다.");
            AddTranslation("skill_wusheng_desc", "你可以将一张红色牌当【杀】使用或打出。", "You may use or play a red card as [Slash].", "빨간 카드를 [살]로 사용할 수 있습니다.");
            AddTranslation("skill_paoxiao_desc", "锁定技，你使用【杀】无次数限制。", "Compulsory, you have no limit on the number of [Slash] you can use.", "강제 효과, [살] 사용 횟수 제한 없음.");
            AddTranslation("skill_jianxiong_desc", "当你受到伤害后，你可以获得造成伤害的牌。", "After you take damage, you may obtain the card that caused the damage.", "데미지를 입은 후 해당 카드를 얻을 수 있습니다.");
            AddTranslation("skill_zhiheng_desc", "出牌阶段限一次，你可以弃置任意数量的牌，然后摸等量的牌。", "Once per play phase, you may discard any number of cards, then draw the same amount.", "출패 단계 1회, 카드를 버리고 같은 수만큼 뽑습니다.");

            // ==================== 新增本地化文本 ====================

            // 主菜单
            AddTranslation("msg_story_mode_coming_soon", "故事模式开发中，敬请期待！", "Story mode coming soon!", "스토리 모드 개발 중입니다!");
            AddTranslation("msg_settings_coming_soon", "设置功能开发中，敬请期待！", "Settings coming soon!", "설정 기능 개발 중입니다!");

            // 战斗UI - 选择相关
            AddTranslation("ui_card_selected", "已选择", "Selected", "선택됨");
            AddTranslation("ui_target", "目标", "Target", "대상");
            AddTranslation("ui_play_card", "出牌", "Play", "출패");

            // 战斗UI - 响应相关
            AddTranslation("msg_complete_response_first", "请先完成响应!", "Please complete response first!", "먼저 응답을 완료하세요!");
            AddTranslation("msg_end_play_phase", "结束出牌阶段", "End play phase", "출패 단계 종료");
            AddTranslation("msg_responded_card", "{0} 打出了【{1}】", "{0} played [{1}]", "{0}이(가) [{1}]을(를) 냈습니다");
            AddTranslation("msg_no_response", "{0} 没有响应", "{0} did not respond", "{0}이(가) 응답하지 않았습니다");
            AddTranslation("msg_need_card", "需要出【{0}】", "Need to play [{0}]", "[{0}]을(를) 내야 합니다");

            // 战斗UI - 卡牌使用限制
            AddTranslation("msg_no_cards_to_obtain", "目标没有可获得的牌!", "Target has no cards to obtain!", "대상에게 얻을 카드가 없습니다!");
            AddTranslation("msg_no_cards_to_discard", "目标没有可弃置的牌!", "Target has no cards to discard!", "대상에게 버릴 카드가 없습니다!");
            AddTranslation("msg_nullification_only_response", "【无懈可击】只能在响应锦囊牌时使用", "[Nullification] can only be used to counter trick cards", "[무해가격]은 계략 카드에 대응할 때만 사용 가능");
            AddTranslation("msg_delayed_trick_not_implemented", "延时锦囊暂未实现", "Delayed tricks not implemented yet", "지연 계략 미구현");

            // 阶段和阵营
            AddTranslation("phase_unknown", "未知阶段", "Unknown Phase", "알 수 없는 단계");
            AddTranslation("faction_unknown", "未知", "Unknown", "알 수 없음");

            // ⭐ 弃牌系统
            AddTranslation("msg_discard_prompt", "请选择 {0} 张牌弃置", "Please select {0} card(s) to discard", "{0}장의 카드를 버리세요");
            AddTranslation("msg_discard_progress", "请选择要弃置的牌 ({0}/{1})", "Select cards to discard ({0}/{1})", "버릴 카드를 선택하세요 ({0}/{1})");

            Debug.Log($"[LocalizationManager] 本地化初始化完成，当前语言：{currentLanguage}");
        }

        /// <summary>
        /// 添加翻译
        /// </summary>
        private void AddTranslation(string key, string chinese, string english, string korean)
        {
            if (!translations.ContainsKey(key))
            {
                translations[key] = new Dictionary<Language, string>();
            }

            translations[key][Language.Chinese] = chinese;
            translations[key][Language.English] = english;
            translations[key][Language.Korean] = korean;
        }

        /// <summary>
        /// 获取文本
        /// </summary>
        public string GetText(string key)
        {
            if (translations.ContainsKey(key) && translations[key].ContainsKey(currentLanguage))
            {
                return translations[key][currentLanguage];
            }

            Debug.LogWarning($"[LocalizationManager] 找不到key: {key}，语言：{currentLanguage}");
            return key; // 找不到则返回key本身
        }

        /// <summary>
        /// 获取格式化文本（带参数）
        /// </summary>
        public string GetTextFormatted(string key, params object[] args)
        {
            string template = GetText(key);
            try
            {
                return string.Format(template, args);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[LocalizationManager] 格式化失败: {key}, 错误: {e.Message}");
                return template;
            }
        }

        /// <summary>
        /// 设置语言
        /// </summary>
        public void SetLanguage(Language language)
        {
            if (currentLanguage != language)
            {
                currentLanguage = language;
                Debug.Log($"[LocalizationManager] 语言切换: {language}");

                // 保存到PlayerPrefs
                PlayerPrefs.SetInt("Language", (int)language);
                PlayerPrefs.Save();

                // 触发事件
                OnLanguageChanged?.Invoke(language);
            }
        }

        /// <summary>
        /// 获取当前语言
        /// </summary>
        public Language GetCurrentLanguage()
        {
            return currentLanguage;
        }

        /// <summary>
        /// 从PlayerPrefs加载语言设置
        /// </summary>
        public void LoadLanguageFromPrefs()
        {
            if (PlayerPrefs.HasKey("Language"))
            {
                int savedLanguage = PlayerPrefs.GetInt("Language");
                currentLanguage = (Language)savedLanguage;
                Debug.Log($"[LocalizationManager] 从PlayerPrefs加载语言: {currentLanguage}");
            }
        }
    }
}