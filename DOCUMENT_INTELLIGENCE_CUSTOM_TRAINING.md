# Azure Document Intelligence Custom Model Training for Payslip Authenticity

## Overview

This guide walks you through training a custom Azure Document Intelligence model to validate payslip authenticity. Custom trained models learn patterns from your labeled training data and can recognize document structure, fields, and authenticity indicators.

---

## 1. Prerequisites

### Required Azure Resources
- **Azure subscription** with access to Document Intelligence
- **Azure Storage Account** (Standard or Premium tier) for training data
- **Document Intelligence resource** (pricing tier: S0 or higher for custom models)
- **Azure Key Vault** (optional, for credential management)

### Required Tools & SDKs
```powershell
# Install CLI tools
az extension add -n cognitiveservices

# Install SDK (for C# integration with MCP Server)
dotnet add package Azure.AI.DocumentIntelligence --version 1.0+
```

### Access & Credentials
- Document Intelligence **endpoint URL**
- Document Intelligence **API key**
- Storage account **connection string** or **access key**

---

## 2. Training Data Preparation

### Dataset Structure

Create the following folder structure in your Azure Storage Account:

```
training-dataset/
├── sample-documents/
│   ├── authentic/
│   │   ├── payslip_001.pdf
│   │   ├── payslip_002.pdf
│   │   └── payslip_003.pdf
│   └── fraudulent/
│       ├── fake_payslip_001.pdf
│       ├── fake_payslip_002.pdf
│       └── fake_payslip_003.pdf
├── labels/
│   ├── payslip_001.labels.json
│   ├── payslip_002.labels.json
│   ├── fake_payslip_001.labels.json
│   └── fake_payslip_002.labels.json
└── manifest.jsonl
```

### Minimum Training Data Requirements

| Aspect | Requirement |
|--------|-------------|
| Total Documents | Minimum 50 documents (recommended 100+) |
| Authentic Payslips | ~70% of dataset (70+ documents) |
| Fraudulent Payslips | ~30% of dataset (30+ documents) |
| Per Class | Minimum 5 samples per document type/class |
| Document Quality | Clear, legible PDFs (min 200 DPI) |
| Document Size | 50 KB - 50 MB per document |

### Label File Format (labels.json)

For each training document, create a corresponding `.labels.json` file:

```json
{
  "document": "payslip_001.pdf",
  "docType": "payslip",
  "docTypeConfidence": 0.95,
  "authenticityClass": "genuine",
  "fields": [
    {
      "fieldName": "employee_name",
      "value": "Mohamed Ashiq Faleel",
      "boundingBoxes": [
        {
          "page": 1,
          "x": 0.4,
          "y": 0.15,
          "width": 0.3,
          "height": 0.05
        }
      ]
    },
    {
      "fieldName": "gross_salary",
      "value": "13600",
      "boundingBoxes": [
        {
          "page": 1,
          "x": 0.6,
          "y": 0.3,
          "width": 0.15,
          "height": 0.04
        }
      ]
    },
    {
      "fieldName": "deductions_total",
      "value": "2100",
      "boundingBoxes": [
        {
          "page": 1,
          "x": 0.6,
          "y": 0.35,
          "width": 0.15,
          "height": 0.04
        }
      ]
    },
    {
      "fieldName": "net_salary",
      "value": "11500",
      "boundingBoxes": [
        {
          "page": 1,
          "x": 0.6,
          "y": 0.4,
          "width": 0.15,
          "height": 0.04
        }
      ]
    },
    {
      "fieldName": "authenticity_indicator",
      "value": "complete_structure_with_deductions",
      "confidence": 0.92
    }
  ],
  "metadata": {
    "company": "Tech Cloud Tree",
    "payPeriod": "Feb 2026",
    "markedByUser": true,
    "markedAt": "2026-04-13T00:00:00Z"
  }
}
```

### Fraudulent Document Labels

