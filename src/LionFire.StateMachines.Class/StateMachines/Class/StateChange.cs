namespace LionFire.StateMachines.Class
{
    //public interface IStateChange
    //{
    //}

    //public class StateChange<TState, TTransition>
    //{
    //    public TTransition Transition { get; set; }
    //    public object StateChangeContext { get; set; }
    //}
    public class StateChange<TState, TTransition,TOwner>
    {
        public bool IsDeterminingCanChange { get; set; }
        public StateTransitionTypeBinding<TState, TTransition,TOwner> Transition { get; set; }
        public object StateChangeContext { get; set; }
        public object TransitionData { get; internal set; }
    }
}
