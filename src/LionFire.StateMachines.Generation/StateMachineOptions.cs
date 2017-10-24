namespace LionFire.StateMachines.Class
{

    public class GenerateStateMachineOptions
    {
        public StateMachineAttribute Attribute { get; set; }
        public GenerateStateMachineOptions() { }
        public GenerateStateMachineOptions(StateMachineAttribute attr) { this.Attribute = attr; }
    }
}
