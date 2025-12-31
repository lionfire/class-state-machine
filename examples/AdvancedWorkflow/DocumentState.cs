namespace AdvancedWorkflow;

/// <summary>
/// Represents the possible states of a document in a review workflow.
/// </summary>
public enum DocumentState
{
    /// <summary>Document is being drafted.</summary>
    Draft,

    /// <summary>Document is under review.</summary>
    UnderReview,

    /// <summary>Document has been approved.</summary>
    Approved,

    /// <summary>Document has been rejected.</summary>
    Rejected,

    /// <summary>Document has been published.</summary>
    Published,

    /// <summary>Document has been archived.</summary>
    Archived
}
