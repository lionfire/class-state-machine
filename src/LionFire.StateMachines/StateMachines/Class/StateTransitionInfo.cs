namespace LionFire.StateMachines.Class
{
    public class StateTransitionInfo<TState, TTransition> : IStateTransitionInfo
    {
        public StateTransitionInfo()
        {

        }
        public TTransition Id { get; set; }
        public TState From { get; set; }
        public bool HasFrom { get; set; }
        public TState To { get; set; }
        public bool HasTo { get; set; }
        string IStateTransitionInfo.From => HasFrom ? From.ToString() : "(null)";

        string IStateTransitionInfo.To => HasTo ? To.ToString() : "(null)";
    }
}