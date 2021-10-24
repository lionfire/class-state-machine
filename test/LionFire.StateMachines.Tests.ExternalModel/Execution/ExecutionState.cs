//using LionFire.StateMachines.Class;
using System;

namespace LionFire.Execution
{
    [Flags]
    public enum ExecutionState : int
    {
      //  [Start]
        Uninitialized = 1 << 0,
        Ready = 1 << 1,
        Running = 1 << 2,
        Finished = 1 << 3,
//[End]
        Disposed = 1 << 4,
    }
}
