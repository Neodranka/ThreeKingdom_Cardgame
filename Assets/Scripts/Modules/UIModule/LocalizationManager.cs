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

            // ⭐ 故事模式 - UI文本
            AddTranslation("story_mode_title", "故事模式", "Story Mode", "스토리 모드");
            AddTranslation("ui_campaigns", "战役", "Campaigns", "전역");
            AddTranslation("ui_battles", "战斗", "Battles", "전투");
            AddTranslation("story_locked", "尚未解锁", "Locked", "잠금");
            AddTranslation("story_difficulty", "难度", "Difficulty", "난이도");
            AddTranslation("story_special_rule", "特殊规则", "Special Rule", "특수 규칙");
            AddTranslation("story_start_battle", "开始战斗", "Start Battle", "전투 시작");
            AddTranslation("story_completed", "已完成", "Completed", "완료");

            // ⭐ 故事模式 - 战役名称
            AddTranslation("campaign_yellow_turban", "黄巾之乱", "Yellow Turban Rebellion", "황건적의 난");
            AddTranslation("campaign_dong_zhuo", "董卓讨伐战", "Campaign Against Dong Zhuo", "동탁 토벌전");
            AddTranslation("campaign_guandu", "官渡之战", "Battle of Guandu", "관도 대전");
            AddTranslation("campaign_chibi", "赤壁之战", "Battle of Red Cliffs", "적벽 대전");
            AddTranslation("campaign_three_kingdoms", "三国鼎立", "Rise of Three Kingdoms", "삼국 정립");

            // ⭐ 故事模式 - 战役描述
            AddTranslation("campaign_yellow_turban_desc", "东汉末年，张角率领黄巾军起义，天下大乱。刘备、关羽、张飞桃园结义，投身平乱战争。",
                "At the end of the Eastern Han Dynasty, Zhang Jiao led the Yellow Turban uprising, plunging the land into chaos. Liu Bei, Guan Yu, and Zhang Fei swore brotherhood and joined the war.",
                "후한 말기, 장각이 황건적을 이끌고 봉기하여 천하가 혼란에 빠졌습니다. 유비, 관우, 장비가 도원결의를 맺고 전쟁에 참전합니다.");
            AddTranslation("campaign_dong_zhuo_desc", "董卓把持朝政，残暴不仁。十八路诸侯会盟，共讨国贼。",
                "Dong Zhuo seized control of the court with tyranny. Eighteen lords formed an alliance to overthrow the tyrant.",
                "동탁이 조정을 장악하고 폭정을 일삼습니다. 18로 제후가 연합하여 국적을 토벌합니다.");
            AddTranslation("campaign_guandu_desc", "曹操与袁绍两大势力在官渡对峙，这是决定北方霸权的关键之战。",
                "Cao Cao and Yuan Shao face off at Guandu in a decisive battle for northern supremacy.",
                "조조와 원소 두 세력이 관도에서 대치하며, 북방 패권을 결정짓는 전투가 펼쳐집니다.");
            AddTranslation("campaign_chibi_desc", "曹操率大军南下，孙刘联军在赤壁抵抗。火攻连环船，以少胜多。",
                "Cao Cao marches south with a massive army. The Sun-Liu alliance resists at Red Cliffs, using fire to defeat the enemy against all odds.",
                "조조가 대군을 이끌고 남하합니다. 손유 연합군이 적벽에서 화공으로 적을 물리칩니다.");
            AddTranslation("campaign_three_kingdoms_desc", "魏、蜀、吴三国鼎立，诸葛亮北伐，谱写最后的英雄史诗。",
                "Wei, Shu, and Wu stand as three powers. Zhuge Liang's Northern Expeditions write the final heroic epic.",
                "위, 촉, 오 삼국이 정립합니다. 제갈량의 북벌로 마지막 영웅 서사시가 펼쳐집니다.");

            // ⭐ 故事模式 - 黄巾之乱战斗
            AddTranslation("battle_yt_1", "初战黄巾", "First Battle Against Yellow Turbans", "황건적 초전");
            AddTranslation("battle_yt_1_desc", "刘关张三人第一次与黄巾军正面交锋。", "Liu Bei and his brothers face the Yellow Turbans for the first time.", "유비 삼형제가 처음으로 황건적과 대면합니다.");
            AddTranslation("battle_yt_1_brief", "击败张宝和张梁，证明三兄弟的实力。", "Defeat Zhang Bao and Zhang Liang to prove the brothers' strength.", "장보와 장량을 물리쳐 삼형제의 실력을 증명하세요.");

            AddTranslation("battle_yt_2", "青州救援", "Qingzhou Rescue", "청주 구원");
            AddTranslation("battle_yt_2_desc", "关羽独自前往青州救援被困友军。", "Guan Yu goes alone to rescue trapped allies in Qingzhou.", "관우가 홀로 청주의 아군을 구하러 갑니다.");
            AddTranslation("battle_yt_2_brief", "以关羽之力，击退黄巾军主力。", "Use Guan Yu's might to repel the Yellow Turban main force.", "관우의 힘으로 황건적 주력을 격퇴하세요.");

            AddTranslation("battle_yt_3", "广宗决战", "Battle of Guangzong", "광종 결전");
            AddTranslation("battle_yt_3_desc", "张飞率军直捣黄巾军大本营。", "Zhang Fei leads troops to attack the Yellow Turban headquarters.", "장비가 군대를 이끌고 황건적 본영을 공격합니다.");
            AddTranslation("battle_yt_3_brief", "突破敌军防线，击败黄巾三将。", "Break through enemy lines and defeat the three Yellow Turban generals.", "적진을 돌파하고 황건 삼장을 물리치세요.");

            AddTranslation("battle_yt_4", "擒杀张角", "Capture Zhang Jiao", "장각 토벌");
            AddTranslation("battle_yt_4_desc", "黄巾之乱的终章，讨伐张角。", "The final chapter of the Yellow Turban Rebellion - defeat Zhang Jiao.", "황건적의 난 최종장, 장각을 토벌합니다.");
            AddTranslation("battle_yt_4_brief", "击败黄巾首领张角，平定叛乱。", "Defeat the Yellow Turban leader Zhang Jiao to end the rebellion.", "황건적 수령 장각을 물리쳐 반란을 평정하세요.");

            // ⭐ 故事模式 - 董卓讨伐战战斗
            AddTranslation("battle_dz_1", "汜水关", "Sishui Pass", "사수관");
            AddTranslation("battle_dz_1_desc", "联军进攻汜水关，华雄守关。", "The allied forces attack Sishui Pass, defended by Hua Xiong.", "연합군이 화웅이 지키는 사수관을 공격합니다.");
            AddTranslation("battle_dz_1_brief", "作为曹操军，突破华雄的防线。", "As Cao Cao's forces, break through Hua Xiong's defense.", "조조군으로서 화웅의 방어선을 돌파하세요.");

            AddTranslation("battle_dz_2", "温酒斩华雄", "Slaying Hua Xiong", "온주참화웅");
            AddTranslation("battle_dz_2_desc", "关羽请战，温酒尚温，已斩华雄。", "Guan Yu volunteers - the wine is still warm when Hua Xiong falls.", "관우가 출전하여 술이 식기 전에 화웅을 벱니다.");
            AddTranslation("battle_dz_2_brief", "操控关羽，一战成名。", "Control Guan Yu and become famous in one battle.", "관우를 조종하여 일전에 명성을 얻으세요.");

            AddTranslation("battle_dz_3", "虎牢关", "Hulao Pass", "호뢰관");
            AddTranslation("battle_dz_3_desc", "三英战吕布，天下闻名。", "Three heroes battle Lu Bu - a legendary fight.", "삼영웅이 여포와 싸우는 천하의 명장면입니다.");
            AddTranslation("battle_dz_3_brief", "协力对抗天下第一猛将吕布。", "Work together to face Lu Bu, the mightiest warrior.", "천하제일 맹장 여포에 맞서 싸우세요.");

            AddTranslation("battle_dz_4", "董卓伏诛", "Fall of Dong Zhuo", "동탁 처단");
            AddTranslation("battle_dz_4_desc", "进入洛阳，讨伐国贼董卓。", "Enter Luoyang and defeat the tyrant Dong Zhuo.", "낙양에 입성하여 국적 동탁을 토벌합니다.");
            AddTranslation("battle_dz_4_brief", "击败董卓和吕布，解放京城。", "Defeat Dong Zhuo and Lu Bu to liberate the capital.", "동탁과 여포를 물리쳐 수도를 해방시키세요.");

            // ⭐ 故事模式 - 官渡之战战斗
            AddTranslation("battle_gd_1", "白马之围", "Siege of Baima", "백마 포위전");
            AddTranslation("battle_gd_1_desc", "袁绍大军围攻白马，曹操亲自救援。", "Yuan Shao's army besieges Baima. Cao Cao comes to the rescue.", "원소의 대군이 백마를 포위하고 조조가 구원합니다.");
            AddTranslation("battle_gd_1_brief", "突破袁绍军的包围。", "Break through Yuan Shao's encirclement.", "원소군의 포위를 돌파하세요.");

            AddTranslation("battle_gd_2", "斩颜良诛文丑", "Slaying Yan Liang and Wen Chou", "안량·문추 참수");
            AddTranslation("battle_gd_2_desc", "关羽斩杀袁绍手下大将。", "Guan Yu slays Yuan Shao's top generals.", "관우가 원소 휘하의 대장들을 베어냅니다.");
            AddTranslation("battle_gd_2_brief", "以关羽之勇，斩杀颜良文丑。", "With Guan Yu's valor, slay Yan Liang and Wen Chou.", "관우의 용맹으로 안량과 문추를 베세요.");

            AddTranslation("battle_gd_3", "官渡对峙", "Standoff at Guandu", "관도 대치");
            AddTranslation("battle_gd_3_desc", "两军在官渡对峙，决定天下归属。", "Two armies face off at Guandu to decide the fate of the realm.", "두 군대가 관도에서 대치하며 천하의 운명을 결정합니다.");
            AddTranslation("battle_gd_3_brief", "坚守阵地，等待战机。", "Hold your ground and wait for the right moment.", "진지를 지키고 전기를 기다리세요.");

            AddTranslation("battle_gd_4", "火烧乌巢", "Burning of Wuchao", "오소 화공");
            AddTranslation("battle_gd_4_desc", "曹操奇袭乌巢，烧毁袁绍粮草。", "Cao Cao raids Wuchao and burns Yuan Shao's supplies.", "조조가 오소를 기습하여 원소의 군량을 불태웁니다.");
            AddTranslation("battle_gd_4_brief", "奇袭敌军粮仓，一举定乾坤。", "Raid the enemy's granary to turn the tide.", "적의 군량고를 기습하여 전세를 뒤집으세요.");

            // ⭐ 故事模式 - 赤壁之战战斗
            AddTranslation("battle_cb_1", "舌战群儒", "Debate with Scholars", "설전군유");
            AddTranslation("battle_cb_1_desc", "诸葛亮孤身入东吴，说服孙权抗曹。", "Zhuge Liang goes alone to Eastern Wu to persuade Sun Quan.", "제갈량이 홀로 동오에 가서 손권을 설득합니다.");
            AddTranslation("battle_cb_1_brief", "以智谋折服东吴群臣。", "Use wisdom to convince Eastern Wu's ministers.", "지략으로 동오의 신하들을 설득하세요.");

            AddTranslation("battle_cb_2", "蒋干盗书", "Jiang Gan's Theft", "장간의 편지 절도");
            AddTranslation("battle_cb_2_desc", "周瑜设计，让蒋干带走假情报。", "Zhou Yu schemes to have Jiang Gan steal false information.", "주유가 계략을 세워 장간에게 거짓 정보를 훔쳐가게 합니다.");
            AddTranslation("battle_cb_2_brief", "利用反间计，除掉蔡瑁张允。", "Use the counter-spy plot to eliminate Cai Mao and Zhang Yun.", "반간계로 채모와 장윤을 제거하세요.");

            AddTranslation("battle_cb_3", "黄盖诈降", "Huang Gai's Feigned Surrender", "황개의 거짓 항복");
            AddTranslation("battle_cb_3_desc", "黄盖苦肉计，诈降曹操。", "Huang Gai uses the self-torture scheme to fake surrender.", "황개가 고육지계로 조조에게 거짓 항복합니다.");
            AddTranslation("battle_cb_3_brief", "实施火攻计划的关键一步。", "A crucial step in the fire attack plan.", "화공 계획의 핵심 단계입니다.");

            AddTranslation("battle_cb_4", "华容道", "Huarong Path", "화용도");
            AddTranslation("battle_cb_4_desc", "关羽在华容道截住败走的曹操。", "Guan Yu intercepts the fleeing Cao Cao at Huarong Path.", "관우가 화용도에서 도망가는 조조를 막습니다.");
            AddTranslation("battle_cb_4_brief", "义释曹操，还是执行军令？", "Release Cao Cao out of honor, or follow orders?", "의리로 조조를 놓아주겠습니까, 군령을 따르겠습니까?");

            AddTranslation("battle_cb_5", "赤壁决战", "Battle of Red Cliffs", "적벽 결전");
            AddTranslation("battle_cb_5_desc", "火烧赤壁，以少胜多的传奇之战。", "The legendary battle of Red Cliffs - victory against overwhelming odds.", "적벽대전, 소수로 다수를 이긴 전설적 전투입니다.");
            AddTranslation("battle_cb_5_brief", "指挥联军，击溃曹操大军。", "Command the allied forces to crush Cao Cao's army.", "연합군을 지휘하여 조조의 대군을 격파하세요.");

            // ⭐ 故事模式 - 三国鼎立战斗
            AddTranslation("battle_tk_1", "汉中争夺", "Battle for Hanzhong", "한중 쟁탈전");
            AddTranslation("battle_tk_1_desc", "刘备与曹操争夺汉中要地。", "Liu Bei and Cao Cao fight for the strategic Hanzhong.", "유비와 조조가 전략적 요충지 한중을 두고 싸웁니다.");
            AddTranslation("battle_tk_1_brief", "夺取汉中，建立蜀汉基业。", "Capture Hanzhong to establish Shu Han.", "한중을 점령하여 촉한의 기반을 다지세요.");

            AddTranslation("battle_tk_2", "空城计", "Empty Fort Strategy", "공성계");
            AddTranslation("battle_tk_2_desc", "诸葛亮独守空城，退司马懿大军。", "Zhuge Liang holds an empty city against Sima Yi's army.", "제갈량이 빈 성으로 사마의의 대군을 물리칩니다.");
            AddTranslation("battle_tk_2_brief", "以智取胜，吓退敌军。", "Win through wisdom and scare off the enemy.", "지략으로 승리하고 적군을 물리치세요.");

            AddTranslation("battle_tk_3", "荆州之殇", "Fall of Jingzhou", "형주의 비극");
            AddTranslation("battle_tk_3_desc", "关羽大意失荆州，吴军偷袭。", "Guan Yu loses Jingzhou through carelessness - Wu army ambush.", "관우가 방심하여 형주를 잃고 오군이 기습합니다.");
            AddTranslation("battle_tk_3_brief", "能否守住荆州，改变历史？", "Can you hold Jingzhou and change history?", "형주를 지켜 역사를 바꿀 수 있을까요?");

            AddTranslation("battle_tk_4", "北伐中原", "Northern Expedition", "북벌");
            AddTranslation("battle_tk_4_desc", "诸葛亮六出祁山，北伐曹魏。", "Zhuge Liang's six campaigns from Qishan against Cao Wei.", "제갈량의 기산 6차 북벌입니다.");
            AddTranslation("battle_tk_4_brief", "指挥北伐军，与司马懿决战。", "Command the Northern Expedition army against Sima Yi.", "북벌군을 지휘하여 사마의와 결전하세요.");

            AddTranslation("battle_tk_5", "三国终章", "Final Chapter", "삼국 종장");
            AddTranslation("battle_tk_5_desc", "天下三分，最终谁能统一？", "The realm is divided into three - who will unify it?", "천하가 셋으로 나뉘었습니다. 누가 통일할 것인가?");
            AddTranslation("battle_tk_5_brief", "这是最终决战，书写你的三国传奇。", "This is the final battle - write your Three Kingdoms legend.", "최종 결전입니다. 당신의 삼국지 전설을 써내려가세요.");

            // ⭐ 故事模式 - 特殊规则
            AddTranslation("rule_burn_supplies", "火烧粮草：每回合结束时，所有玩家弃一张手牌", "Burn Supplies: All players discard one card at end of each turn", "군량 화공: 매 턴 종료 시 모든 플레이어가 카드 1장을 버립니다");
            AddTranslation("rule_fire_attack", "火攻：【杀】造成的伤害+1", "Fire Attack: [Slash] deals +1 damage", "화공: [살]이 주는 피해가 +1 증가합니다");
            AddTranslation("rule_huarong_path", "华容道：关羽选择是否放走曹操", "Huarong Path: Guan Yu chooses whether to release Cao Cao", "화용도: 관우가 조조를 놓아줄지 선택합니다");
            AddTranslation("rule_northern_expedition", "北伐：蜀军士气高涨，摸牌+1", "Northern Expedition: Shu army morale is high, draw +1 card", "북벌: 촉군의 사기가 높아 카드를 1장 더 뽑습니다");
            AddTranslation("rule_final_battle", "终极决战：所有角色体力上限+1", "Final Battle: All characters gain +1 max HP", "최종 결전: 모든 캐릭터의 체력 상한이 +1 증가합니다");

            // ==================== 赤壁之战 · 详细数据 ====================

            // 战役信息
            AddTranslation("campaign_chibi_desc", "建安十三年，曹操南下，兵锋所至，无人能挡。赤壁，既是退路的尽头，也是蜀汉命运的转折点。",
                "In the 13th year of Jian'an, Cao Cao marches south with an unstoppable army. Red Cliffs marks both the end of retreat and the turning point of Shu Han's fate.",
                "건안 13년, 조조가 남하하여 막을 수 없는 기세입니다. 적벽은 퇴로의 끝이자 촉한 운명의 전환점입니다.");

            // 第一战：柴桑盟议
            AddTranslation("battle_chibi_1", "柴桑盟议", "Chaisang Alliance", "시상 맹의");
            AddTranslation("battle_chibi_1_subtitle", "降？还是战？", "Surrender? Or Fight?", "항복인가? 싸울 것인가?");
            AddTranslation("battle_chibi_1_desc", "孙权面对主和派与主战派的激烈争论，必须做出抉择——是俯首称臣，还是奋起抗曹？",
                "Sun Quan faces heated debate between the peace and war factions. He must choose - submit or resist Cao Cao?",
                "손권은 화의파와 주전파의 격렬한 논쟁 앞에서 결단을 내려야 합니다. 굴복할 것인가, 항전할 것인가?");
            AddTranslation("battle_chibi_1_briefing", "击败张昭（使其体力归零）即可获胜。张昭不会主动攻击，仅使用【主和】技能削弱孙权。",
                "Defeat Zhang Zhao to win. He won't attack directly but uses [Advocate Peace] to weaken Sun Quan.",
                "장소를 물리치면 승리입니다. 그는 직접 공격하지 않고 [주화] 기술로 손권을 약화시킵니다.");

            // 第二战：诸葛渡江
            AddTranslation("battle_chibi_2", "诸葛渡江", "Zhuge Crosses the River", "제갈량 도강");
            AddTranslation("battle_chibi_2_subtitle", "智与疑的交锋", "Clash of Wit and Doubt", "지혜와 의심의 대결");
            AddTranslation("battle_chibi_2_desc", "诸葛亮奉命出使东吴，说服周瑜联合抗曹。然而周瑜此人，最恨被人牵着走……",
                "Zhuge Liang is sent to Eastern Wu to persuade Zhou Yu to join against Cao Cao. But Zhou Yu hates being led by others...",
                "제갈량이 동오로 파견되어 주유를 설득합니다. 하지만 주유는 남에게 끌려다니는 것을 싫어합니다...");
            AddTranslation("battle_chibi_2_briefing", "这是一场生存战，存活至第6回合即可获胜。无需击败敌人，保住诸葛亮的性命即可。",
                "This is a survival battle. Survive until turn 6 to win. No need to defeat enemies, just keep Zhuge Liang alive.",
                "생존전입니다. 6턴까지 살아남으면 승리합니다. 적을 물리칠 필요 없이 제갈량을 살리세요.");

            // 第三战：长坂坡突围
            AddTranslation("battle_chibi_3", "长坂坡突围", "Changban Breakout", "장판파 돌파");
            AddTranslation("battle_chibi_3_subtitle", "百万军中护主", "Protecting the Lord Among a Million", "백만 대군 속 주군 호위");
            AddTranslation("battle_chibi_3_desc", "曹军先头部队追上来了！赵云必须护送刘备突出重围。",
                "Cao Cao's vanguard has caught up! Zhao Yun must escort Liu Bei to break through the siege.",
                "조조군 선발대가 따라잡았습니다! 조운이 유비를 호위하여 포위를 돌파해야 합니다.");
            AddTranslation("battle_chibi_3_briefing", "新手引导战斗，难度较低。熟悉基础操作和龙胆技能使用。",
                "Tutorial battle with low difficulty. Learn basic operations and Longdan skill usage.",
                "튜토리얼 전투로 난이도가 낮습니다. 기본 조작과 용담 기술 사용법을 익히세요.");

            // 第四战：张飞断桥
            AddTranslation("battle_chibi_4", "张飞断桥", "Zhang Fei Breaks the Bridge", "장비 교량 파괴");
            AddTranslation("battle_chibi_4_subtitle", "燕人张翼德在此！", "I am Zhang Yide of Yan!", "연인 장익덕이 여기 있다!");
            AddTranslation("battle_chibi_4_desc", "曹操亲自率领部队追了上来。张飞单骑断桥，横矛立马，以一己之力阻挡曹军追击。",
                "Cao Cao personally leads troops in pursuit. Zhang Fei alone blocks the bridge with his spear to stop the pursuers.",
                "조조가 직접 군대를 이끌고 추격합니다. 장비가 홀로 다리를 막고 창을 세워 추격군을 저지합니다.");
            AddTranslation("battle_chibi_4_briefing", "击败夏侯杰即可获胜。夏侯杰体力降至2点时会触发【胆裂】，无法使用闪且伤害-1。",
                "Defeat Xiahou Jie to win. When his HP drops to 2, [Terror] triggers - he can't use Dodge and deals -1 damage.",
                "하후걸을 물리치면 승리입니다. 체력이 2가 되면 [담렬]이 발동되어 섬을 사용할 수 없고 피해가 -1됩니다.");

            // 第五战：黄盖诈降
            AddTranslation("battle_chibi_5", "黄盖诈降", "Huang Gai's Feigned Surrender", "황개의 거짓 항복");
            AddTranslation("battle_chibi_5_subtitle", "最痛的一步棋", "The Most Painful Move", "가장 고통스러운 한 수");
            AddTranslation("battle_chibi_5_desc", "黄盖单膝跪地，请命受刑。苦肉计，是火攻的前奏——必须让曹操相信黄盖是真心投降。",
                "Huang Gai kneels and asks to be punished. The self-torture scheme precedes the fire attack - Cao Cao must believe the surrender is real.",
                "황개가 무릎을 꿇고 형벌을 청합니다. 고육지계는 화공의 전주곡으로 조조가 진짜 항복이라 믿게 해야 합니다.");
            AddTranslation("battle_chibi_5_briefing", "累积3个诈降标记即可获胜。每次发动【苦肉诈降】会获得1个标记。注意躲避蒋干的【盗书】。",
                "Accumulate 3 surrender marks to win. Each [Self-Torture Surrender] gives 1 mark. Avoid Jiang Gan's [Steal Documents].",
                "항복 표시 3개를 모으면 승리합니다. [고육사항]을 발동할 때마다 1개를 얻습니다. 장간의 [도서]를 피하세요.");

            // 第六战：赤壁火起
            AddTranslation("battle_chibi_6", "赤壁火起", "Fire at Red Cliffs", "적벽의 불");
            AddTranslation("battle_chibi_6_subtitle", "火，照亮了天下", "Fire Illuminates the World", "불이 천하를 밝힌다");
            AddTranslation("battle_chibi_6_desc", "夜色如墨，江水翻涌。东南风起，三更点火。这是决定天下大势的一战——孙刘联军对阵曹操八十万大军。",
                "Night is dark as ink, the river surges. The southeast wind rises, fire at the third watch. This battle decides the fate of the realm - Sun-Liu alliance vs Cao Cao's 800,000 troops.",
                "밤은 먹처럼 어둡고 강물이 출렁입니다. 동남풍이 불고 삼경에 불을 붙입니다. 천하의 운명을 결정하는 전투입니다.");
            AddTranslation("battle_chibi_6_briefing", "击败曹操即可获胜。注意：第2回合火攻伤害+2，第3回合再+1。联军初始手牌-1。",
                "Defeat Cao Cao to win. Note: Fire damage +2 on turn 2, +1 more on turn 3. Alliance starts with -1 hand cards.",
                "조조를 물리치면 승리입니다. 주의: 2턴에 화공 피해 +2, 3턴에 추가 +1. 연합군 초기 패 -1장.");

            // 角色名称
            AddTranslation("char_sunquan", "孙权", "Sun Quan", "손권");
            AddTranslation("char_lusu", "鲁肃", "Lu Su", "노숙");
            AddTranslation("char_chengpu", "程普", "Cheng Pu", "정보");
            AddTranslation("char_zhangzhao", "张昭", "Zhang Zhao", "장소");
            AddTranslation("char_zhugeliang", "诸葛亮", "Zhuge Liang", "제갈량");
            AddTranslation("char_zhouyu", "周瑜", "Zhou Yu", "주유");
            AddTranslation("char_lvmeng", "吕蒙", "Lv Meng", "여몽");
            AddTranslation("char_zhaoyun", "赵云", "Zhao Yun", "조운");
            AddTranslation("char_zhangfei", "张飞", "Zhang Fei", "장비");
            AddTranslation("char_xiahoujie", "夏侯杰", "Xiahou Jie", "하후걸");
            AddTranslation("char_huanggai", "黄盖", "Huang Gai", "황개");
            AddTranslation("char_jianggan", "蒋干", "Jiang Gan", "장간");
            AddTranslation("char_liubei", "刘备", "Liu Bei", "유비");
            AddTranslation("char_guanyu", "关羽", "Guan Yu", "관우");
            AddTranslation("char_caocao", "曹操", "Cao Cao", "조조");
            AddTranslation("char_xiahoudun", "夏侯惇", "Xiahou Dun", "하후돈");
            AddTranslation("char_xiahouyuan", "夏侯渊", "Xiahou Yuan", "하후연");
            AddTranslation("char_zhangliao", "张辽", "Zhang Liao", "장료");
            AddTranslation("char_mifang", "糜芳", "Mi Fang", "미방");
            AddTranslation("char_caojun_cavalry", "曹军骑兵", "Cao Cavalry", "조조군 기병");

            // 特殊规则
            AddTranslation("rule_zhangzhao_no_attack", "张昭不会主动攻击", "Zhang Zhao won't attack", "장소는 공격하지 않습니다");
            AddTranslation("rule_lusu_support", "鲁肃每回合可为友军补牌", "Lu Su can give cards to allies each turn", "노숙이 매 턴 아군에게 카드를 줍니다");
            AddTranslation("rule_zhouyu_support", "周瑜会适时提醒黄盖", "Zhou Yu will remind Huang Gai", "주유가 황개에게 조언합니다");
            AddTranslation("rule_retreat_debuff", "连日败退：刘备方初始手牌-1", "Continuous Retreat: Liu Bei's side starts with -1 cards", "연일 패퇴: 유비측 초기 패 -1장");
            AddTranslation("rule_chain_ships", "铁索连船：曹军攻击距离+1", "Chained Ships: Cao's army attack range +1", "철쇄연선: 조조군 공격 거리 +1");
            AddTranslation("rule_conspiracy", "密谋成功：火攻伤害+2", "Conspiracy Success: Fire damage +2", "밀모 성공: 화공 피해 +2");
            AddTranslation("rule_east_wind", "东风渐起：火攻伤害再+1", "East Wind Rising: Fire damage +1 more", "동풍: 화공 피해 추가 +1");

            // 对白 - 第一战
            AddTranslation("dialogue_chibi1_opening_1", "冬夜，柴桑。大帐之内，只有火盆噼啪作响。", "Winter night, Chaisang. Inside the tent, only the brazier crackles.", "겨울밤, 시상. 장막 안에는 화로 소리만 탁탁 울립니다.");
            AddTranslation("dialogue_chibi1_opening_zhangzhao", "主公，曹操八十万大军，非人力可挡。不如暂且归顺，保全江东。", "My lord, Cao Cao's 800,000 troops cannot be stopped by human strength. Better to submit and preserve Jiangdong.", "주공, 조조의 80만 대군은 인력으로 막을 수 없습니다. 잠시 귀순하여 강동을 보전하시죠.");
            AddTranslation("dialogue_chibi1_opening_sunquan", "父兄基业，岂能拱手让人？", "How can I hand over my father's and brother's legacy?", "부형의 기업을 어찌 남에게 넘길 수 있겠습니까?");
            AddTranslation("dialogue_chibi1_sunquan_kill", "江东不是用来跪的！", "Jiangdong is not for kneeling!", "강동은 무릎 꿇을 곳이 아니다!");
            AddTranslation("dialogue_chibi1_zhangzhao_zhuhe", "一时之忍，未必不是长久之安！", "Temporary endurance may lead to lasting peace!", "일시적 인내가 오랜 평화를 가져올 수 있습니다!");
            AddTranslation("dialogue_chibi1_lusu_help", "主公，刘备尚在，尚可一战！", "My lord, Liu Bei still stands - we can still fight!", "주공, 유비가 아직 있으니 싸울 수 있습니다!");
            AddTranslation("dialogue_chibi1_chengpu_panic", "主公！江东不能没有你！", "My lord! Jiangdong cannot be without you!", "주공! 강동은 당신 없이 안 됩니다!");
            AddTranslation("dialogue_chibi1_zhangzhao_defeat", "主公……老臣愚钝了。", "My lord... this old servant was foolish.", "주공... 노신이 어리석었습니다.");
            AddTranslation("dialogue_chibi1_victory_1", "孤已决意！与曹操势不两立！", "My decision is made! Cao Cao and I cannot coexist!", "내 결심은 섰다! 조조와 양립할 수 없다!");
            AddTranslation("dialogue_chibi1_victory_2", "众臣：愿随主公死战！", "Ministers: We will fight to the death with our lord!", "신하들: 주공과 함께 죽을 때까지 싸우겠습니다!");

            // 对白 - 第二战
            AddTranslation("dialogue_chibi2_opening_1", "江面薄雾缭绕，船桨破水。", "Thin mist shrouds the river, oars break the water.", "강 위에 옅은 안개가 끼고 노가 물을 가릅니다.");
            AddTranslation("dialogue_chibi2_opening_zhouyu", "诸葛孔明？久闻大名。", "Zhuge Kongming? I've heard much about you.", "제갈공명? 오래전부터 이름을 들었소.");
            AddTranslation("dialogue_chibi2_zhugeliang_guanxing", "局未开，胜负已分三成。", "Before the game begins, thirty percent of victory is already decided.", "국이 펼쳐지기 전에 승부는 이미 30%가 결정됩니다.");
            AddTranslation("dialogue_chibi2_zhouyu_attack", "空谈之士，也会流血吗？", "Does a scholar of empty talk also bleed?", "공론만 하는 선비도 피를 흘리는가?");
            AddTranslation("dialogue_chibi2_lvmeng_warning", "都督，此人不可小觑。", "Commander, this man should not be underestimated.", "도독, 이 자를 가볍게 보면 안 됩니다.");
            AddTranslation("dialogue_chibi2_round4_narration", "江风渐急，船身微微倾斜。", "The river wind picks up, the boat tilts slightly.", "강바람이 거세지고 배가 살짝 기웁니다.");
            AddTranslation("dialogue_chibi2_zhouyu_question", "曹操水军连船，铁索横江。你这把火，从哪儿来？", "Cao Cao's navy chains ships together, iron links cross the river. Where will your fire come from?", "조조 수군이 배를 연결하고 철쇄가 강을 가로지르오. 그대의 불은 어디서 오는가?");
            AddTranslation("dialogue_chibi2_zhugeliang_answer", "从你我头顶。", "From above our heads.", "당신과 내 머리 위에서.");
            AddTranslation("dialogue_chibi2_victory", "好一个卧龙！这一战，瑜不再独行。", "What a Sleeping Dragon! In this battle, Yu will not fight alone.", "과연 와룡이로군! 이 싸움에서 유는 더 이상 홀로 싸우지 않겠소.");

            // 对白 - 第三战
            AddTranslation("dialogue_chibi3_opening_mifang", "不好了主公！赵云投降曹操了！", "Bad news, my lord! Zhao Yun has surrendered to Cao Cao!", "큰일입니다, 주공! 조운이 조조에게 항복했습니다!");
            AddTranslation("dialogue_chibi3_opening_liubei", "怎么可能，子龙是我的兄弟，怎么会背叛我？", "Impossible! Zilong is my brother - how could he betray me?", "말도 안 돼, 자룡은 내 형제인데 어떻게 배신할 수 있겠는가?");
            AddTranslation("dialogue_chibi3_victory", "大哥，那我去断后，你们先走！", "Big brother, let me hold the rear - you go first!", "형님, 제가 후방을 막을 테니 먼저 가세요!");

            // 对白 - 第四战
            AddTranslation("dialogue_chibi4_opening", "曹操亲自率领部队追了上来。", "Cao Cao personally leads the pursuit.", "조조가 직접 추격대를 이끌고 왔습니다.");
            AddTranslation("dialogue_chibi4_zhangfei_roar", "我乃燕人张翼德，谁敢与我决一死战！", "I am Zhang Yide of Yan! Who dares fight me to the death!", "나는 연인 장익덕이다! 누가 감히 나와 결사전을 벌이겠는가!");
            AddTranslation("dialogue_chibi4_xiahoujie_fear", "曹军骑将夏侯杰，惊骇失措。", "Cao's cavalry general Xiahou Jie is terrified and panics.", "조조군 기장 하후걸이 공포에 질려 당황합니다.");
            AddTranslation("dialogue_chibi4_caocao_retreat", "昔日云长就说过，张翼德可在百万军中取敌将首级。听我命令，全军后撤！", "Yunchang once said Zhang Yide could take an enemy general's head among a million troops. On my order, all forces retreat!", "예전에 운장이 말하길 장익덕은 백만 대군 속에서 적장의 목을 벨 수 있다고 했다. 내 명령이다, 전군 후퇴!");
            AddTranslation("dialogue_chibi4_zhangfei_order", "命令士卒，把这个桥给拆了。", "Order the soldiers to tear down this bridge.", "병사들에게 명령한다, 이 다리를 부숴라.");
            AddTranslation("dialogue_chibi4_narration_zhaoyun", "就在此时，赵云骑马而来，胸中挂着刘备的儿子刘禅。", "At this moment, Zhao Yun arrives on horseback, carrying Liu Bei's son Liu Shan at his chest.", "바로 그때, 조운이 말을 타고 와 품에 유비의 아들 유선을 안고 있었습니다.");
            AddTranslation("dialogue_chibi4_liubei_praise", "在百万军中带着我的儿子杀出重围，子龙一身都是胆啊！", "Fighting through a million troops with my son - Zilong is all courage!", "백만 대군 속에서 내 아들을 데리고 돌파하다니, 자룡은 온몸이 담력이구나!");

            // 对白 - 第五战
            AddTranslation("dialogue_chibi5_opening", "营帐之内，气氛压抑。", "Inside the tent, the atmosphere is oppressive.", "장막 안의 분위기가 무겁습니다.");
            AddTranslation("dialogue_chibi5_huanggai_request", "都督，这顿打，我来挨。", "Commander, let me take this beating.", "도독, 이 매는 제가 맞겠습니다.");
            AddTranslation("dialogue_chibi5_zhouyu_warning", "若你撑不住，计策便成空谈。", "If you can't endure, the plan becomes empty talk.", "네가 버티지 못하면 계책은 공염불이 됩니다.");
            AddTranslation("dialogue_chibi5_huanggai_hit", "这一棍，打得值！", "This beating is worth it!", "이 매는 값진 것이다!");
            AddTranslation("dialogue_chibi5_zhouyu_enough", "够了……别再硬撑。", "Enough... don't push yourself further.", "됐어... 더 이상 무리하지 마.");
            AddTranslation("dialogue_chibi5_caocao_believe", "周瑜用人，果然刻薄。", "Zhou Yu's treatment of people is truly harsh.", "주유의 사람 대하는 것이 정말 각박하군.");
            AddTranslation("dialogue_chibi5_jianggan_doubt", "奇怪……为何总觉得不对？", "Strange... why does something feel off?", "이상해... 왜 뭔가 이상한 느낌이 들지?");
            AddTranslation("dialogue_chibi5_victory_narration", "曹操收下降书。", "Cao Cao accepts the surrender letter.", "조조가 항복서를 받습니다.");
            AddTranslation("dialogue_chibi5_caocao_accept", "若黄盖真降，赤壁，不过一把火的事。", "If Huang Gai truly surrenders, Red Cliffs is just a matter of one fire.", "황개가 진짜 항복한다면, 적벽은 불 한 번이면 끝이다.");

            // 对白 - 第六战
            AddTranslation("dialogue_chibi6_opening", "夜色如墨，江水翻涌。", "Night dark as ink, the river surges.", "밤은 먹처럼 어둡고 강물이 출렁입니다.");
            AddTranslation("dialogue_chibi6_zhugeliang_wind", "东南风，三更到五更。", "Southeast wind, from the third to fifth watch.", "동남풍, 삼경부터 오경까지.");
            AddTranslation("dialogue_chibi6_zhouyu_fire", "那就三更点火。", "Then we light the fire at the third watch.", "그럼 삼경에 불을 붙이지.");
            AddTranslation("dialogue_chibi6_round2_conspiracy", "【密谋】黄盖自请苦肉，诈降曹营。火攻之计，悄然展开。", "[Conspiracy] Huang Gai volunteers for self-torture, feigning surrender to Cao's camp. The fire attack plan unfolds silently.", "[밀모] 황개가 고육을 자청하고 조조 진영에 거짓 항복합니다. 화공 계책이 은밀히 펼쳐집니다.");
            AddTranslation("dialogue_chibi6_round3_wind", "【天象】夜半江风突变，东风渐起。", "[Omen] At midnight the river wind shifts - the east wind rises.", "[천상] 한밤중 강바람이 바뀌고 동풍이 불기 시작합니다.");
            AddTranslation("dialogue_chibi6_huanggai_fire", "这一把火，烧给天下看！", "This fire burns for all to see!", "이 불길, 천하에 보여주마!");
            AddTranslation("dialogue_chibi6_caocao_trap", "不好！中计了！", "No! We've fallen into their trap!", "안 돼! 계략에 빠졌다!");
            AddTranslation("dialogue_chibi6_zhangliao_chaos", "主公，船阵已乱！", "My lord, the ship formation is in chaos!", "주공, 선진이 어지럽습니다!");
            AddTranslation("dialogue_chibi6_zhugeliang_strong", "风，正盛。", "The wind is at its strongest.", "바람이 가장 강합니다.");
            AddTranslation("dialogue_chibi6_caocao_heaven", "天不助我！", "Heaven does not help me!", "하늘이 나를 돕지 않는구나!");
            AddTranslation("dialogue_chibi6_zhouyu_humanity", "不是天，是你小看了人心！", "It's not heaven - you underestimated the human heart!", "하늘이 아니라, 네가 인심을 얕보았다!");
            AddTranslation("dialogue_chibi6_ending", "火光映红江面，曹军溃散，战船尽毁。赤壁之战，尘埃落定。联军稳住阵型，曹军已无再战之力。以少胜多，终于在乱世之中，看见一线生机。",
                "Fire illuminates the river red, Cao's army scatters, warships destroyed. The Battle of Red Cliffs is decided. The alliance holds formation while Cao's army has no strength to fight again. Victory against overwhelming odds - finally, a glimmer of hope in troubled times.",
                "불빛이 강을 붉게 물들이고 조조군이 흩어지며 전선이 불타버립니다. 적벽대전이 결판났습니다. 연합군은 대열을 유지하고 조조군은 더 이상 싸울 힘이 없습니다. 과감히 승리하여 마침내 난세에 한 줄기 희망이 보입니다.");

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