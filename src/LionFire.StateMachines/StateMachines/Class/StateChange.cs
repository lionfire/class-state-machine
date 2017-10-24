using System;
using System.Collections.Generic;
using System.Threading;

namespace LionFire.StateMachines.Class
{

    //public enum StateChangeState
    //{
    //    Created=0,
    //    Preparing,
    //    //Completed,
    //    //Aborted,
    //    //RolledBack,
    //    //Undone,
    //}

    public class StateChange<TState, TTransition, TOwner> : IStateChange<TState, TTransition>
    {
        //internal StateChangeState StateChangeState { get; set; }

        internal TransitionBinding<TState, TTransition, TOwner> TransitionBinding { get; set; }

        public TTransition Transition { get; set; }
        public TState From => TransitionBinding.From.Id;
        public TState To => TransitionBinding.To.Id;

        public object Context { get; set; }

        public List<object> FailureReasons { get; set; }

        /// <summary>
        /// If the state change is cancelable, use this to cancel.  If not cancelable, this will return null.
        /// </summary>
        public CancellationTokenSource CancellationTokenSource { get; set; }

        public IStateMachine<TState, TTransition> StateMachine { get; set; }
    }
}
