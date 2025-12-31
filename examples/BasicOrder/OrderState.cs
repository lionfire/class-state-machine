namespace BasicOrder;

/// <summary>
/// Represents the possible states of an order.
/// </summary>
public enum OrderState
{
    /// <summary>Order has been created but not submitted.</summary>
    Draft,

    /// <summary>Order has been submitted and is awaiting processing.</summary>
    Submitted,

    /// <summary>Order is being processed.</summary>
    Processing,

    /// <summary>Order has been shipped to the customer.</summary>
    Shipped,

    /// <summary>Order has been delivered to the customer.</summary>
    Delivered,

    /// <summary>Order has been cancelled.</summary>
    Cancelled
}
