using LionFire.StateMachines.Class;

namespace AdvancedWorkflow;

/// <summary>
/// A document class demonstrating advanced state machine features:
/// - Guard properties (Can{Transition}) to conditionally allow transitions
/// - Before/After transition hooks
/// - State entry/exit handlers
/// - Event subscription for state changes
/// </summary>
[StateMachine(typeof(DocumentState), typeof(DocumentTransition))]
public partial class Document
{
    public string Title { get; }
    public string Author { get; }
    public string? Content { get; set; }
    public string? ReviewerComments { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public DateTime? PublishedAt { get; private set; }

    // Simulated permissions
    public bool HasReviewerPermission { get; set; } = true;
    public bool HasPublisherPermission { get; set; } = true;

    public Document(string title, string author)
    {
        Title = title;
        Author = author;

        // Subscribe to state changes
        StateMachine.StateChanged += OnStateChanged;
    }

    // Event handler for all state changes
    private void OnStateChanged(object? sender, EventArgs e)
    {
        Console.WriteLine($"  [Event] State changed to: {StateMachine.CurrentState}");
    }

    // ==========================================
    // GUARD PROPERTIES
    // ==========================================
    // Guards prevent transitions when they return false

    /// <summary>
    /// Guard: Can only submit for review if document has content.
    /// </summary>
    public bool CanSubmitForReview => !string.IsNullOrWhiteSpace(Content);

    /// <summary>
    /// Guard: Can only approve if user has reviewer permission.
    /// </summary>
    public bool CanApprove => HasReviewerPermission;

    /// <summary>
    /// Guard: Can only reject if user has reviewer permission.
    /// </summary>
    public bool CanReject => HasReviewerPermission;

    /// <summary>
    /// Guard: Can only publish if user has publisher permission.
    /// </summary>
    public bool CanPublish => HasPublisherPermission;

    // ==========================================
    // TRANSITION HANDLERS (On{Transition})
    // ==========================================

    private void OnSubmitForReview()
    {
        Console.WriteLine($"  Document '{Title}' submitted for review by {Author}");
    }

    private void OnApprove()
    {
        ApprovedAt = DateTime.UtcNow;
        Console.WriteLine($"  Document '{Title}' approved at {ApprovedAt}");
    }

    private void OnReject()
    {
        Console.WriteLine($"  Document '{Title}' rejected");
    }

    private void OnRequestChanges()
    {
        ReviewerComments = null; // Clear for new revision
        Console.WriteLine($"  Document '{Title}' returned to draft for changes");
    }

    private void OnPublish()
    {
        PublishedAt = DateTime.UtcNow;
        Console.WriteLine($"  Document '{Title}' published at {PublishedAt}");
    }

    private void OnArchive()
    {
        Console.WriteLine($"  Document '{Title}' archived");
    }

    private void OnUnarchive()
    {
        Console.WriteLine($"  Document '{Title}' unarchived");
    }

    // ==========================================
    // BEFORE/AFTER HOOKS
    // ==========================================

    private void BeforeApprove()
    {
        Console.WriteLine($"  [Before] About to approve '{Title}'...");
    }

    private void AfterApprove()
    {
        Console.WriteLine($"  [After] Approval complete. Sending notification email...");
    }

    private void BeforePublish()
    {
        Console.WriteLine($"  [Before] Validating document before publish...");
    }

    private void AfterPublish()
    {
        Console.WriteLine($"  [After] Document published. Updating search index...");
    }

    // ==========================================
    // HELPER METHODS
    // ==========================================

    public void AddReviewerComment(string comment)
    {
        ReviewerComments = comment;
        Console.WriteLine($"  Reviewer added comment: {comment}");
    }

    public override string ToString() =>
        $"[{StateMachine.CurrentState}] {Title} by {Author}";
}
