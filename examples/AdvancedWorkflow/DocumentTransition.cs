using LionFire.StateMachines;

namespace AdvancedWorkflow;

/// <summary>
/// Represents the possible transitions for a document.
/// </summary>
public enum DocumentTransition
{
    /// <summary>Submit document for review.</summary>
    [Transition(DocumentState.Draft, DocumentState.UnderReview)]
    SubmitForReview,

    /// <summary>Approve the document.</summary>
    [Transition(DocumentState.UnderReview, DocumentState.Approved)]
    Approve,

    /// <summary>Reject the document.</summary>
    [Transition(DocumentState.UnderReview, DocumentState.Rejected)]
    Reject,

    /// <summary>Request changes to a rejected document.</summary>
    [Transition(DocumentState.Rejected, DocumentState.Draft)]
    RequestChanges,

    /// <summary>Publish an approved document.</summary>
    [Transition(DocumentState.Approved, DocumentState.Published)]
    Publish,

    /// <summary>Archive a published document.</summary>
    [Transition(DocumentState.Published, DocumentState.Archived)]
    Archive,

    /// <summary>Unarchive a document back to published state.</summary>
    [Transition(DocumentState.Archived, DocumentState.Published)]
    Unarchive
}
