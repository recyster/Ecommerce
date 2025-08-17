using ECommerce.Infrastructure.Caching;
using ECommerce.Infrastructure.Messaging;
using ECommerce.Worker;
using Serilog;
var builder = Host.CreateApplicationBuilder(args);

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/worker-log-.txt", rollingInterval: RollingInterval.Day)
     .WriteTo.Seq(builder.Configuration["Seq:Url"] ?? "http://localhost:5341")
    .MinimumLevel.Information()
    .CreateLogger();
 builder.Services.AddSingleton<IRabbitMqConsumer>(sp =>
    new RabbitMqConsumer("localhost"));
builder.Services.AddSingleton<IRedisService>(sp =>
    new RedisService("localhost:6379,abortConnect=false" ));
builder.Services.AddHostedService<OrderProcessingWorker>();

var host = builder.Build();
host.Run();
