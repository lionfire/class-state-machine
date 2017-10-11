using System;
using System.Reflection;

namespace LionFire.StateMachines.Class
{
    public class StateTypeBinding<TState, TTransition, TOwner>
    {
        public TState Id { get; set; }
        //public StateTransitionInfo<TState, TTransition> Info { get; set; }

        public StateTypeBinding(TState state)
        {
            //this.Info = StateMachine<TState, TTransition>.GetStateInfo(state);
            this.Id = state;
        }
        public MethodInfo CanEnter { get; set; } 
        public MethodInfo OnEntering { get; set; } 
        public MethodInfo CanLeave { get; set; }
        public MethodInfo OnLeaving { get; set; }


    }
}