using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LionFire.StateMachines
{

    public class StateChangeAbortedException : StateMachineException
    {
        // REVIEW - Consider how this would support / reuse MultiTyping, ValidationContext, IExceptionInfo etc. without imposing constraints.  Maybe IEnumerable<object> is good enough.  Maybe MultiType is better?
        public IEnumerable<object> Reasons { get; private set; }

        public StateChangeAbortedException() { }
        public StateChangeAbortedException(IEnumerable<object> reasons) : base(reasons.Select(r => r.ToString()).Aggregate((x, y) => x + Environment.NewLine + y))
        {
            this.Reasons = reasons;
        }
        public StateChangeAbortedException(string message) : base(message) { }
        public StateChangeAbortedException(string message, Exception inner) : base(message, inner) { }

    }
}
