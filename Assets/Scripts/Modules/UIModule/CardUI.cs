using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace ThreeKingdoms.UI
{
    /// <summary>
    /// 卡牌UI控制器
    /// </summary>
    public class CardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("UI引用")]
        public Image cardBackground;
        public Image suitIcon;
        public TextMeshProUGUI cardNameText;
        public TextMeshProUGUI pointText;
        public TextMeshProUGUI suitText;
        public GameObject selectedBorder;

        [Header("状态")]
        public Card cardData;
        public bool isSelected = false;
        public bool isInteractable = true;

        [Header("动画")]
        public float hoverScale = 1.1f;
        public float hoverYOffset = 20f; // Y轴偏移量
        public float animationSpeed = 5f;

        private Vector3 originalScale;
        private Vector2 originalPosition; // 保存完整的原始位置
        private RectTransform rectTransform;
        private Canvas hoverCanvas; // 用于控制渲染顺序
        private GraphicRaycaster graphicRaycaster;
        private bool isHovering = false;
        private bool positionInitialized = false;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            originalScale = transform.localScale;

            if (selectedBorder != null)
            {
                selectedBorder.SetActive(false);
            }
        }

        private void Start()
        {
            // 延迟保存位置,等待Layout计算完成
            StartCoroutine(InitializePosition());
        }

        private System.Collections.IEnumerator InitializePosition()
        {
            // 等待至少2帧,确保Layout完全计算完成
            yield return null;
            yield return null;

            originalPosition = rectTransform.anchoredPosition;
            positionInitialized = true;
        }

        /// <summary>
        /// 刷新原始位置(用于布局改变后)
        /// </summary>
        public void RefreshOriginalPosition()
        {
            positionInitialized = false;
            StartCoroutine(InitializePosition());
        }

        private void Update()
        {
            // 等待位置初始化完成
            if (!positionInitialized) return;

            // 平滑动画效果
            if (isHovering || isSelected)
            {
                // 目标位置: 上移
                Vector2 targetPos = originalPosition + new Vector2(0, hoverYOffset);
                rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, targetPos, Time.deltaTime * animationSpeed);

                // 目标缩放
                transform.localScale = Vector3.Lerp(transform.localScale, originalScale * hoverScale, Time.deltaTime * animationSpeed);
            }
            else
            {
                // 目标位置: 原始位置
                rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, originalPosition, Time.deltaTime * animationSpeed);

                // 目标缩放
                transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.deltaTime * animationSpeed);
            }
        }

        /// <summary>
        /// 设置卡牌数据
        /// </summary>
        public void SetCard(Card card)
        {
            cardData = card;

            if (cardNameText != null)
            {
                // 使用本地化卡牌名称
                cardNameText.text = CardNameHelper.GetLocalizedCardName(card.cardName);
            }

            if (pointText != null)
            {
                // 使用本地化点数
                pointText.text = CardNameHelper.GetLocalizedPoint(card.point);
            }

            if (suitText != null)
            {
                // 使用本地化花色
                suitText.text = CardNameHelper.GetLocalizedSuit(card.suit);
            }

            // 设置字体
            if (LocalizationManager.Instance != null)
            {
                TMPFontHelper.SetFontByLanguage(cardNameText);
            }

            UpdateDisplay();

            // 重新初始化位置
            positionInitialized = false;
            StartCoroutine(InitializePosition());
        }

        /// <summary>
        /// 更新显示
        /// </summary>
        private void UpdateDisplay()
        {
            if (cardData == null) return;

            // 设置卡牌名称
            if (cardNameText != null)
            {
                cardNameText.text = cardData.cardName;
            }

            // 设置点数
            if (pointText != null)
            {
                pointText.text = GetPointText(cardData.point);
            }

            // 设置花色
            if (suitText != null)
            {
                suitText.text = GetSuitSymbol(cardData.suit);
            }

            // 设置背景颜色
            if (cardBackground != null)
            {
                Color bgColor = GetCardTypeColor(cardData.cardType);
                cardBackground.color = bgColor;
            }

            // 设置花色颜色
            Color suitColor = cardData.IsRed() ? new Color(0.8f, 0.2f, 0.2f) : Color.black;
            if (suitText != null)
            {
                suitText.color = suitColor;
            }
            if (pointText != null)
            {
                pointText.color = suitColor;
            }
        }

        /// <summary>
        /// 获取点数文本
        /// </summary>
        private string GetPointText(int point)
        {
            switch (point)
            {
                case 1: return "A";
                case 11: return "J";
                case 12: return "Q";
                case 13: return "K";
                default: return point.ToString();
            }
        }

        /// <summary>
        /// 获取花色符号
        /// </summary>
        private string GetSuitSymbol(CardSuit suit)
        {
            switch (suit)
            {
                case CardSuit.Spade: return "♠";
                case CardSuit.Heart: return "♥";
                case CardSuit.Club: return "♣";
                case CardSuit.Diamond: return "♦";
                default: return "";
            }
        }

        /// <summary>
        /// 获取卡牌类型颜色
        /// </summary>
        private Color GetCardTypeColor(CardType type)
        {
            switch (type)
            {
                case CardType.Basic:
                    return new Color(0.95f, 0.95f, 0.85f); // 米黄色
                case CardType.Trick:
                    return new Color(0.85f, 0.92f, 0.95f); // 浅蓝色
                case CardType.Equipment:
                    return new Color(0.9f, 0.85f, 0.95f); // 浅紫色
                default:
                    return Color.white;
            }
        }

        /// <summary>
        /// 鼠标进入
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!isInteractable) return;

            isHovering = true;

            // 创建临时Canvas来提升渲染层级
            if (hoverCanvas == null)
            {
                hoverCanvas = gameObject.AddComponent<Canvas>();
                hoverCanvas.overrideSorting = true;
                graphicRaycaster = gameObject.AddComponent<GraphicRaycaster>();
            }
            hoverCanvas.sortingOrder = 100; // 提升到最前面
        }

        /// <summary>
        /// 鼠标离开
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isInteractable) return;

            isHovering = false;

            // 如果没有选中,移除临时Canvas - 这样不会影响层级
            if (!isSelected && hoverCanvas != null)
            {
                // 关键:必须先删除GraphicRaycaster,因为它依赖Canvas
                if (graphicRaycaster != null)
                {
                    Destroy(graphicRaycaster);
                    graphicRaycaster = null;
                }
                // 然后再删除Canvas
                Destroy(hoverCanvas);
                hoverCanvas = null;
            }
        }

        /// <summary>
        /// 点击卡牌
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (!isInteractable) return;

            ToggleSelect();
        }

        /// <summary>
        /// 切换选中状态
        /// </summary>
        public void ToggleSelect()
        {
            isSelected = !isSelected;

            if (selectedBorder != null)
            {
                selectedBorder.SetActive(isSelected);
            }

            // 选中时保持Canvas层级
            if (isSelected)
            {
                if (hoverCanvas == null)
                {
                    hoverCanvas = gameObject.AddComponent<Canvas>();
                    hoverCanvas.overrideSorting = true;
                    graphicRaycaster = gameObject.AddComponent<GraphicRaycaster>();
                }
                hoverCanvas.sortingOrder = 100;
            }
            else
            {
                // 取消选中时移除Canvas
                if (hoverCanvas != null && !isHovering)
                {
                    // 关键:必须先删除GraphicRaycaster,因为它依赖Canvas
                    if (graphicRaycaster != null)
                    {
                        Destroy(graphicRaycaster);
                        graphicRaycaster = null;
                    }
                    // 然后再删除Canvas
                    Destroy(hoverCanvas);
                    hoverCanvas = null;
                }
            }

            // 通知BattleUI
            BattleUI battleUI = FindObjectOfType<BattleUI>();
            if (battleUI != null)
            {
                battleUI.OnCardSelected(this);
            }
        }

        /// <summary>
        /// 设置选中状态
        /// </summary>
        public void SetSelected(bool selected)
        {
            if (isSelected == selected) return;

            isSelected = selected;
            if (selectedBorder != null)
            {
                selectedBorder.SetActive(selected);
            }

            if (selected)
            {
                if (hoverCanvas == null)
                {
                    hoverCanvas = gameObject.AddComponent<Canvas>();
                    hoverCanvas.overrideSorting = true;
                    graphicRaycaster = gameObject.AddComponent<GraphicRaycaster>();
                }
                hoverCanvas.sortingOrder = 100;
            }
            else
            {
                // 取消选中时移除Canvas
                if (hoverCanvas != null && !isHovering)
                {
                    // 关键:必须先删除GraphicRaycaster,因为它依赖Canvas
                    if (graphicRaycaster != null)
                    {
                        Destroy(graphicRaycaster);
                        graphicRaycaster = null;
                    }
                    // 然后再删除Canvas
                    Destroy(hoverCanvas);
                    hoverCanvas = null;
                }
            }
        }

        /// <summary>
        /// 设置可交互性
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            isInteractable = interactable;

            // 调整透明度
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = interactable ? 1f : 0.5f;
        }

        /// <summary>
        /// 播放使用动画
        /// </summary>
        public void PlayUseAnimation(System.Action onComplete = null)
        {
            // 简单的淡出动画 - 使用协程替代LeanTween
            StartCoroutine(FadeOut(onComplete));
        }

        /// <summary>
        /// 淡出动画协程
        /// </summary>
        private System.Collections.IEnumerator FadeOut(System.Action onComplete)
        {
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            float duration = 0.3f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = 1f - (elapsed / duration);
                yield return null;
            }

            canvasGroup.alpha = 0f;
            onComplete?.Invoke();
            Destroy(gameObject);
        }
    }
}