# Azure Deployment Plan - MCP Server (Document Analysis & Payslip Analyzer)

**Status**: `Ready for Execution`  
**Last Updated**: 2026-02-28  
**Mode**: DEPLOY TO EXISTING WEB APP

---

## ✅ Azure Resources Already Deployed

The following resources **already exist** and are **ready for deployment**:

| Resource | Name | Region | Status |
|----------|------|--------|--------|
| **Web App** | `wa-mcpserver-sweden` | swedencentral | ✅ Active |
| **Resource Group** | `rg-mcpserverdemo-sweden` | swedencentral | ✅ Active |

**All deployments will target these existing resources.**

---

## 1. Executive Summary

Deploy the Document Analysis & Payslip Analyzer MCP Server (.NET 8.0 ASP.NET Core application) to an **existing Azure Web App** (`wa-mcpserver-sweden` in `rg-mcpserverdemo-sweden`).

### Deployment Details
- **App Name**: Document Analysis & Payslip Analyzer MCP Server
- **Runtime**: .NET 8.0
- **Framework**: ASP.NET Core
- **Target Service**: Azure Web App (Linux or Windows)
- **Configuration Method**: App Service Application Settings
- **Secrets Storage**: Azure Key Vault (recommended) or App Service Settings

---

## 2. Application Analysis

### Components
- **Type**: Web API (MCP Protocol Server)
- **Port**: 5000 (HTTP)
- **Framework**: ASP.NET Core with ModelContextProtocol
- **Key Tools**:
  - DocumentExtractionService
  - PayslipAnalyzerTool
  - RandomNumberTool
  - ChuckNorrisJokeTool
  - AzureStorageSearchTool
  - RandomNumberTool
  - ChuckNorrisJokeTool
  - AzureStorageTool
  - AzureStorageSearchTool
  - AzureDocumentIntelligenceTool (requires configuration)

### Dependencies
- Azure.Storage.Blobs (v12.20.0)
- Azure.Identity (v11.4)
- Azure.AI.DocumentIntelligence (v1.0.0)
- ModelContextProtocol.AspNetCore (v0.4.0-preview.3)

### Configuration Requirements
- **Document Intelligence** settings required:
  - `DOCUMENT_INTELLIGENCE_ENDPOINT` - Azure Document Intelligence endpoint URL
  - `DOCUMENT_INTELLIGENCE_KEY` - Azure Document Intelligence API key

---

## 3. Deployment Strategy

### Approach: Direct Publish to Existing Web App

Since the Web App resources are already created, we'll:
1. Build the application in Release mode
2. Publish directly to the Web App using VS Code or Azure CLI
3. Configure app settings in the Azure Portal or via CLI
4. Set up secrets in Azure Key Vault
5. Verify deployment with health checks

### Deployment Methods (Choose One)

#### Option A: VS Code Azure App Service Extension (Easiest)
- Install Azure App Service extension in VS Code
- Right-click project → Deploy to Web App
- Select existing Web App: `wa-mcpserver-sweden`
- Confirm deployment

#### Option B: Azure CLI (Recommended for CI/CD)
```bash
az webapp up --resource-group rg-mcpserverdemo-sweden \
  --name wa-mcpserver-sweden \
  --runtime "DOTNETCORE|8.0" \
  --os-type linux
```

#### Option C: Visual Studio Publish
- File → Publish
- Select Azure App Service
- Choose existing `wa-mcpserver-sweden`
- Publish

---

## 4. Configuration: Document Intelligence Credentials

### Option A: Azure Key Vault (Most Secure - Recommended)

#### Step 1: Create a Key Vault (if not exists)
```bash
az keyvault create --resource-group rg-mcpserverdemo-sweden \
  --name kv-mcpserver-sweden \
  --location swedencentral
```

#### Step 2: Store the Secrets
```bash
# Store Document Intelligence Endpoint
az keyvault secret set --vault-name kv-mcpserver-sweden \
  --name DocumentIntelligenceEndpoint \
  --value "https://<your-resource>.cognitiveservices.azure.com/"

# Store Document Intelligence Key
az keyvault secret set --vault-name kv-mcpserver-sweden \
  --name DocumentIntelligenceKey \
  --value "<your-api-key>"
```

#### Step 3: Grant Web App Access to Key Vault
```bash
# Get Web App managed identity
resourceId=$(az webapp show --resource-group rg-mcpserverdemo-sweden \
  --name wa-mcpserver-sweden --query id -o tsv)

# Assign Key Vault access
az keyvault set-policy --name kv-mcpserver-sweden \
  --object-id $(az webapp show --resource-group rg-mcpserverdemo-sweden \
  --name wa-mcpserver-sweden --query identity.principalId -o tsv) \
  --secret-permissions get list
```

#### Step 4: Update Program.cs (Code Change Required)
Modify [src/Program.cs](../src/Program.cs) to load secrets from Key Vault:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add Key Vault
var keyVaultUrl = new Uri($"https://kv-mcpserver-sweden.vault.azure.net/");
builder.Configuration.AddAzureKeyVault(
    keyVaultUrl,
    new DefaultAzureCredential());

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

var app = builder.Build();
app.MapMcp("/mcp");
app.Run();
```

---

### Option B: App Service Application Settings (Simpler)

#### Step 1: Deploy the Web App
```bash
az webapp up --resource-group rg-mcpserverdemo-sweden \
  --name wa-mcpserver-sweden \
  --runtime "DOTNETCORE|8.0"
