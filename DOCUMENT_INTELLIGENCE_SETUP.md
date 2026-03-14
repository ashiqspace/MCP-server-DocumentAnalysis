# Document Intelligence Credentials - Quick Reference

## Before You Start

You need your own Azure resources. This guide assumes:
- You have created your **Resource Group** (e.g., `my-rg`)
- You have created your **Web App** (e.g., `my-mcpserver`)
- You have created an **Azure Document Intelligence** resource
- Azure CLI is installed and you're authenticated (`az login`)

**Example commands to create these:**
```bash
# Create resource group
az group create --name my-rg --location eastus

# Create App Service Plan
az appservice plan create --name my-plan --resource-group my-rg --sku B1

# Create Web App
az webapp create --resource-group my-rg --plan my-plan --name my-mcpserver --runtime DOTNET:8

# Create Document Intelligence resource
az cognitiveservices account create --name my-di --resource-group my-rg --kind FormRecognizer --sku S0 --location eastus
```

Replace `my-rg` and `my-di` with your actual resource names throughout this guide.

---

## Where to Store Credentials

### ✅ Option 1: Azure Key Vault (Most Secure - RECOMMENDED)

**Pros**: Encrypted at rest, audit logs, role-based access, centralized management  
**Cons**: Requires additional setup

**Steps**:
```bash
# 1. Create Key Vault (replace YOUR VALUES)
az keyvault create --resource-group my-rg \
  --name kv-mymcpserver --location eastus

# 2. Store secrets
az keyvault secret set --vault-name kv-mymcpserver \
  --name DocumentIntelligenceEndpoint --value "https://YOUR-RESOURCE.cognitiveservices.azure.com/"

az keyvault secret set --vault-name kv-mymcpserver \
  --name DocumentIntelligenceKey --value "YOUR-API-KEY"

# 3. Grant Web App access
az keyvault set-policy --name kv-mcpserver-sweden \
  --object-id $(az webapp show --resource-group my-rg \
```bash
# Grant your Web App access to Document Intelligence
WEB_APP_PRINCIPAL_ID=$(az webapp identity assign \
  --resource-group my-rg \
  --name my-mcpserver --query identity.principalId -o tsv) \

az role assignment create \
  --assignee $WEB_APP_PRINCIPAL_ID \
  --role "Cognitive Services User" \
  --scope /subscriptions/YOUR-SUBSCRIPTION-ID/resourceGroups/my-rg/providers/Microsoft.CognitiveServices/accounts/my-di
``` \
  --secret-permissions get list
```

**Update Program.cs**:
```csharp
var keyVaultUrl = new Uri("https://kv-mcpserver-sweden.vault.azure.net/");
builder.Configuration.AddAzureKeyVault(keyVaultUrl, new DefaultAzureCredential());
```

---

### ✅ Option 2: App Service Application Settings (Simple & Good)

**Pros**: Built-in, no additional resources, easy to manage  
**Cons**: Stored as plaintext in service-level config (still encrypted at rest by Azure)

**Steps**:

1. **Via Azure Portal**:
   - Go to `wa-mcpserver-sweden` → **Configuration**
   - Click **New application setting**
   - Add these settings:

   | Name | Value |
   |------|-------|
   | `DOCUMENT_INTELLIGENCE_ENDPOINT` | `https://YOUR-RESOURCE.cognitiveservices.azure.com/` |
   | `DOCUMENT_INTELLIGENCE_KEY` | `YOUR-API-KEY` |

   - Click **Save** → **Restart** Web App

2. **Via Azure CLI**:
   ```bash
   az webapp config appsettings set \
     --resource-group my-rg \
     --name wa-mcpserver-sweden \
     --settings DOCUMENT_INTELLIGENCE_ENDPOINT="https://YOUR-RESOURCE.cognitiveservices.azure.com/" \
     DOCUMENT_INTELLIGENCE_KEY="YOUR-API-KEY"
   ```

**No code changes needed** - already configured in [AzureDocumentIntelligenceTool.cs](src/Tools/AzureDocumentIntelligenceTool.cs)

---

### ❌ Option 3: appsettings.json (NOT Recommended for Secrets)

**Cons**: Secrets hardcoded in source, visible in repo, security risk

**DO NOT** commit credentials to git. Use for non-sensitive config only.

---

## Where to Get Document Intelligence Credentials

### Already Have a Document Intelligence Resource?

1. **Azure Portal** → Search "Document Intelligence"
2. Select your resource
3. Go to **Keys and Endpoint** (left sidebar)
4. Copy:
   - **Endpoint**: `https://YOUR-REGION.api.cognitive.microsoft.com/`
   - **Key 1** or **Key 2**: Your API key

### Need to Create Document Intelligence?

```bash
# Create resource
az cognitiveservices account create \
  --name doc-intelligence-sweden \
  --resource-group rg-mcpserverdemo-sweden \
  --kind DocumentIntelligence \
  --sku S0 \
  --location swedencentral \
  --yes

# Get credentials
az cognitiveservices account show \
  --name doc-intelligence-sweden \
  --resource-group rg-mcpserverdemo-sweden \
  --query properties.endpoint

az cognitiveservices account keys list \
  --name doc-intelligence-sweden \
  --resource-group rg-mcpserverdemo-sweden
```

---

## Verify Credentials are Working

After storing credentials in your Web App settings, test the connection:

```bash
# Test via curl
curl https://wa-mcpserver-sweden.azurewebsites.net/mcp \
  -X POST \
  -H "Content-Type: application/json" \
  -d '{
    "jsonrpc": "2.0",
    "method": "tools/call",
    "params": {
      "name": "extractDocumentData",
      "arguments": {
        "documentPath": "test.pdf"
      }
    },
    "id": 1
  }'
```

Check logs for errors:
```bash
az webapp log tail --resource-group my-rg \
  --name wa-mcpserver-sweden --follow
```

---

## Security Best Practices

✅ **DO**:
- Use Azure Key Vault for sensitive data
- Rotate API keys regularly
- Use app settings for non-sensitive config
- Enable Azure Monitor/Application Insights
- Use Azure AD authentication where possible

❌ **DON'T**:
- Commit secrets to git
- Store credentials in appsettings.json
- Share API keys via email
- Use the same key across environments
- Hardcode endpoints in code

---

## Related Files

- Deployment Plan: [.azure/plan.md](./.azure/plan.md)
- Tool Implementation: [src/Tools/AzureDocumentIntelligenceTool.cs](src/Tools/AzureDocumentIntelligenceTool.cs)
- App Config: [src/Program.cs](src/Program.cs)

