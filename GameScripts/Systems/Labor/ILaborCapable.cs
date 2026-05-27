namespace Mewtations.Systems.Labor
{
    public enum LaborReadinessState
    {
        Ready,
        Tired,
        Exhausted,
        Recovering
    }

    /// <summary>
    /// Foundation for all labor capabilities. Decouples crafting/gathering from the Stacklands Worker inheritance.
    /// </summary>
    public interface ILaborCapable
    {
        /// <summary>
        /// Can this entity perform labor right now?
        /// </summary>
        bool CanPerformLabor();

        /// <summary>
        /// The efficiency multiplier (1.0 is default). Affected by tiredness and injuries.
        /// </summary>
        float GetLaborEfficiency();

        /// <summary>
        /// Hook to consume stamina after a labor action.
        /// </summary>
        void ConsumeLaborStamina(float amount);

        /// <summary>
        /// Current state of readiness, representing exhaustion/recovery.
        /// </summary>
        LaborReadinessState CurrentLaborState { get; }
    }
}
