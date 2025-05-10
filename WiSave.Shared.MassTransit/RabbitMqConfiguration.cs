namespace WiSave.Shared.MassTransit;

public sealed record RabbitMqConfiguration
{
    public string Host { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string Vhost { get; init; } = "/";
}