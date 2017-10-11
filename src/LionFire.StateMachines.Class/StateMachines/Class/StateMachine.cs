using System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// TODO: Once this gets fleshed out, move to Core? Or create shared library?

namespace LionFire.StateMachines.Class
{

    //public interface IState
    //{
    //    string Name { get; }
    //}

    //public abstract class StateBase : IState
    //{
    //    public string Name { get; private set; }
    //    public StateBase(string name) { this.Name = name; }
    //    //    public abstract IEnumerable<Type> AllowsTransitionTo { get; }
    //    //    public abstract IEnumerable<Type> AllowsTransitionFrom { get; }
    //}
    //public class State
    //{
    //    public State(string name) : base(name) { }

    //    public static implicit operator State(string name) { return new State(name); }
    //}
    //public class Uninitialized : StateBase
    //{
    //}

    //public interface IHasStateMachine<TState,TTransition>
    //{
    //    IStateMachine<TState,TTransition> StateMachine { get; }
    //}
    //public interface IHasStateMachine
    //{
    //    IStateMachine StateMachine { get; }
    //}
    //public static class IHasStateMachineExtensions
    //{
    //    public static TState CurrentState<TState, TTransition>(this IHasStateMachine<TState, TTransition> sm)
    //    {
    //        return default(TState);//STUB
    //    }
    //}

    public interface IStateModel { }
    public interface IStateModel<S, T> : IStateModel
    {
        //S States { get; }
        //T Transitions { get; }
    }

    //public class TypeStateMachine<TStates, TTransitions, TOwner> : IStateModel<TStates, TTransitions>
    //{

    //    public void Bind()
    //    {
    //        foreach (var transition in Transitions)
    //        {

    //        }
    //    }
    //}
    //public class TransitionTypeBinding<TState, TTransition>
    //{
    //    public StateTransitionInfo<TState, TTransition> TransitionInfo { get; set; }

    //    //public Action<StateChange<TState,TTransition>> OnTransitioning { get; set; }
    //    public MethodInfo OnTransitioningMethod { get; set; }

    //    public static TransitionTypeBinding<TState, TTransition> TryBind<TOwner>(StateTransitionInfo<TState, TTransition> TransitionInfo)
    //    {
    //        var mi = typeof(TOwner).GetMethod($"On{TransitionInfo.ActionName}");
    //        if (mi != null)
    //        {
    //            return new TransitionTypeBinding<TState, TTransition> { OnTransitioningMethod = mi };
    //        }
    //        return null;
    //    }
    //}

    //public class StateTypeBinding<TState, TTransition>
    //{
    //    //public StateInfo<TState, TTransition> StateInfo { get; set; }
    //    public TState State { get; set; }
    //    public Action<StateChange<TState, TTransition>> OnEntering { get; set; }
    //    public Action<StateChange<TState, TTransition>> OnLeaving { get; set; }

    //}


    public static class StateMachine
    {
        public static IStateTransitionInfo GetTransitionInfo(Type stateType, Type transitionType, object transition)
        {
            return (IStateTransitionInfo)typeof(StateMachine<,>).MakeGenericType(stateType, transitionType).GetMethod(nameof(GetTransitionInfo /* wrong one but they should match*/)).Invoke(null, new object[] { transition });
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TTransition">An enum consisting of flags for states</typeparam>
    public static class StateMachine<TState, TTransition> //: IStateModel<TState, TTransition>
    //, IFreezable
    {

        public static TState StartingState { get; private set; }
        //public TState States { get; private set; }
        //public TTransition Transitions { get; private set; }


        public static StateMachineState<TState, TTransition, TOwner> CreateState<TOwner>(TOwner owner)
        {
            return new StateMachineState<TState, TTransition, TOwner>(owner);
        }

        static Dictionary<TTransition, StateTransitionInfo<TState, TTransition>> transitions = new Dictionary<TTransition, StateTransitionInfo<TState, TTransition>>();
        //Dictionary<TStates, IState> states = new Dictionary<TStates, IState>();

        static StateMachine()
        {
            foreach (var mi in typeof(TState).GetFields())
            {
                if (mi.GetCustomAttribute<StartAttribute>() != null)
                {
                    StartingState = (TState)mi.GetValue(null);
                    break;
                }
            }

            foreach (var mi in typeof(TTransition).GetFields())
            {
                var attr = mi.GetCustomAttribute<TransitionAttribute>();
                if (attr == null) continue;

                var val = (TTransition)mi.GetValue(null);
                transitions.Add(val, new StateTransitionInfo<TState, TTransition>
                {
                    Id = val,
                    From = (TState)attr.From,
                    To = (TState)attr.To,
                });

            }
        }
        //public StateMachine<TState, TTransition> Add(TState from, TState to, TTransition transition = default(TTransition))
        //{
        //    if (isFrozen) throw new Exception("Object is frozen");
        //    ////if (transition == null) transition = $"{from} -> {to}";
        //    var t = new StateTransitionInfo<TState, TTransition>()
        //    {
        //        Id = transition,
        //        From = from,
        //        To = to,
        //    };
        //    transitions.Add(t.Id, t);
        //    return this;
        //}
        //public void Freeze()
        //{
        //    isFrozen = true;
        //}
        //private bool isFrozen = false;

        //public StateMachine<TState, TTransition> Add(string from, string to, string transitionName = null)
        //{
        //    //to = to.Split('.').Last();
        //    return Add((TState)Enum.Parse(typeof(TState), from), (TState)Enum.Parse(typeof(TState), to), (TTransition)Enum.Parse(typeof(TTransition), transitionName));
        //}


        //public StateMachine<TStates, TTransitions> TrimForClass<TStateOwner>()
        //{
        //    //var mis = typeof(TStateOwner).GetRuntimeMethods();

        //    Type t = typeof(TStateOwner);
        //    var mis = t.GetMethods();
        //    // see if mis contains OnTransition or OnState or AfterState methods.  Either no param, or object param.

        //    //TStates 

        //    //foreach(var

        //    //var trimmed = new StateModel<TStates, TTransitions>();

        //    return this;
        //}

        public static StateTransitionInfo<TState, TTransition> GetTransitionInfo(TTransition transition)
        {
            if (transitions.ContainsKey(transition)) return transitions[transition];
            return null;
        }
    }

    public interface ITransitionComponent<TTargetState>
    {
        bool CanTransition(Type from, Type to, object context);
        Task Transition(Type from, Type to, object context = null);
    }
    public static class IStateMachineExtensions
    {
        //public static Task Transition(this IStateMachine sm, Type to, object context = null) { return sm.Transition(null, to, context); }
        //public static Task Transition<TFrom, TTo>(this IStateMachine sm, object context = null) { return sm.Transition(typeof(TFrom), typeof(TTo), context); }
    }

    public class StateTransitionException : Exception
    {
        public StateTransitionException() { }
        public StateTransitionException(string message) : base(message) { }
        public StateTransitionException(string message, Exception inner) : base(message, inner) { }
    }
}
namespace LionFire.StateMachines.Class
{
    //public static class StateChanger
    //{
    //    public static Task<StateChangeContext> ToState(this object obj, object state)
    //    {
    //        //var c = new StateChangeContext();

    //    }
    //}

    public class StateChangeContext
    {
    }

    //public struct ValidatingStateChangeContext
    //{
    //    public ValidationContext ValidationContext { get; set; }
    //}

}
