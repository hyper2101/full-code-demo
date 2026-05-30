using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameScripts.Systems.Threat.Generation
{
    public class StandardEnemyGenerator : IEnemyGenerator
    {
        public EnemyTeamData GenerateTeam(EnemyPool pool, int targetLevel)
        {
            var data = new EnemyTeamData();

            if (pool == null || pool.ValidEnemyIDs.Count == 0)
            {
                Debug.LogWarning("EnemyPool rỗng. Không thể sinh quái.");
                data.TargetLevel = targetLevel;
                return data;
            }

            // Lấy max level thực tế của người chơi để chặn vượt bậc (Tier cap)
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
                    maxCatLevel = System.Linq.Enumerable.Max(catLevels);
                }
            }

            // Target level của quái không được phép vượt qua bậc sức mạnh cao nhất mà người chơi đã mở
            // Giả sử mỗi bậc là 10 level.
            int playerMaxTier = (maxCatLevel / 10);
            int cappedTargetLevel = Mathf.Min(targetLevel, (playerMaxTier * 10) + 9);
            
            // Nếu người chơi thậm chí còn chưa tới level đó, giới hạn thẳng bằng maxCatLevel + 2 (an toàn)
            cappedTargetLevel = Mathf.Min(cappedTargetLevel, maxCatLevel + 2);

            data.TargetLevel = cappedTargetLevel;

            // Tính số lượng quái dựa trên targetLevel
            int enemyCount = Mathf.Clamp(cappedTargetLevel / 5, 1, 5); // Tối đa 5 con
            
            for (int i = 0; i < enemyCount; i++)
            {
                // Chọn ngẫu nhiên 1 loại quái từ Pool
                string selectedEnemy = pool.ValidEnemyIDs[UnityEngine.Random.Range(0, pool.ValidEnemyIDs.Count)];
                
                var spawnInfo = new EnemySpawnInfo 
                { 
                    EnemyID = selectedEnemy,
                    Level = cappedTargetLevel
                };

                // Random sức mạnh dựa theo bậc (Tier) của quái
                int enemyTier = (cappedTargetLevel / 10);
                if (enemyTier >= 1) spawnInfo.AssignedPowers.Add("Power_Tier1_Random");
                if (enemyTier >= 2) spawnInfo.AssignedPowers.Add("Power_Tier2_Random");
                if (enemyTier >= 3) spawnInfo.AssignedPowers.Add("Power_Tier3_Random");

                data.Enemies.Add(spawnInfo);
            }

            return data;
        }
    }
}
