using System.Text.Json;
using Confluent.Kafka;

public class QueueService : IQueueService
{
    private readonly ProducerConfig _producerConfig;
    public QueueService(KafkaSettings kafkaSettings)
    {
        _producerConfig = new(){
            BootstrapServers = kafkaSettings.BootsrapServers
        };
    }

    public async Task<QueueResponse> AddQueue<T>(T payLoad)
    {
        
        using var producer = new ProducerBuilder<Null, string>(_producerConfig).Build();
        try
        {
            var dr = await producer.ProduceAsync("CourierLocationsTopic", new Message<Null,string> { Value= JsonSerializer.Serialize<T>(payLoad) });
            return new QueueResponse{
                QueueResponseType = QueueResponseTypes.Success,
                ErrorMessage = null
            };
        }
        catch (ProduceException<Null, string> ex)
        {
            return new QueueResponse{
                QueueResponseType = QueueResponseTypes.Error,
                ErrorMessage = ex.Message
            };
        }
    }
}