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

## ✅ Azure Resources (Pre-Deployed)

The following Azure resources are **already in place** and ready for deployment:

| Resource | Name | Region | Status |
|----------|------|--------|--------|
| **Web App** | `wa-mcpserver-sweden` | swedencentral | ✅ Active |
| **Resource Group** | `rg-mcpserverdemo-sweden` | swedencentral | ✅ Active |

All deployments will automatically deploy to these **existing resources**.

## Two Essential Commands

### 1. Local Development
```powershell
.\scripts\deploy.ps1 -Mode local
```
Runs the MCP server locally at `http://localhost:5000` for development and testing.

### 2. Azure Deployment
```powershell
.\scripts\deploy.ps1 -Mode azure -Configuration Release
```
Deploys to production at `https://wa-mcpserver-sweden.azurewebsites.net`

---

## Running Locally

**PowerShell:**
```powershell
.\scripts\deploy.ps1 -Mode local
```

The server will start on `http://localhost:5000`.

**For detailed local development instructions, see [DEPLOYMENT.md](DEPLOYMENT.md)**

## Deploying to Azure App Service

**PowerShell:**
```powershell
.\scripts\deploy.ps1 -Mode azure -Configuration Release
```

The deployment script will:
1. Build the project in Release mode (optimized for performance)
2. Publish the application
3. Deploy to the existing Web App: `wa-mcpserver-sweden`
4. Make the application available at: `https://wa-mcpserver-sweden.azurewebsites.net`

**For detailed deployment instructions and advanced options, see [DEPLOYMENT.md](DEPLOYMENT.md)**

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
├── src/                              # Source code
│   ├── MCPServerDemo.csproj         # .NET 8 project file
│   ├── Program.cs                    # Application entry point
│   ├── Tools/                        # MCP tools
│   │   ├── EchoTool.cs
│   │   ├── RandomNumberTool.cs
│   │   ├── ChuckNorrisJokeTool.cs
│   │   ├── AzureStorageTool.cs
│   │   ├── PayslipAnalyzerTool.cs
│   │   └── AzureDocumentIntelligenceTool.cs
│   ├── Models/                       # Data models
│   │   └── McpModels.cs
│   ├── Services/                     # MCP server services
│   │   └── McpService.cs
│   ├── appsettings.json              # Production configuration
│   ├── appsettings.Development.json # Local development configuration
│   └── appsettings.Production.json  # Azure production configuration
├── scripts/                          # Deployment scripts
│   ├── deploy.ps1                   # PowerShell script (Windows)
│   └── deploy.sh                    # Bash script (Linux/macOS)
├── .azure/                           # Azure configuration
│   └── plan.md                      # Detailed deployment plan
├── README.md                         # This file
├── DEPLOYMENT.md                     # 📖 Complete deployment guide
├── DOCUMENT_INTELLIGENCE_SETUP.md   # Document Intelligence configuration
└── PAYSLIP_ANALYZER_SETUP.md        # Payslip analyzer configuration
```

## Documentation

| Document | Purpose |
|----------|---------|
| [QUICKSTART.md](QUICKSTART.md) | ⚡ **Get started in 2 minutes** - Quick commands & examples |
| [DEPLOYMENT.md](DEPLOYMENT.md) | 📖 **Complete deployment guide** - Detailed instructions & troubleshooting |
| [SCRIPTS_REFERENCE.md](SCRIPTS_REFERENCE.md) | 🔧 **Script documentation** - Parameters, examples & CI/CD integration |
| [DOCUMENT_INTELLIGENCE_SETUP.md](DOCUMENT_INTELLIGENCE_SETUP.md) | Azure Document Intelligence credentials & setup |
| [PAYSLIP_ANALYZER_SETUP.md](PAYSLIP_ANALYZER_SETUP.md) | AI Foundry payslip analyzer configuration |
| [.azure/plan.md](.azure/plan.md) | Detailed Azure deployment plan & architecture |

## Requirements

- **.NET 8 SDK** - [Download](https://dotnet.microsoft.com/download)
- **Azure CLI** - [Install](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli) (for Azure deployment)
- **Azure subscription** - For deploying to cloud

## Scripts

### Deployment Scripts
- **Windows**: `.\scripts\deploy.ps1` - PowerShell deployment script
- **Linux/macOS**: `./scripts/deploy.sh` - Bash deployment script

Both scripts support:
- Local execution with `local` mode
- Azure deployment with `azure` mode
- Custom resource groups and Web App names
- Debug and Release configurations

**Usage**: See [DEPLOYMENT.md](DEPLOYMENT.md) for detailed examples

## License

MIT
