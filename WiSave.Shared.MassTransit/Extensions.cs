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
        Action<IRabbitMqBusFactoryConfigurator, IBusRegistrationContext>? configureBroker = null,
        Action<IBusRegistrationConfigurator>? configureMassTransit = null,
        params Type[] consumerTypes)
        where TBus : class, IBus
    {
        services.AddConsumeObserver<ConsumeObserver>();
        services.AddConsumeObserver<PublishObserver>();

        services.AddMassTransit<TBus>(x =>
        {
            x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter(includeNamespace: false));

            x.AddConsumers(consumerTypes);

            x.AddDelayedMessageScheduler();

            configureMassTransit?.Invoke(x);
            
            x.AddConfigureEndpointsCallback((context, _, cfg) =>
            {
                cfg.UseDelayedRedelivery(r => r.Intervals(TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(30)));
                cfg.UseMessageRetry(r => r.Immediate(5));
                cfg.UseInMemoryOutbox(context);
            });

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.UseJsonSerializer();
                
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

                configureBroker?.Invoke(cfg, context);

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}