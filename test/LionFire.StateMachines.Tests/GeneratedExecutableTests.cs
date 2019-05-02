using Xunit;
using LionFire.Execution;
using ExecutionState = LionFire.Execution.ExecutionState;
using System;

namespace LionFire.StateMachines.Class.Tests
{
    public class GeneratedExecutableTests
    {

        public GeneratedExecutableTests()
        {
        }

        #region TransitionBinding

        [Fact]
        public void TransitionBinding()
        {
            var binding = StateInfoProvider<ExecutionState, ExecutionTransition, GeneratedExecutable>.Default.GetTransitionTypeBinding(ExecutionTransition.Initialize);

            Assert.NotNull(binding.CanTransitionMethod);
            Assert.Equal("get_CanInitialize", binding.CanTransitionMethod.Name);
        }

        #endregion

        #region StateBinding

        //[Fact]
        //public void StateBinding()
        //{
        //    var binding = StateInfoProvider<ExecutionState, ExecutionTransition, GeneratedExecutable>.Default.GetStateTypeBinding(ExecutionState.Ready);

        //    //Assert.NotNull(binding.CanEnter);
        //    //Assert.NotNull(binding.CanLeave);
        //}

        #endregion

        #region CanEnter

        [Fact]
        public void EnterPrereq()
        {
            var te = new GeneratedExecutable();
            te.Initialize();
            Assert.Equal(1, te.CanReadyCount);
            Assert.Equal(ExecutionState.Ready, te.StateMachine.CurrentState);
        }

        [Fact]
        public void EnterPrereqEx()
        {
            var te = new GeneratedExecutable();
            te.ReadyPrereq = false;

            Exception ex = Assert.Throws<CannotChangeStateException>(() => te.Initialize());

            Assert.Equal(ExecutionState.Uninitialized, te.StateMachine.CurrentState);
        }

        [Fact]
        public void TryEnterTrue()
        {
            var te = new GeneratedExecutable();

            Assert.True(te.StateMachine.TryChangeState(ExecutionTransition.Initialize));

            Assert.Equal(ExecutionState.Ready, te.StateMachine.CurrentState);
        }

        [Fact]
        public void TryEnterFalse()
        {
            var te = new GeneratedExecutable();
            te.ReadyPrereq = false;

            Assert.False(te.StateMachine.TryChangeState(ExecutionTransition.Initialize));

            Assert.Equal(ExecutionState.Uninitialized, te.StateMachine.CurrentState);
        }

        #endregion
        #region CanLeave

        public void LeavePrereq()
        {
            var te = new GeneratedExecutable();
            te.Initialize();
            Assert.Equal(1, te.CanLeaveUninitializedCount);
            Assert.Equal(ExecutionState.Ready, te.StateMachine.CurrentState);
        }

        [Fact]
        public void LeavePrereqEx()
        {
            var te = new GeneratedExecutable();
            te.LeaveUninitializedPrereq = false;

            Exception ex = Assert.Throws<CannotChangeStateException>(() => te.Initialize());

            Assert.Equal(ExecutionState.Uninitialized, te.StateMachine.CurrentState);
        }

        [Fact]
        public void TryLeaveTrue()
        {
            var te = new GeneratedExecutable();

            Assert.Equal(true, te.StateMachine.TryChangeState(ExecutionTransition.Initialize));

            Assert.Equal(ExecutionState.Ready, te.StateMachine.CurrentState);
        }

        [Fact]
        public void TryLeaveFalse()
        {
            var te = new GeneratedExecutable();
            te.LeaveUninitializedPrereq = false;

            Assert.Equal(false, te.StateMachine.TryChangeState(ExecutionTransition.Initialize));

            Assert.Equal(ExecutionState.Uninitialized, te.StateMachine.CurrentState);
        }

        #endregion
        #region CanTransition

        [Fact]
        public void TransitionPrereq()
        {
            var te = new GeneratedExecutable();
            te.Initialize();
            Assert.Equal(1, te.CanInitializeCount);
            Assert.Equal(ExecutionState.Ready, te.StateMachine.CurrentState);
        }

        [Fact]
        public void TransitionPrereqEx()
        {
            var te = new GeneratedExecutable();
            te.InitializePrereq = false;

            Exception ex = Assert.Throws<CannotChangeStateException>(() => te.Initialize());

            Assert.Equal(ExecutionState.Uninitialized, te.StateMachine.CurrentState);
        }

        [Fact]
        public void TryTransitionTrue()
        {
            var te = new GeneratedExecutable();

            Assert.Equal(true, te.StateMachine.TryChangeState(ExecutionTransition.Initialize));

            Assert.Equal(ExecutionState.Ready, te.StateMachine.CurrentState);
        }

        [Fact]
        public void TryTransitionFalse()
        {
            var te = new GeneratedExecutable();
            te.InitializePrereq = false;

            Assert.Equal(false, te.StateMachine.TryChangeState(ExecutionTransition.Initialize));

            Assert.Equal(ExecutionState.Uninitialized, te.StateMachine.CurrentState);
        }
        #endregion


        [Fact]
        public void Current_State_Flow()
        {
            var te = new GeneratedExecutable();
            Assert.Equal(ExecutionState.Uninitialized,te.StateMachine.CurrentState);
            te.Initialize();
            Assert.Equal(ExecutionState.Ready, te.StateMachine.CurrentState);
            te.Start();
            Assert.Equal(ExecutionState.Running, te.StateMachine.CurrentState);
            te.Complete();
            Assert.Equal(ExecutionState.Finished, te.StateMachine.CurrentState);
            //te.Dispose();
            //Assert.Equal(te.StateMachine.CurrentState, ExecutionState.Disposed);
        }

        [Fact]
        public void OnStateHandlers()
        {
            var te = new GeneratedExecutable();
            Func<string> next = () => te.LogMessageQueue.Dequeue();

            te.Initialize();

            Assert.Equal("AfterUninitialized", next());
            Assert.Equal("OnInitialize", next());
            Assert.Equal("OnReady", next());

            te.Start();

            //Assert.Equal(next(), "OnStart");
            //Assert.Equal(te.LogMessageQueue.Dequeue(), "AfterReady");
            te.Complete();
            //Assert.Equal(te.LogMessageQueue.Dequeue(), "AfterComplete");
            //te.Dispose();
        }
    }
}
