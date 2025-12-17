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

            // ⭐ 重置当前玩家的回合状态
            currentPlayer.ResetTurnState();

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

                // ⭐ 启动游戏结束流程
                StartCoroutine(HandleGameEnd(winner));
                return true;
            }

            return false;
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

            return true;
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
                target.TakeDamage(1, user);

                if (EventManager.Instance != null)
                {
                    EventManager.Instance.TriggerPlayerDamaged(target, user, 1, slashCard);
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
        /// 处理南蛮入侵的响应（协程）
        /// </summary>
        private IEnumerator ProcessSavageAssaultResponses(Player user, Card card)
        {
            foreach (var player in players)
            {
                if (player == user || !player.isAlive) continue;

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
                    Debug.Log($"[南蛮入侵] {player.playerName} 没有【杀】，受到1点伤害");
                    player.TakeDamage(1, user);

                    if (EventManager.Instance != null)
                    {
                        EventManager.Instance.TriggerPlayerDamaged(player, user, 1, card);
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
        /// 处理万箭齐发的响应（协程）
        /// </summary>
        private IEnumerator ProcessArrowBarrageResponses(Player user, Card card)
        {
            foreach (var player in players)
            {
                if (player == user || !player.isAlive) continue;

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
                    Debug.Log($"[万箭齐发] {player.playerName} 没有【闪】，受到1点伤害");
                    player.TakeDamage(1, user);

                    if (EventManager.Instance != null)
                    {
                        EventManager.Instance.TriggerPlayerDamaged(player, user, 1, card);
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
    }
}