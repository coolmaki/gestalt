namespace Supercluster.Lib.Domain;

public abstract class AggregateRoot : Entity
{
    // ------------------------------------------------------------
    // Backing Fields
    // ------------------------------------------------------------

    private readonly List<DomainEvent> _events = [];

    // ------------------------------------------------------------
    // Properties
    // ------------------------------------------------------------

    public IReadOnlyCollection<DomainEvent> Events => _events.AsReadOnly();

    // ------------------------------------------------------------
    // Methods
    // ------------------------------------------------------------

    internal void RaiseEvent(DomainEvent e)
    {
        _events.Add(e);
    }

    public void ClearEvents()
    {
        _events.Clear();
    }
}
