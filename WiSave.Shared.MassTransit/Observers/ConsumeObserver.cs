using MassTransit;
using Microsoft.Extensions.Logging;

namespace WiSave.Shared.MassTransit.Observers;

public class ConsumeObserver(ILogger<ConsumeObserver> logger) : IConsumeObserver
{
    public Task PreConsume<T>(ConsumeContext<T> context) where T : class
    {
        logger.LogInformation("PreConsume: {MessageType}", typeof(T).Name);
        return Task.CompletedTask;
    }

    public Task PostConsume<T>(ConsumeContext<T> context) where T : class
    {
        logger.LogInformation("PostConsume: {MessageType}", typeof(T).Name);
        return Task.CompletedTask;
    }

    public Task ConsumeFault<T>(ConsumeContext<T> context, Exception exception) where T : class
    {
        logger.LogError(exception, "ConsumeFault: {MessageType}", typeof(T).Name);
        return Task.CompletedTask;
    }
}