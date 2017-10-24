using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LionFire.StateMachines
{
    public class CannotChangeStateException : StateMachineException
    {
        public IEnumerable<object> Reasons { get; private set; }
        public CannotChangeStateException() { }
        public CannotChangeStateException(IEnumerable<object> reasons) : base(reasons.Select(r => r.ToString()).Aggregate((x, y) => x + Environment.NewLine + y))
        { this.Reasons = reasons; }
        public CannotChangeStateException(string message) : base(message) { }
        public CannotChangeStateException(string message, Exception inner) : base(message, inner) { }
    }
}
