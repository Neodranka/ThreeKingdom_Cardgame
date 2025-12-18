using System.Collections.Generic;

namespace ThreeKingdoms.Story.Campaigns
{
    /// <summary>
    /// 官渡之战战役数据 v2
    /// 《北方争霸·以少胜多》- 共4场战斗
    /// </summary>
    public static class GuanduCampaignData
    {
        public static CampaignData CreateCampaign()
        {
            var storyBattleList = new List<StoryBattle>
            {
                CreateBattle1_SlayYanliang(),    // 兵临官渡 - 关羽斩颜良
                CreateBattle2_DefendGuandu(),    // 坚守官渡 - 关羽斩文丑
                CreateBattle3_NightRaidWuchao(), // 夜袭乌巢 - 曹操夜袭
                CreateBattle4_YuanArmyCollapse() // 袁军溃败 - 曹操击败袁绍
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

        #region 第一战：兵临官渡

        /// <summary>
        /// 兵临官渡 - 关羽斩颜良
        /// 「十胜十败」
        /// </summary>
        private static StoryBattle CreateBattle1_SlayYanliang()
        {
            var battle = new StoryBattle
            {
                battleId = "guandu_1",
                nameKey = "battle_guandu_1",
                subtitleKey = "battle_guandu_1_subtitle",
                descriptionKey = "battle_guandu_1_desc",
                briefingKey = "battle_guandu_1_briefing",
                difficulty = 2,

                // 我方角色 - 关羽
                allies = new List<BattleCharacter>
                {
                    new BattleCharacter("guanyu_story", "char_guanyu", 4, true, "wusheng")
                },

                // 敌方角色 - 颜良 + 袁军骑将x2
                enemies = new List<BattleCharacter>
                {
                    new BattleCharacter("yanliang", "char_yanliang", 4, false, "weiwu"),
                    new BattleCharacter("yuanjun_cavalry_1", "char_yuanjun_cavalry", 2, false),
                    new BattleCharacter("yuanjun_cavalry_2", "char_yuanjun_cavalry", 2, false)
                },

                // 胜利条件：击杀颜良
                victoryCondition = new VictoryCondition
                {
                    type = VictoryType.DefeatTarget,
                    targetCharacterId = "yanliang"
                },

                // 失败条件：关羽阵亡
                defeatCondition = new DefeatCondition
                {
                    type = DefeatType.PlayerDeath
                },

                // 特殊规则
                specialRules = new List<SpecialRule>
                {
                    // 士气大振：击败颜良后，后续战斗体力上限+1
                    new SpecialRule("morale_boost", "rule_morale_boost", RuleType.Custom)
                },

                // 局内事件
                events = new List<BattleEvent>
                {
                    // 战斗开场
                    new BattleEvent(EventTrigger.OnBattleStart, "",
                        new Dialogue("char_yanliang", "dialogue_guandu1_yanliang_challenge")),
                    new BattleEvent(EventTrigger.OnBattleStart, "",
                        new Dialogue("char_guanyu", "dialogue_guandu1_guanyu_answer")),

                    // 关羽使用杀
                    new BattleEvent(EventTrigger.OnCardPlayed, "杀",
                        new Dialogue("char_guanyu", "dialogue_guandu1_guanyu_slash")),

                    // 颜良体力≤2
                    new BattleEvent(EventTrigger.OnHealthLow, "yanliang",
                        new Dialogue("char_yanliang", "dialogue_guandu1_yanliang_lowHP")),

                    // 颜良被击败
                    new BattleEvent(EventTrigger.OnDeath, "yanliang",
                        new Dialogue("char_guanyu", "dialogue_guandu1_guanyu_victory"))
                },

                // 开场对白
                openingDialogue = new List<Dialogue>
                {
                    new Dialogue("char_chengyu", "dialogue_guandu1_opening_chengyu"),
                    new Dialogue("char_caocao", "dialogue_guandu1_opening_caocao1"),
                    new Dialogue("char_guojia", "dialogue_guandu1_opening_guojia1"),
                    new Dialogue("char_caocao", "dialogue_guandu1_opening_caocao2"),
                    new Dialogue("char_guojia", "dialogue_guandu1_opening_guojia2"),
                    new Dialogue("char_guojia", "dialogue_guandu1_opening_guojia3"),
                    new Dialogue("char_guojia", "dialogue_guandu1_opening_guojia4"),
                    new Dialogue("char_caocao", "dialogue_guandu1_opening_caocao3"),
                    new Dialogue("char_soldier", "dialogue_guandu1_opening_soldier"),
                    new Dialogue("char_guanyu", "dialogue_guandu1_opening_guanyu"),
                    new Dialogue("char_caocao", "dialogue_guandu1_opening_caocao4")
                },

                // 胜利对白
                victoryDialogue = new List<Dialogue>
                {
                    new Dialogue("char_guanyu", "dialogue_guandu1_victory_guanyu"),
                    new Dialogue("char_caocao", "dialogue_guandu1_victory_caocao"),
                    Dialogue.Narration("dialogue_guandu1_victory_narration")
                }
            };

            return battle;
        }

        #endregion

        #region 第二战：坚守官渡

        /// <summary>
        /// 坚守官渡 - 关羽斩文丑
        /// 「死守不退」
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

                // 我方角色 - 关羽、夏侯惇、张辽
                allies = new List<BattleCharacter>
                {
                    new BattleCharacter("guanyu_story", "char_guanyu", 4, true, "wusheng"),
                    new BattleCharacter("xiahoudun", "char_xiahoudun", 4, false, "ganglie"),
                    new BattleCharacter("zhangliao", "char_zhangliao", 4, false, "tuxi")
                },

                // 敌方角色 - 文丑、张郃、高览
                enemies = new List<BattleCharacter>
                {
                    new BattleCharacter("wenchou", "char_wenchou", 4, false, "qiangjian"),
                    new BattleCharacter("zhanghe", "char_zhanghe", 4, false, "duobian"),
                    new BattleCharacter("gaolan", "char_gaolan", 4, false, "mashu")
                },

                // 胜利条件：击杀文丑
                victoryCondition = new VictoryCondition
                {
                    type = VictoryType.DefeatTarget,
                    targetCharacterId = "wenchou"
                },

                // 失败条件：关羽阵亡
                defeatCondition = new DefeatCondition
                {
                    type = DefeatType.PlayerDeath
                },

                // 特殊规则
                specialRules = new List<SpecialRule>
                {
                    // 士气低迷：我方受到伤害30%概率+1
                    new SpecialRule("low_morale", "rule_low_morale", RuleType.DamageIncrease),
                    // 复仇之战：文丑对关羽使用杀或决斗时伤害+1
                    new SpecialRule("revenge_battle", "rule_revenge_battle", RuleType.DamageBonus)
                    { targetId = "wenchou", extraInfo = "guanyu_story" }
                },

                // 局内事件
                events = new List<BattleEvent>
                {
                    // 文丑首次对关羽造成伤害
                    new BattleEvent(EventTrigger.OnDamageTaken, "guanyu_story",
                        new Dialogue("char_wenchou", "dialogue_guandu2_wenchou_revenge")),

                    // 张辽发动突袭
                    new BattleEvent(EventTrigger.OnSkillActivate, "tuxi",
                        new Dialogue("char_zhangliao", "dialogue_guandu2_zhangliao_tuxi")),

                    // 夏侯惇发动刚烈
                    new BattleEvent(EventTrigger.OnSkillActivate, "ganglie",
                        new Dialogue("char_xiahoudun", "dialogue_guandu2_xiahoudun_ganglie")),

                    // 文丑体力≤2
                    new BattleEvent(EventTrigger.OnHealthLow, "wenchou",
                        new Dialogue("char_wenchou", "dialogue_guandu2_wenchou_lowHP")),

                    // 文丑被击败
                    new BattleEvent(EventTrigger.OnDeath, "wenchou",
                        new Dialogue("char_guanyu", "dialogue_guandu2_guanyu_victory"))
                },

                // 开场对白
                openingDialogue = new List<Dialogue>
                {
                    Dialogue.Narration("dialogue_guandu2_opening_narration"),
                    new Dialogue("char_caocao", "dialogue_guandu2_opening_caocao1"),
                    new Dialogue("char_chengyu", "dialogue_guandu2_opening_chengyu"),
                    new Dialogue("char_caocao", "dialogue_guandu2_opening_caocao2"),
                    new Dialogue("char_xunyu", "dialogue_guandu2_opening_xunyu1"),
                    new Dialogue("char_xunyu", "dialogue_guandu2_opening_xunyu2"),
                    new Dialogue("char_xunyu", "dialogue_guandu2_opening_xunyu3"),
                    new Dialogue("char_soldier", "dialogue_guandu2_opening_soldier"),
                    new Dialogue("char_caocao", "dialogue_guandu2_opening_caocao3"),
                    new Dialogue("char_guanyu", "dialogue_guandu2_opening_guanyu"),
                    new Dialogue("char_wenchou", "dialogue_guandu2_opening_wenchou"),
                    new Dialogue("char_guanyu", "dialogue_guandu2_opening_guanyu2")
                },

                // 胜利对白
                victoryDialogue = new List<Dialogue>
                {
                    new Dialogue("char_guanyu", "dialogue_guandu2_victory_guanyu"),
                    new Dialogue("char_caocao", "dialogue_guandu2_victory_caocao")
                }
            };

            return battle;
        }

