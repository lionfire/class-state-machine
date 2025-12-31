using AdvancedWorkflow;

Console.WriteLine("=== LionFire.StateMachines Advanced Workflow Example ===\n");

// Create a document
var doc = new Document("API Design Guidelines", "Alice");
Console.WriteLine($"Created: {doc}\n");

// Try to submit without content - guard should prevent it
Console.WriteLine("--- Attempting to submit empty document ---");
var canSubmit = doc.StateMachine.TryTransition(DocumentTransition.SubmitForReview);
Console.WriteLine($"Submit succeeded: {canSubmit} (expected: False - no content)\n");

// Add content and try again
Console.WriteLine("--- Adding content and submitting ---");
doc.Content = "This document describes our API design guidelines...";
doc.SubmitForReview();
Console.WriteLine($"Status: {doc}\n");

// Approve the document (with before/after hooks)
Console.WriteLine("--- Approving document (watch for Before/After hooks) ---");
doc.Approve();
Console.WriteLine($"Status: {doc}\n");

// Publish the document
Console.WriteLine("--- Publishing document ---");
doc.Publish();
Console.WriteLine($"Status: {doc}\n");

// Archive the document
Console.WriteLine("--- Archiving document ---");
doc.Archive();
Console.WriteLine($"Status: {doc}\n");

// Unarchive it
Console.WriteLine("--- Unarchiving document ---");
doc.Unarchive();
Console.WriteLine($"Status: {doc}\n");

// =============================================
// Rejection workflow example
// =============================================
Console.WriteLine("=== Rejection Workflow Example ===\n");

var doc2 = new Document("Security Policy", "Bob");
doc2.Content = "Initial draft of security policy...";
Console.WriteLine($"Created: {doc2}");

doc2.SubmitForReview();
Console.WriteLine($"Status: {doc2}\n");

// Add reviewer comment and reject
Console.WriteLine("--- Reviewer rejects with comments ---");
doc2.AddReviewerComment("Please add section on password requirements");
doc2.Reject();
Console.WriteLine($"Status: {doc2}\n");

// Author requests changes (back to draft)
Console.WriteLine("--- Author requests changes to revise ---");
doc2.RequestChanges();
Console.WriteLine($"Status: {doc2}\n");

// Update and resubmit
Console.WriteLine("--- Author updates and resubmits ---");
doc2.Content += "\n\nPassword Requirements: Minimum 12 characters...";
doc2.SubmitForReview();
Console.WriteLine($"Status: {doc2}\n");

// Approve and publish
doc2.Approve();
doc2.Publish();
Console.WriteLine($"Final status: {doc2}\n");

// =============================================
// Permission guard example
// =============================================
Console.WriteLine("=== Permission Guard Example ===\n");

var doc3 = new Document("Internal Memo", "Charlie");
doc3.Content = "Memo content...";
doc3.SubmitForReview();
Console.WriteLine($"Status: {doc3}");

// Remove reviewer permission
Console.WriteLine("--- Attempting to approve without permission ---");
doc3.HasReviewerPermission = false;
var approved = doc3.StateMachine.TryTransition(DocumentTransition.Approve);
Console.WriteLine($"Approve succeeded: {approved} (expected: False - no permission)");
Console.WriteLine($"Status: {doc3}\n");

// Restore permission and approve
Console.WriteLine("--- Restoring permission and approving ---");
doc3.HasReviewerPermission = true;
doc3.Approve();
Console.WriteLine($"Final status: {doc3}");
