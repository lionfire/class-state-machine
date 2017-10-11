#if UNUSED
using System;
using System.Collections.Generic;
using System.Text;

namespace LionFire.StateMachines.Class.States
{
    [Flags]
    public enum CoreStates
    {
        Uninitialized = 0,
        Disposed = 1 << 0,
        Faulted = 1 << 1,
    }
}

#endif
