using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace ThreeKingdoms
{
    /// <summary>
    /// UI管理器
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("UI引用")]
        public TextMeshProUGUI turnInfoText;
        public TextMeshProUGUI phaseText;
        public TextMeshProUGUI deckInfoText;
        public Button endPhaseButton;
        public Transform playerInfoContainer;

        [Header("玩家信息预制体")]
        public GameObject playerInfoPrefab;

        private Dictionary<Player, GameObject> playerInfoPanels = new Dictionary<Player, GameObject>();

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
            if (endPhaseButton != null)
            {
                endPhaseButton.onClick.AddListener(OnEndPhaseClicked);
            }
        }

        private void Update()
        {
            UpdateUI();
        }

        /// <summary>
        /// 更新UI显示
        /// </summary>
        private void UpdateUI()
        {
            if (!BattleManager.Instance.gameStarted) return;

            // 更新回合信息（使用本地化）
            if (turnInfoText != null)
            {
                Player currentPlayer = BattleManager.Instance.GetCurrentPlayer();
                string turnText = LocalizationManager.Instance.GetTextFormatted("ui_turn", BattleManager.Instance.turnCount);
                string currentPlayerText = LocalizationManager.Instance.GetText("ui_current_player");
                turnInfoText.text = $"{turnText}\n{currentPlayerText}: {currentPlayer?.playerName}";
            }

            // 更新阶段信息（使用本地化）
            if (phaseText != null)
            {
                string phaseLabel = LocalizationManager.Instance.GetText("ui_phase");
                string phaseValue = GetLocalizedPhaseText(BattleManager.Instance.currentPhase);
                phaseText.text = $"{phaseLabel}: {phaseValue}";
            }

            // 更新牌堆信息（使用本地化）
            if (deckInfoText != null)
            {
                string drawPile = LocalizationManager.Instance.GetText("ui_draw_pile");
                string discardPile = LocalizationManager.Instance.GetText("ui_discard_pile");
                string cards = LocalizationManager.Instance.GetText("ui_cards");

                deckInfoText.text = $"{drawPile}: {DeckManager.Instance.GetDrawPileCount()} {cards}\n" +
                                   $"{discardPile}: {DeckManager.Instance.GetDiscardPileCount()} {cards}";
            }
        }

        /// <summary>
        /// 获取阶段文本
        /// </summary>
        private string GetLocalizedPhaseText(TurnPhase phase)
        {
            switch (phase)
            {
                case TurnPhase.Prepare:
                    return LocalizationManager.Instance.GetText("phase_prepare");
                case TurnPhase.Judge:
                    return LocalizationManager.Instance.GetText("phase_judge");
                case TurnPhase.Draw:
                    return LocalizationManager.Instance.GetText("phase_draw");
                case TurnPhase.Play:
                    return LocalizationManager.Instance.GetText("phase_play");
                case TurnPhase.Discard:
                    return LocalizationManager.Instance.GetText("phase_discard");
                case TurnPhase.End:
                    return LocalizationManager.Instance.GetText("phase_end");
                default:
                    return "Unknown Phase";
            }
        }

        /// <summary>
        /// 创建玩家信息面板
        /// </summary>
        public void CreatePlayerInfoPanels(List<Player> players)
        {
            if (playerInfoContainer == null || playerInfoPrefab == null) return;

            // 清空现有面板
            foreach (var panel in playerInfoPanels.Values)
            {
                Destroy(panel);
            }
            playerInfoPanels.Clear();

            // 为每个玩家创建面板
            foreach (var player in players)
            {
                GameObject panel = Instantiate(playerInfoPrefab, playerInfoContainer);
                playerInfoPanels[player] = panel;
                UpdatePlayerInfo(player, panel);
            }
        }

        /// <summary>
        /// 更新玩家信息
        /// </summary>
        private void UpdatePlayerInfo(Player player, GameObject panel)
        {
            if (panel == null) return;

            // 假设预制体有这些组件
            TextMeshProUGUI nameText = panel.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI hpText = panel.transform.Find("HPText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI handCountText = panel.transform.Find("HandCountText")?.GetComponent<TextMeshProUGUI>();

            if (nameText != null)
            {
                nameText.text = player.playerName;
                nameText.color = player.isAlive ? Color.white : Color.gray;
            }

            if (hpText != null)
            {
                hpText.text = $"HP: {player.currentHP}/{player.maxHP}";
            }

            if (handCountText != null)
            {
                handCountText.text = $"手牌: {player.handCards.Count}";
            }
        }

        /// <summary>
        /// 结束出牌阶段按钮点击
        /// </summary>
        private void OnEndPhaseClicked()
        {
            if (BattleManager.Instance.currentPhase == TurnPhase.Play)
            {
                BattleManager.Instance.EndPlayPhase();
            }
        }

        /// <summary>
        /// 显示消息
        /// </summary>
        public void ShowMessage(string message)
        {
            Debug.Log($"[UI消息] {message}");
            // TODO: 实现UI消息显示
        }
    }
}
