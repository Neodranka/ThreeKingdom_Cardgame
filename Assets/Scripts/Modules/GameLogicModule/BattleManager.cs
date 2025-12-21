using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using ThreeKingdoms.AI;
using ThreeKingdoms.Story;

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
        public int turnCount = 0;           // 轮数（所有玩家完成一轮算一回合）
        private int roundStartPlayerIndex = 0;  // ⭐ 每轮开始的玩家索引

        [Header("游戏状态")]
        public bool gameStarted = false;
        public bool gameOver = false;

        // ⭐ 判定结果标志（用于跳过阶段）
        private bool skipDrawPhase = false;
        private bool skipPlayPhase = false;

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
            turnCount = 1;  // ⭐ 第一轮
            currentPlayerIndex = 0;
            roundStartPlayerIndex = 0;  // ⭐ 记录第一轮开始的玩家

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
            // ⭐ turnCount 改为在 EndTurn 中当所有玩家轮完时递增

            Debug.Log($"========== 第 {turnCount} 轮 - {currentPlayer.playerName} 的回合 ==========");
            Debug.Log($"当前玩家: {currentPlayer.playerName}");

            // ⭐ 重置当前玩家的回合状态
            currentPlayer.ResetTurnState();

            // ⭐ 触发回合开始事件（用于技能如仁德重置状态）
            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerTurnStart(currentPlayer);
            }

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

            // 重置跳过标志
            skipDrawPhase = false;
            skipPlayPhase = false;

            // 处理判定区的牌
            if (currentPlayer.judgeCards.Count > 0)
            {
                StartCoroutine(ProcessJudgePhase(currentPlayer));
            }
            else
            {
                NextPhase();
            }
        }

        /// <summary>
        /// ⭐ 处理判定阶段（协程）
        /// </summary>
        private IEnumerator ProcessJudgePhase(Player player)
        {
            // 判定区的牌按后进先出顺序处理（从最后一张开始）
            while (player.judgeCards.Count > 0)
            {
                // 取最后一张（最新放入的）
                int lastIndex = player.judgeCards.Count - 1;
                Card judgeCard = player.judgeCards[lastIndex];

                Debug.Log($"[判定阶段] {player.playerName} 进行【{judgeCard.cardName}】的判定");

                if (UI.BattleUI.Instance != null)
                {
                    string cardName = CardNameHelper.GetLocalizedCardName(judgeCard.cardName);
                    UI.BattleUI.Instance.AddLocalizedLog("msg_judgment", player.playerName, cardName);
                }

                // ⭐ 询问无懈可击
                bool nullified = false;
                yield return StartCoroutine(RequestNullification(player, judgeCard, player, (result) =>
                {
                    nullified = result;
                }));

                if (nullified)
                {
                    Debug.Log($"[判定阶段] 【{judgeCard.cardName}】被无懈可击抵消");
                    player.judgeCards.RemoveAt(lastIndex);
                    DeckManager.Instance.DiscardCard(judgeCard);
                    yield return new WaitForSeconds(0.3f);
                    continue;
                }

                // 执行判定：从牌堆顶翻一张牌
                Card judgmentResult = DeckManager.Instance.DrawCard();
                if (judgmentResult == null)
                {
                    Debug.LogWarning("[判定阶段] 无法抽取判定牌！");
                    break;
                }

                // 显示判定结果
                string suitSymbol = GetSuitSymbol(judgmentResult.suit);
                Debug.Log($"[判定阶段] 判定结果：{suitSymbol}{judgmentResult.point}");

                if (UI.BattleUI.Instance != null)
                {
                    UI.BattleUI.Instance.AddLocalizedLog("msg_judgment_result", suitSymbol, judgmentResult.point.ToString());
                }

                // 显示判定牌动画
                if (UI.PlayedCardDisplayManager.Instance != null)
                {
                    UI.PlayedCardDisplayManager.Instance.ShowPlayedCard(judgmentResult, player, null);
                }

                yield return new WaitForSeconds(0.8f);

                // 根据判定结果处理效果
                ProcessJudgmentResult(player, judgeCard, judgmentResult);

                // 将判定牌放入弃牌堆
                DeckManager.Instance.DiscardCard(judgmentResult);

                yield return new WaitForSeconds(0.5f);
            }

            // 更新UI
            UpdateUI();

            // 进入下一阶段
            NextPhase();
        }

        /// <summary>
        /// ⭐ 获取花色符号
        /// </summary>
        private string GetSuitSymbol(CardSuit suit)
        {
            return suit switch
            {
                CardSuit.Spade => "♠",
                CardSuit.Heart => "♥",
                CardSuit.Club => "♣",
                CardSuit.Diamond => "♦",
                _ => "?"
            };
        }

        /// <summary>
        /// ⭐ 处理判定结果
        /// </summary>
        private void ProcessJudgmentResult(Player player, Card judgeCard, Card result)
        {
            // 从判定区移除该牌
            player.judgeCards.Remove(judgeCard);

            switch (judgeCard.cardName)
            {
                case "乐不思蜀":
                    // 判定结果不是红桃，跳过出牌阶段
                    if (result.suit != CardSuit.Heart)
                    {
                        skipPlayPhase = true;
                        Debug.Log($"[判定阶段] 乐不思蜀生效，{player.playerName} 将跳过出牌阶段");
                        if (UI.BattleUI.Instance != null)
                        {
                            UI.BattleUI.Instance.AddLocalizedLog("msg_indulgence_effect", player.playerName);
                        }
                    }
                    else
                    {
                        Debug.Log($"[判定阶段] 乐不思蜀未生效");
                        if (UI.BattleUI.Instance != null)
                        {
                            UI.BattleUI.Instance.AddLocalizedLog("msg_indulgence_miss", player.playerName);
                        }
                    }
                    DeckManager.Instance.DiscardCard(judgeCard);
                    break;

                case "闪电":
                    // 判定结果是黑桃2-9，受到3点雷电伤害
                    if (result.suit == CardSuit.Spade && result.point >= 2 && result.point <= 9)
                    {
                        Debug.Log($"[判定阶段] 闪电击中 {player.playerName}，受到3点雷电伤害！");
                        if (UI.BattleUI.Instance != null)
                        {
                            UI.BattleUI.Instance.AddLocalizedLog("msg_lightning_hit", player.playerName);
                        }
                        player.TakeDamage(3, null); // 雷电伤害，无来源

                        if (EventManager.Instance != null)
                        {
                            EventManager.Instance.TriggerPlayerDamaged(player, null, 3, judgeCard);
                        }

                        DeckManager.Instance.DiscardCard(judgeCard);
                    }
                    else
                    {
                        // 闪电传给下家
                        Debug.Log($"[判定阶段] 闪电未击中，传递给下家");
                        if (UI.BattleUI.Instance != null)
                        {
                            UI.BattleUI.Instance.AddLocalizedLog("msg_lightning_miss");
                        }
                        PassLightningToNext(player, judgeCard);
                    }
                    break;

                case "兵粮寸断":
                    // 判定结果不是梅花，跳过摸牌阶段
                    if (result.suit != CardSuit.Club)
                    {
                        skipDrawPhase = true;
                        Debug.Log($"[判定阶段] 兵粮寸断生效，{player.playerName} 将跳过摸牌阶段");
                        if (UI.BattleUI.Instance != null)
                        {
                            UI.BattleUI.Instance.AddLocalizedLog("msg_supply_shortage_effect", player.playerName);
                        }
                    }
                    else
                    {
                        Debug.Log($"[判定阶段] 兵粮寸断未生效");
                        if (UI.BattleUI.Instance != null)
                        {
                            UI.BattleUI.Instance.AddLocalizedLog("msg_supply_shortage_miss", player.playerName);
                        }
                    }
                    DeckManager.Instance.DiscardCard(judgeCard);
                    break;

                default:
                    // 其他判定牌（如八卦阵等技能判定）
                    DeckManager.Instance.DiscardCard(judgeCard);
                    break;
            }
        }

        /// <summary>
        /// ⭐ 将闪电传给下一个存活的玩家
        /// </summary>
        private void PassLightningToNext(Player currentPlayer, Card lightning)
        {
            int currentIndex = players.IndexOf(currentPlayer);
            for (int i = 1; i < players.Count; i++)
            {
                int nextIndex = (currentIndex + i) % players.Count;
                Player nextPlayer = players[nextIndex];

                if (nextPlayer.isAlive)
                {
                    // 检查下家判定区是否已有闪电
                    bool hasLightning = false;
                    foreach (var card in nextPlayer.judgeCards)
                    {
                        if (card.cardName == "闪电")
                        {
                            hasLightning = true;
                            break;
                        }
                    }

                    if (!hasLightning)
                    {
                        nextPlayer.judgeCards.Add(lightning);
                        Debug.Log($"[判定阶段] 闪电传递给 {nextPlayer.playerName}");
                        return;
                    }
                }
            }

            // 如果没有合适的玩家接收，弃掉闪电
            Debug.Log("[判定阶段] 无人可接收闪电，弃置");
            DeckManager.Instance.DiscardCard(lightning);
        }

        /// <summary>
        /// 摸牌阶段
        /// </summary>
        private void DrawPhase()
        {
            Debug.Log("【摸牌阶段】");
            Player currentPlayer = GetCurrentPlayer();

            // ⭐ 检查是否被兵粮寸断跳过
            if (skipDrawPhase)
            {
                Debug.Log($"{currentPlayer.playerName} 因兵粮寸断跳过摸牌阶段");
                skipDrawPhase = false;
                NextPhase();
                return;
            }

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

            // ⭐ 检查是否被乐不思蜀跳过
            if (skipPlayPhase)
            {
                Debug.Log($"{currentPlayer.playerName} 因乐不思蜀跳过出牌阶段");
                skipPlayPhase = false;
                NextPhase();
                return;
            }

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
            StartCoroutine(DiscardPhaseCoroutine());
        }

        /// <summary>
        /// ⭐ 弃牌阶段协程（支持玩家选择）
        /// </summary>
        private IEnumerator DiscardPhaseCoroutine()
        {
            Player currentPlayer = GetCurrentPlayer();

            int handCardLimit = currentPlayer.GetHandCardLimit();
            int cardsToDiscard = currentPlayer.handCards.Count - handCardLimit;

            if (cardsToDiscard > 0)
            {
                Debug.Log($"{currentPlayer.playerName} 需要弃置 {cardsToDiscard} 张牌（手牌{currentPlayer.handCards.Count}，上限{handCardLimit}）");

                // ⭐ 使用UI让玩家选择弃牌
                if (UI.BattleUI.Instance != null)
                {
                    bool discardComplete = false;

                    UI.BattleUI.Instance.RequestDiscard(currentPlayer, cardsToDiscard, (discardedCards) =>
                    {
                        discardComplete = true;
                    });

                    // 等待弃牌完成
                    while (!discardComplete)
                    {
                        yield return null;
                    }
                }
                else
                {
                    // 没有UI时自动弃牌（弃置最前面的牌）
                    for (int i = 0; i < cardsToDiscard && currentPlayer.handCards.Count > 0; i++)
                    {
                        Card card = currentPlayer.handCards[0];
                        currentPlayer.DiscardCard(card);
                        DeckManager.Instance.DiscardCard(card);
                    }
                    Debug.Log($"{currentPlayer.playerName} 自动弃置了 {cardsToDiscard} 张牌");
                }

                // 更新UI
                UpdateUI();
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
            // ⭐ 触发回合结束事件
            Player endingPlayer = GetCurrentPlayer();
            if (EventManager.Instance != null && endingPlayer != null)
            {
                EventManager.Instance.TriggerTurnEnd(endingPlayer);
            }

            // 切换到下一个玩家
            do
            {
                currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
            }
            while (!players[currentPlayerIndex].isAlive && !AllPlayersDeadExceptOne());

            // ⭐ 当轮回到起始玩家时，递增回合数（一轮完成）
            if (currentPlayerIndex == roundStartPlayerIndex)
            {
                turnCount++;
                Debug.Log($"========== 进入第 {turnCount} 轮 ==========");
            }

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
            // ⭐ 故事模式：由 StoryBattleManager 处理胜负判定，跳过身份场检测
            if (Story.StoryBattleManager.Instance != null && Story.StoryBattleManager.Instance.isBattleActive)
            {
                // 故事模式不使用身份场逻辑，由 StoryBattleManager 自己判定
                return false;
            }

            // ⭐ 身份场模式：使用身份场胜负判定（仅对战模式）
            if (GameConfig.Instance != null && GameConfig.Instance.enableIdentityMode)
            {
                return CheckIdentityModeGameOver();
            }

            // 普通模式：只剩一人存活
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

                // ⭐ 启动游戏结束流程
                StartCoroutine(HandleGameEnd(winner));
                return true;
            }

            return false;
        }

        /// <summary>
        /// ⭐ 身份场模式的胜负判定
        /// </summary>
        private bool CheckIdentityModeGameOver()
        {
            // 统计各身份存活情况
            Player lord = null;
            int aliveLoyalists = 0;
            int aliveRebels = 0;
            int aliveSpies = 0;
            Player lastAlive = null;
            int totalAlive = 0;

            foreach (var player in players)
            {
                if (player.isAlive)
                {
                    totalAlive++;
                    lastAlive = player;

                    switch (player.identity)
                    {
                        case Identity.Lord:
                            lord = player;
                            break;
                        case Identity.Loyalist:
                            aliveLoyalists++;
                            break;
                        case Identity.Rebel:
                            aliveRebels++;
                            break;
                        case Identity.Spy:
                            aliveSpies++;
                            break;
                    }
                }
            }

            // 判定1：主公死亡
            if (lord == null || !lord.isAlive)
            {
                gameOver = true;

                // 如果只剩内奸，内奸获胜
                if (totalAlive == 1 && lastAlive != null && lastAlive.identity == Identity.Spy)
                {
                    Debug.Log($"========== 游戏结束! ==========");
                    Debug.Log($"内奸获胜！{lastAlive.playerName} 成功成为最后的赢家！");
                    identityWinner = Identity.Spy;
                    StartCoroutine(HandleIdentityGameEnd(Identity.Spy, lastAlive));
                }
                else
                {
                    // 反贼获胜
                    Debug.Log($"========== 游戏结束! ==========");
                    Debug.Log($"反贼获胜！主公已被消灭！");
                    identityWinner = Identity.Rebel;
                    StartCoroutine(HandleIdentityGameEnd(Identity.Rebel, null));
                }
                return true;
            }

            // 判定2：所有反贼和内奸死亡 -> 主公/忠臣获胜
            if (aliveRebels == 0 && aliveSpies == 0)
            {
                gameOver = true;
                Debug.Log($"========== 游戏结束! ==========");
                Debug.Log($"主公/忠臣获胜！所有反贼和内奸已被消灭！");
                identityWinner = Identity.Lord;
                StartCoroutine(HandleIdentityGameEnd(Identity.Lord, lord));
                return true;
            }

            return false;
        }

        /// <summary>
        /// ⭐ 身份场胜利方
        /// </summary>
        private Identity identityWinner = Identity.None;

        /// <summary>
        /// ⭐ 处理身份场游戏结束
        /// </summary>
        private IEnumerator HandleIdentityGameEnd(Identity winningIdentity, Player mvp)
        {
            // 显示身份场结果UI
            ShowIdentityGameOverUI(winningIdentity, mvp);

            yield return new WaitForSeconds(4f);

            NavigateAfterGameEnd();
        }

        /// <summary>
        /// ⭐ 显示身份场游戏结束UI
        /// </summary>
        private void ShowIdentityGameOverUI(Identity winningIdentity, Player mvp)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;

            GameObject gameOverPanel = new GameObject("GameOverPanel");
            gameOverPanel.transform.SetParent(canvas.transform, false);

            var panelRect = gameOverPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;

            var panelImage = gameOverPanel.AddComponent<UnityEngine.UI.Image>();
            panelImage.color = new Color(0, 0, 0, 0.8f);

            // 创建结果文本
            GameObject textObj = new GameObject("ResultText");
            textObj.transform.SetParent(gameOverPanel.transform, false);

            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.sizeDelta = new Vector2(600, 200);

            var text = textObj.AddComponent<TMPro.TextMeshProUGUI>();
            text.fontSize = 48;
            text.alignment = TMPro.TextAlignmentOptions.Center;
            text.color = Color.white;

            // 判断玩家是否获胜
            Player humanPlayer = players.Find(p => !p.isAI);
            bool playerWins = false;

            if (humanPlayer != null)
            {
                // 玩家获胜条件
                if (winningIdentity == Identity.Lord || winningIdentity == Identity.Loyalist)
                {
                    playerWins = (humanPlayer.identity == Identity.Lord || humanPlayer.identity == Identity.Loyalist);
                }
                else if (winningIdentity == Identity.Rebel)
                {
                    playerWins = (humanPlayer.identity == Identity.Rebel);
                }
                else if (winningIdentity == Identity.Spy)
                {
                    playerWins = (humanPlayer.identity == Identity.Spy && humanPlayer.isAlive);
                }
            }

            string winnerName = GetIdentityWinnerName(winningIdentity);
            string victoryText = LocalizationManager.Instance?.GetText("msg_victory") ?? "胜利!";
            string defeatText = LocalizationManager.Instance?.GetText("msg_defeat") ?? "失败...";
            string resultText = playerWins
                ? $"<color=#FFD700>{victoryText}</color>\n\n{winnerName}"
                : $"<color=#FF4444>{defeatText}</color>\n\n{winnerName}";

            text.text = resultText;
            UI.TMPFontHelper.SetFontByLanguage(text);

            // 显示所有玩家身份
            GameObject identityListObj = new GameObject("IdentityList");
            identityListObj.transform.SetParent(gameOverPanel.transform, false);

            var listRect = identityListObj.AddComponent<RectTransform>();
            listRect.anchorMin = new Vector2(0.5f, 0.3f);
            listRect.anchorMax = new Vector2(0.5f, 0.3f);
            listRect.sizeDelta = new Vector2(600, 150);

            var listText = identityListObj.AddComponent<TMPro.TextMeshProUGUI>();
            listText.fontSize = 24;
            listText.alignment = TMPro.TextAlignmentOptions.Center;
            listText.color = Color.white;

            string identityReveal = LocalizationManager.Instance?.GetText("identity_reveal") ?? "身份揭晓:";
            string deadStatus = LocalizationManager.Instance?.GetText("identity_dead") ?? "(阵亡)";
            string identityList = identityReveal + "\n";
            foreach (var player in players)
            {
                string identityName = GetIdentityDisplayName(player.identity);
                string status = player.isAlive ? "" : " " + deadStatus;
                identityList += $"{player.generalName}: {identityName}{status}\n";
            }
            listText.text = identityList;
            UI.TMPFontHelper.SetFontByLanguage(listText);
        }

        /// <summary>
        /// ⭐ 获取身份胜利方名称
        /// </summary>
        private string GetIdentityWinnerName(Identity identity)
        {
            switch (identity)
            {
                case Identity.Lord:
                case Identity.Loyalist:
                    return LocalizationManager.Instance?.GetText("identity_win_lord_loyalist") ?? "主公/忠臣获胜";
                case Identity.Rebel:
                    return LocalizationManager.Instance?.GetText("identity_win_rebel") ?? "反贼获胜";
                case Identity.Spy:
                    return LocalizationManager.Instance?.GetText("identity_win_spy") ?? "内奸获胜";
                default:
                    return LocalizationManager.Instance?.GetText("identity_none") ?? "未知";
            }
        }

        /// <summary>
        /// ⭐ 获取身份显示名称（带颜色）
        /// </summary>
        private string GetIdentityDisplayName(Identity identity)
        {
            string lord = LocalizationManager.Instance?.GetText("identity_lord") ?? "主公";
            string loyalist = LocalizationManager.Instance?.GetText("identity_loyalist") ?? "忠臣";
            string rebel = LocalizationManager.Instance?.GetText("identity_rebel") ?? "反贼";
            string spy = LocalizationManager.Instance?.GetText("identity_spy") ?? "内奸";
            string none = LocalizationManager.Instance?.GetText("identity_none") ?? "无";

            switch (identity)
            {
                case Identity.Lord: return $"<color=#FFD700>{lord}</color>";
                case Identity.Loyalist: return $"<color=#00FF00>{loyalist}</color>";
                case Identity.Rebel: return $"<color=#FF4444>{rebel}</color>";
                case Identity.Spy: return $"<color=#9966FF>{spy}</color>";
                default: return none;
            }
        }

        /// <summary>
        /// ⭐ 处理游戏结束
        /// </summary>
        private IEnumerator HandleGameEnd(Player winner)
        {
            // ⭐ 如果是故事模式，完全跳过BattleManager的游戏结束处理
            // StoryBattleManager会处理对话和场景跳转
            string storyBattleId = PlayerPrefs.GetString("StoryBattleId", "");
            bool isStoryMode = !string.IsNullOrEmpty(storyBattleId);

            var storyBattleManager = FindObjectOfType<StoryBattleManager>();
            if (storyBattleManager != null || isStoryMode)
            {
                Debug.Log($"[BattleManager] 故事模式战斗(StoryBattleId={storyBattleId})，由StoryBattleManager处理游戏结束流程");
                yield break; // StoryBattleManager会处理对话和场景跳转
            }

            // 显示游戏结束UI
            ShowGameOverUI(winner);

            // 等待一段时间让玩家看到结果
            yield return new WaitForSeconds(3f);

            // 根据模式返回不同场景
            NavigateAfterGameEnd();
        }

        /// <summary>
        /// ⭐ 显示游戏结束UI
        /// </summary>
        private void ShowGameOverUI(Player winner)
        {
            // 尝试找到或创建游戏结束面板
            GameObject gameOverPanel = GameObject.Find("GameOverPanel");
            if (gameOverPanel == null)
            {
                // 创建简单的游戏结束UI
                Canvas canvas = FindObjectOfType<Canvas>();
                if (canvas != null)
                {
                    gameOverPanel = new GameObject("GameOverPanel");
                    gameOverPanel.transform.SetParent(canvas.transform, false);

                    UnityEngine.UI.Image bg = gameOverPanel.AddComponent<UnityEngine.UI.Image>();
                    bg.color = new Color(0, 0, 0, 0.8f);

                    RectTransform rt = gameOverPanel.GetComponent<RectTransform>();
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;

                    // 添加文字
                    GameObject textObj = new GameObject("GameOverText");
                    textObj.transform.SetParent(gameOverPanel.transform, false);

                    TMPro.TextMeshProUGUI text = textObj.AddComponent<TMPro.TextMeshProUGUI>();

                    bool isPlayerWin = winner != null && !winner.isAI;
                    string resultText = isPlayerWin
                        ? (LocalizationManager.Instance?.GetText("msg_victory") ?? "胜利!")
                        : (LocalizationManager.Instance?.GetText("msg_defeat") ?? "失败...");

                    text.text = resultText;
                    text.fontSize = 72;
                    text.alignment = TMPro.TextAlignmentOptions.Center;
                    text.color = isPlayerWin ? Color.yellow : Color.red;

                    // 设置字体
                    UI.TMPFontHelper.SetFontByLanguage(text);

                    RectTransform textRt = textObj.GetComponent<RectTransform>();
                    textRt.anchorMin = new Vector2(0, 0.4f);
                    textRt.anchorMax = new Vector2(1, 0.6f);
                    textRt.offsetMin = Vector2.zero;
                    textRt.offsetMax = Vector2.zero;

                    // 添加提示文字
                    GameObject hintObj = new GameObject("HintText");
                    hintObj.transform.SetParent(gameOverPanel.transform, false);

                    TMPro.TextMeshProUGUI hintText = hintObj.AddComponent<TMPro.TextMeshProUGUI>();
                    hintText.text = LocalizationManager.Instance?.GetText("msg_returning") ?? "即将返回...";
                    hintText.fontSize = 24;
                    hintText.alignment = TMPro.TextAlignmentOptions.Center;
                    hintText.color = Color.white;
                    UI.TMPFontHelper.SetFontByLanguage(hintText);

                    RectTransform hintRt = hintObj.GetComponent<RectTransform>();
                    hintRt.anchorMin = new Vector2(0, 0.25f);
                    hintRt.anchorMax = new Vector2(1, 0.35f);
                    hintRt.offsetMin = Vector2.zero;
                    hintRt.offsetMax = Vector2.zero;
                }
            }
        }

        /// <summary>
        /// ⭐ 游戏结束后导航到适当场景
        /// </summary>
        private void NavigateAfterGameEnd()
        {
            Debug.Log("[BattleManager] NavigateAfterGameEnd 被调用!");
            Debug.Log($"[BattleManager] 调用堆栈:\n{System.Environment.StackTrace}");

            // 检查是否是故事模式
            string storyBattleId = PlayerPrefs.GetString("StoryBattleId", "");

            if (!string.IsNullOrEmpty(storyBattleId))
            {
                // ⭐ 故事模式：返回故事模式选择界面
                Debug.Log("[BattleManager] 故事模式战斗结束，返回StoryMode场景");

                // 清除故事模式标记
                PlayerPrefs.DeleteKey("StoryBattleId");
                PlayerPrefs.DeleteKey("StoryPlayerGeneral");
                PlayerPrefs.DeleteKey("StoryDifficulty");
                PlayerPrefs.Save();

                SceneManager.LoadScene("StoryMode");
            }
            else
            {
                // ⭐ 普通对战模式：返回主菜单
                Debug.Log("[BattleManager] 普通对战结束，返回MainMenu场景");
                SceneManager.LoadScene("MainMenu");
            }
        }

        /// <summary>
        /// 检查是否可以使用杀
        /// </summary>
        /// <param name="user">使用者</param>
        /// <param name="target">目标</param>
        /// <param name="errorMessage">错误信息输出</param>
        /// <returns>是否可以使用</returns>
        public bool CanUseSlash(Player user, Player target, out string errorMessage)
        {
            errorMessage = "";

            // 检查杀的使用次数
            if (!user.CanUseSlash())
            {
                errorMessage = "本回合已无法再使用杀";
                return false;
            }

            // 检查目标是否在攻击范围内
            if (!user.IsInAttackRange(target))
            {
                int distance = user.GetDistanceTo(target);
                int range = user.GetTotalAttackRange();
                errorMessage = $"目标不在攻击范围内（距离:{distance}, 攻击范围:{range}）";
                return false;
            }

            // 检查目标是否存活
            if (!target.isAlive)
            {
                errorMessage = "目标已阵亡";
                return false;
            }

            // ⭐ 检查空城技能：手牌为空时不能被杀指定
            if (IsKongchengActive(target))
            {
                errorMessage = $"{target.generalName} 发动【空城】，不能被【杀】指定";
                return false;
            }

            return true;
        }

        /// <summary>
        /// ⭐ 检查玩家是否处于空城状态
        /// </summary>
        private bool IsKongchengActive(Player player)
        {
            if (player == null || player.skills == null) return false;

            foreach (var skill in player.skills)
            {
                if (skill is DatabaseModule.Skills.Story.KongchengSkill kongcheng)
                {
                    if (kongcheng.IsKongchengActive())
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// ⭐ 检查玩家是否处于胆裂状态（不能使用或打出闪）
        /// </summary>
        private bool IsDanlieActive(Player player)
        {
            if (player == null || player.skills == null) return false;

            foreach (var skill in player.skills)
            {
                if (skill is DatabaseModule.Skills.Story.DanlieSkill danlie)
                {
                    if (danlie.IsDanlieActive())
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// ⭐ 计算杀造成的伤害（考虑技能修改）
        /// </summary>
        private int CalculateSlashDamage(Player user, Player target, Card slashCard)
        {
            int baseDamage = 1;
            int damageModifier = 0;

            if (user == null || user.skills == null)
            {
                return Mathf.Max(0, baseDamage);
            }

            // 检查攻击者的技能
            foreach (var skill in user.skills)
            {
                // 水战（蔡瑁）：杀的伤害+1
                if (skill is DatabaseModule.Skills.Story.ShuizhanSkill shuizhan)
                {
                    int bonus = shuizhan.GetSlashDamageBonus();
                    if (bonus > 0)
                    {
                        damageModifier += bonus;
                        Debug.Log($"[水战] {user.generalName} 的杀伤害+{bonus}");
                    }
                }

                // 伏击：对满血目标伤害+1
                if (skill is DatabaseModule.Skills.Story.FujiSkill fuji)
                {
                    int bonus = fuji.GetDamageBonus(target);
                    if (bonus > 0)
                    {
                        damageModifier += bonus;
                    }
                }

                // 冲阵：对已受伤目标伤害+1
                if (skill is DatabaseModule.Skills.Story.ChongzhenSkill chongzhen)
                {
                    int bonus = chongzhen.GetDamageBonus(target);
                    if (bonus > 0)
                    {
                        damageModifier += bonus;
                    }
                }

                // 胆裂（夏侯杰）：处于胆裂状态时伤害-1
                if (skill is DatabaseModule.Skills.Story.DanlieSkill danlie)
                {
                    if (danlie.IsDanlieActive())
                    {
                        damageModifier -= 1;
                        Debug.Log($"[胆裂] {user.generalName} 处于胆裂状态，伤害-1");
                    }
                }
            }

            int finalDamage = Mathf.Max(0, baseDamage + damageModifier);

            // ⭐ 故事模式：检查伤害翻倍规则（如吕布第一回合绝世武力）
            if (Story.StoryBattleManager.Instance != null && Story.StoryBattleManager.Instance.isBattleActive)
            {
                if (Story.StoryBattleManager.Instance.CheckDoubleDamage(user))
                {
                    finalDamage *= 2;
                    Debug.Log($"[绝世武力] {user.generalName} 的伤害翻倍: {finalDamage / 2} -> {finalDamage}");
                }
            }

            if (damageModifier != 0 || finalDamage != baseDamage)
            {
                Debug.Log($"[伤害计算] {user.generalName} 对 {target.generalName} 的杀伤害: 基础{baseDamage} + 修正{damageModifier} = {finalDamage}");
            }

            return finalDamage;
        }

        /// <summary>
        /// ⭐ 计算通用伤害（决斗/南蛮/万箭等，不含水战加成）
        /// </summary>
        private int CalculateGeneralDamage(Player source, Player target, int baseDamage)
        {
            int damageModifier = 0;

            if (source == null || source.skills == null)
            {
                return Mathf.Max(0, baseDamage);
            }

            foreach (var skill in source.skills)
            {
                // 伏击：对满血目标伤害+1
                if (skill is DatabaseModule.Skills.Story.FujiSkill fuji)
                {
                    int bonus = fuji.GetDamageBonus(target);
                    if (bonus > 0)
                    {
                        damageModifier += bonus;
                    }
                }

                // 冲阵：对已受伤目标伤害+1
                if (skill is DatabaseModule.Skills.Story.ChongzhenSkill chongzhen)
                {
                    int bonus = chongzhen.GetDamageBonus(target);
                    if (bonus > 0)
                    {
                        damageModifier += bonus;
                    }
                }

                // 胆裂：处于胆裂状态时伤害-1
                if (skill is DatabaseModule.Skills.Story.DanlieSkill danlie)
                {
                    if (danlie.IsDanlieActive())
                    {
                        damageModifier -= 1;
                        Debug.Log($"[胆裂] {source.generalName} 处于胆裂状态，伤害-1");
                    }
                }
            }

            int finalDamage = Mathf.Max(0, baseDamage + damageModifier);
            if (damageModifier != 0)
            {
                Debug.Log($"[伤害计算] {source.generalName} 对 {target.generalName} 的伤害: 基础{baseDamage} + 修正{damageModifier} = {finalDamage}");
            }

            return finalDamage;
        }

        /// <summary>
        /// 使用【杀】
        /// </summary>
        public void UseSlash(Player user, Player target, Card slashCard)
        {
            // 检查是否可以使用杀
            if (!CanUseSlash(user, target, out string errorMessage))
            {
                Debug.LogWarning($"无法使用杀: {errorMessage}");
                if (UI.BattleUI.Instance != null)
                {
                    UI.BattleUI.Instance.ShowMessage(errorMessage);
                }
                return;
            }

            if (!user.PlayCard(slashCard))
            {
                Debug.LogWarning("无法打出此牌!");
                return;
            }

            // ⭐ 增加杀的使用计数
            user.UseSlash();

            Debug.Log($"{user.playerName} 对 {target.playerName} 使用了【杀】");

            // ⭐ 显示出牌动画（从使用者位置飞出）
            if (UI.PlayedCardDisplayManager.Instance != null)
            {
                Vector3? startPos = GetPlayerUIPosition(user);
                UI.PlayedCardDisplayManager.Instance.ShowPlayedCard(slashCard, user, startPos);
            }

            // 先将杀牌放入弃牌堆
            DeckManager.Instance.DiscardCard(slashCard);

            // 使用协程进行异步响应
            StartCoroutine(ProcessSlashResponse(user, target, slashCard));
        }

        /// <summary>
        /// 处理杀的响应（协程）
        /// </summary>
        private IEnumerator ProcessSlashResponse(Player user, Player target, Card slashCard)
        {
            bool responseReceived = false;
            bool dodged = false;

            // 请求目标玩家响应
            if (UI.BattleUI.Instance != null)
            {
                UI.BattleUI.Instance.RequestResponse(target, UI.ResponseType.Dodge, (responseCard) =>
                {
                    responseReceived = true;
                    dodged = responseCard != null;

                    if (dodged)
                    {
                        Debug.Log($"{target.playerName} 打出了【闪】，闪避成功");
                    }
                    else
                    {
                        Debug.Log($"{target.playerName} 没有出【闪】");
                    }
                });

                // 等待响应
                while (!responseReceived)
                {
                    yield return null;
                }
            }
            else
            {
                // 如果没有UI，使用旧的自动逻辑
                dodged = AutoCheckForDodge(target);
            }

            // 处理结果
            if (!dodged)
            {
                // ⭐ 计算技能修正后的伤害
                int damage = CalculateSlashDamage(user, target, slashCard);

                if (damage > 0)
                {
                    target.TakeDamage(damage, user);

                    if (EventManager.Instance != null)
                    {
                        EventManager.Instance.TriggerPlayerDamaged(target, user, damage, slashCard);
                    }
                }
                else
                {
                    Debug.Log($"[伤害计算] {user.generalName} 对 {target.generalName} 的伤害被减免为0");
                }

                // ⭐ 赤壁之战：虎威效果（张飞杀命中30%令目标弃牌）
                if (Story.StoryBattleManager.Instance != null)
                {
                    Story.StoryBattleManager.Instance.TryTriggerHuwei(user, target);
                }
            }

            // 更新UI
            UpdateUI();
        }

        /// <summary>
        /// 自动检查是否有闪（AI用或无UI时）
        /// </summary>
        private bool AutoCheckForDodge(Player player)
        {
            // ⭐ 胆裂检查：处于胆裂状态不能使用或打出闪
            if (IsDanlieActive(player))
            {
                Debug.Log($"[胆裂] {player.generalName} 处于胆裂状态，不能使用或打出【闪】");
                return false;
            }

            foreach (var card in player.handCards)
            {
                if (card.cardName == "闪")
                {
                    player.PlayCard(card);
                    DeckManager.Instance.DiscardCard(card);
                    Debug.Log($"{player.playerName} 自动打出了【闪】");

                    // ⭐ 显示响应牌动画
                    if (UI.PlayedCardDisplayManager.Instance != null)
                    {
                        Vector3? startPos = GetPlayerUIPosition(player);
                        UI.PlayedCardDisplayManager.Instance.ShowPlayedCard(card, player, startPos);
                    }

                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 自动检查是否有杀（AI用或无UI时）
        /// </summary>
        private bool AutoCheckForSlash(Player player)
        {
            foreach (var card in player.handCards)
            {
                if (card.cardName == "杀")
                {
                    player.PlayCard(card);
                    DeckManager.Instance.DiscardCard(card);
                    Debug.Log($"{player.playerName} 自动打出了【杀】");

                    // ⭐ 显示响应牌动画
                    if (UI.PlayedCardDisplayManager.Instance != null)
                    {
                        Vector3? startPos = GetPlayerUIPosition(player);
                        UI.PlayedCardDisplayManager.Instance.ShowPlayedCard(card, player, startPos);
                    }

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

            // ⭐ 显示出牌动画（从使用者位置飞出）
            if (UI.PlayedCardDisplayManager.Instance != null)
            {
                Vector3? startPos = GetPlayerUIPosition(user);
                UI.PlayedCardDisplayManager.Instance.ShowPlayedCard(peachCard, user, startPos);
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
            // ⭐ 检查空城技能：手牌为空时不能被决斗指定
            if (IsKongchengActive(target))
            {
                Debug.Log($"[决斗] {target.generalName} 发动【空城】，不能被【决斗】指定");
                if (UI.BattleUI.Instance != null)
                {
                    UI.BattleUI.Instance.ShowMessage($"{target.generalName} 发动【空城】，不能被【决斗】指定");
                }
                return;
            }

            if (!user.PlayCard(card))
            {
                Debug.LogWarning("无法打出此牌!");
                return;
            }

            Debug.Log($"{user.playerName} 对 {target.playerName} 使用了【决斗】");

            // ⭐ 显示出牌动画（从使用者位置飞出）
            if (UI.PlayedCardDisplayManager.Instance != null)
            {
                Vector3? startPos = GetPlayerUIPosition(user);
                UI.PlayedCardDisplayManager.Instance.ShowPlayedCard(card, user, startPos);
            }

            DeckManager.Instance.DiscardCard(card);

            // 触发使用卡牌事件
            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerCardUsed(user, card, target);
            }

            // 使用协程处理决斗（支持无懈可击）
            StartCoroutine(ProcessDuel(user, target, card));
        }

        /// <summary>
        /// ⭐ 处理决斗流程（协程，支持无懈可击）
        /// </summary>
        private IEnumerator ProcessDuel(Player user, Player target, Card card)
        {
            // ⭐ 询问无懈可击
            bool nullified = false;
            yield return StartCoroutine(RequestNullification(user, card, target, (result) =>
            {
                nullified = result;
            }));

            if (nullified)
            {
                Debug.Log($"[决斗] 【决斗】被无懈可击抵消");
                UpdateUI();
                yield break;
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

                // 显示出牌动画
                if (UI.PlayedCardDisplayManager.Instance != null)
                {
                    Vector3? startPos = GetPlayerUIPosition(currentResponder);
                    UI.PlayedCardDisplayManager.Instance.ShowPlayedCard(slashCard, currentResponder, startPos);
                }

                yield return new WaitForSeconds(0.3f);

                // 交换响应者
                Player temp = currentResponder;
                currentResponder = opponent;
                opponent = temp;
            }

            // 决定谁受伤
            Player loser = targetLost ? target : user;
            // ⭐ 计算技能修正后的伤害
            Player winner = loser == target ? user : target;
            int damage = CalculateGeneralDamage(winner, loser, 1);
            Debug.Log($"[决斗] {loser.playerName} 决斗失败，受到{damage}点伤害");

            if (damage > 0)
            {
                loser.TakeDamage(damage, winner);

                // 触发受伤事件
                if (EventManager.Instance != null)
                {
                    EventManager.Instance.TriggerPlayerDamaged(loser, winner, damage, card);
                }
            }
            else
            {
                Debug.Log($"[伤害计算] {winner.generalName} 对 {loser.generalName} 的伤害被减免为0");
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

            // ⭐ 显示出牌动画
            if (UI.PlayedCardDisplayManager.Instance != null)
            {
                Vector3? startPos = GetPlayerUIPosition(user);
                UI.PlayedCardDisplayManager.Instance.ShowPlayedCard(card, user, startPos);
            }

            DeckManager.Instance.DiscardCard(card);

            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerCardUsed(user, card, target);
            }

            // 使用协程处理（支持无懈可击）
            StartCoroutine(ProcessSnatch(user, target, card));
        }

        /// <summary>
        /// ⭐ 处理顺手牵羊流程（协程，支持无懈可击）
        /// </summary>
        private IEnumerator ProcessSnatch(Player user, Player target, Card card)
        {
            // ⭐ 询问无懈可击
            bool nullified = false;
            yield return StartCoroutine(RequestNullification(user, card, target, (result) =>
            {
                nullified = result;
            }));

            if (nullified)
            {
                Debug.Log($"[顺手牵羊] 【顺手牵羊】被无懈可击抵消");
                UpdateUI();
                yield break;
            }

            if (target.handCards.Count == 0)
            {
                Debug.Log($"[顺手牵羊] {target.playerName} 没有手牌");
                if (UI.BattleUI.Instance != null)
                {
                    UI.BattleUI.Instance.AddLog($"{target.playerName} 没有手牌");
                }
                UpdateUI();
                yield break;
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

            // ⭐ 显示出牌动画
            if (UI.PlayedCardDisplayManager.Instance != null)
            {
                Vector3? startPos = GetPlayerUIPosition(user);
                UI.PlayedCardDisplayManager.Instance.ShowPlayedCard(card, user, startPos);
            }

            DeckManager.Instance.DiscardCard(card);

            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerCardUsed(user, card, target);
            }

            // 使用协程处理（支持无懈可击）
            StartCoroutine(ProcessDismantlement(user, target, card));
        }

        /// <summary>
        /// ⭐ 处理过河拆桥流程（协程，支持无懈可击）
        /// </summary>
        private IEnumerator ProcessDismantlement(Player user, Player target, Card card)
        {
            // ⭐ 询问无懈可击
            bool nullified = false;
            yield return StartCoroutine(RequestNullification(user, card, target, (result) =>
            {
                nullified = result;
            }));

            if (nullified)
            {
                Debug.Log($"[过河拆桥] 【过河拆桥】被无懈可击抵消");
                UpdateUI();
                yield break;
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
                yield break;
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

            // ⭐ 显示出牌动画（从使用者位置飞出）
            if (UI.PlayedCardDisplayManager.Instance != null)
            {
                Vector3? startPos = GetPlayerUIPosition(user);
                UI.PlayedCardDisplayManager.Instance.ShowPlayedCard(card, user, startPos);
            }

            DeckManager.Instance.DiscardCard(card);

            // 触发使用卡牌事件
            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerCardUsed(user, card, null);
            }

            // 使用协程处理每个玩家的响应
            StartCoroutine(ProcessSavageAssaultResponses(user, card));
        }

        /// <summary>
        /// 处理南蛮入侵的响应（协程，支持无懈可击）
        /// </summary>
        private IEnumerator ProcessSavageAssaultResponses(Player user, Card card)
        {
            foreach (var player in players)
            {
                if (player == user || !player.isAlive) continue;

                // ⭐ 对每个目标询问无懈可击
                bool nullified = false;
                yield return StartCoroutine(RequestNullification(user, card, player, (result) =>
                {
                    nullified = result;
                }));

                if (nullified)
                {
                    Debug.Log($"[南蛮入侵] 对 {player.playerName} 的效果被无懈可击抵消");
                    yield return new WaitForSeconds(0.3f);
                    continue;
                }

                Debug.Log($"[南蛮入侵] {player.playerName} 需要打出【杀】");

                bool responseReceived = false;
                bool hasSlash = false;

                // 请求玩家响应
                if (UI.BattleUI.Instance != null && !player.isAI)
                {
                    UI.BattleUI.Instance.RequestResponse(player, UI.ResponseType.Slash, (responseCard) =>
                    {
                        responseReceived = true;
                        hasSlash = responseCard != null;
                    });

                    while (!responseReceived)
                    {
                        yield return null;
                    }
                }
                else
                {
                    // AI自动响应
                    hasSlash = AutoCheckForSlash(player);
                    responseReceived = true;
                }

                if (hasSlash)
                {
                    Debug.Log($"[南蛮入侵] {player.playerName} 打出了【杀】，免疫伤害");
                }
                else
                {
                    // ⭐ 计算技能修正后的伤害
                    int damage = CalculateGeneralDamage(user, player, 1);
                    Debug.Log($"[南蛮入侵] {player.playerName} 没有【杀】，受到{damage}点伤害");

                    if (damage > 0)
                    {
                        player.TakeDamage(damage, user);

                        if (EventManager.Instance != null)
                        {
                            EventManager.Instance.TriggerPlayerDamaged(player, user, damage, card);
                        }
                    }
                    else
                    {
                        Debug.Log($"[伤害计算] {user.generalName} 对 {player.generalName} 的伤害被减免为0");
                    }
                }

                // 短暂延迟，让玩家看到结果
                yield return new WaitForSeconds(0.5f);
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

            // ⭐ 显示出牌动画（从使用者位置飞出）
            if (UI.PlayedCardDisplayManager.Instance != null)
            {
                Vector3? startPos = GetPlayerUIPosition(user);
                UI.PlayedCardDisplayManager.Instance.ShowPlayedCard(card, user, startPos);
            }

            DeckManager.Instance.DiscardCard(card);

            // 触发使用卡牌事件
            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerCardUsed(user, card, null);
            }

            // 使用协程处理每个玩家的响应
            StartCoroutine(ProcessArrowBarrageResponses(user, card));
        }

        /// <summary>
        /// 处理万箭齐发的响应（协程，支持无懈可击）
        /// </summary>
        private IEnumerator ProcessArrowBarrageResponses(Player user, Card card)
        {
            foreach (var player in players)
            {
                if (player == user || !player.isAlive) continue;

                // ⭐ 对每个目标询问无懈可击
                bool nullified = false;
                yield return StartCoroutine(RequestNullification(user, card, player, (result) =>
                {
                    nullified = result;
                }));

                if (nullified)
                {
                    Debug.Log($"[万箭齐发] 对 {player.playerName} 的效果被无懈可击抵消");
                    yield return new WaitForSeconds(0.3f);
                    continue;
                }

                Debug.Log($"[万箭齐发] {player.playerName} 需要打出【闪】");

                bool responseReceived = false;
                bool hasDodge = false;

                // 请求玩家响应
                if (UI.BattleUI.Instance != null && !player.isAI)
                {
                    UI.BattleUI.Instance.RequestResponse(player, UI.ResponseType.Dodge, (responseCard) =>
                    {
                        responseReceived = true;
                        hasDodge = responseCard != null;
                    });

                    while (!responseReceived)
                    {
                        yield return null;
                    }
                }
                else
                {
                    // AI自动响应
                    hasDodge = AutoCheckForDodge(player);
                    responseReceived = true;
                }

                if (hasDodge)
                {
                    Debug.Log($"[万箭齐发] {player.playerName} 打出了【闪】，免疫伤害");
                }
                else
                {
                    // ⭐ 计算技能修正后的伤害
                    int damage = CalculateGeneralDamage(user, player, 1);
                    Debug.Log($"[万箭齐发] {player.playerName} 没有【闪】，受到{damage}点伤害");

                    if (damage > 0)
                    {
                        player.TakeDamage(damage, user);

                        if (EventManager.Instance != null)
                        {
                            EventManager.Instance.TriggerPlayerDamaged(player, user, damage, card);
                        }
                    }
                    else
                    {
                        Debug.Log($"[伤害计算] {user.generalName} 对 {player.generalName} 的伤害被减免为0");
                    }
                }

                // 短暂延迟，让玩家看到结果
                yield return new WaitForSeconds(0.5f);
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

        #region 延时锦囊

        /// <summary>
        /// ⭐ 使用【乐不思蜀】- 放入目标判定区
        /// </summary>
        public void UseIndulgence(Player user, Player target, Card card)
        {
            if (!user.PlayCard(card))
            {
                Debug.LogWarning("无法打出此牌!");
                return;
            }

            // 检查目标判定区是否已有乐不思蜀
            foreach (var judgeCard in target.judgeCards)
            {
                if (judgeCard.cardName == "乐不思蜀")
                {
                    Debug.Log($"[乐不思蜀] {target.playerName} 判定区已有乐不思蜀");
                    if (UI.BattleUI.Instance != null)
                    {
                        UI.BattleUI.Instance.AddLog($"{target.playerName} 判定区已有乐不思蜀");
                    }
                    // 退回手牌
                    user.handCards.Add(card);
                    UpdateUI();
                    return;
                }
            }

            Debug.Log($"{user.playerName} 对 {target.playerName} 使用了【乐不思蜀】");

            // 显示出牌动画
            if (UI.PlayedCardDisplayManager.Instance != null)
            {
                Vector3? startPos = GetPlayerUIPosition(user);
                UI.PlayedCardDisplayManager.Instance.ShowPlayedCard(card, user, startPos);
            }

            // 放入目标判定区
            target.judgeCards.Add(card);

            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerCardUsed(user, card, target);
            }

            if (UI.BattleUI.Instance != null)
            {
                UI.BattleUI.Instance.AddLog($"{user.playerName} 对 {target.playerName} 使用了【乐不思蜀】");
            }

            UpdateUI();
        }

        /// <summary>
        /// ⭐ 使用【闪电】- 放入自己判定区
        /// </summary>
        public void UseLightning(Player user, Card card)
        {
            if (!user.PlayCard(card))
            {
                Debug.LogWarning("无法打出此牌!");
                return;
            }

            // 检查自己判定区是否已有闪电
            foreach (var judgeCard in user.judgeCards)
            {
                if (judgeCard.cardName == "闪电")
                {
                    Debug.Log($"[闪电] {user.playerName} 判定区已有闪电");
                    if (UI.BattleUI.Instance != null)
                    {
                        UI.BattleUI.Instance.AddLog($"{user.playerName} 判定区已有闪电");
                    }
                    // 退回手牌
                    user.handCards.Add(card);
                    UpdateUI();
                    return;
                }
            }

            Debug.Log($"{user.playerName} 使用了【闪电】");

            // 显示出牌动画
            if (UI.PlayedCardDisplayManager.Instance != null)
            {
                Vector3? startPos = GetPlayerUIPosition(user);
                UI.PlayedCardDisplayManager.Instance.ShowPlayedCard(card, user, startPos);
            }

            // 放入自己判定区
            user.judgeCards.Add(card);

            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerCardUsed(user, card, null);
            }

            if (UI.BattleUI.Instance != null)
            {
                UI.BattleUI.Instance.AddLog($"{user.playerName} 使用了【闪电】");
            }

            UpdateUI();
        }

        /// <summary>
        /// ⭐ 使用【兵粮寸断】- 放入距离1的目标判定区
        /// </summary>
        public void UseSupplyShortage(Player user, Player target, Card card)
        {
            if (!user.PlayCard(card))
            {
                Debug.LogWarning("无法打出此牌!");
                return;
            }

            // 检查距离（兵粮寸断只能对距离1的角色使用）
            int distance = user.GetDistanceTo(target);
            if (distance > 1)
            {
                Debug.Log($"[兵粮寸断] {target.playerName} 距离过远（距离={distance}）");
                if (UI.BattleUI.Instance != null)
                {
                    UI.BattleUI.Instance.AddLog($"目标距离过远");
                }
                // 退回手牌
                user.handCards.Add(card);
                UpdateUI();
                return;
            }

            // 检查目标判定区是否已有兵粮寸断
            foreach (var judgeCard in target.judgeCards)
            {
                if (judgeCard.cardName == "兵粮寸断")
                {
                    Debug.Log($"[兵粮寸断] {target.playerName} 判定区已有兵粮寸断");
                    if (UI.BattleUI.Instance != null)
                    {
                        UI.BattleUI.Instance.AddLog($"{target.playerName} 判定区已有兵粮寸断");
                    }
                    // 退回手牌
                    user.handCards.Add(card);
                    UpdateUI();
                    return;
                }
            }

            Debug.Log($"{user.playerName} 对 {target.playerName} 使用了【兵粮寸断】");

            // 显示出牌动画
            if (UI.PlayedCardDisplayManager.Instance != null)
            {
                Vector3? startPos = GetPlayerUIPosition(user);
                UI.PlayedCardDisplayManager.Instance.ShowPlayedCard(card, user, startPos);
            }

            // 放入目标判定区
            target.judgeCards.Add(card);

            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerCardUsed(user, card, target);
            }

            if (UI.BattleUI.Instance != null)
            {
                UI.BattleUI.Instance.AddLog($"{user.playerName} 对 {target.playerName} 使用了【兵粮寸断】");
            }

            UpdateUI();
        }

        #endregion

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

        /// <summary>
        /// ⭐ 获取玩家UI的屏幕位置（用于出牌动画起点）
        /// </summary>
        private Vector3? GetPlayerUIPosition(Player player)
        {
            if (UI.BattleUI.Instance != null)
            {
                return UI.BattleUI.Instance.GetPlayerScreenPosition(player);
            }
            return null;
        }

        #region 濒死处理

        /// <summary>
        /// ⭐ 处理濒死求桃流程
        /// </summary>
        public IEnumerator ProcessNearDeath(Player dyingPlayer, Player killer)
        {
            Debug.Log($"[濒死] {dyingPlayer.playerName} 进入濒死状态，需要 {dyingPlayer.GetPeachesNeeded()} 张桃");

            // 添加濒死日志
            if (UI.BattleUI.Instance != null)
            {
                UI.BattleUI.Instance.AddLog($"{dyingPlayer.playerName} 进入濒死状态！");
            }

            // 循环直到脱离濒死或确认死亡
            while (dyingPlayer.isNearDeath && dyingPlayer.currentHP <= 0)
            {
                bool saved = false;

                // 从濒死玩家开始，按座位顺序询问每个玩家
                int startIndex = players.IndexOf(dyingPlayer);
                if (startIndex < 0) startIndex = 0;

                for (int i = 0; i < players.Count; i++)
                {
                    int playerIndex = (startIndex + i) % players.Count;
                    Player askPlayer = players[playerIndex];

                    if (!askPlayer.isAlive) continue;

                    // 检查该玩家是否有桃
                    bool hasPeach = HasPeachCard(askPlayer);
                    if (!hasPeach) continue;

                    Debug.Log($"[濒死] 询问 {askPlayer.playerName} 是否使用【桃】救 {dyingPlayer.playerName}");

                    bool responseReceived = false;
                    bool usedPeach = false;

                    if (askPlayer.isAI)
                    {
                        // AI决策：是否救人
                        usedPeach = AIDecideSaveDyingPlayer(askPlayer, dyingPlayer);
                        responseReceived = true;

                        if (usedPeach)
                        {
                            Card peachCard = FindPeachCard(askPlayer);
                            if (peachCard != null)
                            {
                                askPlayer.PlayCard(peachCard);
                                DeckManager.Instance.DiscardCard(peachCard);

                                // 显示出牌动画
                                if (UI.PlayedCardDisplayManager.Instance != null)
                                {
                                    Vector3? startPos = GetPlayerUIPosition(askPlayer);
                                    UI.PlayedCardDisplayManager.Instance.ShowPlayedCard(peachCard, askPlayer, startPos);
                                }

                                Debug.Log($"[濒死] {askPlayer.playerName} 使用【桃】救了 {dyingPlayer.playerName}");

                                if (UI.BattleUI.Instance != null)
                                {
                                    UI.BattleUI.Instance.AddLog($"{askPlayer.playerName} 使用【桃】救了 {dyingPlayer.playerName}");
                                }
                            }
                        }

                        yield return new WaitForSeconds(0.5f);
                    }
                    else
                    {
                        // 人类玩家，显示求桃界面
                        if (UI.BattleUI.Instance != null)
                        {
                            // 设置濒死目标信息
                            UI.BattleUI.Instance.SetNearDeathTarget(dyingPlayer);

                            UI.BattleUI.Instance.RequestResponse(askPlayer, UI.ResponseType.Peach, (responseCard) =>
                            {
                                responseReceived = true;
                                usedPeach = responseCard != null;
                            });

                            // 等待响应
                            while (!responseReceived)
                            {
                                yield return null;
                            }

                            // 清除濒死目标
                            UI.BattleUI.Instance.SetNearDeathTarget(null);
                        }
                        else
                        {
                            // 无UI时自动决策
                            usedPeach = AutoCheckForPeach(askPlayer);
                            responseReceived = true;
                        }
                    }

                    if (usedPeach)
                    {
                        dyingPlayer.SaveFromNearDeath(askPlayer);
                        saved = true;

                        // 如果还需要更多桃（HP仍<=0），继续循环
                        if (dyingPlayer.currentHP > 0)
                        {
                            break; // 已救活，退出询问循环
                        }
                    }
                }

                // 如果这一轮没人救，玩家死亡
                if (!saved || dyingPlayer.currentHP <= 0)
                {
                    if (dyingPlayer.currentHP <= 0)
                    {
                        Debug.Log($"[濒死] 无人救援，{dyingPlayer.playerName} 死亡");
                        if (UI.BattleUI.Instance != null)
                        {
                            UI.BattleUI.Instance.AddLog($"无人救援，{dyingPlayer.playerName} 阵亡！");
                        }
                        dyingPlayer.ExecuteDeath(killer);
                    }
                    break;
                }
            }

            // 更新UI
            UpdateUI();
        }

        /// <summary>
        /// ⭐ 检查玩家是否有桃
        /// </summary>
        private bool HasPeachCard(Player player)
        {
            foreach (var card in player.handCards)
            {
                if (card.cardName == "桃")
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// ⭐ 查找玩家手牌中的桃
        /// </summary>
        private Card FindPeachCard(Player player)
        {
            foreach (var card in player.handCards)
            {
                if (card.cardName == "桃")
                {
                    return card;
                }
            }
            return null;
        }

        /// <summary>
        /// ⭐ AI决定是否救濒死玩家
        /// </summary>
        private bool AIDecideSaveDyingPlayer(Player aiPlayer, Player dyingPlayer)
        {
            // 检查是否是自己濒死
            if (aiPlayer == dyingPlayer)
            {
                // 自己濒死，一定救
                return true;
            }

            // 检查是否是盟友
            bool isAlly = false;

            // 优先使用故事模式的盟友判断
            if (StoryBattleManager.Instance != null && StoryBattleManager.Instance.isBattleActive)
            {
                isAlly = StoryBattleManager.Instance.IsAlly(aiPlayer, dyingPlayer);

                // 检查是否有禁止对盟友用桃的规则
                if (isAlly && !StoryBattleManager.Instance.CanUsePeachOn(aiPlayer, dyingPlayer))
                {
                    Debug.Log($"[AI] {aiPlayer.playerName} 因规则限制无法对盟友 {dyingPlayer.playerName} 使用桃");
                    return false;
                }
            }
            else
            {
                // 普通模式：同阵营是盟友
                isAlly = aiPlayer.faction == dyingPlayer.faction;
            }

            if (isAlly)
            {
                // 盟友濒死，80%概率救（考虑保留桃给自己）
                bool shouldSave = Random.value < 0.8f;
                Debug.Log($"[AI] {aiPlayer.playerName} {(shouldSave ? "决定救" : "决定不救")} 盟友 {dyingPlayer.playerName}");
                return shouldSave;
            }
            else
            {
                // 敌人濒死，不救
                Debug.Log($"[AI] {aiPlayer.playerName} 不救敌人 {dyingPlayer.playerName}");
                return false;
            }
        }

        /// <summary>
        /// ⭐ 自动检查并使用桃（无UI时的备用逻辑）
        /// </summary>
        private bool AutoCheckForPeach(Player player)
        {
            Card peach = FindPeachCard(player);
            if (peach != null)
            {
                player.PlayCard(peach);
                DeckManager.Instance.DiscardCard(peach);
                Debug.Log($"{player.playerName} 自动使用了【桃】");

                if (UI.PlayedCardDisplayManager.Instance != null)
                {
                    Vector3? startPos = GetPlayerUIPosition(player);
                    UI.PlayedCardDisplayManager.Instance.ShowPlayedCard(peach, player, startPos);
                }

                return true;
            }
            return false;
        }

        #endregion

        #region 无懈可击系统

        /// <summary>
        /// ⭐ 询问是否有人使用无懈可击（协程）
        /// </summary>
        /// <param name="trickUser">锦囊使用者</param>
        /// <param name="trickCard">锦囊牌</param>
        /// <param name="target">锦囊目标(可为null，如南蛮入侵等群体锦囊对单个目标结算时传入当前目标)</param>
        /// <param name="callback">回调，true表示锦囊被抵消</param>
        public IEnumerator RequestNullification(Player trickUser, Card trickCard, Player target, System.Action<bool> callback)
        {
            bool nullified = false;
            yield return StartCoroutine(ProcessNullificationChain(trickUser, trickCard, target, false, (result) =>
            {
                nullified = result;
            }));
            callback?.Invoke(nullified);
        }

        /// <summary>
        /// ⭐ 处理无懈可击链（协程）
        /// </summary>
        /// <param name="trickUser">原锦囊使用者</param>
        /// <param name="trickCard">原锦囊牌</param>
        /// <param name="target">锦囊目标</param>
        /// <param name="isCountering">是否在反制无懈可击</param>
        /// <param name="callback">回调</param>
        private IEnumerator ProcessNullificationChain(Player trickUser, Card trickCard, Player target, bool isCountering, System.Action<bool> callback)
        {
            // 从锦囊使用者位置开始，按座位顺序询问
            int startIndex = players.IndexOf(trickUser);
            if (startIndex < 0) startIndex = 0;

            for (int i = 0; i < players.Count; i++)
            {
                int playerIndex = (startIndex + i) % players.Count;
                Player askPlayer = players[playerIndex];

                if (!askPlayer.isAlive) continue;

                // 检查是否有无懈可击
                if (!HasNullificationCard(askPlayer)) continue;

                // AI决策或玩家选择
                bool wantsToNullify = false;
                bool responseReceived = false;

                if (askPlayer.isAI)
                {
                    // AI决策是否使用无懈可击
                    wantsToNullify = AIDecideNullify(askPlayer, trickUser, trickCard, target, isCountering);
                    responseReceived = true;

                    if (wantsToNullify)
                    {
                        // AI使用无懈可击
                        Card nullifyCard = FindNullificationCard(askPlayer);
                        if (nullifyCard != null)
                        {
                            askPlayer.PlayCard(nullifyCard);
                            DeckManager.Instance.DiscardCard(nullifyCard);

                            // 显示出牌动画
                            if (UI.PlayedCardDisplayManager.Instance != null)
                            {
                                Vector3? startPos = GetPlayerUIPosition(askPlayer);
                                UI.PlayedCardDisplayManager.Instance.ShowPlayedCard(nullifyCard, askPlayer, startPos);
                            }

                            string cardName = CardNameHelper.GetLocalizedCardName(trickCard.cardName);
                            Debug.Log($"[无懈可击] {askPlayer.playerName} 使用【无懈可击】{(isCountering ? "反制" : "抵消")}【{cardName}】");

                            if (UI.BattleUI.Instance != null)
                            {
                                if (isCountering)
                                {
                                    UI.BattleUI.Instance.AddLocalizedLog("msg_nullify_counter", askPlayer.playerName);
                                }
                                else
                                {
                                    UI.BattleUI.Instance.AddLocalizedLog("msg_nullify_trick", askPlayer.playerName, cardName);
                                }
                            }
                        }
                    }

                    yield return new WaitForSeconds(0.3f);
                }
                else
                {
                    // 人类玩家，显示无懈可击选项
                    if (UI.BattleUI.Instance != null)
                    {
                        UI.BattleUI.Instance.RequestResponse(askPlayer, UI.ResponseType.Nullify, (responseCard) =>
                        {
                            responseReceived = true;
                            wantsToNullify = responseCard != null;
                        });

                        while (!responseReceived)
                        {
                            yield return null;
                        }
                    }
                    else
                    {
                        responseReceived = true;
                        wantsToNullify = false;
                    }
                }

                if (wantsToNullify)
                {
                    // 有人使用无懈可击，询问是否有人反制
                    bool counterNullified = false;
                    yield return StartCoroutine(ProcessNullificationChain(askPlayer, trickCard, target, !isCountering, (result) =>
                    {
                        counterNullified = result;
                    }));

                    // 如果反制成功，则本次无懈可击无效，继续询问其他玩家
                    if (counterNullified)
                    {
                        Debug.Log($"[无懈可击] {askPlayer.playerName} 的无懈可击被反制");
                        continue;
                    }
                    else
                    {
                        // 无懈可击生效
                        callback?.Invoke(true);
                        yield break;
                    }
                }
            }

            // 没有人使用无懈可击
            callback?.Invoke(false);
        }

        /// <summary>
        /// ⭐ 检查玩家是否有无懈可击
        /// </summary>
        private bool HasNullificationCard(Player player)
        {
            foreach (var card in player.handCards)
            {
                if (card.cardName == "无懈可击")
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// ⭐ 查找玩家手牌中的无懈可击
        /// </summary>
        private Card FindNullificationCard(Player player)
        {
            foreach (var card in player.handCards)
            {
                if (card.cardName == "无懈可击")
                {
                    return card;
                }
            }
            return null;
        }

        /// <summary>
        /// ⭐ AI决定是否使用无懈可击
        /// </summary>
        /// <param name="aiPlayer">AI玩家</param>
        /// <param name="trickUser">锦囊使用者</param>
        /// <param name="trickCard">锦囊牌</param>
        /// <param name="target">锦囊目标</param>
        /// <param name="isCountering">是否在反制无懈可击</param>
        private bool AIDecideNullify(Player aiPlayer, Player trickUser, Card trickCard, Player target, bool isCountering)
        {
            // 判断锦囊使用者和目标与AI的关系
            bool userIsAlly = IsAllyForAI(aiPlayer, trickUser);
            bool targetIsAlly = target != null && IsAllyForAI(aiPlayer, target);
            bool targetIsSelf = target == aiPlayer;

            // 判断锦囊类型
            bool isHarmfulTrick = IsHarmfulTrick(trickCard.cardName);
            bool isBeneficialTrick = IsBeneficialTrick(trickCard.cardName);

            // 如果是反制阶段
            if (isCountering)
            {
                // 反制逻辑：如果原无懈可击会伤害盟友，则反制
                // 这里简化为：如果原锦囊对自己或盟友有害，且有人想抵消，我们不反制
                // 如果原锦囊对敌人有害，我们反制别人的无懈可击
                if (isHarmfulTrick)
                {
                    // 原锦囊是伤害性的
                    if (targetIsSelf || targetIsAlly)
                    {
                        // 目标是自己或盟友，不反制（让无懈可击生效保护自己）
                        return false;
                    }
                    else
                    {
                        // 目标是敌人，反制（让伤害锦囊生效）
                        return Random.value < 0.6f; // 60%概率反制
                    }
                }
                else if (isBeneficialTrick)
                {
                    // 原锦囊是有益的
                    if (userIsAlly)
                    {
                        // 盟友使用的有益锦囊被无懈，反制保护盟友的锦囊
                        return Random.value < 0.5f;
                    }
                }
                return false;
            }

            // 非反制阶段：决定是否使用无懈可击抵消锦囊
            if (isHarmfulTrick)
            {
                // 伤害性锦囊
                if (targetIsSelf)
                {
                    // 目标是自己，高概率使用无懈可击
                    return Random.value < 0.8f;
                }
                else if (targetIsAlly)
                {
                    // 目标是盟友，中等概率使用
                    return Random.value < 0.5f;
                }
                else if (!userIsAlly && target == null)
                {
                    // 群体伤害锦囊（目标为null表示还未结算到具体目标）
                    // 暂不使用，等结算到自己时再用
                    return false;
                }
            }
            else if (isBeneficialTrick)
            {
                // 有益锦囊
                if (!userIsAlly)
                {
                    // 敌人使用有益锦囊，考虑抵消
                    return Random.value < 0.4f;
                }
            }

            return false;
        }

        /// <summary>
        /// ⭐ 判断两个玩家是否是盟友（AI用）
        /// </summary>
        private bool IsAllyForAI(Player aiPlayer, Player otherPlayer)
        {
            if (aiPlayer == otherPlayer) return true;

            // 故事模式
            if (StoryBattleManager.Instance != null && StoryBattleManager.Instance.isBattleActive)
            {
                return StoryBattleManager.Instance.IsAlly(aiPlayer, otherPlayer);
            }

            // 普通模式：同阵营是盟友
            return aiPlayer.faction == otherPlayer.faction;
        }

        /// <summary>
        /// ⭐ 判断锦囊是否是伤害性的
        /// </summary>
        private bool IsHarmfulTrick(string cardName)
        {
            return cardName switch
            {
                "南蛮入侵" => true,
                "万箭齐发" => true,
                "决斗" => true,
                "顺手牵羊" => true,
                "过河拆桥" => true,
                "乐不思蜀" => true,
                "兵粮寸断" => true,
                "闪电" => true,
                _ => false
            };
        }

        /// <summary>
        /// ⭐ 判断锦囊是否是有益的
        /// </summary>
        private bool IsBeneficialTrick(string cardName)
        {
            return cardName switch
            {
                "桃园结义" => true,
                "五谷丰登" => true,
                "无中生有" => true,
                _ => false
            };
        }

        #endregion
    }
}