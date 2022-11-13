using common.Database.Model;
using worker;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((builder,services) =>
    {
        services.AddHostedService<Worker>();
        services.Configure<CourirerDbSettings>(
            builder.Configuration.GetSection("CourierDatabase"));
        services.AddSingleton<ICourierService,CourierService>();
    })
    .Build();

host.Run();
