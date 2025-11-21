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
            }
            else
            {
                Debug.Log($"{target.playerName} 使用【闪】躲避了攻击");
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
        /// </summary>
        public void UseDuel(Player user, Player target, Card card)
        {
            user.PlayCard(card);
            DeckManager.Instance.DiscardCard(card);
            target.TakeDamage(1, user);
            Debug.Log($"{user.playerName} 对 {target.playerName} 使用了【决斗】");
        }

        /// <summary>
        /// 使用【南蛮入侵】
        /// </summary>
        public void UseSavageAssault(Player user, Card card)
        {
            user.PlayCard(card);
            DeckManager.Instance.DiscardCard(card);

            foreach (var player in players)
            {
                if (player != user && player.isAlive)
                {
                    player.TakeDamage(1, user);
                }
            }
            Debug.Log($"{user.playerName} 使用了【南蛮入侵】");
        }

        /// <summary>
        /// 使用【万箭齐发】
        /// </summary>
        public void UseArrowBarrage(Player user, Card card)
        {
            user.PlayCard(card);
            DeckManager.Instance.DiscardCard(card);

            foreach (var player in players)
            {
                if (player != user && player.isAlive)
                {
                    player.TakeDamage(1, user);
                }
            }
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
                    // 更新当前玩家手牌
                    UI.BattleUI.Instance.UpdateHandCards(currentPlayer.handCards);

                    // 更新所有玩家信息
                    UI.BattleUI.Instance.UpdateAllPlayerInfo();

                    // 更新当前玩家指示器
                    UI.BattleUI.Instance.UpdateCurrentPlayerIndicator(currentPlayer);
                }
            }
        }
    }
}