using System;

namespace LionFire.StateMachines
{
    /// <summary>
    /// Use this on fields of an enum that defines the transitions for a state machine.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class TransitionAttribute : Attribute
    {
        /// <summary>
        /// The starting state required for this transition.
        /// </summary>
        public object From { get; private set; }
        /// <summary>
        /// The state after this transition completes.
        /// </summary>
        public object To { get; private set; }

        /// <summary>
        /// Use this on fields of an enum that defines the transitions for a state machine. 
        /// </summary>
        /// <param name="from">The starting state required for this transition.</param>
        /// <param name="to">The state after this transition completes.</param>
        public TransitionAttribute(object from, object to)
        {
            this.From = from;
            this.To = to;
        }
    }
}