```

#### Step 2: Set Application Settings in Azure Portal
1. Go to **Azure Portal** → Search for `wa-mcpserver-sweden`
2. Navigate to **Settings** → **Configuration**
3. Click **New application setting** and add:

| Name | Value |
|------|-------|
| `DOCUMENT_INTELLIGENCE_ENDPOINT` | `https://<your-resource>.cognitiveservices.azure.com/` |
| `DOCUMENT_INTELLIGENCE_KEY` | `<your-api-key>` |
| `ASPNETCORE_ENVIRONMENT` | `Production` |

4. Click **Save** and confirm restart

#### Step 3: Code is Already Compatible
The existing code in [src/Tools/AzureDocumentIntelligenceTool.cs](../src/Tools/AzureDocumentIntelligenceTool.cs) already reads from environment variables:

```csharp
var endpoint = new Uri(Environment.GetEnvironmentVariable("DOCUMENT_INTELLIGENCE_ENDPOINT") ?? "");
var credential = new AzureKeyCredential(Environment.GetEnvironmentVariable("DOCUMENT_INTELLIGENCE_KEY") ?? "");
```

No code changes needed!

---

## 5. Step-by-Step Deployment Instructions

### Deployment Steps

#### Step 1: Build the Application
```powershell
cd C:\Projects\MCP-Server\Demo-mcp-server
$env:ASPNETCORE_ENVIRONMENT = "Production"
dotnet publish -c Release -o ./publish
```

#### Step 2: Deploy to Web App (Choose Method)

**Method A - Using Azure CLI:**
```bash
az webapp deployment source config-zip \
  --resource-group rg-mcpserverdemo-sweden \
  --name wa-mcpserver-sweden \
  --src ./publish.zip
```

**Method B - Using VS Code (Easiest):**
1. Install Azure App Service extension
2. Sign in to Azure
3. Right-click on **Web App** → **Deploy to Web App**
4. Select `wa-mcpserver-sweden`
5. Click **Deploy**

#### Step 3: Configure Application Settings

**In Azure Portal:**
1. Navigate to `wa-mcpserver-sweden` → **Configuration**
2. Add the following settings:

```
DOCUMENT_INTELLIGENCE_ENDPOINT = https://<your-resource>.cognitiveservices.azure.com/
DOCUMENT_INTELLIGENCE_KEY = <your-api-key>
ASPNETCORE_ENVIRONMENT = Production
```

3. Click **Save**
4. Restart the Web App (Configuration → Restart)

#### Step 4: Verify Deployment

```bash
# Get the Web App URL
az webapp show --resource-group rg-mcpserverdemo-sweden \
  --name wa-mcpserver-sweden --query defaultHostName -o tsv

# Test the MCP endpoint
curl https://<web-app-url>/mcp -X POST \
  -H "Content-Type: application/json" \
  -d '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2024-11-05","capabilities":{},"clientInfo":{"name":"test","version":"1.0"}}}'
```

---

## 6. Where to Find Document Intelligence Credentials

### If You Already Have Document Intelligence Resource

1. **Azure Portal**: Search for your Document Intelligence resource
2. Navigate to **Keys and Endpoint** (left sidebar)
3. Copy:
   - **Endpoint URL**: Format is `https://<region>.api.cognitive.microsoft.com/`
   - **Key 1** or **Key 2**: Any of the displayed keys

### If You Need to Create Document Intelligence Resource

```bash
# Create Document Intelligence resource
az cognitiveservices account create \
  --name doc-intelligence-sweden \
  --resource-group rg-mcpserverdemo-sweden \
  --kind DocumentIntelligence \
  --sku S0 \
  --location swedencentral \
  --yes

# Get credentials
az cognitiveservices account keys list \
  --name doc-intelligence-sweden \
  --resource-group rg-mcpserverdemo-sweden

az cognitiveservices account show \
  --name doc-intelligence-sweden \
  --resource-group rg-mcpserverdemo-sweden \
  --query properties.endpoint
```

---

## 7. Environment-Specific Configuration

### Development (Local)
- **Setting**: `ASPNETCORE_ENVIRONMENT = "Development"`
- **Credentials**: Store in `appsettings.Development.json` (local only)

### Production (Azure)
- **Setting**: `ASPNETCORE_ENVIRONMENT = "Production"`
- **Credentials**: Use App Service Settings or Key Vault (NEVER commit to repo)

**Current appsettings.Production.json location**: [src/appsettings.Production.json](../src/appsettings.Production.json)

---

## 8. Validation Checklist

- [ ] Application builds successfully in Release mode
- [ ] Web App `wa-mcpserver-sweden` exists and is running
- [ ] Document Intelligence resource created or identified
- [ ] Document Intelligence endpoint and key obtained
- [ ] Application settings configured in Web App
- [ ] Web App restarted after configuration
- [ ] MCP endpoint `/mcp` responds to requests
- [ ] All tools are accessible and functional

---

## 9. Post-Deployment

### Monitor the Deployment
```bash
# Stream application logs
az webapp log tail --resource-group rg-mcpserverdemo-sweden \
  --name wa-mcpserver-sweden --follow
```

### Test the Endpoints
- Health check: `https://<web-app-url>/`
- MCP endpoint: `https://<web-app-url>/mcp`

### Troubleshooting
- Check **Application Insights** (if enabled)
- Review **Log Stream** in Azure Portal
- Check **Configuration** for missing settings

---

## 10. Next Steps

1. **Execute Deployment** (follow Step 5)
2. **Configure Credentials** (follow Step 4)
3. **Validate Endpoints** (follow Step 8)
4. **Monitor Logs** (follow Post-Deployment)

