using System.ComponentModel;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Server;
using EchoMcpServer.Services;
using EchoMcpServer.Prompts;

namespace EchoMcpServer.Tools;

[McpServerToolType]
public class PayslipAnalyzerTool
{
    private readonly string? _aiFoundryEndpoint;
    private readonly string? _aiFoundryKey;
    private readonly string? _aiFoundryModel;
    private readonly IHostEnvironment _environment;
    private readonly DocumentExtractionService _documentExtractionService;

    public PayslipAnalyzerTool(IConfiguration configuration, IHostEnvironment environment, DocumentExtractionService documentExtractionService)
    {
        _environment = environment;
        _documentExtractionService = documentExtractionService;

        // Priority 1: Environment variables (App Service settings or explicit override)
        _aiFoundryEndpoint = Environment.GetEnvironmentVariable("AI_FOUNDRY_ENDPOINT") ?? 
            configuration["AIFoundry:Endpoint"];
        _aiFoundryKey = Environment.GetEnvironmentVariable("AI_FOUNDRY_KEY") ?? 
            configuration["AIFoundry:Key"];
        _aiFoundryModel = Environment.GetEnvironmentVariable("AI_FOUNDRY_MODEL") ?? 
            configuration["AIFoundry:Model"] ?? "gpt-4";

        // Log configuration status
        if (!string.IsNullOrEmpty(_aiFoundryEndpoint) && !string.IsNullOrEmpty(_aiFoundryKey) &&
            !_aiFoundryKey.Contains("YOUR_KEY_HERE"))
        {
            Console.WriteLine($"✓ AI Foundry ({_aiFoundryModel}) configured successfully.");
        }
        else
        {
            Console.WriteLine("⚠ Warning: AI Foundry not configured. Payslip analysis will not be available.");
            if (string.IsNullOrEmpty(_aiFoundryEndpoint))
                Console.WriteLine("  Missing: AI_FOUNDRY_ENDPOINT environment variable");
            if (string.IsNullOrEmpty(_aiFoundryKey))
                Console.WriteLine("  Missing: AI_FOUNDRY_KEY environment variable");
        }
    }

    /// <summary>
    /// Analyzes a payslip document using GPT-5-nano AI Foundry model
    /// Uses pre-extracted content (from searchFilesByName or searchFilesByEmployeeId)
    /// Authenticates the document and flags if it doesn't appear to be a valid payslip
    /// </summary>
    [McpServerTool(Name = "analyzePayslipContent")]
    [Description("Analyzes a payslip document using AI to verify it's a valid payslip and extract key information. Use with searchFilesByName or searchFilesByEmployeeId to extract content first.")]
    public async Task<string> AnalyzePayslipContent(
        [Description("Pre-extracted payslip content from searchFilesByName or searchFilesByEmployeeId")] string payslipContent,
        [Description("Employee ID for payslip verification")] string employeeId = "",
        [Description("Employee name for payslip verification")] string employeeName = "")
    {
        if (string.IsNullOrEmpty(_aiFoundryEndpoint) || string.IsNullOrEmpty(_aiFoundryKey))
        {
            return "Error: AI Foundry not configured. Please add AI_FOUNDRY_ENDPOINT and AI_FOUNDRY_KEY to your application settings.";
        }

        try
        {
            // Validate that content was provided
            if (string.IsNullOrEmpty(payslipContent))
            {
                return "Error: No payslip content provided. Use searchFilesByName() or searchFilesByEmployeeId() first to extract document content.";
            }

            // Build the prompt using PromptBuilder
            var prompt = PromptBuilder.BuildAnalysisPrompt(payslipContent, employeeId, employeeName);

            // Call GPT-5-nano via Azure AI Foundry
            var response = await CallAIModel(prompt);

            return response;
        }
        catch (Exception ex)
        {
            return $"Error analyzing payslip content: {ex.Message}";
        }
    }


    /// <summary>
    /// Calls the AI model via Azure AI Foundry (GPT-4, GPT-4o, or configured model)
    /// </summary>
    private async Task<string> CallAIModel(string prompt)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("api-key", _aiFoundryKey);

