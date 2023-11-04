namespace Shop.Domain.SeedWork;

public abstract class Entity
{
    public Guid Id { get; protected set; }
    public bool Excluded { get; protected set; }

    private readonly List<IDomainEvent> _events = new();

    public void RaiseEvent(IDomainEvent e) => _events.Add(e);    
    public IReadOnlyCollection<IDomainEvent> GetEventsList() => _events.ToList();
    public void ClearEvents() => _events.Clear();
}
