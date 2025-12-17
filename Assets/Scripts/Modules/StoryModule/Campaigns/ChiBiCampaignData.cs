using System.Collections.Generic;

namespace ThreeKingdoms.Story.Campaigns
{
    /// <summary>
    /// 赤壁之战战役数据 v2
    /// 《赤壁·火起东南》- 共6场战斗
    /// </summary>
    public static class ChiBiCampaignData
    {
        public static CampaignData CreateCampaign()
        {
            var storyBattleList = new List<StoryBattle>
            {
                CreateBattle1_ChangbanVanguard(),      // 长坂先锋
                CreateBattle2_ZhangfeiBridge(),        // 张飞断桥
                CreateBattle3_DebateScholars(),        // 舌战群儒
                CreateBattle4_JiangganStealsLetter(),  // 蒋干盗书
                CreateBattle5_RiverStandoff(),         // 江上对峙
                CreateBattle6_RedCliffsFire()          // 赤壁火起
            };

            var campaign = new CampaignData
            {
                campaignId = "chibi",
                nameKey = "campaign_chibi",
                descriptionKey = "campaign_chibi_desc",
                isUnlocked = true,
                storyBattles = storyBattleList,
                battles = ConvertToBattleDataList(storyBattleList)
            };

            // 第一战默认解锁
            campaign.storyBattles[0].isUnlocked = true;
            if (campaign.battles.Count > 0)
                campaign.battles[0].isUnlocked = true;

            return campaign;
        }

        /// <summary>
        /// 将StoryBattle列表转换为BattleData列表（兼容旧UI）
        /// </summary>
        private static List<BattleData> ConvertToBattleDataList(List<StoryBattle> storyBattles)
        {
            var result = new List<BattleData>();
            foreach (var sb in storyBattles)
            {
                var bd = new BattleData
                {
                    battleId = sb.battleId,
                    nameKey = sb.nameKey,
                    descriptionKey = sb.descriptionKey,
                    briefingKey = sb.briefingKey,
                    battleImage = sb.battleImage,
                    difficulty = sb.difficulty,
                    turnLimit = sb.turnLimit,
                    specialRuleKey = sb.specialRuleKey,
                    isUnlocked = sb.isUnlocked,
                    isCompleted = sb.isCompleted,
                    bestScore = sb.bestScore
                };

                // 提取玩家角色ID
                if (sb.allies != null && sb.allies.Count > 0)
                {
                    foreach (var ally in sb.allies)
                    {
                        if (ally.isPlayer)
                        {
                            bd.playerGeneralId = ally.characterId;
                            break;
                        }
                    }
                }

                // 提取敌方角色ID
                bd.enemyGeneralIds = new List<string>();
                if (sb.enemies != null)
                {
                    foreach (var enemy in sb.enemies)
                    {
                        bd.enemyGeneralIds.Add(enemy.characterId);
                    }
                }

                result.Add(bd);
            }
            return result;
        }

        #region 开场剧情

        /// <summary>
        /// 创建战役开场剧情对白
        /// </summary>
        public static List<Dialogue> CreateOpeningDialogue()
        {
            return new List<Dialogue>
            {
                Dialogue.Narration("dialogue_chibi_campaign_intro"),
                new Dialogue("char_zhugeliang", "dialogue_chibi_opening_zhuge1"),
                new Dialogue("char_liubei", "dialogue_chibi_opening_liubei1"),
                new Dialogue("char_guanyu", "dialogue_chibi_opening_guanyu"),
                new Dialogue("char_zhugeliang", "dialogue_chibi_opening_zhuge2"),
                new Dialogue("char_liubei", "dialogue_chibi_opening_liubei2"),
                Dialogue.Narration("dialogue_chibi_opening_hint"),
                new Dialogue("char_soldier", "dialogue_chibi_opening_soldier"),
                new Dialogue("char_zhaoyun", "dialogue_chibi_opening_zhaoyun")
            };
        }

        #endregion

        #region 第一战：长坂先锋

