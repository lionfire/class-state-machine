using System;
using System.Reflection;

namespace LionFire.StateMachines.Class
{
    internal class StateBinding<TState, TTransition, TOwner>
    {
        public TState Id { get; set; }
        //public StateTransitionInfo<TState, TTransition> Info { get; set; } REVIEW

        public StateBinding(TState state)
        {
            //this.Info = StateMachine<TState, TTransition>.GetStateInfo(state); REVIEW
            this.Id = state;
        }
        public Func<TOwner, IStateChange<TState, TTransition>, bool?> CanEnter { get; set; }
        public Action<TOwner, IStateChange<TState, TTransition>> OnEntering { get; set; }
        public Func<TOwner, IStateChange<TState, TTransition>, bool?> CanLeave { get; set; }
        public Action<TOwner, IStateChange<TState, TTransition>> OnLeaving { get; set; }

    }
}
