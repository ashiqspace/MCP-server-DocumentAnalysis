#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Deploy and run the MCP Server Demo application locally or to Azure.

.DESCRIPTION
    This script automates building, running locally, and deploying the MCPServerDemo
    application to an existing Azure Web App. The project is located in the src folder.

.PARAMETER Mode
    Operation mode: 'local' (default) or 'azure'
    - local: Build and run locally on http://localhost:5000
    - azure: Build, publish, and deploy to existing Azure Web App

.PARAMETER Configuration
    Build configuration: 'Debug' (default) or 'Release'

.PARAMETER ResourceGroup
    Azure resource group name. Default: 'rg-mcpserverentraauth-sweden'

.PARAMETER WebAppName
    Azure Web App name. Default: 'wa-mcpentraidauth'

.PARAMETER NoBuild
    Skip building if the project has already been built.

.PARAMETER Force
    Skip confirmation prompt for Azure deployment. Automatically proceed with deployment.

.EXAMPLE
    .\scripts\deploy.ps1 -Mode local
    Runs the application locally.

.EXAMPLE
    .\scripts\deploy.ps1 -Mode azure -Configuration Release
    Deploys to Azure using the existing Web App.

.EXAMPLE
    .\scripts\deploy.ps1 -Mode azure -Configuration Release -Force
    Deploys to Azure automatically without confirmation prompt.

.EXAMPLE
    .\scripts\deploy.ps1 -Mode azure -Configuration Release -ResourceGroup mygroup -WebAppName myapp
    Deploys to Azure with custom resource group and Web App names.
#>

param(
    [ValidateSet('local', 'azure')]
    [string]$Mode = 'local',
    
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug',
    
    [string]$ResourceGroup = 'rg-mcpserverdemo-sweden',
    
    [string]$WebAppName = 'wa-mcpserver-sweden',
    
    [switch]$NoBuild,
    
    [switch]$Force
)

$ErrorActionPreference = 'Stop'
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $scriptPath
$srcPath = Join-Path $projectRoot 'src'
$projectFile = Join-Path $srcPath 'MCPServerDemo.csproj'
$publishPath = Join-Path $projectRoot 'publish_azure'

Write-Host "╔════════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  MCP Server Demo - Deployment Script                          ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""
Write-Host "Mode:           $Mode" -ForegroundColor Green
Write-Host "Configuration:  $Configuration" -ForegroundColor Green
Write-Host "Project:        $projectFile" -ForegroundColor Green

if ($Mode -eq 'azure') {
    Write-Host "Resource Group: $ResourceGroup" -ForegroundColor Green
    Write-Host "Web App:        $WebAppName" -ForegroundColor Green
}

Write-Host ""

# Check if project file exists
if (-not (Test-Path $projectFile)) {
    Write-Error "Project file not found: $projectFile"
    exit 1
}

# Function to run dotnet commands with error handling
function Invoke-DotNet {
    param(
        [string[]]$Arguments
    )
    $cmd = "dotnet"
    Write-Host "► Running: dotnet $($Arguments -join ' ')" -ForegroundColor Yellow
    & $cmd @Arguments
    if ($LASTEXITCODE -ne 0) {
        Write-Error "dotnet command failed with exit code $LASTEXITCODE"
        exit $LASTEXITCODE
    }
}

# Build the project
if (-not $NoBuild) {
    Write-Host "📦 Building project..." -ForegroundColor Cyan
    Invoke-DotNet @('build', $projectFile, '-c', $Configuration)
    Write-Host "✓ Build successful!" -ForegroundColor Green
    Write-Host ""
}

# Local run mode
if ($Mode -eq 'local') {
    Write-Host "🚀 Starting application locally..." -ForegroundColor Cyan
    Write-Host "Expected URL: http://localhost:5000" -ForegroundColor Yellow
    Write-Host "Press Ctrl+C to stop the application" -ForegroundColor Yellow
    Write-Host ""
    
    # Set Development environment
    $env:ASPNETCORE_ENVIRONMENT = 'Development'
    Write-Host "Environment: $($env:ASPNETCORE_ENVIRONMENT)" -ForegroundColor Yellow
    
    Invoke-DotNet @('run', '--project', $projectFile, '--no-build', '--no-launch-profile', '-c', $Configuration)
}

