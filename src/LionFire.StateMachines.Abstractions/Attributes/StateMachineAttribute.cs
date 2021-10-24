using System;
using System.Collections.Generic;
using System.Text;

namespace LionFire.StateMachines.Class
{
    // TODO: Make generation attribute separate
    // Todo: Change StateMachineAttribute to StateTransitions(Allowed = , Disallowed = )]

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    //[CodeGenerationAttribute(typeof(StateMachineGenerator))]
    //[Conditional("CodeGeneration")]
    public sealed class StateMachineAttribute : Attribute
    {
        public GenerateStateMachineFlags Options { get; private set; }
        public Type StateType { get; private set; }
        public Type TransitionType { get; private set; }

        public object IncludeTransitions { get; set; }
        public object ExcludeTransitions { get; set; }
        public object IncludeStates { get; set; }
        public object ExcludeStates { get; set; }

        public string StateMachineStatePropertyName { get; set; } = "StateMachine";

        public StateMachineAttribute() { }
        public StateMachineAttribute(Type state, Type transition)
        {
            this.StateType = state;
            this.TransitionType = transition;
        }

        public StateMachineAttribute(Type state, Type transition, GenerateStateMachineFlags options)
        {
            this.StateType = state;
            this.TransitionType = transition;
            this.Options = options;
        }

    }
}
