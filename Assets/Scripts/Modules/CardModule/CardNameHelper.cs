using UnityEngine;

namespace ThreeKingdoms
{
    /// <summary>
    /// 卡牌名称辅助类
    /// 提供卡牌判断和本地化功能
    /// </summary>
    public static class CardNameHelper
    {
        // ==================== 卡牌判断方法 ====================

        /// <summary>
        /// 是否为【杀】
        /// </summary>
        public static bool IsSlash(string cardName)
        {
            return cardName == "杀" || cardName == "Slash" || cardName == "살";
        }

        /// <summary>
        /// 是否为【杀】（重载：接受Card对象）
        /// </summary>
        public static bool IsSlash(Card card)
        {
            return card != null && IsSlash(card.cardName);
        }

        /// <summary>
        /// 是否为【闪】
        /// </summary>
        public static bool IsDodge(string cardName)
        {
            return cardName == "闪" || cardName == "Dodge" || cardName == "섬";
        }

        /// <summary>
        /// 是否为【闪】（重载：接受Card对象）
        /// </summary>
        public static bool IsDodge(Card card)
        {
            return card != null && IsDodge(card.cardName);
        }

        /// <summary>
        /// 是否为【桃】
        /// </summary>
        public static bool IsPeach(string cardName)
        {
            return cardName == "桃" || cardName == "Peach" || cardName == "복숭아";
        }

        /// <summary>
        /// 是否为【桃】（重载：接受Card对象）
        /// </summary>
        public static bool IsPeach(Card card)
        {
            return card != null && IsPeach(card.cardName);
        }

        /// <summary>
        /// 是否为基础牌
        /// </summary>
        public static bool IsBasicCard(string cardName)
        {
            return IsSlash(cardName) || IsDodge(cardName) || IsPeach(cardName);
        }

        /// <summary>
        /// 是否为锦囊牌
        /// </summary>
        public static bool IsTrickCard(string cardName)
        {
            return IsDuel(cardName) || IsSavageAssault(cardName) ||
                   IsArrowBarrage(cardName) || IsPeachGarden(cardName) ||
                   IsNullification(cardName) || IsSnatch(cardName) ||
                   IsDismantlement(cardName) || IsIndulgence(cardName) ||
                   IsLightning(cardName) || IsHarvest(cardName);
        }

        /// <summary>
        /// 是否为【决斗】
        /// </summary>
        public static bool IsDuel(string cardName)
        {
            return cardName == "决斗" || cardName == "Duel" || cardName == "결투";
        }

        /// <summary>
        /// 是否为【决斗】（重载：接受Card对象）
        /// </summary>
        public static bool IsDuel(Card card)
        {
            return card != null && IsDuel(card.cardName);
        }

        /// <summary>
        /// 是否为【南蛮入侵】
        /// </summary>
        public static bool IsSavageAssault(string cardName)
        {
            return cardName == "南蛮入侵" || cardName == "Savage Assault" || cardName == "남만침입";
        }

        /// <summary>
        /// 是否为【南蛮入侵】（重载：接受Card对象）
        /// </summary>
        public static bool IsSavageAssault(Card card)
        {
            return card != null && IsSavageAssault(card.cardName);
        }

        /// <summary>
        /// 是否为【万箭齐发】
        /// </summary>
        public static bool IsArrowBarrage(string cardName)
        {
            return cardName == "万箭齐发" || cardName == "Arrow Barrage" || cardName == "만전제발";
        }

        /// <summary>
        /// 是否为【万箭齐发】（重载：接受Card对象）
        /// </summary>
        public static bool IsArrowBarrage(Card card)
        {
            return card != null && IsArrowBarrage(card.cardName);
        }

        /// <summary>
        /// 是否为【桃园结义】
        /// </summary>
        public static bool IsPeachGarden(string cardName)
        {
            return cardName == "桃园结义" || cardName == "Peach Garden" || cardName == "도원결의";
        }

        /// <summary>
        /// 是否为【桃园结义】（重载：接受Card对象）
        /// </summary>
        public static bool IsPeachGarden(Card card)
        {
            return card != null && IsPeachGarden(card.cardName);
        }

        /// <summary>
        /// 是否为【无懈可击】
        /// </summary>
        public static bool IsNullification(string cardName)
        {
            return cardName == "无懈可击" || cardName == "Nullification" || cardName == "무해가격";
        }

