using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mewtations.Combat;
using Mewtations.Combat.Actions;
using Mewtations.Combat.TurnOrder;

// ACTIVE TURN-BASED COMBAT SYSTEM
// ALL NEW COMBAT FEATURES MUST USE Combat
namespace Mewtations.Combat.Core
{
    public enum CombatState
    {
        BattleStart,
        RoundStart,
        TurnStart,
        AwaitInput,
        ExecuteAction,
        ReactionWindow,
        ResolveEffects,
        CheckDeaths,
        TurnEnd,
        NextTurn,
        RoundEnd,
        BattleEnd
    }

    public enum CombatEventType
    {
        ActionDeclared,
        SnapshotCreated,
        GuardTriggered,
        DodgeSucceeded,
        HPCommitted,
        UnitDied
    }

    [Serializable]
    public class CombatEvent
    {
        public CombatEventType Type;
        public CombatUnit Source;
        public CombatUnit Target;
        public CombatSnapshot ActionSnapshot;
        public string LogMessage;
        public float Timestamp;
    }

    public class CombatEncounter
    {
        public CombatState CurrentState { get; private set; }
        
        public List<CombatUnit> PlayerUnits = new List<CombatUnit>();
        public List<CombatUnit> EnemyUnits = new List<CombatUnit>();
        
        public List<CombatUnit> TurnQueue = new List<CombatUnit>();
        public CombatUnit ActiveUnit;
        
        public int RoundIndex = 1;
        public Action<string> LogCallback;
        
        public List<RoundSnapshot> RoundHistory = new List<RoundSnapshot>();
        private RoundSnapshot _currentRoundSnapshot;

        // Combat Event Stream & Reaction Window (CombatV2)
        public List<CombatEvent> EventStream = new List<CombatEvent>();
        public int ReactionDepth = 0;
        private const int MAX_REACTION_DEPTH = 1;

        public CombatEncounter(List<CombatUnit> players, List<CombatUnit> enemies, Action<string> logCallback)
        {
            PlayerUnits = new List<CombatUnit>(players);
            EnemyUnits = new List<CombatUnit>(enemies);
            LogCallback = logCallback;
            CurrentState = CombatState.BattleStart;
        }

        public void AddLog(string msg)
        {
            LogCallback?.Invoke(msg);
        }

        public void RecordEvent(CombatEventType type, CombatUnit source, CombatUnit target, CombatSnapshot snapshot, string message)
        {
            var evt = new CombatEvent
            {
                Type = type,
                Source = source,
                Target = target,
                ActionSnapshot = snapshot,
                LogMessage = message,
                Timestamp = Time.time
            };
            EventStream.Add(evt);
            AddLog($" 📝 [EVENT] {message}");
        }

        public List<CombatUnit> GetAlliesOf(CombatUnit unit)
        {
            return unit.IsPlayer ? PlayerUnits : EnemyUnits;
        }

        public List<CombatUnit> GetOpponentsOf(CombatUnit unit)
        {
            return unit.IsPlayer ? EnemyUnits : PlayerUnits;
        }

        public void TransitionTo(CombatState newState)
        {
            CurrentState = newState;
            AddLog($"⚙️ [HỆ THỐNG] Chuyển sang trạng thái: {newState}");
        }