        // Prepare the request payload
        var requestPayload = new
        {
            model = _aiFoundryModel,
            messages = new[]
            {
                new
                {
                    role = "system",
                    content = PayslipPrompts.SystemPrompt
                },
                new
                {
                    role = "user",
                    content = prompt
                }
            },
            max_tokens = 1500
        };

        var jsonContent = new StringContent(
            JsonSerializer.Serialize(requestPayload),
            System.Text.Encoding.UTF8,
            "application/json");

        // Call the AI Foundry endpoint
        var response = await client.PostAsync(_aiFoundryEndpoint, jsonContent);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"AI Foundry API error: {response.StatusCode} - {errorContent}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<JsonElement>(responseContent);

        // Extract the model's response
        var choices = responseData.GetProperty("choices");
        var message = choices[0].GetProperty("message");
        var analysisResult = message.GetProperty("content").GetString();

        return analysisResult ?? "Unable to analyze payslip content.";
    }

    /// <summary>
    /// Validates that a document matches the expected criteria for a payslip
    /// Can be called independently for quick validation
    /// </summary>
    [McpServerTool(Name = "validatePayslipDocument")]
    [Description("Quick validation to check if a document is likely a payslip before detailed analysis")]
    public async Task<string> ValidatePayslipDocument(
        [Description("URL of the payslip document to validate")] string payslipUrl,
        [Description("Expected employee ID")] string expectedEmployeeId = "")
    {
        try
        {
            if (string.IsNullOrEmpty(_aiFoundryEndpoint) || string.IsNullOrEmpty(_aiFoundryKey))
            {
                return "Error: AI Foundry not configured.";
            }

            // Extract document content first
            var documentContent = await _documentExtractionService.ExtractContentFromPdfAsync(payslipUrl);
            if (string.IsNullOrEmpty(documentContent) || documentContent.Contains("not configured", StringComparison.OrdinalIgnoreCase))
            {
                return $"Error: Unable to extract document content. {documentContent}";
            }

            // Build validation prompt using PromptBuilder
            var validationPrompt = PromptBuilder.BuildValidationPrompt(documentContent, expectedEmployeeId);

            var response = await CallAIModel(validationPrompt);
            return response;
        }
        catch (Exception ex)
        {
            return $"Error validating payslip document: {ex.Message}";
        }
    }

    /// <summary>
    /// Batch processes multiple payslips for an employee and generates a summary
    /// </summary>
    [McpServerTool(Name = "analyzePayslipBatch")]
    [Description("Analyzes multiple payslips for an employee and generates a summary of income and deductions")]
    public async Task<string> AnalyzePayslipBatch(
        [Description("Employee ID to analyze")] string employeeId,
        [Description("Semicolon-separated list of payslip document URLs")] string payslipUrls)
    {
        try
        {
            if (string.IsNullOrEmpty(_aiFoundryEndpoint) || string.IsNullOrEmpty(_aiFoundryKey))
            {
                return "Error: AI Foundry not configured.";
            }

            var payslips = payslipUrls.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(url => url.Trim())
                .ToArray();

            if (payslips.Length == 0)
            {
                return "No payslips provided for batch analysis.";
            }

            // Extract content from all payslips
            var payslipContents = new List<string>();
            foreach (var payslipUrl in payslips)
            {
                var content = await _documentExtractionService.ExtractContentFromPdfAsync(payslipUrl);
                if (string.IsNullOrEmpty(content) || content.Contains("not configured", StringComparison.OrdinalIgnoreCase))
                {
                    return $"Error: Unable to extract document content from {payslipUrl}. {content}";
                }
                payslipContents.Add(content);
            }

            // Format extracted contents for analysis
            var payslipsFormatted = string.Join("\n\n--- PAYSLIP SEPARATOR ---\n\n", 
                payslipContents.Select((content, index) => $"Payslip {index + 1}:\n{content}"));

            // Build batch prompt using PromptBuilder
            var batchPrompt = PromptBuilder.BuildBatchPrompt(employeeId, payslips.Length, payslipsFormatted);

            var response = await CallAIModel(batchPrompt);
            return response;
        }
        catch (Exception ex)
        {
            return $"Error analyzing payslip batch: {ex.Message}";
        }
    }
}
