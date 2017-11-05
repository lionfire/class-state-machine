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
        public StateChange(TTransition transition, object context = null)
        {
            this.Transition = transition;
            this.TransitionBinding = BindingProvider<TState, TTransition, TOwner>.Default.GetTransitionBinding(transition);
            if (TransitionBinding == null) throw new ArgumentException($"Could not get TransitionBinding for transition '{transition}'");
            this.Context = context;
        }

        public bool IsCanceled => CancellationTokenSource != null && CancellationTokenSource.IsCancellationRequested;

        #region Read-only Properties

        internal TransitionBinding<TState, TTransition, TOwner> TransitionBinding { get; set; }

        public TTransition Transition { get; protected set; }
        
        #endregion

        public object Context { get; set; }

        public List<object> FailureReasons { get; set; }

        /// <summary>
        /// If the state change is cancelable, use this to cancel.  If not cancelable, this will return null.
        /// </summary>
        public CancellationTokenSource CancellationTokenSource { get; set; }

        public IStateMachine<TState, TTransition> StateMachine { get; set; }

        #region Derived

        public TState From => TransitionBinding.From == null ? default(TState) : TransitionBinding.From.Id;
        public TState To => TransitionBinding.To == null ? default(TState) : TransitionBinding.To.Id;

        #endregion
    }
}
