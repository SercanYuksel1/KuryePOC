using System.Text.Json;
using common.Database.Model;
using Confluent.Kafka;

namespace worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;
    private readonly ConsumerConfig _consumerConfig;
    private readonly ICourierService _courierService;

    public Worker(ILogger<Worker> logger,IConfiguration configuration,ICourierService courierService)
    {
        _logger = logger;
        _configuration = configuration;
        _consumerConfig = new ConsumerConfig
        { 
            GroupId = _configuration["Kafka:GroupId"],
            BootstrapServers = _configuration["Kafka:Host"],
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
        _courierService = courierService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var c = new ConsumerBuilder<Ignore, string>(_consumerConfig).Build();
        
        c.Subscribe("CourierLocationsTopic");
        try
        {
            while (true)
            {
                try
                {
                    var consumer = c.Consume(stoppingToken);
                    var courierLocationInfo = JsonSerializer.Deserialize<CourierLocationInfo>(consumer.Message.Value);
                    Console.WriteLine("Consumer message:"+consumer.Message.Value);
                    await Insert(courierLocationInfo);
                }
                catch (ConsumeException e)
                {
                    _logger.LogError("An Error Occured When Consume Queue | Detail :"+e.Error.Reason);
                }
            }
        }
        catch (OperationCanceledException)
        {
            c.Close();
        }
        
    }

    private async Task Insert(CourierLocationInfo courierLocationInfo)
    {
        var courierLocation = new CourierLocation{
            CouirierId = courierLocationInfo.CourierId,
            Latitude = courierLocationInfo.Latitude,
            Longitude = courierLocationInfo.Longtitude,
        };

        try{
            await _courierService.CreateAsync(courierLocation);
        }
        catch(Exception ex){
            _logger.LogError("Couldn't Insert To Database | Detail:"+ex.Message);
        }
        
    }
}
