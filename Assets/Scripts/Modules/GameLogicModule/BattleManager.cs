using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using ThreeKingdoms.AI;

namespace ThreeKingdoms
{
    /// <summary>
    /// 回合阶段
    /// </summary>
    public enum TurnPhase
    {
        Prepare,        // 准备阶段
        Judge,          // 判定阶段
        Draw,           // 摸牌阶段
        Play,           // 出牌阶段
        Discard,        // 弃牌阶段
        End             // 结束阶段
    }

    /// <summary>
    /// 战斗管理器
    /// </summary>
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        [Header("游戏设置")]
        public int drawPhaseCardCount = 2;      // 摸牌阶段摸牌数

        [Header("玩家")]
        public List<Player> players = new List<Player>();
        public int currentPlayerIndex = 0;

        [Header("回合信息")]
        public TurnPhase currentPhase = TurnPhase.Prepare;
        public int turnCount = 0;

        [Header("游戏状态")]
        public bool gameStarted = false;
        public bool gameOver = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 开始游戏
        /// </summary>
        public void StartGame()
        {
            if (players.Count < 2)
            {
                Debug.LogError("玩家数量不足,无法开始游戏!");
                return;
            }

            gameStarted = true;
            gameOver = false;
            turnCount = 0;
            currentPlayerIndex = 0;

            // 给所有玩家发起始手牌
            foreach (var player in players)
            {
                int startCardCount = 4; // 起始手牌数
                List<Card> startCards = DeckManager.Instance.DrawCards(startCardCount);
                player.DrawCards(startCards);
            }

            Debug.Log("游戏开始!");
            StartTurn();
        }

        /// <summary>
        /// 开始回合
        /// </summary>
        private void StartTurn()
        {
            if (gameOver) return;

            Player currentPlayer = GetCurrentPlayer();
            turnCount++;

            Debug.Log($"========== 第 {turnCount} 回合 ==========");
            Debug.Log($"当前玩家: {currentPlayer.playerName}");

            // 更新UI
            UpdateUI();

            // 准备阶段
            currentPhase = TurnPhase.Prepare;
            PreparePhase();
        }

        /// <summary>
        /// 准备阶段
        /// </summary>
        private void PreparePhase()
        {
            Debug.Log("【准备阶段】");
            // 准备阶段逻辑
            NextPhase();
        }

        /// <summary>
        /// 判定阶段
        /// </summary>
        private void JudgePhase()
        {
            Debug.Log("【判定阶段】");
            Player currentPlayer = GetCurrentPlayer();

            // 处理判定区的牌
            if (currentPlayer.judgeCards.Count > 0)
            {
                // TODO: 处理判定
            }

            NextPhase();
        }

        /// <summary>
        /// 摸牌阶段
        /// </summary>
        private void DrawPhase()
        {
            Debug.Log("【摸牌阶段】");
            Player currentPlayer = GetCurrentPlayer();

            List<Card> drawnCards = DeckManager.Instance.DrawCards(drawPhaseCardCount);
            currentPlayer.DrawCards(drawnCards);

            Debug.Log($"{currentPlayer.playerName} 摸了 {drawPhaseCardCount} 张牌");

            // 更新UI
            UpdateUI();

            NextPhase();
        }

        /// <summary>
        /// 出牌阶段
        /// </summary>
        private void PlayPhase()
        {
            Debug.Log("【出牌阶段】");
            Player currentPlayer = GetCurrentPlayer();

            if (currentPlayer.isAI && currentPlayer.aiController != null)
            {
                StartCoroutine(currentPlayer.aiController.ExecuteAITurn());
            }
            else
            {
                Debug.Log("等待玩家操作...");
            }
        }

        /// <summary>
        /// 结束出牌阶段
        /// </summary>
        public void EndPlayPhase()
        {
            NextPhase();
        }

