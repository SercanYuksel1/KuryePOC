namespace common.Database.Model;

public class CourirerDbSettings
{
    public string ConnectionString { get; set; } = null!;

    public string DatabaseName { get; set; } = null!;

    public string CourierCollectionName { get; set; } = null!;
}