        private static StoryBattle CreateBattle1_ChangbanVanguard()
        {
            var battle = new StoryBattle
            {
                battleId = "chibi_1",
                nameKey = "battle_chibi_1",
                subtitleKey = "battle_chibi_1_subtitle",
                descriptionKey = "battle_chibi_1_desc",
                briefingKey = "battle_chibi_1_briefing",
                difficulty = 1,

                // 我方角色
                allies = new List<BattleCharacter>
                {
                    new BattleCharacter("zhaoyun", "char_zhaoyun", 4, true, "longdan")
                },

                // 敌方角色
                enemies = new List<BattleCharacter>
                {
                    new BattleCharacter("caojun_cavalry", "char_caojun_cavalry", 2, false)
                },

                // 胜利条件：击败曹军骑兵
                victoryCondition = new VictoryCondition
                {
                    type = VictoryType.DefeatAllEnemies
                },

                // 失败条件：赵云死亡
                defeatCondition = new DefeatCondition
                {
                    type = DefeatType.PlayerDeath
                },

                // 特殊规则
                specialRules = new List<SpecialRule>
                {
                    new SpecialRule("tutorial", "rule_tutorial", RuleType.Custom)
                },

                // 局内事件
                events = new List<BattleEvent>
                {
                    new BattleEvent(EventTrigger.OnBattleStart, "",
                        new Dialogue("char_zhaoyun", "dialogue_chibi1_zhaoyun_start")),

                    new BattleEvent(EventTrigger.OnSkillActivate, "longdan",
                        Dialogue.Narration("dialogue_chibi1_longdan_tip"))
                    // 注：赵云此时已经去救阿斗，不在此处说话
                },

                // 开场对白
                openingDialogue = CreateOpeningDialogue(),

                // 胜利对白（赵云已离队去救阿斗，糜芳误传赵云投敌）
                victoryDialogue = new List<Dialogue>
                {
                    new Dialogue("char_mifang", "dialogue_chibi1_mifang_rumor"),
                    new Dialogue("char_liubei", "dialogue_chibi1_liubei_trust"),
                    new Dialogue("char_mifang", "dialogue_chibi1_mifang_seen"),
                    new Dialogue("char_zhangfei", "dialogue_chibi1_zhangfei_go")
                }
            };

            return battle;
        }

        #endregion

        #region 第二战：张飞断桥

        private static StoryBattle CreateBattle2_ZhangfeiBridge()
        {
            var battle = new StoryBattle
            {
                battleId = "chibi_2",
                nameKey = "battle_chibi_2",
                subtitleKey = "battle_chibi_2_subtitle",
                descriptionKey = "battle_chibi_2_desc",
                briefingKey = "battle_chibi_2_briefing",
                difficulty = 2,

                // 我方角色
                allies = new List<BattleCharacter>
                {
                    new BattleCharacter("zhangfei", "char_zhangfei", 4, true, "paoxiao")
                },

                // 敌方角色
                enemies = new List<BattleCharacter>
                {
                    new BattleCharacter("xiahoujie", "char_xiahoujie", 4, false, "danlie")
                },

                // 胜利条件：击败夏侯杰
                victoryCondition = new VictoryCondition
                {
                    type = VictoryType.DefeatTarget,
                    targetCharacterId = "xiahoujie"
                },

                // 失败条件：张飞死亡
                defeatCondition = new DefeatCondition
                {
                    type = DefeatType.PlayerDeath
                },

                // 特殊规则
                specialRules = new List<SpecialRule>
                {
                    new SpecialRule("huwei", "rule_huwei", RuleType.Custom),        // 虎威：杀有30%令目标弃牌
                    new SpecialRule("no_peach", "rule_no_peach", RuleType.Custom)   // 单骑断桥：不能用桃
                },

                // 局内事件
                events = new List<BattleEvent>
                {
                    new BattleEvent(EventTrigger.OnBattleStart, "",
                        new Dialogue("char_zhangfei", "dialogue_chibi2_zhangfei_roar")),

                    new BattleEvent(EventTrigger.OnSkillActivate, "danlie",
                        Dialogue.Narration("dialogue_chibi2_xiahoujie_fear")),

                    new BattleEvent(EventTrigger.OnDeath, "夏侯杰",
                        Dialogue.Narration("dialogue_chibi2_xiahoujie_death"))
                },

                // 开场对白
                openingDialogue = new List<Dialogue>
                {
                    Dialogue.Narration("dialogue_chibi2_opening")
                },

                // 胜利对白
                victoryDialogue = new List<Dialogue>
                {
                    new Dialogue("char_caocao", "dialogue_chibi2_caocao_retreat"),
                    new Dialogue("char_zhangfei", "dialogue_chibi2_zhangfei_order"),
                    Dialogue.Narration("dialogue_chibi2_zhaoyun_arrive"),
                    new Dialogue("char_zhaoyun", "dialogue_chibi2_zhaoyun_help"),
                    new Dialogue("char_zhangfei", "dialogue_chibi2_zhangfei_go"),
                    new Dialogue("char_liubei", "dialogue_chibi2_liubei_praise")
                }
            };

            return battle;
        }