        /// <summary>
        /// 弃牌阶段
        /// </summary>
        private void DiscardPhase()
        {
            Debug.Log("【弃牌阶段】");
            Player currentPlayer = GetCurrentPlayer();

            int handCardLimit = currentPlayer.GetHandCardLimit();
            int cardsToDiscard = currentPlayer.handCards.Count - handCardLimit;

            if (cardsToDiscard > 0)
            {
                Debug.Log($"{currentPlayer.playerName} 需要弃置 {cardsToDiscard} 张牌");

                // 自动弃牌(实际应该由玩家选择)
                for (int i = 0; i < cardsToDiscard && currentPlayer.handCards.Count > 0; i++)
                {
                    Card card = currentPlayer.handCards[0];
                    currentPlayer.DiscardCard(card);
                    DeckManager.Instance.DiscardCard(card);
                }
            }

            NextPhase();
        }

        /// <summary>
        /// 结束阶段
        /// </summary>
        private void EndPhase()
        {
            Debug.Log("【结束阶段】");
            // 结束阶段逻辑

            EndTurn();
        }

        /// <summary>
        /// 进入下一阶段
        /// </summary>
        private void NextPhase()
        {
            switch (currentPhase)
            {
                case TurnPhase.Prepare:
                    currentPhase = TurnPhase.Judge;
                    JudgePhase();
                    break;
                case TurnPhase.Judge:
                    currentPhase = TurnPhase.Draw;
                    DrawPhase();
                    break;
                case TurnPhase.Draw:
                    currentPhase = TurnPhase.Play;
                    PlayPhase();
                    break;
                case TurnPhase.Play:
                    currentPhase = TurnPhase.Discard;
                    DiscardPhase();
                    break;
                case TurnPhase.Discard:
                    currentPhase = TurnPhase.End;
                    EndPhase();
                    break;
            }
        }

        /// <summary>
        /// 结束回合
        /// </summary>
        private void EndTurn()
        {
            // 切换到下一个玩家
            do
            {
                currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
            }
            while (!players[currentPlayerIndex].isAlive && !AllPlayersDeadExceptOne());

            // 检查游戏是否结束
            if (CheckGameOver())
            {
                return;
            }

            // 开始下一回合
            Invoke(nameof(StartTurn), 1f); // 1秒后开始下一回合
        }

        /// <summary>
        /// 获取当前玩家
        /// </summary>
        public Player GetCurrentPlayer()
        {
            if (currentPlayerIndex >= 0 && currentPlayerIndex < players.Count)
            {
                return players[currentPlayerIndex];
            }
            return null;
        }

        /// <summary>
        /// 检查是否只剩一个玩家存活
        /// </summary>
        private bool AllPlayersDeadExceptOne()
        {
            int aliveCount = 0;
            foreach (var player in players)
            {
                if (player.isAlive) aliveCount++;
            }
            return aliveCount <= 1;
        }

