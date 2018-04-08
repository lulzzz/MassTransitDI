using CQRSlite.Domain;
using CQRSlite.Events;
using MassTransit;
using MassTransit.RabbitMqTransport;
using MassTransitDI.Domain.CommandHandlers;
using MassTransitDI.Domain.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;

namespace MassTransitDI
{
    class Program
    {
        static void Main(string[] args)
        {
            IServiceProvider container = null;

            var configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", true, true)
                    .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            Log.Information("Starting Receiver...");

            var services = new ServiceCollection();

            services.AddSingleton<IEventStore, InMemoryEventStore>();
            services.AddScoped<IEventPublisher, MassTransitEventPublisher>();
            services.AddScoped<ISession, Session>();
            services.AddScoped<IRepository, Repository>();

            services.AddScoped<CreateAggregateCommandHandler>();

            services.AddSingleton(context => Bus.Factory.CreateUsingRabbitMq(x =>
            {
                IRabbitMqHost host = x.Host(new Uri("rabbitmq://guest:guest@localhost:5672/test"), h => { });

                x.ReceiveEndpoint(host, $"receiver_queue", e =>
                {
                    e.UseCqrsLite(container);
                    e.Consumer(() => container.GetService<CreateAggregateCommandHandler>());
                });

                x.UseSerilog();
            }));

            container = services.BuildServiceProvider();

            var busControl = container.GetRequiredService<IBusControl>();

            busControl.Start();

            Log.Information("Receiver started...");

            busControl.Publish<CreateRecord>(new
            {
                Id = NewId.NextGuid(),
                Index = 0,
                CorrelationId = NewId.NextGuid()
            });
        }
    }
}