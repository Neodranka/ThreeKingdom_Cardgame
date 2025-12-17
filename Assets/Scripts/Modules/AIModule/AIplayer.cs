using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ThreeKingdoms.Story;

namespace ThreeKingdoms.AI
{
    /// <summary>
    /// AI玩家控制器
    /// </summary>
    public class AIPlayer : MonoBehaviour
    {
        [Header("AI设置")]
        [Range(0, 3)]
        public int aiLevel = 1; // 0=随机, 1=简单, 2=中等, 3=困难

        [Range(0.5f, 3f)]
        public float thinkingTime = 1.5f; // AI思考时间

        public Player controlledPlayer; // AI控制的玩家

        [Header("行为权重")]
        public float attackWeight = 1.0f;      // 攻击倾向
        public float healWeight = 1.5f;        // 治疗倾向
        public float saveWeight = 0.8f;        // 保牌倾向

        /// <summary>
        /// 执行AI回合
        /// </summary>
        public IEnumerator ExecuteAITurn()
        {
            if (controlledPlayer == null || !controlledPlayer.isAlive)
            {
                yield break;
            }

            Debug.Log($"[AI] {controlledPlayer.playerName} 开始思考...");

            // 模拟思考时间
            yield return new WaitForSeconds(thinkingTime);

            // 出牌阶段AI逻辑
            bool continuePlay = true;
            int maxActions = 10; // 防止无限循环
            int actionCount = 0;

            while (continuePlay && actionCount < maxActions)
            {
                // ⭐ 等待任何响应完成后再继续
                yield return StartCoroutine(WaitForResponseComplete());

                actionCount++;

                // 决定下一步行动
                AIAction action = DecideNextAction();

                if (action == null || action.actionType == AIActionType.EndPhase)
                {
                    continuePlay = false;
                    break;
                }

                // 执行行动
                bool success = ExecuteAction(action);

                if (success)
                {
                    Debug.Log($"[AI] {controlledPlayer.playerName} 执行了: {action.GetDescription()}");

                    // ⭐ 执行完后等待响应完成（如果触发了响应）
                    yield return new WaitForSeconds(0.5f); // 行动间隔
                    yield return StartCoroutine(WaitForResponseComplete());
                }
                else
                {
                    // 如果行动失败,尝试其他行动
                    yield return new WaitForSeconds(0.2f);
                }
            }

            // ⭐ 确保所有响应都完成后再结束
            yield return StartCoroutine(WaitForResponseComplete());

            // 结束出牌阶段
            Debug.Log($"[AI] {controlledPlayer.playerName} 结束出牌");
            BattleManager.Instance.EndPlayPhase();
        }

        /// <summary>
        /// ⭐ 等待响应完成
        /// </summary>
        private IEnumerator WaitForResponseComplete()
        {
            // 检查BattleUI是否正在等待响应
            while (UI.BattleUI.Instance != null && UI.BattleUI.Instance.IsWaitingForResponse())
            {
                Debug.Log($"[AI] {controlledPlayer.playerName} 等待响应完成...");
                yield return new WaitForSeconds(0.3f);
            }
        }

        /// <summary>
        /// 决定下一步行动
        /// </summary>
        private AIAction DecideNextAction()
        {
            List<AIAction> possibleActions = GetPossibleActions();

            if (possibleActions.Count == 0)
            {
                return new AIAction(AIActionType.EndPhase);
            }

            // 根据AI等级选择行动
            switch (aiLevel)
            {
                case 0: // 随机AI
                    return possibleActions[Random.Range(0, possibleActions.Count)];

                case 1: // 简单AI
                    return ChooseActionSimple(possibleActions);

                case 2: // 中等AI
                    return ChooseActionMedium(possibleActions);

                case 3: // 困难AI
                    return ChooseActionHard(possibleActions);

                default:
                    return possibleActions[0];
            }
        }

