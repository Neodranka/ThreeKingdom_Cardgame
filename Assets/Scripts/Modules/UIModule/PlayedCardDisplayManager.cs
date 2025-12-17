using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ThreeKingdoms.UI
{
    /// <summary>
    /// 已打出卡牌展示管理器
    /// 管理出牌动画和展示区显示
    /// </summary>
    public class PlayedCardDisplayManager : MonoBehaviour
    {
        public static PlayedCardDisplayManager Instance { get; private set; }

        [Header("展示区设置")]
        [Tooltip("展示区容器")]
        public Transform displayContainer;

        [Tooltip("展示区最大卡牌数量")]
        public int maxDisplayCount = 5;

        [Tooltip("卡牌间距")]
        public float cardSpacing = 120f;

        [Header("动画设置")]
        [Tooltip("卡牌移动到展示区的时间")]
        public float moveToDisplayDuration = 0.3f;

        [Tooltip("卡牌淡出时间")]
        public float fadeOutDuration = 0.3f;

        [Tooltip("卡牌在展示区停留时间（秒）")]
        public float displayStayTime = 2f;

        [Header("卡牌预制体")]
        public GameObject displayCardPrefab;

        // 当前展示中的卡牌队列
        private Queue<DisplayedCard> displayedCards = new Queue<DisplayedCard>();

        // 显示中的卡牌数据结构
        private class DisplayedCard
        {
            public GameObject cardObject;
            public CanvasGroup canvasGroup;
            public float entryTime;
        }

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
            // 如果没有设置展示区容器，创建一个
            if (displayContainer == null)
            {
                CreateDisplayContainer();
            }
        }

        /// <summary>
        /// 创建展示区容器
        /// </summary>
        private void CreateDisplayContainer()
        {
            // 创建展示区
            GameObject containerObj = new GameObject("PlayedCardDisplayArea");
            containerObj.transform.SetParent(transform, false);

            RectTransform rt = containerObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0, 50); // 稍微偏上
            rt.sizeDelta = new Vector2(700, 200);

            // 添加水平布局
            HorizontalLayoutGroup hlg = containerObj.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = cardSpacing;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            displayContainer = containerObj.transform;

            Debug.Log("[PlayedCardDisplayManager] 创建了展示区容器");
        }

        /// <summary>
        /// 显示使用的卡牌（外部调用接口）
        /// </summary>
        /// <param name="card">使用的卡牌</param>
        /// <param name="user">使用者</param>
        /// <param name="startPosition">起始位置（可选，世界坐标）</param>
        public void ShowPlayedCard(Card card, Player user, Vector3? startPosition = null)
        {
            StartCoroutine(PlayCardDisplayAnimation(card, user, startPosition));
        }

        /// <summary>
        /// 播放卡牌展示动画
        /// </summary>
        private IEnumerator PlayCardDisplayAnimation(Card card, Player user, Vector3? startPosition)
        {
            // 如果已达到最大数量，先移除最早的卡牌
            if (displayedCards.Count >= maxDisplayCount)
            {
                yield return StartCoroutine(RemoveOldestCard());
            }

            // 创建展示用的卡牌UI
            GameObject cardObj = CreateDisplayCard(card, user);
            if (cardObj == null) yield break;

            RectTransform cardRT = cardObj.GetComponent<RectTransform>();
            CanvasGroup canvasGroup = cardObj.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = cardObj.AddComponent<CanvasGroup>();
            }

            // 设置初始位置（在屏幕下方）
            Vector3 initialPos;
            if (startPosition.HasValue)
            {
                initialPos = startPosition.Value;
            }
            else
            {
                // 默认从屏幕底部中央开始
                initialPos = new Vector3(Screen.width / 2, -100, 0);
            }

            // 转换为本地坐标
            cardRT.position = initialPos;
            canvasGroup.alpha = 0f;

            // 计算目标位置
            Vector3 targetPos = CalculateTargetPosition(displayedCards.Count);

            // 添加到队列
            DisplayedCard displayedCard = new DisplayedCard
            {
                cardObject = cardObj,
                canvasGroup = canvasGroup,
                entryTime = Time.time
            };
            displayedCards.Enqueue(displayedCard);

            // 播放进入动画
            yield return StartCoroutine(AnimateCardEntry(cardRT, canvasGroup, targetPos));

            // 重新排列所有卡牌
            RearrangeCards();
        }

        /// <summary>
        /// 创建展示用的卡牌
        /// </summary>
        private GameObject CreateDisplayCard(Card card, Player user)
        {
            GameObject cardObj;

            if (displayCardPrefab != null)
            {
                cardObj = Instantiate(displayCardPrefab, displayContainer);
            }
            else
            {
                // 动态创建简单的卡牌展示
                cardObj = CreateSimpleCardDisplay(card, user);
            }

            return cardObj;
        }

        /// <summary>
        /// 创建简单的卡牌展示（无预制体时）
        /// </summary>
        private GameObject CreateSimpleCardDisplay(Card card, Player user)
        {
            GameObject cardObj = new GameObject($"DisplayCard_{card.cardName}");
            cardObj.transform.SetParent(displayContainer, false);

            RectTransform rt = cardObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(100, 140);

            // 背景
            Image bg = cardObj.AddComponent<Image>();
            bg.color = GetCardBackgroundColor(card.cardType);

            // 卡牌名称
            GameObject nameObj = new GameObject("CardName");
            nameObj.transform.SetParent(cardObj.transform, false);
            RectTransform nameRT = nameObj.AddComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0, 0.3f);
            nameRT.anchorMax = new Vector2(1, 0.7f);
            nameRT.offsetMin = new Vector2(5, 0);
            nameRT.offsetMax = new Vector2(-5, 0);

            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = CardNameHelper.GetLocalizedCardName(card.cardName);
            nameText.fontSize = 18;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.color = Color.white;
            TMPFontHelper.SetFontByLanguage(nameText);

            // 花色和点数
            GameObject suitObj = new GameObject("SuitNumber");
            suitObj.transform.SetParent(cardObj.transform, false);
            RectTransform suitRT = suitObj.AddComponent<RectTransform>();
            suitRT.anchorMin = new Vector2(0, 0.7f);
            suitRT.anchorMax = new Vector2(1, 1f);
            suitRT.offsetMin = new Vector2(5, 0);
            suitRT.offsetMax = new Vector2(-5, -5);

            TextMeshProUGUI suitText = suitObj.AddComponent<TextMeshProUGUI>();
            suitText.text = $"{GetSuitSymbol(card.suit)} {GetNumberString(card.point)}";
            suitText.fontSize = 14;
            suitText.alignment = TextAlignmentOptions.TopLeft;
            suitText.color = GetSuitColor(card.suit);
            TMPFontHelper.SetFontByLanguage(suitText);

            // 使用者名称
            GameObject userObj = new GameObject("UserName");
            userObj.transform.SetParent(cardObj.transform, false);
            RectTransform userRT = userObj.AddComponent<RectTransform>();
            userRT.anchorMin = new Vector2(0, 0);
            userRT.anchorMax = new Vector2(1, 0.25f);
            userRT.offsetMin = new Vector2(5, 5);
            userRT.offsetMax = new Vector2(-5, 0);

            TextMeshProUGUI userText = userObj.AddComponent<TextMeshProUGUI>();
            userText.text = user?.playerName ?? "";
            userText.fontSize = 12;
            userText.alignment = TextAlignmentOptions.Center;
            userText.color = new Color(0.8f, 0.8f, 0.8f);
            TMPFontHelper.SetFontByLanguage(userText);

            return cardObj;
        }

        /// <summary>
        /// 获取卡牌背景颜色
        /// </summary>
        private Color GetCardBackgroundColor(CardType type)
        {
            switch (type)
            {
                case CardType.Basic:
                    return new Color(0.3f, 0.3f, 0.3f, 0.95f);
                case CardType.Trick:
                    return new Color(0.2f, 0.3f, 0.4f, 0.95f);
                case CardType.Equipment:
                    return new Color(0.4f, 0.3f, 0.2f, 0.95f);
                default:
                    return new Color(0.3f, 0.3f, 0.3f, 0.95f);
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
                default: return "?";
            }
        }

        /// <summary>
        /// 获取花色颜色
        /// </summary>
        private Color GetSuitColor(CardSuit suit)
        {
            switch (suit)
            {
                case CardSuit.Heart:
                case CardSuit.Diamond:
                    return Color.red;
                default:
                    return Color.white;
            }
        }

        /// <summary>
        /// 获取点数字符串
        /// </summary>
        private string GetNumberString(int number)
        {
            switch (number)
            {
                case 1: return "A";
                case 11: return "J";
                case 12: return "Q";
                case 13: return "K";
                default: return number.ToString();
            }
        }

        /// <summary>
        /// 计算目标位置
        /// </summary>
        private Vector3 CalculateTargetPosition(int index)
        {
            // 返回展示区容器的位置（由HorizontalLayoutGroup自动排列）
            return displayContainer.position;
        }

        /// <summary>
        /// 卡牌进入动画
        /// </summary>
        private IEnumerator AnimateCardEntry(RectTransform cardRT, CanvasGroup canvasGroup, Vector3 targetPos)
        {
            Vector3 startPos = cardRT.position;
            float elapsed = 0f;

            while (elapsed < moveToDisplayDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / moveToDisplayDuration;
                float smoothT = Mathf.SmoothStep(0, 1, t);

                // 位置插值
                cardRT.position = Vector3.Lerp(startPos, targetPos, smoothT);

                // 透明度插值
                canvasGroup.alpha = Mathf.Lerp(0, 1, smoothT);

                // 缩放效果
                float scale = Mathf.Lerp(0.5f, 1f, smoothT);
                cardRT.localScale = new Vector3(scale, scale, 1);

                yield return null;
            }

            // 确保最终状态
            canvasGroup.alpha = 1f;
            cardRT.localScale = Vector3.one;
        }

        /// <summary>
        /// 移除最早的卡牌
        /// </summary>
        private IEnumerator RemoveOldestCard()
        {
            if (displayedCards.Count == 0) yield break;

            DisplayedCard oldestCard = displayedCards.Dequeue();
            if (oldestCard.cardObject != null)
            {
                yield return StartCoroutine(AnimateCardExit(oldestCard));
                Destroy(oldestCard.cardObject);
            }
        }

        /// <summary>
        /// 卡牌退出动画
        /// </summary>
        private IEnumerator AnimateCardExit(DisplayedCard card)
        {
            if (card.canvasGroup == null) yield break;

            float elapsed = 0f;
            Vector3 startPos = card.cardObject.transform.position;
            Vector3 endPos = startPos + new Vector3(0, 100, 0); // 向上移动

            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeOutDuration;

                // 透明度淡出
                card.canvasGroup.alpha = Mathf.Lerp(1, 0, t);

                // 向上移动
                card.cardObject.transform.position = Vector3.Lerp(startPos, endPos, t);

                // 缩小
                float scale = Mathf.Lerp(1f, 0.5f, t);
                card.cardObject.transform.localScale = new Vector3(scale, scale, 1);

                yield return null;
            }
        }

        /// <summary>
        /// 重新排列展示区的卡牌
        /// </summary>
        private void RearrangeCards()
        {
            // HorizontalLayoutGroup会自动处理排列
            // 如果需要手动排列，在这里实现
        }

        /// <summary>
        /// 清空展示区
        /// </summary>
        public void ClearDisplay()
        {
            while (displayedCards.Count > 0)
            {
                DisplayedCard card = displayedCards.Dequeue();
                if (card.cardObject != null)
                {
                    Destroy(card.cardObject);
                }
            }
        }

        /// <summary>
        /// 定期清理过期的卡牌
        /// </summary>
        private void Update()
        {
            // 自动清理停留时间过长的卡牌
            if (displayedCards.Count > 0)
            {
                DisplayedCard oldestCard = displayedCards.Peek();
                if (Time.time - oldestCard.entryTime > displayStayTime)
                {
                    StartCoroutine(RemoveOldestCard());
                }
            }
        }

        private void OnDestroy()
        {
            ClearDisplay();
        }
    }
}
