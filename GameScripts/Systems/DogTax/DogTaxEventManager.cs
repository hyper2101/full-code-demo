using UnityEngine;
using Mewtations.Core;
using Mewtations.Combat.Encounters;
using Mewtations.Combat.Core;
using GameScripts.Systems.Threat;
using System.Collections;
using Systems.Narrative;
namespace GameScripts.Systems.DogTax
{
    public enum DogTaxCycleState
    {
        Inactive,
        Debt,
        Threat,
        Resolved
    }

    public class DogTaxEventManager : MonoBehaviour
    {
        public static DogTaxEventManager Instance;

        public int NextTaxMonth = 10;
        public DogTaxCycleState CurrentState = DogTaxCycleState.Inactive;
        
        // Runtime instances
        private DebtData _activeDebt;
        private ThreatInstance _activeThreat;
        private GameCard _spawnedDebtCard;
        private GameCard _spawnedThreatCard;

        // Anti-Double Trigger Lock
        private bool _isResolving = false;

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        // Called by WorldManager or DayEventSystem
        public void OnMonthTick(int currentMonth)
        {
            if (_isResolving) return;

            if (CurrentState == DogTaxCycleState.Inactive && currentMonth >= NextTaxMonth)
            {
                TriggerDogTaxDialogue();
            }
            else if (CurrentState == DogTaxCycleState.Debt && _activeDebt != null)
            {
                if (currentMonth >= _activeDebt.ExpirationMonth)
                {
                    HandleDebtExpired();
                }
            }
            else if (CurrentState == DogTaxCycleState.Threat && _activeThreat != null)
            {
                if (currentMonth >= _activeThreat.ThreatExpiryMonth)
                {
                    HandleThreatExpired();
                }
            }
        }

        private void TriggerDogTaxDialogue()
        {
            Debug.Log($"[DogTax] Triggering Narrative Event dog_tax_t1_intro");

            // Create prototype data dynamically. Normally this is a ScriptableObject asset
            var eventData = ScriptableObject.CreateInstance<NarrativeEventData>();
            eventData.EventID = "dog_tax_t1_intro";
            eventData.PortraitLeftID = "player";
            eventData.PortraitRightID = "dog_mafia_t1";
            
            eventData.Lines.Add(new DialogueLine { SpeakerId = "dog_mafia_t1", TextKey = "dogtax_t1_intro_01" });
            eventData.Lines.Add(new DialogueLine { SpeakerId = "dog_mafia_t1", TextKey = "dogtax_t2_warning" });
            
            eventData.Choices.Add(new DialogueChoice { TextKey = "dogtax_pay", ResultActionId = "resolve_dogtax_pay" });
            eventData.Choices.Add(new DialogueChoice { TextKey = "dogtax_refuse", ResultActionId = "trigger_dogtax_combat" });

            if (NarrativeEventSystem.Instance != null)
            {
                NarrativeEventSystem.Instance.TriggerEvent(eventData);
            }
            else
            {
                Debug.LogError("NarrativeEventSystem.Instance is missing!");
            }
        }

        public void SpawnDebt(Severity severity)
        {
            if (_isResolving) return;
            
            CurrentState = DogTaxCycleState.Debt;
            int currentMonth = WorldManager.instance != null ? WorldManager.instance.CurrentMonth : 0;
            _activeDebt = new DebtData
            {
                Severity = severity,
                ExpirationMonth = currentMonth + 3 // 3 months to pay
            };
            
            int resourceValue = severity == Severity.Normal ? 10 : (severity == Severity.Escalated ? 15 : 20);
            // Generate resource requirements
            for(int i=0; i < resourceValue; i++)
            {
                _activeDebt.RequiredResources.Add(Random.value > 0.5f ? "resource_crystal" : "resource_ore");
            }
            
            Debug.Log($"[DogTax] Debt Generated: {_activeDebt.RequiredResources.Count} resources needed.");
            
            // Spawn DebtCardComponent on board
            if (WorldManager.instance != null && WorldManager.instance.CurrentBoard != null)
            {
                Vector3 spawnPos = WorldManager.instance.CurrentBoard.MiddleOfBoard() + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
                
                GameCard spawnedCard = WorldManager.instance.CreateCard(spawnPos, "dogtax_debt", true, true, true);
                if (spawnedCard != null)
                {
                    _spawnedDebtCard = spawnedCard;
                    var debtComp = spawnedCard.GetComponent<GameScripts.Systems.Threat.UI.DebtCardComponent>();
                    if (debtComp == null)
                    {
                        debtComp = spawnedCard.gameObject.AddComponent<GameScripts.Systems.Threat.UI.DebtCardComponent>();
                    }
                    
                    debtComp.Initialize(_activeDebt.RequiredResources, OnDebtPaid);
                    
                    // Focus Camera
                    StartCoroutine(FocusCameraOnCardRoutine(spawnedCard, 3f));
                }
            }
        }

