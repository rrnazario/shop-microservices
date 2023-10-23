namespace Shop.Common.Contracts;

public record ProductCreatedIntegrationEvent(Guid Id, string Name, string Description, DateTime CreatedDate);
