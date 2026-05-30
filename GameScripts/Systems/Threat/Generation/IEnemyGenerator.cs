using System.Collections.Generic;

namespace GameScripts.Systems.Threat.Generation
{
    public class EnemySpawnInfo
    {
        public string EnemyID;
        public int Level;
        public List<string> AssignedPowers = new List<string>();
    }

    public class EnemyTeamData
    {
        public int TargetLevel;
        public List<EnemySpawnInfo> Enemies = new List<EnemySpawnInfo>();

        public EnemyTeamData()
        {
            Enemies = new List<EnemySpawnInfo>();
        }
    }

    public interface IEnemyGenerator
    {
        EnemyTeamData GenerateTeam(EnemyPool pool, int targetLevel);
    }
}
