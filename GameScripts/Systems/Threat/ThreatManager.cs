using System;
using System.Collections.Generic;
using UnityEngine;
using GameScripts.Systems.Threat.Generation;

namespace GameScripts.Systems.Threat
{
    public class ThreatManager : MonoBehaviour
    {
        public static ThreatManager Instance { get; private set; }

        public int CatGodAnger = 0;
        public ThreatData CatGodWrathTemplate;

        public List<ThreatData> AllThreatDatas = new List<ThreatData>();
        public List<ThreatInstance> ActiveThreats = new List<ThreatInstance>();
        private Generation.IEnemyGenerator _enemyGenerator = new Generation.StandardEnemyGenerator();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            // TODO: Initialize concrete EnemyGenerator in Phase 5
        }

        public ThreatInstance CreateThreat(ThreatData data, ThreatSourceType source, int targetLevel, int warningDays)
        {
            var instance = new ThreatInstance(data, source);
            instance.TargetLevel = targetLevel;
            
            if (_enemyGenerator != null)
            {
                instance.GeneratedEnemyTeam = _enemyGenerator.GenerateTeam(data.EnemyPool, targetLevel);
                instance.TargetLevel = instance.GeneratedEnemyTeam.TargetLevel; // Đồng bộ với cap level của Generator
            }
            else
            {
                instance.GeneratedEnemyTeam = new Generation.EnemyTeamData { TargetLevel = targetLevel };
            }

            // Snapshot rewards and tribute
            if (data.VictoryRewardPack != null)
            {
                instance.VictoryRewardPackId = data.VictoryRewardPack.PackId;
            }
            instance.RequiredTributeItems = new List<string>(data.TributeRequiredItems);
            
            instance.State = ThreatState.Warning;
            instance.DaysRemaining = warningDays;

            ActiveThreats.Add(instance);
            
            // TODO: Update WarningUI in Phase 2
            
            return instance;
        }

        public bool HasActivePenalty(ThreatPenaltyType type)
        {
            foreach (var threat in ActiveThreats)
            {
                if (threat.State == ThreatState.Active && threat.BaseData.PenaltyType == type)
                {
                    return true;
                }
            }
            return false;
        }

        public void OnDayPassed(int currentDay)
        {
            // Reverse loop to allow safe removal or state change
            for (int i = ActiveThreats.Count - 1; i >= 0; i--)
            {
                var threat = ActiveThreats[i];

                if (threat.State == ThreatState.Warning || threat.State == ThreatState.Cooldown || threat.State == ThreatState.Active)
                {
                    threat.DaysRemaining--;

                    if (threat.DaysRemaining <= 0)
                    {
                        if (threat.State == ThreatState.Warning)
                        {
                            ActivateThreat(threat);
                        }
                        else if (threat.State == ThreatState.Cooldown)
                        {
                            RescheduleThreat(threat);
                        }
                        else if (threat.State == ThreatState.Active)
                        {
                            EscalateThreat(threat);
                        }
                    }
                }
            }
        }

        private void EscalateThreat(ThreatInstance threat)
        {
            if (threat.BaseData.NextEscalation != null)
            {
                Debug.Log($"Threat Escalated: {threat.BaseData.DisplayName} -> {threat.BaseData.NextEscalation.DisplayName}");
                ActiveThreats.Remove(threat);
                
                DestroyThreatCard(threat);
                
                // Tạo Threat mới bằng template NextEscalation, giữ nguyên level hiện tại
                CreateThreat(threat.BaseData.NextEscalation, threat.Source, threat.TargetLevel, 2);
            }
            else
            {
                // Nếu không có cấp độ tiếp theo, reset lại thời gian Active
                threat.DaysRemaining = 10;
                Debug.Log($"Threat persists: {threat.BaseData.DisplayName}");
            }
        }

        private void DestroyThreatCard(ThreatInstance threat)
        {
            if (string.IsNullOrEmpty(threat.SpawnedCardUniqueId)) return;
            if (WorldManager.instance == null || WorldManager.instance.UniqueIdToCard == null) return;
            
            if (WorldManager.instance.UniqueIdToCard.TryGetValue(threat.SpawnedCardUniqueId, out GameCard card))
            {
                if (card != null && !card.Destroyed)
                {
                    card.DestroyCard();
                }
            }
        }

        private void ActivateThreat(ThreatInstance threat)
        {
            threat.State = ThreatState.Active;
            threat.DaysRemaining = 10; // Thời gian cho phép giải quyết trước khi leo thang
            
            // Spawn Card on the board
            Vector3 spawnPos = Vector3.zero;
            if (WorldManager.instance != null && WorldManager.instance.CurrentBoard != null)
            {
                spawnPos = WorldManager.instance.CurrentBoard.MiddleOfBoard();
                CardData cardObj = WorldManager.instance.CreateCard(spawnPos, threat.BaseData.CardPrefabId, true, false, true);
                
                if (cardObj != null)
                {
                    threat.SpawnedCardUniqueId = cardObj.UniqueId;
                    // Tích hợp Component
                    var component = cardObj.gameObject.AddComponent<GameScripts.Systems.Threat.UI.ThreatCardComponent>();
                    component.Initialize(threat);
                }
            }
            // TODO: Logic cho spawn
            Debug.Log($"Threat Activated: {threat.BaseData.DisplayName}");
        }

