#if UNUSED
using System.Collections.Generic;

namespace LionFire.StateMachines.Class
{
    public interface IRemembersLastStateTransition
    {
        //StateChange LastStateChange { get; set; }
    }
    // Basis for undo/redo?
    public interface IRemembersStateTransitions<TState,TTransition,TOwner> : IRemembersLastStateTransition
    {
        Queue<StateChange<TState, TTransition,TOwner>> History { get; }
    }
}
#endif
