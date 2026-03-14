using Azure;
using Azure.AI.DocumentIntelligence;
using Microsoft.Extensions.Configuration;

namespace EchoMcpServer.Services;

/// <summary>
/// Service for extracting content from documents using Azure Document Intelligence
/// Provides a centralized way to handle PDF extraction for all tools
/// </summary>
public class DocumentExtractionService
{
    private readonly DocumentIntelligenceClient? _documentIntelligenceClient;
    private readonly string? _documentIntelligenceEndpoint;

    public DocumentExtractionService(IConfiguration configuration)
    {
        // Priority 1: Environment variables (App Service settings)
        _documentIntelligenceEndpoint = Environment.GetEnvironmentVariable("DOCUMENT_INTELLIGENCE_ENDPOINT") ?? 
            configuration["DocumentIntelligence:Endpoint"];
        var documentIntelligenceKey = Environment.GetEnvironmentVariable("DOCUMENT_INTELLIGENCE_KEY") ?? 
            configuration["DocumentIntelligence:ApiKey"];
        
        if (!string.IsNullOrEmpty(_documentIntelligenceEndpoint) && !string.IsNullOrEmpty(documentIntelligenceKey) &&
            !documentIntelligenceKey.Contains("YOUR_ACCOUNT_KEY_HERE"))
        {
            try
            {
                var credential = new Azure.AzureKeyCredential(documentIntelligenceKey);
                _documentIntelligenceClient = new DocumentIntelligenceClient(new Uri(_documentIntelligenceEndpoint), credential);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠ Warning: Failed to initialize Document Intelligence client: {ex.Message}");
                _documentIntelligenceClient = null;
            }
        }
        else
        {
            _documentIntelligenceClient = null;
        }
    }

    /// <summary>
    /// Checks if Document Intelligence is configured and available
    /// </summary>
    public bool IsConfigured => _documentIntelligenceClient != null;

    /// <summary>
    /// Extracts content from a PDF file using Document Intelligence
    /// </summary>
    /// <param name="pdfUri">URI of the PDF document (can be blob URL or file path)</param>
    /// <returns>Extracted text content from the PDF</returns>
    public async Task<string> ExtractContentFromPdfAsync(string pdfUri)
    {
        if (_documentIntelligenceClient == null)
        {
            return "Error: Document Intelligence not configured.";
        }

        try
        {
            // Use prebuilt-read model for document reading
            var operation = await _documentIntelligenceClient.AnalyzeDocumentAsync(
                WaitUntil.Completed,
                "prebuilt-read",
                new Uri(pdfUri));

            var result = operation.Value;

            if (result.Pages == null || result.Pages.Count == 0)
            {
                return "No content found in PDF.";
            }

            var extractedText = new System.Text.StringBuilder();
            foreach (var page in result.Pages)
            {
                if (page.Lines != null)
                {
                    foreach (var line in page.Lines)
                    {
                        extractedText.AppendLine(line.Content);
                    }
                }
            }

            return extractedText.ToString();
        }
        catch (Exception ex)
        {
            return $"Error extracting PDF content: {ex.Message}";
        }
    }
}
