using System;
using System.Collections.Generic;
using System.Text;

namespace LionFire.StateMachines
{
    public class InvalidFromStateException : StateMachineException
    {
        public InvalidFromStateException() { }
        public InvalidFromStateException(string message) : base(message) { }
        public InvalidFromStateException(string message, Exception inner) : base(message, inner) { }
    }
}
