# Echo MCP Server

A streamable HTTP MCP (Model Context Protocol) server built with C# .NET 8, designed for Azure App Service deployment.

## Features

- **SSE (Server-Sent Events) Transport**: Supports streaming responses over HTTP
- **Echo Tool**: A simple tool that echoes back "Hello " + your message
- **Azure App Service Ready**: Configured for easy deployment to Azure
- **Health Check Endpoint**: Built-in health monitoring for Azure

## Endpoints

- `POST /sse` - MCP server endpoint (Server-Sent Events)
- `GET /health` - Health check endpoint
- `GET /` - Server information

## Tools

### Echo
Echoes back a greeting with the provided message.

**Input:**
- `message` (string, required): The message to echo back

**Output:**
- Returns "Hello {message}"

## Running Locally

```bash
dotnet restore
dotnet run
```

The server will start on `http://localhost:5000` (or the port specified in launchSettings.json).

## Deploying to Azure App Service

1. Build the project:
```bash
dotnet publish -c Release -o ./publish
```

2. Deploy to Azure App Service using:
   - Azure CLI
   - Visual Studio
   - GitHub Actions
   - Azure DevOps Pipelines

## Testing the Echo Tool

Send a POST request to `/sse` with the following JSON-RPC format:

```json
{"jsonrpc":"2.0","id":"1","method":"initialize","params":{"protocolVersion":"2024-11-05","clientInfo":{"name":"test-client","version":"1.0.0"}}}
{"jsonrpc":"2.0","id":"2","method":"tools/list","params":{}}
{"jsonrpc":"2.0","id":"3","method":"tools/call","params":{"name":"Echo","arguments":{"message":"World"}}}
```

Expected response from Echo tool:
```
Hello World
```

## Project Structure

```
├── Models/
│   └── McpModels.cs          # MCP protocol models
├── Services/
│   └── McpService.cs         # MCP server implementation
├── Tools/
│   └── EchoTool.cs           # Echo tool implementation
├── Program.cs                 # Application entry point
├── EchoMcpServer.csproj      # Project file
└── appsettings.json          # Configuration
```

## Requirements

- .NET 8 SDK
- Azure subscription (for deployment)

## License

MIT
