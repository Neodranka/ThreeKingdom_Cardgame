using System.Collections.Generic;

namespace ThreeKingdoms.Story.Campaigns
{
    /// <summary>
    /// 讨董之战战役数据 v2
    /// 《讨伐逆贼·匡扶汉室》- 共4场战斗
    /// </summary>
    public static class TaodongCampaignData
    {
        public static CampaignData CreateCampaign()
        {
            var storyBattleList = new List<StoryBattle>
            {
                CreateBattle1_Proclamation(),      // 檄文传天下 - 曹操发檄文
                CreateBattle2_SlayHuaxiong(),      // 阵前斩雄 - 关羽温酒斩华雄
                CreateBattle3_HulaoBattle(),       // 虎牢关血战 - 三英战吕布
                CreateBattle4_LuoyangBattle()      // 洛阳之战 - 董卓西逃
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

        #region 第一战：檄文传天下

        /// <summary>
        /// 檄文传天下 - 曹操发檄文讨董
        /// 「义兵举义旗」
        /// </summary>
        private static StoryBattle CreateBattle1_Proclamation()
        {
            var battle = new StoryBattle
            {
                battleId = "taodong_1",
                nameKey = "battle_taodong_1",
                subtitleKey = "battle_taodong_1_subtitle",
                descriptionKey = "battle_taodong_1_desc",
                briefingKey = "battle_taodong_1_briefing",
                difficulty = 1,

                // 我方角色 - 曹操、夏侯惇、夏侯渊
                allies = new List<BattleCharacter>
                {
                    new BattleCharacter("caocao_story", "char_caocao", 4, true, "jianxiong"),
                    new BattleCharacter("xiahoudun", "char_xiahoudun", 4, false, "ganglie"),
                    new BattleCharacter("xiahouyuan", "char_xiahouyuan", 4, false, "shensu")
                },

                // 敌方角色 - 西凉兵 x 3
                enemies = new List<BattleCharacter>
                {
                    new BattleCharacter("xiliang_soldier_1", "char_xiliang_soldier", 3, false),
                    new BattleCharacter("xiliang_soldier_2", "char_xiliang_soldier", 3, false),
                    new BattleCharacter("xiliang_soldier_3", "char_xiliang_soldier", 3, false)
                },

                // 胜利条件：击败全部西凉兵
                victoryCondition = new VictoryCondition
                {
                    type = VictoryType.DefeatAllEnemies
                },

                // 失败条件：曹操阵亡
                defeatCondition = new DefeatCondition
                {
                    type = DefeatType.PlayerDeath
                },

                // 特殊规则
                specialRules = new List<SpecialRule>
                {
                    // 义愤填膺：曹操首次造成伤害+1
                    new SpecialRule("righteous_fury", "rule_righteous_fury", RuleType.FirstDamageBonus)
                    { targetId = "caocao_story" },
                    // 救民：击败敌人后全体回复1点体力
                    new SpecialRule("save_people", "rule_save_people", RuleType.HealOnKill)
                },

                // 局内事件
                events = new List<BattleEvent>
                {
                    // 曹操首次击杀
                    new BattleEvent(EventTrigger.OnKill, "caocao_story",
                        new Dialogue("char_caocao", "dialogue_taodong1_caocao_firstkill")),

                    // 夏侯惇造成伤害
                    new BattleEvent(EventTrigger.OnDamageDone, "xiahoudun",
                        new Dialogue("char_xiahoudun", "dialogue_taodong1_xiahoudun_damage")),

                    // 夏侯渊发动神速
                    new BattleEvent(EventTrigger.OnSkillActivate, "shensu",
                        new Dialogue("char_xiahouyuan", "dialogue_taodong1_xiahouyuan_shensu")),

                    // 最后一个西凉兵被击败
                    new BattleEvent(EventTrigger.OnAllEnemiesDefeated, "",
                        Dialogue.Narration("dialogue_taodong1_all_defeated"))
                },

                // 开场对白
                openingDialogue = new List<Dialogue>
                {
                    Dialogue.Narration("dialogue_taodong1_opening_narration"),
                    new Dialogue("char_caocao", "dialogue_taodong1_opening_caocao1"),
                    new Dialogue("char_caocao", "dialogue_taodong1_opening_caocao2"),
                    new Dialogue("char_xiahoudun", "dialogue_taodong1_opening_xiahoudun"),
                    new Dialogue("char_xiahouyuan", "dialogue_taodong1_opening_xiahouyuan"),
                    new Dialogue("char_caocao", "dialogue_taodong1_opening_caocao3")
                },

                // 胜利对白
                victoryDialogue = new List<Dialogue>
                {
                    new Dialogue("char_caocao", "dialogue_taodong1_victory_caocao"),
                    Dialogue.Narration("dialogue_taodong1_victory_narration")
                }
            };

            return battle;
        }

        #endregion

        #region 第二战：阵前斩雄

        /// <summary>
        /// 阵前斩雄 - 关羽温酒斩华雄
        /// 「酒尚温时」
        /// </summary>
        private static StoryBattle CreateBattle2_SlayHuaxiong()
        {
            var battle = new StoryBattle
            {
                battleId = "taodong_2",
                nameKey = "battle_taodong_2",
                subtitleKey = "battle_taodong_2_subtitle",
                descriptionKey = "battle_taodong_2_desc",
                briefingKey = "battle_taodong_2_briefing",
                difficulty = 2,

                // 我方角色 - 关羽（单挑）
                allies = new List<BattleCharacter>
                {
                    new BattleCharacter("guanyu_story", "char_guanyu", 4, true, "wusheng")
                },

                // 敌方角色 - 华雄（单挑）
                enemies = new List<BattleCharacter>
                {
                    new BattleCharacter("huaxiong", "char_huaxiong", 5, false, "yaowuv2")
                },

                // 胜利条件：击败华雄
                victoryCondition = new VictoryCondition
                {
                    type = VictoryType.DefeatTarget,
                    targetCharacterId = "huaxiong"
                },

                // 失败条件：关羽阵亡
                defeatCondition = new DefeatCondition
                {
                    type = DefeatType.PlayerDeath
                },

                // 特殊规则
                specialRules = new List<SpecialRule>
                {
                    // 单挑：1v1对决，不受外部干扰
                    new SpecialRule("duel_mode", "rule_duel_mode", RuleType.Custom),
                    // 酒尚温：关羽首次使用杀不可被闪避
                    new SpecialRule("wine_still_warm", "rule_wine_still_warm", RuleType.FirstSlashUndodgeable)
                    { targetId = "guanyu_story" }
                },

                // 局内事件
                events = new List<BattleEvent>
                {
                    // 关羽首次使用杀
                    new BattleEvent(EventTrigger.OnCardPlayed, "杀",
                        new Dialogue("char_guanyu", "dialogue_taodong2_guanyu_slash")),

                    // 华雄使用杀
                    new BattleEvent(EventTrigger.OnCardPlayed, "杀",
                        new Dialogue("char_huaxiong", "dialogue_taodong2_huaxiong_slash")),

                    // 华雄体力≤2
                    new BattleEvent(EventTrigger.OnHealthLow, "huaxiong",
                        new Dialogue("char_huaxiong", "dialogue_taodong2_huaxiong_lowHP")),

                    // 华雄被击败
                    new BattleEvent(EventTrigger.OnDeath, "huaxiong",
                        new Dialogue("char_guanyu", "dialogue_taodong2_guanyu_victory"))
                },

                // 开场对白
                openingDialogue = new List<Dialogue>
                {
                    Dialogue.Narration("dialogue_taodong2_opening_narration"),
                    new Dialogue("char_yuanshao", "dialogue_taodong2_opening_yuanshao"),
                    new Dialogue("char_caocao", "dialogue_taodong2_opening_caocao1"),
                    new Dialogue("char_guanyu", "dialogue_taodong2_opening_guanyu"),
                    new Dialogue("char_yuanshu", "dialogue_taodong2_opening_yuanshu"),
                    new Dialogue("char_caocao", "dialogue_taodong2_opening_caocao2"),
                    new Dialogue("char_caocao", "dialogue_taodong2_opening_caocao3"),
                    new Dialogue("char_guanyu", "dialogue_taodong2_opening_guanyu2"),
                    new Dialogue("char_huaxiong", "dialogue_taodong2_opening_huaxiong")
                },

                // 胜利对白
                victoryDialogue = new List<Dialogue>
                {
                    Dialogue.Narration("dialogue_taodong2_victory_narration"),
                    new Dialogue("char_caocao", "dialogue_taodong2_victory_caocao"),
                    new Dialogue("char_guanyu", "dialogue_taodong2_victory_guanyu")
                }
            };

            return battle;
        }

        #endregion

        #region 第三战：虎牢关血战

        /// <summary>
        /// 虎牢关血战 - 三英战吕布
        /// 「绝世武力」
        /// </summary>
        private static StoryBattle CreateBattle3_HulaoBattle()
        {
            var battle = new StoryBattle
            {
                battleId = "taodong_3",
                nameKey = "battle_taodong_3",
                subtitleKey = "battle_taodong_3_subtitle",
                descriptionKey = "battle_taodong_3_desc",
                briefingKey = "battle_taodong_3_briefing",
                difficulty = 4,

                // 我方角色 - 张飞（关羽R2加入，刘备R3加入）
                allies = new List<BattleCharacter>
                {
                    new BattleCharacter("zhangfei_story", "char_zhangfei", 4, true, "paoxiao")
                },

                // 敌方角色 - 吕布（4HP基础+3临时=7HP，无双）
                enemies = new List<BattleCharacter>
                {
                    new BattleCharacter("lvbu", "char_lvbu", 4, false, "wushuang")
                },

                // 胜利条件：击败吕布
                victoryCondition = new VictoryCondition
                {
                    type = VictoryType.DefeatTarget,
                    targetCharacterId = "lvbu"
                },

                // 失败条件：张飞阵亡（或所有我方角色阵亡）
                defeatCondition = new DefeatCondition
                {
                    type = DefeatType.AllAlliesDeath
                },

                // 特殊规则
                specialRules = new List<SpecialRule>
                {
                    // 一骑当千：吕布战斗开始时额外+3体力上限，刘备上场时移除
                    // 参数：triggerTurn=0（战斗开始），value=3（+3体力）
                    new SpecialRule("one_man_army", "rule_one_man_army", RuleType.TemporaryHPBonus, 0, 3)
                    { targetId = "lvbu", extraInfo = "3" },
                    // 绝世武力：第1回合吕布使用杀的伤害x2
                    new SpecialRule("peerless_might", "rule_peerless_might", RuleType.DoubleDamage, 1)
                    { targetId = "lvbu" },
                    // 逐步增援：第2回合关羽加入，第3回合刘备加入
                    new SpecialRule("gradual_reinforcement", "rule_gradual_reinforcement", RuleType.ReinforcementOnRound)
                    { extraInfo = "guanyu_story:2,liubei_story:3" }
                },

                // 增援角色配置
                reinforcements = new List<ReinforcementData>
                {
                    new ReinforcementData
                    {
                        roundNumber = 2,
                        character = new BattleCharacter("guanyu_story", "char_guanyu", 4, false, "wusheng")
                    },
                    new ReinforcementData
                    {
                        roundNumber = 3,
                        character = new BattleCharacter("liubei_story", "char_liubei", 4, false, "rende")
                    }
                },

                // 局内事件
                events = new List<BattleEvent>
                {
                    // 张飞第1回合开场
                    new BattleEvent(EventTrigger.OnBattleStart, "",
                        new Dialogue("char_zhangfei", "dialogue_taodong3_zhangfei_challenge")),

                    // 第2回合关羽加入
                    new BattleEvent(EventTrigger.OnRoundStart, "2",
                        new Dialogue("char_guanyu", "dialogue_taodong3_guanyu_join")),

                    // 第3回合刘备加入
                    new BattleEvent(EventTrigger.OnRoundStart, "3",
                        new Dialogue("char_liubei", "dialogue_taodong3_liubei_join")),

                    // 吕布受到伤害时
                    new BattleEvent(EventTrigger.OnDamageTaken, "lvbu",
                        new Dialogue("char_lvbu", "dialogue_taodong3_lvbu_damaged")),

                    // 张飞造成伤害
                    new BattleEvent(EventTrigger.OnDamageDone, "zhangfei_story",
                        new Dialogue("char_zhangfei", "dialogue_taodong3_zhangfei_hit")),

                    // 关羽造成伤害
                    new BattleEvent(EventTrigger.OnDamageDone, "guanyu_story",
                        new Dialogue("char_guanyu", "dialogue_taodong3_guanyu_hit")),

                    // 刘备造成伤害
                    new BattleEvent(EventTrigger.OnDamageDone, "liubei_story",
                        new Dialogue("char_liubei", "dialogue_taodong3_liubei_hit")),

                    // 吕布体力≤3
                    new BattleEvent(EventTrigger.OnHealthLow, "lvbu",
                        new Dialogue("char_lvbu", "dialogue_taodong3_lvbu_lowHP"))
                    { triggerParam2 = "3" }
                },

                // 开场对白
                openingDialogue = new List<Dialogue>
                {
                    Dialogue.Narration("dialogue_taodong3_opening_narration"),
                    new Dialogue("char_lvbu", "dialogue_taodong3_opening_lvbu"),
                    new Dialogue("char_zhangfei", "dialogue_taodong3_opening_zhangfei")
                },

                // 胜利对白
                victoryDialogue = new List<Dialogue>
                {
                    Dialogue.Narration("dialogue_taodong3_victory_narration"),
                    new Dialogue("char_zhangfei", "dialogue_taodong3_victory_zhangfei"),
                    new Dialogue("char_guanyu", "dialogue_taodong3_victory_guanyu"),
                    new Dialogue("char_liubei", "dialogue_taodong3_victory_liubei")
                }
            };

            return battle;
        }

        #endregion

        #region 第四战：洛阳之战

        /// <summary>
        /// 洛阳之战 - 董卓西逃
        /// 「挟天子西遁」
        /// </summary>
        private static StoryBattle CreateBattle4_LuoyangBattle()
        {
            var battle = new StoryBattle
            {
                battleId = "taodong_4",
                nameKey = "battle_taodong_4",
                subtitleKey = "battle_taodong_4_subtitle",
                descriptionKey = "battle_taodong_4_desc",
                briefingKey = "battle_taodong_4_briefing",
                difficulty = 3,

                // 我方角色 - 曹操、孙坚、袁绍
                allies = new List<BattleCharacter>
                {
                    new BattleCharacter("caocao_story", "char_caocao", 4, true, "jianxiong"),
                    new BattleCharacter("sunjian", "char_sunjian", 4, false, "yinghun"),
                    new BattleCharacter("yuanshao_taodong", "char_yuanshao", 4, false, "xueyi")
                },

                // 敌方角色 - 董卓、李傕、郭汜
                enemies = new List<BattleCharacter>
                {
                    new BattleCharacter("dongzhuo", "char_dongzhuo", 6, false, "jiuchiroulin"),
                    new BattleCharacter("lijue", "char_lijue", 4, false, "jielve"),
                    new BattleCharacter("guosi", "char_guosi", 4, false, "xiongbao")
                },

                // 胜利条件：董卓HP=0（触发逃走事件）
                victoryCondition = new VictoryCondition
                {
                    type = VictoryType.DefeatTarget,
                    targetCharacterId = "dongzhuo"
                },

                // 失败条件：曹操阵亡
                defeatCondition = new DefeatCondition
                {
                    type = DefeatType.PlayerDeath
                },

                // 特殊规则
                specialRules = new List<SpecialRule>
                {
                    // 西凉铁骑：敌方所有角色攻击距离+1
                    new SpecialRule("xiliang_cavalry", "rule_xiliang_cavalry", RuleType.AttackRangeBonus)
                    { targetId = "enemies" },
                    // 挟天子西遁：董卓HP=0时不会死亡，而是触发逃走事件
                    new SpecialRule("emperor_escape", "rule_emperor_escape", RuleType.EscapeOnDeath)
                    { targetId = "dongzhuo" }
                },

                // 局内事件
                events = new List<BattleEvent>
                {
                    // 战斗开场
                    new BattleEvent(EventTrigger.OnBattleStart, "",
                        new Dialogue("char_dongzhuo", "dialogue_taodong4_dongzhuo_start")),

                    // 孙坚造成伤害
                    new BattleEvent(EventTrigger.OnDamageDone, "sunjian",
                        new Dialogue("char_sunjian", "dialogue_taodong4_sunjian_damage")),

                    // 董卓体力≤2
                    new BattleEvent(EventTrigger.OnHealthLow, "dongzhuo",
                        new Dialogue("char_dongzhuo", "dialogue_taodong4_dongzhuo_lowHP")),

                    // 董卓HP=0触发逃走
                    new BattleEvent(EventTrigger.OnDeath, "dongzhuo",
                        new Dialogue("char_dongzhuo", "dialogue_taodong4_dongzhuo_escape"))
                },

                // 开场对白
                openingDialogue = new List<Dialogue>
                {
                    Dialogue.Narration("dialogue_taodong4_opening_narration"),
                    new Dialogue("char_caocao", "dialogue_taodong4_opening_caocao"),
                    new Dialogue("char_sunjian", "dialogue_taodong4_opening_sunjian"),
                    new Dialogue("char_yuanshao", "dialogue_taodong4_opening_yuanshao"),
                    new Dialogue("char_dongzhuo", "dialogue_taodong4_opening_dongzhuo")
                },

                // 胜利对白
                victoryDialogue = new List<Dialogue>
                {
                    Dialogue.Narration("dialogue_taodong4_victory_narration"),
                    new Dialogue("char_caocao", "dialogue_taodong4_victory_caocao"),
                    new Dialogue("char_sunjian", "dialogue_taodong4_victory_sunjian")
                },

                // 最终结局对白
                endingDialogue = new List<Dialogue>
                {
                    Dialogue.Narration("dialogue_taodong_ending")
                }
            };

            return battle;
        }

        #endregion
    }
}
