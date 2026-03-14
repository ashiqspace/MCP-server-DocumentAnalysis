# AI Foundry GPT-5-nano Payslip Analyzer Setup

## ✅ Azure Resources Already In Place

The following Azure resources are **already deployed and ready**:

- **Web App**: `wa-mcpserver-sweden` (swedencentral)
- **Resource Group**: `rg-mcpserverdemo-sweden`

All configurations will be applied to these existing resources. See [DEPLOYMENT.md](DEPLOYMENT.md) for deployment instructions.

---

## Overview

The new `PayslipAnalyzerTool` uses Azure AI Foundry's GPT-5-nano model to analyze and authenticate payslip documents. It can:
- Verify if a document is actually a payslip
- Extract key payslip information (employee ID, pay period, salary, deductions)
- Flag suspicious or invalid documents
- Match payslips to specific employees
- Analyze multiple payslips in batch mode

## Configuration

### Option 1: Azure App Service Settings (Recommended)

Store your AI Foundry credentials in the same place as Document Intelligence:

**Via Azure Portal:**
1. Go to `wa-mcpserver-sweden` → **Configuration**
2. Click **New application setting** and add:

| Name | Value |
|------|-------|
| `AI_FOUNDRY_ENDPOINT` | `https://YOUR_AI_FOUNDRY_RESOURCE.cognitiveservices.azure.com/openai/deployments/gpt-5-nano/chat/completions?api-version=2025-01-01-preview` |
| `AI_FOUNDRY_KEY` | `YOUR_AI_FOUNDRY_KEY` |

3. Click **Save** → **Restart** Web App

**Via Azure CLI:**
```bash
az webapp config appsettings set \
  --resource-group rg-mcpserverdemo-sweden \
  --name wa-mcpserver-sweden \
  --settings AI_FOUNDRY_ENDPOINT="https://YOUR_AI_FOUNDRY_RESOURCE.cognitiveservices.azure.com/openai/deployments/gpt-5-nano/chat/completions?api-version=2025-01-01-preview" \
  AI_FOUNDRY_KEY="YOUR_AI_FOUNDRY_KEY"
```

### Option 2: Local Development (appsettings.Development.json)

For local testing, update [src/appsettings.Development.json](src/appsettings.Development.json):

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "AzureStorage": {
    "ConnectionString": "YOUR_STORAGE_CONNECTION_STRING",
    "ContainerName": "documents"
  },
  "DocumentIntelligence": {
    "Endpoint": "https://YOUR-RESOURCE.cognitiveservices.azure.com/",
    "ApiKey": "YOUR-DOCUMENT-INTELLIGENCE-KEY"
  },
  "AIFoundry": {
    "Endpoint": "https://YOUR_AI_FOUNDRY_RESOURCE.cognitiveservices.azure.com/openai/deployments/gpt-5-nano/chat/completions?api-version=2025-01-01-preview",
    "Key": "YOUR_AI_FOUNDRY_KEY"
  }
}
```

## Available Tools

### 1. **analyzePayslipContent**
Analyzes extracted payslip content to verify it's a valid payslip and extract key information.

**Parameters:**
- `extractedContent` (required): The extracted text from the payslip
- `employeeId` (optional): Employee ID to verify against the payslip
- `employeeName` (optional): Employee name to verify against the payslip

**Example Request:**
```bash
curl -X POST https://wa-mcpserver-sweden-bghbdhh2ereab8ej.swedencentral-01.azurewebsites.net/mcp \
  -H "Content-Type: application/json" \
  -d '{
    "jsonrpc":"2.0",
    "id":1,
    "method":"tools/call",
    "params":{
      "name":"analyzePayslipContent",
      "arguments":{
        "extractedContent":"Employee: John Doe\nEmployee ID: EMP12345\nPay Period: 2026-02-01 to 2026-02-28\nGross Salary: $5,000\nIncome Tax: $750\nHealthcare: $250\nRetirement: $300\nNet Pay: $3,700",
        "employeeId":"EMP12345",
        "employeeName":"John Doe"
      }
    }
  }'
```

**Response Example:**
```
Document Type Verification: Yes, this is a valid payslip
Employee Verification: Matches (John Doe, EMP12345)
Key Information Extracted:
  * Employee Name: John Doe
  * Employee ID: EMP12345
  * Pay Period: 2026-02-01 to 2026-02-28
  * Gross Pay: $5,000
  * Deductions: Income Tax ($750), Healthcare ($250), Retirement ($300)
  * Net Pay: $3,700
