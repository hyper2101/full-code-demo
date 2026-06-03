using UnityEngine;
using Mewtations.Combat;
using GameScripts.Combat.Core;

namespace GameScripts.Systems.Enemies
{
    [CreateAssetMenu(fileName = "NewDogEnemy", menuName = "Enemies/Dog Enemy Definition")]
    public class DogEnemyDefinition : ScriptableObject
    {
        public string Id;

        public string NameKey;
        public string DescriptionKey;

        public Sprite Portrait;

        public DogArchetype Archetype;

        [Header("Combat Profile")]
        public CombatAttackPattern AttackPattern;
        public CatElement Element;
        public CombatSkillDefinition ActiveCombatSkill;

        [Header("Base Stats")]
        public int BaseHP;
        public int BaseATK;
        public int BaseDEF;
        public int BaseSPD;

        [Header("Scaling")]
        public ScalingProfile ScalingProfile;
    }
}
