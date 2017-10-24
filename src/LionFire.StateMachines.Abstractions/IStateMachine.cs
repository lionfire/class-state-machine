using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.StateMachines
{
    /// <summary>
    /// An instance of a state machine.  Has a current state.
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <typeparam name="TTransition"></typeparam>
    public interface IStateMachine<TState, TTransition>
    {
        TState CurrentState { get; }

        void Transition(TTransition transition, object context = null);
        //Task TransitionAsync(TTransition transition, object context = null);

        IStateChange<TState, TTransition> LastStateChange { get; }

        /// <summary>
        /// State is changing.
        /// </summary>
        event StateChangeEventHandler<TState, TTransition> StateChanging;

        /// <summary>
        /// State changed. 
        /// </summary>
        event StateChangeEventHandler<TState, TTransition> StateChanged;


        /// <summary>
        /// The state change did not complete.  (See also: transitionContext may contain a ValidationContext which may provide reasons why the state change was canceled or invalid.)
        /// </summary>
        event StateChangeEventHandler<TState, TTransition> StateChangeAborted;
    }

}