Flags & Issues: None
Analysis Confidence: High
Summary: Valid payslip for the correct employee with all expected fields present and no anomalies detected.
```

---

### 2. **validatePayslipDocument**
Quick validation to check if a document is likely a payslip (before extracting content).

**Parameters:**
- `documentIdentifier` (required): Document name or brief content snippet
- `expectedEmployeeId` (optional): Expected employee ID for verification

**Example Request:**
```bash
curl -X POST https://wa-mcpserver-sweden.azurewebsites.net/mcp \
  -H "Content-Type: application/json" \
  -d '{
    "jsonrpc":"2.0",
    "id":1,
    "method":"tools/call",
    "params":{
      "name":"validatePayslipDocument",
      "arguments":{
        "documentIdentifier":"2026-02_Payslip_EMP12345.pdf",
        "expectedEmployeeId":"EMP12345"
      }
    }
  }'
```

---

### 3. **analyzePayslipBatch**
Analyzes multiple payslips for an employee and generates a summary with financial totals and trend analysis.

**Parameters:**
- `employeeId` (required): Employee ID
- `payslipContents` (required): Multiple extracted payslip contents separated by `---PAYSLIP_SEPARATOR---`

**Example Request:**
```bash
curl -X POST https://wa-mcpserver-sweden.azurewebsites.net/mcp \
  -H "Content-Type: application/json" \
  -d '{
    "jsonrpc":"2.0",
    "id":1,
    "method":"tools/call",
    "params":{
      "name":"analyzePayslipBatch",
      "arguments":{
        "employeeId":"EMP12345",
        "payslipContents":"Employee: John Doe\nEmployee ID: EMP12345\nPay Period: 2026-01-01 to 2026-01-31\nGross Salary: $5,000\nNet Pay: $3,700\n---PAYSLIP_SEPARATOR---\nEmployee: John Doe\nEmployee ID: EMP12345\nPay Period: 2026-02-01 to 2026-02-28\nGross Salary: $5,000\nNet Pay: $3,700"
      }
    }
  }'
```

**Response Example:**
```
Employee Verification: Consistent
Payslip Count: 2
Date Range: 2026-01-01 to 2026-02-28
Financial Summary:
  * Total Gross: $10,000
  * Total Deductions: $2,600
  * Total Net: $7,400
  * Average per Period: $5,000
Trends & Anomalies: None detected - consistent income
Issues/Flags: None
Summary: Two valid payslips for employee EMP12345 with consistent income and deduction patterns across both pay periods.
```

## Workflow Integration

### Complete Document Analysis Workflow

1. **Search for files** by employee ID or name:
   ```
   searchFilesByEmployeeId(employeeId)
   ```

2. **Extract content** from PDF payslips:
   - Automatically done during `searchFilesByEmployeeId` using Document Intelligence

3. **Validate** extracted content:
   ```
   validatePayslipDocument(documentName, employeeId)
   ```

4. **Analyze** for authenticity and information extraction:
   ```
   analyzePayslipContent(extractedContent, employeeId, employeeName)
   ```

5. **Batch analysis** for multiple payslips:
   ```
   analyzePayslipBatch(employeeId, payslipContents)
   ```

## Security Considerations

✅ **DO:**
- Store credentials in App Service settings or Azure Key Vault
- Use environment variables in production
- Never commit credentials to git
- Rotate API keys regularly
- Enable Azure Monitor for audit logging

❌ **DON'T:**
- Hardcode credentials in source code
- Share API keys via email
- Use the same key across environments
- Store credentials in appsettings.json

## Troubleshooting

### "AI Foundry not configured" Error
1. Check if `AI_FOUNDRY_ENDPOINT` and `AI_FOUNDRY_KEY` are set in Web App settings
2. Verify the endpoint URL includes the full path with `api-version=2025-01-01-preview`
3. Ensure the API key is correct and not expired
4. Restart the Web App after changing settings

### "API error" Response
- Check Azure logs for detailed error messages
- Verify the model deployment name is `gpt-5-nano`
- Ensure your quota and rate limits aren't exceeded
- Test the endpoint directly with a simple curl request

### Slow Response Times
- GPT-5-nano analysis takes 1-3 seconds per payslip
- Batch analysis scales with the number of payslips
- Consider async processing for large batches

## Related Files

- **Payslip Analyzer**: [src/Tools/PayslipAnalyzerTool.cs](src/Tools/PayslipAnalyzerTool.cs)
- **Storage Search Tool**: [src/Tools/AzureStorageSearchTool.cs](src/Tools/AzureStorageSearchTool.cs)
- **Document Intelligence**: [src/Tools/AzureDocumentIntelligenceTool.cs](src/Tools/AzureDocumentIntelligenceTool.cs)
- **Program Configuration**: [src/Program.cs](src/Program.cs)

