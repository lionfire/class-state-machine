// TEMP copy to see if VS intellisense does any better at showing that files were generated, and picking up the generated classes with intellisense.

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
