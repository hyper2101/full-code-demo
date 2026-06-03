using System;
using UnityEngine;

namespace GameScripts.Combat.Core
{
    public enum MultiHitPolicy
    {
        StopOnDeath,
        RetargetOnDeath
    }

    [CreateAssetMenu(fileName = "NewCombatSkill", menuName = "Combat System/Combat Skill Definition")]
    public class CombatSkillDefinition : ScriptableObject
    {
        public string Id;
        public string SkillNameKey;
        public string SkillDescriptionKey;

        [Header("Damage")]
        public float FinalDamageMultiplier = 1.0f;

        [Header("Execution Barrage Settings")]
        public int HitCount = 1;
        public MultiHitPolicy MultiHitPolicy = MultiHitPolicy.StopOnDeath;
        public float RawAtkMultiplier = 0f;
        public float RawMissingHpMultiplier = 0f;

        [Header("Crushing Formation Settings")]
        public int RageReduction = 0;
    }
}
