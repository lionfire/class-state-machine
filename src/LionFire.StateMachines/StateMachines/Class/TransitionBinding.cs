using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LionFire.StateMachines.Class
{
    internal class TransitionBinding<TState, TTransition, TOwner>
    {

        #region Relationships

        public StateTransitionInfo<TState, TTransition> Info { get; set; }

        public StateBinding<TState, TTransition, TOwner> From { get; set; }
        public StateBinding<TState, TTransition, TOwner> To { get; set; }

        #endregion

        #region Construction

        public TransitionBinding(TTransition transition)
        {
            this.Info = StateMachine<TState, TTransition>.GetTransitionInfo(transition);
        }

        #endregion

        #region Methods

        public Func<TOwner, IStateChange<TState, TTransition>,bool?> CanTransition { get; set; } = CanNoop;
        public Action<TOwner, IStateChange<TState, TTransition>> OnTransitioning { get; set; } = Noop;


        private static Func<TOwner, IStateChange<TState, TTransition>,bool?> CanNoop = (o, sc) => null;
        private static Action<TOwner, IStateChange<TState, TTransition>> Noop = (o, sc) => { };

        #endregion
    }
}
