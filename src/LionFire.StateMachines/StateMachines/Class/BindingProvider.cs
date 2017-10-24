using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace LionFire.StateMachines.Class
{

    public class BindingProvider<TState, TTransition, TOwner>
    {
        [Flags]
        private enum MemberType
        {
            None = 0,
            Method = 1 << 1,
            Property = 1 << 2,
            Any = Method | Property,
        }

        #region Cache

        Dictionary<string, MethodInfo> methods;
        Dictionary<string, PropertyInfo> properties;

        public void ClearIntermediateCache()
        {
            methods = null;
            properties = null;
        }

        private Dictionary<TTransition, TransitionBinding<TState, TTransition, TOwner>> transitions = new Dictionary<TTransition, TransitionBinding<TState, TTransition, TOwner>>();
        private Dictionary<TState, StateBinding<TState, TTransition, TOwner>> states = new Dictionary<TState, StateBinding<TState, TTransition, TOwner>>();

        #endregion

        #region Static Accessor and Configuration

        public static BindingProvider<TState, TTransition, TOwner> Default { get; set; } = new BindingProvider<TState, TTransition, TOwner>();

        public static StateMachineConventions Conventions
        {
            get => conventions ?? StateMachineConventions.DefaultConventions;
            set => conventions = value;
        }
        private static StateMachineConventions conventions;

        public static BindingFlags MethodBindingFlags { get; set; } = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        public static BindingFlags PropertyBindingFlags { get; set; } = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        #endregion

        #region (Public) Get Methods

        internal virtual StateBinding<TState, TTransition, TOwner> GetStateBinding(TState state)
        {
            if (states.ContainsKey(state)) return states[state];

            var fi = typeof(TState).GetField(state.ToString());
            var aTransition = fi.GetCustomAttribute<StateAttribute>();

            var binding = new StateBinding<TState, TTransition, TOwner>(state)
            {
                CanEnter = GetHandlerFunc(GetMethod(state, Conventions.CanEnterStatePrefixes)),
                CanLeave = GetHandlerFunc(GetMethod(state, Conventions.CanLeaveStatePrefixes)),
                OnEntering = GetHandlerAction(GetMethod(state, Conventions.EnteringStatePrefixes)),
                OnLeaving = GetHandlerAction(GetMethod(state, Conventions.EnteringStatePrefixes)),
            };

            states.Add(state, binding);
            return binding;
        }

        internal TransitionBinding<TState, TTransition, TOwner> GetTransitionBinding(TTransition transition)
        {
            if (transitions.ContainsKey(transition)) return transitions[transition];

            var fi = typeof(TTransition).GetField(transition.ToString());
            var aTransition = fi.GetCustomAttribute<TransitionAttribute>();

            var fromInfo = GetStateBinding((TState)aTransition.From);
            var toInfo = GetStateBinding((TState)aTransition.To);

            var binding = new TransitionBinding<TState, TTransition, TOwner>(transition)
            {
                Info = StateMachine<TState, TTransition>.GetTransitionInfo(transition),
                CanTransition = GetHandlerFunc(GetMethod(transition, Conventions.CanTransitionPrefixes)),
                OnTransitioning = GetHandlerAction(GetMethod(transition, Conventions.OnTransitionPrefixes, MemberType.Method)),
                From = fromInfo,
                To = toInfo,
            };

            transitions.Add(transition, binding);
            return binding;
        }

        #endregion

        #region (Private) Helper Methods

        private MethodInfo GetMethod(TTransition transition, IEnumerable<string> prefixes, MemberType memberType = MemberType.Any)
        {
            return _GetMethod(transition.ToString(), prefixes, memberType);
        }
        private MethodInfo GetMethod(TState state, IEnumerable<string> prefixes, MemberType memberType = MemberType.Any)
        {
            return _GetMethod(state.ToString(), prefixes, memberType);
        }

        private MethodInfo _GetMethod(string stateOrTransitionName, IEnumerable<string> prefixes, MemberType memberType = MemberType.Any)
        {
            if (memberType.HasFlag(MemberType.Method))
            {
                if (methods == null) methods = typeof(TOwner).GetMethods(MethodBindingFlags).ToDictionary(fi => fi.Name);

                foreach (var prefix in prefixes)
                {
                    var fieldName = prefix + stateOrTransitionName;
                    if (methods.ContainsKey(fieldName))
                    {
                        return methods[fieldName];
                    }
                }
            }

            if (memberType.HasFlag(MemberType.Property))
            {
                if (properties == null) properties = typeof(TOwner).GetProperties(PropertyBindingFlags).ToDictionary(fi => fi.Name);

                foreach (var prefix in prefixes)
                {
                    var fieldName = prefix + stateOrTransitionName;
                    if (properties.ContainsKey(fieldName) && properties[fieldName].CanRead)
                    {
                        return properties[fieldName].GetGetMethod();
                    }
                }
            }

            return null;
        }

        private Action<TOwner, IStateChange<TState, TTransition>> GetHandlerAction(MethodInfo mi)
        {
            if (mi == null) return null;
            var param = mi.GetParameters();
            if (param.Length == 0)
            {
                return (o, sc) => mi.Invoke(o, Array.Empty<object>());
            }
            else if (param.Length == 1 && param[0].ParameterType == typeof(IStateChange<TState, TTransition>))
            {
                return (o, sc) => mi.Invoke(o, new object[] { sc });
            }
            else
            {
                Debug.WriteLine("[state machine] Unsupported state machine method.  Must have zero parameters, or one IStateChange<TState,TTransition> parameter: " + mi.Name);
                return null;
            }
        }
        private Func<TOwner, IStateChange<TState, TTransition>,bool?> GetHandlerFunc(MethodInfo mi)
        {
            if (mi == null) return null;
            var param = mi.GetParameters();
            if (param.Length == 0)
            {
                return (o, sc) => (bool?)mi.Invoke(o, Array.Empty<object>());
            }
            else if (param.Length == 1 && param[0].ParameterType == typeof(IStateChange<TState, TTransition>))
            {
                return (o, sc) => (bool?)mi.Invoke(o, new object[] { sc });
            }
            else
            {
                Debug.WriteLine("[state machine] Unsupported state machine method.  Must have zero parameters, or one IStateChange<TState,TTransition> parameter: " + mi.Name);
                return null;
            }
        }

        #endregion

    }
}