        public void AddDebtAmount(int resourceCount)
        {
            if (_isResolving) return;

            if (CurrentState == DogTaxCycleState.Debt && _activeDebt != null && _spawnedDebtCard != null && !_spawnedDebtCard.Destroyed)
            {
                for (int i = 0; i < resourceCount; i++)
                {
                    _activeDebt.RequiredResources.Add(Random.value > 0.5f ? "resource_crystal" : "resource_ore");
                }
                var debtComp = _spawnedDebtCard.GetComponent<GameScripts.Systems.Threat.UI.DebtCardComponent>();
                if (debtComp != null)
                {
                    debtComp.Initialize(_activeDebt.RequiredResources, OnDebtPaid);
                }
                Debug.Log($"[DogTax] Debt Increased: Now {_activeDebt.RequiredResources.Count} resources needed.");
            }
            else if (CurrentState != DogTaxCycleState.Threat && CurrentState != DogTaxCycleState.Debt)
            {
                SpawnDebt(Severity.Normal);
            }
        }

        private IEnumerator FocusCameraOnCardRoutine(GameCard targetCard, float duration)
        {
            if (GameCamera.instance != null && targetCard != null)
            {
                GameCamera.instance.FocusOn(targetCard.CardData);
                yield return new WaitForSeconds(duration);
                // After duration, we don't necessarily reset, we just let player pan away.
                // If there's an unfocus method, call it here. Otherwise, let it be.
            }
        }

        private void HandleDebtExpired()
        {
            if (_isResolving) return;
            _isResolving = true;
            
            Debug.Log("[DogTax] Debt Expired. Escalating to Threat.");
            Severity threatSeverity = _activeDebt.Severity == Severity.Escalated ? Severity.Critical : Severity.Normal;
            _activeDebt = null;
            
            if (_spawnedDebtCard != null && !_spawnedDebtCard.Destroyed)
            {
                _spawnedDebtCard.DestroyCard(true, true);
                _spawnedDebtCard = null;
            }
            
            SpawnThreat(threatSeverity);
            
            _isResolving = false;
        }

        public void SpawnThreat(Severity severity)
        {
            CurrentState = DogTaxCycleState.Threat;
            int currentMonth = WorldManager.instance != null ? WorldManager.instance.CurrentMonth : 0;
            
            // 1. Generate Encounter via EncounterManager
            int encounterId = 999;
            if (EncounterManager.Instance != null)
            {
                var template = Resources.Load<EncounterTemplateSO>("Encounters/DogTaxEncounter");
                EncounterData newEncounter;
                if (template != null)
                {
                    newEncounter = EncounterGenerator.Generate(template, Random.Range(0, 99999), 1);
                    newEncounter.TurnLimit = 30;
                }
                else
                {
                    // Fallback if template is not in Resources yet
                    newEncounter = new EncounterData
                    {
                        EncounterName = "Dog Tax Patrol",
                        Context = EncounterContext.DogTax,
                        TurnLimit = 30
                    };
                }
                
                encounterId = EncounterManager.Instance.RegisterEncounter(newEncounter);
            }

            _activeThreat = new ThreatInstance(null, ThreatSourceType.Event)
            {
                CurrentSeverity = severity,
                ThreatExpiryMonth = currentMonth + 5, // 5 months until forced consequence
                EncounterId = encounterId
            };
            
            Debug.Log($"[DogTax] Threat Generated. Severity: {severity}. Expiry: {_activeThreat.ThreatExpiryMonth}. EncounterID: {encounterId}");
            
            // 2. Spawn ThreatCardComponent on board
            if (WorldManager.instance != null && WorldManager.instance.CurrentBoard != null)
            {
                Vector3 spawnPos = WorldManager.instance.CurrentBoard.MiddleOfBoard() + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
                GameCard spawnedCard = WorldManager.instance.CreateCard(spawnPos, "dogtax_threat", true, true, true);
                if (spawnedCard != null)
                {
                    _spawnedThreatCard = spawnedCard;
                    var threatComp = spawnedCard.GetComponent<GameScripts.Systems.Threat.UI.ThreatCardComponent>();
                    if (threatComp == null)
                    {
                        threatComp = spawnedCard.gameObject.AddComponent<GameScripts.Systems.Threat.UI.ThreatCardComponent>();
                    }
                    threatComp.Initialize(_activeThreat);
                    StartCoroutine(FocusCameraOnCardRoutine(spawnedCard, 3f));
                }
            }
        }

