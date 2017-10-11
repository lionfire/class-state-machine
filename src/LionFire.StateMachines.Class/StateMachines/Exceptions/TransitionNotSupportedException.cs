using System;
using System.Collections.Generic;
using System.Text;

namespace LionFire.StateMachines
{
    public class TransitionNotAllowedException : StateMachineException
    {
        public TransitionNotAllowedException() { }
        public TransitionNotAllowedException(string message) : base(message) { }
        public TransitionNotAllowedException(string message, Exception inner) : base(message, inner) { }
    }
}
