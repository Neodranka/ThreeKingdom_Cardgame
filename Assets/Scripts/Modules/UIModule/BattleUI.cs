using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ThreeKingdoms.UI
{
    /// <summary>
    /// 战斗UI主控制器
    /// </summary>
    public class BattleUI : MonoBehaviour
    {
        public static BattleUI Instance { get; private set; }

        [Header("预制体")]
        public GameObject cardUIPrefab;
        public GameObject playerInfoPrefab;

        [Header("玩家手牌区")]
        public Transform handCardContainer;
        public ScrollRect handCardScrollRect;

        [Header("玩家信息区")]
        public Transform localPlayerInfoContainer;
        public Transform otherPlayersContainer;

        [Header("游戏信息")]
        public TextMeshProUGUI turnInfoText;
        public TextMeshProUGUI phaseText;
        public TextMeshProUGUI deckInfoText;
        public TextMeshProUGUI messageText;

        [Header("操作按钮")]
        public Button endPhaseButton;
        public Button useCardButton;
        public Button cancelButton;
        public GameObject targetSelectionPanel;

        [Header("牌堆显示")]
        public TextMeshProUGUI drawPileText;
        public TextMeshProUGUI discardPileText;

        [Header("动作日志")]
        public Transform logContainer;
        public TextMeshProUGUI logPrefab;
        public int maxLogEntries = 10;

        private List<CardUI> handCardUIs = new List<CardUI>();
        private Dictionary<Player, PlayerInfoUI> playerInfoUIs = new Dictionary<Player, PlayerInfoUI>();
        private CardUI selectedCard = null;
        private Player selectedTarget = null;
        private Queue<string> logMessages = new Queue<string>();

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

        private void Start()
        {
            SetupButtons();
            UpdateButtonStates();
        }

        private void Update()
        {
            UpdateGameInfo();
        }

        /// <summary>
        /// 设置按钮事件
        /// </summary>
        private void SetupButtons()
        {
            if (endPhaseButton != null)
            {
                endPhaseButton.onClick.AddListener(OnEndPhaseClicked);
            }

            if (useCardButton != null)
            {
                useCardButton.onClick.AddListener(OnUseCardClicked);
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(OnCancelClicked);
            }
        }

        /// <summary>
        /// 初始化玩家UI
        /// </summary>
        public void InitializePlayers(List<Player> players, Player localPlayer)
        {
            // 清空现有UI
            playerInfoUIs.Clear();

            // 创建本地玩家UI
            if (localPlayer != null)
            {
                GameObject localPlayerObj = Instantiate(playerInfoPrefab, localPlayerInfoContainer);
                PlayerInfoUI localPlayerUI = localPlayerObj.GetComponent<PlayerInfoUI>();
                localPlayerUI.SetPlayer(localPlayer, true);
                playerInfoUIs[localPlayer] = localPlayerUI;
            }

            // 创建其他玩家UI
            foreach (var player in players)
            {
                if (player == localPlayer) continue;

                GameObject playerObj = Instantiate(playerInfoPrefab, otherPlayersContainer);
                PlayerInfoUI playerUI = playerObj.GetComponent<PlayerInfoUI>();
                playerUI.SetPlayer(player, false);
                playerInfoUIs[player] = playerUI;

                // 添加点击事件用于选择目标
                Button targetButton = playerObj.GetComponent<Button>();
                if (targetButton == null)
                {
                    targetButton = playerObj.AddComponent<Button>();
                }
                
                Player targetPlayer = player; // 捕获变量
                targetButton.onClick.AddListener(() => OnPlayerSelected(targetPlayer));
            }

            AddLog("游戏开始!");
        }

        /// <summary>
        /// 更新手牌显示
        /// </summary>
        public void UpdateHandCards(List<Card> cards)
        {
            // 清空现有手牌UI
            foreach (var cardUI in handCardUIs)
            {
                Destroy(cardUI.gameObject);
            }
            handCardUIs.Clear();

            // 创建新的手牌UI
            foreach (var card in cards)
            {
                GameObject cardObj = Instantiate(cardUIPrefab, handCardContainer);
                CardUI cardUI = cardObj.GetComponent<CardUI>();
                cardUI.SetCard(card);
                handCardUIs.Add(cardUI);
            }
        }

        /// <summary>
        /// 更新所有玩家信息
        /// </summary>
        public void UpdateAllPlayerInfo()
        {
            foreach (var kvp in playerInfoUIs)
            {
                kvp.Value.UpdateDisplay();
            }
        }

        /// <summary>
        /// 更新当前玩家指示器
        /// </summary>
        public void UpdateCurrentPlayerIndicator(Player currentPlayer)
        {
            foreach (var kvp in playerInfoUIs)
            {
                kvp.Value.SetCurrentPlayer(kvp.Key == currentPlayer);
            }
        }

        /// <summary>
        /// 更新游戏信息
        /// </summary>
        private void UpdateGameInfo()
        {
            if (BattleManager.Instance == null) return;

            // 更新回合信息
            if (turnInfoText != null)
            {
                Player currentPlayer = BattleManager.Instance.GetCurrentPlayer();
                if (currentPlayer != null)
                {
                    turnInfoText.text = $"第 {BattleManager.Instance.turnCount} 回合\n{currentPlayer.playerName}";
                }
            }

            // 更新阶段信息
            if (phaseText != null)
            {
                phaseText.text = GetPhaseText(BattleManager.Instance.currentPhase);
            }

            // 更新牌堆信息
            if (DeckManager.Instance != null)
            {
                if (drawPileText != null)
                {
                    drawPileText.text = $"牌堆\n{DeckManager.Instance.GetDrawPileCount()}";
                }

                if (discardPileText != null)
                {
                    discardPileText.text = $"弃牌堆\n{DeckManager.Instance.GetDiscardPileCount()}";
                }
            }

            // 更新按钮状态
            UpdateButtonStates();
        }

        /// <summary>
        /// 获取阶段文本
        /// </summary>
        private string GetPhaseText(TurnPhase phase)
        {
            switch (phase)
            {
                case TurnPhase.Prepare: return "准备阶段";
                case TurnPhase.Judge: return "判定阶段";
                case TurnPhase.Draw: return "摸牌阶段";
                case TurnPhase.Play: return "出牌阶段";
                case TurnPhase.Discard: return "弃牌阶段";
                case TurnPhase.End: return "结束阶段";
                default: return "未知阶段";
            }
        }

        /// <summary>
        /// 更新按钮状态
        /// </summary>
        private void UpdateButtonStates()
        {
            bool isPlayPhase = BattleManager.Instance != null && 
                              BattleManager.Instance.currentPhase == TurnPhase.Play;
            
            bool isLocalPlayerTurn = IsLocalPlayerTurn();

            // 结束阶段按钮
            if (endPhaseButton != null)
            {
                endPhaseButton.interactable = isPlayPhase && isLocalPlayerTurn;
            }

            // 使用卡牌按钮
            if (useCardButton != null)
            {
                useCardButton.interactable = selectedCard != null && isPlayPhase && isLocalPlayerTurn;
            }

            // 取消按钮
            if (cancelButton != null)
            {
                cancelButton.gameObject.SetActive(selectedCard != null);
            }
        }

        /// <summary>
        /// 检查是否是本地玩家回合
        /// </summary>
        private bool IsLocalPlayerTurn()
        {
            if (BattleManager.Instance == null) return false;
            
            Player currentPlayer = BattleManager.Instance.GetCurrentPlayer();
            // 这里假设第一个玩家是本地玩家,实际应该有更好的标记方式
            return currentPlayer == BattleManager.Instance.players[0];
        }

        /// <summary>
        /// 卡牌被选中回调
        /// </summary>
        public void OnCardSelected(CardUI cardUI)
        {
            // 取消之前的选择
            if (selectedCard != null && selectedCard != cardUI)
            {
                selectedCard.SetSelected(false);
            }

            selectedCard = cardUI.isSelected ? cardUI : null;
            UpdateButtonStates();

            if (selectedCard != null)
            {
                ShowMessage($"已选择: {selectedCard.cardData.cardName}");
            }
            else
            {
                ShowMessage("");
            }
        }

        /// <summary>
        /// 玩家被选中回调
        /// </summary>
        private void OnPlayerSelected(Player player)
        {
            if (selectedCard == null) return;
            if (!player.isAlive) return;

            selectedTarget = player;
            ShowMessage($"目标: {player.playerName}");
        }

        /// <summary>
        /// 结束出牌阶段按钮点击
        /// </summary>
        private void OnEndPhaseClicked()
        {
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.EndPlayPhase();
                AddLog("结束出牌阶段");
            }
        }

        /// <summary>
        /// 使用卡牌按钮点击
        /// </summary>
        private void OnUseCardClicked()
        {
            if (selectedCard == null) return;

            Card card = selectedCard.cardData;
            Player user = BattleManager.Instance.GetCurrentPlayer();

            // 根据卡牌类型执行不同操作
            switch (card.cardName)
            {
                case "杀":
                    if (selectedTarget == null)
                    {
                        ShowMessage("请选择目标!");
                        return;
                    }
                    UseSlash(user, selectedTarget, card);
                    break;

                case "桃":
                    UsePeach(user, card);
                    break;

                case "闪":
                    ShowMessage("【闪】只能在响应【杀】时使用");
                    return;

                case "决斗":
                    if (selectedTarget == null)
                    {
                        ShowMessage("请选择目标!");
                        return;
                    }
                    BattleManager.Instance.UseDuel(user, selectedTarget, card);
                    RemoveCardUI(card);
                    AddLog($"{user.playerName} 对 {selectedTarget.playerName} 使用了【决斗】");
                    break;

                case "南蛮入侵":
                    BattleManager.Instance.UseSavageAssault(user, card);
                    RemoveCardUI(card);
                    AddLog($"{user.playerName} 使用了【南蛮入侵】");
                    break;

                case "万箭齐发":
                    BattleManager.Instance.UseArrowBarrage(user, card);
                    RemoveCardUI(card);
                    AddLog($"{user.playerName} 使用了【万箭齐发】");
                    break;

                case "桃园结义":
                    BattleManager.Instance.UsePeachGarden(user, card);
                    RemoveCardUI(card);
                    AddLog($"{user.playerName} 使用了【桃园结义】");
                    break;

                case "顺手牵羊":
                    if (selectedTarget == null)
                    {
                        ShowMessage("msg_select_target");
                        return;
                    }

                    if (selectedTarget.handCards.Count == 0 && selectedTarget.equipments.Count == 0)
                    {
                        ShowMessage("目标没有可获得的牌!");
                        return;
                    }

                    BattleManager.Instance.UseSnatch(user, selectedTarget, card);
                    RemoveCardUI(card);

                    string snatchName = CardNameHelper.GetLocalizedCardName("顺手牵羊");
                    //AddLocalizedLog("msg_used_card_on_target", user.playerName, selectedTarget.playerName, snatchName);
                    break;

                case "过河拆桥":
                    if (selectedTarget == null)
                    {
                        ShowMessage("msg_select_target");
                        return;
                    }

                    if (selectedTarget.handCards.Count == 0 && selectedTarget.equipments.Count == 0)
                    {
                        ShowMessage("目标没有可弃置的牌!");
                        return;
                    }

                    BattleManager.Instance.UseDismantlement(user, selectedTarget, card);
                    RemoveCardUI(card);

                    string dismantleName = CardNameHelper.GetLocalizedCardName("过河拆桥");
                    //AddLocalizedLog("msg_used_card_on_target", user.playerName, selectedTarget.playerName, dismantleName);
                    break;

                case "五谷丰登":
                    BattleManager.Instance.UseHarvest(user, card);
                    RemoveCardUI(card);

                    string harvestName = CardNameHelper.GetLocalizedCardName("五谷丰登");
                    //AddLocalizedLog("msg_used_card", user.playerName, harvestName);
                    break;

                case "无懈可击":
                    ShowMessage("【无懈可击】只能在响应锦囊牌时使用");
                    return;

                case "乐不思蜀":
                case "闪电":
                    ShowMessage("延时锦囊暂未实现");
                    return;

                default:
                    ShowMessage($"卡牌 {card.cardName} 的效果尚未实现");
                    return;
            }

            // 清除选择
            ClearSelection();

            // 更新所有UI
            UpdateAllPlayerInfo();
        }

        /// <summary>
        /// 取消按钮点击
        /// </summary>
        private void OnCancelClicked()
        {
            ClearSelection();
        }

        /// <summary>
        /// 清除选择
        /// </summary>
        private void ClearSelection()
        {
            if (selectedCard != null)
            {
                selectedCard.SetSelected(false);
                selectedCard = null;
            }
            selectedTarget = null;
            ShowMessage("");
        }

        /// <summary>
        /// 使用【杀】
        /// </summary>
        private void UseSlash(Player user, Player target, Card card)
        {
            BattleManager.Instance.UseSlash(user, target, card);
            
            // 移除手牌UI
            RemoveCardUI(card);
            
            // 更新显示
            UpdateAllPlayerInfo();
            
            AddLog($"{user.playerName} 对 {target.playerName} 使用了【杀】");
            
            // 播放动画
            if (playerInfoUIs.ContainsKey(target))
            {
                playerInfoUIs[target].PlayDamageAnimation();
            }
        }

        /// <summary>
        /// 使用【桃】
        /// </summary>
        private void UsePeach(Player user, Card card)
        {
            BattleManager.Instance.UsePeach(user, card);
            
            // 移除手牌UI
            RemoveCardUI(card);
            
            // 更新显示
            UpdateAllPlayerInfo();
            
            AddLog($"{user.playerName} 使用了【桃】");
            
            // 播放动画
            if (playerInfoUIs.ContainsKey(user))
            {
                playerInfoUIs[user].PlayRecoverAnimation();
            }
        }

        /// <summary>
        /// 移除卡牌UI
        /// </summary>
        /// <summary>
        /// 移除卡牌UI - 改进版
        /// </summary>
        private void RemoveCardUI(Card card)
        {
            CardUI toRemove = null;
            foreach (var cardUI in handCardUIs)
            {
                if (cardUI.cardData == card)
                {
                    toRemove = cardUI;
                    break;
                }
            }

            if (toRemove != null)
            {
                handCardUIs.Remove(toRemove);

                // 播放使用动画,动画完成后刷新剩余卡牌位置
                toRemove.PlayUseAnimation(() => {
                    StartCoroutine(RefreshAllCardPositions());
                });
            }
        }

        /// <summary>
        /// 刷新所有卡牌位置(在卡牌被移除后调用)
        /// </summary>
        private System.Collections.IEnumerator RefreshAllCardPositions()
        {
            // 等待Layout重新计算完成
            yield return null;
            yield return null;

            // 刷新所有剩余卡牌的原始位置
            foreach (var cardUI in handCardUIs)
            {
                if (cardUI != null)
                {
                    cardUI.RefreshOriginalPosition();
                }
            }
        }

        /// <summary>
        /// 显示消息
        /// </summary>
        public void ShowMessage(string message)
        {
            if (messageText != null)
            {
                messageText.text = message;
            }
        }

        /// <summary>
        /// 添加日志
        /// </summary>
        public void AddLog(string message)
        {
            if (logContainer == null) return;

            // 限制日志数量
            if (logMessages.Count >= maxLogEntries)
            {
                logMessages.Dequeue();
                if (logContainer.childCount > 0)
                {
                    Destroy(logContainer.GetChild(0).gameObject);
                }
            }

            logMessages.Enqueue(message);

            // 创建日志条目
            if (logPrefab != null)
            {
                TextMeshProUGUI logEntry = Instantiate(logPrefab, logContainer);
                logEntry.text = $"[{System.DateTime.Now:HH:mm:ss}] {message}";
            }

            Debug.Log($"[游戏日志] {message}");
        }

        /// <summary>
        /// 显示目标选择面板
        /// </summary>
        public void ShowTargetSelection(bool show)
        {
            if (targetSelectionPanel != null)
            {
                targetSelectionPanel.SetActive(show);
            }
        }

        /// <summary>
        /// 获取玩家InfoUI
        /// </summary>
        public PlayerInfoUI GetPlayerInfoUI(Player player)
        {
            return playerInfoUIs.ContainsKey(player) ? playerInfoUIs[player] : null;
        }
    }
}
