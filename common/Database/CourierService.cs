using common.Database.Model;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

public class CourierService : ICourierService
{
    private readonly IMongoCollection<CourierLocation> _courierLocationsCollection;

    public CourierService(
        IOptions<CourirerDbSettings> courierDbSettings)
    {
        var mongoClient = new MongoClient(
            courierDbSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            courierDbSettings.Value.DatabaseName);

        _courierLocationsCollection = mongoDatabase.GetCollection<CourierLocation>(
            courierDbSettings.Value.CourierCollectionName);
    }

    

    public async Task<CourierLocation?> GetAsync(string id) =>
        await _courierLocationsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(CourierLocation newCourierLocation) =>
        await _courierLocationsCollection.InsertOneAsync(newCourierLocation);

    public async Task UpdateAsync(string id, CourierLocation courierLocation) =>
        await _courierLocationsCollection.ReplaceOneAsync(x => x.Id == id, courierLocation);

    public async Task RemoveAsync(string id) =>
        await _courierLocationsCollection.DeleteOneAsync(x => x.Id == id);
    
    public async Task<List<CourierLocation>> GetAsync() =>
        await _courierLocationsCollection.Aggregate()
        .Group(c => c.CouirierId, g => new CourierLocation{
            CouirierId = g.Last().CouirierId,
            Latitude = g.Last().Latitude,
            Longitude = g.Last().Longitude,
            CreatedAt = g.Last().CreatedAt})
        .ToListAsync();

    public async Task<CourierLocation?> GetWithCourierIdAsync(string courierId) =>
        await _courierLocationsCollection.Find(x => x.CouirierId == courierId).SortByDescending(x => x.CreatedAt).FirstOrDefaultAsync();
}