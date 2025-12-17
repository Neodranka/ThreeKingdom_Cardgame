using UnityEngine;
using System;

namespace ThreeKingdoms
{
    /// <summary>
    /// 游戏事件管理器
    /// 用于技能系统监听游戏中的各种事件
    /// </summary>
    public class EventManager : MonoBehaviour
    {
        public static EventManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Debug.Log("[EventManager] 事件系统初始化完成");
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // ===== 伤害相关事件 =====
        
        /// <summary>
        /// 玩家受到伤害后
        /// 参数：受伤者, 伤害来源, 伤害值, 造成伤害的牌
        /// </summary>
        public event Action<Player, Player, int, Card> OnPlayerDamaged;

        public void TriggerPlayerDamaged(Player victim, Player source, int damage, Card damageCard)
        {
            Debug.Log($"[事件] {victim.generalName} 受到 {damage} 点伤害");
            OnPlayerDamaged?.Invoke(victim, source, damage, damageCard);
        }

        // ===== 回合相关事件 =====
        
        /// <summary>
        /// 回合开始
        /// 参数：当前回合玩家
        /// </summary>
        public event Action<Player> OnTurnStart;

        public void TriggerTurnStart(Player player)
        {
            Debug.Log($"[事件] {player.generalName} 的回合开始");
            OnTurnStart?.Invoke(player);
        }

        /// <summary>
        /// 回合结束
        /// </summary>
        public event Action<Player> OnTurnEnd;

        public void TriggerTurnEnd(Player player)
        {
            Debug.Log($"[事件] {player.generalName} 的回合结束");
            OnTurnEnd?.Invoke(player);
        }

        // ===== 出牌相关事件 =====
        
        /// <summary>
        /// 玩家使用卡牌
        /// 参数：使用者, 卡牌, 目标（可能为null）
        /// </summary>
        public event Action<Player, Card, Player> OnCardUsed;

        public void TriggerCardUsed(Player user, Card card, Player target = null)
        {
            Debug.Log($"[事件] {user.generalName} 使用了 {card.cardName}");
            OnCardUsed?.Invoke(user, card, target);
        }

        // ===== 死亡相关事件 =====
        
        /// <summary>
        /// 玩家死亡
        /// 参数：死亡者, 击杀者
        /// </summary>
        public event Action<Player, Player> OnPlayerDeath;

        public void TriggerPlayerDeath(Player victim, Player killer)
        {
            Debug.Log($"[事件] {victim.generalName} 死亡");
            OnPlayerDeath?.Invoke(victim, killer);
        }

        // ===== 摸牌相关事件 =====
        
        /// <summary>
        /// 玩家摸牌
        /// </summary>
        public event Action<Player, Card> OnCardDrawn;

        public void TriggerCardDrawn(Player player, Card card)
        {
            OnCardDrawn?.Invoke(player, card);
        }

        // ===== 弃牌相关事件 =====

        /// <summary>
        /// 玩家弃牌
        /// </summary>
        public event Action<Player, Card> OnCardDiscarded;

        public void TriggerCardDiscarded(Player player, Card card)
        {
            OnCardDiscarded?.Invoke(player, card);
        }

        // ===== 摸牌阶段事件 =====

        /// <summary>
        /// 摸牌阶段
        /// </summary>
        public event Action<Player> OnDrawPhase;

        public void TriggerDrawPhase(Player player)
        {
            Debug.Log($"[事件] {player.generalName} 的摸牌阶段");
            OnDrawPhase?.Invoke(player);
        }

        // ===== 出牌阶段事件 =====

        /// <summary>
        /// 玩家出牌（简化版，只传牌）
        /// </summary>
        public event Action<Player, Card> OnCardPlayed;

        public void TriggerCardPlayed(Player player, Card card)
        {
            Debug.Log($"[事件] {player.generalName} 打出了 {card.cardName}");
            OnCardPlayed?.Invoke(player, card);
        }

        // ===== 技能相关事件 =====

        /// <summary>
        /// 技能触发
        /// </summary>
        public event Action<Player, string> OnSkillTriggered;

        public void TriggerSkillTriggered(Player player, string skillId)
        {
            Debug.Log($"[事件] {player.generalName} 发动了技能 {skillId}");
            OnSkillTriggered?.Invoke(player, skillId);
        }

        // ===== 故事模式专用事件 =====

        /// <summary>
        /// 故事事件触发
        /// </summary>
        public event Action<string, string> OnStoryEventTriggered;

        public void TriggerStoryEvent(string eventType, string param)
        {
            Debug.Log($"[故事事件] {eventType}: {param}");
            OnStoryEventTriggered?.Invoke(eventType, param);
        }

        /// <summary>
        /// 玩家死亡（简化版）
        /// </summary>
        public void TriggerPlayerDeath(Player victim)
        {
            TriggerPlayerDeath(victim, null);
        }
    }
}
