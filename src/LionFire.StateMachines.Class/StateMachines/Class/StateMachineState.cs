using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LionFire.StateMachines.Class
{

    public class StateMachineState<TState, TTransition, TOwner> : IStateMachineState<TState, TTransition, TOwner>
    {

        #region Relationships

        public TOwner Owner { get; private set; }

        #endregion

        #region Construction

        public StateMachineState(TOwner owner)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            this.Owner = owner;
            currentState = StateMachine<TState, TTransition>.StartingState;
        }

        #endregion

        #region State

        public StateChange<TState, TTransition, TOwner> LastTransition { get; set; }


        #region CurrentState

        public TState CurrentState
        {
            get { return currentState; }
            private set
            {
                if (EqualityComparer<TState>.Default.Equals(value, currentState)) return;

                var oldState = currentState;
                currentState = value;
                StateChangedForFromTo?.Invoke(Owner, oldState, value);
                //transitionBinding.To?.OnEntered(Owner); // Subscribe to changed event for this.

            }
        }
        private TState currentState;

        public event Action<TOwner, TState, TState> StateChangedForFromTo;

        #endregion

        #endregion


        //public void ChangeState(TTransition transition, object transitionData = null)
        //{
        //    ChangeState(stateMachine.GetTransitionInfo(transition), transitionData);
        //}
        //        public Task ChangeStateAsync(StateTransitionTypeBinding<TState, TTransition, TOwner> transitionBinding, object transitionData = null)
        //        {
        //            ChangeState_CheckAlready(transitionBinding, transitionData);

        //            throw new NotImplementedException();
        //            //return Task.CompletedTask;
        //        }

        //        private bool? ChangeState_CheckAlready(StateTransitionTypeBinding<TState, TTransition, TOwner> transitionBinding, object transitionData = null)
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
        //                        && LastTransition.TransitionData == transitionData)
        //                    {
        //                        // Okay, if transition is Last transition, and transitionData == Last transition transitionData, and this transition is idempotent (assumed true by default) 
        //                        return true;
        //                    }
        //                    return true;
        //                }
        //            }

        //        }
        //        private void ChangeState_ValidateNotAlready(StateTransitionTypeBinding<TState, TTransition, TOwner> transitionBinding, object transitionData = null)
        //        {
        //#error todo
        //            throw new StateMachineException($"Transition {transitionBinding.Info.Id} ({transitionBinding.Info.From} -> {transitionBinding.Info.To}) not valid from state {currentState}");
        //        }

        #region CannotChangeStateRegions

        public IEnumerable<object> CannotChangeStateReasons(TTransition transition, object transitionData = null)
        {
            return CannotChangeStateReasons(StateInfoProvider<TState, TTransition, TOwner>.Default.GetTransitionTypeBinding(transition), transitionData);
        }
        public IEnumerable<object> CannotChangeStateReasons(StateTransitionTypeBinding<TState, TTransition, TOwner> transitionBinding, object transitionData = null)
        {
            lock (lockObject)
            {
                if (!CurrentState.Equals(transitionBinding.From.Id))
                {
                    return new object[] { new InvalidFromStateException($"{transitionBinding.Info.Id} requires starting state of {transitionBinding.From.ToString()} but CurrentState is {CurrentState}") };
                }

                var parameters = new object[] { };

                List<object> results = null;

                if (false == (bool?)transitionBinding.From?.CanLeave?.Invoke(Owner, parameters))
                {
                    results = new List<object>();
                    results.Add(new StateMachineException("CanLeave returned false"));
                }
                if (false == (bool?)transitionBinding.CanTransitionMethod?.Invoke(Owner, parameters))
                {
                    if (results == null) results = new List<object>();
                    results.Add(new StateMachineException("CanTransition returned false"));
                }
                if (false == (bool?)transitionBinding.To?.CanEnter?.Invoke(Owner, parameters))
                {
                    if (results == null) results = new List<object>();
                    results.Add(new StateMachineException("CanEnter returned false"));
                }

                return results ?? Enumerable.Empty<object>();
            }
        }

        #endregion

        #region CanChangeState

        public bool CanChangeState(TTransition transition, object transitionData = null)
        {
            return CanChangeState(StateInfoProvider<TState, TTransition, TOwner>.Default.GetTransitionTypeBinding(transition), transitionData);
        }

        #endregion

        public bool TryChangeState(TTransition transition, object transitionData = null)
        {
            return TryChangeState(StateInfoProvider<TState, TTransition, TOwner>.Default.GetTransitionTypeBinding(transition), transitionData);
        }

        public void ChangeState(TTransition transition, object transitionData = null)
        {
            ChangeState(StateInfoProvider<TState, TTransition, TOwner>.Default.GetTransitionTypeBinding(transition), transitionData);
        }

        private object lockObject = new object();

        public bool TryChangeState(StateTransitionTypeBinding<TState, TTransition, TOwner> transitionBinding, object transitionData = null)
        {
            // See also - ChangeState for similar code
            lock (lockObject)
            {
                //// TODO FIXME: Pass this sc to the CanChangeState?
                //var sc = new StateChange<TState, TTransition, TOwner>
                //{
                //    Transition = transitionBinding,
                //    TransitionData = transitionData,
                //    IsDeterminingCanChange = true,
                //    //StateChangeContext =
                //};

                var reasons = CannotChangeStateReasons(transitionBinding, transitionData);
                if (reasons.Any()) return false;

                //sc.IsTest = false;

                //var context = new MultiTypeSeed

                //var sc = new StateChange<TState, TTransition, TOwner>
                //{
                //    Transition = transitionBinding,
                //    TransitionData = transitionData,
                //};

                DoChangeState(transitionBinding, transitionData);
                return true;
            }
        }

        private void DoChangeState(StateTransitionTypeBinding<TState, TTransition, TOwner> transitionBinding, object transitionData = null)
        {
            var parameters = new object[] { };

            transitionBinding.From?.OnLeaving?.Invoke(Owner, parameters);
            transitionBinding.OnTransitioningMethod?.Invoke(Owner, parameters);
            transitionBinding.To?.OnEntering?.Invoke(Owner, parameters);

            CurrentState = transitionBinding.Info.To;
        }

        public bool CanChangeState(StateTransitionTypeBinding<TState, TTransition, TOwner> transitionBinding, object transitionData = null)
        {
            return !CannotChangeStateReasons(transitionBinding, transitionData).Any();
        }

        public void ChangeState(StateTransitionTypeBinding<TState, TTransition, TOwner> transitionBinding, object transitionData = null)
        {
            // See also - TryChangeState for similar code

            lock (lockObject)
            {
                //// TODO FIXME: Pass this sc to the CanChangeState?
                //var sc = new StateChange<TState, TTransition, TOwner>
                //{
                //    Transition = transitionBinding,
                //    TransitionData = transitionData,
                //    IsDeterminingCanChange = true,
                //    //StateChangeContext =
                //};

                var reasons = CannotChangeStateReasons(transitionBinding, transitionData);
                if (reasons.Any()) throw new CannotChangeStateException(reasons);

                //sc.IsTest = false;

                //var context = new MultiTypeSeed

                //var sc = new StateChange<TState, TTransition, TOwner>
                //{
                //    Transition = transitionBinding,
                //    TransitionData = transitionData,
                //};

                DoChangeState(transitionBinding);
            }
        }
    }

    //public struct MultiTypeSeed : IExtendableMultiTyped
    //{
    //private MultiTyped {get;set;}
    // private void Create() { MultiTyped = new MultiTyped(); }
    //}

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
