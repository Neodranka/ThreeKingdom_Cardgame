using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System;

namespace ThreeKingdoms.UI
{
    /// <summary>
    /// 响应类型枚举
    /// </summary>
    public enum ResponseType
    {
        None,
        Dodge,      // 响应杀，需要出闪
        Slash,      // 响应决斗/南蛮，需要出杀
        Peach       // 濒死求桃
    }

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

        [Header("响应系统UI")]
        public GameObject responsePanel;
        public TextMeshProUGUI responsePromptText;
        public Button responsePlayButton;
        public Button responseSkipButton;
        public Transform responseCardContainer;

        private List<CardUI> handCardUIs = new List<CardUI>();
        private Dictionary<Player, PlayerInfoUI> playerInfoUIs = new Dictionary<Player, PlayerInfoUI>();
        private CardUI selectedCard = null;
        private Player selectedTarget = null;
        private Queue<string> logMessages = new Queue<string>();

        // 响应系统状态
        private bool isWaitingForResponse = false;
        private ResponseType currentResponseType = ResponseType.None;
        private Player responsePlayer = null;
        private Action<Card> responseCallback = null;
        private CardUI selectedResponseCard = null;

        // ⭐ 弃牌系统状态
        private bool isWaitingForDiscard = false;
        private int cardsToDiscard = 0;
        private List<CardUI> selectedDiscardCards = new List<CardUI>();
        private Action<List<Card>> discardCallback = null;
        private GameObject discardPanel = null;
        private TextMeshProUGUI discardPromptText = null;
        private Button discardConfirmButton = null;

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

            // 响应系统按钮
            if (responsePlayButton != null)
            {
                responsePlayButton.onClick.AddListener(OnResponsePlayClicked);
            }

            if (responseSkipButton != null)
            {
                responseSkipButton.onClick.AddListener(OnResponseSkipClicked);
            }

            // 初始化时隐藏响应面板
            if (responsePanel != null)
            {
                responsePanel.SetActive(false);
            }

