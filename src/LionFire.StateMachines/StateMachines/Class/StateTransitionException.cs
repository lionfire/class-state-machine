using System;

namespace LionFire.StateMachines.Class
{
    public class StateTransitionException : Exception
    {
        public StateTransitionException() { }
        public StateTransitionException(string message) : base(message) { }
        public StateTransitionException(string message, Exception inner) : base(message, inner) { }
    }
}
