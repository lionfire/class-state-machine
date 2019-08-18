using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LionFire.StateMachines.Class
{

    public class StateMachineState<TState, TTransition, TOwner> : IStateMachine<TState, TTransition>
    {
        #region Relationships

        public TOwner Owner { get; private set; }

        #endregion

        #region (Static)

        internal readonly static StateChange<TState, TTransition, TOwner> startingLastStateChange;

        static StateMachineState()
        {
            var startingTransition = StateMachine<TState, TTransition>.StartingTransition;
            var startingTransitionBinding = BindingProvider<TState, TTransition, TOwner>.Default.GetTransitionBinding(startingTransition);
            startingLastStateChange = new StateChange<TState, TTransition, TOwner>(StateMachine<TState, TTransition>.StartingTransition);
        }

        #endregion

        #region Construction

        public StateMachineState(TOwner owner = default(TOwner))
        {
            //if (owner == null) throw new ArgumentNullException(nameof(owner));
            this.Owner = owner;
            currentState = StateMachine<TState, TTransition>.StartingState;
            lastStateChange = startingLastStateChange;
        }

        #endregion

        #region State

        public IStateChange<TState, TTransition> LastStateChange => lastStateChange;
        private StateChange<TState, TTransition, TOwner> lastStateChange;

        #region CurrentState

        public TState CurrentState
        {
            get { return lastStateChange.TransitionBinding.To.Id; }
        }
        private TState currentState;

        #endregion

        #endregion

        #region Events

        public event StateChangeEventHandler<TState, TTransition> StateChanging;
        public event StateChangeEventHandler<TState, TTransition> StateChanged;
        public event StateChangeEventHandler<TState, TTransition> StateChangeAborted;

        #endregion

        public IEnumerable<object> CannotChangeStateReasons(TTransition transition, object context = null)
        {
            lock (lockObject)
            {
                // SIMILAR: ChangeState/TryChangeState 
                var stateChange = new StateChange<TState, TTransition, TOwner>(transition, context);
                // IsPreviewOnly = true, // FUTURE?  Could it do any good to indicate this?

                var reasons = CannotChangeStateReasons(stateChange);
                return reasons;
            }
        }

        public bool CanChangeState(TTransition transition, object context = null)
        {
            return !CannotChangeStateReasons(transition, context).Any();
        }

        public bool TryTransition(TTransition transition, object context = null)
        {
            // SIMILAR: See also - ChangeState for similar code
            lock (lockObject)
            {
                var stateChange = new StateChange<TState, TTransition, TOwner>(transition, context);

                var reasons = CannotChangeStateReasons(stateChange);
                if (reasons.Any()) return false;

                DoTransition(stateChange);
                return stateChange.CancellationTokenSource?.IsCancellationRequested != true;
            }
        }

        public void Transition(TTransition transition, object context = null)
        {
            // SIMILAR: See also - TryChangeState for similar code
            lock (lockObject)
            {
                var stateChange = new StateChange<TState, TTransition, TOwner>(transition, context);

                var reasons = CannotChangeStateReasons(stateChange);
                if (reasons.Any()) throw new CannotChangeStateException(reasons);

                DoTransition(stateChange);

                if (stateChange.CancellationTokenSource != null && stateChange.CancellationTokenSource.IsCancellationRequested)
                {
                    throw new StateChangeCanceledException();
                }
            }
        }

        #region (Private)

        private object lockObject = new object();
        internal IEnumerable<object> CannotChangeStateReasons(StateChange<TState, TTransition, TOwner> stateChange, bool quitOnFirstReason = false)
        {
            lock (lockObject)
            {
                if (!CurrentState.Equals(stateChange.TransitionBinding.From.Id))
                {
                    return new object[] { new InvalidFromStateException($"{stateChange.TransitionBinding.Info.Id} requires starting state of {stateChange.TransitionBinding.From.ToString()} but CurrentState is {CurrentState}") };
                }

                var parameters = Array.Empty<object>();

                List<object> results = null;

                var canLeave = stateChange.TransitionBinding.From?.CanLeave;
                if (canLeave != null && false == canLeave(Owner, stateChange))
                {
                    results = new List<object>();
                    results.Add(new StateMachineException($"CanLeave for {stateChange.TransitionBinding.From.Id} state returned false"));
                    if (quitOnFirstReason) return results;
                }

                var canTransition = stateChange.TransitionBinding.CanTransition;
                if (canTransition != null && false == canTransition(Owner, stateChange))
                {
                    if (results == null) results = new List<object>();
                    results.Add(new StateMachineException($"CanTransition for {stateChange.Transition} transition returned false"));
                    if (quitOnFirstReason) return results;
                }

                var canEnter = stateChange.TransitionBinding.To?.CanEnter;
                if (canEnter != null && false == canEnter(Owner, stateChange))
                {
                    if (results == null) results = new List<object>();
                    results.Add(new StateMachineException($"CanEnter for {stateChange.TransitionBinding.From.Id} state returned false"));
                    if (quitOnFirstReason) return results;
                }

                return results ?? Enumerable.Empty<object>();
            }
        }




        private void DoTransition(StateChange<TState, TTransition, TOwner> stateChange, object context = null)
        {
            TransitionBinding<TState, TTransition, TOwner> transitionBinding = stateChange.TransitionBinding;

            var parameters = Array.Empty<object>();

            StateChanging?.Invoke(stateChange);

            if (!stateChange.IsCanceled && transitionBinding.From?.OnLeaving != null) transitionBinding.From?.OnLeaving(Owner, stateChange);
            if (!stateChange.IsCanceled && transitionBinding.OnTransitioning != null) transitionBinding.OnTransitioning(Owner, stateChange);
            if (!stateChange.IsCanceled && transitionBinding.To?.OnEntering != null) transitionBinding.To?.OnEntering(Owner, stateChange);

            if (stateChange.IsCanceled)
            {
                try
                {
                    StateChangeAborted?.Invoke(stateChange);
                }
                catch (Exception)
                {
                    throw;
                    // FUTURE: exception handling, and be sure to return.
                    //return;
                }
            }
            lastStateChange = new StateChange<TState, TTransition, TOwner>(transitionBinding.Info.Id, context);

            StateChanged?.Invoke(stateChange);
        }

        #endregion
    }

#if FUTUREOPTIMIZATION
    public class IntStateMachineState<TState, TTransition> : IStateMachineState<TState, TTransition>
    {

    #region CurrentState

        public TState CurrentState
        {
            get { return (TState)(object)currentState; }
            set { currentState = (int)(object)value; }
        }
        private int currentState;

    #endregion

        internal bool TransitionFromTo(TState from, TState to)
        {
            var fromI = (int)(object)from;
            var toI = (int)(object)to;
            return Interlocked.CompareExchange(ref currentState, toI, fromI) == toI;
        }

        public StateChange<TState, TTransition> LastTransition { get; set; }

    }


#endif
}
