using System.Collections.Generic;
using System;
using LionFire.Execution;

namespace LionFire.StateMachines.Class.Tests
{

    [StateMachine(typeof(ExecutionState), typeof(ExecutionTransition), GenerateStateMachineFlags.DisableGeneration)]
    //, ExecutionTransition.Initialize | ExecutionTransition.Start | ExecutionTransition.Finish | ExecutionTransition.CleanUp
    public partial class ManualExecutable
    {

        #region Manual portion


        //public void Initialize() => StateMachine. ChangeState(ExecutionTransition.Initialize);
        public void Initialize() => StateMachine.ChangeState(Transitions.Initialize);
        public void Start() => StateMachine.ChangeState(Transitions.Start);
        //public void Complete() => StateMachine.ChangeState(ExecutionTransition.Complete);
        //public void Terminate() => StateMachine.ChangeState(ExecutionTransition.Terminate);
        //public void Dispose() => StateMachine.ChangeState(ExecutionTransition.Dispose);

        public static class Transitions
        {
            public static StateTransitionTypeBinding<ExecutionState, ExecutionTransition, ManualExecutable> Initialize = new StateTransitionTypeBinding<ExecutionState, ExecutionTransition, ManualExecutable>(ExecutionTransition.Initialize)
            {
                Info = StateMachine<ExecutionState, ExecutionTransition>.GetTransitionInfo(ExecutionTransition.Initialize),
                //OnTransitioningMethod = (owner) => owner.OnInitializing(),
                From = States.Uninitialized,
                To = States.Ready,
            };
            public static StateTransitionTypeBinding<ExecutionState, ExecutionTransition, ManualExecutable> Start = new StateTransitionTypeBinding<ExecutionState, ExecutionTransition, ManualExecutable>(ExecutionTransition.Initialize)
            {
                Info = StateMachine<ExecutionState, ExecutionTransition>.GetTransitionInfo(ExecutionTransition.Start),
                //OnTransitioningMethod = (owner) => owner.OnStarting(),
                From = States.Ready,
                To = States.Running,
            };
        }
        public static class States
        {
            public static StateTypeBinding<ExecutionState, ExecutionTransition, ManualExecutable> Uninitialized = new StateTypeBinding<ExecutionState, ExecutionTransition, ManualExecutable>(ExecutionState.Uninitialized)
            {
                //EnteringStateAction = owner => owner.OnReady(),
                //OnLeaving = owner => owner.AfterUninitialized(),
            };
            public static StateTypeBinding<ExecutionState, ExecutionTransition, ManualExecutable> Ready = new StateTypeBinding<ExecutionState, ExecutionTransition, ManualExecutable>(ExecutionState.Ready)
            {
                //OnEntering = owner => owner.OnReady(),
                //OnLeaving = owner => owner.AfterReady(),
            };
            public static StateTypeBinding<ExecutionState, ExecutionTransition, ManualExecutable> Running = new StateTypeBinding<ExecutionState, ExecutionTransition, ManualExecutable>(ExecutionState.Running)
            {
                //EnteringStateAction = owner => owner.OnReady(),
                //LeavingStateAction = owner => owner.AfterReady(),
            };
            public static StateTypeBinding<ExecutionState, ExecutionTransition, ManualExecutable> Finished = new StateTypeBinding<ExecutionState, ExecutionTransition, ManualExecutable>(ExecutionState.Finished)
            {
                //OnEntering = owner => owner.OnFinished(),
                //LeavingStateAction = owner => owner.AfterReady(),
            };
        }
        public StateMachineState<ExecutionState, ExecutionTransition, ManualExecutable> StateMachine => stateMachine;
        private StateMachineState<ExecutionState, ExecutionTransition, ManualExecutable> stateMachine;
        private void InitStateMachine()
        {
            stateMachine = StateMachine<ExecutionState, ExecutionTransition>.CreateState(this);
        }

        #endregion

        public ManualExecutable()
        {
            InitStateMachine();
        }

        public void AfterUninitialized()
        {
            Log("AfterUninitialized");
        }
        public void OnInitializing()
        {
            Log("OnInitializing");
        }
        public void OnReady()
        {
            Log("OnReady");
        }
        public void AfterReady()
        {
            Log("AfterReady");
        }
        public void OnStarting()
        {
            Log("OnStarting");
        }
        public void AfterComplete()
        {
            Log("AfterComplete");
        }

        public void OnFinished()
        {
            Log("OnFinished");
        }

        public Stack<string> LastMessage { get; set; } = new Stack<string>();

        private void Log(string msg) => LastMessage.Push(msg);
    }
}
