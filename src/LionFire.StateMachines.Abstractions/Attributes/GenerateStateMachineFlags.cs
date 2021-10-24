using System;

namespace LionFire.StateMachines.Class
{
    [Flags]
    public enum GenerateStateMachineFlags
    {
        None = 0,

        /// <summary>
        /// Generate missing methods such as Start, Run, Stop
        /// </summary>
        DisablePruneUnusedStates = 1 << 0,

        /// <summary>
        /// Include all transitions from model, even if there are no handler methods and they are not mentioned in the transitionsAllowed parameter
        /// </summary>
        DisablePruneUnusedTransitions = 1 << 1,

        DisableGeneration = 1 << 2,

        NoLock = 1 << 3,
    }
}
