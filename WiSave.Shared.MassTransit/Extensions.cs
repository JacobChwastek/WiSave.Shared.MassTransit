using System.Text.Json.Serialization;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using WiSave.Shared.MassTransit.Converters;
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

            x.AddConfigureEndpointsCallback((context,name,cfg) =>
            {
                cfg.UseDelayedRedelivery(r => r.Intervals(TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(30)));
                cfg.UseMessageRetry(r => r.Immediate(5));
                cfg.UseInMemoryOutbox(context);
            });
            
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitMqConfiguration.Host, rabbitMqConfiguration.Vhost, h =>
                {
                    h.Username(rabbitMqConfiguration.Username);
                    h.Password(rabbitMqConfiguration.Password);
                });
                
                cfg.ConfigureJsonSerializerOptions(options =>
                {
                    options.Converters.Add(new DateOnlyJsonConverter());
                    return options;
                });
                
                configureAdditional?.Invoke(cfg, context);

                cfg.ConfigureEndpoints(context);
            });
        });
        
        return services;
    }
    
}