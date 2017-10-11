namespace LionFire.StateMachines.Class
{
    public class StateTransitionInfo<TState, TTransition>:IStateTransitionInfo
    {
        //public virtual string ActionName => Id.ToString();

        public TTransition Id { get; set; }
        public TState From { get; set; }
        public TState To { get; set; }

        string IStateTransitionInfo.From => From.ToString();

        string IStateTransitionInfo.To => To.ToString();
    }
}