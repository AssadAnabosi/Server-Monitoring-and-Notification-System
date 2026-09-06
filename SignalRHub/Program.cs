using SignalRHub;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

var app = builder.Build();

app.MapGet("/", () => "SignalR alert hub is running.");
app.MapHub<AlertsHub>("/hubs/alerts");

app.Run();