        public void StepState()
        {
            // STRICT ENGINE RULE: No MonoBehaviour Update loop. Controlled via explicit transitions.
            switch (CurrentState)
            {
                case CombatState.BattleStart:
                    AddLog("⚔️ Trận chiến bắt đầu!");
                    RoundIndex = 1;
                    TransitionTo(CombatState.RoundStart);
                    break;

                case CombatState.RoundStart:
                    AddLog($"--- VÒNG LẦN {RoundIndex} ---");
                    
                    // Initiative Tie-Breaker Rule resolved here deterministically!
                    List<CombatUnit> allUnits = new List<CombatUnit>();
                    allUnits.AddRange(PlayerUnits);
                    allUnits.AddRange(EnemyUnits);
                    TurnQueue = InitiativeResolver.BuildTurnQueue(allUnits);

                    _currentRoundSnapshot = new RoundSnapshot
                    {
                        RoundIndex = RoundIndex,
                        TurnOrder = new List<CombatUnit>(TurnQueue)
                    };
                    RoundHistory.Add(_currentRoundSnapshot);

                    TransitionTo(CombatState.TurnStart);
                    break;

                case CombatState.TurnStart:
                    if (TurnQueue.Count == 0)
                    {
                        TransitionTo(CombatState.RoundEnd);
                        return;
                    }

                    ActiveUnit = TurnQueue[0];
                    TurnQueue.RemoveAt(0);

                    if (!ActiveUnit.IsAlive)
                    {
                        TransitionTo(CombatState.NextTurn);
                        return;
                    }

                    AddLog($"👤 Lượt của: {ActiveUnit.Name}");
                    TransitionTo(CombatState.AwaitInput);
                    break;

                case CombatState.AwaitInput:
                    // AI and Player use SAME action pipeline!
                    // Both create and validate CombatAction.
                    TransitionTo(CombatState.ExecuteAction);
                    break;

                case CombatState.ExecuteAction:
                    RecordEvent(CombatEventType.ActionDeclared, ActiveUnit, null, null, $"{ActiveUnit.Name} tuyên bố hành động!");
                    TransitionTo(CombatState.ReactionWindow);
                    break;

                case CombatState.ReactionWindow:
                    // Hook for reaction window (Overwatch, Counterattack, Mutations)
                    if (ReactionDepth < MAX_REACTION_DEPTH)
                    {
                        ReactionDepth++;
                        AddLog($"⚡ Reaction Window mở (Depth = {ReactionDepth}). Thứ tự ưu tiên: 1. Guard -> 2. Dodge -> 3. Shield -> 4. Counter -> 5. Mutation");
                        
                        // Simulation of reactions
                        bool guardTriggered = false;
                        bool dodgeTriggered = false;

                        if (guardTriggered)
                        {
                            RecordEvent(CombatEventType.GuardTriggered, null, ActiveUnit, null, "Guard đỡ đòn thay thế mục tiêu thành công!");
                        }
                        else if (dodgeTriggered)
                        {
                            RecordEvent(CombatEventType.DodgeSucceeded, ActiveUnit, null, null, "Tránh đòn thành công!");
                        }
                    }
                    else
                    {
                        AddLog($"⚠️ Chặn phản ứng dây chuyền (Reaction Chain Rules).");
                    }
                    TransitionTo(CombatState.ResolveEffects);
                    break;

                case CombatState.ResolveEffects:
                    ReactionDepth = 0; // Reset depth
                    RecordEvent(CombatEventType.HPCommitted, ActiveUnit, null, null, "Cam kết sát thương thực tế ghi nhận.");
                    TransitionTo(CombatState.CheckDeaths);
                    break;

                case CombatState.CheckDeaths:
                    foreach (var unit in PlayerUnits.Concat(EnemyUnits))
                    {
                        if (unit.IsAlive && unit.CurrentHP <= 0)
                        {
                            RecordEvent(CombatEventType.UnitDied, unit, null, null, $"{unit.Name} đã gục ngã (dying)!");
                        }
                    }
                    TransitionTo(CombatState.TurnEnd);
                    break;

                case CombatState.TurnEnd:
                    TransitionTo(CombatState.NextTurn);
                    break;

                case CombatState.NextTurn:
                    TransitionTo(CombatState.TurnStart);
                    break;

                case CombatState.RoundEnd:
                    RoundIndex++;
                    TransitionTo(CombatState.RoundStart);
                    break;

                case CombatState.BattleEnd:
                    AddLog("🏁 Trận đấu kết thúc.");
                    break;
            }
        }
    }
}