        /// <summary>
        /// 获取所有可能的行动
        /// </summary>
        private List<AIAction> GetPossibleActions()
        {
            List<AIAction> actions = new List<AIAction>();

            foreach (var card in controlledPlayer.handCards)
            {
                switch (card.cardName)
                {
                    case "桃":
                        // 如果HP不满,可以使用桃
                        if (controlledPlayer.currentHP < controlledPlayer.maxHP)
                        {
                            actions.Add(new AIAction(AIActionType.UsePeach, card, null));
                        }
                        break;

                    case "杀":
                        // ⭐ 检查是否还能使用杀
                        if (controlledPlayer.CanUseSlash())
                        {
                            // 寻找攻击范围内的目标
                            var targets = GetSlashTargets();
                            foreach (var target in targets)
                            {
                                actions.Add(new AIAction(AIActionType.UseSlash, card, target));
                            }
                        }
                        break;

                    case "决斗":
                        var duelTargets = GetAttackTargets();
                        foreach (var target in duelTargets)
                        {
                            actions.Add(new AIAction(AIActionType.UseDuel, card, target));
                        }
                        break;

                    case "南蛮入侵":
                        if (GetAliveEnemies().Count > 0)
                        {
                            actions.Add(new AIAction(AIActionType.UseSavageAssault, card, null));
                        }
                        break;

                    case "万箭齐发":
                        if (GetAliveEnemies().Count > 0)
                        {
                            actions.Add(new AIAction(AIActionType.UseArrowBarrage, card, null));
                        }
                        break;

                    case "桃园结义":
                        // 如果自己或队友HP不满
                        if (ShouldUsePeachGarden())
                        {
                            actions.Add(new AIAction(AIActionType.UsePeachGarden, card, null));
                        }
                        break;
                }
            }

            // 总是可以选择结束
            actions.Add(new AIAction(AIActionType.EndPhase));

            return actions;
        }

        /// <summary>
        /// ⭐ 简单AI选择(优先治疗>攻击>结束) - 考虑故事模式规则
        /// </summary>
        private AIAction ChooseActionSimple(List<AIAction> actions)
        {
            // 优先级: 濒死治疗 > 普通治疗 > 攻击敌人 > AOE > 结束

            // 1. 如果快死了,优先用桃
            if (controlledPlayer.currentHP <= 1)
            {
                var peach = actions.FirstOrDefault(a => a.actionType == AIActionType.UsePeach);
                if (peach != null) return peach;
            }

            // 2. 如果HP不满,考虑治疗
            if (controlledPlayer.currentHP < controlledPlayer.maxHP * 0.7f)
            {
                var heal = actions.FirstOrDefault(a => a.actionType == AIActionType.UsePeach);
                if (heal != null && Random.value < 0.7f) return heal;
            }

            // 3. ⭐ 攻击敌人（只攻击敌人，不攻击盟友）
            var attacks = actions.Where(a =>
                (a.actionType == AIActionType.UseSlash || a.actionType == AIActionType.UseDuel) &&
                a.target != null && !IsAlly(a.target)
            ).ToList();

            if (attacks.Count > 0)
            {
                // 攻击HP最少的敌人
                var attack = attacks.OrderBy(a => a.target?.currentHP ?? 999).First();
                return attack;
            }

            // 4. ⭐ 使用AOE技能（只在有敌人时使用）
            var enemies = GetAliveEnemies();
            if (enemies.Count > 0)
            {
                var aoe = actions.FirstOrDefault(a => a.actionType == AIActionType.UseSavageAssault ||
                                                      a.actionType == AIActionType.UseArrowBarrage);
                if (aoe != null && Random.value < 0.5f) return aoe;
            }

            // 5. 结束
            return new AIAction(AIActionType.EndPhase);
        }

        /// <summary>
        /// 中等AI选择(评估威胁和收益)
        /// </summary>
        private AIAction ChooseActionMedium(List<AIAction> actions)
        {
            float bestScore = float.MinValue;
            AIAction bestAction = null;

            foreach (var action in actions)
            {
                float score = EvaluateAction(action);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestAction = action;
                }
            }

            return bestAction ?? new AIAction(AIActionType.EndPhase);
        }

        /// <summary>
        /// 困难AI选择(深度评估和策略)
        /// </summary>
        private AIAction ChooseActionHard(List<AIAction> actions)
        {
            // 困难AI会考虑多步策略
            return ChooseActionMedium(actions); // 暂时使用中等AI逻辑
        }

