using System.Collections.Generic;
using UnityEngine;

namespace ThreeKingdoms
{
    /// <summary>
    /// 牌堆管理器
    /// </summary>
    public class DeckManager : MonoBehaviour
    {
        public static DeckManager Instance { get; private set; }

        [Header("牌堆")]
        private List<Card> drawPile = new List<Card>();      // 摸牌堆
        private List<Card> discardPile = new List<Card>();   // 弃牌堆

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
            InitializeDeck();
        }

        /// <summary>
        /// 初始化牌堆
        /// </summary>
        public void InitializeDeck()
        {
            drawPile.Clear();
            discardPile.Clear();

            // 创建基本的三国杀卡牌
            CreateBasicCards();
            
            // 洗牌
            ShuffleDeck();

            Debug.Log($"牌堆初始化完成,共 {drawPile.Count} 张牌");
        }

        /// <summary>
        /// 创建基础卡牌
        /// </summary>
        private void CreateBasicCards()
        {
            // 【杀】- 30张
            for (int i = 0; i < 30; i++)
            {
                CardSuit suit = (CardSuit)(i % 4);
                int point = (i % 13) + 1;
                drawPile.Add(new Card("杀", CardType.Basic, suit, point));
            }

            // 【闪】- 15张
            for (int i = 0; i < 15; i++)
            {
                CardSuit suit = (CardSuit)(i % 4);
                int point = (i % 13) + 1;
                drawPile.Add(new Card("闪", CardType.Basic, suit, point));
            }

            // 【桃】- 8张
            for (int i = 0; i < 8; i++)
            {
                CardSuit suit = i < 4 ? CardSuit.Heart : CardSuit.Diamond;
                int point = (i % 13) + 1;
                drawPile.Add(new Card("桃", CardType.Basic, suit, point));
            }

            // 【决斗】- 3张
            for (int i = 0; i < 3; i++)
            {
                drawPile.Add(new Card("决斗", CardType.Trick, CardSuit.Spade, i + 1));
            }

            // 【南蛮入侵】- 3张
            for (int i = 0; i < 3; i++)
            {
                drawPile.Add(new Card("南蛮入侵", CardType.Trick, CardSuit.Spade, i + 7));
            }

            // 【万箭齐发】- 2张
            drawPile.Add(new Card("万箭齐发", CardType.Trick, CardSuit.Heart, 1));
            drawPile.Add(new Card("万箭齐发", CardType.Trick, CardSuit.Heart, 2));

            // 【无懈可击】- 3张
            for (int i = 0; i < 3; i++)
            {
                CardSuit suit = i % 2 == 0 ? CardSuit.Spade : CardSuit.Club;
                drawPile.Add(new Card("无懈可击", CardType.Trick, suit, 11));
            }

            // 【顺手牵羊】- 3张
            for (int i = 0; i < 3; i++)
            {
                drawPile.Add(new Card("顺手牵羊", CardType.Trick, CardSuit.Spade, i + 3));
            }

            // 【过河拆桥】- 3张
            for (int i = 0; i < 3; i++)
            {
                drawPile.Add(new Card("过河拆桥", CardType.Trick, CardSuit.Spade, i + 3));
            }
        }

        /// <summary>
        /// 洗牌
        /// </summary>
        public void ShuffleDeck()
        {
            for (int i = 0; i < drawPile.Count; i++)
            {
                int randomIndex = Random.Range(i, drawPile.Count);
                Card temp = drawPile[i];
                drawPile[i] = drawPile[randomIndex];
                drawPile[randomIndex] = temp;
            }
            Debug.Log("牌堆已洗牌");
        }

        /// <summary>
        /// 摸一张牌
        /// </summary>
        public Card DrawCard()
        {
            // 如果摸牌堆为空,将弃牌堆洗入摸牌堆
            if (drawPile.Count == 0)
            {
                if (discardPile.Count == 0)
                {
                    Debug.LogWarning("牌堆和弃牌堆都为空!");
                    return null;
                }

                Debug.Log("摸牌堆为空,将弃牌堆洗入摸牌堆");
                drawPile.AddRange(discardPile);
                discardPile.Clear();
                ShuffleDeck();
            }

            Card card = drawPile[0];
            drawPile.RemoveAt(0);
            return card;
        }

        /// <summary>
        /// 摸多张牌
        /// </summary>
        public List<Card> DrawCards(int count)
        {
            List<Card> cards = new List<Card>();
            for (int i = 0; i < count; i++)
            {
                Card card = DrawCard();
                if (card != null)
                {
                    cards.Add(card);
                }
            }
            return cards;
        }

        /// <summary>
        /// 弃牌
        /// </summary>
        public void DiscardCard(Card card)
        {
            if (card != null)
            {
                discardPile.Add(card);
            }
        }

        /// <summary>
        /// 获取牌堆剩余数量
        /// </summary>
        public int GetDrawPileCount()
        {
            return drawPile.Count;
        }

        /// <summary>
        /// 获取弃牌堆数量
        /// </summary>
        public int GetDiscardPileCount()
        {
            return discardPile.Count;
        }
    }
}