        /// <summary>
        /// 检查游戏是否结束
        /// </summary>
        private bool CheckGameOver()
        {
            int aliveCount = 0;
            Player winner = null;

            foreach (var player in players)
            {
                if (player.isAlive)
                {
                    aliveCount++;
                    winner = player;
                }
            }

            if (aliveCount == 1)
            {
                gameOver = true;
                if (winner != null)
                {
                    Debug.Log($"========== 游戏结束! ==========");
                    Debug.Log($"获胜者: {winner.playerName}");
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// 使用【杀】
        /// </summary>
        public void UseSlash(Player user, Player target, Card slashCard)
        {
            if (!user.PlayCard(slashCard))
            {
                Debug.LogWarning("无法打出此牌!");
                return;
            }

            Debug.Log($"{user.playerName} 对 {target.playerName} 使用了【杀】");

            // 询问目标是否出【闪】
            bool dodged = AskForDodge(target);

            if (!dodged)
            {
                target.TakeDamage(1, user);

                // ⭐ 添加这几行
                if (EventManager.Instance != null)
                {
                    EventManager.Instance.TriggerPlayerDamaged(target, user, 1, slashCard);
                }
            }

            DeckManager.Instance.DiscardCard(slashCard);
        }

        /// <summary>
        /// 询问出【闪】
        /// </summary>
        private bool AskForDodge(Player player)
        {
            // 简化版:自动检查是否有闪
            foreach (var card in player.handCards)
            {
                if (card.cardName == "闪")
                {
                    player.PlayCard(card);
                    DeckManager.Instance.DiscardCard(card);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 使用【桃】
        /// </summary>
        public void UsePeach(Player user, Card peachCard)
        {
            if (!user.PlayCard(peachCard))
            {
                Debug.LogWarning("无法打出此牌!");
                return;
            }

            user.Recover(1);
            DeckManager.Instance.DiscardCard(peachCard);

            // 更新UI
            UpdateUI();
        }
        /// <summary>
        /// 使用【决斗】
        /// 双方轮流出【杀】，先没杀的人受到1点伤害
        /// </summary>
        public void UseDuel(Player user, Player target, Card card)
        {
            if (!user.PlayCard(card))
            {
                Debug.LogWarning("无法打出此牌!");
                return;
            }

            Debug.Log($"{user.playerName} 对 {target.playerName} 使用了【决斗】");
            DeckManager.Instance.DiscardCard(card);

            // 触发使用卡牌事件
            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerCardUsed(user, card, target);
            }

            // 决斗流程：目标先出杀
            Player currentResponder = target;
            Player opponent = user;
            bool targetLost = false;

            while (true)
            {
                Debug.Log($"[决斗] 等待 {currentResponder.playerName} 出【杀】");

                // 检查是否有杀
                Card slashCard = FindSlashInHand(currentResponder);

                if (slashCard == null)
                {
                    // 没有杀，决斗失败
                    Debug.Log($"[决斗] {currentResponder.playerName} 没有【杀】");
                    targetLost = currentResponder == target;
                    break;
                }

                // 有杀，打出
                currentResponder.PlayCard(slashCard);
                DeckManager.Instance.DiscardCard(slashCard);
                Debug.Log($"[决斗] {currentResponder.playerName} 打出了【杀】");

                // 交换响应者
                Player temp = currentResponder;
                currentResponder = opponent;
                opponent = temp;
            }

            // 决定谁受伤
            Player loser = targetLost ? target : user;
            Debug.Log($"[决斗] {loser.playerName} 决斗失败，受到1点伤害");
            loser.TakeDamage(1, loser == target ? user : target);

            // 触发受伤事件
            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerPlayerDamaged(loser, loser == target ? user : target, 1, card);
            }

            // 更新UI
            UpdateUI();
        }

        /// <summary>
        /// 在手牌中查找【杀】
        /// </summary>
        private Card FindSlashInHand(Player player)
        {
            foreach (var card in player.handCards)
            {
                if (card.cardName == "杀")
                {
                    return card;
                }
            }
            return null;
        }

        /// <summary>
        /// 使用【顺手牵羊】
        /// </summary>
        public void UseSnatch(Player user, Player target, Card card)
        {
            if (!user.PlayCard(card))
            {
                Debug.LogWarning("无法打出此牌!");
                return;
            }

            Debug.Log($"{user.playerName} 对 {target.playerName} 使用了【顺手牵羊】");
            DeckManager.Instance.DiscardCard(card);

            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerCardUsed(user, card, target);
            }

            if (target.handCards.Count == 0)
            {
                Debug.Log($"[顺手牵羊] {target.playerName} 没有手牌");
                if (UI.BattleUI.Instance != null)
                {
                    UI.BattleUI.Instance.AddLog($"{target.playerName} 没有手牌");
                }
                UpdateUI();
                return;
            }

            int randomIndex = Random.Range(0, target.handCards.Count);
            Card snatched = target.handCards[randomIndex];

            target.handCards.RemoveAt(randomIndex);
            user.handCards.Add(snatched);

            Debug.Log($"[顺手牵羊] {user.playerName} 获得了 {target.playerName} 的一张手牌");

            if (UI.BattleUI.Instance != null)
            {
                string cardName = CardNameHelper.GetLocalizedCardName(snatched.cardName);
                UI.BattleUI.Instance.AddLog($"{user.playerName} 获得了 {target.playerName} 的【{cardName}】");
            }

            UpdateUI();
        }

        /// <summary>
        /// 使用【过河拆桥】
        /// </summary>
        public void UseDismantlement(Player user, Player target, Card card)
        {
            if (!user.PlayCard(card))
            {
                Debug.LogWarning("无法打出此牌!");
                return;
            }

            Debug.Log($"{user.playerName} 对 {target.playerName} 使用了【过河拆桥】");
            DeckManager.Instance.DiscardCard(card);

            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerCardUsed(user, card, target);
            }

            int totalCards = target.handCards.Count + target.equipments.Count;
            if (totalCards == 0)
            {
                Debug.Log($"[过河拆桥] {target.playerName} 没有可弃置的牌");
                if (UI.BattleUI.Instance != null)
                {
                    UI.BattleUI.Instance.AddLog($"{target.playerName} 没有可弃置的牌");
                }
                UpdateUI();
                return;
            }

            int randomChoice = Random.Range(0, totalCards);
            Card discarded = null;
            string cardType = "";

            if (randomChoice < target.handCards.Count)
            {
                discarded = target.handCards[randomChoice];
                target.handCards.RemoveAt(randomChoice);
                cardType = "手牌";
            }
            else
            {
                int equipIndex = randomChoice - target.handCards.Count;
                discarded = target.equipments[equipIndex];
                target.equipments.RemoveAt(equipIndex);
                cardType = "装备";
            }

            DeckManager.Instance.DiscardCard(discarded);

            Debug.Log($"[过河拆桥] {user.playerName} 弃置了 {target.playerName} 的一张{cardType}");

            if (UI.BattleUI.Instance != null)
            {
                string cardName = CardNameHelper.GetLocalizedCardName(discarded.cardName);
                UI.BattleUI.Instance.AddLog($"{user.playerName} 弃置了 {target.playerName} 的【{cardName}】");
            }

            UpdateUI();
        }

        /// <summary>
        /// 使用【五谷丰登】
        /// </summary>
        public void UseHarvest(Player user, Card card)
        {
            if (!user.PlayCard(card))
            {
                Debug.LogWarning("无法打出此牌!");
                return;
            }

            Debug.Log($"{user.playerName} 使用了【五谷丰登】");
            DeckManager.Instance.DiscardCard(card);

            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerCardUsed(user, card, null);
            }

            int aliveCount = 0;
            foreach (var player in players)
            {
                if (player.isAlive) aliveCount++;
            }

            List<Card> harvestCards = new List<Card>();
            for (int i = 0; i < aliveCount; i++)
            {
                Card drawn = DeckManager.Instance.DrawCard();
                if (drawn != null)
                {
                    harvestCards.Add(drawn);
                }
            }

            Debug.Log($"[五谷丰登] 亮出了 {harvestCards.Count} 张牌");

            int currentPlayerIndex = players.IndexOf(user);

            for (int i = 0; i < aliveCount && harvestCards.Count > 0; i++)
            {
                int playerIndex = (currentPlayerIndex + i) % players.Count;
                Player player = players[playerIndex];

                if (!player.isAlive) continue;

                int randomIndex = Random.Range(0, harvestCards.Count);
                Card chosen = harvestCards[randomIndex];
                harvestCards.RemoveAt(randomIndex);

                player.handCards.Add(chosen);

                Debug.Log($"[五谷丰登] {player.playerName} 获得了【{chosen.cardName}】");

                if (UI.BattleUI.Instance != null)
                {
                    string cardName = CardNameHelper.GetLocalizedCardName(chosen.cardName);
                    UI.BattleUI.Instance.AddLog($"{player.playerName} 获得了【{cardName}】");
                }
            }

            UpdateUI();
        }

        /// <summary>
        /// 使用【南蛮入侵】
        /// 所有其他角色需打出【杀】，否则受到1点伤害
        /// </summary>
        public void UseSavageAssault(Player user, Card card)
        {
            if (!user.PlayCard(card))
            {
                Debug.LogWarning("无法打出此牌!");
                return;
            }

            Debug.Log($"{user.playerName} 使用了【南蛮入侵】");
            DeckManager.Instance.DiscardCard(card);

            // 触发使用卡牌事件
            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerCardUsed(user, card, null);
            }

            // 结算每个其他玩家
            foreach (var player in players)
            {
                if (player == user || !player.isAlive) continue;

                Debug.Log($"[南蛮入侵] {player.playerName} 需要打出【杀】");

                // 检查是否有杀
                Card slashCard = FindSlashInHand(player);

                if (slashCard != null)
                {
                    // 有杀，打出
                    player.PlayCard(slashCard);
                    DeckManager.Instance.DiscardCard(slashCard);
                    Debug.Log($"[南蛮入侵] {player.playerName} 打出了【杀】，免疫伤害");
                }
                else
                {
                    // 没有杀，受到伤害
                    Debug.Log($"[南蛮入侵] {player.playerName} 没有【杀】，受到1点伤害");
                    player.TakeDamage(1, user);

                    // 触发受伤事件
                    if (EventManager.Instance != null)
                    {
                        EventManager.Instance.TriggerPlayerDamaged(player, user, 1, card);
                    }
                }
            }

            // 更新UI
            UpdateUI();
        }

