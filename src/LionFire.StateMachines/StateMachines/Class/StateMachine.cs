using System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.StateMachines.Class
{
    //public interface IStateModel { }
    //public interface IStateModel<S, T> : IStateModel
    //{
    //    //S States { get; }
    //    //T Transitions { get; }
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
    {

        #region Factory

        //public static IStateMachine<TState, TTransition> Create(object owner)
        public static StateMachineState<TState, TTransition, TOwner> Create<TOwner>(TOwner owner)
        {
            var type = typeof(StateMachineState<,,>).MakeGenericType(typeof(TState), typeof(TTransition), owner.GetType());
            return (StateMachineState<TState, TTransition, TOwner>)Activator.CreateInstance(type, new object[] { owner });
        }

        #endregion

        public static TState StartingState { get; private set; }
        public static TState EndingState { get; private set; }
        public static TTransition StartingTransition { get; private set; }
        public static TTransition EndingTransition { get; private set; }

        //public TState States { get; private set; }
        //public TTransition Transitions { get; private set; }

        static Dictionary<TTransition, StateTransitionInfo<TState, TTransition>> transitions = new Dictionary<TTransition, StateTransitionInfo<TState, TTransition>>();
        //Dictionary<TStates, IState> states = new Dictionary<TStates, IState>();

        static StateMachine()
        {
            foreach (var mi in typeof(TTransition).GetFields())
            {
                var attr = mi.GetCustomAttribute<TransitionAttribute>();
                if (attr != null && attr.From == null && attr.To != null && attr.To is TState startingState)
                {
                    StartingState = startingState;
                    StartingTransition = (TTransition)mi.GetValue(null); // UNTESTED
                    break;
                }
            }

            var defValue = typeof(TState).BaseType == typeof(Enum) ? (TState)(object) 0 : default(TState);
            if (StartingState?.Equals(defValue) == true) throw new ArgumentException("StartingState missing.  There must be at least one Transition with a null From and a non-null To.");

            foreach (var mi in typeof(TTransition).GetFields())
            {
                var attr = mi.GetCustomAttribute<TransitionAttribute>();
                if (attr != null && attr.To == null && attr.From != null && attr.From is TState endingState)
                {
                    // NOTE: Only the first ending transition will be used.  ENH: Validate there is only one ending transition (and end state)
                    EndingState = endingState;
                    EndingTransition = (TTransition)mi.GetValue(null); // UNTESTED
                    break;
                }
            }

#if StartingStateAttribute // OLD
            if(StartingState==null){
                foreach (var mi in typeof(TState).GetFields())
                {
                    if (mi.GetCustomAttribute<StartAttribute>() != null)
                    {
                        StartingState = (TState)mi.GetValue(null);
                        break;
                    }
                }
            }
#endif

            foreach (var mi in typeof(TTransition).GetFields())
            {
                var attr = mi.GetCustomAttribute<TransitionAttribute>();
                if (attr == null) continue;

                var val = (TTransition)mi.GetValue(null);
                var sti = new StateTransitionInfo<TState, TTransition>()
                {
                    Id = val,
                };

                if (attr.From != null) { sti.From = (TState)attr.From; sti.HasFrom = true; }
                else sti.HasFrom = false;
                if (attr.To != null) { sti.To = (TState)attr.To; sti.HasTo = true; }
                else sti.HasTo = false;

                transitions.Add(val, sti);
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

    //public interface ITransitionComponent<TTargetState>
    //{
    //    bool CanTransition(Type from, Type to, object context);
    //    Task Transition(Type from, Type to, object context = null);
    //}
}
