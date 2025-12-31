using LionFire.StateMachines.Class;

namespace BasicOrder;

/// <summary>
/// A simple order class demonstrating the state machine library.
///
/// The [StateMachine] attribute triggers source generation that creates:
/// - A StateMachine property for accessing state machine functionality
/// - Public methods for each transition (Submit(), Process(), Ship(), etc.)
/// </summary>
[StateMachine(typeof(OrderState), typeof(OrderTransition))]
public partial class Order
{
    public string OrderId { get; }
    public string CustomerName { get; }
    public decimal Total { get; }

    public Order(string orderId, string customerName, decimal total)
    {
        OrderId = orderId;
        CustomerName = customerName;
        Total = total;
    }

    // Convention method: Called when Submit transition occurs
    private void OnSubmit()
    {
        Console.WriteLine($"Order {OrderId} submitted by {CustomerName}");
    }

    // Convention method: Called when Process transition occurs
    private void OnProcess()
    {
        Console.WriteLine($"Order {OrderId} is being processed");
    }

    // Convention method: Called when Ship transition occurs
    private void OnShip()
    {
        Console.WriteLine($"Order {OrderId} has been shipped");
    }

    // Convention method: Called when Deliver transition occurs
    private void OnDeliver()
    {
        Console.WriteLine($"Order {OrderId} has been delivered!");
    }

    // Convention methods for cancellation
    private void OnCancelDraft() => Console.WriteLine($"Draft order {OrderId} cancelled");
    private void OnCancelSubmitted() => Console.WriteLine($"Submitted order {OrderId} cancelled");

    public override string ToString() =>
        $"Order {OrderId}: {CustomerName}, ${Total:F2} - Status: {StateMachine.CurrentState}";
}
