using UnityEngine;

namespace ThreeKingdoms
{
    /// <summary>
    /// 卡牌名称辅助类
    /// 提供统一的卡牌名称判断方法，避免硬编码
    /// </summary>
    public static class CardNameHelper
    {
        // 卡牌名称常量（内部使用，不显示给玩家）
        public const string SLASH = "杀";
        public const string DODGE = "闪";
        public const string PEACH = "桃";
        public const string DUEL = "决斗";
        public const string SAVAGE_ASSAULT = "南蛮入侵";
        public const string ARROW_BARRAGE = "万箭齐发";
        public const string STEAL = "顺手牵羊";
        public const string DISMANTLE = "过河拆桥";
        public const string NULLIFICATION = "无懈可击";
        public const string PEACH_GARDEN = "桃园结义";

        /// <summary>
        /// 检查是否是杀
        /// </summary>
        public static bool IsSlash(Card card)
        {
            return card != null && card.cardName == SLASH;
        }

        /// <summary>
        /// 检查是否是闪
        /// </summary>
        public static bool IsDodge(Card card)
        {
            return card != null && card.cardName == DODGE;
        }

        /// <summary>
        /// 检查是否是桃
        /// </summary>
        public static bool IsPeach(Card card)
        {
            return card != null && card.cardName == PEACH;
        }

        /// <summary>
        /// 检查是否是决斗
        /// </summary>
        public static bool IsDuel(Card card)
        {
            return card != null && card.cardName == DUEL;
        }

        /// <summary>
        /// 检查是否是AOE伤害牌（南蛮入侵、万箭齐发）
        /// </summary>
        public static bool IsAOEDamage(Card card)
        {
            return card != null && (card.cardName == SAVAGE_ASSAULT || card.cardName == ARROW_BARRAGE);
        }

        /// <summary>
        /// 检查是否是基本牌
        /// </summary>
        public static bool IsBasicCard(Card card)
        {
            return card != null && card.cardType == CardType.Basic;
        }

        /// <summary>
        /// 检查是否是锦囊牌
        /// </summary>
        public static bool IsTrickCard(Card card)
        {
            return card != null && card.cardType == CardType.Trick;
        }

        /// <summary>
        /// 检查是否是装备牌
        /// </summary>
        public static bool IsEquipmentCard(Card card)
        {
            return card != null && card.cardType == CardType.Equipment;
        }

        /// <summary>
        /// 获取显示用的卡牌名称（本地化）
        /// </summary>
        public static string GetDisplayName(Card card)
        {
            if (card == null) return "";

            // 如果有LocalizationManager，返回本地化名称
            if (LocalizationManager.Instance != null)
            {
                return LocalizationManager.Instance.GetCardName(card.cardName);
            }

            // 否则返回原名称
            return card.cardName;
        }

        /// <summary>
        /// 从显示名称获取内部名称
        /// </summary>
        public static string GetInternalName(string displayName)
        {
            if (LocalizationManager.Instance != null)
            {
                string key = LocalizationManager.Instance.GetCardNameKey(displayName);
                if (key != null)
                {
                    // 根据key返回内部名称
                    switch (key)
                    {
                        case "card_slash": return SLASH;
                        case "card_dodge": return DODGE;
                        case "card_peach": return PEACH;
                        case "card_duel": return DUEL;
                        case "card_savage_assault": return SAVAGE_ASSAULT;
                        case "card_arrow_barrage": return ARROW_BARRAGE;
                    }
                }
            }

            return displayName;
        }

        /// <summary>
        /// 检查卡牌是否是红色
        /// </summary>
        public static bool IsRed(Card card)
        {
            return card != null && (card.suit == CardSuit.Heart || card.suit == CardSuit.Diamond);
        }

        /// <summary>
        /// 检查卡牌是否是黑色
        /// </summary>
        public static bool IsBlack(Card card)
        {
            return card != null && (card.suit == CardSuit.Spade || card.suit == CardSuit.Club);
        }
    }
}
