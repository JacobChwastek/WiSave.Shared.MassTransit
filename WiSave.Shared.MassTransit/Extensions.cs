using System.Text.Json.Serialization;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using WiSave.Shared.MassTransit.Observers;

namespace WiSave.Shared.MassTransit;

public static class Extensions
{
    public static IServiceCollection AddMessaging<TBus>(
        this IServiceCollection services,
        RabbitMqConfiguration rabbitMqConfiguration,
        Action<IRabbitMqBusFactoryConfigurator, IBusRegistrationContext>? configureAdditional = null,
        params Type[] consumerTypes)
        where TBus : class, IBus
    {
        services.AddConsumeObserver<ConsumeObserver>();
        services.AddConsumeObserver<PublishObserver>();

        services.AddMassTransit<TBus>(x =>
        {
            x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter(includeNamespace: false));
            
            x.AddConsumers(consumerTypes);

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitMqConfiguration.Host, rabbitMqConfiguration.Vhost, h =>
                {
                    h.Username(rabbitMqConfiguration.Username);
                    h.Password(rabbitMqConfiguration.Password);
                });
                
                
                configureAdditional?.Invoke(cfg, context);

                cfg.ConfigureEndpoints(context);
            });
        });
        
        return services;
    }
    
}