        public void ResolveThreat(ThreatInstance threat, bool isVictory)
        {
            if (isVictory)
            {
                threat.State = ThreatState.Resolved;
                ActiveThreats.Remove(threat);
                
                // Trả thưởng
                if (WorldManager.instance != null && WorldManager.instance.CurrentBoard != null && !string.IsNullOrEmpty(threat.VictoryRewardPackId))
                {
                    Vector3 spawnPos = WorldManager.instance.CurrentBoard.MiddleOfBoard();
                    WorldManager.instance.CreateCard(spawnPos, threat.VictoryRewardPackId, true, false, true);
                }
                Debug.Log($"Threat Resolved: {threat.BaseData.DisplayName}");
            }
            else
            {
                // Thua trận: Giữ nguyên trạng thái Active, không ném vào Cooldown
                Debug.Log($"Threat Failed. Threat {threat.BaseData.DisplayName} remains Active on the board!");
            }
        }

        private void RescheduleThreat(ThreatInstance threat)
        {
            threat.State = ThreatState.Scheduled;
            // TODO: Logic for cycle reschedule
            Debug.Log($"Threat Rescheduled: {threat.BaseData.DisplayName}");
        }

        public List<ThreatInstance> GetSaveData()
        {
            return ActiveThreats;
        }

        private bool _needsRelinking = false;
        private int _relinkAttempts = 0;

        public void LoadSaveData(List<ThreatInstance> savedThreats)
        {
            if (savedThreats != null)
            {
                ActiveThreats = savedThreats;
                // Khôi phục tham chiếu ScriptableObject từ ID
                foreach (var threat in ActiveThreats)
                {
                    threat.BaseData = AllThreatDatas.Find(d => d.ThreatID == threat.BaseDataId);
                }
                _needsRelinking = true;
                _relinkAttempts = 0;
                // TODO: Refresh UI after loading
            }
            else
            {
                ActiveThreats = new List<ThreatInstance>();
            }
        }

        private void Update()
        {
            if (_needsRelinking && WorldManager.instance != null && WorldManager.instance.UniqueIdToCard != null && WorldManager.instance.UniqueIdToCard.Count > 0)
            {
                bool allLinked = true;
                foreach (var threat in ActiveThreats)
                {
                    if (threat.State == ThreatState.Active && !string.IsNullOrEmpty(threat.SpawnedCardUniqueId))
                    {
                        if (WorldManager.instance.UniqueIdToCard.TryGetValue(threat.SpawnedCardUniqueId, out GameCard card))
                        {
                            var comp = card.GetComponent<GameScripts.Systems.Threat.UI.ThreatCardComponent>();
                            if (comp == null)
                            {
                                comp = card.gameObject.AddComponent<GameScripts.Systems.Threat.UI.ThreatCardComponent>();
                                comp.Initialize(threat);
                            }
                        }
                        else
                        {
                            allLinked = false;
                        }
                    }
                }
                
                _relinkAttempts++;
                if (allLinked || _relinkAttempts > 60)
                {
                    _needsRelinking = false;
                }
            }
        }

        // --- Phase 6: Cat God Integration ---
        public void IncreaseCatGodAnger(int amount)
        {
            CatGodAnger += amount;
            Debug.Log($"Cat God Anger increased! Current: {CatGodAnger}");

            if (CatGodAnger >= 10 && CatGodWrathTemplate != null)
            {
                CatGodAnger = 0; // Reset after triggering
                
                // Tính toán level trung bình và level cao nhất của Top 5 mèo
                int averageLevel = 1;
                int maxCatLevel = 1;

                if (WorldManager.instance != null)
                {
                    var catLevels = System.Linq.Enumerable.ToList(
                        System.Linq.Enumerable.Select(
                            System.Linq.Enumerable.Where(
                                WorldManager.instance.AllCards, 
                                c => c != null && c.CardData is CatCardData && !c.Destroyed
                            ),
                            c => (c.CardData as CatCardData)?.Level ?? 1
                        )
                    );

                    if (catLevels.Count > 0)
                    {
                        var topCats = System.Linq.Enumerable.ToList(
                            System.Linq.Enumerable.Take(
                                System.Linq.Enumerable.OrderByDescending(catLevels, l => l), 
                                5
                            )
                        );
                        averageLevel = (int)System.Linq.Enumerable.Average(topCats);
                        maxCatLevel = catLevels.Max();
                    }
                }
                
                // Thay vì nhân 1.5 (gây lạm phát ở late game), ta cộng một hằng số khó khăn (ví dụ +5 level)
                int wrathLevel = averageLevel + 5;
                
                // Vẫn áp dụng Cap để không bao giờ quái vượt qua Tier cao nhất mà người chơi đang sở hữu
                wrathLevel = Mathf.Min(wrathLevel, (maxCatLevel / 10) * 10 + 9);
                
                // Thêm chặn an toàn thứ 2: Không được vượt qua level cao nhất của Mèo + 2
                wrathLevel = Mathf.Min(wrathLevel, maxCatLevel + 2);
                
                // Spawn the wrath threat.
                CreateThreat(CatGodWrathTemplate, ThreatSourceType.WorldEvent, wrathLevel, 5);
            }
        }
    }
}
