namespace RabbitMQClientLibrary;

public class RabbitMQOptions
{
    public const string SectionName = "RabbitMQOptions";
    
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    
    public override string ToString()
    {
        return $"amqp://{Uri.EscapeDataString(UserName)}:{Uri.EscapeDataString(Password)}@{HostName}:{Port}/{Uri.EscapeDataString(VirtualHost.TrimStart('/'))}";
    }
}
