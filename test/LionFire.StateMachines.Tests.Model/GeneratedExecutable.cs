using System.Collections.Generic;
using System;
using LionFire.Execution;
using TS = LionFire.Execution.ExecutionState;

namespace LionFire.StateMachines.Class.Tests
{
    [StateMachine(typeof(ExecutionState), typeof(ExecutionTransition), GenerateStateMachineFlags.None, IncludeTransitions = ExecutionTransition.Dispose
        //, IncludeStates = ExecutionState.Disposed
        )]
    //StateMachineStatePropertyName ="sm",
    //, ExecutionTransition.Initialize | ExecutionTransition.Start | ExecutionTransition.Finish | ExecutionTransition.CleanUp
    public partial class GeneratedExecutable
    {

        public bool InitializePrereq { get; set; } = true;
        public int CanInitializeCount { get; set; }
        public bool CanInitialize
        {
            get
            {
                CanInitializeCount++;
                return InitializePrereq;
            }
        }

        public void OnInitialize() => Log("OnInitialize");

        #region Leave Uninitialized

        public bool LeaveUninitializedPrereq { get; set; } = true;
        public int CanLeaveUninitializedCount { get; set; }
        public bool CanLeaveUninitialized
        {
            get
            {
                CanLeaveUninitializedCount++;
                return LeaveUninitializedPrereq;
            }
        }

        public void AfterUninitialized()
        {
            Log("AfterUninitialized");
        }

        #endregion

        #region CanReady

        public bool ReadyPrereq { get; set; } = true;
        public int CanReadyCount { get; set; }
        public bool CanReady
        {
            get
            {
                CanReadyCount++;
                return ReadyPrereq;
            }
        }

        #endregion

        public void OnReady() => Log("OnReady");
        public void AfterReady() => Log("AfterReady");
        public void OnStart() => Log("OnStart");
        public void AfterComplete() => Log("AfterComplete");

        public void OnFinished() => Log("OnFinished");

        #region Log for testing

        public Queue<string> LogMessageQueue { get; set; } = new Queue<string>();

        private void Log(string msg) => LogMessageQueue.Enqueue(msg);

        #endregion
    }

    //    public static class Transitions
    //    {
    //        public static StateTransitionTypeBinding<TS, ExecutionTransition, GeneratedExecutable> Initialize = new StateTransitionTypeBinding<TS, ExecutionTransition, GeneratedExecutable>(ExecutionTransition.Initialize)
    //        {
    //            Info = StateMachine<TS, ExecutionTransition>.GetTransitionInfo(ExecutionTransition.Initialize),
    //            OnTransitioningMethod = (owner) => owner.OnInitializing(),
    //            From = States.Uninitialized,
    //            To = States.Ready,
    //        };

    //    public static class States
    //    {
    //        public static StateTypeBinding<TS, ExecutionTransition, GeneratedExecutable> Uninitialized = new StateTypeBinding<TS, ExecutionTransition, GeneratedExecutable>(TS.Uninitialized)
    //        {
    //            //EnteringStateAction = owner => owner.OnReady(),
    //            OnLeaving = owner => owner.AfterUninitialized(),
    //        };




#if ExpectedGeneratedOutput

    public partial class GeneratedExecutable
    {
        public void Initialize() => StateMachine.Transition(ExecutionTransition.Initialize);
        public void Start() => StateMachine.Transition(ExecutionTransition.Start);
        public void Complete() => StateMachine.Transition(ExecutionTransition.Complete);
        public StateMachineState<LionFire.Execution.ExecutionState, LionFire.Execution.ExecutionTransition, GeneratedExecutable> StateMachine
        {
            get
            {
                if (stateMachine == null)
                {
                    stateMachine = StateMachine<TS, ExecutionTransition>.Create(this);
                }
                return stateMachine;
            }
        }
        private StateMachineState<TS, ExecutionTransition, GeneratedExecutable> stateMachine;

        // 
        //  Generated on 2021-10-24 8:26:17 AM
        // 
        // BaseDir: e:/Program Files/Microsoft Visual Studio/2022/Preview/MSBuild/Current/Bin/Roslyn
        /// Compilation: 
        //  - Source module name LionFire.StateMachines.Tests.Model.dll
        //  - source module locations: SourceFile(C:/src/StateMachines/test/LionFire.StateMachines.Tests.Model/GeneratedExecutable.cs[0..3334)), SourceFile(C:/src/StateMachines/test/LionFire.StateMachines.Tests.Model/ManualExecutable.cs[5204..5204)), SourceFile(C:/src/StateMachines/test/../obj/LionFire.StateMachines.Tests.Model/Debug/netstandard2.0/.NETStandard,Version=v2.0.AssemblyAttributes.cs[22..191)), SourceFile(C:/src/StateMachines/test/../obj/LionFire.StateMachines.Tests.Model/Debug/netstandard2.0/LionFire.StateMachines.Tests.Model.AssemblyInfo.cs[407..1344))
        // #r \"C:/src/StateMachines/bin/LionFire.StateMachines.Abstractions/Debug/netstandard2.0/LionFire.StateMachines.Abstractions.dll\"
        // #r \"C:/src/StateMachines/bin/LionFire.StateMachines.Tests.ExternalModel/Debug/netstandard2.0/LionFire.StateMachines.Tests.ExternalModel.dll\"
        // 
        // Resolved LionFire.Execution.ExecutionState
        // Resolved LionFire.Execution.ExecutionTransition
        // 
        // StateMachine: 
        //  - StateType: LionFire.Execution.ExecutionState
        //  - TransitionType: LionFire.Execution.ExecutionTransition
        //  - Options: None
        // 
        //  - method OnInitialize() for Initialize
        //  - method OnStart() for Start
        //  - method AfterComplete() for Complete
        //  - method AfterUninitialized() for state 'Uninitialized'
        //  - method OnReady() for state 'Ready'
        //  - method OnFinished() for state 'Finished'
        // States:
        //  * Uninitialized
        //  * Ready
        //  * Running
        //  * Finished
        //  - Disposed
        // 
        // Transitions:
        //  * Initialize
        //  - Invalidate
        //  - Deinitialize
        //  * Start
        //  - Skip
        //  - Noop
        //  - Undo
        //  * Complete
        //  - Terminate
        //  - Fail
        //  - CleanUp
        //  - Reset
        //  - Reuse
        //  - Create
        //  - Cancel
        //  - End
        //  - Fault
        //  - Dispose
        // 
        // }
    }
#endif

}
