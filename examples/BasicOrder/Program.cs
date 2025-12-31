using BasicOrder;

Console.WriteLine("=== LionFire.StateMachines Basic Order Example ===\n");

// Create a new order
var order = new Order("ORD-001", "John Smith", 99.99m);
Console.WriteLine($"Created: {order}\n");

// Process the order through its lifecycle
Console.WriteLine("--- Submitting order ---");
order.Submit();
Console.WriteLine($"Status: {order}\n");

Console.WriteLine("--- Processing order ---");
order.Process();
Console.WriteLine($"Status: {order}\n");

Console.WriteLine("--- Shipping order ---");
order.Ship();
Console.WriteLine($"Status: {order}\n");

Console.WriteLine("--- Delivering order ---");
order.Deliver();
Console.WriteLine($"Status: {order}\n");

// Demonstrate TryTransition for checking if transition is valid
Console.WriteLine("--- Attempting to ship again (should fail) ---");
var canShipAgain = order.StateMachine.TryTransition(OrderTransition.Ship);
Console.WriteLine($"Ship succeeded: {canShipAgain}");
Console.WriteLine($"Current state: {order.StateMachine.CurrentState}\n");

// Create another order and cancel it
Console.WriteLine("=== Cancellation Example ===\n");
var order2 = new Order("ORD-002", "Jane Doe", 49.99m);
Console.WriteLine($"Created: {order2}");
order2.CancelDraft();
Console.WriteLine($"Status: {order2}");
