using System;
using System.Collections.Generic;
using System.Text;

namespace LionFire.StateMachines
{
    ///// <summary>
    ///// Transition event handler
    ///// </summary>
    /////// <typeparam name="TState">The enum used by the state machine for states</typeparam>
    ///// <typeparam name="TTransition">The enum used by the state machine for transitions</typeparam>
    public delegate void StateChangeEventHandler<TState,TTransition>(IStateChange<TState,TTransition> context);
}
