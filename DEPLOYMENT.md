# MCP Server Demo - Deployment Guide

## Overview

The MCP Server Demo is fully configured for deployment to **existing Azure resources**. This guide covers running the application locally and deploying to Azure.

### Azure Resources Status

⚠️ **You Need Your Own Azure Resources**

This project is configured to deploy to **your own Azure resources**. You must create:

| Resource | What to Create | Example Name |
|----------|-----------|--------|
| **Resource Group** | Create new or use existing | `my-rg` |
| **Web App** | Create in App Service Plan | `my-mcpserver` |
| **Region** | Choose your region | `eastus`, `westeurope`, etc. |

**Before deploying**, ensure you have:
1. An Azure subscription
2. Azure CLI installed and authenticated (`az login`)
3. Created your resource group: `az group create --name YOUR-RG --location YOUR-REGION`
4. Created your App Service Plan and Web App

**Then deploy using your own names:**
```powershell
.\scripts\deploy.ps1 -Mode azure -Configuration Release `
  -ResourceGroup YOUR-RESOURCE-GROUP -WebAppName YOUR-WEB-APP-NAME
```

---

## Quick Start

### 🔧 Two Essential Commands

**Important:** Replace `YOUR-RESOURCE-GROUP` and `YOUR-WEB-APP-NAME` with your own Azure resources.

This project provides two main deployment commands:

#### 1. Local Development
```powershell
.\scripts\deploy.ps1 -Mode local
```
**Use this to:** Run the MCP server locally on your machine for development and testing.
- Builds the project
- Starts the server at `http://localhost:5000`
- Perfect for debugging and local development

#### 2. Azure Deployment
```powershell
.\scripts\deploy.ps1 -Mode azure -Configuration Release `
  -ResourceGroup YOUR-RESOURCE-GROUP -WebAppName YOUR-WEB-APP-NAME
```
**Use this to:** Deploy to your Azure Web App.
- Builds in Release mode (optimized)
- Publishes the application
- Deploys to your pre-configured Azure Web App
- Makes the application live at: `https://YOUR-WEB-APP-NAME.azurewebsites.net`

---

### Run Locally (Windows PowerShell)

Basic local run (Debug mode):

```powershell
.\scripts\deploy.ps1 -Mode local
```

Local run with Release configuration:

```powershell
.\scripts\deploy.ps1 -Mode local -Configuration Release
```



### Deploy to Azure (Windows PowerShell)

```powershell
.\scripts\deploy.ps1 -Mode azure -Configuration Release
```



---

## Detailed Usage

### PowerShell Script (`scripts/deploy.ps1`)

**Parameters:**

| Parameter | Values | Default | Description |
|-----------|--------|---------|-------------|
| `-Mode` | `local`, `azure` | `local` | Operation mode |
| `-Configuration` | `Debug`, `Release` | `Debug` | Build configuration |
| `-ResourceGroup` | text | `rg-mcpserverdemo-sweden` | Azure resource group |
| `-WebAppName` | text | `wa-mcpserver-sweden` | Azure Web App name |
| `-NoBuild` | flag | false | Skip building if already built |

**Examples:**

```powershell
# Run locally with Debug
.\scripts\deploy.ps1 -Mode local

# Deploy to Azure with custom resources
.\scripts\deploy.ps1 -Mode azure -Configuration Release `
  -ResourceGroup my-rg -WebAppName my-mcpserver

# Deploy without rebuilding
.\scripts\deploy.ps1 -Mode azure -Configuration Release `
  -ResourceGroup my-rg -WebAppName my-mcpserver -NoBuild
```



---

## Project Structure

```
Demo-mcp-server/
├── src/                              # Source code
│   ├── MCPServerDemo.csproj         # Main project file
│   ├── Program.cs                    # Application entry point
│   ├── Tools/                        # MCP tools
│   │   ├── RandomNumberTool.cs
│   │   ├── ChuckNorrisJokeTool.cs
│   │   ├── AzureStorageSearchTool.cs
│   │   └── PayslipAnalyzerTool.cs
│   ├── appsettings.json              # Production settings
│   ├── appsettings.Development.json # Local development settings
│   └── appsettings.Production.json  # Azure production settings
├── scripts/                          # Deployment scripts
│   └── deploy.ps1                   # PowerShell deployment script
├── .azure/                           # Azure configuration
│   └── plan.md                      # Deployment plan
├── README.md                         # Project overview
├── DOCUMENT_INTELLIGENCE_SETUP.md   # Document Intelligence configuration
└── PAYSLIP_ANALYZER_SETUP.md        # Payslip analyzer configuration
```

---

## Local Development

### Prerequisites

- .NET 8 SDK or later
- PowerShell 7+ (Windows) or bash (Linux/macOS)

### Running Locally

**PowerShell:**
```powershell
.\scripts\deploy.ps1 -Mode local
```

The application will start on `http://localhost:5000`

4. Test the health endpoint:
   ```bash
   curl http://localhost:5000/health
   ```

5. Test the MCP endpoint with:
   ```bash
   curl -X POST http://localhost:5000/mcp \
     -H "Content-Type: application/json" \
     -d '{"jsonrpc":"2.0","id":"1","method":"initialize","params":{"protocolVersion":"2024-11-05","clientInfo":{"name":"test","version":"1.0.0"}}}'
   ```

---

## Azure Deployment

### Prerequisites

