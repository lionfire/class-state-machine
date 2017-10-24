namespace LionFire.StateMachines.Class
{
    // REVIEW - is this interface needed?  Will states have descriptive info added via attributes or other means?
    public interface IStateTransitionInfo
    {
        string From { get; }
        string To { get; }
    }
}