```json
{
  "document": "fake_payslip_001.pdf",
  "docType": "payslip",
  "docTypeConfidence": 0.3,
  "authenticityClass": "fraudulent",
  "fraudIndicators": [
    "minimal_fields_only",
    "no_deductions_breakdown",
    "missing_employer_info",
    "suspicious_salary_amount",
    "inconsistent_formatting"
  ],
  "fields": [
    {
      "fieldName": "employee_name",
      "value": "Shaahid",
      "boundingBoxes": [
        {
          "page": 1,
          "x": 0.1,
          "y": 0.1,
          "width": 0.2,
          "height": 0.03
        }
      ]
    },
    {
      "fieldName": "salary",
      "value": "2500",
      "boundingBoxes": [
        {
          "page": 1,
          "x": 0.1,
          "y": 0.2,
          "width": 0.15,
          "height": 0.03
        }
      ]
    }
  ],
  "metadata": {
    "fraudRisk": "HIGH",
    "reason": "Minimal memo pattern - only Name, Month, Salary fields"
  }
}
```

### Manifest File (manifest.jsonl)

Create a `manifest.jsonl` file referencing all training documents:

```jsonl
{"document": "sample-documents/authentic/payslip_001.pdf", "labels": "labels/payslip_001.labels.json"}
{"document": "sample-documents/authentic/payslip_002.pdf", "labels": "labels/payslip_002.labels.json"}
{"document": "sample-documents/authentic/payslip_003.pdf", "labels": "labels/payslip_003.labels.json"}
{"document": "sample-documents/fraudulent/fake_payslip_001.pdf", "labels": "labels/fake_payslip_001.labels.json"}
{"document": "sample-documents/fraudulent/fake_payslip_002.pdf", "labels": "labels/fake_payslip_002.labels.json"}
```

---

## 3. Upload Training Data to Azure Storage

### Using Azure CLI

```powershell
# Set variables
$resourceGroup = "rg-mcpserverdemo-sweden"
$storageAccount = "yourstorageaccountname"
$container = "training-dataset"
$localPath = "C:\TrainingData\"

# Create storage container
az storage container create `
  --name $container `
  --account-name $storageAccount `
  --resource-group $resourceGroup

# Upload all files
az storage blob upload-batch `
  --destination $container `
  --source $localPath `
  --account-name $storageAccount
```

### Using Azure Portal

1. Navigate to Storage Account → Containers
2. Create new container: `training-dataset`
3. Upload folder structure with all documents and labels
4. Verify all files uploaded successfully

---

## 4. Train Custom Model

### Option A: Azure Portal (Recommended for First-time)

1. **Go to Document Intelligence Studio**
   - URL: https://documentintelligence.ai.azure.com
   - Sign in with your Azure account

2. **Create New Project**
   - Click "Build" → "Custom Model"
   - Select "Document Classifier" or "Extracted Model"
   - Name: `PayslipAuthenticityValidator`

3. **Link Training Data**
   - Choose "Azure Blob Storage"
   - Select subscription and storage account
   - Select `training-dataset` container
   - Point to `manifest.jsonl`

4. **Configure Model**
   - **Model Type**: Custom Extracted (learns field extraction + structure)
   - **Document Classes**: 
     - Authentic_Payslip
     - Fraudulent_Payslip_Memo
     - Fraudulent_Payslip_Suspicious
   - **Fields to Extract**: employee_name, gross_salary, deductions, net_salary, authenticity_indicator

5. **Train Model**
   - Click "Train"
   - Wait for training completion (typically 5-15 minutes)
   - Review accuracy metrics

### Option B: REST API

```powershell
# Define parameters
$endpoint = "https://<region>.api.cognitive.microsoft.com/"
$apiKey = "YOUR_API_KEY"
$modelId = "payslip-authenticity-validator"

# Prepare request body
$trainingRequest = @{
    buildRequest = @{
        modelId = $modelId
        buildMode = "template"
        baseModelId = "prebuilt-document"
        azureBlobSource = @{
            containerUrl = "https://<storageaccount>.blob.core.windows.net/training-dataset?<sas-token>"
            prefix = ""
        }
        tags = "payslip", "authenticity", "fraud-detection"
        description = "Custom model for validating payslip authenticity"
    }
} | ConvertTo-Json -Depth 10

# Build model
$response = Invoke-RestMethod `
    -Uri "$endpoint/documentintelligence/document-classifiers:build?api-version=2024-02-29-preview" `
    -Headers @{
        "Ocp-Apim-Subscription-Key" = $apiKey
        "Content-Type" = "application/json"
    } `
    -Method Post `
    -Body $trainingRequest

Write-Host "Training started. Model ID: $($response.modelId)"
Write-Host "Status: $($response.status)"
```

