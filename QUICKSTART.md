# Quick Start Guide

## 🚀 Get Started in 2 Minutes

### Two Core Commands

Everything you need is in these two commands:

#### 💻 Local Development
```powershell
.\scripts\deploy.ps1 -Mode local
```
Starts the server locally at `http://localhost:5000` for development and testing.

#### ☁️ Deploy to Azure
```powershell
.\scripts\deploy.ps1 -Mode azure -Configuration Release
```
Deploys to production Web App: `wa-mcpserver-sweden`

---

### Run Locally

**PowerShell:**
```powershell
.\scripts\deploy.ps1 -Mode local
```

Application starts at: `http://localhost:5000`

---

### Deploy to Azure

**PowerShell:**
```powershell
.\scripts\deploy.ps1 -Mode azure -Configuration Release
```

The script will:
- Build the project in Release mode (optimized)
- Publish the application
- Upload to existing Web App: `wa-mcpserver-sweden`
- Application will be live at: `https://wa-mcpserver-sweden.azurewebsites.net`

---

## 📋 Deployment Targets

✅ **Already in Place - Ready to Deploy**

- **Web App**: `wa-mcpserver-sweden`
- **Region**: swedencentral
- **Resource Group**: `rg-mcpserverdemo-sweden`

---

## ✅ What's Included

- ✅ Automated build & deployment scripts (PowerShell)
- ✅ Local development support
- ✅ Azure Web App deployment
- ✅ Health check endpoint
- ✅ MCP protocol support
- ✅ 5 built-in tools (Random, Chuck Norris, Storage Search, Payslip Analyzer, Document Extraction)
- ✅ Azure Document Intelligence integration
- ✅ AI Foundry payslip analyzer

---

## 📚 Full Documentation

| Document | Purpose |
|----------|---------|
| [DEPLOYMENT.md](DEPLOYMENT.md) | Complete deployment guide with examples |
| [DOCUMENT_INTELLIGENCE_SETUP.md](DOCUMENT_INTELLIGENCE_SETUP.md) | Document Intelligence credentials |
| [PAYSLIP_ANALYZER_SETUP.md](PAYSLIP_ANALYZER_SETUP.md) | AI Foundry setup |
| [README.md](README.md) | Project overview |

---

## 🔧 Prerequisites

- **.NET 8 SDK** - [Download](https://dotnet.microsoft.com/download)
- **Azure CLI** - [Install](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli) (for Azure deployment)
- **Authenticated with Azure** - Run `az login`

---

## 🧪 Test Your Deployment

### Local Testing
```bash
# Health check
curl http://localhost:5000/health

# Tool list request
curl -X POST http://localhost:5000/mcp \
  -H "Content-Type: application/json" \
  -d '{"jsonrpc":"2.0","id":"1","method":"initialize","params":{"protocolVersion":"2024-11-05","clientInfo":{"name":"test","version":"1.0.0"}}}'
```

### Azure Testing
```bash
# Health check
curl https://wa-mcpserver-sweden.azurewebsites.net/health

# View logs
az webapp log tail --name wa-mcpserver-sweden --resource-group rg-mcpserverdemo-sweden
```

---

## ⚡ Common Commands

### Build Locally
```bash
dotnet build --project src/MCPServerDemo.csproj
```

### Run Locally (Manual)
```bash
cd src
dotnet run
```

### Deploy Without Rebuild
```powershell
# PowerShell
.\scripts\deploy.ps1 -Mode azure -Configuration Release -NoBuild
```

```bash
# Bash
NO_BUILD=true ./scripts/deploy.sh azure Release
```

### View Application Settings
```bash
az webapp config appsettings list \
  --resource-group rg-mcpserverdemo-sweden \
  --name wa-mcpserver-sweden
```

### Restart Web App
```bash
az webapp restart \
  --resource-group rg-mcpserverdemo-sweden \
  --name wa-mcpserver-sweden
```

---

## 🆘 Troubleshooting

**Port 5000 already in use?**
```powershell
# PowerShell - Kill dotnet processes
Get-Process -Name dotnet | Stop-Process -Force
Start-Sleep -Seconds 2
.\scripts\deploy.ps1 -Mode local
```

```bash
# Bash - Kill dotnet processes
pkill -f dotnet
sleep 2
./scripts/deploy.sh local
```

**Azure deployment failed?**
```bash
# Check logs
az webapp log tail --name wa-mcpserver-sweden \
  --resource-group rg-mcpserverdemo-sweden --follow

# Check deployment history
az webapp deployment list --name wa-mcpserver-sweden \
  --resource-group rg-mcpserverdemo-sweden
```

**Not authenticated with Azure?**
```bash
az login
az account set --subscription "YOUR_SUBSCRIPTION_NAME"
```

---

## 📖 Full Documentation Tree

```
.
├── QUICKSTART.md (this file)         ← You are here
├── DEPLOYMENT.md                     ← Full deployment guide
├── README.md                         ← Project overview
├── DOCUMENT_INTELLIGENCE_SETUP.md    ← Credentials setup
├── PAYSLIP_ANALYZER_SETUP.md         ← AI Foundry setup
├── .azure/plan.md                    ← Detailed Azure plan
└── scripts/
    ├── deploy.ps1                    ← Windows PowerShell script
    └── deploy.sh                     ← Linux/macOS Bash script
```

---

## 💡 Tips

1. **Local First**: Always test locally before deploying to Azure
2. **Release Mode**: Use `-Configuration Release` for Azure deployments for better performance
3. **Logs Are Your Friend**: Always check `az webapp log tail` when debugging
4. **Confirm Deployments**: The script asks for confirmation before uploading to Azure
5. **Keep Scripts Executable**: On Linux/macOS, run `chmod +x ./scripts/deploy.sh` first

---

## 🎯 Next Steps

1. **Run locally**: `.\scripts\deploy.ps1 -Mode local`
2. **Test the endpoints** at `http://localhost:5000/health`
3. **Deploy to Azure**: `.\scripts\deploy.ps1 -Mode azure -Configuration Release`
4. **Configure credentials**: See [DOCUMENT_INTELLIGENCE_SETUP.md](DOCUMENT_INTELLIGENCE_SETUP.md)
5. **Monitor logs**: `az webapp log tail --name wa-mcpserver-sweden --resource-group rg-mcpserverdemo-sweden`

---

**Need more details? See [DEPLOYMENT.md](DEPLOYMENT.md)**
