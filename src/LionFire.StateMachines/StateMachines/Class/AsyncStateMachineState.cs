#if false // TODO: Re-copy from StateMachineState.cs
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LionFire.StateMachines.Class
{

    [Obsolete("Not implemented yet")]
    public class AsyncStateMachineState<TState, TTransition, TOwner> : IAsyncStateMachine<TState, TTransition>
        
    {

        #region Relationships

        public TOwner Owner { get; private set; }

        #endregion

        #region (Static)

        internal  static StateChange<TState, TTransition, TOwner> startingLastStateChange => StateMachineState<TState, TTransition, TOwner>.startingLastStateChange;

        #endregion

        #region Construction

        public AsyncStateMachineState(TOwner owner = default(TOwner))
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

        //        public Task ChangeStateAsync(StateTransitionTypeBinding<TState, TTransition, TOwner> transitionBinding, object context = null)
        //        {
        //            ChangeState_CheckAlready(transitionBinding, context);

        //            throw new NotImplementedException();
        //            //return Task.CompletedTask;
        //        }

        //        private bool? ChangeState_CheckAlready(StateTransitionTypeBinding<TState, TTransition, TOwner> transitionBinding, object context = null)
        //        {
        //#error todo
        //            if (currentState.Equals(transitionBinding.Info.From))
        //            {
        //                return false;
        //            }
        //            else
        //            {
        //                if (currentState.Equals(transitionBinding.Info.To))
        //                {
        //                    if (LastTransition.Transition.Info.Id.Equals(transitionBinding.Info.Id)
        //                        && LastTransition.context == context)
        //                    {
        //                        // Okay, if transition is Last transition, and context == Last transition context, and this transition is idempotent (assumed true by default) 
        //                        return true;
        //                    }
        //                    return true;
        //                }
        //            }

        //        }
        //        private void ChangeState_ValidateNotAlready(StateTransitionTypeBinding<TState, TTransition, TOwner> transitionBinding, object context = null)
        //        {
        //#error todo
        //            throw new StateMachineException($"Transition {transitionBinding.Info.Id} ({transitionBinding.Info.From} -> {transitionBinding.Info.To}) not valid from state {currentState}");
        //        }

        public IEnumerable<object> CannotChangeStateReasons(TTransition transition, object context = null)
        {
            lock (lockObject)
            {
                // SIMILAR: ChangeState/TryChangeState 
                var stateChange = new StateChange<TState, TTransition, TOwner>(transition, context);
                //{
                //    // IsPreviewOnly = true, // FUTURE?  Could it do any good to indicate this?
                //};

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
                return !stateChange.CancellationTokenSource.IsCancellationRequested;
            }
        }

        public Task BeginTransition(TTransition transition, object context = null)
        {
            throw new NotImplementedException();
        }
        public void Transition(TTransition transition, object context = null)
        {
            BeginTransition(transition, context).ConfigureAwait(false).GetAwaiter().GetResult(); // REVIEW the wait
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

        internal void Transition(TransitionBinding<TState, TTransition, TOwner> transitionBinding, object context = null)
        {
            // SIMILAR: See also - TryChangeState for similar code
            lock (lockObject)
            {
                var stateChange = new StateChange<TState, TTransition, TOwner>
                {
                    TransitionBinding = transitionBinding,
                    Context = context,
                };

                var reasons = CannotChangeStateReasons(stateChange);
                if (reasons.Any()) throw new CannotChangeStateException(reasons);

                DoTransition(stateChange);
                if (stateChange.CancellationTokenSource.IsCancellationRequested)
                {
                    throw new StateChangeAbortedException();
                }
            }
        }

        private void DoTransition(StateChange<TState, TTransition, TOwner> stateChange)
        {
            TransitionBinding<TState, TTransition, TOwner> transitionBinding = stateChange.TransitionBinding;

            var parameters = Array.Empty<object>();

            StateChanging?.Invoke(stateChange);

            if (stateChange.CancellationTokenSource != null)
            {
                if (!stateChange.CancellationTokenSource.IsCancellationRequested) transitionBinding.From?.OnLeaving(Owner, stateChange);
                if (!stateChange.CancellationTokenSource.IsCancellationRequested) transitionBinding.OnTransitioning(Owner, stateChange);
                if (!stateChange.CancellationTokenSource.IsCancellationRequested) transitionBinding.To?.OnEntering(Owner, stateChange);

                if (stateChange.CancellationTokenSource.IsCancellationRequested)
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
            }
            lastStateChange = new StateChange<TState, TTransition, TOwner>
            {
                Transition = transitionBinding.Info.Id
            };
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
#endif