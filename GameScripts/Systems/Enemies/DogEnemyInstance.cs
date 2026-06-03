namespace GameScripts.Systems.Enemies
{
    public class DogEnemyInstance
    {
        public DogEnemyDefinition Definition;
        public int Level;

        // Caching runtime stats
        public int HP { get; private set; }
        public int MaxHP { get; private set; }
        public int ATK { get; private set; }
        public int DEF { get; private set; }
        public int SPD { get; private set; }

        public DogEnemyInstance(DogEnemyDefinition definition, int level)
        {
            Definition = definition;
            Level = level;
            CalculateRuntimeStats();
        }

        private void CalculateRuntimeStats()
        {
            // Level 1 uses base stats. Subsequent levels add ScalingProfile per level.
            int levelBonus = Level - 1;
            if (levelBonus < 0) levelBonus = 0;

            MaxHP = Definition.BaseHP + (Definition.ScalingProfile.HPPerLevel * levelBonus);
            HP = MaxHP;
            ATK = Definition.BaseATK + (Definition.ScalingProfile.ATKPerLevel * levelBonus);
            DEF = Definition.BaseDEF + (Definition.ScalingProfile.DEFPerLevel * levelBonus);
            SPD = Definition.BaseSPD + (Definition.ScalingProfile.SPDPerLevel * levelBonus);
        }
    }
}
