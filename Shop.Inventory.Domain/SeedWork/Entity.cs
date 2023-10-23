namespace Shop.Inventory.Domain.SeedWork;

public abstract class Entity
{
    public Guid Id { get; set; }
    public int Version { get; set; }

    private List<DomainEvent> _events = new();

    public void RaiseEvent(DomainEvent e) 
    {
        _events.Add(e);
        Version++;
    }
    public IReadOnlyCollection<DomainEvent> GetEventsList() => _events.ToList();
    public void ClearEvents() => _events.Clear();
}
