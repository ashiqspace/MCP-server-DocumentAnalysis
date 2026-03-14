# MCP Server Demo - Deployment Guide

## Overview

The MCP Server Demo is fully configured for deployment to **existing Azure resources**. This guide covers running the application locally and deploying to Azure.

### Azure Resources Status

✅ **Already Deployed and Ready**

| Resource | Name | Region | Status |
|----------|------|--------|--------|
| Web App | `wa-mcpserver-sweden` | swedencentral | ✅ Active |
| Resource Group | `rg-mcpserverdemo-sweden` | swedencentral | ✅ Active |

**All deployments will be to these existing resources.**

---

## Quick Start

### 🔧 Two Essential Commands

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
.\scripts\deploy.ps1 -Mode azure -Configuration Release
```
**Use this to:** Deploy to the Azure Web App (`wa-mcpserver-sweden`).
- Builds in Release mode (optimized)
- Publishes the application
- Deploys to the pre-configured Azure Web App
- Makes the application live at the production URL

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

# Deploy to Azure with Release config
.\scripts\deploy.ps1 -Mode azure -Configuration Release

# Deploy without rebuilding
.\scripts\deploy.ps1 -Mode azure -Configuration Release -NoBuild

# Deploy to custom Azure resources
.\scripts\deploy.ps1 -Mode azure -Configuration Release `
  -ResourceGroup "my-rg" -WebAppName "my-webapp"
```



---

## Project Structure

```
Demo-mcp-server/
├── src/                              # Source code
│   ├── MCPServerDemo.csproj         # Main project file
│   ├── Program.cs                    # Application entry point
│   ├── Tools/                        # MCP tools
│   │   ├── EchoTool.cs
│   │   ├── RandomNumberTool.cs
│   │   ├── ChuckNorrisJokeTool.cs
│   │   ├── AzureStorageTool.cs
│   │   ├── PayslipAnalyzerTool.cs
│   │   └── AzureDocumentIntelligenceTool.cs
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

**PowerShell:**
```powershell
# Deploy with confirmation prompt
.\scripts\deploy.ps1 -Mode azure -Configuration Release

# Answer 'yes' to confirm deployment
# Monitor deployment in Azure Portal or with:
az webapp log tail --name wa-mcpserver-sweden --resource-group rg-mcpserverdemo-sweden
```

### Verify Deployment

After deployment, verify the application:

```bash
# Check Web App status
az webapp show --name wa-mcpserver-sweden \
  --resource-group rg-mcpserverdemo-sweden \
  --query "{state:state, defaultHostName:defaultHostName}"

# View application logs
az webapp log tail --name wa-mcpserver-sweden \
  --resource-group rg-mcpserverdemo-sweden

# Test the application
curl https://wa-mcpserver-sweden.azurewebsites.net/health

# Test the MCP endpoint
curl -X POST https://wa-mcpserver-sweden.azurewebsites.net/mcp \
  -H "Content-Type: application/json" \
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
  }
}
```

### Azure Deployment

Configure Azure settings in two ways:

#### Option 1: Azure Portal
1. Go to `wa-mcpserver-sweden` → Configuration
2. Click "New application setting"
3. Add your settings

#### Option 2: Azure CLI
```bash
az webapp config appsettings set \
  --resource-group rg-mcpserverdemo-sweden \
  --name wa-mcpserver-sweden \
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
- Bash: Kill existing process
  ```bash
  pkill -f dotnet
  sleep 2
  ./scripts/deploy.sh local
  ```

**"Build failed"**
- Clean and rebuild:
  ```bash
  dotnet clean --project src/MCPServerDemo.csproj
  .\scripts\deploy.ps1 -Mode local -configuration Release
  ```

### Azure Deployment Issues

**"Azure CLI not found"**
- Install Azure CLI from https://learn.microsoft.com/en-us/cli/azure/install-azure-cli
- Verify: `az --version`

**"Not authenticated"**
- Run: `az login`
- Select your subscription: `az account set --subscription "SUBSCRIPTION_NAME"`

**"Deployment failed"**
- Check Web App logs:
  ```bash
  az webapp log tail --name wa-mcpserver-sweden \
    --resource-group rg-mcpserverdemo-sweden
  ```
- Review deployment history:
  ```bash
  az webapp deployment list --name wa-mcpserver-sweden \
    --resource-group rg-mcpserverdemo-sweden
  ```

**"Web App not found"**
- Verify resource group and name:
  ```bash
  az webapp list --resource-group rg-mcpserverdemo-sweden
  ```
- Use correct names in deployment script parameter

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