        /// <summary>
        /// ⭐ 评估行动得分 - 考虑故事模式盟友/敌人
        /// </summary>
        private float EvaluateAction(AIAction action)
        {
            float score = 0f;

            switch (action.actionType)
            {
                case AIActionType.UsePeach:
                    // 根据当前HP计算治疗价值
                    float hpPercent = (float)controlledPlayer.currentHP / controlledPlayer.maxHP;
                    score = (1f - hpPercent) * 100f * healWeight;
                    break;

                case AIActionType.UseSlash:
                case AIActionType.UseDuel:
                    if (action.target != null)
                    {
                        // ⭐ 检查目标是否是盟友 - 盟友不应该攻击
                        if (IsAlly(action.target))
                        {
                            score = -1000f; // 强烈不建议攻击盟友
                            break;
                        }

                        // ⭐ 检查目标是否可以被攻击
                        if (StoryBattleManager.Instance != null &&
                            !StoryBattleManager.Instance.IsTargetAttackable(controlledPlayer, action.target))
                        {
                            score = -1000f; // 不能攻击受保护的目标
                            break;
                        }

                        // 优先攻击低HP敌人
                        float targetHpPercent = (float)action.target.currentHP / action.target.maxHP;
                        score = (1f - targetHpPercent) * 80f * attackWeight;

                        // 如果能击杀,额外加分
                        if (action.target.currentHP <= 1)
                        {
                            score += 50f;
                        }
                    }
                    break;

                case AIActionType.UseSavageAssault:
                case AIActionType.UseArrowBarrage:
                    // ⭐ AOE价值 = 敌人数量 - 盟友数量（AOE会伤害盟友）
                    int enemyCount = GetAliveEnemies().Count;
                    int allyCount = GetAliveAllies().Count - 1; // 排除自己
                    score = (enemyCount - allyCount * 0.5f) * 30f * attackWeight;

                    // 如果盟友比敌人多，不建议使用AOE
                    if (allyCount > enemyCount)
                    {
                        score -= 50f;
                    }
                    break;

                case AIActionType.UsePeachGarden:
                    // ⭐ 考虑需要治疗的盟友数量
                    int healCount = 0;
                    if (controlledPlayer.currentHP < controlledPlayer.maxHP) healCount++;

                    foreach (var ally in GetAliveAllies())
                    {
                        if (ally != controlledPlayer && ally.currentHP < ally.maxHP)
                        {
                            healCount++;
                        }
                    }
                    score = healCount * 40f * healWeight;
                    break;

                case AIActionType.EndPhase:
                    // 保牌价值
                    score = controlledPlayer.handCards.Count * 5f * saveWeight;
                    break;
            }

            // 添加随机性
            score += Random.Range(-10f, 10f);

            return score;
        }

