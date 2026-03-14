using EchoMcpServer.Services;

var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddSingleton<DocumentExtractionService>();

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

app.MapMcp("/mcp");

app.Run();
