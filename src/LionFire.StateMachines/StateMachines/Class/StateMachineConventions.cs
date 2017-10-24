using System.Collections.Generic;

namespace LionFire.StateMachines.Class
{
    public class StateMachineConventions
    {
        public List<string> CanLeaveStatePrefixes { get; set; }
        public List<string> CanEnterStatePrefixes { get; set; }
        public List<string> LeavingStatePrefixes { get; set; }
        public List<string> EnteringStatePrefixes { get; set; }
        public List<string> CanTransitionPrefixes { get; set; }
        public List<string> OnTransitionPrefixes { get; set; }

        #region Static

        public static StateMachineConventions DefaultConventions { get; set; } = Flexible;

        public static StateMachineConventions Flexible
        {
            get
            {
                return new StateMachineConventions
                {
                    CanLeaveStatePrefixes = new List<string> { "CanLeave" },
                    CanEnterStatePrefixes = new List<string> { "CanEnter", "Can" },
                    LeavingStatePrefixes = new List<string> { "OnLeaving", "Leaving", "After" },
                    EnteringStatePrefixes = new List<string> { "OnEntering", "Entering", "Before", "On" },
                    CanTransitionPrefixes = new List<string> { "Can" },
                    OnTransitionPrefixes = new List<string> { "On", "During" },
                };
            }
        }
        public static StateMachineConventions Short
        {
            get
            {
                return new StateMachineConventions
                {
                    CanLeaveStatePrefixes = new List<string> { "CanLeave" },
                    CanEnterStatePrefixes = new List<string> { "Can" },
                    LeavingStatePrefixes = new List<string> { "After" },
                    EnteringStatePrefixes = new List<string> { "On" },
                    CanTransitionPrefixes = new List<string> { "Can" },
                    OnTransitionPrefixes = new List<string> { "On" },
                };
            }
        }
        public static StateMachineConventions Pedantic
        {
            get
            {
                return new StateMachineConventions
                {
                    CanLeaveStatePrefixes = new List<string> { "CanLeave" },
                    CanEnterStatePrefixes = new List<string> { "CanEnter" },
                    LeavingStatePrefixes = new List<string> { "OnLeaving"  },
                    EnteringStatePrefixes = new List<string> { "OnEntering" },
                    CanTransitionPrefixes = new List<string> { "Can" },
                    OnTransitionPrefixes = new List<string> { "During" },
                };
            }
        }

        #endregion

    }
}
