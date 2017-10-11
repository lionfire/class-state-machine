using System;
using System.Collections.Generic;
using System.Text;

namespace LionFire.StateMachines.Class
{
    public interface IStateMachineState<TState, TTransition,TOwner>
    {
        TState CurrentState { get; }
        StateChange<TState,TTransition,TOwner> LastTransition { get; }
    }

}
