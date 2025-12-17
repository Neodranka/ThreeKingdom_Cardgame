using System.Collections.Generic;

namespace ThreeKingdoms.Story.Campaigns
{
    /// <summary>
    /// 赤壁之战战役数据
    /// 《赤壁·火起东南》- 共6场战斗
    /// </summary>
    public static class ChiBiCampaignData
    {
        public static CampaignData CreateCampaign()
        {
            var storyBattleList = new List<StoryBattle>
            {
                CreateBattle1_ChaisangAlliance(),
                CreateBattle2_ZhugeAcrossRiver(),
                CreateBattle3_ChangbanBreakout(),
                CreateBattle4_ZhangfeiBreaksBridge(),
                CreateBattle5_HuanggaiFakeSurrender(),
                CreateBattle6_RedCliffsFire()
            };

            var campaign = new CampaignData
            {
                campaignId = "chibi",
                nameKey = "campaign_chibi",
                descriptionKey = "campaign_chibi_desc",
                isUnlocked = true,
                storyBattles = storyBattleList,
                // 同时填充battles以兼容旧UI
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

        #region 第一战：柴桑盟议

        private static StoryBattle CreateBattle1_ChaisangAlliance()
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
                    new BattleCharacter("sunquan_story", "char_sunquan", 4, true, "zhiheng"),
                    new BattleCharacter("lusu", "char_lusu", 3, false, "dimeng"),
                    new BattleCharacter("chengpu", "char_chengpu", 4, false, "laodang")
                },

                // 敌方角色
                enemies = new List<BattleCharacter>
                {
                    new BattleCharacter("zhangzhao", "char_zhangzhao", 3, false, "zhuhe")
                },

                // 胜利条件：击败张昭
                victoryCondition = new VictoryCondition
                {
                    type = VictoryType.DefeatTarget,
                    targetCharacterId = "zhangzhao"
                },

                // 失败条件：孙权死亡
                defeatCondition = new DefeatCondition
                {
                    type = DefeatType.PlayerDeath
                },

                // 特殊规则
                specialRules = new List<SpecialRule>
                {
                    new SpecialRule("no_attack", "rule_zhangzhao_no_attack", RuleType.EnemyNoAttack),
                    new SpecialRule("lusu_support", "rule_lusu_support", RuleType.AllyAutoSupport)
                },

                // 局内事件
                events = new List<BattleEvent>
                {
                    // 孙权首次出杀
                    new BattleEvent(EventTrigger.OnFirstKill, "孙权",
                        new Dialogue("char_sunquan", "dialogue_chibi1_sunquan_kill")),

                    // 张昭发动主和
                    new BattleEvent(EventTrigger.OnSkillActivate, "zhuhe",
                        new Dialogue("char_zhangzhao", "dialogue_chibi1_zhangzhao_zhuhe")),

                    // 鲁肃补牌
                    new BattleEvent(EventTrigger.OnSkillActivate, "dimeng",
                        new Dialogue("char_lusu", "dialogue_chibi1_lusu_help")),

                    // 孙权濒死
                    new BattleEvent(EventTrigger.OnNearDeath, "孙权",
                        new Dialogue("char_chengpu", "dialogue_chibi1_chengpu_panic")),

                    // 张昭死亡前
                    new BattleEvent(EventTrigger.OnHPBelow, "张昭",
                        new Dialogue("char_zhangzhao", "dialogue_chibi1_zhangzhao_defeat"))
                    { triggerParam2 = "1" }
                },

                // 开场对白
                openingDialogue = new List<Dialogue>
                {
                    Dialogue.Narration("dialogue_chibi1_opening_1"),
                    new Dialogue("char_zhangzhao", "dialogue_chibi1_opening_zhangzhao"),
                    new Dialogue("char_sunquan", "dialogue_chibi1_opening_sunquan")
                },

                // 胜利对白
                victoryDialogue = new List<Dialogue>
                {
                    new Dialogue("char_sunquan", "dialogue_chibi1_victory_1"),
                    Dialogue.Narration("dialogue_chibi1_victory_2")
                }
            };

            return battle;
        }

        #endregion

        #region 第二战：诸葛渡江

