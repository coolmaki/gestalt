namespace Supercluster.Lib.Domain;

public abstract class AgreggateRoot : Entity
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
