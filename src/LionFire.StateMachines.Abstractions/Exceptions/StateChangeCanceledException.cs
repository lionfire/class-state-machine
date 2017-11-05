using System;

namespace LionFire.StateMachines
{
    public class StateChangeCanceledException : StateChangeAbortedException
    {
        public StateChangeCanceledException() { }
        public StateChangeCanceledException(string message) : base(message) { }
        public StateChangeCanceledException(string message, Exception inner) : base(message, inner) { }
    }
}