        /// <summary>
        /// 使用【万箭齐发】
        /// 所有其他角色需打出【闪】，否则受到1点伤害
        /// </summary>
        public void UseArrowBarrage(Player user, Card card)
        {
            if (!user.PlayCard(card))
            {
                Debug.LogWarning("无法打出此牌!");
                return;
            }

            Debug.Log($"{user.playerName} 使用了【万箭齐发】");
            DeckManager.Instance.DiscardCard(card);

            // 触发使用卡牌事件
            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerCardUsed(user, card, null);
            }

            // 结算每个其他玩家
            foreach (var player in players)
            {
                if (player == user || !player.isAlive) continue;

                Debug.Log($"[万箭齐发] {player.playerName} 需要打出【闪】");

                // 检查是否有闪
                Card dodgeCard = FindDodgeInHand(player);

                if (dodgeCard != null)
                {
                    // 有闪，打出
                    player.PlayCard(dodgeCard);
                    DeckManager.Instance.DiscardCard(dodgeCard);
                    Debug.Log($"[万箭齐发] {player.playerName} 打出了【闪】，免疫伤害");
                }
                else
                {
                    // 没有闪，受到伤害
                    Debug.Log($"[万箭齐发] {player.playerName} 没有【闪】，受到1点伤害");
                    player.TakeDamage(1, user);

                    // 触发受伤事件
                    if (EventManager.Instance != null)
                    {
                        EventManager.Instance.TriggerPlayerDamaged(player, user, 1, card);
                    }
                }
            }

