using System.Text.Json;
using common.Database.Model;
using Microsoft.Extensions.Caching.Distributed;

var builder = WebApplication.CreateBuilder(args);

const string ALL_COURIERS_INFO_KEY = "AllCouriers"; 
IConfiguration configuration = builder.Configuration;

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IQueueService>(s => new QueueService(new KafkaSettings{BootsrapServers = configuration["Kafka:Host"]}));
builder.Services.Configure<CourirerDbSettings>(
            configuration.GetSection("CourierDatabase"));
builder.Services.AddSingleton<ICourierService,CourierService>();
builder.Services.AddStackExchangeRedisCache(opt =>
            {
                opt.Configuration = $"{configuration["Redis:Host"]}:{configuration["Redis:Port"]}";
            });

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();


app.MapPost("/save-courier-location", async(Courier courier,IQueueService queueService,ILogger<Program> logger) =>
{
    var queueResponse = await queueService.AddQueue<CourierLocationInfo>(new CourierLocationInfo{
        CourierId = courier.Id,
        Latitude = courier.Latitude,
        Longtitude = courier.Longitude
    });
    if(queueResponse.QueueResponseType != QueueResponseTypes.Success)
        logger.LogError("Couldn't Add To Queue | Detail :"+queueResponse.ErrorMessage);
})
.WithName("SaveCourierLocation")
.WithOpenApi();

app.MapGet("/get-courier-last-location/{courierId}", async(string courierId,ICourierService courierService,IDistributedCache distributedCache,ILogger<Program> logger) =>
{
    var courierLastLocationInfo = await distributedCache.GetStringAsync(courierId);
    if(courierLastLocationInfo == null)
    {
        var lastLocation = await courierService.GetWithCourierIdAsync(courierId);
        if(lastLocation != null)
        {
            await distributedCache.SetStringAsync(courierId,JsonSerializer.Serialize(lastLocation),new DistributedCacheEntryOptions{
                AbsoluteExpiration = DateTimeOffset.UtcNow.AddSeconds(58),
            });
            return Results.Ok(lastLocation);
        }
        return Results.NotFound();
    }

    try{
        return Results.Ok(JsonSerializer.Deserialize<CourierLocation>(courierLastLocationInfo));
    }
    catch(Exception ex)
    {
        return Results.StatusCode(500);
    }
    
})
.WithName("GetCourierLastLocation")
.WithOpenApi();

app.MapGet("/get-all-couriers-last-location", async(ICourierService courierService,IDistributedCache distributedCache,ILogger<Program> logger) =>
{
    var allLocationsInfo = await distributedCache.GetStringAsync(ALL_COURIERS_INFO_KEY);
    if(allLocationsInfo == null)
    {
        var allLocations = await courierService.GetAsync();
        if(allLocations != null)
        {
            await distributedCache.SetStringAsync(ALL_COURIERS_INFO_KEY,JsonSerializer.Serialize(allLocations),new DistributedCacheEntryOptions{
                AbsoluteExpiration = DateTimeOffset.UtcNow.AddSeconds(58),
            });
            return Results.Ok(allLocations);
        }
        return Results.NotFound();
    }

    try{
        return Results.Ok(JsonSerializer.Deserialize<List<CourierLocation>>(allLocationsInfo));
    }
    catch(Exception ex)
    {
        return Results.StatusCode(500);
    }
    
})
.WithName("GetAllCourierLastLocation")
.WithOpenApi();


app.Run();
