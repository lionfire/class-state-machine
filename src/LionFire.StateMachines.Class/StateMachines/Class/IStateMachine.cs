

namespace LionFire.StateMachines.Class
{
    public interface IStateMachine
    {
        //bool CanTransition(IState from, IState to, object context);
        //Task Transition(IState from, IState to, object context = null);
    }
    public interface IStateMachine<TState, TTransition>
    {
    }
}
