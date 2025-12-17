using System.Collections.Generic;

namespace ThreeKingdoms.Story.Campaigns
{
    /// <summary>
    /// 官渡之战战役数据
    /// 《北方争霸·官渡风云》- 共4场战斗
    /// </summary>
    public static class GuanduCampaignData
    {
        public static CampaignData CreateCampaign()
        {
            var storyBattleList = new List<StoryBattle>
            {
                CreateBattle1_YuanCampDecision(),
                CreateBattle2_DefendGuandu(),
                CreateBattle3_XuyouStrategy(),
                CreateBattle4_BurnWuchao()
            };

            var campaign = new CampaignData
            {
                campaignId = "guandu",
                nameKey = "campaign_guandu",
                descriptionKey = "campaign_guandu_desc",
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

        #region 第一战：袁营决策

        /// <summary>
        /// 袁营决策 - 袁绍视角
        /// 兵强马壮，暗藏隐患
        /// </summary>
        private static StoryBattle CreateBattle1_YuanCampDecision()
        {
            var battle = new StoryBattle
            {
                battleId = "guandu_1",
                nameKey = "battle_guandu_1",
                subtitleKey = "battle_guandu_1_subtitle",
                descriptionKey = "battle_guandu_1_desc",
                briefingKey = "battle_guandu_1_briefing",
                difficulty = 2,

                // 我方角色 - 袁绍阵营
                allies = new List<BattleCharacter>
                {
                    new BattleCharacter("yuanshao_story", "char_yuanshao", 4, true, "xueyi"),
                    new BattleCharacter("tianfeng", "char_tianfeng", 3, false, "sijian"),
                    new BattleCharacter("jushou", "char_jushou", 3, false, "jianying")
                },

                // 敌方角色 - 内部隐患（概念敌人）
                enemies = new List<BattleCharacter>
                {
                    new BattleCharacter("morale_problem", "char_morale_problem", 4, false, "dongyao"),
                    new BattleCharacter("supply_problem", "char_supply_problem", 3, false, "xiaohao")
                },

                // 胜利条件：击败两个隐患
                victoryCondition = new VictoryCondition
                {
                    type = VictoryType.DefeatAllEnemies
                },

                // 失败条件：袁绍死亡
                defeatCondition = new DefeatCondition
                {
                    type = DefeatType.PlayerDeath
                },

                // 特殊规则
                specialRules = new List<SpecialRule>
                {
                    // 内部不和：每回合30%概率弃牌
                    new SpecialRule("internal_strife", "rule_internal_strife", RuleType.RandomDiscard),
                    // 强势主公：首次伤害防止
                    new SpecialRule("strong_lord", "rule_strong_lord", RuleType.FirstDamagePrevention),
                    // 四世三公：初始手牌+1
                    new SpecialRule("noble_family", "rule_noble_family", RuleType.ModifyInitialCards, 0, 1)
                    { targetId = "yuanshao_story" }
                },

                // 局内事件
                events = new List<BattleEvent>
                {
                    // 田丰发动死谏
                    new BattleEvent(EventTrigger.OnSkillActivate, "sijian",
                        new Dialogue("char_tianfeng", "dialogue_guandu1_tianfeng_warn")),

                    // 袁绍首次受伤被防止
                    new BattleEvent(EventTrigger.OnDamageTaken, "袁绍",
                        new Dialogue("char_yuanshao", "dialogue_guandu1_yuanshao_confident")),

                    // 触发内部不和弃牌
                    new BattleEvent(EventTrigger.OnCardPlayed, "discard",
                        new Dialogue("char_shenpei", "dialogue_guandu1_shenpei_argue")),

                    // 敌方角色被击败
                    new BattleEvent(EventTrigger.OnDeath, "morale_problem",
                        new Dialogue("char_jushou", "dialogue_guandu1_jushou_stable"))
                },

                // 开场对白
                openingDialogue = new List<Dialogue>
                {
                    new Dialogue("char_yuanshao", "dialogue_guandu1_opening_yuanshao")
                },

                // 胜利对白
                victoryDialogue = new List<Dialogue>
                {
                    new Dialogue("char_yuanshao", "dialogue_guandu1_victory_yuanshao"),
                    Dialogue.Narration("dialogue_guandu1_victory_narration")
                }
            };

            return battle;
        }

        #endregion

        #region 第二战：坚守官渡

        /// <summary>
        /// 坚守官渡 - 曹操视角
        /// 以少敌多
        /// </summary>
        private static StoryBattle CreateBattle2_DefendGuandu()
        {
            var battle = new StoryBattle
            {
                battleId = "guandu_2",
                nameKey = "battle_guandu_2",
                subtitleKey = "battle_guandu_2_subtitle",
                descriptionKey = "battle_guandu_2_desc",
                briefingKey = "battle_guandu_2_briefing",
                difficulty = 3,
                turnLimit = 8, // 存活8回合

                // 我方角色 - 曹操阵营
                allies = new List<BattleCharacter>
                {
                    new BattleCharacter("caocao_story", "char_caocao", 4, true, "jianxiong"),
                    new BattleCharacter("xunyu", "char_xunyu", 3, false, "quhu"),
                    new BattleCharacter("guojia", "char_guojia", 3, false, "tiandu")
                },

                // 敌方角色 - 袁绍阵营
                enemies = new List<BattleCharacter>
                {
                    new BattleCharacter("yuanshao_story", "char_yuanshao", 5, false, "xueyi"),
                    new BattleCharacter("yanliang", "char_yanliang", 4, false, "shuangxiong")
                },

                // 胜利条件：击败袁绍或存活8回合
                victoryCondition = new VictoryCondition
                {
                    type = VictoryType.SurviveTurns,
                    targetTurn = 8
                },

                // 失败条件：曹操死亡
                defeatCondition = new DefeatCondition
                {
                    type = DefeatType.PlayerDeath
                },

                // 特殊规则
                specialRules = new List<SpecialRule>
                {
                    // 官渡要塞：首次两次伤害各-1
                    new SpecialRule("guandu_fort", "rule_guandu_fort", RuleType.DamageReduction),
                    // 兵力差距：袁军初始手牌+1
                    new SpecialRule("troop_advantage", "rule_troop_advantage", RuleType.ModifyInitialCards, 0, 1)
                    { targetId = "enemies" },
                    // 以逸待劳：曹操未造成伤害时摸1牌
                    new SpecialRule("wait_enemy", "rule_wait_enemy", RuleType.DrawOnNoDamage)
                },

                // 局内事件
                events = new List<BattleEvent>
                {
                    // 荀彧出牌时
                    new BattleEvent(EventTrigger.OnTurnStart, "荀彧",
                        new Dialogue("char_xunyu", "dialogue_guandu2_xunyu_advice")),

                    // 郭嘉发动天妒
                    new BattleEvent(EventTrigger.OnSkillActivate, "tiandu",
                        new Dialogue("char_guojia", "dialogue_guandu2_guojia_predict")),

                    // 第4回合开始
                    new BattleEvent(EventTrigger.OnRoundStart, "4",
                        Dialogue.Narration("dialogue_guandu2_round4_stalemate")),

                    // 触发官渡要塞减伤
                    new BattleEvent(EventTrigger.OnDamageTaken, "曹操",
                        new Dialogue("char_caocao", "dialogue_guandu2_caocao_gamble")),

                    // 颜良被击败
                    new BattleEvent(EventTrigger.OnDeath, "yanliang",
                        new Dialogue("char_yuanshao", "dialogue_guandu2_yuanshao_angry"))
                },

                // 开场对白
                openingDialogue = new List<Dialogue>
                {
                    new Dialogue("char_caocao", "dialogue_guandu2_opening_caocao")
                },

                // 胜利对白
                victoryDialogue = new List<Dialogue>
                {
                    new Dialogue("char_caocao", "dialogue_guandu2_victory_caocao"),
                    new Dialogue("char_xunyu", "dialogue_guandu2_victory_xunyu")
                }
            };

            return battle;
        }

        #endregion

        #region 第三战：许攸献策

        /// <summary>
        /// 许攸献策 - 曹操视角
        /// 乌巢，胜负手
        /// </summary>
        private static StoryBattle CreateBattle3_XuyouStrategy()
        {
            var battle = new StoryBattle
            {
                battleId = "guandu_3",
                nameKey = "battle_guandu_3",
                subtitleKey = "battle_guandu_3_subtitle",
                descriptionKey = "battle_guandu_3_desc",
                briefingKey = "battle_guandu_3_briefing",
                difficulty = 2,

                // 我方角色
                allies = new List<BattleCharacter>
                {
                    new BattleCharacter("caocao_story", "char_caocao", 4, true, "jianxiong"),
                    new BattleCharacter("xuyou", "char_xuyou", 3, false, "xiance")
                },

                // 敌方角色
                enemies = new List<BattleCharacter>
                {
                    new BattleCharacter("yuanshao_story", "char_yuanshao", 4, false, "xueyi"),
                    new BattleCharacter("shenpei", "char_shenpei", 3, false, "caiji")
                },

                // 胜利条件：击败袁绍
                victoryCondition = new VictoryCondition
                {
                    type = VictoryType.DefeatTarget,
                    targetCharacterId = "yuanshao_story"
                },

                // 失败条件：曹操死亡
                defeatCondition = new DefeatCondition
                {
                    type = DefeatType.PlayerDeath
                },

                // 特殊规则
                specialRules = new List<SpecialRule>
                {
                    // 献策乌巢：火攻/火杀额外结算
                    new SpecialRule("wuchao_intel", "rule_wuchao_intel", RuleType.FireDamageBonus, 0, 1),
                    // 袁营猜忌：袁绍每回合弃牌
                    new SpecialRule("yuan_suspicion", "rule_yuan_suspicion", RuleType.ForcedDiscard),
                    // 赤足相迎：许攸首次献策曹操额外摸牌
                    new SpecialRule("barefoot_welcome", "rule_barefoot_welcome", RuleType.BonusDraw)
                },

                // 局内事件
                events = new List<BattleEvent>
                {
                    // 许攸发动献策
                    new BattleEvent(EventTrigger.OnSkillActivate, "xiance",
                        new Dialogue("char_xuyou", "dialogue_guandu3_xuyou_wuchao")),

                    // 审配发动猜忌
                    new BattleEvent(EventTrigger.OnSkillActivate, "caiji",
                        new Dialogue("char_shenpei", "dialogue_guandu3_shenpei_suspect")),

                    // 袁绍触发猜忌弃牌
                    new BattleEvent(EventTrigger.OnTurnEnd, "袁绍",
                        new Dialogue("char_yuanshao", "dialogue_guandu3_yuanshao_dismiss"))
                },

                // 开场对白
                openingDialogue = new List<Dialogue>
                {
                    new Dialogue("char_xuyou", "dialogue_guandu3_opening_xuyou"),
                    new Dialogue("char_caocao", "dialogue_guandu3_opening_caocao")
                },

                // 胜利对白
                victoryDialogue = new List<Dialogue>
                {
                    new Dialogue("char_caocao", "dialogue_guandu3_victory_caocao"),
                    new Dialogue("char_xuyou", "dialogue_guandu3_victory_xuyou")
                }
            };

            return battle;
        }

        #endregion

        #region 第四战：火烧乌巢

        /// <summary>
        /// 火烧乌巢 - 曹操视角
        /// 天下归曹
        /// </summary>
        private static StoryBattle CreateBattle4_BurnWuchao()
        {
            var battle = new StoryBattle
            {
                battleId = "guandu_4",
                nameKey = "battle_guandu_4",
                subtitleKey = "battle_guandu_4_subtitle",
                descriptionKey = "battle_guandu_4_desc",
                briefingKey = "battle_guandu_4_briefing",
                difficulty = 4,

                // 我方角色
                allies = new List<BattleCharacter>
                {
                    new BattleCharacter("caocao_story", "char_caocao", 4, true, "jianxiong"),
                    new BattleCharacter("caohong", "char_caohong", 4, false, "yuanhu"),
                    new BattleCharacter("zhanghe", "char_zhanghe", 4, false, "qiaobian")
                },

                // 敌方角色
                enemies = new List<BattleCharacter>
                {
                    new BattleCharacter("chunyuqiong", "char_chunyuqiong", 4, false, "shijiu"),
                    new BattleCharacter("yuanjun_guard1", "char_yuanjun_guard", 3, false, "shouliang"),
                    new BattleCharacter("yuanjun_guard2", "char_yuanjun_guard", 3, false, "shouliang")
                },

                // 胜利条件：击败淳于琼
                victoryCondition = new VictoryCondition
                {
                    type = VictoryType.DefeatTarget,
                    targetCharacterId = "chunyuqiong"
                },

                // 失败条件：曹操死亡
                defeatCondition = new DefeatCondition
                {
                    type = DefeatType.PlayerDeath
                },

                // 特殊规则
                specialRules = new List<SpecialRule>
                {
                    // 夜袭：敌方初始手牌-1
                    new SpecialRule("night_raid", "rule_night_raid", RuleType.ModifyInitialCards, 0, -1)
                    { targetId = "enemies" },
                    // 粮草焚毁：第2回合开始敌方每回合失去1体力
                    new SpecialRule("burn_supplies", "rule_burn_supplies", RuleType.DamageOverTime, 2),
                    // 背水一战：曹操手牌<=2时杀伤害+1
                    new SpecialRule("desperate_fight", "rule_desperate_fight", RuleType.LowHandDamageBonus),
                    // 降将助战：张郃首次伤害+1
                    new SpecialRule("defector_help", "rule_defector_help", RuleType.FirstDamageBonus)
                    { targetId = "zhanghe" }
                },

                // 局内事件
                events = new List<BattleEvent>
                {
                    // 曹洪发言
                    new BattleEvent(EventTrigger.OnBattleStart, "",
                        new Dialogue("char_caohong", "dialogue_guandu4_caohong_loyal")),

                    // 第2回合粮草焚毁
                    new BattleEvent(EventTrigger.OnRoundStart, "2",
                        Dialogue.Narration("dialogue_guandu4_round2_fire")),

                    // 淳于琼使用酒
                    new BattleEvent(EventTrigger.OnCardPlayed, "酒",
                        new Dialogue("char_chunyuqiong", "dialogue_guandu4_chunyuqiong_drunk")),

                    // 张郃首次造成伤害
                    new BattleEvent(EventTrigger.OnDamageTaken, "",
                        new Dialogue("char_zhanghe", "dialogue_guandu4_zhanghe_defect")),

                    // 淳于琼被击败
                    new BattleEvent(EventTrigger.OnDeath, "chunyuqiong",
                        new Dialogue("char_chunyuqiong", "dialogue_guandu4_chunyuqiong_defeat"))
                },

                // 开场对白
                openingDialogue = new List<Dialogue>
                {
                    new Dialogue("char_caocao", "dialogue_guandu4_opening_caocao")
                },

                // 胜利对白
                victoryDialogue = new List<Dialogue>
                {
                    Dialogue.Narration("dialogue_guandu4_victory_narration"),
                    new Dialogue("char_yuanshao", "dialogue_guandu4_yuanshao_despair"),
                    new Dialogue("char_caocao", "dialogue_guandu4_victory_caocao")
                }
            };

            return battle;
        }

        #endregion
    }
}
