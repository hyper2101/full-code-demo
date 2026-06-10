using System;

public enum AttachmentReason
{
    Drop,
    LoadRestore,
    AutoCollect,
    RecipeResult,
    SpawnInitialization,
    DebugForceAttach
}

public struct AttachContext
{
    public AttachmentReason Reason;
    public bool BypassValidation;

    public static AttachContext Drop() => new AttachContext { Reason = AttachmentReason.Drop, BypassValidation = false };
    public static AttachContext Restore() => new AttachContext { Reason = AttachmentReason.LoadRestore, BypassValidation = true };
    public static AttachContext Force(AttachmentReason reason) => new AttachContext { Reason = reason, BypassValidation = true };
}
