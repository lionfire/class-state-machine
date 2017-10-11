using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LionFire.StateMachines.Class
{
    [Flags]
    public enum MemberType
    {
        None = 0,
        Method = 1 << 1,
        Property = 1 << 2,
        Any = Method | Property,
    }

    public class StateInfoProvider<TState, TTransition, TOwner>
    {
        #region Cache

        Dictionary<string, MethodInfo> methods;
        Dictionary<string, PropertyInfo> properties;

        //public void ClearCache()
        //{
        //    methods = null;
        //    properties = null;
        //}

        #endregion

        #region Static Accessor and Configuration

        public static StateInfoProvider<TState, TTransition, TOwner> Default { get; set; } = new StateInfoProvider<TState, TTransition, TOwner>();

        public static StateMachineConventions Conventions
        {
            get => conventions ?? StateMachineConventions.DefaultConventions;
            set => conventions = value;
        }
        private static StateMachineConventions conventions;

        #endregion

        #region (Public) Get Methods

        public virtual StateTypeBinding<TState, TTransition, TOwner> GetStateTypeBinding(TState state)
        {
            // TODO TOOPTIMIZE - Cache result
            var fi = typeof(TState).GetField(state.ToString());
            var aTransition = fi.GetCustomAttribute<StateAttribute>();

            var binding = new StateTypeBinding<TState, TTransition, TOwner>(state)
            {
                CanEnter = GetMethod(state, Conventions.CanEnterStatePrefixes),
                CanLeave = GetMethod(state, Conventions.CanLeaveStatePrefixes),
                OnEntering = GetMethod(state, Conventions.EnteringStatePrefixes, MemberType.Method),
                OnLeaving = GetMethod(state, Conventions.LeavingStatePrefixes, MemberType.Method),
            };
            return binding;
        }
        public virtual StateTransitionTypeBinding<TState, TTransition, TOwner> GetTransitionTypeBinding(TTransition transition)
        {
            // TODO TOOPTIMIZE - Cache result
            var fi = typeof(TTransition).GetField(transition.ToString());
            var aTransition = fi.GetCustomAttribute<TransitionAttribute>();

            var fromInfo = GetStateTypeBinding((TState)aTransition.From);
            var toInfo = GetStateTypeBinding((TState)aTransition.To);

            var binding = new StateTransitionTypeBinding<TState, TTransition, TOwner>(transition)
            {
                Info = StateMachine<TState, TTransition>.GetTransitionInfo(transition),
                CanTransitionMethod = GetMethod(transition, Conventions.CanTransitionPrefixes),
                OnTransitioningMethod = GetMethod(transition, Conventions.OnTransitionPrefixes, MemberType.Method),
                From = fromInfo,
                To = toInfo,
            };
            return binding;
        }

        #endregion

        #region (Private) Helper Methods

        internal MethodInfo GetMethod(TTransition transition, IEnumerable<string> prefixes, MemberType memberType = MemberType.Any)
        {
            return _GetMethod(transition.ToString(), prefixes, memberType);
        }
        internal MethodInfo GetMethod(TState state, IEnumerable<string> prefixes, MemberType memberType = MemberType.Any)
        {
            return _GetMethod(state.ToString(), prefixes, memberType);
        }

 

        private MethodInfo _GetMethod(string stateOrTransitionName, IEnumerable<string> prefixes, MemberType memberType = MemberType.Any)
        {
            if (memberType.HasFlag(MemberType.Method))
            {
                if (methods == null) methods = typeof(TOwner).GetMethods(BindingFlags.Public | BindingFlags.Instance).ToDictionary(fi => fi.Name);

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
                if (properties == null) properties = typeof(TOwner).GetProperties(BindingFlags.Public | BindingFlags.Instance).ToDictionary(fi => fi.Name);

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

        #endregion



   
    }
}
