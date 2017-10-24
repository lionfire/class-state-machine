// FUTURE
//using Xunit;
//using LionFire.Execution;
//using ExecutionState = LionFire.Execution.ExecutionState;

//namespace LionFire.StateMachines.Class.Tests
//{
//    public class ManualExecutableTests
//    {

//        public ManualExecutableTests()
//        {
//        }

//        [Fact]
//        public void NormalExecutableStateFlow()
//        {
//            var te = new ManualExecutable();
//            //var teA= new ManualExecutableA();
//            Assert.Equal(te.StateMachine.CurrentState, ExecutionState.Uninitialized);
//            te.Initialize();
//            Assert.Equal(te.LastMessage.Pop(), "OnReady");
//            Assert.Equal(te.LastMessage.Pop(), "OnInitializing");
//            Assert.Equal(te.LastMessage.Pop(), "AfterUninitialized");
//            Assert.Equal(te.StateMachine.CurrentState, ExecutionState.Ready);
//            te.Start();
//            Assert.Equal(te.LastMessage.Pop(), "OnStarting");
//            Assert.Equal(te.LastMessage.Pop(), "AfterReady");
//            Assert.Equal(te.StateMachine.CurrentState, ExecutionState.Running);
//            //te.Complete();
//            //Assert.Equal(te.StateMachine.CurrentState, ExecutionState.Finished);
//            //Assert.Equal(te.LastMessage.Pop(), "AfterComplete");
//            //te.Dispose();
//            //Assert.Equal(te.StateMachine.CurrentState, ExecutionState.Disposed);
//        }

//    }
//}
