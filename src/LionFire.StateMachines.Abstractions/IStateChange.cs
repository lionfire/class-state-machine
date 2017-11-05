using System.Collections.Generic;
using System.Threading;

namespace LionFire.StateMachines
{
    public interface IStateChange<TState,TTransition>
    {
        IStateMachine<TState,TTransition> StateMachine { get; }

         TState From { get;  }
         TState To { get;  }

        TTransition Transition { get; }

        /// <summary>
        /// Context information for the transition, or null if none exists.</param>
        /// </summary>
        object Context { get; set; }

        /// <summary>
        /// If the state change is cancelable, use this to cancel.  If not cancelable, this will return null.
        /// </summary>
        CancellationTokenSource CancellationTokenSource { get; }
        bool IsCanceled { get; }

        List<object> FailureReasons { get; set; }
    }

    public static class IStateChangeExtensions
    {
        public static void AddFailureReason<TState, TTransition>(this IStateChange<TState,TTransition> sc, object failureReason)
        {
            if (sc.FailureReasons == null) sc.FailureReasons = new List<object>();
            sc.FailureReasons.Add(failureReason);

        }
    }
}
