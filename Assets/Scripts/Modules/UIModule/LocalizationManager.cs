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
            AddTranslation("menu_title", "三国演义", "Romance of Three Kingdoms", "삼국연의");
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

            // --- 魏国 (Wei) ---
            AddTranslation("general_caocao", "曹操", "Cao Cao", "조조");
            AddTranslation("general_xiahoudun", "夏侯惇", "Xiahou Dun", "하후돈");
            AddTranslation("general_xiahouyuan", "夏侯渊", "Xiahou Yuan", "하후연");
            AddTranslation("general_zhangliao", "张辽", "Zhang Liao", "장료");
            AddTranslation("general_xuhuang", "徐晃", "Xu Huang", "서황");
            AddTranslation("general_xuzhu", "许褚", "Xu Zhu", "허저");
            AddTranslation("general_simayi", "司马懿", "Sima Yi", "사마의");
            AddTranslation("general_jianggan", "蒋干", "Jiang Gan", "장간");
            AddTranslation("general_xiahoujie", "夏侯杰", "Xiahou Jie", "하후걸");
            AddTranslation("general_caojun_cavalry", "曹军骑兵", "Cao Cavalry", "조군 기병");

            // --- 蜀国 (Shu) ---
            AddTranslation("general_liubei", "刘备", "Liu Bei", "유비");
            AddTranslation("general_guanyu", "关羽", "Guan Yu", "관우");
            AddTranslation("general_zhangfei", "张飞", "Zhang Fei", "장비");
            AddTranslation("general_zhugeliang", "诸葛亮", "Zhuge Liang", "제갈량");
            AddTranslation("general_zhaoyun", "赵云", "Zhao Yun", "조운");
            AddTranslation("general_huangzhong", "黄忠", "Huang Zhong", "황충");
            AddTranslation("general_machao", "马超", "Ma Chao", "마초");

            // --- 吴国 (Wu) ---
            AddTranslation("general_sunquan", "孙权", "Sun Quan", "손권");
            AddTranslation("general_zhouyu", "周瑜", "Zhou Yu", "주유");
            AddTranslation("general_lvmeng", "吕蒙", "Lv Meng", "여몽");
            AddTranslation("general_huanggai", "黄盖", "Huang Gai", "황개");
            AddTranslation("general_sunjian", "孙坚", "Sun Jian", "손견");
            AddTranslation("general_luxun", "陆逊", "Lu Xun", "육손");
            AddTranslation("general_ganning", "甘宁", "Gan Ning", "감녕");
            AddTranslation("general_lusu", "鲁肃", "Lu Su", "노숙");
            AddTranslation("general_chengpu", "程普", "Cheng Pu", "정보");
            AddTranslation("general_zhangzhao", "张昭", "Zhang Zhao", "장소");

            // --- 群雄 (Qun) ---
            AddTranslation("general_lvbu", "吕布", "Lv Bu", "여포");
            AddTranslation("general_diaochan", "貂蝉", "Diao Chan", "초선");
            AddTranslation("general_huatuo", "华佗", "Hua Tuo", "화타");
            AddTranslation("general_dongzhuo", "董卓", "Dong Zhuo", "동탁");
            AddTranslation("general_yuanshao", "袁绍", "Yuan Shao", "원소");
            AddTranslation("general_huaxiong", "华雄", "Hua Xiong", "화웅");
            AddTranslation("general_yanliang", "颜良", "Yan Liang", "안량");
            AddTranslation("general_wenchou", "文丑", "Wen Chou", "문추");
            AddTranslation("general_zhanghe", "张郃", "Zhang He", "장합");
            AddTranslation("general_gaolan", "高览", "Gao Lan", "고람");
            AddTranslation("general_chunyuqiong", "淳于琼", "Chunyu Qiong", "순우경");
            AddTranslation("general_lijue", "李傕", "Li Jue", "이각");
            AddTranslation("general_guosi", "郭汜", "Guo Si", "곽사");

            // ⭐ 赤壁战役角色
            AddTranslation("general_caimao", "蔡瑁", "Cai Mao", "채모");
            AddTranslation("general_yufan", "虞翻", "Yu Fan", "우번");
            AddTranslation("general_caojun_sailor", "曹军水兵", "Cao Sailor", "조군 수병");

            // 阵营
            AddTranslation("faction_wei", "魏", "Wei", "위");
            AddTranslation("faction_shu", "蜀", "Shu", "촉");
            AddTranslation("faction_wu", "吴", "Wu", "오");
            AddTranslation("faction_qun", "群", "Neutral", "군");

            // ⭐ 身份场身份
            AddTranslation("identity_lord", "主公", "Lord", "주공");
            AddTranslation("identity_loyalist", "忠臣", "Loyalist", "충신");
            AddTranslation("identity_rebel", "反贼", "Rebel", "반적");
            AddTranslation("identity_spy", "内奸", "Spy", "내간");
            AddTranslation("identity_mode", "身份场", "Identity Mode", "신분전");
            AddTranslation("identity_reveal", "身份揭晓", "Identity Reveal", "신분 공개");
            AddTranslation("identity_win_lord", "主公/忠臣获胜", "Lord/Loyalist Wins", "주공/충신 승리");
            AddTranslation("identity_win_rebel", "反贼获胜", "Rebels Win", "반적 승리");
            AddTranslation("identity_win_spy", "内奸获胜", "Spy Wins", "내간 승리");

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

            // ⭐ 玩家人数设置
            AddTranslation("ui_player_count", "玩家人数", "Player Count", "플레이어 수");
            AddTranslation("ui_players", "{0}人", "{0} Players", "{0}명");

            // ⭐ 战斗角色标签
            AddTranslation("ui_ally", "我方", "Ally", "아군");
            AddTranslation("ui_enemy", "敌方", "Enemy", "적군");
            AddTranslation("ui_ally_index", "友方{0}", "Ally {0}", "아군 {0}");
            AddTranslation("ui_enemy_index", "敌方{0}", "Enemy {0}", "적군 {0}");
            AddTranslation("ui_click_continue", "▼ 点击继续", "▼ Click to continue", "▼ 클릭하여 계속");

            // 游戏状态
            AddTranslation("ui_game_over", "游戏结束", "Game Over", "게임 종료");
            AddTranslation("ui_winner", "获胜者", "Winner", "승자");
            AddTranslation("ui_waiting", "等待中...", "Waiting...", "대기 중...");
            AddTranslation("ui_your_turn", "你的回合", "Your Turn", "당신의 턴");
            AddTranslation("ui_ai_thinking", "AI思考中...", "AI Thinking...", "AI 생각 중...");

            // ⭐ 胜利/失败结果
            AddTranslation("msg_victory", "胜利!", "Victory!", "승리!");
            AddTranslation("msg_defeat", "失败...", "Defeat...", "패배...");
            AddTranslation("msg_returning", "即将返回...", "Returning...", "돌아가는 중...");

            // ⭐ 身份模式
            AddTranslation("identity_lord", "主公", "Lord", "주공");
            AddTranslation("identity_loyalist", "忠臣", "Loyalist", "충신");
            AddTranslation("identity_rebel", "反贼", "Rebel", "반적");
            AddTranslation("identity_spy", "内奸", "Spy", "내간");
            AddTranslation("identity_none", "无", "None", "없음");
            AddTranslation("identity_reveal", "身份揭晓:", "Identity Reveal:", "신분 공개:");
            AddTranslation("identity_dead", "(阵亡)", "(Dead)", "(사망)");
            AddTranslation("identity_win_lord_loyalist", "主公/忠臣获胜", "Lord/Loyalist Victory", "주공/충신 승리");
            AddTranslation("identity_win_rebel", "反贼获胜", "Rebel Victory", "반적 승리");
            AddTranslation("identity_win_spy", "内奸获胜", "Spy Victory", "내간 승리");

            // ⭐ 胜利/失败条件标签
            AddTranslation("ui_victory_label", "胜利:", "Victory:", "승리:");
            AddTranslation("ui_defeat_label", "失败:", "Defeat:", "패배:");

            // ⭐ 胜利条件类型
            AddTranslation("victory_defeat_all", "击败所有敌人", "Defeat all enemies", "모든 적 처치");
            AddTranslation("victory_defeat_target", "击败 {0}", "Defeat {0}", "{0} 처치");
            AddTranslation("victory_survive_turns", "存活 {0} 回合", "Survive {0} turns", "{0}턴 생존");
            AddTranslation("victory_accumulate_marks", "累积 {0} 个标记", "Accumulate {0} marks", "표식 {0}개 획득");
            AddTranslation("victory_protect_ally", "保护 {0} 存活并击败所有敌人", "Protect {0} and defeat all enemies", "{0}을(를) 보호하고 모든 적 처치");

            // ⭐ 失败条件类型
            AddTranslation("defeat_player_death", "主角死亡", "Player dies", "주인공 사망");
            AddTranslation("defeat_ally_death", "{0} 死亡", "{0} dies", "{0} 사망");
            AddTranslation("defeat_all_allies_death", "我方全灭", "All allies die", "아군 전멸");
            AddTranslation("defeat_exceed_count", "特定事件发生 {0} 次", "Event occurs {0} times", "특정 이벤트 {0}회 발생");
            AddTranslation("defeat_turn_limit", "超过 {0} 回合", "Exceed {0} turns", "{0}턴 초과");

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

            // ⭐ 装备消息
            AddTranslation("msg_equipped", "{0} 装备了【{1}】", "{0} equipped [{1}]", "{0}이(가) [{1}]을(를) 장착함");
            AddTranslation("msg_equipment_replaced", "{0} 将【{1}】替换为【{2}】", "{0} replaced [{1}] with [{2}]", "{0}이(가) [{1}]을(를) [{2}]로 교체함");

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
            AddTranslation("msg_longdan_dodge_as_slash", "{0} 发动【龙胆】，将【闪】当【杀】对 {1} 使用", "{0} used [Longdan], using [Dodge] as [Slash] against {1}", "{0}이(가) [용담]을 발동, [섬]을 [살]로 {1}에게 사용");
            AddTranslation("msg_longdan_hint", "【龙胆】可选择目标当杀使用", "[Longdan] Can select target to use as Slash", "[용담] 대상 선택 시 [살]로 사용 가능");
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

            // ==================== 官渡之战/讨董之战 新技能 ====================

            // 官渡之战技能
            AddTranslation("skill_weiwu", "威武", "Mighty", "위무");
            AddTranslation("skill_weiwu_desc", "摸牌阶段少摸1张牌，你可以将黑色牌当【决斗】使用。", "Draw 1 less card in draw phase. You may use black cards as [Duel].", "드로우 단계에 1장 적게 뽑습니다. 검은 카드를 [결투]로 사용할 수 있습니다.");

            AddTranslation("skill_qiangjian", "强健", "Robust", "강건");
            AddTranslation("skill_qiangjian_desc", "摸牌阶段少摸1张牌，你可以将黑色牌当【决斗】使用。", "Draw 1 less card in draw phase. You may use black cards as [Duel].", "드로우 단계에 1장 적게 뽑습니다. 검은 카드를 [결투]로 사용할 수 있습니다.");

            AddTranslation("skill_duobian", "多变", "Versatile", "다변");
            AddTranslation("skill_duobian_desc", "锁定技，出牌阶段你可以额外使用1张【杀】。", "Compulsory. You may use 1 extra [Slash] during play phase.", "강제 효과. 출패 단계에 [살] 1장 추가 사용 가능.");

            AddTranslation("skill_mashu", "马术", "Horsemanship", "마술");
            AddTranslation("skill_mashu_desc", "锁定技，计算与其他角色的距离时-1。", "Compulsory. When calculating distance to other characters, -1.", "강제 효과. 다른 캐릭터와의 거리 계산 시 -1.");

            AddTranslation("skill_duanliang", "断粮", "Cut Supply", "단량");
            AddTranslation("skill_duanliang_desc", "你可以将黑色基本牌或黑色装备牌当【兵粮寸断】使用。", "You may use black basic or equipment cards as [Supply Shortage].", "검은색 기본패나 장비패를 [병량촌단]으로 사용할 수 있습니다.");

            AddTranslation("skill_yingming", "英明", "Wise", "영명");
            AddTranslation("skill_yingming_desc", "锁定技，摸牌阶段你多摸1张牌。", "Compulsory. Draw 1 extra card during draw phase.", "강제 효과. 드로우 단계에 1장 추가 뽑기.");

            AddTranslation("skill_qiji", "齐击", "Volley", "제격");
            AddTranslation("skill_qiji_desc", "你可以将两张不同花色的牌当【万箭齐发】使用。", "You may use two cards of different suits as [Arrow Barrage].", "다른 무늬의 카드 2장을 [만전제발]로 사용할 수 있습니다.");

            // 讨董之战技能
            AddTranslation("skill_yaowuv2", "耀武", "Display Might", "요무");
            AddTranslation("skill_yaowuv2_desc", "当你使用红色【杀】造成伤害后，可以弃置1张牌获得目标1张牌。", "After dealing damage with red [Slash], you may discard 1 card to take 1 card from target.", "빨간 [살]로 피해를 준 후 1장 버리고 대상의 카드 1장 획득 가능.");

            AddTranslation("skill_wushuang", "无双", "Peerless", "무쌍");
            AddTranslation("skill_wushuang_desc", "锁定技，【杀】需两张【闪】响应；【决斗】对方需打出两张【杀】。", "Compulsory. [Slash] requires two [Dodge] to counter; [Duel] requires opponent to play two [Slash].", "강제 효과. [살]은 [섬] 2장 필요; [결투]시 상대는 [살] 2장 필요.");

            AddTranslation("skill_yinghun", "英魂", "Heroic Soul", "영혼");
            AddTranslation("skill_yinghun_desc", "回合开始时，若你已受伤，可令一名角色摸X张牌然后弃X张牌（X为你已损失的体力值）。", "At start of turn, if you are wounded, you may have a character draw X cards then discard X cards (X = HP lost).", "턴 시작 시, 부상 상태면 한 캐릭터가 X장 뽑고 X장 버리게 할 수 있습니다 (X = 잃은 체력).");

            AddTranslation("skill_xueyi", "血裔", "Bloodline", "혈예");
            AddTranslation("skill_xueyi_desc", "锁定技，摸牌阶段你额外摸X张牌（X为已受伤角色数）。", "Compulsory. Draw X extra cards during draw phase (X = number of wounded characters).", "강제 효과. 드로우 단계에 X장 추가 뽑기 (X = 부상 캐릭터 수).");

            AddTranslation("skill_jiuchiroulin", "酒池肉林", "Wine Pool", "주지육림");
            AddTranslation("skill_jiuchiroulin_desc", "你可以将1张【杀】当【酒】使用；你可以将1张【酒】当【杀】使用。", "You may use [Slash] as [Wine]; you may use [Wine] as [Slash].", "[살]을 [술]로, [술]을 [살]로 사용할 수 있습니다.");

            AddTranslation("skill_jielve", "劫掠", "Plunder", "겁략");
            AddTranslation("skill_jielve_desc", "当你造成伤害后，可以弃置1张牌获得目标1张牌。", "After dealing damage, you may discard 1 card to take 1 card from target.", "피해를 준 후 1장 버리고 대상의 카드 1장 획득 가능.");

            AddTranslation("skill_xiongbao", "凶暴", "Ferocious", "흉포");
            AddTranslation("skill_xiongbao_desc", "每当你造成1点伤害后，可以摸1张牌。", "After dealing 1 damage, you may draw 1 card.", "1점 피해를 줄 때마다 1장 뽑을 수 있습니다.");

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

            // 延时锦囊
            AddTranslation("msg_already_has_indulgence", "目标判定区已有【乐不思蜀】", "Target already has [Indulgence] in judgment area", "대상의 판정 구역에 이미 [낙불사촉]이 있습니다");
            AddTranslation("msg_already_has_lightning", "判定区已有【闪电】", "Already has [Lightning] in judgment area", "판정 구역에 이미 [번개]가 있습니다");
            AddTranslation("msg_already_has_supply_shortage", "目标判定区已有【兵粮寸断】", "Target already has [Supply Shortage] in judgment area", "대상의 판정 구역에 이미 [병량촌단]이 있습니다");
            AddTranslation("msg_supply_shortage_distance", "【兵粮寸断】只能对距离1的角色使用", "[Supply Shortage] can only be used on targets at distance 1", "[병량촌단]은 거리 1인 대상에게만 사용 가능");
            AddTranslation("msg_used_delayed_trick", "{0} 对 {2} 使用了【{1}】", "{0} used [{1}] on {2}", "{0}이(가) {2}에게 [{1}]을 사용했습니다");
            AddTranslation("msg_used_lightning", "{0} 放置了【闪电】", "{0} placed [Lightning]", "{0}이(가) [번개]를 배치했습니다");

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

            // ⭐ 赤壁之战 Custom 规则相关消息
            AddTranslation("msg_no_peach_rule", "【单骑断桥】不能使用桃！", "[Single Rider Bridge] Cannot use Peach!", "[단기단교] 도를 사용할 수 없습니다!");
            AddTranslation("msg_huwei_trigger", "【虎威】{0} 弃置1张手牌！", "[Tiger's Might] {0} discards 1 card!", "[호위] {0}이(가) 1장을 버립니다!");
            AddTranslation("msg_huwei_no_cards", "【虎威】{0} 没有手牌可弃", "[Tiger's Might] {0} has no cards to discard", "[호위] {0}은(는) 버릴 카드가 없음");
            AddTranslation("msg_persuade_offer", "【以理服人】是否令目标弃2牌代替伤害？", "[Persuasion] Make target discard 2 cards instead of damage?", "[이리복인] 피해 대신 대상이 2장 버리게 하시겠습니까?");
            AddTranslation("msg_persuade_effect", "【以理服人】{0} 弃置2张牌代替受到伤害", "[Persuasion] {0} discards 2 cards instead of taking damage", "[이리복인] {0}이(가) 피해 대신 2장을 버립니다");
            AddTranslation("msg_forge_letter", "【伪造书信】获得反间标记！({0}/3)", "[Forged Letter] Gained counter-intelligence mark! ({0}/3)", "[위조서신] 반간 표시 획득! ({0}/3)");
            AddTranslation("msg_trick_fake_intel", "【中计】蒋干发现假情报！", "[Tricked] Jiang Gan found fake intelligence!", "[중계] 장간이 가짜 정보를 발견했습니다!");
            AddTranslation("msg_suspicion_effect", "【猜忌】曹操对蔡瑁产生怀疑，蔡瑁体力-{0}", "[Suspicion] Cao Cao suspects Cai Mao. Cai Mao HP -{0}", "[의심] 조조가 채모를 의심합니다. 채모 체력 -{0}");
            AddTranslation("msg_seasick_effect", "【水土不服】{0} 不适应水战，损失1点体力", "[Seasickness] {0} is not used to naval combat, loses 1 HP", "[수토불복] {0}이(가) 수전에 적응 못해 체력 1 손실");
            AddTranslation("msg_guanyu_priority", "【关羽】优先攻击血量最低的敌人", "[Guan Yu] Prioritizes attacking the lowest HP enemy", "[관우] 체력이 가장 낮은 적을 우선 공격합니다");
            AddTranslation("msg_debate_mode", "【舌战模式】杀和闪无效，只能使用锦囊牌", "[Debate Mode] Slash and Dodge ineffective, only Trick cards work", "[설전모드] 살과 섬 무효, 금낭패만 사용 가능");

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

            // ==================== 官渡之战/讨董之战 角色 ====================

            // 官渡之战角色
            AddTranslation("char_yanliang", "颜良", "Yan Liang", "안량");
            AddTranslation("char_wenchou", "文丑", "Wen Chou", "문추");
            AddTranslation("char_zhanghe", "张郃", "Zhang He", "장합");
            AddTranslation("char_gaolan", "高览", "Gao Lan", "고람");
            AddTranslation("char_chunyuqiong", "淳于琼", "Chunyu Qiong", "순우경");
            AddTranslation("char_yuanshao", "袁绍", "Yuan Shao", "원소");
            AddTranslation("char_xuhuang", "徐晃", "Xu Huang", "서황");
            AddTranslation("char_yuanjun_cavalry", "袁军骑将", "Yuan Cavalry", "원군 기장");
            AddTranslation("char_caojun_infantry", "曹军步兵", "Cao Infantry", "조조군 보병");

            // 讨董之战角色
            AddTranslation("char_huaxiong", "华雄", "Hua Xiong", "화웅");
            AddTranslation("char_lvbu", "吕布", "Lv Bu", "여포");
            AddTranslation("char_dongzhuo", "董卓", "Dong Zhuo", "동탁");
            AddTranslation("char_lijue", "李傕", "Li Jue", "이각");
            AddTranslation("char_guosi", "郭汜", "Guo Si", "곽사");
            AddTranslation("char_sunjian", "孙坚", "Sun Jian", "손견");
            AddTranslation("char_xiliang_soldier", "西凉兵", "Xiliang Soldier", "서량병");

            // 其他NPC
            AddTranslation("char_chengyu", "程昱", "Cheng Yu", "정욱");
            AddTranslation("char_guojia", "郭嘉", "Guo Jia", "곽가");
            AddTranslation("char_xunyu", "荀彧", "Xun Yu", "순욱");
            AddTranslation("char_xuyou", "许攸", "Xu You", "허유");
            AddTranslation("char_yuanshu", "袁术", "Yuan Shu", "원술");

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

            // ==================== 讨董之战 v2 (Campaign Against Dong Zhuo) ====================
            AddTranslation("campaign_taodong", "讨董之战", "Campaign Against Dong Zhuo", "동탁 토벌전");
            AddTranslation("campaign_taodong_desc", "董卓废帝弄权，诸侯会盟讨伐。温酒斩华雄，三英战吕布，传颂千古的英雄故事在此上演！",
                "Dong Zhuo deposes the emperor and seizes power. Lords unite to campaign against him. The legendary tales of slaying Hua Xiong and the three heroes battling Lu Bu unfold here!",
                "동탁이 황제를 폐하고 권력을 잡습니다. 제후들이 연합하여 토벌에 나섭니다. 화웅 참살과 삼영전 여포의 전설이 펼쳐집니다!");

            // 讨董之战 v2 - 第一战：檄文传天下
            AddTranslation("battle_taodong_1", "檄文传天下", "Proclamation to All", "격문전천하");
            AddTranslation("battle_taodong_1_subtitle", "义兵举义旗", "Righteous Army Rising", "의병거의기");
            AddTranslation("battle_taodong_1_desc", "曹操发檄文号召天下讨董，夏侯惇夏侯渊随军出征，与西凉兵交战！",
                "Cao Cao issues a proclamation calling all to campaign against Dong Zhuo. Xiahou Dun and Xiahou Yuan march with him against Xiliang troops!",
                "조조가 격문을 발표하여 천하에 동탁 토벌을 호소합니다. 하후돈과 하후연이 출정하여 서량병과 싸웁니다!");
            AddTranslation("battle_taodong_1_briefing", "击败全部西凉兵即可获胜。义愤填膺：曹操首次伤害+1。救民：击败敌人后全体回复1体力。",
                "Defeat all Xiliang soldiers to win. Righteous Fury: Cao Cao's first damage +1. Save People: Heal 1 HP for all when defeating an enemy.",
                "서량병 전원 격파 시 승리. 의분전응: 조조 첫 피해 +1. 구민: 적 격파 시 전체 체력 1 회복.");

            // 讨董之战 v2 - 第二战：阵前斩雄
            AddTranslation("battle_taodong_2", "阵前斩雄", "Slay the Champion", "진전참웅");
            AddTranslation("battle_taodong_2_subtitle", "酒尚温时", "Wine Still Warm", "주상온시");
            AddTranslation("battle_taodong_2_desc", "华雄连斩数将，诸侯皆惧。关羽请缨出战，曹操以酒相送。温酒斩华雄，威震天下！",
                "Hua Xiong has slain several generals, all lords fear him. Guan Yu volunteers to fight, Cao Cao gives him wine. He slays Hua Xiong while the wine is still warm!",
                "화웅이 여러 장수를 베자 제후들이 두려워합니다. 관우가 출전을 청하고 조조가 술을 건넵니다. 술이 식기 전에 화웅을 베었습니다!");
            AddTranslation("battle_taodong_2_briefing", "击败华雄即可获胜。单挑：1对1决斗。酒尚温：关羽首次杀不可闪避。",
                "Defeat Hua Xiong to win. Duel: 1v1 combat. Wine Still Warm: Guan Yu's first Slash cannot be dodged.",
                "화웅 격파 시 승리. 단도: 1대1 결투. 주상온: 관우 첫 살 회피 불가.");

            // 讨董之战 v2 - 第三战：虎牢关血战
            AddTranslation("battle_taodong_3", "虎牢关血战", "Battle at Hulao Pass", "호뢰관 혈전");
            AddTranslation("battle_taodong_3_subtitle", "绝世武力", "Peerless Might", "절세무력");
            AddTranslation("battle_taodong_3_desc", "虎牢关前，吕布一人独战诸侯。张飞先战，关羽刘备相继加入，三英战吕布！",
                "Before Hulao Pass, Lu Bu fights the lords alone. Zhang Fei engages first, Guan Yu and Liu Bei join in succession - three heroes battle Lu Bu!",
                "호뢰관 앞에서 여포가 홀로 제후들과 싸웁니다. 장비가 먼저 싸우고, 관우와 유비가 차례로 합류합니다. 삼영전 여포!");
            AddTranslation("battle_taodong_3_briefing", "击败吕布即可获胜。一骑当千：吕布+3体力（第3回合后消失）。绝世武力：第1回合吕布杀伤害x2。逐步增援：关羽第2回合加入，刘备第3回合加入。",
                "Defeat Lu Bu to win. One Man Army: Lu Bu +3 HP (removed after turn 3). Peerless Might: Lu Bu's Slash x2 damage in turn 1. Reinforcement: Guan Yu joins turn 2, Liu Bei joins turn 3.",
                "여포 격파 시 승리. 일기당천: 여포 +3체력 (3턴 후 소멸). 절세무력: 1턴 여포 살 피해 x2. 증원: 관우 2턴 합류, 유비 3턴 합류.");

            // 讨董之战 v2 - 第四战：洛阳之战
            AddTranslation("battle_taodong_4", "洛阳之战", "Battle of Luoyang", "낙양 전투");
            AddTranslation("battle_taodong_4_subtitle", "挟天子西遁", "Emperor's Flight West", "협천자서둔");
            AddTranslation("battle_taodong_4_desc", "联军进逼洛阳，董卓命李傕郭汜断后。击败董卓时，他将挟天子西逃！",
                "The alliance advances on Luoyang, Dong Zhuo orders Li Jue and Guo Si to cover the retreat. When Dong Zhuo is defeated, he flees west with the emperor!",
                "연합군이 낙양으로 진격하고 동탁이 이각과 곽사에게 후미를 맡깁니다. 동탁을 물리치면 그가 천자를 끼고 서쪽으로 도주합니다!");
            AddTranslation("battle_taodong_4_briefing", "董卓HP=0即可获胜。西凉铁骑：敌方攻击距离+1。挟天子西遁：董卓HP=0时逃走而非死亡。",
                "Reduce Dong Zhuo to 0 HP to win. Xiliang Cavalry: Enemy attack range +1. Emperor's Flight: Dong Zhuo escapes instead of dying at 0 HP.",
                "동탁 HP=0 시 승리. 서량철기: 적 공격 거리 +1. 협천자서둔: 동탁 HP=0 시 사망 대신 도주.");

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

            // ==================== 官渡之战 v2 (Battle of Guandu) ====================
            AddTranslation("campaign_guandu_desc", "建安五年，袁绍率大军南下攻曹。关羽斩颜良诛文丑，曹操夜袭乌巢烧粮，以少胜多，奠定北方霸业。",
                "In 200 AD, Yuan Shao marches south with a massive army. Guan Yu slays Yan Liang and Wen Chou, Cao Cao raids Wuchao at night. Against all odds, victory is achieved.",
                "건안 5년, 원소가 대군을 이끌고 남하합니다. 관우가 안량과 문추를 베고, 조조가 오소를 야습합니다. 과소적다의 승리입니다.");

            // 官渡之战 v2 - 第一战：兵临官渡（关羽斩颜良）
            AddTranslation("battle_guandu_1", "兵临官渡", "Armies at Guandu", "병림관도");
            AddTranslation("battle_guandu_1_subtitle", "十胜十败", "Ten Victories, Ten Defeats", "십승십패");
            AddTranslation("battle_guandu_1_desc", "袁绍大军压境，颜良为先锋。关羽临时助阵，单骑冲阵斩颜良！",
                "Yuan Shao's massive army approaches, with Yan Liang as vanguard. Guan Yu temporarily assists, charging alone to slay Yan Liang!",
                "원소의 대군이 다가오고 안량이 선봉입니다. 관우가 임시로 참전하여 단기로 돌진해 안량을 베었습니다!");
            AddTranslation("battle_guandu_1_briefing", "击杀颜良即可获胜。胜利后全军士气大振，后续战斗体力上限+1。",
                "Defeat Yan Liang to win. Victory boosts morale - +1 max HP in subsequent battles.",
                "안량을 격파하면 승리. 승리 시 사기가 오르며 이후 전투에서 체력 상한 +1.");

            // 官渡之战 v2 - 第二战：坚守官渡（关羽斩文丑）
            AddTranslation("battle_guandu_2", "坚守官渡", "Defend Guandu", "관도 수비");
            AddTranslation("battle_guandu_2_subtitle", "死守不退", "Hold The Line", "사수불퇴");
            AddTranslation("battle_guandu_2_desc", "颜良既死，文丑为报仇而来。关羽再战文丑，张辽夏侯惇协力！",
                "With Yan Liang dead, Wen Chou comes for revenge. Guan Yu faces Wen Chou again, with Zhang Liao and Xiahou Dun's support!",
                "안량이 죽자 문추가 복수하러 옵니다. 관우가 다시 문추와 싸우고, 장료와 하후돈이 협력합니다!");
            AddTranslation("battle_guandu_2_briefing", "击杀文丑即可获胜。我方受伤害30%概率+1，文丑对关羽伤害+1。",
                "Defeat Wen Chou to win. Allies take +1 damage at 30% chance. Wen Chou deals +1 damage to Guan Yu.",
                "문추를 격파하면 승리. 아군 피해 30% 확률로 +1. 문추가 관우에게 피해 +1.");

            // 官渡之战 v2 - 第三战：夜袭乌巢
            AddTranslation("battle_guandu_3", "夜袭乌巢", "Night Raid on Wuchao", "오소 야습");
            AddTranslation("battle_guandu_3_subtitle", "孤注一掷", "All or Nothing", "고주일척");
            AddTranslation("battle_guandu_3_desc", "许攸来投，献计烧乌巢。曹操率徐晃张辽夜袭粮仓，一战定乾坤！",
                "Xu You defects and suggests burning Wuchao. Cao Cao leads Xu Huang and Zhang Liao in a night raid on the supply depot - one battle to decide all!",
                "허유가 투항하여 오소를 불태울 계책을 바칩니다. 조조가 서황, 장료를 이끌고 야습합니다!");
            AddTranslation("battle_guandu_3_briefing", "击杀淳于琼即可获胜。夜袭：敌方起始手牌-1。第2回合起敌方每回合失去1体力。曹操手牌<=2时杀伤害+1。",
                "Defeat Chunyu Qiong to win. Night Raid: enemies start with -1 cards. From turn 2, enemies lose 1 HP per turn. Cao Cao's Slash +1 when hand <=2.",
                "순우경을 격파하면 승리. 야습: 적 초기 패 -1. 2턴부터 적 매 턴 체력 -1. 조조 수패<=2일 때 살 피해 +1.");

            // 官渡之战 v2 - 第四战：袁军溃败
            AddTranslation("battle_guandu_4", "袁军溃败", "Yuan Army Collapses", "원군 궤멸");
            AddTranslation("battle_guandu_4_subtitle", "定鼎北方", "Northern Dominion", "정정북방");
            AddTranslation("battle_guandu_4_desc", "乌巢粮草被焚，袁军军心崩溃。曹操乘胜追击，击败袁绍！",
                "With Wuchao's supplies burned, Yuan's army collapses. Cao Cao presses the advantage to defeat Yuan Shao!",
                "오소의 군량이 불타자 원군의 군심이 붕괴됩니다. 조조가 승기를 몰아 원소를 격파합니다!");
            AddTranslation("battle_guandu_4_briefing", "击败袁绍即可获胜。军心崩溃：敌方手牌上限-1。饥疲交迫：敌方每回合50%失去1体力。乘胜追击：曹操杀伤害后摸1牌。",
                "Defeat Yuan Shao to win. Army Collapse: enemy hand limit -1. Hungry & Tired: enemies 50% lose 1 HP per turn. Victory Pursuit: Cao Cao draws 1 after Slash damage.",
                "원소를 격파하면 승리. 군심붕괴: 적 수패 상한 -1. 기피교박: 적 매 턴 50% 체력 -1. 승승장구: 조조 살 피해 후 1장 뽑기.");

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

            // ==================== 官渡之战 v2 对白 (Guandu v2 Dialogues) ====================

            // 兵临官渡 - 开场对白
            AddTranslation("dialogue_guandu1_opening_chengyu", "主公，探报来了！袁绍大军已渡黄河，颜良为先锋，势如破竹！",
                "My lord, the scouts report! Yuan Shao's army has crossed the Yellow River with Yan Liang as vanguard, unstoppable!",
                "주공, 정찰 보고입니다! 원소의 대군이 황하를 건넜고 안량이 선봉으로 파죽지세입니다!");
            AddTranslation("dialogue_guandu1_opening_caocao1", "颜良……河北名将，勇冠三军。何人可敌？",
                "Yan Liang... a famous general of Hebei, the bravest of three armies. Who can face him?",
                "안량... 하북의 명장으로 삼군 중 가장 용맹하다. 누가 막을 수 있겠는가?");
            AddTranslation("dialogue_guandu1_opening_guojia1", "主公，颜良虽勇，却有一人可敌。",
                "My lord, though Yan Liang is brave, there is one who can face him.",
                "주공, 안량이 용맹하지만 그를 상대할 수 있는 자가 있습니다.");
            AddTranslation("dialogue_guandu1_opening_caocao2", "哦？奉孝所言何人？",
                "Oh? Who does Fengxiao speak of?",
                "오? 봉효가 말하는 자가 누구인가?");
            AddTranslation("dialogue_guandu1_opening_guojia2", "关羽关云长。其武艺当世无双，义气更是天下皆知。",
                "Guan Yu, Guan Yunchang. His martial arts are unmatched, and his righteousness is known throughout the land.",
                "관우 관운장입니다. 그의 무예는 당세에 비할 바 없고, 의리는 천하가 아는 바입니다.");
            AddTranslation("dialogue_guandu1_opening_guojia3", "主公厚待关羽，正可借此机会，让他立功报恩。",
                "My lord has treated Guan Yu well. This is the chance to let him repay that kindness through merit.",
                "주공께서 관우를 후대하셨으니, 이 기회에 공을 세워 은혜를 갚게 하십시오.");
            AddTranslation("dialogue_guandu1_opening_guojia4", "若能斩颜良，袁军锐气必挫，此战可定大局。",
                "If he slays Yan Liang, Yuan's army morale will collapse - this battle can decide everything.",
                "안량을 베면 원군의 예기가 꺾이고 이 전투로 대세를 정할 수 있습니다.");
            AddTranslation("dialogue_guandu1_opening_caocao3", "好！来人，请关将军！",
                "Good! Someone, summon General Guan!",
                "좋다! 누군가 관 장군을 모셔오라!");
            AddTranslation("dialogue_guandu1_opening_soldier", "报！关将军到！",
                "Report! General Guan has arrived!",
                "보고합니다! 관 장군 도착!");
            AddTranslation("dialogue_guandu1_opening_guanyu", "丞相唤羽，有何差遣？",
                "The Chancellor summoned me - what task do you have?",
                "승상께서 부르셨으니 무슨 일입니까?");
            AddTranslation("dialogue_guandu1_opening_caocao4", "颜良猖獗，请云长出马！若能斩此贼，必当重赏！",
                "Yan Liang is rampant - please ride forth, Yunchang! If you slay this villain, great reward awaits!",
                "안량이 날뛰고 있으니 운장이 출전해 주시오! 이 적을 베면 후한 상을 내리겠소!");

            // 兵临官渡 - 战斗事件对白
            AddTranslation("dialogue_guandu1_yanliang_challenge", "何人敢来送死！颜良在此！",
                "Who dares to come die! Yan Liang is here!",
                "누가 감히 죽으러 오느냐! 안량이 여기 있다!");
            AddTranslation("dialogue_guandu1_guanyu_answer", "关某来也！",
                "Guan is here!",
                "관 모가 왔다!");
            AddTranslation("dialogue_guandu1_guanyu_slash", "看我青龙刀！",
                "Behold my Green Dragon Blade!",
                "내 청룡도를 보아라!");
            AddTranslation("dialogue_guandu1_yanliang_lowHP", "这……这红脸长髯之人是谁？好快的刀！",
                "This... who is this red-faced, long-bearded man? Such a fast blade!",
                "이... 이 붉은 얼굴에 긴 수염의 자가 누구냐? 이리도 빠른 칼이라니!");
            AddTranslation("dialogue_guandu1_guanyu_victory", "某斩颜良，以报丞相厚恩！",
                "I have slain Yan Liang to repay the Chancellor's kindness!",
                "안량을 베어 승상의 후은에 보답하였소!");

            // 兵临官渡 - 胜利对白
            AddTranslation("dialogue_guandu1_victory_guanyu", "颜良已斩，某当告辞。",
                "Yan Liang is slain - I shall take my leave.",
                "안량을 베었으니 이만 물러가겠소.");
            AddTranslation("dialogue_guandu1_victory_caocao", "云长真神将也！袁军必惧！",
                "Yunchang is truly a divine general! Yuan's army must be terrified!",
                "운장은 진정 신장이로다! 원군이 두려워할 것이다!");
            AddTranslation("dialogue_guandu1_victory_narration", "关羽阵斩颜良，袁军大惊。曹军士气大振，准备迎接下一场恶战。",
                "Guan Yu slew Yan Liang in battle, shocking Yuan's army. Cao's army morale soared, preparing for the next fierce battle.",
                "관우가 안량을 베자 원군이 크게 놀랐다. 조군의 사기가 크게 올랐고 다음 격전을 준비했다.");

            // 坚守官渡 - 开场对白
            AddTranslation("dialogue_guandu2_opening_narration", "颜良既死，袁绍命文丑率军报仇。曹操命关羽再度出战。",
                "With Yan Liang dead, Yuan Shao ordered Wen Chou to lead an army for revenge. Cao Cao commanded Guan Yu to fight again.",
                "안량이 죽자 원소는 문추에게 복수하라 명했다. 조조는 관우에게 다시 출전을 명했다.");
            AddTranslation("dialogue_guandu2_opening_caocao1", "云长，文丑来势汹汹，欲为颜良报仇！",
                "Yunchang, Wen Chou comes fiercely, seeking revenge for Yan Liang!",
                "운장, 문추가 거세게 오고 있소, 안량의 복수를 하려고!");
            AddTranslation("dialogue_guandu2_opening_chengyu", "文丑与颜良并称河北双雄，武艺不在颜良之下！",
                "Wen Chou is called one of Hebei's Twin Heroes alongside Yan Liang - his martial arts rival Yan Liang's!",
                "문추는 안량과 함께 하북쌍웅으로 불리며 무예가 안량에 뒤지지 않습니다!");
            AddTranslation("dialogue_guandu2_opening_caocao2", "可有良策？",
                "Do we have a good strategy?",
                "좋은 계책이 있는가?");
            AddTranslation("dialogue_guandu2_opening_xunyu1", "文丑性急，可以诱敌深入，然后伏击。",
                "Wen Chou is impatient - we can lure him deep then ambush.",
                "문추는 성급하니 유인해서 매복하면 됩니다.");
            AddTranslation("dialogue_guandu2_opening_xunyu2", "张辽、夏侯惇可为援军，待文丑追击时夹击之。",
                "Zhang Liao and Xiahou Dun can be reinforcements, flanking when Wen Chou pursues.",
                "장료와 하후돈을 지원군으로 두어 문추가 추격할 때 협공합니다.");
            AddTranslation("dialogue_guandu2_opening_xunyu3", "关羽为诱饵，且战且退，引文丑入伏。",
                "Guan Yu will be the bait, fighting while retreating, luring Wen Chou into the ambush.",
                "관우를 미끼로 삼아 싸우며 후퇴하여 문추를 함정으로 유인합니다.");
            AddTranslation("dialogue_guandu2_opening_soldier", "报！文丑军已至！",
                "Report! Wen Chou's army has arrived!",
                "보고합니다! 문추군 도착!");
            AddTranslation("dialogue_guandu2_opening_caocao3", "众将听令！依计行事！",
                "All generals, heed my orders! Execute the plan!",
                "제장들 명을 따르라! 계획대로 행하라!");
            AddTranslation("dialogue_guandu2_opening_guanyu", "文丑！今日便是你的死期！",
                "Wen Chou! Today is your death day!",
                "문추! 오늘이 네 제삿날이다!");
            AddTranslation("dialogue_guandu2_opening_wenchou", "关羽！我来取你项上人头，为颜良报仇！",
                "Guan Yu! I come for your head to avenge Yan Liang!",
                "관우! 네 목을 가져가 안량의 원수를 갚겠다!");
            AddTranslation("dialogue_guandu2_opening_guanyu2", "来便来！某岂惧你！",
                "Then come! Do you think I fear you!",
                "오려면 오라! 내가 너를 두려워할 것 같으냐!");

            // 坚守官渡 - 战斗事件对白
            AddTranslation("dialogue_guandu2_wenchou_revenge", "颜良兄弟，看我为你报仇！",
                "Brother Yan Liang, watch me avenge you!",
                "안량 형제여, 내가 복수하는 것을 보라!");
            AddTranslation("dialogue_guandu2_zhangliao_tuxi", "文丑，中计了！",
                "Wen Chou, you've fallen into our trap!",
                "문추, 계략에 걸렸다!");
            AddTranslation("dialogue_guandu2_xiahoudun_ganglie", "谁敢伤我主公！",
                "Who dares hurt my lord!",
                "누가 감히 우리 주공을 해치느냐!");
            AddTranslation("dialogue_guandu2_wenchou_lowHP", "可恶！中了埋伏！",
                "Curse it! An ambush!",
                "빌어먹을! 매복에 걸렸다!");
            AddTranslation("dialogue_guandu2_guanyu_victory", "文丑已斩，袁军可破矣！",
                "Wen Chou is slain - Yuan's army can be defeated!",
                "문추를 베었으니 원군을 격파할 수 있다!");

            // 坚守官渡 - 胜利对白
            AddTranslation("dialogue_guandu2_victory_guanyu", "二将既除，袁军无人矣。",
                "With both generals gone, Yuan's army has no one left.",
                "두 장수가 제거되니 원군에 인재가 없다.");
            AddTranslation("dialogue_guandu2_victory_caocao", "云长连斩二将，真乃天下无敌！",
                "Yunchang slew two generals in succession - truly unmatched in the world!",
                "운장이 두 장수를 연달아 베었으니 진정 천하무적이로다!");

            // 夜袭乌巢 - 开场对白
            AddTranslation("dialogue_guandu3_opening_narration1", "官渡相持，曹军粮草将尽。正在此时，袁绍谋士许攸来投。",
                "At Guandu's stalemate, Cao's supplies run low. At this moment, Yuan Shao's advisor Xu You defects.",
                "관도에서 대치 중 조군의 군량이 바닥나가고 있었다. 바로 이때 원소의 모사 허유가 투항했다.");
            AddTranslation("dialogue_guandu3_opening_xuyou1", "曹公！许攸不才，特来相投！",
                "Lord Cao! Xu You, though unworthy, comes to join you!",
                "조공! 허유가 재주는 없으나 특별히 투항하러 왔습니다!");
            AddTranslation("dialogue_guandu3_opening_caocao1", "子远来此，必有良策教我！",
                "Ziyuan comes - you must have a good plan to teach me!",
                "자원이 왔으니 좋은 계책을 알려주시오!");
            AddTranslation("dialogue_guandu3_opening_xuyou2", "袁绍粮草尽屯乌巢，守将淳于琼嗜酒成性！",
                "Yuan Shao's supplies are all stored at Wuchao, guarded by Chunyu Qiong who loves his wine!",
                "원소의 군량이 모두 오소에 있는데, 수장 순우경은 술을 좋아합니다!");
            AddTranslation("dialogue_guandu3_opening_caocao2", "乌巢？此话当真？",
                "Wuchao? Is this true?",
                "오소? 이게 사실이오?");
            AddTranslation("dialogue_guandu3_opening_xuyou3", "千真万确！若能烧其粮草，袁军不战自溃！",
                "Absolutely true! If you burn his supplies, Yuan's army will collapse without fighting!",
                "천진만확입니다! 군량을 불태우면 원군은 싸우지 않고도 무너집니다!");
            AddTranslation("dialogue_guandu3_opening_xuyou4", "但须速战速决，不可恋战！",
                "But you must strike fast and retreat fast - don't get bogged down!",
                "하지만 속전속결해야 하며 오래 싸우면 안 됩니다!");
            AddTranslation("dialogue_guandu3_opening_zhangliao", "主公，末将愿随行！",
                "My lord, I volunteer to go with you!",
                "주공, 말장이 함께 가겠습니다!");
            AddTranslation("dialogue_guandu3_opening_xuhuang", "徐晃愿为先锋！",
                "Xu Huang volunteers as vanguard!",
                "서황이 선봉을 서겠습니다!");
            AddTranslation("dialogue_guandu3_opening_caocao3", "好！点齐精兵五千，今夜随我夜袭乌巢！",
                "Good! Gather 5000 elite soldiers - tonight we raid Wuchao!",
                "좋다! 정병 오천을 모아라, 오늘 밤 오소를 야습한다!");
            AddTranslation("dialogue_guandu3_opening_narration2", "夜半时分，曹操率军悄然出发，直扑乌巢。",
                "At midnight, Cao Cao silently led his army, heading straight for Wuchao.",
                "한밤중에 조조가 군대를 이끌고 조용히 출발해 오소로 향했다.");
            AddTranslation("dialogue_guandu3_opening_caocao4", "众将听令！此战只许胜，不许败！",
                "All generals, hear my orders! This battle allows only victory, not defeat!",
                "제장들 명을 들으라! 이 싸움은 오직 승리만 허락한다, 패배는 없다!");
            AddTranslation("dialogue_guandu3_opening_caocao5", "杀！",
                "Kill!",
                "죽여라!");

            // 夜袭乌巢 - 战斗事件对白
            AddTranslation("dialogue_guandu3_caocao_start", "乌巢到了！给我放火！",
                "We've reached Wuchao! Set it ablaze!",
                "오소에 도착했다! 불을 질러라!");
            AddTranslation("dialogue_guandu3_burn_supplies", "火光冲天，袁军粮草开始燃烧！",
                "Flames rise to the sky as Yuan's supplies begin to burn!",
                "불꽃이 하늘을 찌르며 원군의 군량이 타기 시작한다!");
            AddTranslation("dialogue_guandu3_xuhuang_duanliang", "断其粮道，袁军必败！",
                "Cut their supply line - Yuan's army will surely fall!",
                "군량로를 끊으면 원군은 반드시 패한다!");
            AddTranslation("dialogue_guandu3_chunyuqiong_lowHP", "什么……敌袭？再让我……再喝一杯……",
                "What... enemy attack? Let me... have another drink...",
                "뭐... 적습? 한 잔만... 한 잔만 더...");
            AddTranslation("dialogue_guandu3_chunyuqiong_defeat", "粮草……完了……主公……",
                "The supplies... are done for... my lord...",
                "군량이... 끝났다... 주공...");

            // 夜袭乌巢 - 胜利对白
            AddTranslation("dialogue_guandu3_victory_narration", "乌巢大火，照亮夜空。袁军粮草化为灰烬。",
                "The great fire at Wuchao lit up the night sky. Yuan's supplies turned to ash.",
                "오소의 대화재가 밤하늘을 밝혔다. 원군의 군량이 재로 변했다.");
            AddTranslation("dialogue_guandu3_victory_soldier", "主公！粮草尽毁！",
                "My lord! The supplies are completely destroyed!",
                "주공! 군량이 모두 소실되었습니다!");
            AddTranslation("dialogue_guandu3_victory_caocao", "传令三军！全力追击！袁绍败局已定！",
                "Order all armies! Pursue with full force! Yuan Shao's defeat is sealed!",
                "전군에 명하라! 전력으로 추격하라! 원소의 패배는 확정이다!");
            AddTranslation("dialogue_guandu3_victory_xuyou", "恭喜主公！此战必能名垂青史！",
                "Congratulations, my lord! This battle will surely be recorded in history!",
                "축하드립니다 주공! 이 전투는 반드시 역사에 남을 것입니다!");

            // 袁军溃败 - 开场对白
            AddTranslation("dialogue_guandu4_opening_narration1", "乌巢粮草被焚的消息传开，袁军军心大乱。",
                "News of Wuchao's burning spread - Yuan's army fell into chaos.",
                "오소의 군량이 불탔다는 소식이 퍼지자 원군이 크게 혼란에 빠졌다.");
            AddTranslation("dialogue_guandu4_opening_narration2", "将士饥疲，张郃高览率部投降，袁绍孤立无援。",
                "Soldiers hungry and tired, Zhang He and Gao Lan surrendered with their troops - Yuan Shao stood alone.",
                "병사들이 굶주리고 지쳐 장합과 고람이 부대를 이끌고 항복했다. 원소는 고립무원이 되었다.");
            AddTranslation("dialogue_guandu4_opening_soldier1", "主公！张郃、高览率部投曹！",
                "My lord! Zhang He and Gao Lan have defected to Cao with their troops!",
                "주공! 장합과 고람이 부대를 이끌고 조조에게 투항했습니다!");
            AddTranslation("dialogue_guandu4_opening_soldier2", "将士们已经三天没吃饱饭了……",
                "The soldiers haven't eaten properly for three days...",
                "병사들이 사흘 동안 제대로 먹지 못했습니다...");
            AddTranslation("dialogue_guandu4_opening_narration3", "曹操趁势发起总攻，誓要一举击溃袁军。",
                "Cao Cao seized the momentum to launch a final assault, vowing to crush Yuan's army in one stroke.",
                "조조가 기세를 몰아 총공격을 개시하며 원군을 일거에 격파하겠다고 맹세했다.");
            AddTranslation("dialogue_guandu4_opening_caocao", "袁绍！今日便是你的末日！",
                "Yuan Shao! Today is your last day!",
                "원소! 오늘이 네 최후의 날이다!");

            // 袁军溃败 - 战斗事件对白
            AddTranslation("dialogue_guandu4_caocao_start", "全军出击！活捉袁绍！",
                "All forces attack! Capture Yuan Shao alive!",
                "전군 출격! 원소를 생포하라!");
            AddTranslation("dialogue_guandu4_yuanshao_qiji", "看我万箭齐发！",
                "Behold my Arrow Barrage!",
                "내 만전제발을 보아라!");
            AddTranslation("dialogue_guandu4_hungry_tired", "袁军士兵饥疲交加，战力大减。",
                "Yuan's soldiers are hungry and exhausted, their combat power greatly reduced.",
                "원군 병사들이 굶주리고 지쳐 전투력이 크게 떨어졌다.");
            AddTranslation("dialogue_guandu4_yuanshao_lowHP", "怎会如此……我袁家四世三公，岂能败于曹阿瞒！",
                "How can this be... My Yuan family served as Three Dukes for four generations - how can we lose to Cao the bootlicker!",
                "어찌 이럴 수가... 우리 원가는 사세삼공인데 조아만에게 질 수 있단 말인가!");
            AddTranslation("dialogue_guandu4_yuanshao_defeat", "天不助我……撤……撤军……",
                "Heaven doesn't help me... retreat... retreat...",
                "하늘이 나를 돕지 않는구나... 철... 철군...");

            // 袁军溃败 - 胜利对白
            AddTranslation("dialogue_guandu4_victory_narration", "袁绍仓皇北逃，官渡之战以曹操大胜告终。此战奠定曹操统一北方的基础。",
                "Yuan Shao fled north in panic. The Battle of Guandu ended in Cao Cao's great victory, laying the foundation for his unification of the North.",
                "원소가 황급히 북으로 도주하며 관도대전은 조조의 대승으로 끝났다. 이 전투가 조조의 북방 통일 기반을 마련했다.");
            AddTranslation("dialogue_guandu4_victory_caocao", "官渡既定，北方将归曹氏！传令三军，班师回朝！",
                "With Guandu decided, the North will belong to the Cao clan! Order all armies to return to the capital!",
                "관도가 정해졌으니 북방은 조씨의 것이 되리라! 전군에 명하라, 회군하라!");

            // 官渡之战 - 结局
            AddTranslation("dialogue_guandu_ending", "官渡之战后，曹操统一北方已成定局。袁绍不久后忧愤而死，河北四州尽归曹操。这一战，彻底改变了天下格局。",
                "After the Battle of Guandu, Cao Cao's unification of the North became certain. Yuan Shao soon died of grief and anger, and all four provinces of Hebei fell to Cao Cao. This battle completely changed the balance of power in the realm.",
                "관도대전 후 조조의 북방 통일은 기정사실이 되었다. 원소는 얼마 지나지 않아 울분으로 죽고 하북 사주가 모두 조조에게 귀속되었다. 이 전투가 천하의 판도를 완전히 바꾸었다.");

            // ==================== 讨董之战 v2 对白 (Taodong v2 Dialogues) ====================

            // 檄文传天下 - 开场对白
            AddTranslation("dialogue_taodong1_opening_narration", "董卓废立天子，残暴无道。曹操发矫诏，召集天下诸侯共讨国贼。",
                "Dong Zhuo deposed the emperor, ruling with brutality. Cao Cao issued a false edict, summoning lords to destroy the traitor.",
                "동탁이 천자를 폐립하고 잔폭하게 굴었다. 조조가 교조를 발하여 천하 제후들을 불러 국적을 토벌하게 했다.");
            AddTranslation("dialogue_taodong1_opening_caocao1", "董卓乱政，天下共愤！今日曹操不才，愿为先锋，诛此国贼！",
                "Dong Zhuo's tyranny angers all! Today, though unworthy, Cao Cao volunteers as vanguard to slay this traitor!",
                "동탁의 난정에 천하가 공분한다! 오늘 조조가 재주는 없으나 선봉이 되어 이 국적을 베겠다!");
            AddTranslation("dialogue_taodong1_opening_caocao2", "元让，妙才，随我杀敌！",
                "Yuanrang, Miaocai, follow me to slay the enemy!",
                "원양, 묘재, 나를 따라 적을 베라!");
            AddTranslation("dialogue_taodong1_opening_xiahoudun", "主公有令，末将万死不辞！",
                "With my lord's order, I would die ten thousand deaths!",
                "주공의 명이 있으니 말장은 만 번 죽어도 사양치 않겠습니다!");
            AddTranslation("dialogue_taodong1_opening_xiahouyuan", "西凉兵算什么！看我夏侯妙才！",
                "What are Xiliang soldiers! Watch me, Xiahou Miaocai!",
                "서량병이 뭐라고! 나 하후묘재를 보아라!");
            AddTranslation("dialogue_taodong1_opening_caocao3", "杀！为天下苍生！",
                "Kill! For the people of the realm!",
                "죽여라! 천하 창생을 위하여!");

            // 檄文传天下 - 战斗事件对白
            AddTranslation("dialogue_taodong1_caocao_firstkill", "西凉贼兵，也敢阻挡讨贼大军！",
                "Xiliang bandits dare block the army fighting tyranny!",
                "서량 도적들이 감히 토적 대군을 막으려 하다니!");
            AddTranslation("dialogue_taodong1_xiahoudun_damage", "看我独眼夏侯的厉害！",
                "Feel the might of one-eyed Xiahou!",
                "외눈 하후의 위력을 보아라!");
            AddTranslation("dialogue_taodong1_xiahouyuan_shensu", "神速！杀！",
                "Swift as the wind! Kill!",
                "신속! 죽여라!");
            AddTranslation("dialogue_taodong1_all_defeated", "西凉兵已退！继续前进！",
                "Xiliang soldiers are routed! Press forward!",
                "서량병이 물러났다! 계속 전진!");

            // 檄文传天下 - 胜利对白
            AddTranslation("dialogue_taodong1_victory_caocao", "第一仗告捷！但真正的恶战还在后面！",
                "First battle won! But the real fight lies ahead!",
                "첫 번째 전투 승리! 하지만 진짜 싸움은 앞에 있다!");
            AddTranslation("dialogue_taodong1_victory_narration", "曹操击退西凉先锋，联军士气大振。然而，汜水关上华雄正厉兵秣马，等待下一场血战。",
                "Cao Cao repelled the Xiliang vanguard, boosting alliance morale. However, at Sishui Pass, Hua Xiong prepares for the next bloody battle.",
                "조조가 서량 선봉을 물리치자 연합군 사기가 크게 올랐다. 그러나 사수관에서 화웅이 다음 혈전을 준비하고 있었다.");

            // 阵前斩雄 - 开场对白
            AddTranslation("dialogue_taodong2_opening_narration", "华雄勇猛，连斩联军数将。诸侯惶恐，无人敢应战。",
                "Hua Xiong was fierce, slaying several alliance generals in succession. The lords were terrified - none dared accept his challenge.",
                "화웅이 용맹하여 연합군 장수 여럿을 연달아 베었다. 제후들이 두려워 아무도 감히 응전하지 못했다.");
            AddTranslation("dialogue_taodong2_opening_yuanshao", "谁人敢去迎战华雄？",
                "Who dares face Hua Xiong in battle?",
                "누가 감히 화웅과 맞서 싸우겠는가?");
            AddTranslation("dialogue_taodong2_opening_caocao1", "某麾下有一人，可斩华雄！",
                "Under my command is one who can slay Hua Xiong!",
                "제 휘하에 화웅을 벨 수 있는 자가 있습니다!");
            AddTranslation("dialogue_taodong2_opening_guanyu", "某愿往！",
                "I volunteer to go!",
                "제가 가겠습니다!");
            AddTranslation("dialogue_taodong2_opening_yuanshu", "哈！一个马弓手也敢大言不惭！",
                "Ha! A mere mounted archer dares speak so boldly!",
                "하! 고작 마궁수가 감히 큰소리치다니!");
            AddTranslation("dialogue_taodong2_opening_caocao2", "此人虽为马弓手，却有万夫不当之勇！",
                "Though a mounted archer, he has the courage to face ten thousand!",
                "마궁수이지만 만부부당의 용맹이 있습니다!");
            AddTranslation("dialogue_taodong2_opening_caocao3", "关将军，且饮此酒，为君壮行！",
                "General Guan, drink this wine to strengthen your spirit!",
                "관 장군, 이 술을 마시고 기세를 올리시오!");
            AddTranslation("dialogue_taodong2_opening_guanyu2", "酒且斟下，某去便回！",
                "Pour the wine - I'll be back before it cools!",
                "술을 따라 두시오, 금방 돌아오겠소!");
            AddTranslation("dialogue_taodong2_opening_huaxiong", "又是何人来送死！",
                "Who else comes to die!",
                "또 누가 죽으러 오느냐!");

            // 阵前斩雄 - 战斗事件对白
            AddTranslation("dialogue_taodong2_guanyu_slash", "看刀！",
                "Watch my blade!",
                "칼을 받아라!");
            AddTranslation("dialogue_taodong2_huaxiong_slash", "小小马弓手，也敢与我交锋！",
                "A mere mounted archer dares cross blades with me!",
                "고작 마궁수가 감히 나와 칼을 겨루다니!");
            AddTranslation("dialogue_taodong2_huaxiong_lowHP", "这……这人好快的刀！",
                "This... this man's blade is so fast!",
                "이... 이자의 칼이 이렇게 빠르다니!");
            AddTranslation("dialogue_taodong2_guanyu_victory", "华雄已斩！",
                "Hua Xiong is slain!",
                "화웅을 베었다!");

            // 阵前斩雄 - 胜利对白
            AddTranslation("dialogue_taodong2_victory_narration", "关羽提华雄首级归来，酒尚温热。温酒斩华雄，自此名震天下！",
                "Guan Yu returned with Hua Xiong's head - the wine was still warm. Slaying Hua Xiong while the wine was warm, his fame spread across the land!",
                "관우가 화웅의 수급을 들고 돌아왔을 때 술이 아직 따뜻했다. 온주참화웅, 이로써 이름이 천하에 떨쳤다!");
            AddTranslation("dialogue_taodong2_victory_caocao", "云长真神人也！此酒为君庆功！",
                "Yunchang is truly divine! This wine celebrates your achievement!",
                "운장은 진정 신인이로다! 이 술로 공을 축하하오!");
            AddTranslation("dialogue_taodong2_victory_guanyu", "区区华雄，不足挂齿。",
                "A mere Hua Xiong - not worth mentioning.",
                "고작 화웅, 언급할 가치도 없소.");

            // 虎牢关血战 - 开场对白
            AddTranslation("dialogue_taodong3_opening_narration", "华雄既死，董卓亲自坐镇虎牢关，遣吕布迎战。吕布天下第一猛将，无人能敌。",
                "With Hua Xiong dead, Dong Zhuo personally commanded at Hulao Pass, sending Lu Bu to fight. Lu Bu, the mightiest warrior, was unmatched by anyone.",
                "화웅이 죽자 동탁이 직접 호로관에 진을 쳤고 여포를 내보냈다. 여포는 천하제일 맹장으로 아무도 대적할 수 없었다.");
            AddTranslation("dialogue_taodong3_opening_lvbu", "吕布在此！谁敢与我一战！",
                "Lu Bu is here! Who dares fight me!",
                "여포가 여기 있다! 누가 감히 나와 싸우겠느냐!");
            AddTranslation("dialogue_taodong3_opening_zhangfei", "三姓家奴！燕人张飞在此！",
                "Three-family slave! Zhang Fei of Yan is here!",
                "삼성가노! 연인 장비가 여기 있다!");

            // 虎牢关血战 - 战斗事件对白
            AddTranslation("dialogue_taodong3_zhangfei_challenge", "吕布！今日便取你性命！",
                "Lu Bu! Today I take your life!",
                "여포! 오늘 네 목숨을 가져가겠다!");
            AddTranslation("dialogue_taodong3_guanyu_join", "二弟！我来助你！",
                "Second brother! I come to help you!",
                "둘째 동생! 내가 도우러 왔다!");
            AddTranslation("dialogue_taodong3_liubei_join", "二位贤弟！为兄来也！",
                "My two worthy brothers! Elder brother is here!",
                "두 현제여! 형이 왔다!");
            AddTranslation("dialogue_taodong3_lvbu_damaged", "可恶！三人围攻一人，算什么英雄！",
                "Curse you! Three against one - what kind of heroes are you!",
                "빌어먹을! 셋이 하나를 공격하다니 무슨 영웅이냐!");
            AddTranslation("dialogue_taodong3_zhangfei_hit", "吃俺老张一矛！",
                "Taste my spear!",
                "내 창을 받아라!");
            AddTranslation("dialogue_taodong3_guanyu_hit", "再接某一刀！",
                "And another slash from me!",
                "내 칼도 받아라!");
            AddTranslation("dialogue_taodong3_liubei_hit", "三弟小心！",
                "Third brother, be careful!",
                "셋째 동생 조심해라!");
            AddTranslation("dialogue_taodong3_lvbu_lowHP", "罢了！今日暂且饶你们！后会有期！",
                "Enough! I'll spare you today! We'll meet again!",
                "그만이다! 오늘은 일단 봐주마! 다시 만나자!");

            // 虎牢关血战 - 胜利对白
            AddTranslation("dialogue_taodong3_victory_narration", "三英战吕布，终于击退天下第一猛将。此战过后，刘关张三人威名远播。",
                "Three heroes fought Lu Bu, finally driving back the mightiest warrior. After this battle, Liu, Guan, and Zhang's fame spread far and wide.",
                "삼영이 여포와 싸워 마침내 천하제일 맹장을 물리쳤다. 이 전투 후 유관장 삼인의 위명이 널리 퍼졌다.");
            AddTranslation("dialogue_taodong3_victory_zhangfei", "痛快！这吕布也不过如此！",
                "Satisfying! This Lu Bu is nothing special!",
                "통쾌하다! 이 여포도 별것 아니군!");
            AddTranslation("dialogue_taodong3_victory_guanyu", "三弟莫要大意，吕布武艺确实高强。",
                "Don't be careless, third brother - Lu Bu's martial arts are truly formidable.",
                "셋째 방심하지 마라, 여포의 무예는 확실히 대단하다.");
            AddTranslation("dialogue_taodong3_victory_liubei", "二位贤弟辛苦！今日之战，传为美谈！",
                "My two worthy brothers worked hard! Today's battle will become legend!",
                "두 현제 수고했다! 오늘 전투는 미담이 될 것이다!");

            // 洛阳之战 - 开场对白
            AddTranslation("dialogue_taodong4_opening_narration", "吕布败退，联军进逼洛阳。董卓见大势已去，决定挟天子西迁长安。",
                "Lu Bu retreated in defeat, the alliance pressed toward Luoyang. Seeing the situation was lost, Dong Zhuo decided to take the emperor west to Chang'an.",
                "여포가 패퇴하자 연합군이 낙양으로 진격했다. 동탁은 대세가 기울었음을 알고 천자를 끼고 장안으로 서천하기로 했다.");
            AddTranslation("dialogue_taodong4_opening_caocao", "董卓欲逃！众将随我追击！",
                "Dong Zhuo wants to flee! All generals, follow me in pursuit!",
                "동탁이 도주하려 한다! 제장들 나를 따라 추격하라!");
            AddTranslation("dialogue_taodong4_opening_sunjian", "孙坚愿为先锋！",
                "Sun Jian volunteers as vanguard!",
                "손견이 선봉을 서겠습니다!");
            AddTranslation("dialogue_taodong4_opening_yuanshao", "各路诸侯，全力追击！",
                "All lords, pursue with full force!",
                "모든 제후들, 전력으로 추격하라!");
            AddTranslation("dialogue_taodong4_opening_dongzhuo", "李傕！郭汜！给我挡住他们！",
                "Li Jue! Guo Si! Hold them off for me!",
                "이각! 곽사! 저들을 막아라!");

            // 洛阳之战 - 战斗事件对白
            AddTranslation("dialogue_taodong4_dongzhuo_start", "一群乌合之众，也想拦我！",
                "A rabble of fools think they can stop me!",
                "오합지졸들이 감히 나를 막으려 하다니!");
            AddTranslation("dialogue_taodong4_sunjian_damage", "董贼受死！",
                "Die, traitor Dong!",
                "동적 죽어라!");
            AddTranslation("dialogue_taodong4_dongzhuo_lowHP", "可恶……快护我撤退！",
                "Curse it... quickly protect my retreat!",
                "빌어먹을... 빨리 후퇴를 엄호하라!");
            AddTranslation("dialogue_taodong4_dongzhuo_escape", "洛阳就留给你们！天子在我手中，谁敢动我！哈哈哈！",
                "Luoyang is yours! The emperor is in my hands - who dares touch me! Hahaha!",
                "낙양은 너희에게 준다! 천자는 내 손에 있다, 누가 감히 나를 건드리겠느냐! 하하하!");

            // 洛阳之战 - 胜利对白
            AddTranslation("dialogue_taodong4_victory_narration", "董卓挟天子西遁长安，联军收复洛阳。然而洛阳已被董卓一把大火烧成废墟。",
                "Dong Zhuo fled west to Chang'an with the emperor. The alliance recovered Luoyang, but Dong Zhuo had already burned it to ruins.",
                "동탁이 천자를 끼고 장안으로 도주했다. 연합군이 낙양을 수복했으나 동탁이 이미 불태워 폐허가 되어 있었다.");
            AddTranslation("dialogue_taodong4_victory_caocao", "洛阳虽复，天子却落入董贼之手……可恨！",
                "Though Luoyang is recovered, the emperor fell into the traitor's hands... How hateful!",
                "낙양을 수복했으나 천자는 동적의 손에... 분하다!");
            AddTranslation("dialogue_taodong4_victory_sunjian", "此火为洛阳百姓所燃，董卓之罪，罄竹难书！",
                "This fire was lit for Luoyang's people - Dong Zhuo's crimes are too numerous to count!",
                "이 불은 낙양 백성들을 위해 피운 것이다. 동탁의 죄는 이루 말할 수 없다!");

            // 讨董之战 - 结局
            AddTranslation("dialogue_taodong_ending", "讨董联盟虽然收复洛阳，却因诸侯各怀异心而瓦解。董卓挟天子于长安继续专权，而天下英雄各自割据，乱世序幕就此拉开。",
                "Though the alliance recovered Luoyang, it dissolved as lords pursued their own interests. Dong Zhuo continued his tyranny in Chang'an with the emperor, while heroes across the land carved out territories - the curtain of chaos had risen.",
                "토동 연맹이 낙양을 수복했으나 제후들이 각자 속셈을 품어 와해되었다. 동탁은 장안에서 천자를 끼고 계속 전횡했고, 천하의 영웅들은 각자 할거하니 난세의 서막이 열렸다.");

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