        /// <summary>
        /// 执行行动
        /// </summary>
        private bool ExecuteAction(AIAction action)
        {
            try
            {
                switch (action.actionType)
                {
                    case AIActionType.UsePeach:
                        BattleManager.Instance.UsePeach(controlledPlayer, action.card);
                        return true;

                    case AIActionType.UseSlash:
                        BattleManager.Instance.UseSlash(controlledPlayer, action.target, action.card);
                        return true;

                    case AIActionType.UseDuel:
                        BattleManager.Instance.UseDuel(controlledPlayer, action.target, action.card);
                        return true;

                    case AIActionType.UseSavageAssault:
                        BattleManager.Instance.UseSavageAssault(controlledPlayer, action.card);
                        return true;

                    case AIActionType.UseArrowBarrage:
                        BattleManager.Instance.UseArrowBarrage(controlledPlayer, action.card);
                        return true;

                    case AIActionType.UsePeachGarden:
                        BattleManager.Instance.UsePeachGarden(controlledPlayer, action.card);
                        return true;

                    default:
                        return false;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[AI] 执行行动失败: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// ⭐ 获取可攻击的目标（考虑故事模式规则）
        /// </summary>
        private List<Player> GetAttackTargets()
        {
            List<Player> targets = new List<Player>();

            // ⭐ 优先使用StoryBattleManager的规则检查
            if (StoryBattleManager.Instance != null && StoryBattleManager.Instance.isBattleActive)
            {
                // 只攻击敌人，不攻击盟友
                foreach (var player in BattleManager.Instance.players)
                {
                    if (player != controlledPlayer && player.isAlive &&
                        StoryBattleManager.Instance.IsTargetAttackable(controlledPlayer, player))
                    {
                        targets.Add(player);
                    }
                }
                return targets;
            }

            // 回退逻辑：普通模式下攻击所有非自己的存活玩家
            foreach (var player in BattleManager.Instance.players)
            {
                if (player != controlledPlayer && player.isAlive)
                {
                    targets.Add(player);
                }
            }

            return targets;
        }

        /// <summary>
        /// ⭐ 获取杀的有效目标（考虑攻击范围和故事模式规则）
        /// </summary>
        private List<Player> GetSlashTargets()
        {
            // ⭐ 优先使用StoryBattleManager的规则检查
            if (StoryBattleManager.Instance != null && StoryBattleManager.Instance.isBattleActive)
            {
                return StoryBattleManager.Instance.GetValidAttackTargets(controlledPlayer);
            }

            // 回退逻辑：使用Player类的方法获取攻击范围内的目标
            return controlledPlayer.GetValidSlashTargets();
        }

        /// <summary>
        /// ⭐ 获取存活的敌人（区分故事模式盟友/敌人）
        /// </summary>
        private List<Player> GetAliveEnemies()
        {
            // ⭐ 优先使用StoryBattleManager的敌人列表
            if (StoryBattleManager.Instance != null && StoryBattleManager.Instance.isBattleActive)
            {
                return StoryBattleManager.Instance.GetEnemyPlayers();
            }

            return GetAttackTargets();
        }

        /// <summary>
        /// ⭐ 获取存活的盟友
        /// </summary>
        private List<Player> GetAliveAllies()
        {
            if (StoryBattleManager.Instance != null && StoryBattleManager.Instance.isBattleActive)
            {
                return StoryBattleManager.Instance.GetAllyPlayers();
            }

            // 回退逻辑：只返回自己
            return new List<Player> { controlledPlayer };
        }

        /// <summary>
        /// ⭐ 检查是否是盟友
        /// </summary>
        private bool IsAlly(Player other)
        {
            if (other == null || other == controlledPlayer) return false;

            if (StoryBattleManager.Instance != null && StoryBattleManager.Instance.isBattleActive)
            {
                return StoryBattleManager.Instance.IsAlly(controlledPlayer, other);
            }

            return controlledPlayer.faction == other.faction;
        }

        /// <summary>
        /// ⭐ 判断是否应该使用桃园结义（考虑盟友）
        /// </summary>
        private bool ShouldUsePeachGarden()
        {
            // 如果自己HP低于70%
            if (controlledPlayer.currentHP < controlledPlayer.maxHP * 0.7f)
            {
                return true;
            }

            // ⭐ 检查盟友是否需要治疗
            var allies = GetAliveAllies();
            foreach (var ally in allies)
            {
                if (ally != controlledPlayer && ally.currentHP < ally.maxHP * 0.5f)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// ⭐ 获取需要治疗的盟友（濒死优先）
        /// </summary>
        private Player GetAllyNeedingHeal()
        {
            var allies = GetAliveAllies();

            // 优先救濒死的盟友
            foreach (var ally in allies)
            {
                if (ally != controlledPlayer && ally.currentHP <= 0)
                {
                    // 检查是否可以对盟友使用桃
                    if (StoryBattleManager.Instance == null ||
                        StoryBattleManager.Instance.CanUsePeachOn(controlledPlayer, ally))
                    {
                        return ally;
                    }
                }
            }

            // 然后是HP很低的盟友
            foreach (var ally in allies)
            {
                if (ally != controlledPlayer && ally.currentHP <= ally.maxHP * 0.3f)
                {
                    if (StoryBattleManager.Instance == null ||
                        StoryBattleManager.Instance.CanUsePeachOn(controlledPlayer, ally))
                    {
                        return ally;
                    }
                }
            }

            return null;
        }
    }

    /// <summary>
    /// AI行动类型
    /// </summary>
    public enum AIActionType
    {
        UsePeach,           // 使用桃
        UseSlash,           // 使用杀
        UseDuel,            // 使用决斗
        UseSavageAssault,   // 使用南蛮入侵
        UseArrowBarrage,    // 使用万箭齐发
        UsePeachGarden,     // 使用桃园结义
        EndPhase            // 结束阶段
    }

    /// <summary>
    /// AI行动
    /// </summary>
    public class AIAction
    {
        public AIActionType actionType;
        public Card card;
        public Player target;

        public AIAction(AIActionType type, Card card = null, Player target = null)
        {
            this.actionType = type;
            this.card = card;
            this.target = target;
        }

        public string GetDescription()
        {
            switch (actionType)
            {
                case AIActionType.UsePeach:
                    return "使用【桃】";
                case AIActionType.UseSlash:
                    return $"对 {target?.playerName} 使用【杀】";
                case AIActionType.UseDuel:
                    return $"对 {target?.playerName} 使用【决斗】";
                case AIActionType.UseSavageAssault:
                    return "使用【南蛮入侵】";
                case AIActionType.UseArrowBarrage:
                    return "使用【万箭齐发】";
                case AIActionType.UsePeachGarden:
                    return "使用【桃园结义】";
                case AIActionType.EndPhase:
                    return "结束出牌";
                default:
                    return "未知行动";
            }
        }
    }
}