        /// <summary>
        /// 是否为【无懈可击】（重载：接受Card对象）
        /// </summary>
        public static bool IsNullification(Card card)
        {
            return card != null && IsNullification(card.cardName);
        }

        /// <summary>
        /// 是否为【顺手牵羊】
        /// </summary>
        public static bool IsSnatch(string cardName)
        {
            return cardName == "顺手牵羊" || cardName == "Snatch" || cardName == "순수견양";
        }

        /// <summary>
        /// 是否为【顺手牵羊】（重载：接受Card对象）
        /// </summary>
        public static bool IsSnatch(Card card)
        {
            return card != null && IsSnatch(card.cardName);
        }

        /// <summary>
        /// 是否为【过河拆桥】
        /// </summary>
        public static bool IsDismantlement(string cardName)
        {
            return cardName == "过河拆桥" || cardName == "Dismantlement" || cardName == "과하철교";
        }

        /// <summary>
        /// 是否为【过河拆桥】（重载：接受Card对象）
        /// </summary>
        public static bool IsDismantlement(Card card)
        {
            return card != null && IsDismantlement(card.cardName);
        }

        /// <summary>
        /// 是否为【乐不思蜀】
        /// </summary>
        public static bool IsIndulgence(string cardName)
        {
            return cardName == "乐不思蜀" || cardName == "Indulgence" || cardName == "락불사촉";
        }

        /// <summary>
        /// 是否为【乐不思蜀】（重载：接受Card对象）
        /// </summary>
        public static bool IsIndulgence(Card card)
        {
            return card != null && IsIndulgence(card.cardName);
        }

        /// <summary>
        /// 是否为【闪电】
        /// </summary>
        public static bool IsLightning(string cardName)
        {
            return cardName == "闪电" || cardName == "Lightning" || cardName == "번개";
        }

        /// <summary>
        /// 是否为【闪电】（重载：接受Card对象）
        /// </summary>
        public static bool IsLightning(Card card)
        {
            return card != null && IsLightning(card.cardName);
        }

        /// <summary>
        /// 是否为【五谷丰登】
        /// </summary>
        public static bool IsHarvest(string cardName)
        {
            return cardName == "五谷丰登" || cardName == "Harvest" || cardName == "오곡풍등";
        }

        /// <summary>
        /// 是否为【五谷丰登】（重载：接受Card对象）
        /// </summary>
        public static bool IsHarvest(Card card)
        {
            return card != null && IsHarvest(card.cardName);
        }

        /// <summary>
        /// 是否为延时锦囊
        /// </summary>
        public static bool IsDelayedTrick(string cardName)
        {
            return IsIndulgence(cardName) || IsLightning(cardName);
        }

        /// <summary>
        /// 是否为AOE锦囊（群体伤害/效果）
        /// </summary>
        public static bool IsAOETrick(string cardName)
        {
            return IsSavageAssault(cardName) || IsArrowBarrage(cardName) ||
                   IsPeachGarden(cardName) || IsHarvest(cardName);
        }

        // ==================== 本地化方法 ====================

        /// <summary>
        /// 获取卡牌的本地化名称
        /// </summary>
        public static string GetLocalizedCardName(string cardName)
        {
            if (LocalizationManager.Instance == null)
            {
                return cardName; // 如果本地化系统未初始化，返回原名
            }

            // 映射中文卡牌名到本地化key
            string key = GetCardLocalizationKey(cardName);
            if (!string.IsNullOrEmpty(key))
            {
                return LocalizationManager.Instance.GetText(key);
            }

            return cardName;
        }

        /// <summary>
        /// 获取卡牌的本地化key
        /// </summary>
        private static string GetCardLocalizationKey(string cardName)
        {
            // 基础牌
            if (IsSlash(cardName)) return "card_slash";
            if (IsDodge(cardName)) return "card_dodge";
            if (IsPeach(cardName)) return "card_peach";

            // 锦囊牌
            if (IsDuel(cardName)) return "card_duel";
            if (IsSavageAssault(cardName)) return "card_savage_assault";
            if (IsArrowBarrage(cardName)) return "card_arrow_barrage";
            if (IsPeachGarden(cardName)) return "card_peach_garden";
            if (IsNullification(cardName)) return "card_nullification";
            if (IsSnatch(cardName)) return "card_snatch";
            if (IsDismantlement(cardName)) return "card_dismantlement";
            if (IsIndulgence(cardName)) return "card_indulgence";
            if (IsLightning(cardName)) return "card_lightning";
            if (IsHarvest(cardName)) return "card_harvest";

            return null;
        }

