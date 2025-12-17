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
            AddTranslation("card_supply_shortage", "兵粮寸断", "Supply Shortage", "병량촌단");

            // 判定相关消息
            AddTranslation("msg_judgment", "{0} 进行【{1}】的判定", "{0} judging for [{1}]", "{0}이(가) [{1}]에 대해 판정");
            AddTranslation("msg_judgment_result", "判定结果：{0}{1}", "Judgment result: {0}{1}", "판정 결과: {0}{1}");
            AddTranslation("msg_indulgence_effect", "{0} 乐不思蜀生效，跳过出牌阶段", "{0}'s Indulgence takes effect, skip play phase", "{0}의 락불사촉 효과 발동, 출패 단계 건너뜀");
            AddTranslation("msg_indulgence_miss", "{0} 乐不思蜀未生效", "{0}'s Indulgence missed", "{0}의 락불사촉 무효화");
            AddTranslation("msg_lightning_hit", "闪电击中 {0}，受到3点雷电伤害", "Lightning struck {0}, dealing 3 damage", "번개가 {0}에게 적중, 3 피해");
            AddTranslation("msg_lightning_miss", "闪电未击中，传递给下家", "Lightning missed, passing to next player", "번개 빗나감, 다음 플레이어에게 전달");
            AddTranslation("msg_supply_shortage_effect", "{0} 兵粮寸断生效，跳过摸牌阶段", "{0}'s Supply Shortage takes effect, skip draw phase", "{0}의 병량촌단 효과 발동, 드로우 단계 건너뜀");
            AddTranslation("msg_supply_shortage_miss", "{0} 兵粮寸断未生效", "{0}'s Supply Shortage missed", "{0}의 병량촌단 무효화");

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
            AddTranslation("msg_nullify_required", "是否使用【无懈可击】？", "Use [Nullification]?", "[무효화]를 사용하시겠습니까?");
            AddTranslation("msg_peach_for_player", "请出【桃】救 {0}", "Play [Peach] to save {0}", "[도]를 내서 {0}을(를) 구하세요");
            AddTranslation("msg_near_death", "{0} 进入濒死状态！", "{0} is dying!", "{0}이(가) 빈사 상태!");
            AddTranslation("msg_saved_by_peach", "{0} 被 {1} 用【桃】救回", "{0} was saved by {1}'s [Peach]", "{0}이(가) {1}의 [도]로 구출됨");
            AddTranslation("msg_response_required", "请响应", "Please respond", "응답하세요");
            AddTranslation("msg_responded_dodge", "{0} 打出了【闪】", "{0} played [Dodge]", "{0}이(가) [섬]을 냄");
            AddTranslation("msg_responded_slash", "{0} 打出了【杀】", "{0} played [Slash]", "{0}이(가) [살]을 냄");
            AddTranslation("msg_no_dodge", "{0} 没有【闪】", "{0} has no [Dodge]", "{0}은(는) [섬]이 없음");
            AddTranslation("msg_no_slash", "{0} 没有【杀】", "{0} has no [Slash]", "{0}은(는) [살]이 없음");

            // 无懈可击消息
            AddTranslation("msg_nullify_trick", "{0} 使用【无懈可击】抵消【{1}】", "{0} used [Nullification] to cancel [{1}]", "{0}이(가) [무효화]로 [{1}]을(를) 무효화함");
            AddTranslation("msg_nullify_counter", "{0} 使用【无懈可击】反制", "{0} used [Nullification] to counter", "{0}이(가) [무효화]로 반제함");
            AddTranslation("msg_nullify_failed", "{0} 的【无懈可击】被反制", "{0}'s [Nullification] was countered", "{0}의 [무효화]가 반제됨");

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

            // 第一战：长坂先锋（教程）
            AddTranslation("battle_chibi_1", "长坂先锋", "Changban Vanguard", "장판 선봉");
            AddTranslation("battle_chibi_1_subtitle", "七进七出", "Seven In, Seven Out", "칠진칠출");
            AddTranslation("battle_chibi_1_desc", "曹军追兵将至！赵云必须击退先头部队，为刘备撤退争取时间。",
                "Cao's pursuers are coming! Zhao Yun must repel the vanguard to buy time for Liu Bei's retreat.",
                "조조군 추격대가 다가오고 있습니다! 조운이 선봉대를 격퇴하여 유비의 후퇴를 위한 시간을 벌어야 합니다.");
            AddTranslation("battle_chibi_1_briefing", "新手教程战斗。击败曹军骑兵即可获胜。熟悉【龙胆】技能的使用方法。",
                "Tutorial battle. Defeat the Cao cavalry to win. Learn how to use the [Longdan] skill.",
                "튜토리얼 전투입니다. 조조군 기병을 물리치면 승리합니다. [용담] 기술 사용법을 익히세요.");

            // 第二战：张飞断桥
            AddTranslation("battle_chibi_2", "张飞断桥", "Zhang Fei Breaks the Bridge", "장비 교량 파괴");
            AddTranslation("battle_chibi_2_subtitle", "燕人张翼德在此！", "I am Zhang Yide of Yan!", "연인 장익덕이 여기 있다!");
            AddTranslation("battle_chibi_2_desc", "曹操亲自率领部队追了上来。张飞单骑断桥，横矛立马，以一己之力阻挡曹军追击。",
                "Cao Cao personally leads troops in pursuit. Zhang Fei alone blocks the bridge with his spear to stop the pursuers.",
                "조조가 직접 군대를 이끌고 추격합니다. 장비가 홀로 다리를 막고 창을 세워 추격군을 저지합니다.");
            AddTranslation("battle_chibi_2_briefing", "击败夏侯杰即可获胜。夏侯杰体力降至2点时会触发【胆裂】，无法使用闪且伤害-1。张飞可无限出【杀】。",
                "Defeat Xiahou Jie to win. When his HP drops to 2, [Terror] triggers - he can't use Dodge and deals -1 damage. Zhang Fei can use unlimited [Slash].",
                "하후걸을 물리치면 승리합니다. 체력이 2가 되면 [담렬]이 발동되어 섬을 사용할 수 없고 피해가 -1됩니다. 장비는 무제한 [살]을 사용할 수 있습니다.");

            // 第三战：舌战群儒
            AddTranslation("battle_chibi_3", "舌战群儒", "Debate with Scholars", "설전군유");
            AddTranslation("battle_chibi_3_subtitle", "以言为剑", "Words as Swords", "말로 싸우다");
            AddTranslation("battle_chibi_3_desc", "诸葛亮只身入东吴，面对张昭、虞翻等主和派的刁难。他必须以雄辩之才，说服孙权联刘抗曹。",
                "Zhuge Liang enters Wu alone to face the peace advocates Zhang Zhao and Yu Fan. He must use his eloquence to persuade Sun Quan to ally with Liu against Cao.",
                "제갈량이 홀로 동오에 들어가 장소, 우번 등 화의파의 난관에 직면합니다. 웅변으로 손권을 설득하여 유비와 연합해야 합니다.");
            AddTranslation("battle_chibi_3_briefing", "击败张昭和虞翻，或存活6回合即可获胜。鲁肃会在旁协助。注意敌人的【主和】和【诘难】技能。",
                "Defeat Zhang Zhao and Yu Fan, or survive 6 turns to win. Lu Su will assist. Watch out for enemies' [Advocate Peace] and [Challenge] skills.",
                "장소와 우번을 물리치거나 6턴 생존시 승리합니다. 노숙이 도와줍니다. 적의 [주화]와 [힐난] 기술에 주의하세요.");

            // 第四战：蒋干盗书
            AddTranslation("battle_chibi_4", "蒋干盗书", "Jiang Gan Steals the Letter", "장간 도서");
            AddTranslation("battle_chibi_4_subtitle", "周郎妙计安天下", "Zhou Yu's Brilliant Scheme", "주유의 묘계");
            AddTranslation("battle_chibi_4_desc", "蒋干奉曹操之命前来劝降，周瑜将计就计，设下反间之局。诸葛亮在旁观察，共谋除掉曹军水军都督蔡瑁。",
                "Jiang Gan comes to persuade Zhou Yu to surrender. Zhou Yu turns the tables with a counter-intelligence scheme. Zhuge Liang observes, plotting to eliminate Cai Mao.",
                "장간이 조조의 명을 받아 항복을 권유하러 옵니다. 주유가 반간계를 꾸미고 제갈량이 관찰하며 채모를 제거할 계획을 세웁니다.");
            AddTranslation("battle_chibi_4_briefing", "累积3个反间标记即可获胜。发动【反间】获得标记。若蒋干连续3次成功【盗书】则失败。",
                "Accumulate 3 counter-intelligence marks to win. Use [Counter-Scheme] to gain marks. Lose if Jiang Gan succeeds 3 times with [Steal Documents].",
                "반간 표시 3개를 모으면 승리합니다. [반간]으로 표시를 얻으세요. 장간이 [도서]를 3번 성공하면 패배합니다.");

            // 第五战：江上对峙
            AddTranslation("battle_chibi_5", "江上对峙", "River Standoff", "강상 대치");
            AddTranslation("battle_chibi_5_subtitle", "风雨欲来", "The Storm is Coming", "폭풍 전야");
            AddTranslation("battle_chibi_5_desc", "刘备与关羽在江上与曹军水师对峙。曹军士兵不习水战，战力大减。只需坚守至援军到来。",
                "Liu Bei and Guan Yu face Cao's navy on the river. Cao's soldiers are not used to naval combat, greatly reducing their strength. Just hold until reinforcements arrive.",
                "유비와 관우가 강 위에서 조조 수군과 대치합니다. 조조군 병사들은 수전에 익숙하지 않아 전력이 크게 떨어집니다. 원군이 올 때까지 버티세요.");
            AddTranslation("battle_chibi_5_briefing", "击败所有曹军水兵或存活5回合即可获胜。曹军水兵拥有【北人】技能，手牌上限-1，出杀需弃牌。",
                "Defeat all Cao sailors or survive 5 turns to win. Cao sailors have [Northerner] skill: -1 hand limit, must discard when using Slash.",
                "모든 조조 수병을 물리치거나 5턴 생존시 승리합니다. 조조 수병은 [북인] 기술: 패 상한 -1, 살 사용시 버려야 합니다.");

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
            AddTranslation("char_yufan", "虞翻", "Yu Fan", "우번");
            AddTranslation("char_caimao", "蔡瑁", "Cai Mao", "채모");
            AddTranslation("char_caojun_sailor", "曹军水兵", "Cao Sailor", "조조군 수병");
            AddTranslation("char_soldier", "士兵", "Soldier", "병사");

            // 特殊规则
            AddTranslation("rule_zhangzhao_no_attack", "张昭不会主动攻击", "Zhang Zhao won't attack", "장소는 공격하지 않습니다");
            AddTranslation("rule_lusu_support", "鲁肃每回合可为友军补牌", "Lu Su can give cards to allies each turn", "노숙이 매 턴 아군에게 카드를 줍니다");
            AddTranslation("rule_zhouyu_support", "周瑜会适时提醒黄盖", "Zhou Yu will remind Huang Gai", "주유가 황개에게 조언합니다");
            AddTranslation("rule_retreat_debuff", "连日败退：刘备方初始手牌-1", "Continuous Retreat: Liu Bei's side starts with -1 cards", "연일 패퇴: 유비측 초기 패 -1장");
            AddTranslation("rule_chain_ships", "铁索连船：曹军攻击距离+1", "Chained Ships: Cao's army attack range +1", "철쇄연선: 조조군 공격 거리 +1");
            AddTranslation("rule_conspiracy", "密谋成功：火攻伤害+2", "Conspiracy Success: Fire damage +2", "밀모 성공: 화공 피해 +2");
            AddTranslation("rule_east_wind", "东风渐起：火攻伤害再+1", "East Wind Rising: Fire damage +1 more", "동풍: 화공 피해 추가 +1");

            // ==================== 赤壁之战v2 对白 ====================

            // 战役开场对白
            AddTranslation("dialogue_chibi_campaign_intro", "建安十三年，曹操挥师南下，荆州望风而降。刘备败走当阳，前路茫茫。在这绝境之中，一场改变天下的大战，即将拉开序幕……",
                "In the 13th year of Jian'an, Cao Cao marches south. Jingzhou surrenders at sight. Liu Bei flees in defeat at Dangyang, the road ahead unclear. In this desperate situation, a battle that will change the world is about to begin...",
                "건안 13년, 조조가 남하합니다. 형주가 풍문만 듣고 항복합니다. 유비는 당양에서 패하여 앞길이 막막합니다. 이 절망 속에서 천하를 바꿀 대전이 막을 올리려 합니다...");
            AddTranslation("dialogue_chibi_opening_zhuge1", "主公，曹军虽众，但远道而来，士卒疲惫。此时正是联合孙权，共抗曹操的良机。",
                "My lord, though Cao's army is large, they come from afar and the soldiers are weary. This is the perfect time to ally with Sun Quan against Cao Cao.",
                "주공, 조조군이 많지만 먼 길을 와서 병사들이 지쳐 있습니다. 지금이 손권과 연합하여 조조에 맞설 좋은 기회입니다.");
            AddTranslation("dialogue_chibi_opening_liubei1", "孔明所言极是。只是……我军新败，孙权可愿与我联手？",
                "Kongming speaks truly. But... our army has just been defeated. Will Sun Quan be willing to ally with us?",
                "공명의 말이 맞소. 하지만... 우리 군이 막 패했는데, 손권이 우리와 손을 잡으려 할까요?");
            AddTranslation("dialogue_chibi_opening_guanyu", "大哥放心，有我和三弟在，曹军休想轻易得逞！",
                "Rest assured, brother. With me and Third Brother here, Cao's army won't have their way easily!",
                "형님 안심하세요. 저와 삼제가 있으니 조조군이 쉽게 뜻을 이루지 못할 것입니다!");
            AddTranslation("dialogue_chibi_opening_zhuge2", "亮愿只身前往江东，说服孙权。但在此之前，我们必须先稳住阵脚。",
                "I am willing to go to Jiangdong alone to persuade Sun Quan. But first, we must secure our position.",
                "제가 홀로 강동에 가서 손권을 설득하겠습니다. 하지만 그 전에 우리 진영을 안정시켜야 합니다.");
            AddTranslation("dialogue_chibi_opening_liubei2", "好！子龙，你先率军殿后，为我军争取时间！",
                "Good! Zilong, you lead the rear guard first and buy us time!",
                "좋소! 자룡, 먼저 후방을 맡아 우리 군에 시간을 벌어주시오!");
            AddTranslation("dialogue_chibi_opening_hint", "【提示】接下来是教程战斗，熟悉基本操作。",
                "[Hint] Next is the tutorial battle. Learn the basic controls.",
                "[힌트] 다음은 튜토리얼 전투입니다. 기본 조작을 익히세요.");
            AddTranslation("dialogue_chibi_opening_soldier", "报——！曹军先锋已至！",
                "Report! Cao's vanguard has arrived!",
                "보고합니다! 조조군 선봉이 도착했습니다!");
            AddTranslation("dialogue_chibi_opening_zhaoyun", "主公放心，赵云在此，定保主公周全！",
                "Rest assured, my lord. With Zhao Yun here, I will definitely keep you safe!",
                "주공 안심하세요. 조운이 여기 있으니 반드시 주공을 지키겠습니다!");

            // 第一战：长坂先锋
            AddTranslation("dialogue_chibi1_zhaoyun_start", "来者何人？想挡我赵云的去路，先问问我手中的枪！",
                "Who goes there? If you want to block Zhao Yun's path, first ask my spear!",
                "누구냐? 조운의 길을 막으려면 먼저 내 창에게 물어봐라!");
            AddTranslation("dialogue_chibi1_longdan_tip", "【提示】龙胆技能：可将【杀】当【闪】使用，或将【闪】当【杀】使用。灵活运用，攻守兼备！",
                "[Tip] Longdan skill: Use [Slash] as [Dodge] or [Dodge] as [Slash]. Use flexibly for both offense and defense!",
                "[팁] 용담 기술: [살]을 [섬]으로, [섬]을 [살]로 사용할 수 있습니다. 공수 겸비로 유연하게 활용하세요!");
            AddTranslation("dialogue_chibi1_zhaoyun_win", "区区先锋，不足为惧。主公，我们走！",
                "A mere vanguard is nothing to fear. My lord, let's go!",
                "고작 선봉따위, 두려울 것 없소. 주공, 가시죠!");
            AddTranslation("dialogue_chibi1_mifang_rumor", "主公！不好了！有人传言，说赵云投降曹操了！",
                "My lord! Bad news! There are rumors that Zhao Yun has surrendered to Cao Cao!",
                "주공! 큰일입니다! 조운이 조조에게 항복했다는 소문이 있습니다!");
            AddTranslation("dialogue_chibi1_liubei_trust", "子龙断不会背叛我！他定是去救阿斗了！",
                "Zilong would never betray me! He must have gone to save A-Dou!",
                "자룡은 절대 나를 배신하지 않을 것이오! 틀림없이 아두를 구하러 갔을 거요!");
            AddTranslation("dialogue_chibi1_mifang_seen", "我亲眼所见！赵云往北方去了！",
                "I saw it with my own eyes! Zhao Yun went north!",
                "제가 직접 봤습니다! 조운이 북쪽으로 갔습니다!");
            AddTranslation("dialogue_chibi1_zhangfei_go", "大哥，那我去断后，你们先走！",
                "Big brother, then I'll hold the rear. You go first!",
                "형님, 그럼 제가 후방을 막겠습니다. 먼저 가세요!");

            // 第二战：张飞断桥
            AddTranslation("dialogue_chibi2_opening", "长坂桥上，张飞横矛立马，怒目圆睁。",
                "On Changban Bridge, Zhang Fei stands with his spear raised, eyes blazing with fury.",
                "장판교 위에서 장비가 창을 가로잡고 말 위에 서서 눈을 부릅뜨고 있습니다.");
            AddTranslation("dialogue_chibi2_zhangfei_roar", "我乃燕人张翼德！谁敢与我决一死战！",
                "I am Zhang Yide of Yan! Who dares fight me to the death!",
                "나는 연인 장익덕이다! 누가 감히 나와 결사전을 벌이겠는가!");
            AddTranslation("dialogue_chibi2_xiahoujie_fear", "夏侯杰面色惨白，身体不由自主地颤抖……",
                "Xiahou Jie's face turns pale, his body trembling involuntarily...",
                "하후걸의 얼굴이 창백해지고 몸이 저절로 떨립니다...");
            AddTranslation("dialogue_chibi2_xiahoujie_death", "夏侯杰惊骇过度，坠马而亡。",
                "Xiahou Jie dies from excessive terror, falling from his horse.",
                "하후걸이 과도한 공포로 말에서 떨어져 죽습니다.");
            AddTranslation("dialogue_chibi2_caocao_retreat", "云长曾言，翼德可于百万军中取上将首级。全军撤退！",
                "Yunchang once said Yide could take a general's head among a million troops. All forces retreat!",
                "운장이 말하길 익덕은 백만 대군 속에서 상장의 목을 벨 수 있다 했다. 전군 후퇴!");
            AddTranslation("dialogue_chibi2_zhangfei_order", "来人！把这桥给我拆了！",
                "Men! Tear down this bridge!",
                "여봐라! 이 다리를 부숴라!");
            AddTranslation("dialogue_chibi2_zhaoyun_arrive", "就在此时，赵云策马而至，怀中抱着刘禅。",
                "At that moment, Zhao Yun arrives on horseback, carrying Liu Shan in his arms.",
                "바로 그때 조운이 말을 타고 와서 품에 유선을 안고 있었습니다.");
            AddTranslation("dialogue_chibi2_zhaoyun_help", "三将军！主公在前方等候！",
                "Third General! The lord awaits ahead!",
                "삼장군! 주공이 앞에서 기다리고 계십니다!");
            AddTranslation("dialogue_chibi2_zhangfei_go", "好！我们走！",
                "Good! Let's go!",
                "좋다! 가자!");
            AddTranslation("dialogue_chibi2_liubei_praise", "子龙一身是胆！翼德威震天下！有你们在，刘备何惧曹操！",
                "Zilong is all courage! Yide's might shakes the world! With you two, why should Liu Bei fear Cao Cao!",
                "자룡은 온몸이 담력이고 익덕의 위엄이 천하를 진동시키는구나! 너희가 있으니 유비가 어찌 조조를 두려워하겠는가!");

            // 第三战：舌战群儒
            AddTranslation("dialogue_chibi3_opening", "诸葛亮只身入东吴，面对主和派的诘难。",
                "Zhuge Liang enters Eastern Wu alone, facing the challenges of the peace faction.",
                "제갈량이 홀로 동오에 들어가 화의파의 힐난에 직면합니다.");
            AddTranslation("dialogue_chibi3_zhangzhao_question", "刘备屡战屡败，先生何以认为他能抗曹？",
                "Liu Bei loses battle after battle. Why does the master believe he can resist Cao Cao?",
                "유비는 싸울 때마다 지는데, 선생은 왜 그가 조조에 맞설 수 있다고 생각하시오?");
            AddTranslation("dialogue_chibi3_zhuge_guanxing", "观星之术，可知天命。曹操虽强，但天命不在他。",
                "Through stargazing, one can know heaven's will. Though Cao Cao is strong, heaven's mandate is not with him.",
                "관성술로 천명을 알 수 있소. 조조가 강하지만 천명이 그에게 있지 않소.");
            AddTranslation("dialogue_chibi3_yufan_challenge", "好一张利嘴！只怕嘴上功夫，战场上可用不上！",
                "What a sharp tongue! But I fear such verbal skills are useless on the battlefield!",
                "입심 하나는 대단하군! 하지만 전장에서는 소용없을 것 같은데!");
            AddTranslation("dialogue_chibi3_zhangzhao_surrender", "降曹可保江东百姓平安，这难道不是大义？",
                "Surrendering to Cao can ensure peace for Jiangdong's people. Is this not righteousness?",
                "조조에게 항복하면 강동 백성이 평안할 수 있는데, 이것이 대의가 아니겠소?");
            AddTranslation("dialogue_chibi3_lusu_hint", "孔明先生，不必与他们争论太久。关键是说服吴侯。",
                "Master Kongming, no need to argue with them for too long. The key is to persuade Lord Wu.",
                "공명 선생, 그들과 너무 오래 논쟁할 필요 없습니다. 관건은 오후를 설득하는 것입니다.");
            AddTranslation("dialogue_chibi3_zhangzhao_defeat", "这……竟无言以对。",
                "This... I have no words to respond.",
                "이... 할 말이 없구려.");
            AddTranslation("dialogue_chibi3_zhuge_victory", "诸公，曹操虽有百万大军，但骄兵必败。孙刘联合，必能破曹！",
                "Gentlemen, though Cao Cao has a million troops, an arrogant army is doomed to fail. If Sun and Liu unite, Cao can be defeated!",
                "여러분, 조조에게 백만 대군이 있지만 교만한 군대는 반드시 패합니다. 손유가 연합하면 반드시 조조를 깨뜨릴 수 있습니다!");
            AddTranslation("dialogue_chibi3_lusu_report", "吴侯已决意抗曹！诸葛先生，请随我去见都督周瑜！",
                "Lord Wu has decided to resist Cao! Master Zhuge, please come with me to meet Commander Zhou Yu!",
                "오후가 항조를 결심하셨습니다! 제갈 선생, 저와 함께 도독 주유를 만나러 가시죠!");
            AddTranslation("dialogue_chibi3_alliance", "孙刘联盟，正式成立。",
                "The Sun-Liu Alliance is officially formed.",
                "손유 동맹이 정식으로 성립되었습니다.");

            // 第四战：蒋干盗书
            AddTranslation("dialogue_chibi4_opening", "蒋干奉曹操之命，前来劝降周瑜。",
                "Jiang Gan comes on Cao Cao's orders to persuade Zhou Yu to surrender.",
                "장간이 조조의 명을 받아 주유를 설득하러 왔습니다.");
            AddTranslation("dialogue_chibi4_zhouyu_plan", "蒋干此来，正好将计就计。孔明，你我配合，除掉蔡瑁张允！",
                "Jiang Gan's arrival is perfect for our scheme. Kongming, let's work together to eliminate Cai Mao and Zhang Yun!",
                "장간이 왔으니 마침 계략을 꾸밀 수 있겠소. 공명, 함께 채모와 장윤을 제거합시다!");
            AddTranslation("dialogue_chibi4_jianggan_steal", "蒋干：这封书信……莫非是蔡瑁张允与周瑜的密谋！",
                "Jiang Gan: This letter... could it be a secret plot between Cai Mao, Zhang Yun and Zhou Yu!",
                "장간: 이 서신은... 채모, 장윤과 주유의 밀모가 아닌가!");
            AddTranslation("dialogue_chibi4_zhouyu_drunk", "来来来，子翼，今日不醉不归！",
                "Come, come, Ziyi! Today we don't stop until we're drunk!",
                "자, 자익, 오늘은 취하지 않으면 돌아가지 않을 거야!");
            AddTranslation("dialogue_chibi4_jianggan_found", "就是这个！我要把这书信带回给丞相！",
                "This is it! I must bring this letter back to the Prime Minister!",
                "바로 이거야! 이 서신을 승상에게 가져가야겠다!");
            AddTranslation("dialogue_chibi4_caimao_loyal", "末将对丞相忠心耿耿，绝无二心！",
                "This general is absolutely loyal to the Prime Minister, with no second thoughts!",
                "소장은 승상에게 충성을 다하며 결코 다른 마음이 없습니다!");
            AddTranslation("dialogue_chibi4_jianggan_return", "蒋干连夜赶回曹营，将书信呈上。",
                "Jiang Gan rushes back to Cao's camp overnight and presents the letter.",
                "장간이 밤새 조조 진영으로 돌아가 서신을 바칩니다.");
            AddTranslation("dialogue_chibi4_caocao_kill", "蔡瑁张允，竟敢通敌！来人，推出去斩了！",
                "Cai Mao and Zhang Yun dare to collude with the enemy! Guards, take them out and execute them!",
                "채모와 장윤이 감히 적과 내통하다니! 여봐라, 끌어내어 참수하라!");
            AddTranslation("dialogue_chibi4_caimao_dead", "蔡瑁张允被斩，曹军水军群龙无首。",
                "Cai Mao and Zhang Yun are executed. Cao's navy is left leaderless.",
                "채모와 장윤이 참수되어 조조 수군이 지휘관을 잃었습니다.");
            AddTranslation("dialogue_chibi4_zhouyu_laugh", "哈哈哈！曹操自断臂膀，赤壁之战，胜算又多三分！",
                "Hahaha! Cao Cao cuts off his own arms. Our chances of victory at Red Cliffs just increased by thirty percent!",
                "하하하! 조조가 스스로 팔을 자르는구나. 적벽 대전의 승산이 30% 더 늘었다!");

            // 第五战：江上对峙
            AddTranslation("dialogue_chibi5_opening", "联军与曹军隔江对峙，决战在即。",
                "The allied forces face Cao's army across the river. The decisive battle is imminent.",
                "연합군과 조조군이 강을 사이에 두고 대치합니다. 결전이 임박했습니다.");
            AddTranslation("dialogue_chibi5_liubei_worry", "曹军水师虽失主帅，但兵力仍然庞大……",
                "Though Cao's navy lost its commander, their forces are still massive...",
                "조조 수군이 주장을 잃었지만 병력은 여전히 막대합니다...");
            AddTranslation("dialogue_chibi5_sailor_sick", "曹军士兵不习水战，纷纷晕船。",
                "Cao's soldiers are not used to naval combat and many get seasick.",
                "조조군 병사들이 수전에 익숙하지 않아 배멀미를 합니다.");
            AddTranslation("dialogue_chibi5_guanyu_kill", "北方士卒，不过如此！",
                "Northern soldiers are nothing but this!",
                "북방 병사들이 고작 이 정도인가!");
            AddTranslation("dialogue_chibi5_liubei_wait", "关羽，不可恋战！我们的目标是拖延时间！",
                "Guan Yu, don't get too caught up in battle! Our goal is to buy time!",
                "관우, 싸움에 빠지지 마시오! 우리 목표는 시간을 버는 것이오!");
            AddTranslation("dialogue_chibi5_reinforcement", "远处江面上，周瑜的战船正在接近！",
                "In the distance, Zhou Yu's warships are approaching!",
                "멀리 강 위에서 주유의 전선이 다가오고 있습니다!");
            AddTranslation("dialogue_chibi5_zhouyu_arrive", "刘皇叔！援军到了！准备发起总攻！",
                "Lord Liu! Reinforcements have arrived! Prepare for the final assault!",
                "유황숙! 원군이 도착했습니다! 총공격을 준비하세요!");
            AddTranslation("dialogue_chibi5_zhuge_wind", "都督，今夜子时，必有东南风！",
                "Commander, tonight at midnight, there will surely be a southeast wind!",
                "도독, 오늘 밤 자정에 반드시 동남풍이 불 것입니다!");
            AddTranslation("dialogue_chibi5_huanggai_fire", "火船已备！只等风起！",
                "The fire ships are ready! We only wait for the wind!",
                "화선 준비 완료! 바람만 기다립니다!");
            AddTranslation("dialogue_chibi5_alliance_formed", "孙刘联军，整装待发。决战，即将开始！",
                "The Sun-Liu allied forces are ready. The decisive battle is about to begin!",
                "손유 연합군이 준비를 마쳤습니다. 결전이 곧 시작됩니다!");

            // 第六战：赤壁火起
            AddTranslation("dialogue_chibi6_opening", "夜色如墨，江水翻涌。东南风起，火攻之时已到。",
                "Night dark as ink, the river surges. The southeast wind rises - the time for the fire attack has come.",
                "밤은 먹처럼 어둡고 강물이 출렁입니다. 동남풍이 불기 시작하니 화공의 때가 왔습니다.");
            AddTranslation("dialogue_chibi6_zhuge_wind", "风起了！东南风！",
                "The wind rises! Southeast wind!",
                "바람이 분다! 동남풍이다!");
            AddTranslation("dialogue_chibi6_zhouyu_fire", "黄盖！点火！",
                "Huang Gai! Light the fire!",
                "황개! 불을 붙여라!");
            AddTranslation("dialogue_chibi6_round2_conspiracy", "【密谋成功】黄盖率领火船冲向曹军水寨，火攻伤害+2！",
                "[Conspiracy Success] Huang Gai leads fire ships charging toward Cao's naval camp. Fire damage +2!",
                "[밀모 성공] 황개가 화선을 이끌고 조조 수채로 돌진합니다. 화공 피해 +2!");
            AddTranslation("dialogue_chibi6_round3_wind", "【东风大作】东南风越刮越猛，火势蔓延！火攻伤害再+1！",
                "[East Wind Surges] The southeast wind blows stronger, fire spreads! Fire damage +1 more!",
                "[동풍 대작] 동남풍이 더욱 거세지고 불길이 번집니다! 화공 피해 추가 +1!");
            AddTranslation("dialogue_chibi6_huanggai_fire", "曹贼！受死吧！",
                "Cao villain! Meet your death!",
                "조조 역적! 죽어라!");
            AddTranslation("dialogue_chibi6_huanggai_kill", "这把火，烧的值！",
                "This fire is worth it!",
                "이 불길, 값진 것이다!");
            AddTranslation("dialogue_chibi6_caocao_trap", "中计了！快撤！",
                "We've fallen into their trap! Retreat quickly!",
                "계략에 빠졌다! 빨리 후퇴하라!");
            AddTranslation("dialogue_chibi6_zhangliao_chaos", "主公！战船都连在一起，无法散开！",
                "My lord! The warships are all chained together and cannot separate!",
                "주공! 전선이 모두 연결되어 있어 흩어질 수가 없습니다!");
            AddTranslation("dialogue_chibi6_zhuge_strong", "天时地利人和，缺一不可。曹操，你输了。",
                "The right time, place, and people - all three are indispensable. Cao Cao, you have lost.",
                "천시, 지리, 인화 - 하나라도 빠지면 안 됩니다. 조조, 당신이 졌습니다.");
            AddTranslation("dialogue_chibi6_xiahoudun_retreat", "主公，快走华容道！",
                "My lord, quickly take the Huarong Road!",
                "주공, 빨리 화용도로 가세요!");
            AddTranslation("dialogue_chibi6_caocao_heaven", "天不亡我曹操！今日虽败，他日必报此仇！",
                "Heaven will not destroy me, Cao Cao! Though defeated today, I will surely avenge this one day!",
                "하늘이 나 조조를 망하게 하지 않을 것이다! 오늘은 패했지만 언젠가 반드시 이 원수를 갚겠다!");
            AddTranslation("dialogue_chibi6_zhouyu_humanity", "曹操败了！赤壁大捷！",
                "Cao Cao is defeated! Great victory at Red Cliffs!",
                "조조가 패했다! 적벽 대첩이다!");
            AddTranslation("dialogue_chibi6_ending1", "火光映红江面，曹军溃不成军。赤壁之战，以孙刘联军的胜利告终。",
                "Fire illuminates the river red, Cao's army collapses. The Battle of Red Cliffs ends with victory for the Sun-Liu Alliance.",
                "불빛이 강을 붉게 물들이고 조조군이 무너집니다. 적벽 대전이 손유 연합군의 승리로 끝났습니다.");
            AddTranslation("dialogue_chibi6_ending2", "这一战，奠定了三分天下的格局。乱世之中，终于看到了一线生机。",
                "This battle established the pattern of three kingdoms. In troubled times, a glimmer of hope finally appears.",
                "이 전투로 천하삼분의 구도가 확립되었습니다. 난세 속에서 마침내 한 줄기 희망이 보입니다.");

            // ==================== 讨董之战 (Campaign Against Dong Zhuo) ====================
            AddTranslation("campaign_taodong", "讨董之战", "Campaign Against Dong Zhuo", "동탁 토벌전");
            AddTranslation("campaign_taodong_desc", "董卓挟天子以令诸侯，关东诸侯会盟讨伐。然而联军各怀心思，注定是一场失败的勤王之战。",
                "Dong Zhuo controls the emperor to command the lords. The eastern lords ally to campaign against him, but each has their own agenda - a doomed campaign to save the emperor.",
                "동탁이 천자를 끼고 제후들을 호령합니다. 관동 제후들이 동맹을 맺어 토벌에 나서지만, 각자 속셈이 달라 실패할 운명의 근왕전입니다.");

            // 讨董之战 - 第一战：酸枣会盟
            AddTranslation("battle_taodong_1", "酸枣会盟", "Suanzao Alliance", "산조 회맹");
            AddTranslation("battle_taodong_1_subtitle", "讨董联军，各怀心思", "United Against Dong Zhuo, Yet Divided", "동탁 토벌, 그러나 각자 속셈");
            AddTranslation("battle_taodong_1_desc", "酸枣城外，关东诸侯齐聚。名为讨董，实则人心难测。曹操必须在这场政治博弈中站稳脚跟。",
                "Outside Suanzao city, eastern lords gather. United against Dong Zhuo in name, but hearts are hard to read. Cao Cao must establish himself in this political game.",
                "산조성 밖에 관동 제후들이 모였습니다. 명목상 동탁 토벌이지만 인심을 알기 어렵습니다. 조조는 이 정치적 게임에서 입지를 다져야 합니다.");
            AddTranslation("battle_taodong_1_briefing", "击败诸侯观望或存活6回合即可获胜。袁绍袁术不可攻击。",
                "Defeat Lords' Hesitation or survive 6 turns to win. Cannot attack Yuan Shao or Yuan Shu.",
                "제후 관망을 격파하거나 6턴 생존하면 승리. 원소와 원술을 공격할 수 없습니다.");

            // 讨董之战 - 第二战：荥阳血战
            AddTranslation("battle_taodong_2", "荥阳血战", "Battle of Xingyang", "형양 혈전");
            AddTranslation("battle_taodong_2_subtitle", "孤军深入", "Isolated Army", "고군 심입");
            AddTranslation("battle_taodong_2_desc", "诸侯按兵不动，曹操愤而独进，在荥阳遭遇徐荣。这是一场以寡敌众的血战。",
                "While lords hold back, Cao Cao advances alone in anger, meeting Xu Rong at Xingyang. A bloody battle against overwhelming odds.",
                "제후들이 움직이지 않자 조조가 분노하여 홀로 진격하고 형양에서 서영을 만납니다. 압도적 적과의 혈전입니다.");
            AddTranslation("battle_taodong_2_briefing", "击败徐荣或存活5回合即可获胜。曹操初始手牌-1，首回合不能用桃。",
                "Defeat Xu Rong or survive 5 turns to win. Cao Cao starts with -1 hand cards and cannot use Peach in first turn.",
                "서영을 격파하거나 5턴 생존하면 승리. 조조 초기 수패-1, 첫 턴 도 사용 불가.");

            // 讨董之战 - 第三战：斩杀华雄
            AddTranslation("battle_taodong_3", "斩杀华雄", "Slay Hua Xiong", "화웅 참살");
            AddTranslation("battle_taodong_3_subtitle", "江东猛虎", "Tiger of Jiangdong", "강동의 맹호");
            AddTranslation("battle_taodong_3_desc", "汜水关前，华雄连斩联军数将。孙坚亲率江东子弟迎战！",
                "Before Si River Pass, Hua Xiong slays several allied generals. Sun Jian personally leads Jiangdong's sons to battle!",
                "사수관 앞에서 화웅이 연합군 장수 여럿을 베었습니다. 손견이 친히 강동 자제를 이끌고 싸웁니다!");
            AddTranslation("battle_taodong_3_briefing", "击败华雄即可获胜。第1回合孙坚可额外出1杀。",
                "Defeat Hua Xiong to win. Sun Jian can use an extra Slash in turn 1.",
                "화웅을 격파하면 승리. 1턴에 손견은 살 1장 추가 사용 가능.");

            // 讨董之战 - 第四战：联军瓦解
            AddTranslation("battle_taodong_4", "联军瓦解", "Alliance Collapses", "연합군 와해");
            AddTranslation("battle_taodong_4_subtitle", "大厦将倾", "The Great Edifice Falls", "대하 장경");
            AddTranslation("battle_taodong_4_desc", "董卓焚毁洛阳迁都长安，联军失去目标。袁氏兄弟争权，诸侯各自为战。",
                "Dong Zhuo burns Luoyang and moves to Chang'an. The alliance loses its purpose. Yuan brothers fight for power, lords go separate ways.",
                "동탁이 낙양을 불태우고 장안으로 천도합니다. 연합군은 목표를 잃습니다. 원씨 형제가 권력을 다투고 제후들은 각자 도생합니다.");
            AddTranslation("battle_taodong_4_briefing", "击败联军内斗或存活7回合即可获胜。第3回合开始所有角色每回合失去1体力。",
                "Defeat Alliance Strife or survive 7 turns to win. From turn 3, all characters lose 1 HP each turn.",
                "연합 내분을 격파하거나 7턴 생존하면 승리. 3턴부터 모든 캐릭터가 매 턴 체력 1 손실.");

            // 讨董之战对话
            AddTranslation("dialogue_taodong1_opening_narration", "酸枣城外，旌旗蔽日，关东诸侯齐聚一堂。名为讨董，实则人心难测。",
                "Outside Suanzao, banners block the sun as eastern lords gather. United against Dong Zhuo in name, but hearts remain unfathomable.",
                "산조성 밖에 깃발이 해를 가리고 관동 제후들이 모였습니다. 명목상 동탁 토벌이지만 인심은 알 수 없습니다.");
            AddTranslation("dialogue_taodong1_yuanshao_speech", "董卓乱政，天人共愤！今日推我为盟主，绍当竭尽全力，统率诸军，共讨国贼！",
                "Dong Zhuo's tyranny angers heaven and man! Today you elect me as alliance leader - I shall lead all armies to destroy the traitor!",
                "동탁의 난정에 천인이 공분합니다! 오늘 저를 맹주로 추대하셨으니 전력을 다해 제군을 이끌고 국적을 토벌하겠습니다!");
            AddTranslation("dialogue_taodong1_caocao_doubt", "人虽多，却未必同心……此战，恐不易。",
                "Many in number, but not necessarily united... this battle will not be easy.",
                "사람은 많으나 한마음인지... 이 싸움은 쉽지 않겠군.");
            AddTranslation("dialogue_taodong1_yuanshu_greedy", "粮草是我的，凭什么全给别人？",
                "The supplies are mine - why should I share them all?",
                "군량은 내 것인데 왜 다 남에게 줘야 하는가?");
            AddTranslation("dialogue_taodong1_consensus", "在曹操的周旋下，联军勉强达成共识。",
                "Through Cao Cao's efforts, the alliance barely reaches consensus.",
                "조조의 중재로 연합군이 간신히 합의에 도달했습니다.");
            AddTranslation("dialogue_taodong1_victory_caocao", "联军虽立，却迟迟不进。真正的战争，尚未开始。",
                "The alliance stands, but hesitates to advance. The real war has not yet begun.",
                "연합군이 섰으나 진격을 머뭇거립니다. 진짜 전쟁은 아직 시작되지 않았습니다.");
            AddTranslation("dialogue_taodong1_victory_xiahoudun", "主公，我等何时出兵？",
                "My lord, when do we march?",
                "주공, 언제 출병합니까?");
            AddTranslation("dialogue_taodong1_victory_caocao2", "等不及了……我们先动！",
                "Can't wait any longer... we move first!",
                "더 기다릴 수 없다... 우리가 먼저 움직인다!");

            AddTranslation("dialogue_taodong2_opening_caocao", "诸侯坐视不前，岂是讨贼之道！我曹操，纵死亦不愿苟安！",
                "The lords sit idle - is this how we defeat the traitor! I, Cao Cao, would rather die than cower!",
                "제후들이 앉아서 관망하다니, 이것이 역적 토벌입니까! 나 조조는 차라리 죽을지언정 구차히 안주하지 않겠소!");
            AddTranslation("dialogue_taodong2_xiahoudun_loyal", "主公既决，我等誓死相随！",
                "If my lord is resolved, we follow to the death!",
                "주공께서 결심하셨으니 저희는 죽음으로 따르겠습니다!");
            AddTranslation("dialogue_taodong2_xurong_mock", "就凭你这点兵马，也敢犯我西凉军？",
                "With such few troops, you dare attack my Xiliang army?",
                "이 정도 병력으로 감히 내 서량군에 덤비겠다고?");
            AddTranslation("dialogue_taodong2_caocao_wounded", "曹操被流矢所伤，血染战袍。",
                "Cao Cao is wounded by a stray arrow, blood staining his robe.",
                "조조가 유시에 맞아 피가 전포를 물들입니다.");
            AddTranslation("dialogue_taodong2_caocao_vow", "董卓未除，天下难安……此仇，他日必报！",
                "Until Dong Zhuo falls, the realm knows no peace... This grudge will be repaid!",
                "동탁을 제거하지 않으면 천하가 편안할 수 없다... 이 원한은 반드시 갚으리라!");
            AddTranslation("dialogue_taodong2_victory_narration", "曹操终因寡不敌众，被流矢所伤，只得夜退。然其英勇之名，传遍天下。",
                "Cao Cao, outnumbered, is wounded and forced to retreat by night. Yet his brave reputation spreads throughout the land.",
                "조조는 중과부적으로 부상당하고 야간에 퇴각해야 했습니다. 그러나 그의 용맹한 이름은 천하에 퍼졌습니다.");
            AddTranslation("dialogue_taodong2_victory_caocao", "今日之败，他日必十倍奉还！",
                "Today's defeat will be repaid tenfold!",
                "오늘의 패배는 언젠가 열 배로 갚겠다!");

            AddTranslation("dialogue_taodong3_opening_sunjian", "董卓暴行，人神共愤！今日不破敌军，誓不回师！",
                "Dong Zhuo's atrocities anger men and gods! Today we break the enemy or never return!",
                "동탁의 폭행에 인신이 공분합니다! 오늘 적군을 깨지 못하면 돌아가지 않겠습니다!");
            AddTranslation("dialogue_taodong3_huaxiong_mock", "关东鼠辈，也敢与我一战？哈哈哈！",
                "Eastern rats dare fight me? Ha ha ha!",
                "관동 쥐새끼들이 감히 나와 싸우겠다고? 하하하!");
            AddTranslation("dialogue_taodong3_huanggai_brave", "这点伤算什么，给我上！",
                "This wound is nothing - charge!",
                "이 정도 상처가 뭐야, 돌격!");
            AddTranslation("dialogue_taodong3_chengpu_praise", "主公神勇！江东儿郎，随我杀敌！",
                "My lord is mighty! Jiangdong sons, follow me to slay the enemy!",
                "주공이 신용하십니다! 강동 자제들이여, 나를 따라 적을 죽여라!");
            AddTranslation("dialogue_taodong3_huaxiong_shocked", "什么？这孙坚竟如此厉害！",
                "What? This Sun Jian is so formidable!",
                "뭐라고? 이 손견이 이렇게 대단할 줄이야!");
            AddTranslation("dialogue_taodong3_sunjian_victory", "董卓走狗，不过如此！",
                "Dong Zhuo's lackey - nothing more!",
                "동탁의 앞잡이, 별것 아니군!");
            AddTranslation("dialogue_taodong3_victory_narration", "华雄已被斩杀！",
                "Hua Xiong has been slain!",
                "화웅이 참살되었습니다!");
            AddTranslation("dialogue_taodong3_victory_sunjian", "传首各营，以振军心！进军洛阳！",
                "Send his head to all camps to raise morale! March on Luoyang!",
                "목을 각 진영에 보내 군심을 높여라! 낙양으로 진군!");

            AddTranslation("dialogue_taodong4_opening_narration", "洛阳化为焦土，百姓流离失所。联军虽进，却已失去目标。",
                "Luoyang becomes scorched earth, people displaced. The alliance advances but has lost its purpose.",
                "낙양이 초토화되고 백성들이 유리걸식합니다. 연합군이 진격했으나 이미 목표를 잃었습니다.");
            AddTranslation("dialogue_taodong4_yuanshu_argue", "粮草是我的，凭什么全给你？",
                "The supplies are mine - why give them all to you?",
                "군량은 내 것인데 왜 다 네게 줘야 해?");
            AddTranslation("dialogue_taodong4_yuanshao_blame", "你这是误国！",
                "You're ruining the nation!",
                "네가 나라를 망치는 거야!");
            AddTranslation("dialogue_taodong4_round3_ruins", "董卓焚城的余烬仍在燃烧，诸侯却已开始内斗。",
                "The embers of Dong Zhuo's burning city still glow, yet the lords already fight among themselves.",
                "동탁이 불태운 성의 잿더미가 아직 타고 있는데 제후들은 이미 내분을 시작했습니다.");
            AddTranslation("dialogue_taodong4_sunjian_lament", "可恨！讨董大业，竟毁于诸侯之手！",
                "Curse it! The great campaign against Dong Zhuo, ruined by the lords themselves!",
                "통탄스럽다! 동탁 토벌 대업이 제후들 손에 무너지다니!");
            AddTranslation("dialogue_taodong4_caocao_ambition", "讨董未成，诸侯已散……天下大乱，正是英雄用武之时！",
                "Dong Zhuo remains, the lords scatter... Great chaos - the time for heroes to act!",
                "동탁 토벌은 실패하고 제후들은 흩어지는군... 천하대란, 바로 영웅이 활약할 때다!");
            AddTranslation("dialogue_taodong4_yuanshao_sigh", "一场勤王之战，竟落得如此下场。",
                "A campaign to save the emperor, ending like this.",
                "근왕전이 이런 결말을 맞다니.");
            AddTranslation("dialogue_taodong4_victory_caocao", "董卓虽未除，但天下格局已变。这乱世，我曹操必将有所作为！",
                "Though Dong Zhuo remains, the world has changed. In this chaos, I, Cao Cao, will make my mark!",
                "동탁은 제거하지 못했으나 천하 판도가 바뀌었다. 이 난세에서 나 조조는 반드시 대업을 이루리라!");
            AddTranslation("dialogue_taodong4_ending_narration", "讨董联军瓦解，勤王大业功亏一篑。然而，曹操、孙坚等人在战火中崭露头角，新的时代，即将到来……",
                "The alliance collapses, the emperor's rescue fails. Yet Cao Cao, Sun Jian and others emerge from the flames. A new era is about to begin...",
                "연합군이 와해되고 근왕 대업이 수포로 돌아갔습니다. 그러나 조조, 손견 등이 전란 속에서 두각을 나타내고 새로운 시대가 열리려 합니다...");

            // ==================== 官渡之战 (Battle of Guandu) ====================
            AddTranslation("campaign_guandu_desc", "建安四年，北方霸主袁绍与曹操决战官渡。兵力悬殊，却以弱胜强，奠定曹操北方霸业。",
                "In 199 AD, northern hegemon Yuan Shao and Cao Cao clash at Guandu. Despite inferior numbers, the weak defeats the strong, establishing Cao Cao's northern dominance.",
                "건안 4년, 북방 패자 원소와 조조가 관도에서 결전합니다. 병력이 열세임에도 약자가 강자를 이기며 조조의 북방 패업을 확립합니다.");

            // 官渡之战 - 第一战：袁营决策
            AddTranslation("battle_guandu_1", "袁营决策", "Yuan's Camp Decision", "원영 결책");
            AddTranslation("battle_guandu_1_subtitle", "兵强马壮，暗藏隐患", "Strong Army, Hidden Troubles", "강병하나 숨은 우환");
            AddTranslation("battle_guandu_1_desc", "袁绍召集谋士决定是否南下攻曹。表面气势如虹，实则内部意见分裂。",
                "Yuan Shao gathers advisors to decide on attacking Cao Cao south. Seemingly unstoppable, but internally divided.",
                "원소가 모사들을 모아 조조 남하 공격을 결정합니다. 겉으로는 기세등등하나 내부는 분열되어 있습니다.");
            AddTranslation("battle_guandu_1_briefing", "击败军心动摇和粮草隐患即可获胜。注意内部不和规则会随机弃牌。",
                "Defeat Morale Problem and Supply Problem to win. Watch for Internal Strife forcing random discards.",
                "군심동요와 군량우환을 격파하면 승리. 내부불화 규칙으로 무작위 패 버림 주의.");

            // 官渡之战 - 第二战：坚守官渡
            AddTranslation("battle_guandu_2", "坚守官渡", "Defend Guandu", "관도 수비");
            AddTranslation("battle_guandu_2_subtitle", "以少敌多", "The Few Against Many", "과소적다");
            AddTranslation("battle_guandu_2_desc", "曹操在官渡布防，以守为攻。兵力悬殊，唯有智取。",
                "Cao Cao fortifies Guandu, defending to attack. Outnumbered, only cunning can prevail.",
                "조조가 관도에서 방어진을 치고 수비로 공격합니다. 병력이 열세이니 지략만이 승리의 길입니다.");
            AddTranslation("battle_guandu_2_briefing", "击败袁绍或存活8回合即可获胜。官渡要塞前两次伤害各-1。",
                "Defeat Yuan Shao or survive 8 turns to win. Guandu Fortress reduces first two damages by 1 each.",
                "원소를 격파하거나 8턴 생존하면 승리. 관도요새 첫 두 피해 각각 -1.");

            // 官渡之战 - 第三战：许攸献策
            AddTranslation("battle_guandu_3", "许攸献策", "Xu You's Strategy", "허유의 헌책");
            AddTranslation("battle_guandu_3_subtitle", "乌巢，胜负手", "Wuchao, The Decisive Move", "오소, 승부수");
            AddTranslation("battle_guandu_3_desc", "许攸因家人被扣押愤而投奔曹操，献上袁军粮仓乌巢的情报。",
                "Xu You, angry that his family was detained, defects to Cao Cao with intelligence about Yuan's supply depot at Wuchao.",
                "허유가 가족이 구금되자 분노하여 조조에게 투항하고 원군 군량창고 오소의 정보를 바칩니다.");
            AddTranslation("battle_guandu_3_briefing", "击败袁绍即可获胜。火攻火杀效果翻倍。袁绍每回合被迫弃牌。",
                "Defeat Yuan Shao to win. Fire attacks have double effect. Yuan Shao forced to discard each turn.",
                "원소를 격파하면 승리. 화공/화살 효과 2배. 원소 매 턴 강제 패 버림.");

            // 官渡之战 - 第四战：火烧乌巢
            AddTranslation("battle_guandu_4", "火烧乌巢", "Burn Wuchao", "오소 화공");
            AddTranslation("battle_guandu_4_subtitle", "天下归曹", "The World Falls to Cao", "천하귀조");
            AddTranslation("battle_guandu_4_desc", "曹操亲率精兵五千，夜袭袁军粮仓乌巢。不成功，便成仁！",
                "Cao Cao personally leads 5000 elite troops in a night raid on Wuchao. Victory or death!",
                "조조가 친히 정예병 5천을 이끌고 오소를 야습합니다. 성공 아니면 죽음!");
            AddTranslation("battle_guandu_4_briefing", "击败淳于琼即可获胜。第2回合开始敌方每回合失去1体力。曹操手牌<=2时杀伤害+1。",
                "Defeat Chunyu Qiong to win. From turn 2, enemies lose 1 HP per turn. Cao Cao's Slash +1 damage when hand <=2.",
                "순우경을 격파하면 승리. 2턴부터 적 매 턴 체력-1. 조조 수패<=2일 때 살 피해+1.");

            // 官渡之战对话
            AddTranslation("dialogue_guandu1_opening_yuanshao", "曹操挟天子以令诸侯，狼子野心！今日便要定下南下大计！",
                "Cao Cao controls the emperor to command lords - wolf's ambition! Today we decide the southern campaign!",
                "조조가 천자를 끼고 제후를 호령하니 늑대의 야심이다! 오늘 남하 대계를 정한다!");
            AddTranslation("dialogue_guandu1_tianfeng_warn", "主公不可轻进！曹操善用兵，我军远征粮道过长！",
                "My lord, don't advance rashly! Cao Cao excels at warfare, our supply lines are too long!",
                "주공, 경솔히 진격하지 마십시오! 조조는 용병에 능하고 우리 군량로가 너무 깁니다!");
            AddTranslation("dialogue_guandu1_yuanshao_confident", "我袁绍四世三公，兵马数十万，难道还怕他曹操？",
                "I, Yuan Shao, come from four generations of ministers, with hundreds of thousands of troops - should I fear Cao Cao?",
                "나 원소는 사세삼공의 가문에 수십만 병마를 거느렸는데 조조가 두렵단 말인가?");
            AddTranslation("dialogue_guandu1_shenpei_argue", "田丰言辞过激，未免助长怯战之风。",
                "Tian Feng's words are too harsh - he encourages cowardice.",
                "전풍의 말이 너무 과격하여 겁쟁이 기풍을 조장합니다.");
            AddTranslation("dialogue_guandu1_jushou_stable", "军心已稳，可以出兵了。",
                "Morale is stable - we can march.",
                "군심이 안정되었으니 출병할 수 있습니다.");
            AddTranslation("dialogue_guandu1_victory_yuanshao", "够了！我意已决，三军整备，即日南下！",
                "Enough! My decision is made - all armies prepare, we march south today!",
                "됐다! 내 결심은 섰다, 삼군 정비하라, 오늘 남하한다!");
            AddTranslation("dialogue_guandu1_victory_narration", "田丰低头长叹，神情忧虑。",
                "Tian Feng lowers his head and sighs deeply, his expression worried.",
                "전풍이 고개를 숙이고 길게 탄식하며 걱정스러운 표정을 짓습니다.");

            AddTranslation("dialogue_guandu2_opening_caocao", "袁绍兵多，但迟疑不决。官渡若守得住，胜负未可知。",
                "Yuan Shao has many troops, but hesitates. If we hold Guandu, victory is uncertain.",
                "원소는 병력이 많으나 우유부단합니다. 관도를 지키면 승부를 알 수 없습니다.");
            AddTranslation("dialogue_guandu2_xunyu_advice", "主公，当以逸待劳，消耗其锐气。",
                "My lord, we should wait at ease for the exhausted enemy, wearing down their edge.",
                "주공, 편히 쉬며 피로한 적을 기다려 그들의 예기를 소모시켜야 합니다.");
            AddTranslation("dialogue_guandu2_guojia_predict", "袁绍必犯错，我们只需等。",
                "Yuan Shao will make mistakes - we need only wait.",
                "원소는 반드시 실수할 것입니다, 우리는 기다리기만 하면 됩니다.");
            AddTranslation("dialogue_guandu2_round4_stalemate", "双方僵持，袁军锐气渐失。",
                "Both sides deadlock, Yuan's army gradually loses morale.",
                "양측이 교착하고 원군의 예기가 점차 꺾입니다.");
            AddTranslation("dialogue_guandu2_caocao_gamble", "好！就赌这一局！",
                "Good! Let's bet on this game!",
                "좋아! 이 판에 걸겠다!");
            AddTranslation("dialogue_guandu2_yuanshao_angry", "颜良！可恶！",
                "Yan Liang! Curse it!",
                "안량! 빌어먹을!");
            AddTranslation("dialogue_guandu2_victory_caocao", "袁绍虽众，不足为惧。传令下去，继续坚守！",
                "Though Yuan Shao has numbers, he's not to be feared. Pass the order - continue defending!",
                "원소가 병력이 많으나 두려워할 것 없다. 명령을 전달하라, 계속 수비!");
            AddTranslation("dialogue_guandu2_victory_xunyu", "主公英明，袁军已现疲态。",
                "My lord is wise - Yuan's army shows fatigue.",
                "주공이 영명하십니다, 원군이 피로한 기색을 보입니다.");

            AddTranslation("dialogue_guandu3_opening_xuyou", "我为袁绍出谋划策，他却听信审配，扣押我家人！此处不留我，自有留我处！",
                "I advised Yuan Shao, yet he trusted Shen Pei and detained my family! If I'm not wanted here, somewhere else will have me!",
                "내가 원소를 위해 계책을 냈건만 심배를 믿고 내 가족을 구금하다니! 여기서 안 받아주면 다른 곳이 받아주리라!");
            AddTranslation("dialogue_guandu3_opening_caocao", "子远来此，天助我也！",
                "Ziyuan comes to me - heaven helps me!",
                "자원이 오다니, 하늘이 나를 돕는구나!");
            AddTranslation("dialogue_guandu3_xuyou_wuchao", "乌巢！袁绍的粮草全在乌巢！守将淳于琼嗜酒如命，正是可乘之机！",
                "Wuchao! All Yuan Shao's supplies are at Wuchao! The guard Chunyu Qiong loves wine - this is our chance!",
                "오소! 원소의 군량이 전부 오소에 있습니다! 수장 순우경은 술을 목숨처럼 좋아하니 바로 기회입니다!");
            AddTranslation("dialogue_guandu3_shenpei_suspect", "许攸叛逃，必有阴谋，须严加防范！",
                "Xu You's defection must be a plot - we must be vigilant!",
                "허유가 탈주했으니 필시 음모가 있다, 엄히 방비해야 합니다!");
            AddTranslation("dialogue_guandu3_yuanshao_dismiss", "许攸小人，不足为虑！",
                "Xu You is a petty man - not worth worrying about!",
                "허유는 소인배, 걱정할 것 없다!");
            AddTranslation("dialogue_guandu3_victory_caocao", "乌巢……好！传令精兵，今夜随我亲征！",
                "Wuchao... Good! Order elite troops - tonight I lead the raid myself!",
                "오소... 좋아! 정예병에게 명령하라, 오늘 밤 내가 친히 출정한다!");
            AddTranslation("dialogue_guandu3_victory_xuyou", "主公英明，此战必胜！",
                "My lord is wise - this battle will surely be won!",
                "주공이 영명하시니 이 싸움은 반드시 이깁니다!");

            AddTranslation("dialogue_guandu4_opening_caocao", "今夜，不成功，便成仁。",
                "Tonight - victory or death.",
                "오늘 밤, 성공 아니면 죽음이다.");
            AddTranslation("dialogue_guandu4_caohong_loyal", "愿随主公赴死！",
                "I follow my lord to death!",
                "주공과 함께 죽겠습니다!");
            AddTranslation("dialogue_guandu4_round2_fire", "火光骤起，乌巢大乱！",
                "Fire erupts suddenly - Wuchao in chaos!",
                "불길이 갑자기 솟아오르고 오소가 대혼란에 빠집니다!");
            AddTranslation("dialogue_guandu4_chunyuqiong_drunk", "什么？敌袭？再……再让我喝一杯……",
                "What? Enemy attack? Let me... let me have one more drink...",
                "뭐라고? 적습? 다... 다시 한 잔만...");
            AddTranslation("dialogue_guandu4_zhanghe_defect", "袁绍不听良策，已无胜机。不如另投明主！",
                "Yuan Shao ignores good advice - no chance of victory. Better to find a wiser lord!",
                "원소가 좋은 계책을 듣지 않으니 승기가 없다. 차라리 명주를 찾겠다!");
            AddTranslation("dialogue_guandu4_chunyuqiong_defeat", "粮草……完了……",
                "The supplies... finished...",
                "군량이... 끝났다...");
            AddTranslation("dialogue_guandu4_victory_narration", "乌巢火起，粮草尽毁。袁军闻讯，军心崩溃。",
                "Fire at Wuchao - supplies destroyed. Yuan's army hears the news - morale collapses.",
                "오소에 불이 나고 군량이 전소됩니다. 원군이 소식을 듣고 군심이 붕괴합니다.");
            AddTranslation("dialogue_guandu4_yuanshao_despair", "天不助我……",
                "Heaven does not help me...",
                "하늘이 나를 돕지 않는구나...");
            AddTranslation("dialogue_guandu4_victory_caocao", "官渡之战，尘埃落定。北方，将归于曹氏！",
                "The Battle of Guandu is decided. The North will belong to the Cao clan!",
                "관도대전이 결판났다. 북방은 조씨의 것이 되리라!");

            // 角色名称
            AddTranslation("char_yuanshao", "袁绍", "Yuan Shao", "원소");
            AddTranslation("char_yuanshu", "袁术", "Yuan Shu", "원술");
            AddTranslation("char_tianfeng", "田丰", "Tian Feng", "전풍");
            AddTranslation("char_jushou", "沮授", "Ju Shou", "저수");
            AddTranslation("char_shenpei", "审配", "Shen Pei", "심배");
            AddTranslation("char_xuyou", "许攸", "Xu You", "허유");
            AddTranslation("char_yanliang", "颜良", "Yan Liang", "안량");
            AddTranslation("char_wenchou", "文丑", "Wen Chou", "문추");
            AddTranslation("char_chunyuqiong", "淳于琼", "Chunyu Qiong", "순우경");
            AddTranslation("char_xunyu", "荀彧", "Xun Yu", "순욱");
            AddTranslation("char_guojia", "郭嘉", "Guo Jia", "곽가");
            AddTranslation("char_caohong", "曹洪", "Cao Hong", "조홍");
            AddTranslation("char_zhanghe", "张郃", "Zhang He", "장합");
            AddTranslation("char_sunjian", "孙坚", "Sun Jian", "손견");
            AddTranslation("char_huaxiong", "华雄", "Hua Xiong", "화웅");
            AddTranslation("char_huzhen", "胡轸", "Hu Zhen", "호진");
            AddTranslation("char_xurong", "徐荣", "Xu Rong", "서영");
            AddTranslation("char_morale_problem", "军心动摇", "Morale Problem", "군심동요");
            AddTranslation("char_supply_problem", "粮草隐患", "Supply Problem", "군량우환");
            AddTranslation("char_lords_hesitation", "诸侯观望", "Lords' Hesitation", "제후 관망");
            AddTranslation("char_xiliang_cavalry", "西凉骑兵", "Xiliang Cavalry", "서량 기병");
            AddTranslation("char_xiliang_soldier", "西凉兵", "Xiliang Soldier", "서량병");
            AddTranslation("char_yuanjun_guard", "袁军守将", "Yuan Army Guard", "원군 수장");
            AddTranslation("char_alliance_strife", "联军内斗", "Alliance Strife", "연합 내분");

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