        private static StoryBattle CreateBattle2_ZhugeAcrossRiver()
        {
            var battle = new StoryBattle
            {
                battleId = "chibi_2",
                nameKey = "battle_chibi_2",
                subtitleKey = "battle_chibi_2_subtitle",
                descriptionKey = "battle_chibi_2_desc",
                briefingKey = "battle_chibi_2_briefing",
                difficulty = 2,
                turnLimit = 6, // 存活6回合

                // 我方角色
                allies = new List<BattleCharacter>
                {
                    new BattleCharacter("zhugeliang_story", "char_zhugeliang", 3, true, "guanxing", "kongcheng"),
                    new BattleCharacter("lusu", "char_lusu", 3, false, "dimeng")
                },

                // 敌方角色
                enemies = new List<BattleCharacter>
                {
                    new BattleCharacter("zhouyu_story", "char_zhouyu", 3, false, "yingzi", "fanjian"),
                    new BattleCharacter("lvmeng", "char_lvmeng", 4, false, "keji")
                },

                // 胜利条件：存活6回合
                victoryCondition = new VictoryCondition
                {
                    type = VictoryType.SurviveTurns,
                    targetTurn = 6
                },

                // 失败条件：诸葛亮死亡
                defeatCondition = new DefeatCondition
                {
                    type = DefeatType.PlayerDeath
                },

                // 局内事件
                events = new List<BattleEvent>
                {
                    // 诸葛亮发动观星
                    new BattleEvent(EventTrigger.OnSkillActivate, "guanxing",
                        new Dialogue("char_zhugeliang", "dialogue_chibi2_zhugeliang_guanxing")),

                    // 周瑜首次造成伤害
                    new BattleEvent(EventTrigger.OnDamageTaken, "诸葛亮",
                        new Dialogue("char_zhouyu", "dialogue_chibi2_zhouyu_attack")),

                    // 吕蒙出牌
                    new BattleEvent(EventTrigger.OnTurnStart, "吕蒙",
                        new Dialogue("char_lvmeng", "dialogue_chibi2_lvmeng_warning")),

                    // 第4回合开始
                    new BattleEvent(EventTrigger.OnRoundStart, "4",
                        Dialogue.Narration("dialogue_chibi2_round4_narration"),
                        new Dialogue("char_zhouyu", "dialogue_chibi2_zhouyu_question"),
                        new Dialogue("char_zhugeliang", "dialogue_chibi2_zhugeliang_answer"))
                },

                // 开场对白
                openingDialogue = new List<Dialogue>
                {
                    Dialogue.Narration("dialogue_chibi2_opening_1"),
                    new Dialogue("char_zhouyu", "dialogue_chibi2_opening_zhouyu")
                },

                // 胜利对白
                victoryDialogue = new List<Dialogue>
                {
                    new Dialogue("char_zhouyu", "dialogue_chibi2_victory")
                }
            };

            return battle;
        }

        #endregion

        #region 第三战：长坂坡突围

