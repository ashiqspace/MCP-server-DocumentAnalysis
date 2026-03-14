# Deployment Scripts Reference

## Overview

The MCP Server Demo includes automated deployment scripts for local execution and Azure deployment.

- **Windows**: `scripts/deploy.ps1` (PowerShell)

---

## Scripts/deploy.ps1 (Windows PowerShell)

### Location
```
scripts/deploy.ps1
```

### Usage

```powershell
.\scripts\deploy.ps1 [OPTIONS]
```

### Parameters

| Parameter | Type | Default | Values | Description |
|-----------|------|---------|--------|-------------|
| `-Mode` | switch | `local` | `local`, `azure` | Operation mode: run locally or deploy to Azure |
| `-Configuration` | switch | `Debug` | `Debug`, `Release` | Build configuration |
| `-ResourceGroup` | string | `rg-mcpserverdemo-sweden` | any | Azure resource group name |
| `-WebAppName` | string | `wa-mcpserver-sweden` | any | Azure Web App name |
| `-NoBuild` | flag | false | (none) | Skip building if already built |

### Examples

```powershell
# Run locally with Debug config (default)
.\scripts\deploy.ps1 -Mode local

# Run locally with Release config
.\scripts\deploy.ps1 -Mode local -Configuration Release

# Deploy to Azure with Release config
.\scripts\deploy.ps1 -Mode azure -Configuration Release

# Deploy without rebuilding
.\scripts\deploy.ps1 -Mode azure -Configuration Release -NoBuild

# Deploy to custom Azure resources
.\scripts\deploy.ps1 -Mode azure -Configuration Release `
  -ResourceGroup "my-rg" -WebAppName "my-webapp"

# Combine multiple options
.\scripts\deploy.ps1 `
  -Mode azure `
  -Configuration Release `
  -ResourceGroup "custom-rg" `
  -WebAppName "custom-app" `
  -NoBuild
```

### What It Does

#### Local Mode (`-Mode local`)
1. Validates that `src/MCPServerDemo.csproj` exists
2. Builds the project (unless `-NoBuild` is specified)
3. Runs the application with `dotnet run`
4. Application available at `http://localhost:5000`
5. Press Ctrl+C to stop

#### Azure Mode (`-Mode azure`)
1. Validates that `src/MCPServerDemo.csproj` exists
2. Builds the project in specified configuration (unless `-NoBuild`)
3. Publishes to `publish_azure/` directory
4. Verifies Azure CLI installation
5. Checks Azure authentication (prompts `az login` if needed)
6. Displays deployment details
7. **Asks user for confirmation** before proceeding
8. Creates ZIP file of published application
9. Uploads ZIP to Azure Web App using deployment API
10. Displays application URL and next steps
11. Cleans up temporary ZIP file

### Output

The script provides colored, structured output:

```
╔════════════════════════════════════════════════════════════════╗
║  MCP Server Demo - Deployment Script                          ║
╚════════════════════════════════════════════════════════════════╝

Mode:           local
Configuration:  Debug
Project:        C:\path\to\src\MCPServerDemo.csproj

📦 Building project...
► Running: dotnet build --project C:\path\to\src\MCPServerDemo.csproj -c Debug
✓ Build successful!

🚀 Starting application locally...
Expected URL: http://localhost:5000
Press Ctrl+C to stop the application
```

### Error Handling

The script exits with error code `1` if:
- Project file doesn't exist
- Build fails
- `dotnet run` fails
- Azure CLI is not installed
- Azure login fails
- Deployment fails
- User cancels deployment

---

## Scripts/deploy.sh (Linux/macOS Bash)

### Location
```
scripts/deploy.sh
```

### Usage

```bash
./scripts/deploy.sh [MODE] [CONFIGURATION]
```

### Parameters (Positional)

| Position | Name | Default | Values | Description |
|----------|------|---------|--------|-------------|
| 1 | MODE | `local` | `local`, `azure` | Operation mode |
| 2 | CONFIGURATION | `Debug` | `Debug`, `Release` | Build configuration |

### Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `RESOURCE_GROUP` | `rg-mcpserverdemo-sweden` | Azure resource group |
| `WEB_APP_NAME` | `wa-mcpserver-sweden` | Azure Web App name |
| `NO_BUILD` | `false` | Skip building if already built |

### Examples

```bash
# Make script executable first
chmod +x ./scripts/deploy.sh

# Run locally with Debug config (default)
./scripts/deploy.sh local

# Run locally with Release config
./scripts/deploy.sh local Release

# Deploy to Azure with Release config
./scripts/deploy.sh azure Release

# Deploy without rebuilding
NO_BUILD=true ./scripts/deploy.sh azure Release

# Deploy to custom Azure resources
RESOURCE_GROUP="my-rg" WEB_APP_NAME="my-webapp" ./scripts/deploy.sh azure Release

# Combine environment variables
RESOURCE_GROUP="custom-rg" \
  WEB_APP_NAME="custom-app" \
  NO_BUILD=true \
  ./scripts/deploy.sh azure Release
```

### What It Does

#### Local Mode
1. Validates that `src/MCPServerDemo.csproj` exists
2. Builds the project (unless `NO_BUILD=true`)
3. Runs the application with `dotnet run`
4. Application available at `http://localhost:5000`
5. Press Ctrl+C to stop

#### Azure Mode
1. Validates that `src/MCPServerDemo.csproj` exists
2. Builds the project in specified configuration (unless `NO_BUILD=true`)
3. Publishes to `publish_azure/` directory
4. Verifies `az` command availability
5. Checks Azure authentication (prompts `az login` if needed)
6. Displays deployment details
7. **Asks user for confirmation** before proceeding
8. Creates ZIP file of published application
9. Uploads ZIP to Azure Web App using deployment API
10. Displays application URL and next steps
11. Cleans up temporary ZIP file

### Output

The script provides colored, structured output:

```
╔════════════════════════════════════════════════════════════════╗
║  MCP Server Demo - Deployment Script                          ║
╚════════════════════════════════════════════════════════════════╝

Mode:           local
Configuration:  Debug
Project:        /path/to/src/MCPServerDemo.csproj

📦 Building project...
► Running: dotnet build --project /path/to/src/MCPServerDemo.csproj -c Debug
✓ Build successful!

🚀 Starting application locally...
Expected URL: http://localhost:5000
Press Ctrl+C to stop the application
```

### Error Handling

The script exits with error code `1` if:
- Invalid MODE or CONFIGURATION provided
- Project file doesn't exist
- Build fails
- `dotnet run` fails
- Azure CLI (`az`) is not found
- Azure login fails
- Deployment fails
- User cancels deployment

---

## Common Workflows

### Development Loop

```powershell
# Terminal 1: Run locally
.\scripts\deploy.ps1 -Mode local -Configuration Debug

# Terminal 2: Make code changes and rebuild
dotnet build --project src/MCPServerDemo.csproj -c Debug
# Application auto-reloads (if using hot reload)
```

### Prepare for Azure Deployment

```powershell
# Build in Release mode
.\scripts\deploy.ps1 -Mode local -Configuration Release

# Test locally to verify Release build works correctly
# Then deploy when satisfied
.\scripts\deploy.ps1 -Mode azure -Configuration Release
```

### Quick Deployment Iteration

```powershell
# First deployment builds everything
.\scripts\deploy.ps1 -Mode azure -Configuration Release

# Subsequent deployments skip build if code hasn't changed
.\scripts\deploy.ps1 -Mode azure -Configuration Release -NoBuild
```

### Cross-Platform Consistency

Both scripts maintain identical behavior:

```bash
# All these are equivalent in terms of what they do:
.\scripts\deploy.ps1 -Mode azure -Configuration Release  # Windows
./scripts/deploy.sh azure Release                        # Linux/macOS
```

---

## Implementation Details

### Directory Structure

