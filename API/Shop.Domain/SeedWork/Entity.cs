using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Shop.Domain.SeedWork;

public abstract class Entity
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity), Key]
    public Guid Id { get; set; }
    public bool Excluded { get; set; }

    private List<IDomainEvent> _events = new();

    public void RaiseEvent(IDomainEvent e) => _events.Add(e);    
    public IReadOnlyCollection<IDomainEvent> GetEventsList() => _events.ToList();
    public void ClearEvents() => _events.Clear();
}