        #endregion

        #region 第三战：舌战群儒

        private static StoryBattle CreateBattle3_DebateScholars()
        {
            var battle = new StoryBattle
            {
                battleId = "chibi_3",
                nameKey = "battle_chibi_3",
                subtitleKey = "battle_chibi_3_subtitle",
                descriptionKey = "battle_chibi_3_desc",
                briefingKey = "battle_chibi_3_briefing",
                difficulty = 2,
                turnLimit = 6,

                // 我方角色
                allies = new List<BattleCharacter>
                {
                    new BattleCharacter("zhugeliang", "char_zhugeliang", 3, true, "guanxing", "kongcheng"),
                    new BattleCharacter("lusu", "char_lusu", 3, false, "dimeng")
                },

                // 敌方角色
                enemies = new List<BattleCharacter>
                {
                    new BattleCharacter("zhangzhao", "char_zhangzhao", 3, false, "zhuhe"),
                    new BattleCharacter("yufan", "char_yufan", 3, false, "jienan")
                },

                // 胜利条件：击败张昭和虞翻，或存活6回合
                victoryCondition = new VictoryCondition
                {
                    type = VictoryType.DefeatAllEnemiesOrSurvive,
                    targetTurn = 6
                },

                // 失败条件：诸葛亮死亡
                defeatCondition = new DefeatCondition
                {
                    type = DefeatType.PlayerDeath
                },

                // 特殊规则
                specialRules = new List<SpecialRule>
                {
                    new SpecialRule("debate", "rule_debate", RuleType.Custom),         // 舌战模式
                    new SpecialRule("persuade", "rule_persuade", RuleType.Custom),     // 以理服人
                    new SpecialRule("lusu_help", "rule_lusu_help", RuleType.AllyAutoSupport) // 鲁肃斡旋
                },

                // 局内事件
                events = new List<BattleEvent>
                {
                    new BattleEvent(EventTrigger.OnBattleStart, "",
                        new Dialogue("char_zhangzhao", "dialogue_chibi3_zhangzhao_question")),

                    new BattleEvent(EventTrigger.OnSkillActivate, "guanxing",
                        new Dialogue("char_zhugeliang", "dialogue_chibi3_zhuge_guanxing")),

                    new BattleEvent(EventTrigger.OnSkillActivate, "jienan",
                        new Dialogue("char_yufan", "dialogue_chibi3_yufan_challenge")),

                    new BattleEvent(EventTrigger.OnSkillActivate, "zhuhe",
                        new Dialogue("char_zhangzhao", "dialogue_chibi3_zhangzhao_surrender")),

                    new BattleEvent(EventTrigger.OnRoundStart, "4",
                        new Dialogue("char_lusu", "dialogue_chibi3_lusu_hint")),

                    new BattleEvent(EventTrigger.OnDeath, "张昭",
                        new Dialogue("char_zhangzhao", "dialogue_chibi3_zhangzhao_defeat"))
                },

                // 开场对白
                openingDialogue = new List<Dialogue>
                {
                    Dialogue.Narration("dialogue_chibi3_opening")
                },

                // 胜利对白
                victoryDialogue = new List<Dialogue>
                {
                    new Dialogue("char_zhugeliang", "dialogue_chibi3_zhuge_victory"),
                    new Dialogue("char_lusu", "dialogue_chibi3_lusu_report"),
                    Dialogue.Narration("dialogue_chibi3_alliance")
                }
            };

            return battle;
        }

