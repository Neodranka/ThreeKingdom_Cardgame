using System.Collections.Generic;

namespace ThreeKingdoms.Story.Campaigns
{
    /// <summary>
    /// 讨董之战战役数据
    /// 《入洛风云·讨董联军》- 共4场战斗
    /// </summary>
    public static class TaodongCampaignData
    {
        public static CampaignData CreateCampaign()
        {
            var storyBattleList = new List<StoryBattle>
            {
                CreateBattle1_SuanzaoAlliance(),
                CreateBattle2_XingyangBattle(),
                CreateBattle3_SlayHuaxiong(),
                CreateBattle4_AllianceCollapse()
            };

            var campaign = new CampaignData
            {
                campaignId = "taodong",
                nameKey = "campaign_taodong",
                descriptionKey = "campaign_taodong_desc",
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

        #region 第一战：酸枣会盟

        /// <summary>
        /// 酸枣会盟 - 曹操视角
        /// 讨董联军，各怀心思
        /// </summary>
        private static StoryBattle CreateBattle1_SuanzaoAlliance()
        {
            var battle = new StoryBattle
            {
                battleId = "taodong_1",
                nameKey = "battle_taodong_1",
                subtitleKey = "battle_taodong_1_subtitle",
                descriptionKey = "battle_taodong_1_desc",
                briefingKey = "battle_taodong_1_briefing",
                difficulty = 2,
                turnLimit = 6,

                // 我方角色
                allies = new List<BattleCharacter>
                {
                    new BattleCharacter("caocao_story", "char_caocao", 4, true, "jianxiong"),
                    new BattleCharacter("xiahoudun", "char_xiahoudun", 4, false, "ganglie")
                },

                // 敌方角色 - 名义盟友但实为阻碍
                enemies = new List<BattleCharacter>
                {
                    new BattleCharacter("yuanshao_story", "char_yuanshao", 4, false, "xueyi"),
                    new BattleCharacter("yuanshu", "char_yuanshu", 4, false, "wangzun"),
                    new BattleCharacter("lords_hesitation", "char_lords_hesitation", 3, false, "xiaoji")
                },

                // 胜利条件：击败诸侯观望或存活6回合
                victoryCondition = new VictoryCondition
                {
                    type = VictoryType.DefeatTarget,
                    targetCharacterId = "lords_hesitation"
                },

                // 失败条件：曹操死亡
                defeatCondition = new DefeatCondition
                {
                    type = DefeatType.PlayerDeath
                },

                // 特殊规则
                specialRules = new List<SpecialRule>
                {
                    // 各怀私心：所有角色摸牌-1
                    new SpecialRule("selfish_lords", "rule_selfish_lords", RuleType.ModifyDrawCards, 0, -1),
                    // 盟主威望：袁绍首次伤害防止1点
                    new SpecialRule("leader_prestige", "rule_leader_prestige", RuleType.DamageReduction)
                    { targetId = "yuanshao_story" },
                    // 名义联盟：不能对袁绍袁术出杀
                    new SpecialRule("nominal_alliance", "rule_nominal_alliance", RuleType.NoAttackTarget)
                },

                // 局内事件
                events = new List<BattleEvent>
                {
                    // 袁绍发言
                    new BattleEvent(EventTrigger.OnBattleStart, "",
                        new Dialogue("char_yuanshao", "dialogue_taodong1_yuanshao_speech")),

                    // 第2回合开始
                    new BattleEvent(EventTrigger.OnRoundStart, "2",
                        new Dialogue("char_caocao", "dialogue_taodong1_caocao_doubt")),

                    // 袁术回合开始
                    new BattleEvent(EventTrigger.OnTurnStart, "袁术",
                        new Dialogue("char_yuanshu", "dialogue_taodong1_yuanshu_greedy")),

                    // 诸侯观望被击败
                    new BattleEvent(EventTrigger.OnDeath, "lords_hesitation",
                        Dialogue.Narration("dialogue_taodong1_consensus"))
                },

                // 开场对白
                openingDialogue = new List<Dialogue>
                {
                    Dialogue.Narration("dialogue_taodong1_opening_narration")
                },

                // 胜利对白
                victoryDialogue = new List<Dialogue>
                {
                    new Dialogue("char_caocao", "dialogue_taodong1_victory_caocao"),
                    new Dialogue("char_xiahoudun", "dialogue_taodong1_victory_xiahoudun"),
                    new Dialogue("char_caocao", "dialogue_taodong1_victory_caocao2")
                }
            };

            return battle;
        }

        #endregion

        #region 第二战：荥阳血战

        /// <summary>
        /// 荥阳血战 - 曹操视角
        /// 孤军深入
        /// </summary>
        private static StoryBattle CreateBattle2_XingyangBattle()
        {
            var battle = new StoryBattle
            {
                battleId = "taodong_2",
                nameKey = "battle_taodong_2",
                subtitleKey = "battle_taodong_2_subtitle",
                descriptionKey = "battle_taodong_2_desc",
                briefingKey = "battle_taodong_2_briefing",
                difficulty = 4,
                turnLimit = 5,

                // 我方角色
                allies = new List<BattleCharacter>
                {
                    new BattleCharacter("caocao_story", "char_caocao", 4, true, "jianxiong"),
                    new BattleCharacter("xiahoudun", "char_xiahoudun", 4, false, "ganglie"),
                    new BattleCharacter("xiahouyuan", "char_xiahouyuan", 4, false, "shensu")
                },

                // 敌方角色
                enemies = new List<BattleCharacter>
                {
                    new BattleCharacter("xurong", "char_xurong", 5, false, "fuji"),
                    new BattleCharacter("xiliang_cavalry", "char_xiliang_cavalry", 3, false, "chongzhen")
                },

                // 胜利条件：击败徐荣或存活5回合
                victoryCondition = new VictoryCondition
                {
                    type = VictoryType.SurviveTurns,
                    targetTurn = 5
                },

                // 失败条件：曹操死亡
                defeatCondition = new DefeatCondition
                {
                    type = DefeatType.PlayerDeath
                },

                // 特殊规则
                specialRules = new List<SpecialRule>
                {
                    // 孤军深入：曹操初始手牌-1，首回合不能用桃
                    new SpecialRule("isolated_army", "rule_isolated_army", RuleType.ModifyInitialCards, 0, -1)
                    { targetId = "caocao_story" },
                    // 死战不退：曹操受伤后摸1牌
                    new SpecialRule("fight_to_death", "rule_fight_to_death", RuleType.DrawOnDamage),
                    // 西凉精骑：对已受伤目标杀+1伤害
                    new SpecialRule("xiliang_elite", "rule_xiliang_elite", RuleType.DamageToWounded)
                },

                // 局内事件
                events = new List<BattleEvent>
                {
                    // 夏侯惇回合开始
                    new BattleEvent(EventTrigger.OnTurnStart, "夏侯惇",
                        new Dialogue("char_xiahoudun", "dialogue_taodong2_xiahoudun_loyal")),

                    // 徐荣首次出杀
                    new BattleEvent(EventTrigger.OnCardPlayed, "杀",
                        new Dialogue("char_xurong", "dialogue_taodong2_xurong_mock")),

                    // 曹操体力<=2
                    new BattleEvent(EventTrigger.OnHPBelow, "曹操",
                        Dialogue.Narration("dialogue_taodong2_caocao_wounded"))
                    { triggerParam2 = "2" },

                    // 曹操触发死战不退
                    new BattleEvent(EventTrigger.OnDamageTaken, "曹操",
                        new Dialogue("char_caocao", "dialogue_taodong2_caocao_vow"))
                },

                // 开场对白
                openingDialogue = new List<Dialogue>
                {
                    new Dialogue("char_caocao", "dialogue_taodong2_opening_caocao")
                },

                // 胜利对白
                victoryDialogue = new List<Dialogue>
                {
                    Dialogue.Narration("dialogue_taodong2_victory_narration"),
                    new Dialogue("char_caocao", "dialogue_taodong2_victory_caocao")
                }
            };

            return battle;
        }

        #endregion

        #region 第三战：斩杀华雄

        /// <summary>
        /// 斩杀华雄 - 孙坚视角
        /// 江东猛虎
        /// </summary>
        private static StoryBattle CreateBattle3_SlayHuaxiong()
        {
            var battle = new StoryBattle
            {
                battleId = "taodong_3",
                nameKey = "battle_taodong_3",
                subtitleKey = "battle_taodong_3_subtitle",
                descriptionKey = "battle_taodong_3_desc",
                briefingKey = "battle_taodong_3_briefing",
                difficulty = 3,

                // 我方角色
                allies = new List<BattleCharacter>
                {
                    new BattleCharacter("sunjian_story", "char_sunjian", 4, true, "yinghun"),
                    new BattleCharacter("chengpu", "char_chengpu", 4, false, "laodang"),
                    new BattleCharacter("huanggai_story", "char_huanggai", 4, false, "kurou")
                },

                // 敌方角色
                enemies = new List<BattleCharacter>
                {
                    new BattleCharacter("huaxiong", "char_huaxiong", 5, false, "yaowu"),
                    new BattleCharacter("huzhen", "char_huzhen", 4, false, "qiezhan"),
                    new BattleCharacter("xiliang_soldier", "char_xiliang_soldier", 3, false)
                },

                // 胜利条件：击败华雄
                victoryCondition = new VictoryCondition
                {
                    type = VictoryType.DefeatTarget,
                    targetCharacterId = "huaxiong"
                },

                // 失败条件：孙坚死亡
                defeatCondition = new DefeatCondition
                {
                    type = DefeatType.PlayerDeath
                },

                // 特殊规则
                specialRules = new List<SpecialRule>
                {
                    // 江东猛虎：孙坚首次造成伤害后摸1牌
                    new SpecialRule("jiangdong_tiger", "rule_jiangdong_tiger", RuleType.DrawOnFirstDamage),
                    // 士气如虹：我方手牌上限+1
                    new SpecialRule("high_morale", "rule_high_morale", RuleType.ModifyHandLimit, 0, 1)
                    { targetId = "allies" },
                    // 先声夺人：第1回合孙坚可额外出1杀
                    new SpecialRule("preemptive_strike", "rule_preemptive_strike", RuleType.ExtraSlash, 1)
                },

                // 局内事件
                events = new List<BattleEvent>
                {
                    // 华雄回合开始
                    new BattleEvent(EventTrigger.OnTurnStart, "华雄",
                        new Dialogue("char_huaxiong", "dialogue_taodong3_huaxiong_mock")),

                    // 黄盖发动苦肉
                    new BattleEvent(EventTrigger.OnSkillActivate, "kurou",
                        new Dialogue("char_huanggai", "dialogue_taodong3_huanggai_brave")),

                    // 孙坚触发江东猛虎
                    new BattleEvent(EventTrigger.OnDamageTaken, "",
                        new Dialogue("char_chengpu", "dialogue_taodong3_chengpu_praise")),

                    // 华雄体力<=2
                    new BattleEvent(EventTrigger.OnHPBelow, "华雄",
                        new Dialogue("char_huaxiong", "dialogue_taodong3_huaxiong_shocked"))
                    { triggerParam2 = "2" },

                    // 华雄被击败
                    new BattleEvent(EventTrigger.OnDeath, "huaxiong",
                        new Dialogue("char_sunjian", "dialogue_taodong3_sunjian_victory"))
                },

                // 开场对白
                openingDialogue = new List<Dialogue>
                {
                    new Dialogue("char_sunjian", "dialogue_taodong3_opening_sunjian")
                },

                // 胜利对白
                victoryDialogue = new List<Dialogue>
                {
                    Dialogue.Narration("dialogue_taodong3_victory_narration"),
                    new Dialogue("char_sunjian", "dialogue_taodong3_victory_sunjian")
                }
            };

            return battle;
        }

        #endregion

        #region 第四战：联军瓦解

        /// <summary>
        /// 联军瓦解 - 曹操视角
        /// 大厦将倾
        /// </summary>
        private static StoryBattle CreateBattle4_AllianceCollapse()
        {
            var battle = new StoryBattle
            {
                battleId = "taodong_4",
                nameKey = "battle_taodong_4",
                subtitleKey = "battle_taodong_4_subtitle",
                descriptionKey = "battle_taodong_4_desc",
                briefingKey = "battle_taodong_4_briefing",
                difficulty = 3,
                turnLimit = 7,

                // 我方角色
                allies = new List<BattleCharacter>
                {
                    new BattleCharacter("caocao_story", "char_caocao", 4, true, "jianxiong"),
                    new BattleCharacter("sunjian_story", "char_sunjian", 4, false, "yinghun")
                },

                // 敌方角色
                enemies = new List<BattleCharacter>
                {
                    new BattleCharacter("yuanshao_story", "char_yuanshao", 4, false, "xueyi"),
                    new BattleCharacter("yuanshu", "char_yuanshu", 4, false, "wangzun"),
                    new BattleCharacter("alliance_strife", "char_alliance_strife", 5, false, "fenlie")
                },

                // 胜利条件：击败联军内斗或存活7回合
                victoryCondition = new VictoryCondition
                {
                    type = VictoryType.DefeatTarget,
                    targetCharacterId = "alliance_strife"
                },

                // 失败条件：曹操死亡
                defeatCondition = new DefeatCondition
                {
                    type = DefeatType.PlayerDeath
                },

                // 特殊规则
                specialRules = new List<SpecialRule>
                {
                    // 联盟崩溃：所有角色摸牌-1
                    new SpecialRule("alliance_collapse", "rule_alliance_collapse", RuleType.ModifyDrawCards, 0, -1),
                    // 各自为战：我方角色间不能用桃
                    new SpecialRule("every_man_for_himself", "rule_every_man_for_himself", RuleType.NoAllyPeach),
                    // 洛阳焦土：第3回合开始所有角色每回合失去1体力
                    new SpecialRule("luoyang_ruins", "rule_luoyang_ruins", RuleType.DamageOverTime, 3)
                },

                // 局内事件
                events = new List<BattleEvent>
                {
                    // 袁术回合开始
                    new BattleEvent(EventTrigger.OnTurnStart, "袁术",
                        new Dialogue("char_yuanshu", "dialogue_taodong4_yuanshu_argue")),

                    // 袁绍回合开始
                    new BattleEvent(EventTrigger.OnTurnStart, "袁绍",
                        new Dialogue("char_yuanshao", "dialogue_taodong4_yuanshao_blame")),

                    // 第3回合洛阳焦土
                    new BattleEvent(EventTrigger.OnRoundStart, "3",
                        Dialogue.Narration("dialogue_taodong4_round3_ruins")),

                    // 孙坚受到伤害
                    new BattleEvent(EventTrigger.OnDamageTaken, "孙坚",
                        new Dialogue("char_sunjian", "dialogue_taodong4_sunjian_lament")),

                    // 联军内斗被击败
                    new BattleEvent(EventTrigger.OnDeath, "alliance_strife",
                        new Dialogue("char_caocao", "dialogue_taodong4_caocao_ambition"))
                },

                // 开场对白
                openingDialogue = new List<Dialogue>
                {
                    Dialogue.Narration("dialogue_taodong4_opening_narration")
                },

                // 胜利对白
                victoryDialogue = new List<Dialogue>
                {
                    new Dialogue("char_yuanshao", "dialogue_taodong4_yuanshao_sigh"),
                    new Dialogue("char_caocao", "dialogue_taodong4_victory_caocao"),
                    Dialogue.Narration("dialogue_taodong4_ending_narration")
                }
            };

            return battle;
        }

        #endregion
    }
}