            // 更新UI
            UpdateUI();
        }

        /// <summary>
        /// 在手牌中查找【闪】
        /// </summary>
        private Card FindDodgeInHand(Player player)
        {
            foreach (var card in player.handCards)
            {
                if (card.cardName == "闪")
                {
                    return card;
                }
            }
            return null;
        }

        /// <summary>
        /// 使用【桃园结义】
        /// </summary>
        public void UsePeachGarden(Player user, Card card)
        {
            user.PlayCard(card);
            DeckManager.Instance.DiscardCard(card);

            foreach (var player in players)
            {
                if (player.isAlive && player.currentHP < player.maxHP)
                {
                    player.Recover(1);
                }
            }
        }
        /// <summary>
        /// 更新UI
        /// </summary>
        private void UpdateUI()
        {
            if (UI.BattleUI.Instance != null)
            {
                Player currentPlayer = GetCurrentPlayer();
                if (currentPlayer != null)
                {
                    // ⭐ 只有本地玩家回合才更新手牌
                    if (currentPlayer == players[0])  // 第一个玩家是本地玩家
                    {
                        UI.BattleUI.Instance.UpdateHandCards(currentPlayer.handCards);
                    }

                    UI.BattleUI.Instance.UpdateAllPlayerInfo();
                    UI.BattleUI.Instance.UpdateCurrentPlayerIndicator(currentPlayer);
                }
            }
        }
    }
}