        #endregion

        #region 第四战：蒋干盗书

        private static StoryBattle CreateBattle4_JiangganStealsLetter()
        {
            var battle = new StoryBattle
            {
                battleId = "chibi_4",
                nameKey = "battle_chibi_4",
                subtitleKey = "battle_chibi_4_subtitle",
                descriptionKey = "battle_chibi_4_desc",
                briefingKey = "battle_chibi_4_briefing",
                difficulty = 3,

                // 我方角色
                allies = new List<BattleCharacter>
                {
                    new BattleCharacter("zhouyu", "char_zhouyu", 3, true, "yingzi", "fanjian"),
                    new BattleCharacter("zhugeliang", "char_zhugeliang", 3, false, "guanxing")
                },

                // 敌方角色
                enemies = new List<BattleCharacter>
                {
                    new BattleCharacter("jianggan", "char_jianggan", 3, false, "daoshu"),
                    new BattleCharacter("caimao", "char_caimao", 4, false, "shuizhan")
                },

                // 胜利条件：累积3个反间标记
                victoryCondition = new VictoryCondition
                {
                    type = VictoryType.AccumulateMarks,
                    targetCount = 3
                },

                // 失败条件：周瑜死亡或蒋干连续3次成功盗书
                defeatCondition = new DefeatCondition
                {
                    type = DefeatType.PlayerDeathOrExceedCount,
                    maxCount = 3
                },

                // 特殊规则
                specialRules = new List<SpecialRule>
                {
                    new SpecialRule("forge_letter", "rule_forge_letter", RuleType.Custom),   // 伪造书信
                    new SpecialRule("trick", "rule_trick", RuleType.Custom),                 // 中计
                    new SpecialRule("suspicion", "rule_suspicion", RuleType.Custom)          // 曹操猜忌
                },

                // 局内事件
                events = new List<BattleEvent>
                {
                    new BattleEvent(EventTrigger.OnBattleStart, "",
                        new Dialogue("char_zhouyu", "dialogue_chibi4_zhouyu_plan")),

                    new BattleEvent(EventTrigger.OnSkillActivate, "daoshu",
                        new Dialogue("char_jianggan", "dialogue_chibi4_jianggan_steal")),

                    new BattleEvent(EventTrigger.OnSkillActivate, "fanjian",
                        new Dialogue("char_zhouyu", "dialogue_chibi4_zhouyu_drunk")),

                    new BattleEvent(EventTrigger.OnMarkerGained, "fanjian_2",
                        new Dialogue("char_jianggan", "dialogue_chibi4_jianggan_found")),

                    new BattleEvent(EventTrigger.OnHPBelow, "蔡瑁",
                        new Dialogue("char_caimao", "dialogue_chibi4_caimao_loyal"))
                    { triggerParam2 = "3" }
                },

                // 开场对白
                openingDialogue = new List<Dialogue>
                {
                    Dialogue.Narration("dialogue_chibi4_opening")
                },

                // 胜利对白
                victoryDialogue = new List<Dialogue>
                {
                    Dialogue.Narration("dialogue_chibi4_jianggan_return"),
                    new Dialogue("char_caocao", "dialogue_chibi4_caocao_kill"),
                    Dialogue.Narration("dialogue_chibi4_caimao_dead"),
                    new Dialogue("char_zhouyu", "dialogue_chibi4_zhouyu_laugh")
                }
            };

            return battle;
        }

        #endregion

        #region 第五战：江上对峙

