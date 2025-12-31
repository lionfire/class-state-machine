using LionFire.StateMachines;

namespace BasicOrder;

/// <summary>
/// Represents the possible transitions for an order.
/// Each transition is decorated with [Transition] to define the from/to states.
/// </summary>
public enum OrderTransition
{
    /// <summary>Submit a draft order for processing.</summary>
    [Transition(OrderState.Draft, OrderState.Submitted)]
    Submit,

    /// <summary>Begin processing a submitted order.</summary>
    [Transition(OrderState.Submitted, OrderState.Processing)]
    Process,

    /// <summary>Ship a processed order.</summary>
    [Transition(OrderState.Processing, OrderState.Shipped)]
    Ship,

    /// <summary>Mark a shipped order as delivered.</summary>
    [Transition(OrderState.Shipped, OrderState.Delivered)]
    Deliver,

    /// <summary>Cancel a draft order.</summary>
    [Transition(OrderState.Draft, OrderState.Cancelled)]
    CancelDraft,

    /// <summary>Cancel a submitted order.</summary>
    [Transition(OrderState.Submitted, OrderState.Cancelled)]
    CancelSubmitted
}
