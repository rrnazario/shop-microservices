namespace Shop.Common.Configurations;

public record EFPersistenceOptions(
    int MaxRetryCount,
    int CommandTimeout,
    bool EnableDetailedErrors,
    bool EnableSensitiveDataLogging)
{
    public const string PersistenceSection = "EFPersistence";
}