        private static StoryBattle CreateBattle5_RiverStandoff()
        {
            var battle = new StoryBattle
            {
                battleId = "chibi_5",
                nameKey = "battle_chibi_5",
                subtitleKey = "battle_chibi_5_subtitle",
                descriptionKey = "battle_chibi_5_desc",
                briefingKey = "battle_chibi_5_briefing",
                difficulty = 2,
                turnLimit = 5,

                // 我方角色
                allies = new List<BattleCharacter>
                {
                    new BattleCharacter("liubei", "char_liubei", 4, true, "rende"),
                    new BattleCharacter("guanyu", "char_guanyu", 4, false, "wusheng")
                },

                // 敌方角色
                enemies = new List<BattleCharacter>
                {
                    new BattleCharacter("caojun_sailor1", "char_caojun_sailor", 3, false, "beiren"),
                    new BattleCharacter("caojun_sailor2", "char_caojun_sailor", 3, false, "beiren"),
                    new BattleCharacter("caojun_sailor3", "char_caojun_sailor", 3, false, "beiren")
                },

                // 胜利条件：击败所有敌人或存活5回合
                victoryCondition = new VictoryCondition
                {
                    type = VictoryType.DefeatAllEnemiesOrSurvive,
                    targetTurn = 5
                },

                // 失败条件：刘备死亡
                defeatCondition = new DefeatCondition
                {
                    type = DefeatType.PlayerDeath
                },

                // 特殊规则
                specialRules = new List<SpecialRule>
                {
                    new SpecialRule("retreat_debuff", "rule_retreat_debuff", RuleType.ModifyInitialCards, 0, -1)
                    { targetId = "allies" },
                    new SpecialRule("seasick", "rule_seasick", RuleType.Custom),        // 水土不服
                    new SpecialRule("guanyu_priority", "rule_guanyu_priority", RuleType.Custom) // 关羽优先攻击
                },

                // 局内事件
                events = new List<BattleEvent>
                {
                    new BattleEvent(EventTrigger.OnBattleStart, "",
                        new Dialogue("char_liubei", "dialogue_chibi5_liubei_worry")),

                    new BattleEvent(EventTrigger.OnSkillActivate, "beiren",
                        Dialogue.Narration("dialogue_chibi5_sailor_sick")),

                    new BattleEvent(EventTrigger.OnDeath, "曹军水兵",
                        new Dialogue("char_guanyu", "dialogue_chibi5_guanyu_kill")),

                    new BattleEvent(EventTrigger.OnRoundStart, "3",
                        new Dialogue("char_liubei", "dialogue_chibi5_liubei_wait")),

                    new BattleEvent(EventTrigger.OnRoundStart, "5",
                        Dialogue.Narration("dialogue_chibi5_reinforcement"))
                },

                // 开场对白
                openingDialogue = new List<Dialogue>
                {
                    Dialogue.Narration("dialogue_chibi5_opening")
                },

                // 胜利对白
                victoryDialogue = new List<Dialogue>
                {
                    new Dialogue("char_zhouyu", "dialogue_chibi5_zhouyu_arrive"),
                    new Dialogue("char_zhugeliang", "dialogue_chibi5_zhuge_wind"),
                    new Dialogue("char_huanggai", "dialogue_chibi5_huanggai_fire"),
                    Dialogue.Narration("dialogue_chibi5_alliance_formed")
                }
            };

            return battle;
        }

        #endregion

        #region 第六战：赤壁火起