### Option C: Python SDK

```python
from azure.ai.documentintelligence import DocumentIntelligenceAdministrationClient
from azure.ai.documentintelligence.models import BuildDocumentClassifierRequest, AzureBlobFileListSource
from azure.core.credentials import AzureKeyCredential

endpoint = "https://<region>.api.cognitive.microsoft.com/"
key = "YOUR_API_KEY"

client = DocumentIntelligenceAdministrationClient(endpoint, AzureKeyCredential(key))

request = BuildDocumentClassifierRequest(
    classifier_id="payslip-authenticity-validator",
    description="Validates payslip authenticity and detects fraud",
    doc_types={
        "authentic_payslip": {
            "azure_blob_file_list_source": {
                "container_url": "https://<storage>.blob.core.windows.net/training-dataset",
                "file_list": "manifest.jsonl"
            }
        }
    }
)

poller = client.begin_build_classifier(request)
result = poller.result()

print(f"Model trained: {result.classifier_id}")
print(f"Status: {result.status}")
```

---

## 5. Model Performance Metrics

After training completes, review:

| Metric | Target | Details |
|--------|--------|---------|
| **Accuracy** | > 85% | Overall correct predictions |
| **Precision** | > 90% | True positives / all positives |
| **Recall** | > 80% | True positives / actual positives |
| **F1-Score** | > 0.85 | Balanced measure of precision & recall |
| **Fraud Detection Rate** | > 95% | Catching fraudulent documents |
| **False Positive Rate** | < 5% | Avoiding false fraud flags |

---

## 6. Integration with MCP Server

### Update Your C# Code

Add to [DocumentExtractionService.cs](src/Services/DocumentExtractionService.cs):

```csharp
using Azure.AI.DocumentIntelligence;
using Azure.AI.DocumentIntelligence.Models;

public class PayslipAuthenticityValidator
{
    private readonly DocumentIntelligenceClient _client;
    private readonly string _modelId = "payslip-authenticity-validator";
    
    public PayslipAuthenticityValidator(string endpoint, string apiKey)
    {
        _client = new DocumentIntelligenceClient(
            new Uri(endpoint),
            new AzureKeyCredential(apiKey)
        );
    }
    
    public async Task<PayslipValidationResult> ValidatePayslipAsync(Stream documentStream)
    {
        // Use custom trained model for analysis
        var content = new AnalyzeDocumentContent { BytesSource = BinaryData.FromStream(documentStream) };
        
        var operation = await _client.AnalyzeDocumentAsync(
            WaitUntil.Completed,
            _modelId,
            content
        );
        
        var result = operation.Value;
        
        return new PayslipValidationResult
        {
            IsAuthentic = DetermineAuthenticity(result),
            ConfidenceScore = result.Confidence ?? 0,
            ExtractedFields = ExtractFields(result),
            FraudIndicators = IdentifyFraudIndicators(result)
        };
    }
    
    private bool DetermineAuthenticity(AnalyzeResult result)
    {
        // Map model classification to authenticity decision
        var docType = result.DocumentType ?? "unknown";
        return !docType.Contains("fraudulent");
    }
    
    private Dictionary<string, string> ExtractFields(AnalyzeResult result)
    {
        var fields = new Dictionary<string, string>();
        foreach (var doc in result.Documents)
        {
            foreach (var field in doc.Fields.Values)
            {
                fields[field.Name ?? "unknown"] = field.ValueString ?? field.AsString();
            }
        }
        return fields;
    }
    
    private List<string> IdentifyFraudIndicators(AnalyzeResult result)
    {
        var indicators = new List<string>();
        var doc = result.Documents?.FirstOrDefault();
        
        if (doc?.Fields.Count < 5)
            indicators.Add("minimal_fields");
        
        // Add more fraud checks based on extracted fields
        return indicators;
    }
}

public class PayslipValidationResult
{
    public bool IsAuthentic { get; set; }
    public double ConfidenceScore { get; set; }
    public Dictionary<string, string> ExtractedFields { get; set; }
    public List<string> FraudIndicators { get; set; }
}
```