            // 应用字体设置
            ApplyFontsToAllUI();
        }

        /// <summary>
        /// 应用字体到所有UI元素
        /// </summary>
        private void ApplyFontsToAllUI()
        {
            // 获取所有TextMeshProUGUI组件并应用字体
            TextMeshProUGUI[] allTexts = GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var text in allTexts)
            {
                TMPFontHelper.SetFontByLanguage(text);
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

            AddLog(GetLocalizedText("msg_game_start", "游戏开始!"));
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
                    string turnLabel = LocalizationManager.Instance != null
                        ? LocalizationManager.Instance.GetTextFormatted("ui_turn", BattleManager.Instance.turnCount)
                        : $"第 {BattleManager.Instance.turnCount} 回合";
                    turnInfoText.text = $"{turnLabel}\n{currentPlayer.playerName}";
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
                    string drawPileLabel = GetLocalizedText("ui_draw_pile", "牌堆");
                    drawPileText.text = $"{drawPileLabel}\n{DeckManager.Instance.GetDrawPileCount()}";
                }

                if (discardPileText != null)
                {
                    string discardPileLabel = GetLocalizedText("ui_discard_pile", "弃牌堆");
                    discardPileText.text = $"{discardPileLabel}\n{DeckManager.Instance.GetDiscardPileCount()}";
                }
            }

            // 更新按钮状态
            UpdateButtonStates();
        }

        /// <summary>
        /// 获取阶段文本（本地化）
        /// </summary>
        private string GetPhaseText(TurnPhase phase)
        {
            if (LocalizationManager.Instance == null)
            {
                // 无本地化时的fallback
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

            switch (phase)
            {
                case TurnPhase.Prepare: return LocalizationManager.Instance.GetText("phase_prepare");
                case TurnPhase.Judge: return LocalizationManager.Instance.GetText("phase_judge");
                case TurnPhase.Draw: return LocalizationManager.Instance.GetText("phase_draw");
                case TurnPhase.Play: return LocalizationManager.Instance.GetText("phase_play");
                case TurnPhase.Discard: return LocalizationManager.Instance.GetText("phase_discard");
                case TurnPhase.End: return LocalizationManager.Instance.GetText("phase_end");
                default: return LocalizationManager.Instance.GetText("phase_unknown");
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
            // ⭐ 等待响应时，非响应卡牌不可选
            if (isWaitingForResponse)
            {
                // 响应模式下的选择由OnResponseCardSelected处理
                return;
            }

            // 取消之前的选择
            if (selectedCard != null && selectedCard != cardUI)
            {
                selectedCard.SetSelected(false);
            }

            selectedCard = cardUI.isSelected ? cardUI : null;
            UpdateButtonStates();

            if (selectedCard != null)
            {
                string cardName = CardNameHelper.GetLocalizedCardName(selectedCard.cardData.cardName);
                string selectedLabel = LocalizationManager.Instance != null
                    ? LocalizationManager.Instance.GetText("ui_card_selected")
                    : "已选择";
                ShowMessage($"{selectedLabel}: {cardName}");
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
            string targetLabel = LocalizationManager.Instance != null
                ? LocalizationManager.Instance.GetText("ui_target")
                : "目标";
            ShowMessage($"{targetLabel}: {player.playerName}");
        }

        /// <summary>
        /// 结束出牌阶段按钮点击
        /// </summary>
        private void OnEndPhaseClicked()
        {
            // ⭐ 等待响应时禁用其他操作
            if (isWaitingForResponse)
            {
                ShowMessage(GetLocalizedText("msg_complete_response_first", "请先完成响应!"));
                return;
            }

            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.EndPlayPhase();
                AddLog(GetLocalizedText("msg_end_play_phase", "结束出牌阶段"));
            }
        }

        /// <summary>
        /// 使用卡牌按钮点击
        /// </summary>
        private void OnUseCardClicked()
        {
            // ⭐ 等待响应时禁用普通出牌
            if (isWaitingForResponse)
            {
                ShowMessage(GetLocalizedText("msg_complete_response_first", "请先完成响应!"));
                return;
            }

            if (selectedCard == null) return;

            Card card = selectedCard.cardData;
            Player user = BattleManager.Instance.GetCurrentPlayer();

            // 根据卡牌类型执行不同操作
            switch (card.cardName)
            {
                case "杀":
                    if (selectedTarget == null)
                    {
                        ShowMessage(GetLocalizedText("msg_select_target", "请选择目标!"));
                        return;
                    }
                    UseSlash(user, selectedTarget, card);
                    break;

                case "桃":
                    UsePeach(user, card);
                    break;

                case "闪":
                    ShowMessage(GetLocalizedText("msg_dodge_only_response", "【闪】只能在响应【杀】时使用"));
                    return;

                case "决斗":
                    if (selectedTarget == null)
                    {
                        ShowMessage(GetLocalizedText("msg_select_target", "请选择目标!"));
                        return;
                    }
                    BattleManager.Instance.UseDuel(user, selectedTarget, card);
                    RemoveCardUI(card);
                    AddLocalizedLog("msg_used_card_on_target", user.playerName, selectedTarget.playerName, CardNameHelper.GetLocalizedCardName("决斗"));
                    break;

                case "南蛮入侵":
                    BattleManager.Instance.UseSavageAssault(user, card);
                    RemoveCardUI(card);
                    AddLocalizedLog("msg_used_card", user.playerName, CardNameHelper.GetLocalizedCardName("南蛮入侵"));
                    break;

                case "万箭齐发":
                    BattleManager.Instance.UseArrowBarrage(user, card);
                    RemoveCardUI(card);
                    AddLocalizedLog("msg_used_card", user.playerName, CardNameHelper.GetLocalizedCardName("万箭齐发"));
                    break;

                case "桃园结义":
                    BattleManager.Instance.UsePeachGarden(user, card);
                    RemoveCardUI(card);
                    AddLocalizedLog("msg_used_card", user.playerName, CardNameHelper.GetLocalizedCardName("桃园结义"));
                    break;

                case "顺手牵羊":
                    if (selectedTarget == null)
                    {
                        ShowMessage(GetLocalizedText("msg_select_target", "请选择目标!"));
                        return;
                    }

                    if (selectedTarget.handCards.Count == 0 && selectedTarget.equipments.Count == 0)
                    {
                        ShowMessage(GetLocalizedText("msg_no_cards_to_obtain", "目标没有可获得的牌!"));
                        return;
                    }

                    BattleManager.Instance.UseSnatch(user, selectedTarget, card);
                    RemoveCardUI(card);
                    break;

                case "过河拆桥":
                    if (selectedTarget == null)
                    {
                        ShowMessage(GetLocalizedText("msg_select_target", "请选择目标!"));
                        return;
                    }

                    if (selectedTarget.handCards.Count == 0 && selectedTarget.equipments.Count == 0)
                    {
                        ShowMessage(GetLocalizedText("msg_no_cards_to_discard", "目标没有可弃置的牌!"));
                        return;
                    }

                    BattleManager.Instance.UseDismantlement(user, selectedTarget, card);
                    RemoveCardUI(card);
                    break;

                case "五谷丰登":
                    BattleManager.Instance.UseHarvest(user, card);
                    RemoveCardUI(card);
                    break;

                case "无懈可击":
                    ShowMessage(GetLocalizedText("msg_nullification_only_response", "【无懈可击】只能在响应锦囊牌时使用"));
                    return;

                case "乐不思蜀":
                case "闪电":
                    ShowMessage(GetLocalizedText("msg_delayed_trick_not_implemented", "延时锦囊暂未实现"));
                    return;

                default:
                    string cardName = CardNameHelper.GetLocalizedCardName(card.cardName);
                    ShowMessage(LocalizationManager.Instance != null
                        ? LocalizationManager.Instance.GetTextFormatted("msg_card_effect_unimplemented", cardName)
                        : $"卡牌 {cardName} 的效果尚未实现");
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
            // ⭐ 先检查是否可以使用杀
            if (!BattleManager.Instance.CanUseSlash(user, target, out string errorMessage))
            {
                // 显示本地化的错误消息
                if (!user.CanUseSlash())
                {
                    ShowLocalizedMessage("msg_slash_limit_reached");
                }
                else if (!user.IsInAttackRange(target))
                {
                    int distance = user.GetDistanceTo(target);
                    int range = user.GetTotalAttackRange();
                    ShowMessage(LocalizationManager.Instance?.GetTextFormatted("msg_target_out_of_range", distance, range)
                        ?? $"目标不在攻击范围内（距离:{distance}, 范围:{range}）");
                }
                else
                {
                    ShowMessage(errorMessage);
                }
                return;
            }

            BattleManager.Instance.UseSlash(user, target, card);

            // 移除手牌UI
            RemoveCardUI(card);

            // 更新显示
            UpdateAllPlayerInfo();

            AddLocalizedLog("msg_used_card_on_target", user.playerName, target.playerName, CardNameHelper.GetLocalizedCardName("杀"));

            // 播放动画
            if (playerInfoUIs.ContainsKey(target))
            {
                playerInfoUIs[target].PlayDamageAnimation();
            }
        }

        /// <summary>
        /// ⭐ 获取本地化文本（带fallback）
        /// </summary>
        private string GetLocalizedText(string key, string fallback)
        {
            if (LocalizationManager.Instance != null)
            {
                return LocalizationManager.Instance.GetText(key);
            }
            return fallback;
        }

        /// <summary>
        /// 显示本地化消息
        /// </summary>
        private void ShowLocalizedMessage(string key)
        {
            string message = LocalizationManager.Instance?.GetText(key) ?? key;
            ShowMessage(message);
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
            
            AddLocalizedLog("msg_used_card", user.playerName, CardNameHelper.GetLocalizedCardName("桃"));
            
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
        /// ⭐ 添加本地化日志（带格式化参数）
        /// </summary>
        public void AddLocalizedLog(string key, params object[] args)
        {
            if (LocalizationManager.Instance != null)
            {
                string message = LocalizationManager.Instance.GetTextFormatted(key, args);
                AddLog(message);
            }
            else
            {
                // fallback - 直接显示参数
                AddLog(string.Join(" ", args));
            }
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

        /// <summary>
        /// ⭐ 获取玩家在屏幕上的位置（用于动画起点）
        /// </summary>
        public Vector3 GetPlayerScreenPosition(Player player)
        {
            if (playerInfoUIs.ContainsKey(player))
            {
                RectTransform rt = playerInfoUIs[player].GetComponent<RectTransform>();
                if (rt != null)
                {
                    return rt.position;
                }
            }

            // 如果找不到，返回屏幕中心
            return new Vector3(Screen.width / 2, Screen.height / 2, 0);
        }

        #region 响应系统

        /// <summary>
        /// 请求玩家响应（异步）
        /// </summary>
        /// <param name="player">需要响应的玩家</param>
        /// <param name="responseType">响应类型</param>
        /// <param name="callback">响应回调，传入null表示不响应，传入Card表示使用该牌响应</param>
        public void RequestResponse(Player player, ResponseType responseType, Action<Card> callback)
        {
            // 如果是AI玩家，自动响应
            if (player.isAI)
            {
                Card responseCard = FindResponseCard(player, responseType);

                // ⭐ AI响应时，需要实际打出卡牌并显示动画
                if (responseCard != null)
                {
                    player.PlayCard(responseCard);
                    DeckManager.Instance.DiscardCard(responseCard);

                    // 显示响应牌动画
                    if (PlayedCardDisplayManager.Instance != null)
                    {
                        Vector3 startPos = GetPlayerScreenPosition(player);
                        PlayedCardDisplayManager.Instance.ShowPlayedCard(responseCard, player, startPos);
                    }

                    string cardName = CardNameHelper.GetLocalizedCardName(responseCard.cardName);
                    AddLocalizedLog("msg_responded_card", player.playerName, cardName);
                }
                else
                {
                    AddLocalizedLog("msg_no_response", player.playerName);
                }

                callback?.Invoke(responseCard);
                return;
            }

            // 人类玩家，显示响应UI
            responsePlayer = player;
            currentResponseType = responseType;
            responseCallback = callback;
            isWaitingForResponse = true;

            ShowResponsePanel(player, responseType);
        }

        /// <summary>
        /// 显示响应面板
        /// </summary>
        private void ShowResponsePanel(Player player, ResponseType responseType)
        {
            if (responsePanel == null)
            {
                // 如果没有响应面板，创建一个简单的
                CreateResponsePanel();
            }

            responsePanel.SetActive(true);

            // 设置提示文本
            if (responsePromptText != null)
            {
                string promptKey = responseType switch
                {
                    ResponseType.Dodge => "msg_dodge_required",
                    ResponseType.Slash => "msg_slash_required",
                    ResponseType.Peach => "msg_peach_required",
                    _ => "msg_response_required"
                };

                if (LocalizationManager.Instance != null)
                {
                    responsePromptText.text = LocalizationManager.Instance.GetText(promptKey);
                }
                else
                {
                    responsePromptText.text = responseType switch
                    {
                        ResponseType.Dodge => "请出【闪】",
                        ResponseType.Slash => "请出【杀】",
                        ResponseType.Peach => "请出【桃】",
                        _ => "请响应"
                    };
                }
                TMPFontHelper.SetFontByLanguage(responsePromptText);
            }

            // 更新响应按钮状态
            UpdateResponseButtons();

            // 高亮可用的响应卡牌
            HighlightResponseCards(player, responseType);
        }

        /// <summary>
        /// 创建响应面板（如果不存在）
        /// </summary>
        private void CreateResponsePanel()
        {
            // 创建响应面板
            GameObject panelObj = new GameObject("ResponsePanel");
            panelObj.transform.SetParent(transform, false);

            RectTransform panelRT = panelObj.AddComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0.3f, 0.4f);
            panelRT.anchorMax = new Vector2(0.7f, 0.6f);
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;

            Image panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.8f);

            responsePanel = panelObj;

            // 创建提示文本
            GameObject promptObj = new GameObject("PromptText");
            promptObj.transform.SetParent(panelObj.transform, false);
            RectTransform promptRT = promptObj.AddComponent<RectTransform>();
            promptRT.anchorMin = new Vector2(0, 0.6f);
            promptRT.anchorMax = new Vector2(1, 1);
            promptRT.offsetMin = new Vector2(10, 0);
            promptRT.offsetMax = new Vector2(-10, -10);

            responsePromptText = promptObj.AddComponent<TextMeshProUGUI>();
            responsePromptText.fontSize = 24;
            responsePromptText.alignment = TextAlignmentOptions.Center;
            responsePromptText.color = Color.yellow;

            // 创建按钮容器
            GameObject buttonContainer = new GameObject("ButtonContainer");
            buttonContainer.transform.SetParent(panelObj.transform, false);
            RectTransform btnContainerRT = buttonContainer.AddComponent<RectTransform>();
            btnContainerRT.anchorMin = new Vector2(0, 0);
            btnContainerRT.anchorMax = new Vector2(1, 0.5f);
            btnContainerRT.offsetMin = Vector2.zero;
            btnContainerRT.offsetMax = Vector2.zero;

            HorizontalLayoutGroup hlg = buttonContainer.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 20;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;

            // 创建"出牌"按钮
            string playBtnText = GetLocalizedText("ui_play_card", "出牌");
            responsePlayButton = CreateButton(buttonContainer.transform, playBtnText, new Color(0.2f, 0.6f, 0.2f));
            responsePlayButton.onClick.AddListener(OnResponsePlayClicked);

            // 创建"跳过"按钮
            string skipBtnText = GetLocalizedText("ui_skip", "跳过");
            responseSkipButton = CreateButton(buttonContainer.transform, skipBtnText, new Color(0.6f, 0.2f, 0.2f));
            responseSkipButton.onClick.AddListener(OnResponseSkipClicked);
        }

        /// <summary>
        /// 创建按钮辅助方法
        /// </summary>
        private Button CreateButton(Transform parent, string text, Color color)
        {
            GameObject btnObj = new GameObject(text + "Button");
            btnObj.transform.SetParent(parent, false);

            RectTransform btnRT = btnObj.AddComponent<RectTransform>();
            btnRT.sizeDelta = new Vector2(120, 50);

            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = color;

            Button btn = btnObj.AddComponent<Button>();

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;

            TextMeshProUGUI btnText = textObj.AddComponent<TextMeshProUGUI>();
            btnText.text = text;
            btnText.fontSize = 20;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.color = Color.white;
            TMPFontHelper.SetFontByLanguage(btnText);

            return btn;
        }

        /// <summary>
        /// 更新响应按钮状态
        /// </summary>
        private void UpdateResponseButtons()
        {
            if (responsePlayButton != null)
            {
                // 只有选中了有效的响应卡牌才能点击出牌
                responsePlayButton.interactable = selectedResponseCard != null;
            }
        }

        /// <summary>
        /// 高亮可用的响应卡牌
        /// </summary>
        private void HighlightResponseCards(Player player, ResponseType responseType)
        {
            string requiredCardName = responseType switch
            {
                ResponseType.Dodge => "闪",
                ResponseType.Slash => "杀",
                ResponseType.Peach => "桃",
                _ => ""
            };

            foreach (var cardUI in handCardUIs)
            {
                bool canRespond = cardUI.cardData.cardName == requiredCardName;
                cardUI.SetInteractable(canRespond);
            }
        }

        /// <summary>
        /// 隐藏响应面板
        /// </summary>
        private void HideResponsePanel()
        {
            if (responsePanel != null)
            {
                responsePanel.SetActive(false);
            }

            isWaitingForResponse = false;
            currentResponseType = ResponseType.None;
            responsePlayer = null;
            selectedResponseCard = null;

            // 恢复所有卡牌的交互性
            foreach (var cardUI in handCardUIs)
            {
                cardUI.SetInteractable(true);
            }
        }

        /// <summary>
        /// 响应出牌按钮点击
        /// </summary>
        private void OnResponsePlayClicked()
        {
            if (selectedResponseCard == null) return;

            Card responseCard = selectedResponseCard.cardData;

            // 从手牌移除
            if (responsePlayer != null)
            {
                responsePlayer.PlayCard(responseCard);
                DeckManager.Instance.DiscardCard(responseCard);
            }

            // ⭐ 显示响应牌动画（从响应者位置飞出）
            if (PlayedCardDisplayManager.Instance != null && responsePlayer != null)
            {
                Vector3 startPos = GetPlayerScreenPosition(responsePlayer);
                PlayedCardDisplayManager.Instance.ShowPlayedCard(responseCard, responsePlayer, startPos);
            }

            // 移除卡牌UI
            RemoveCardUI(responseCard);

            // 隐藏响应面板
            HideResponsePanel();

            // 调用回调
            responseCallback?.Invoke(responseCard);
            responseCallback = null;

            string cardName = CardNameHelper.GetLocalizedCardName(responseCard.cardName);
            AddLocalizedLog("msg_responded_card", responsePlayer?.playerName, cardName);
        }

        /// <summary>
        /// 响应跳过按钮点击
        /// </summary>
        private void OnResponseSkipClicked()
        {
            // 隐藏响应面板
            HideResponsePanel();

            // 调用回调，传入null表示不响应
            responseCallback?.Invoke(null);
            responseCallback = null;

            AddLocalizedLog("msg_no_response", responsePlayer?.playerName);
        }

        /// <summary>
        /// 响应时选中卡牌
        /// </summary>
        public void OnResponseCardSelected(CardUI cardUI)
        {
            if (!isWaitingForResponse) return;

            // 检查是否是有效的响应卡牌
            string requiredCardName = currentResponseType switch
            {
                ResponseType.Dodge => "闪",
                ResponseType.Slash => "杀",
                ResponseType.Peach => "桃",
                _ => ""
            };

            if (cardUI.cardData.cardName != requiredCardName)
            {
                string localizedCardName = CardNameHelper.GetLocalizedCardName(requiredCardName);
                ShowMessage(LocalizationManager.Instance != null
                    ? LocalizationManager.Instance.GetTextFormatted("msg_need_card", localizedCardName)
                    : $"需要出【{localizedCardName}】");
                return;
            }

            // 取消之前的选择
            if (selectedResponseCard != null && selectedResponseCard != cardUI)
            {
                selectedResponseCard.SetSelected(false);
            }

            selectedResponseCard = cardUI.isSelected ? cardUI : null;
            UpdateResponseButtons();
        }

        /// <summary>
        /// 查找响应卡牌（AI用）
        /// </summary>
        private Card FindResponseCard(Player player, ResponseType responseType)
        {
            string requiredCardName = responseType switch
            {
                ResponseType.Dodge => "闪",
                ResponseType.Slash => "杀",
                ResponseType.Peach => "桃",
                _ => ""
            };

            foreach (var card in player.handCards)
            {
                if (card.cardName == requiredCardName)
                {
                    return card;
                }
            }
            return null;
        }

        /// <summary>
        /// 检查是否正在等待响应
        /// </summary>
        public bool IsWaitingForResponse()
        {
            return isWaitingForResponse;
        }

        #endregion

        #region 弃牌系统

        /// <summary>
        /// ⭐ 请求玩家弃牌
        /// </summary>
        /// <param name="player">需要弃牌的玩家</param>
        /// <param name="count">需要弃牌的数量</param>
        /// <param name="callback">弃牌完成回调</param>
        public void RequestDiscard(Player player, int count, Action<List<Card>> callback)
        {
            if (count <= 0)
            {
                callback?.Invoke(new List<Card>());
                return;
            }

            // AI玩家自动弃牌
            if (player.isAI)
            {
                List<Card> cardsToDiscardList = AISelectCardsToDiscard(player, count);
                foreach (var card in cardsToDiscardList)
                {
                    player.DiscardCard(card);
                    DeckManager.Instance.DiscardCard(card);
                }

                string discardCountText = LocalizationManager.Instance != null
                    ? LocalizationManager.Instance.GetTextFormatted("msg_discarded", player.playerName, cardsToDiscardList.Count)
                    : $"{player.playerName} 弃置了 {cardsToDiscardList.Count} 张牌";
                AddLog(discardCountText);

                callback?.Invoke(cardsToDiscardList);
                return;
            }

            // 人类玩家，显示弃牌UI
            cardsToDiscard = count;
            discardCallback = callback;
            isWaitingForDiscard = true;
            selectedDiscardCards.Clear();

            ShowDiscardPanel(player, count);
        }

        /// <summary>
        /// ⭐ AI选择要弃置的卡牌（保留价值高的牌）
        /// </summary>
        private List<Card> AISelectCardsToDiscard(Player player, int count)
        {
            List<Card> result = new List<Card>();
            List<Card> handCopy = new List<Card>(player.handCards);

            // 简单策略：优先弃置基本牌（闪>杀>桃），保留锦囊和装备
            // 按优先级排序：闪 < 杀 < 桃 < 锦囊 < 装备
            handCopy.Sort((a, b) =>
            {
                int priorityA = GetCardKeepPriority(a);
                int priorityB = GetCardKeepPriority(b);
                return priorityA - priorityB; // 优先级低的排前面（先弃）
            });

            for (int i = 0; i < count && i < handCopy.Count; i++)
            {
                result.Add(handCopy[i]);
            }

            return result;
        }

        /// <summary>
        /// ⭐ 获取卡牌保留优先级（数值越高越想保留）
        /// </summary>
        private int GetCardKeepPriority(Card card)
        {
            switch (card.cardName)
            {
                case "闪": return 1;  // 闪最先弃
                case "杀": return 2;
                case "桃": return 5;  // 桃要保留
                case "无懈可击": return 4;
                case "决斗": return 3;
                case "南蛮入侵": return 3;
                case "万箭齐发": return 3;
                case "顺手牵羊": return 3;
                case "过河拆桥": return 3;
                default:
                    if (card.cardType == CardType.Equipment) return 6; // 装备最想保留
                    return 2;
            }
        }

        /// <summary>
        /// ⭐ 显示弃牌面板
        /// </summary>
        private void ShowDiscardPanel(Player player, int count)
        {
            if (discardPanel == null)
            {
                CreateDiscardPanel();
            }

            discardPanel.SetActive(true);

            // 设置提示文本
            if (discardPromptText != null)
            {
                string promptText = LocalizationManager.Instance != null
                    ? LocalizationManager.Instance.GetTextFormatted("msg_discard_prompt", count)
                    : $"请选择 {count} 张牌弃置";
                discardPromptText.text = promptText;
                TMPFontHelper.SetFontByLanguage(discardPromptText);
            }

            // 更新确认按钮状态
            UpdateDiscardConfirmButton();

            // 设置手牌为可选择状态
            foreach (var cardUI in handCardUIs)
            {
                cardUI.SetInteractable(true);
                cardUI.SetSelected(false);
            }
        }

        /// <summary>
        /// ⭐ 创建弃牌面板
        /// </summary>
        private void CreateDiscardPanel()
        {
            // 创建面板
            discardPanel = new GameObject("DiscardPanel");
            discardPanel.transform.SetParent(transform, false);

            RectTransform panelRect = discardPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.3f, 0.4f);
            panelRect.anchorMax = new Vector2(0.7f, 0.6f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            Image panelImage = discardPanel.AddComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);

            // 创建提示文本
            GameObject promptObj = new GameObject("DiscardPrompt");
            promptObj.transform.SetParent(discardPanel.transform, false);

            RectTransform promptRect = promptObj.AddComponent<RectTransform>();
            promptRect.anchorMin = new Vector2(0, 0.5f);
            promptRect.anchorMax = new Vector2(1, 1);
            promptRect.offsetMin = new Vector2(10, 0);
            promptRect.offsetMax = new Vector2(-10, -10);

            discardPromptText = promptObj.AddComponent<TextMeshProUGUI>();
            discardPromptText.alignment = TextAlignmentOptions.Center;
            discardPromptText.fontSize = 24;
            discardPromptText.color = Color.white;

            // 创建确认按钮
            string confirmText = LocalizationManager.Instance != null
                ? LocalizationManager.Instance.GetText("ui_confirm")
                : "确认";
            discardConfirmButton = CreateButton(discardPanel.transform, confirmText, new Color(0.2f, 0.6f, 0.2f));

            RectTransform btnRect = discardConfirmButton.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0);
            btnRect.anchorMax = new Vector2(0.5f, 0);
            btnRect.anchoredPosition = new Vector2(0, 30);

            discardConfirmButton.onClick.AddListener(OnDiscardConfirmClicked);
            discardConfirmButton.interactable = false;
        }

        /// <summary>
        /// ⭐ 弃牌确认按钮点击
        /// </summary>
        private void OnDiscardConfirmClicked()
        {
            if (selectedDiscardCards.Count != cardsToDiscard) return;

            // 收集要弃的牌
            List<Card> discardedCards = new List<Card>();
            Player player = BattleManager.Instance?.GetCurrentPlayer();

            foreach (var cardUI in selectedDiscardCards)
            {
                Card card = cardUI.cardData;
                discardedCards.Add(card);

                // 从手牌移除
                if (player != null)
                {
                    player.DiscardCard(card);
                }
                DeckManager.Instance.DiscardCard(card);

                // 移除UI
                RemoveCardUI(card);
            }

            // 隐藏弃牌面板
            HideDiscardPanel();

            // 日志
            string discardText = LocalizationManager.Instance != null
                ? LocalizationManager.Instance.GetTextFormatted("msg_discarded", player?.playerName ?? "玩家", discardedCards.Count)
                : $"{player?.playerName ?? "玩家"} 弃置了 {discardedCards.Count} 张牌";
            AddLog(discardText);

            // 调用回调
            discardCallback?.Invoke(discardedCards);
            discardCallback = null;
        }

        /// <summary>
        /// ⭐ 隐藏弃牌面板
        /// </summary>
        private void HideDiscardPanel()
        {
            if (discardPanel != null)
            {
                discardPanel.SetActive(false);
            }

            isWaitingForDiscard = false;
            cardsToDiscard = 0;
            selectedDiscardCards.Clear();

            // 恢复手牌状态
            foreach (var cardUI in handCardUIs)
            {
                cardUI.SetSelected(false);
            }
        }

        /// <summary>
        /// ⭐ 更新弃牌确认按钮状态
        /// </summary>
        private void UpdateDiscardConfirmButton()
        {
            if (discardConfirmButton != null)
            {
                discardConfirmButton.interactable = (selectedDiscardCards.Count == cardsToDiscard);
            }

            // 更新提示文本显示选择进度
            if (discardPromptText != null)
            {
                string promptText = LocalizationManager.Instance != null
                    ? LocalizationManager.Instance.GetTextFormatted("msg_discard_progress", selectedDiscardCards.Count, cardsToDiscard)
                    : $"请选择要弃置的牌 ({selectedDiscardCards.Count}/{cardsToDiscard})";
                discardPromptText.text = promptText;
            }
        }

        /// <summary>
        /// ⭐ 弃牌时卡牌选择处理
        /// </summary>
        public void OnDiscardCardSelected(CardUI cardUI)
        {
            if (!isWaitingForDiscard) return;

            if (cardUI.isSelected)
            {
                // 选中
                if (!selectedDiscardCards.Contains(cardUI))
                {
                    if (selectedDiscardCards.Count < cardsToDiscard)
                    {
                        selectedDiscardCards.Add(cardUI);
                    }
                    else
                    {
                        // 已选满，取消选择
                        cardUI.SetSelected(false);
                        return;
                    }
                }
            }
            else
            {
                // 取消选中
                selectedDiscardCards.Remove(cardUI);
            }

            UpdateDiscardConfirmButton();
        }

        /// <summary>
        /// ⭐ 检查是否正在等待弃牌
        /// </summary>
        public bool IsWaitingForDiscard()
        {
            return isWaitingForDiscard;
        }

        #endregion
    }
}
