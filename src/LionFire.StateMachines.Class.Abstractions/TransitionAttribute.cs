using System;

namespace LionFire.StateMachines.Class
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple =false, Inherited = false)]
    public sealed class TransitionAttribute : Attribute
    {
        public object From { get; private set; }
        public object To { get; private set; }
        public TransitionAttribute(object from, object to)
        {
            this.From = from;
            this.To = to;
        }
    }
}