- Azure CLI (https://learn.microsoft.com/en-us/cli/azure/install-azure-cli)
- Azure subscription
- Authenticated with `az login`

### Deployment Process

1. **Build & Publish**
   - Script builds the application in Release mode
   - Creates a publish package in `publish_azure/`

2. **Package & Upload**
   - Creates a ZIP file of the published application
   - Uploads to Azure Web App using deployment API

3. **Deployment Confirmation**
   - Script asks for confirmation before uploading
   - Provides Web App URL and next steps

### Step-by-Step Deployment

**Windows PowerShell:**
```powershell
# Deploy with your resource group and web app name
.\scripts\deploy.ps1 -Mode azure -Configuration Release `
  -ResourceGroup my-rg `
  -WebAppName my-mcpserver

# Answer 'yes' to confirm deployment
# Monitor deployment in Azure Portal or with:
az webapp log tail --name my-mcpserver --resource-group my-rg
```

### Verify Deployment

After deployment, verify the application:

```bash
# Check Web App status (replace with YOUR values)
az webapp show --name my-mcpserver `
  --resource-group my-rg `
  --query "{state:state, defaultHostName:defaultHostName}"

# View application logs
az webapp log tail --name my-mcpserver `
  --resource-group my-rg

# Test the application (replace with YOUR URL)
curl https://my-mcpserver.azurewebsites.net/health

# Test the MCP endpoint
curl -X POST https://my-mcpserver.azurewebsites.net/mcp `
  -H "Content-Type: application/json" `
  -d '{"jsonrpc":"2.0","id":"1","method":"initialize","params":{"protocolVersion":"2024-11-05","clientInfo":{"name":"test","version":"1.0.0"}}}'
```

---

## Configuration

### Local Development

Edit [src/appsettings.Development.json](src/appsettings.Development.json) to configure local settings:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "DocumentIntelligence": {
    "Endpoint": "https://YOUR-RESOURCE.cognitiveservices.azure.com/",
    "ApiKey": "YOUR-API-KEY"
  },
  "AIFoundry": {
    "Endpoint": "YOUR-AI-FOUNDRY-ENDPOINT",
    "Key": "YOUR-AI-FOUNDRY-KEY"
  }
}
```

### Azure Deployment

Configure Azure settings in two ways:

### Azure Portal
1. Go to your Web App (e.g., `my-mcpserver`) → Configuration
2. Click "New application setting"
3. Add your settings

### Azure CLI
```bash
az webapp config appsettings set \
  --resource-group my-rg \
  --name my-mcpserver \
  --settings \
    DOCUMENT_INTELLIGENCE_ENDPOINT="https://YOUR-RESOURCE.cognitiveservices.azure.com/" \
    DOCUMENT_INTELLIGENCE_KEY="YOUR-API-KEY"
```

For detailed configuration instructions, see:
- [DOCUMENT_INTELLIGENCE_SETUP.md](DOCUMENT_INTELLIGENCE_SETUP.md)
- [PAYSLIP_ANALYZER_SETUP.md](PAYSLIP_ANALYZER_SETUP.md)

---

## Troubleshooting

### Local Execution Issues

**"dotnet: command not found"**
- Install .NET SDK from https://dotnet.microsoft.com/download

**"Port 5000 already in use"**
- PowerShell: Kill existing process
  ```powershell
  Get-Process | Where-Object { $_.ProcessName -eq 'dotnet' } | Stop-Process -Force
  Start-Sleep -Seconds 2
  .\scripts\deploy.ps1 -Mode local
  ```

**"Build failed"**
- Clean and rebuild:
  ```bash
  dotnet clean --project src/MCPServerDemo.csproj
  .\scripts\deploy.ps1 -Mode local -Configuration Release
  ```

### Azure Deployment Issues

**"Azure CLI not found"**
- Install Azure CLI from https://learn.microsoft.com/en-us/cli/azure/install-azure-cli
- Verify: `az --version`

**"Not authenticated"**
- Run: `az login`
- Select your subscription: `az account set --subscription "SUBSCRIPTION_NAME"`

**"Deployment failed"**
- Check Web App logs (replace with your values):
  ```bash
  az webapp log tail --name my-mcpserver `
    --resource-group my-rg
  ```
- Review deployment history:
  ```bash
  az webapp deployment list --name my-mcpserver `
    --resource-group my-rg
  ```

**"Web App not found"**
- Verify resource group and name exist:
  ```bash
  az webapp list --resource-group my-rg
  ```
- Use correct names in deployment script parameters

---

## Performance Considerations

### Local Development
- Use `Debug` configuration for faster iteration
- Application starts on `http://localhost:5000`
- Full logging and debugging available

### Azure Deployment
- Use `Release` configuration for production
- Smaller binary size and better performance
- Automatic scaling available (if configured)
- Monitor with Application Insights

---

## CI/CD Integration

### GitHub Actions Example

```yaml
name: Deploy to Azure

on:
  push:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: azure/login@v1
        with:
          creds: ${{ secrets.AZURE_CREDENTIALS }}
      - name: Deploy
        run: |
          ./scripts/deploy.sh azure Release
        env:
          RESOURCE_GROUP: rg-mcpserverdemo-sweden
          WEB_APP_NAME: wa-mcpserver-sweden
```

---

## Support

For issues or questions:
1. Check the troubleshooting section above
2. Review Azure Portal deployment history
3. Check application logs with `az webapp log tail`
4. See [DOCUMENT_INTELLIGENCE_SETUP.md](DOCUMENT_INTELLIGENCE_SETUP.md) for configuration help

---

## Related Documentation

- [README.md](README.md) - Project overview
- [DOCUMENT_INTELLIGENCE_SETUP.md](DOCUMENT_INTELLIGENCE_SETUP.md) - Document Intelligence setup
- [PAYSLIP_ANALYZER_SETUP.md](PAYSLIP_ANALYZER_SETUP.md) - Payslip analyzer setup
- [.azure/plan.md](.azure/plan.md) - Azure deployment plan
