using UnityEngine;

namespace ThreeKingdoms
{
    /// <summary>
    /// 卡牌类型
    /// </summary>
    public enum CardType
    {
        Basic,      // 基本牌
        Trick,      // 锦囊牌
        Equipment   // 装备牌
    }

    /// <summary>
    /// 装备子类型
    /// </summary>
    public enum EquipmentType
    {
        None,           // 非装备
        Weapon,         // 武器（攻击范围）
        Armor,          // 防具
        DefensiveHorse, // +1马（防御马，他人计算与你距离+1）
        OffensiveHorse  // -1马（进攻马，你计算与他人距离-1）
    }

    /// <summary>
    /// 卡牌花色
    /// </summary>
    public enum CardSuit
    {
        Spade,      // 黑桃
        Heart,      // 红桃
        Club,       // 梅花
        Diamond     // 方片
    }

    /// <summary>
    /// 卡牌基础类
    /// </summary>
    [System.Serializable]
    public class Card
    {
        public string cardName;          // 卡牌名称
        public CardType cardType;        // 卡牌类型
        public CardSuit suit;            // 花色
        public int point;                // 点数 (1-13)
        public string description;       // 描述
        public Sprite cardSprite;        // 卡牌图片
        public EquipmentType equipmentType; // ⭐ 装备子类型
        public int equipmentValue;       // ⭐ 装备数值（武器=攻击范围，马=距离修正）

        public Card(string name, CardType type, CardSuit suit, int point)
        {
            this.cardName = name;
            this.cardType = type;
            this.suit = suit;
            this.point = point;
            this.equipmentType = EquipmentType.None;
            this.equipmentValue = 0;
        }

        /// <summary>
        /// ⭐ 装备牌构造函数
        /// </summary>
        public Card(string name, CardSuit suit, int point, EquipmentType eqType, int eqValue = 1)
        {
            this.cardName = name;
            this.cardType = CardType.Equipment;
            this.suit = suit;
            this.point = point;
            this.equipmentType = eqType;
            this.equipmentValue = eqValue;
        }

        /// <summary>
        /// 获取卡牌显示文本
        /// </summary>
        public string GetDisplayText()
        {
            string suitSymbol = GetSuitSymbol();
            return $"{cardName} {suitSymbol}{point}";
        }

        /// <summary>
        /// 获取花色符号
        /// </summary>
        private string GetSuitSymbol()
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
        /// 判断是否为红色牌
        /// </summary>
        public bool IsRed()
        {
            return suit == CardSuit.Heart || suit == CardSuit.Diamond;
        }

        /// <summary>
        /// 判断是否为黑色牌
        /// </summary>
        public bool IsBlack()
        {
            return suit == CardSuit.Spade || suit == CardSuit.Club;
        }
    }
}