        /// <summary>
        /// 获取花色的本地化符号
        /// </summary>
        public static string GetLocalizedSuit(CardSuit suit)
        {
            if (LocalizationManager.Instance == null)
            {
                return GetSuitSymbol(suit); // 返回默认符号
            }

            switch (suit)
            {
                case CardSuit.Spade:
                    return LocalizationManager.Instance.GetText("suit_spade");
                case CardSuit.Heart:
                    return LocalizationManager.Instance.GetText("suit_heart");
                case CardSuit.Club:
                    return LocalizationManager.Instance.GetText("suit_club");
                case CardSuit.Diamond:
                    return LocalizationManager.Instance.GetText("suit_diamond");
                default:
                    return "";
            }
        }

        /// <summary>
        /// 获取点数的本地化文本
        /// </summary>
        public static string GetLocalizedPoint(int point)
        {
            if (LocalizationManager.Instance == null)
            {
                return GetPointString(point); // 返回默认文本
            }

            switch (point)
            {
                case 1:
                    return LocalizationManager.Instance.GetText("point_a");
                case 11:
                    return LocalizationManager.Instance.GetText("point_j");
                case 12:
                    return LocalizationManager.Instance.GetText("point_q");
                case 13:
                    return LocalizationManager.Instance.GetText("point_k");
                default:
                    return point.ToString();
            }
        }

        /// <summary>
        /// 获取完整的卡牌显示名称（包含花色和点数）
        /// </summary>
        public static string GetFullCardDisplay(Card card)
        {
            if (card == null) return "";

            string localizedName = GetLocalizedCardName(card.cardName);
            string localizedSuit = GetLocalizedSuit(card.suit);
            string localizedPoint = GetLocalizedPoint(card.point);

            return $"{localizedSuit}{localizedPoint} {localizedName}";
        }

        // ==================== 辅助方法（无本地化） ====================

        /// <summary>
        /// 获取花色符号（默认）
        /// </summary>
        private static string GetSuitSymbol(CardSuit suit)
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
        /// 获取点数字符串（默认）
        /// </summary>
        private static string GetPointString(int point)
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
        /// 获取卡牌类型的描述
        /// </summary>
        public static string GetCardTypeDescription(string cardName)
        {
            if (IsBasicCard(cardName))
            {
                return "基础牌"; // 可以改为本地化
            }
            else if (IsDelayedTrick(cardName))
            {
                return "延时锦囊";
            }
            else if (IsTrickCard(cardName))
            {
                return "锦囊牌";
            }

            return "未知类型";
        }

        /// <summary>
        /// 标准化卡牌名称（将不同语言的名称统一为中文用于内部判断）
        /// </summary>
        public static string NormalizeCardName(string cardName)
        {
            // 基础牌
            if (IsSlash(cardName)) return "杀";
            if (IsDodge(cardName)) return "闪";
            if (IsPeach(cardName)) return "桃";

            // 锦囊牌
            if (IsDuel(cardName)) return "决斗";
            if (IsSavageAssault(cardName)) return "南蛮入侵";
            if (IsArrowBarrage(cardName)) return "万箭齐发";
            if (IsPeachGarden(cardName)) return "桃园结义";
            if (IsNullification(cardName)) return "无懈可击";
            if (IsSnatch(cardName)) return "顺手牵羊";
            if (IsDismantlement(cardName)) return "过河拆桥";
            if (IsIndulgence(cardName)) return "乐不思蜀";
            if (IsLightning(cardName)) return "闪电";
            if (IsHarvest(cardName)) return "五谷丰登";

            return cardName; // 无法识别的保持原样
        }

        /// <summary>
        /// Debug输出：显示卡牌的完整信息
        /// </summary>
        public static void DebugCard(Card card, string prefix = "")
        {
            if (card == null)
            {
                Debug.Log($"{prefix}Card is null");
                return;
            }

            string fullDisplay = GetFullCardDisplay(card);
            string cardType = GetCardTypeDescription(card.cardName);

            Debug.Log($"{prefix}{fullDisplay} ({cardType})");
        }
    }
}