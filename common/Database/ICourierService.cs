using common.Database.Model;

public interface ICourierService
{
    public Task<List<CourierLocation>> GetAsync();
        
    public Task<CourierLocation?> GetAsync(string id);
    public Task<CourierLocation?> GetWithCourierIdAsync(string courierId);
    public Task CreateAsync(CourierLocation newCourierLocation);

    public Task UpdateAsync(string id, CourierLocation courierLocation);

    public Task RemoveAsync(string id);
}