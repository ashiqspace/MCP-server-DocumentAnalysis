# Echo MCP Server - Deployment Guide

## Prerequisites

- Azure CLI installed
- Azure subscription
- .NET 8 SDK

## Deployment Steps

### 1. Login to Azure

```bash
az login
az account set --subscription "<your-subscription-id>"
```

### 2. Deploy Infrastructure

Deploy to a new resource group:

```bash
az deployment sub create \
  --location eastus \
  --template-file infra/main.deploy.bicep \
  --parameters resourceGroupName=rg-echo-mcp-server location=eastus sku=B1
```

Or deploy to an existing resource group:

```bash
az deployment group create \
  --resource-group <your-resource-group> \
  --template-file infra/main.bicep \
  --parameters sku=B1
```

### 3. Build and Publish the Application

```bash
dotnet publish -c Release -o ./publish
```

### 4. Deploy the Application

#### Option A: Using Azure CLI

```bash
# Get the Web App name from the deployment output
$webAppName = az deployment group show \
  --resource-group <your-resource-group> \
  --name mainDeployment \
  --query properties.outputs.webAppName.value \
  --output tsv

# Deploy the application
az webapp deploy \
  --resource-group <your-resource-group> \
  --name $webAppName \
  --src-path ./publish.zip \
  --type zip
```

#### Option B: Using Visual Studio

1. Right-click the project in Solution Explorer
2. Select "Publish"
3. Choose "Azure" as the target
4. Select your subscription and the created App Service
5. Click "Publish"

#### Option C: Using GitHub Actions

See `.github/workflows/azure-deploy.yml` for CI/CD setup.

### 5. Verify Deployment

Visit your application:

```bash
$hostname = az deployment group show \
  --resource-group <your-resource-group> \
  --name mainDeployment \
  --query properties.outputs.webAppHostName.value \
  --output tsv

echo "https://$hostname/mcp"
```

## Configuration

### App Settings

You can add application settings in the Azure Portal:

1. Navigate to your App Service
2. Go to "Configuration" -> "Application settings"
3. Add any required environment variables

Or use Azure CLI:

```bash
az webapp config appsettings set \
  --resource-group <your-resource-group> \
  --name $webAppName \
  --settings SETTING_NAME="value"
```

## Scaling

To scale your App Service:

```bash
az appservice plan update \
  --resource-group <your-resource-group> \
  --name <app-service-plan-name> \
  --sku P1v3
```

## Monitoring

View logs in real-time:

```bash
az webapp log tail \
  --resource-group <your-resource-group> \
  --name $webAppName
```

## Clean Up

To delete all resources:

```bash
az group delete --name rg-echo-mcp-server --yes --no-wait
```