        #endregion

        #region 第三战：夜袭乌巢

        /// <summary>
        /// 夜袭乌巢 - 曹操夜袭粮仓
        /// 「孤注一掷」
        /// </summary>
        private static StoryBattle CreateBattle3_NightRaidWuchao()
        {
            var battle = new StoryBattle
            {
                battleId = "guandu_3",
                nameKey = "battle_guandu_3",
                subtitleKey = "battle_guandu_3_subtitle",
                descriptionKey = "battle_guandu_3_desc",
                briefingKey = "battle_guandu_3_briefing",
                difficulty = 3,

                // 我方角色 - 曹操、徐晃、张辽
                allies = new List<BattleCharacter>
                {
                    new BattleCharacter("caocao_story", "char_caocao", 4, true, "jianxiong"),
                    new BattleCharacter("xuhuang", "char_xuhuang", 4, false, "duanliang"),
                    new BattleCharacter("zhangliao", "char_zhangliao", 4, false, "tuxi")
                },

                // 敌方角色 - 淳于琼 + 袁军骑将x2
                enemies = new List<BattleCharacter>
                {
                    new BattleCharacter("chunyuqiong", "char_chunyuqiong", 4, false, "yingming"),
                    new BattleCharacter("yuanjun_cavalry_mashu_1", "char_yuanjun_cavalry", 4, false, "mashu"),
                    new BattleCharacter("yuanjun_cavalry_mashu_2", "char_yuanjun_cavalry", 4, false, "mashu")
                },

                // 胜利条件：击杀淳于琼
                victoryCondition = new VictoryCondition
                {
                    type = VictoryType.DefeatTarget,
                    targetCharacterId = "chunyuqiong"
                },

                // 失败条件：曹操阵亡
                defeatCondition = new DefeatCondition
                {
                    type = DefeatType.PlayerDeath
                },

                // 特殊规则
                specialRules = new List<SpecialRule>
                {
                    // 夜袭：敌方所有角色起始手牌-1
                    new SpecialRule("night_raid", "rule_night_raid", RuleType.ModifyInitialCards, 0, -1)
                    { targetId = "enemies" },
                    // 纵火焚粮：从第2回合开始，敌方每回合结束时全体失去1点体力
                    new SpecialRule("burn_supplies", "rule_burn_supplies", RuleType.DamageOverTime, 2),
                    // 背水一战：曹操手牌数≤2时，使用杀伤害+1
                    new SpecialRule("desperate_fight", "rule_desperate_fight", RuleType.LowHandDamageBonus)
                    { targetId = "caocao_story" }
                },

                // 局内事件
                events = new List<BattleEvent>
                {
                    // 战斗开场
                    new BattleEvent(EventTrigger.OnBattleStart, "",
                        new Dialogue("char_caocao", "dialogue_guandu3_caocao_start")),

                    // 第2回合纵火焚粮触发
                    new BattleEvent(EventTrigger.OnRoundStart, "2",
                        Dialogue.Narration("dialogue_guandu3_burn_supplies")),

                    // 徐晃发动断粮
                    new BattleEvent(EventTrigger.OnSkillActivate, "duanliang",
                        new Dialogue("char_xuhuang", "dialogue_guandu3_xuhuang_duanliang")),

                    // 淳于琼体力≤2
                    new BattleEvent(EventTrigger.OnHealthLow, "chunyuqiong",
                        new Dialogue("char_chunyuqiong", "dialogue_guandu3_chunyuqiong_lowHP")),

                    // 淳于琼被击败
                    new BattleEvent(EventTrigger.OnDeath, "chunyuqiong",
                        new Dialogue("char_chunyuqiong", "dialogue_guandu3_chunyuqiong_defeat"))
                },

                // 开场对白
                openingDialogue = new List<Dialogue>
                {
                    Dialogue.Narration("dialogue_guandu3_opening_narration1"),
                    new Dialogue("char_xuyou", "dialogue_guandu3_opening_xuyou1"),
                    new Dialogue("char_caocao", "dialogue_guandu3_opening_caocao1"),
                    new Dialogue("char_xuyou", "dialogue_guandu3_opening_xuyou2"),
                    new Dialogue("char_caocao", "dialogue_guandu3_opening_caocao2"),
                    new Dialogue("char_xuyou", "dialogue_guandu3_opening_xuyou3"),
                    new Dialogue("char_xuyou", "dialogue_guandu3_opening_xuyou4"),
                    new Dialogue("char_zhangliao", "dialogue_guandu3_opening_zhangliao"),
                    new Dialogue("char_xuhuang", "dialogue_guandu3_opening_xuhuang"),
                    new Dialogue("char_caocao", "dialogue_guandu3_opening_caocao3"),
                    Dialogue.Narration("dialogue_guandu3_opening_narration2"),
                    new Dialogue("char_caocao", "dialogue_guandu3_opening_caocao4"),
                    new Dialogue("char_caocao", "dialogue_guandu3_opening_caocao5")
                },

                // 胜利对白
                victoryDialogue = new List<Dialogue>
                {
                    Dialogue.Narration("dialogue_guandu3_victory_narration"),
                    new Dialogue("char_soldier", "dialogue_guandu3_victory_soldier"),
                    new Dialogue("char_caocao", "dialogue_guandu3_victory_caocao"),
                    new Dialogue("char_xuyou", "dialogue_guandu3_victory_xuyou")
                }
            };

            return battle;
        }