### Add Environment Variables

Update [appsettings.json](src/appsettings.json):

```json
{
  "DocumentIntelligence": {
    "Endpoint": "https://<region>.api.cognitive.microsoft.com/",
    "ApiKey": "YOUR_API_KEY",
    "CustomModelId": "payslip-authenticity-validator"
  }
}
```

### Register Service

Update [Program.cs](src/Program.cs):

```csharp
// Add Document Intelligence services
builder.Services.AddScoped<PayslipAuthenticityValidator>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var endpoint = config["DocumentIntelligence:Endpoint"];
    var apiKey = config["DocumentIntelligence:ApiKey"];
    return new PayslipAuthenticityValidator(endpoint, apiKey);
});
```

---

## 7. Testing the Custom Model

### Test Locally

```csharp
[ApiController]
[Route("api/[controller]")]
public class PayslipValidationController : ControllerBase
{
    private readonly PayslipAuthenticityValidator _validator;
    
    public PayslipValidationController(PayslipAuthenticityValidator validator)
    {
        _validator = validator;
    }
    
    [HttpPost("validate")]
    public async Task<PayslipValidationResult> ValidatePayslip(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        return await _validator.ValidatePayslipAsync(stream);
    }
}
```

### Test via cURL

```bash
curl -X POST https://localhost:5000/api/payslipvalidation/validate \
  -F "file=@payslip_sample.pdf" \
  -H "Content-Type: multipart/form-data"
```

---

## 8. Deployment & Monitoring

### Deploy to Azure

```powershell
# Rebuild and publish
dotnet publish -c Release -o publish_azure

# Deploy
az webapp deployment source config-zip `
  --resource-group rg-mcpserverdemo-sweden `
  --name wa-mcpserver-sweden `
  --src ./deploy.zip
```

### Monitor Model Performance

```powershell
# View model details
az cognitiveservices account keys list `
  --name <di-resource-name> `
  --resource-group $resourceGroup
```

---

## 9. Iterative Improvement

### Collect Feedback
1. Track validation results in Application Insights
2. Log false positives and false negatives
3. Collect user feedback on incorrect classifications

### Retrain Model
```powershell
# Add new labeled documents to training-dataset
# Update manifest.jsonl
# Trigger retraining via Studio or API

# Model versioning
# New models: payslip-authenticity-validator-v2, v3, etc.
```

---

## 10. Cost Estimation

| Resource | Pricing | Estimated Monthly Cost |
|----------|---------|------------------------|
| Document Intelligence (S0) | $1 per 100 pages | $50-200* |
| Storage Account (Standard) | $0.023/GB | $10-30 |
| Custom Model Training | Included in S0 | $0 |
| **Total** | | **$60-230** |

*Depends on document volume and retraining frequency

---

## Troubleshooting

### Common Issues

| Issue | Solution |
|-------|----------|
| Training fails with "Invalid manifest" | Verify manifest.jsonl format, file paths, and blob container access |
| Low model accuracy | Increase training data (50+ → 100+), balance authentic/fraudulent ratio |
| "Unauthorized" errors | Check API key, endpoint region, storage account permissions |
| Slow training | This is normal; training takes 5-30 minutes depending on dataset size |

---

## Next Steps

1. ✅ Prepare training dataset (50-100 payslips)
2. ✅ Create labels for each document
3. ✅ Upload to Azure Storage
4. ✅ Train custom model via Studio
5. ✅ Integrate with MCP Server code
6. ✅ Deploy to Azure
7. ✅ Monitor and collect feedback
8. ✅ Retrain periodically with new data

---

## References

- [Azure Document Intelligence Docs](https://learn.microsoft.com/en-us/azure/ai-services/document-intelligence/)
- [Build Custom Models Guide](https://learn.microsoft.com/en-us/azure/ai-services/document-intelligence/how-to-guides/build-training-data-set)
- [REST API Reference](https://learn.microsoft.com/en-us/rest/api/documentintelligence/)