        private static StoryBattle CreateBattle3_ChangbanBreakout()
        {
            var battle = new StoryBattle
            {
                battleId = "chibi_3",
                nameKey = "battle_chibi_3",
                subtitleKey = "battle_chibi_3_subtitle",
                descriptionKey = "battle_chibi_3_desc",
                briefingKey = "battle_chibi_3_briefing",
                difficulty = 1, // 新手教学

                // 我方角色
                allies = new List<BattleCharacter>
                {
                    new BattleCharacter("zhaoyun_story", "char_zhaoyun", 4, true, "longdan")
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

                // 局内事件
                events = new List<BattleEvent>(),

                // 开场对白
                openingDialogue = new List<Dialogue>
                {
                    new Dialogue("char_mifang", "dialogue_chibi3_opening_mifang"),
                    new Dialogue("char_liubei", "dialogue_chibi3_opening_liubei")
                },

                // 胜利对白
                victoryDialogue = new List<Dialogue>
                {
                    new Dialogue("char_zhangfei", "dialogue_chibi3_victory")
                }
            };

            return battle;
        }

        #endregion

        #region 第四战：张飞断桥

        private static StoryBattle CreateBattle4_ZhangfeiBreaksBridge()
        {
            var battle = new StoryBattle
            {
                battleId = "chibi_4",
                nameKey = "battle_chibi_4",
                subtitleKey = "battle_chibi_4_subtitle",
                descriptionKey = "battle_chibi_4_desc",
                briefingKey = "battle_chibi_4_briefing",
                difficulty = 2,

                // 我方角色
                allies = new List<BattleCharacter>
                {
                    new BattleCharacter("zhangfei_story", "char_zhangfei", 4, true, "paoxiao")
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

                // 局内事件
                events = new List<BattleEvent>
                {
                    // 战斗开始
                    new BattleEvent(EventTrigger.OnBattleStart, "",
                        new Dialogue("char_zhangfei", "dialogue_chibi4_zhangfei_roar")),

                    // 夏侯杰触发胆裂
                    new BattleEvent(EventTrigger.OnSkillActivate, "danlie",
                        Dialogue.Narration("dialogue_chibi4_xiahoujie_fear"))
                },

                // 开场对白
                openingDialogue = new List<Dialogue>
                {
                    Dialogue.Narration("dialogue_chibi4_opening")
                },

                // 胜利对白
                victoryDialogue = new List<Dialogue>
                {
                    new Dialogue("char_caocao", "dialogue_chibi4_caocao_retreat"),
                    new Dialogue("char_zhangfei", "dialogue_chibi4_zhangfei_order"),
                    Dialogue.Narration("dialogue_chibi4_narration_zhaoyun"),
                    new Dialogue("char_liubei", "dialogue_chibi4_liubei_praise")
                }
            };

            return battle;
        }

        #endregion

        #region 第五战：黄盖诈降

        private static StoryBattle CreateBattle5_HuanggaiFakeSurrender()
        {
            var battle = new StoryBattle
            {
                battleId = "chibi_5",
                nameKey = "battle_chibi_5",
                subtitleKey = "battle_chibi_5_subtitle",
                descriptionKey = "battle_chibi_5_desc",
                briefingKey = "battle_chibi_5_briefing",
                difficulty = 2,

                // 我方角色
                allies = new List<BattleCharacter>
                {
                    new BattleCharacter("huanggai_story", "char_huanggai", 4, true, "kurou_zhaxiang"),
                    new BattleCharacter("zhouyu_story", "char_zhouyu", 3, false, "yingzi", "fanjian")
                },

                // 敌方角色
                enemies = new List<BattleCharacter>
                {
                    new BattleCharacter("caocao_story", "char_caocao", 5, false, "jianxiong"),
                    new BattleCharacter("jianggan", "char_jianggan", 3, false, "daoshu")
                },

                // 胜利条件：累积3个诈降标记
                victoryCondition = new VictoryCondition
                {
                    type = VictoryType.AccumulateMarks,
                    targetCount = 3
                },

                // 失败条件：黄盖死亡或蒋干连续3次查看手牌
                defeatCondition = new DefeatCondition
                {
                    type = DefeatType.ExceedCount,
                    maxCount = 3
                },

                // 特殊规则
                specialRules = new List<SpecialRule>
                {
                    new SpecialRule("zhouyu_support", "rule_zhouyu_support", RuleType.AllyAutoSupport)
                },

                // 局内事件
                events = new List<BattleEvent>
                {
                    // 开场对话
                    new BattleEvent(EventTrigger.OnBattleStart, "",
                        new Dialogue("char_huanggai", "dialogue_chibi5_huanggai_request"),
                        new Dialogue("char_zhouyu", "dialogue_chibi5_zhouyu_warning")),

                    // 黄盖首次发动苦肉诈降
                    new BattleEvent(EventTrigger.OnSkillActivate, "kurou_zhaxiang",
                        new Dialogue("char_huanggai", "dialogue_chibi5_huanggai_hit")),

                    // 黄盖血量为1
                    new BattleEvent(EventTrigger.OnHPBelow, "黄盖",
                        new Dialogue("char_zhouyu", "dialogue_chibi5_zhouyu_enough"))
                    { triggerParam2 = "1" },

                    // 曹操获得诈降标记
                    new BattleEvent(EventTrigger.OnMarkerGained, "zhaxiang",
                        new Dialogue("char_caocao", "dialogue_chibi5_caocao_believe")),

                    // 蒋干查看手牌
                    new BattleEvent(EventTrigger.OnSkillActivate, "daoshu",
                        new Dialogue("char_jianggan", "dialogue_chibi5_jianggan_doubt"))
                },

                // 开场对白
                openingDialogue = new List<Dialogue>
                {
                    Dialogue.Narration("dialogue_chibi5_opening")
                },

                // 胜利对白
                victoryDialogue = new List<Dialogue>
                {
                    Dialogue.Narration("dialogue_chibi5_victory_narration"),
                    new Dialogue("char_caocao", "dialogue_chibi5_caocao_accept")
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
                difficulty = 3,

                // 我方角色
                allies = new List<BattleCharacter>
                {
                    new BattleCharacter("liubei_story", "char_liubei", 4, true, "rende"),
                    new BattleCharacter("zhouyu_story", "char_zhouyu", 3, false, "yingzi", "fanjian"),
                    new BattleCharacter("zhugeliang_story", "char_zhugeliang", 3, false, "guanxing", "kongcheng"),
                    new BattleCharacter("huanggai_story", "char_huanggai", 4, false, "kurou"),
                    new BattleCharacter("guanyu_story", "char_guanyu", 4, false, "wusheng")
                },

                // 敌方角色
                enemies = new List<BattleCharacter>
                {
                    new BattleCharacter("caocao_story", "char_caocao", 7, false, "jianxiong"), // 铁索连船+2HP
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
                    type = DefeatType.AllAlliesDeath
                },

                // 特殊规则
                specialRules = new List<SpecialRule>
                {
                    // 连日败退：刘备方初始手牌-1
                    new SpecialRule("retreat_debuff", "rule_retreat_debuff", RuleType.ModifyInitialCards, 0, -1)
                    { targetId = "allies" },

                    // 铁索连船：曹军攻击距离+1
                    new SpecialRule("chain_ships", "rule_chain_ships", RuleType.ModifyAttackRange, 0, 1)
                    { targetId = "enemies" },

                    // 第2回合：密谋成功，火攻伤害+2
                    new SpecialRule("conspiracy", "rule_conspiracy", RuleType.FireDamageBonus, 2, 2),

                    // 第3回合：东风起，火攻伤害再+1（共+3）
                    new SpecialRule("east_wind", "rule_east_wind", RuleType.FireDamageBonus, 3, 3)
                },

                // 局内事件
                events = new List<BattleEvent>
                {
                    // 开场
                    new BattleEvent(EventTrigger.OnBattleStart, "",
                        new Dialogue("char_zhugeliang", "dialogue_chibi6_zhugeliang_wind"),
                        new Dialogue("char_zhouyu", "dialogue_chibi6_zhouyu_fire")),

                    // 第2回合
                    new BattleEvent(EventTrigger.OnRoundStart, "2",
                        Dialogue.Narration("dialogue_chibi6_round2_conspiracy")),

                    // 第3回合
                    new BattleEvent(EventTrigger.OnRoundStart, "3",
                        Dialogue.Narration("dialogue_chibi6_round3_wind")),

                    // 黄盖出火杀
                    new BattleEvent(EventTrigger.OnCardPlayed, "FireKill",
                        new Dialogue("char_huanggai", "dialogue_chibi6_huanggai_fire")),

                    // 曹操首次受到火焰伤害
                    new BattleEvent(EventTrigger.OnDamageTaken, "曹操",
                        new Dialogue("char_caocao", "dialogue_chibi6_caocao_trap")),

                    // 张辽出牌
                    new BattleEvent(EventTrigger.OnTurnStart, "张辽",
                        new Dialogue("char_zhangliao", "dialogue_chibi6_zhangliao_chaos")),

                    // 诸葛亮回合开始
                    new BattleEvent(EventTrigger.OnTurnStart, "诸葛亮",
                        new Dialogue("char_zhugeliang", "dialogue_chibi6_zhugeliang_strong"))
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
                    Dialogue.Narration("dialogue_chibi6_ending")
                }
            };

            return battle;
        }

        #endregion
    }
}
