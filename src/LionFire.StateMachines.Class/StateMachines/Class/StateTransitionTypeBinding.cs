using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LionFire.StateMachines.Class
{

    public class StateTransitionTypeBinding<TState, TTransition, TOwner>
    {
        public StateTransitionInfo<TState, TTransition> Info { get; set; }

        public StateTypeBinding<TState, TTransition, TOwner> From { get; set; }
        public StateTypeBinding<TState, TTransition, TOwner> To { get; set; }

        public StateTransitionTypeBinding(TTransition transition)
        {
            this.Info = StateMachine<TState, TTransition>.GetTransitionInfo(transition);

            //var mi = typeof(TTransition).GetMember(transition.ToString());
            //var attr = mi.GetCustomAttribute<TransitionAttribute>();
            //attr.From

        }
        public MethodInfo CanTransitionMethod { get; set; } = null;
        public MethodInfo OnTransitioningMethod { get; set; } = null;
    }
}