# Azure deployment mode
elseif ($Mode -eq 'azure') {
    Write-Host "📤 Preparing for Azure deployment..." -ForegroundColor Cyan
    
    # Clean publish directory
    if (Test-Path $publishPath) {
        Write-Host "Cleaning old publish files..." -ForegroundColor Yellow
        Remove-Item $publishPath -Recurse -Force
    }
    
    # Publish the application
    Write-Host "Publishing application..." -ForegroundColor Yellow
    Invoke-DotNet @('publish', $projectFile, '-c', $Configuration, '-o', $publishPath)
    Write-Host "✓ Publish successful!" -ForegroundColor Green
    Write-Host ""
    
    # Check if Azure CLI is installed
    try {
        $azVersion = az version 2>$null | ConvertFrom-Json
        Write-Host "✓ Azure CLI is installed" -ForegroundColor Green
    }
    catch {
        Write-Error "Azure CLI is not installed or not in PATH. Please install it from https://learn.microsoft.com/en-us/cli/azure/install-azure-cli"
        exit 1
    }
    
    Write-Host "🔐 Verifying Azure authentication..." -ForegroundColor Cyan
    try {
        $account = az account show 2>$null
        if ($LASTEXITCODE -eq 0) {
            $accountInfo = $account | ConvertFrom-Json
            Write-Host "✓ Logged in as: $($accountInfo.user.name)" -ForegroundColor Green
        }
    }
    catch {
        Write-Host "⚠️  Not authenticated. Running 'az login'..." -ForegroundColor Yellow
        az login
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Azure login failed"
            exit 1
        }
    }
    
    Write-Host ""
    Write-Host "📋 Deployment details:" -ForegroundColor Cyan
    Write-Host "  Resource Group: $ResourceGroup"
    Write-Host "  Web App:        $WebAppName"
    Write-Host "  Region:         swedencentral"
    Write-Host "  Configuration:  $Configuration"
    Write-Host ""
    
    # Confirm deployment
    $confirmation = 'yes'
    if (-not $Force) {
        $confirmation = Read-Host "Deploy to Azure Web App '$WebAppName'? (yes/no)"
    }
    if ($confirmation -ne 'yes') {
        Write-Host "Deployment cancelled by user." -ForegroundColor Yellow
        exit 0
    }
    
    Write-Host ""
    Write-Host "🚀 Deploying to Azure..." -ForegroundColor Cyan
    
    try {
        # Zip the publish directory
        $zipPath = Join-Path $projectRoot 'deploy.zip'
        if (Test-Path $zipPath) {
            Remove-Item $zipPath -Force
        }
        
        Write-Host "Creating deployment package..." -ForegroundColor Yellow
        Compress-Archive -Path "$publishPath\*" -DestinationPath $zipPath -Force
        Write-Host "✓ Package created: $zipPath" -ForegroundColor Green
        
        Write-Host "Uploading to Azure Web App..." -ForegroundColor Yellow
        az webapp deployment source config-zip `
            --resource-group $ResourceGroup `
            --name $WebAppName `
            --src $zipPath
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✓ Deployment successful!" -ForegroundColor Green
            Write-Host ""
            Write-Host "Web App URL: https://$WebAppName.azurewebsites.net" -ForegroundColor Green
            Write-Host "Health check: https://$WebAppName.azurewebsites.net/health" -ForegroundColor Green
            Write-Host ""
            Write-Host "📝 Next steps:" -ForegroundColor Cyan
            Write-Host "  1. Check application logs: az webapp log tail --name $WebAppName --resource-group $ResourceGroup" -ForegroundColor Yellow
            Write-Host "  2. View settings: az webapp config appsettings list --name $WebAppName --resource-group $ResourceGroup" -ForegroundColor Yellow
            Write-Host "  3. Configure credentials: See DOCUMENT_INTELLIGENCE_SETUP.md" -ForegroundColor Yellow
        }
        else {
            Write-Error "Azure deployment failed"
            exit 1
        }
    }
    finally {
        # Cleanup
        if (Test-Path $zipPath) {
            Remove-Item $zipPath -Force
        }
    }
}

Write-Host ""
Write-Host "╚════════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
