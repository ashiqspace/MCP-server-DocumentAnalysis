# Quick Setup Checklist: Azure Document Intelligence Custom Model Training

## Phase 1: Prerequisites (Complete First)

- [ ] **Azure Resources**
  - [ ] Document Intelligence resource created (S0 or higher tier)
  - [ ] Storage account created (Standard tier minimum)
  - [ ] Blob container named `training-dataset` created
  - [ ] API key and endpoint noted

- [ ] **Credentials Configured**
  - [ ] Document Intelligence endpoint: `https://<region>.api.cognitive.microsoft.com/`
  - [ ] Document Intelligence API key: `<your-key>`
  - [ ] Storage account connection string: `DefaultEndpointsProtocol=https;...`

## Phase 2: Training Data Preparation (Critical)

### Gather Documents
- [ ] Collect 50-100 payslip sample documents
  - [ ] 70% authentic payslips (~70 docs)
  - [ ] 30% fraudulent payslips (~30 docs)
- [ ] Ensure documents are clear PDFs (200+ DPI recommended)

### Create Labels
- [ ] For each document, create `.labels.json` file
  - [ ] Authentic payslips: Mark as `"authenticityClass": "genuine"`
  - [ ] Fraudulent payslips: Mark as `"authenticityClass": "fraudulent"`
  - [ ] Include field bounding boxes (employee_name, gross_salary, deductions, net_salary)

### Upload to Azure
- [ ] Create folder structure:
  ```
  training-dataset/
  ├── sample-documents/
  │   ├── authentic/ (70 documents)
  │   └── fraudulent/ (30 documents)
  ├── labels/ (100 .labels.json files)
  └── manifest.jsonl
  ```
- [ ] Upload all files to blob container
- [ ] Verify files are accessible

## Phase 3: Train Model

### Option A: Use Azure Portal (Easiest)
- [ ] Navigate to https://documentintelligence.ai.azure.com
- [ ] Click "Build" → "Custom Model"
- [ ] Select "Document Classifier" or "Extracted Model"
- [ ] Name: `PayslipAuthenticityValidator`
- [ ] Link storage training-dataset container
- [ ] Configure document classes:
  - [ ] `Authentic_Payslip`
  - [ ] `Fraudulent_Payslip`
- [ ] Click "Train" and wait for completion
- [ ] Note the trained model ID

### Option B: Use REST API or SDK
- [ ] Use the PowerShell script provided in guide
- [ ] Wait for training completion (5-15 minutes)
- [ ] Verify model status

## Phase 4: Update MCP Server Code

- [ ] Update environment variables in `appsettings.json`:
  ```json
  "DocumentIntelligence": {
    "Endpoint": "https://<region>.api.cognitive.microsoft.com/",
    "ApiKey": "<your-key>",
    "CustomModelId": "payslip-authenticity-validator"
  }
  ```

- [ ] Add `PayslipAuthenticityValidator.cs` service class
  - [ ] Use custom model for document analysis
  - [ ] Extract fields from classified documents
  - [ ] Determine authenticity based on model output

- [ ] Update `Program.cs` to register service

- [ ] Modify `PayslipAnalyzerTool.cs` to:
  - [ ] Call custom model first
  - [ ] Combine ML results with rule-based validation
  - [ ] Return combined confidence score

## Phase 5: Testing

### Local Testing
- [ ] Build locally: `dotnet build`
- [ ] Run locally: `dotnet run`
- [ ] Test endpoint:
  ```bash
  curl -X POST http://localhost:5000/api/payslipvalidation/validate \
    -F "file=@test_payslip.pdf"
  ```
- [ ] Verify model is being called
- [ ] Check response format and accuracy

### Azure Testing
- [ ] Build release: `dotnet build -c Release`
- [ ] Deploy to Azure: `.\scripts\deploy.ps1 -Mode azure`
- [ ] Test live endpoint:
  ```
  https://wa-mcpserver-sweden.azurewebsites.net/api/payslipvalidation/validate
  ```
- [ ] Monitor Application Insights for errors

## Phase 6: Monitoring & Improvement

### Track Performance
- [ ] Enable Application Insights logging
- [ ] Log all validation results
- [ ] Track false positives and false negatives
- [ ] Collect user feedback

### Improve Model
- [ ] When you have 20+ incorrect predictions:
  - [ ] Add those documents to training set with correct labels
  - [ ] Upload updated manifest.jsonl
  - [ ] Retrain model (new version: v2, v3, etc.)
  - [ ] Validate accuracy improvement
  - [ ] Deploy new model version

## File Locations Reference

| File | Purpose | Status |
|------|---------|--------|
| [DOCUMENT_INTELLIGENCE_CUSTOM_TRAINING.md](./DOCUMENT_INTELLIGENCE_CUSTOM_TRAINING.md) | Complete setup guide | Created ✓ |
| [src/Services/DocumentExtractionService.cs](./src/Services/DocumentExtractionService.cs) | Document processing | Update needed |
| [src/Tools/PayslipAnalyzerTool.cs](./src/Tools/PayslipAnalyzerTool.cs) | Payslip validation tool | Update needed |
| [src/Program.cs](./src/Program.cs) | Service registration | Update needed |
| [src/appsettings.json](./src/appsettings.json) | Configuration | Update needed |

## Estimated Timeline

- **Phase 1** (Prerequisites): 30 minutes
- **Phase 2** (Data Preparation): 2-4 hours (document collection + labeling)
- **Phase 3** (Model Training): 30 minutes (wait time)
- **Phase 4** (Code Updates): 1-2 hours
- **Phase 5** (Testing): 1 hour
- **Phase 6** (Monitoring): Ongoing

**Total: 5-9 hours for initial setup**

## Key Decision Points

### 1. Custom Classifier vs. Extractor?
- **Custom Classifier**: Classifies docs as "Authentic" vs "Fraudulent" (recommended for your use case)
- **Custom Extractor**: Extracts fields + authenticates (more complex, more accurate)
- → **Recommendation**: Start with Classifier, upgrade to Extractor if needed

### 2. Single Model or Per-Organization Models?
- **Single Shared Model**: All organizations use same trained model
  - Pros: Simpler, lower cost
  - Cons: May miss organization-specific patterns
- **Per-Organization Models**: Train separate models for each customer
  - Pros: Higher accuracy, organization-specific
  - Cons: Higher cost, more management
- → **Recommendation**: Start with single shared model, transition to per-org if accuracy decreases

### 3. Combine with Existing Prompts?
- **Approach 1**: Replace prompts completely with ML model
- **Approach 2**: Use ML model + keep existing rule-based prompts
  - Return highest confidence result
  - Average both scores
  - Use prompts as fallback
- → **Recommendation**: Combine both (ensemble approach) for best accuracy

## Next Action

👉 **Start with Phase 1**: Verify you have Azure Document Intelligence resource and Storage Account ready. Once confirmed, proceed with Phase 2 to gather and label training data.

Need help with specific phase? Reference the detailed guide in: [DOCUMENT_INTELLIGENCE_CUSTOM_TRAINING.md](./DOCUMENT_INTELLIGENCE_CUSTOM_TRAINING.md)
