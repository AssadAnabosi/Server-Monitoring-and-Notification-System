namespace SignalRClientLibrary;

public sealed class SignalRHubOptions
{
    public const string SectionName = "SignalRHub";

    public string Url { get; set; } = "http://localhost:5000/hubs/alerts";
}