```
project/
├── src/
│   ├── MCPServerDemo.csproj
│   ├── Program.cs
│   ├── Tools/
│   ├── appsettings.json
│   └── ...
├── scripts/
│   ├── deploy.ps1
│   └── deploy.sh
├── publish_azure/        # Created during Azure deployment
└── deploy.zip            # Temporary, deleted after upload
```

### Build Artifacts

- **Debug**: `src/bin/Debug/net8.0/`
- **Release**: `src/bin/Release/net8.0/`
- **Publish**: `publish_azure/`

### Azure Deployment Process

1. **Publish**: `dotnet publish -c Release -o publish_azure/`
2. **Zip**: Create `deploy.zip` from `publish_azure/` contents
3. **Upload**: Use `az webapp deployment source config-zip`
4. **Cleanup**: Remove temporary `deploy.zip`

### Authentication

The scripts handle Azure authentication:
- Check if already authenticated with `az account show`
- If not, prompt user to run `az login`
- Verify subscription is correctly selected

---

## Customization

### Change Default Azure Resources

Edit the script defaults:

**PowerShell** (`scripts/deploy.ps1`):
```powershell
[string]$ResourceGroup = 'your-resource-group',
[string]$WebAppName = 'your-web-app-name',
```

**Bash** (`scripts/deploy.sh`):
```bash
RESOURCE_GROUP="${RESOURCE_GROUP:-your-resource-group}"
WEB_APP_NAME="${WEB_APP_NAME:-your-web-app-name}"
```

### Add Additional Build Steps

**PowerShell**: Add to the build section:
```powershell
# Before build
Write-Host "Running tests..."
Invoke-DotNet @('test', '--project', $projectFile, '-c', $Configuration)
```

**Bash**: Add to the build section:
```bash
# Before build
echo "Running tests..."
run_dotnet test --project "$PROJECT_FILE" -c "$CONFIGURATION"
```

### Disable Build Prompts

**PowerShell**: Remove confirmation prompt:
```powershell
# Replace this:
$confirmation = Read-Host "Deploy to Azure Web App '$WebAppName'? (yes/no)"
if ($confirmation -ne 'yes') { exit 0 }

# With this (automatic deployment):
Write-Host "Deploying to Azure Web App '$WebAppName'..."
```

---

## Troubleshooting Scripts

### PowerShell Execution Policy

If you get an execution policy error:

```powershell
# Check current policy
Get-ExecutionPolicy

# Set to allow local scripts
Set-ExecutionPolicy -ExecutionPolicy RemoteS signed -Scope CurrentUser
```

### Bash Permissions

If bash script doesn't execute:

```bash
chmod +x ./scripts/deploy.sh
./scripts/deploy.sh local
```

### Script Not Found

Ensure you're in the project root:

```powershell
# Should be at: C:\Projects\MCP-Server\Demo-mcp-server
Get-Location

# Run script with relative path
.\scripts\deploy.ps1 -Mode local
```

---

## Integration with CI/CD

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
          chmod +x ./scripts/deploy.sh
          ./scripts/deploy.sh azure Release
      - name: Check deployment
        run: |
          az webapp log tail --name wa-mcpserver-sweden \
            --resource-group rg-mcpserverdemo-sweden \
            --timeout 30
```

### Azure DevOps Pipeline Example

```yaml
trigger:
  - main

pool:
  vmImage: 'ubuntu-latest'

steps:
  - checkout: self
  
  - task: UseDotNet@2
    inputs:
      version: '8.x'
  
  - script: |
      chmod +x ./scripts/deploy.sh
      ./scripts/deploy.sh azure Release
    displayName: 'Deploy to Azure'
    env:
      RESOURCE_GROUP: rg-mcpserverdemo-sweden
      WEB_APP_NAME: wa-mcpserver-sweden
```

---

## Related Documentation

- [DEPLOYMENT.md](../DEPLOYMENT.md) - Complete deployment guide
- [QUICKSTART.md](../QUICKSTART.md) - Quick start examples
- [README.md](../README.md) - Project overview
- [.azure/plan.md](../.azure/plan.md) - Azure deployment plan