        private static StoryBattle CreateBattle6_RedCliffsFire()
        {
            var battle = new StoryBattle
            {
                battleId = "chibi_6",
                nameKey = "battle_chibi_6",
                subtitleKey = "battle_chibi_6_subtitle",
                descriptionKey = "battle_chibi_6_desc",
                briefingKey = "battle_chibi_6_briefing",
                difficulty = 4,

                // 我方角色
                allies = new List<BattleCharacter>
                {
                    new BattleCharacter("liubei", "char_liubei", 4, true, "rende"),
                    new BattleCharacter("zhouyu", "char_zhouyu", 3, false, "yingzi", "fanjian"),
                    new BattleCharacter("zhugeliang", "char_zhugeliang", 3, false, "guanxing", "kongcheng"),
                    new BattleCharacter("huanggai", "char_huanggai", 4, false, "kurou")
                },

                // 敌方角色
                enemies = new List<BattleCharacter>
                {
                    new BattleCharacter("caocao", "char_caocao", 7, false, "jianxiong"),
                    new BattleCharacter("xiahoudun", "char_xiahoudun", 4, false, "ganglie"),
                    new BattleCharacter("xiahouyuan", "char_xiahouyuan", 4, false, "shenshu"),
                    new BattleCharacter("zhangliao", "char_zhangliao", 4, false, "tuxi")
                },

                // 胜利条件：击败曹操
                victoryCondition = new VictoryCondition
                {
                    type = VictoryType.DefeatTarget,
                    targetCharacterId = "caocao"
                },

                // 失败条件：刘备死亡或我方全灭
                defeatCondition = new DefeatCondition
                {
                    type = DefeatType.PlayerDeathOrAllAlliesDeath
                },

                // 特殊规则
                specialRules = new List<SpecialRule>
                {
                    // 铁索连船：曹军攻击距离+1
                    new SpecialRule("chain_ships", "rule_chain_ships", RuleType.ModifyAttackRange, 0, 1)
                    { targetId = "enemies" },

                    // 第2回合：密谋，火攻伤害+2
                    new SpecialRule("conspiracy", "rule_conspiracy", RuleType.FireDamageBonus, 2, 2),

                    // 第3回合：东风起，火攻伤害再+1（共+3）
                    new SpecialRule("east_wind", "rule_east_wind", RuleType.FireDamageBonus, 3, 3)
                },

                // 局内事件
                events = new List<BattleEvent>
                {
                    new BattleEvent(EventTrigger.OnBattleStart, "",
                        new Dialogue("char_zhugeliang", "dialogue_chibi6_zhuge_wind"),
                        new Dialogue("char_zhouyu", "dialogue_chibi6_zhouyu_fire")),

                    new BattleEvent(EventTrigger.OnRoundStart, "2",
                        Dialogue.Narration("dialogue_chibi6_round2_conspiracy")),

                    new BattleEvent(EventTrigger.OnRoundStart, "3",
                        Dialogue.Narration("dialogue_chibi6_round3_wind")),

                    new BattleEvent(EventTrigger.OnSkillActivate, "kurou",
                        new Dialogue("char_huanggai", "dialogue_chibi6_huanggai_fire")),

                    new BattleEvent(EventTrigger.OnCardPlayed, "FireKill",
                        new Dialogue("char_huanggai", "dialogue_chibi6_huanggai_kill")),

                    new BattleEvent(EventTrigger.OnDamageTaken, "曹操",
                        new Dialogue("char_caocao", "dialogue_chibi6_caocao_trap")),

                    new BattleEvent(EventTrigger.OnTurnStart, "张辽",
                        new Dialogue("char_zhangliao", "dialogue_chibi6_zhangliao_chaos")),

                    new BattleEvent(EventTrigger.OnTurnStart, "诸葛亮",
                        new Dialogue("char_zhugeliang", "dialogue_chibi6_zhuge_strong")),

                    new BattleEvent(EventTrigger.OnHPBelow, "曹操",
                        new Dialogue("char_xiahoudun", "dialogue_chibi6_xiahoudun_retreat"))
                    { triggerParam2 = "3" }
                },

                // 开场对白
                openingDialogue = new List<Dialogue>
                {
                    Dialogue.Narration("dialogue_chibi6_opening")
                },

                // 胜利对白
                victoryDialogue = new List<Dialogue>
                {
                    new Dialogue("char_caocao", "dialogue_chibi6_caocao_heaven"),
                    new Dialogue("char_zhouyu", "dialogue_chibi6_zhouyu_humanity"),
                    Dialogue.Narration("dialogue_chibi6_ending1"),
                    Dialogue.Narration("dialogue_chibi6_ending2")
                }
            };

            return battle;
        }

        #endregion
    }
}