        private void HandleThreatExpired()
        {
            if (_isResolving) return;
            _isResolving = true;
            
            Debug.Log("[DogTax] Threat Expired. Applying forced consequence.");
            var consequence = new ConsequenceData
            {
                Type = ConsequenceType.LoseResource,
                Magnitude = 5,
                Severity = _activeThreat.CurrentSeverity
            };
            
            ConsequenceResolver.ApplyConsequence(consequence);
            
            // If it's a critical threat expired, maybe Dogma Seizure (worse consequence)
            if (_activeThreat.CurrentSeverity == Severity.Critical)
            {
                ConsequenceResolver.ApplyConsequence(new ConsequenceData { Type = ConsequenceType.DestroyBuilding, Magnitude = 1, Severity = Severity.Critical });
            }

            _activeThreat = null;
            if (_spawnedThreatCard != null && !_spawnedThreatCard.Destroyed)
            {
                _spawnedThreatCard.DestroyCard(true, true);
                _spawnedThreatCard = null;
            }
            ResolveCycle();
        }

        // Called by Combat Engine when Encounter finishes
        public void OnCombatEnded(int encounterId, bool isVictory)
        {
            if (CurrentState != DogTaxCycleState.Threat || _activeThreat == null) return;
            if (_activeThreat.EncounterId != encounterId) return;
            
            if (_isResolving) return;
            _isResolving = true;

            if (isVictory)
            {
                Debug.Log("[DogTax] Combat Victory.");
                
                Vector3 rewardPos = Vector3.zero;
                if (_spawnedThreatCard != null && !_spawnedThreatCard.Destroyed)
                {
                    rewardPos = _spawnedThreatCard.transform.position;
                }
                
                EncounterRewardResolver.ResolveRewards(EncounterContext.DogTax, rewardPos);
                
                _activeThreat = null;
                ResolveCycle();
            }
            else
            {
                Debug.Log("[DogTax] Combat Defeat. Escalating...");
                Severity currentSev = _activeThreat.CurrentSeverity;
                _activeThreat = null;
                
                if (currentSev == Severity.Critical)
                {
                    // Seizure
                    ConsequenceResolver.ApplyConsequence(new ConsequenceData { Type = ConsequenceType.LoseCat, Magnitude = 1, Severity = Severity.Critical });
                    
                    if (_spawnedThreatCard != null && !_spawnedThreatCard.Destroyed)
                    {
                        _spawnedThreatCard.DestroyCard(true, true);
                        _spawnedThreatCard = null;
                    }
                    ResolveCycle();
                }
                else
                {
                    if (_spawnedThreatCard != null && !_spawnedThreatCard.Destroyed)
                    {
                        _spawnedThreatCard.DestroyCard(true, true);
                        _spawnedThreatCard = null;
                    }
                    SpawnDebt(Severity.Escalated);
                    _isResolving = false; // unlock since we spawned debt
                }
            }
        }

        public void OnDebtPaid()
        {
            if (CurrentState != DogTaxCycleState.Debt) return;
            if (_isResolving) return;
            
            _isResolving = true;
            Debug.Log("[DogTax] Debt Paid.");
            _activeDebt = null;
            // The DebtCardComponent destroys itself on paid, but we should clear our reference
            _spawnedDebtCard = null; 
            ResolveCycle();
        }

        private void ResolveCycle()
        {
            CurrentState = DogTaxCycleState.Resolved;
            Debug.Log("[DogTax] Cycle Resolved.");
            ScheduleNextTax();
            _isResolving = false;
        }

        private void ScheduleNextTax()
        {
            if (CurrentState != DogTaxCycleState.Resolved) return;
            
            int currentMonth = WorldManager.instance != null ? WorldManager.instance.CurrentMonth : 0;
            NextTaxMonth = currentMonth + 10;
            CurrentState = DogTaxCycleState.Inactive;
            Debug.Log($"[DogTax] Next Tax Scheduled for month {NextTaxMonth}");
        }
    }
}