        #endregion

        #region 第四战：袁军溃败

        /// <summary>
        /// 袁军溃败 - 曹操击败袁绍
        /// 「定鼎北方」
        /// </summary>
        private static StoryBattle CreateBattle4_YuanArmyCollapse()
        {
            var battle = new StoryBattle
            {
                battleId = "guandu_4",
                nameKey = "battle_guandu_4",
                subtitleKey = "battle_guandu_4_subtitle",
                descriptionKey = "battle_guandu_4_desc",
                briefingKey = "battle_guandu_4_briefing",
                difficulty = 2,

                // 我方角色 - 曹操、曹军步兵x2
                allies = new List<BattleCharacter>
                {
                    new BattleCharacter("caocao_story", "char_caocao", 4, true, "jianxiong"),
                    new BattleCharacter("caojun_infantry_1", "char_caojun_infantry", 3, false),
                    new BattleCharacter("caojun_infantry_2", "char_caojun_infantry", 3, false)
                },

                // 敌方角色 - 袁绍 + 袁军骑将x2
                enemies = new List<BattleCharacter>
                {
                    new BattleCharacter("yuanshao", "char_yuanshao", 4, false, "qiji"),
                    new BattleCharacter("yuanjun_cavalry_mashu_3", "char_yuanjun_cavalry", 4, false, "mashu"),
                    new BattleCharacter("yuanjun_cavalry_mashu_4", "char_yuanjun_cavalry", 4, false, "mashu")
                },

                // 胜利条件：击败袁绍
                victoryCondition = new VictoryCondition
                {
                    type = VictoryType.DefeatTarget,
                    targetCharacterId = "yuanshao"
                },

                // 失败条件：曹操阵亡
                defeatCondition = new DefeatCondition
                {
                    type = DefeatType.PlayerDeath
                },

                // 特殊规则
                specialRules = new List<SpecialRule>
                {
                    // 军心崩溃：敌方所有角色手牌上限-1
                    new SpecialRule("army_collapse", "rule_army_collapse", RuleType.ReduceHandLimit)
                    { targetId = "enemies" },
                    // 饥疲交迫：敌方每回合结束时有50%概率失去1点体力
                    new SpecialRule("hungry_tired", "rule_hungry_tired", RuleType.RandomDamage),
                    // 乘胜追击：曹操使用杀造成伤害后，可摸1张牌
                    new SpecialRule("victory_pursuit", "rule_victory_pursuit", RuleType.DrawOnDamage)
                    { targetId = "caocao_story" }
                },

                // 局内事件
                events = new List<BattleEvent>
                {
                    // 战斗开场
                    new BattleEvent(EventTrigger.OnBattleStart, "",
                        new Dialogue("char_caocao", "dialogue_guandu4_caocao_start")),

                    // 袁绍发动齐击
                    new BattleEvent(EventTrigger.OnSkillActivate, "qiji",
                        new Dialogue("char_yuanshao", "dialogue_guandu4_yuanshao_qiji")),

                    // 触发饥疲交迫
                    new BattleEvent(EventTrigger.OnTurnEnd, "enemies",
                        Dialogue.Narration("dialogue_guandu4_hungry_tired")),

                    // 袁绍体力≤2
                    new BattleEvent(EventTrigger.OnHealthLow, "yuanshao",
                        new Dialogue("char_yuanshao", "dialogue_guandu4_yuanshao_lowHP")),

                    // 袁绍被击败
                    new BattleEvent(EventTrigger.OnDeath, "yuanshao",
                        new Dialogue("char_yuanshao", "dialogue_guandu4_yuanshao_defeat"))
                },

                // 开场对白
                openingDialogue = new List<Dialogue>
                {
                    Dialogue.Narration("dialogue_guandu4_opening_narration1"),
                    Dialogue.Narration("dialogue_guandu4_opening_narration2"),
                    new Dialogue("char_soldier", "dialogue_guandu4_opening_soldier1"),
                    new Dialogue("char_soldier", "dialogue_guandu4_opening_soldier2"),
                    Dialogue.Narration("dialogue_guandu4_opening_narration3"),
                    new Dialogue("char_caocao", "dialogue_guandu4_opening_caocao")
                },

                // 胜利对白
                victoryDialogue = new List<Dialogue>
                {
                    Dialogue.Narration("dialogue_guandu4_victory_narration"),
                    new Dialogue("char_caocao", "dialogue_guandu4_victory_caocao")
                },

                // 最终结局对白
                endingDialogue = new List<Dialogue>
                {
                    Dialogue.Narration("dialogue_guandu_ending")
                }
            };

            return battle;
        }

        #endregion
    }
}
