using System.ComponentModel;
using Azure;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Server;
using EchoMcpServer.Services;

namespace EchoMcpServer.Tools;

[McpServerToolType]
public class AzureStorageSearchTool
{
    private readonly BlobContainerClient _containerClient;
    private readonly DocumentExtractionService _documentExtractionService;

    public AzureStorageSearchTool(IConfiguration configuration, IHostEnvironment environment, DocumentExtractionService documentExtractionService)
    {
        _documentExtractionService = documentExtractionService;

        // Priority 1: Environment variable (Azure deployment & explicit override)
        var connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
        
        // Priority 2: Configuration file
        // - In Development: Uses appsettings.Development.json (local with credentials)
        // - In Production: Uses appsettings.json (no credentials - relies on env var)
        if (string.IsNullOrEmpty(connectionString))
        {
            connectionString = configuration["AzureStorage:ConnectionString"];
        }
        
        var containerName = configuration["AzureStorage:ContainerName"] ?? "documents";
        
        if (!string.IsNullOrEmpty(connectionString) && !connectionString.Contains("YOUR_ACCOUNT_KEY_HERE"))
        {
            // Use connection string for authenticated access to read tags
            _containerClient = new BlobContainerClient(connectionString, containerName);
        }
        else
        {
            // Fall back to anonymous access (limited functionality)
            var fallbackUri = "https://samcpserverdemo.blob.core.windows.net/documents";
            Console.WriteLine("Warning: No connection string found.");
            
            if (environment.IsDevelopment())
            {
                Console.WriteLine("LOCAL DEVELOPMENT - Add your connection string to: appsettings.Development.json");
                Console.WriteLine("File location: src/appsettings.Development.json");
                Console.WriteLine("Replace YOUR_ACCOUNT_KEY_HERE with your actual storage account key.");
            }
            else
            {
                Console.WriteLine("AZURE PRODUCTION - Set AZURE_STORAGE_CONNECTION_STRING in App Service Configuration.");
                Console.WriteLine("Go to Azure Portal > App Service > Settings > Configuration > Application settings");
            }
            
            _containerClient = new BlobContainerClient(new Uri(fallbackUri));
        }
    }

    /// <summary>
    /// Searches for files by employee name in Azure Storage and returns their URLs and extracted content
    /// </summary>
    [McpServerTool(Name = "searchFilesByName")]
    [Description("Searches for files by employee name in Azure Storage and returns their download URLs and extracted content from PDF payslips")]
    public async Task<string> SearchFilesByName([Description("Employee name to search for")] string employeeName)
    {
        try
        {
            var results = new List<string>();
            await foreach (var blobItem in _containerClient.GetBlobsAsync())
            {
                var blobClient = _containerClient.GetBlobClient(blobItem.Name);
                
                // First, try to match against blob index tags for employee name
                var tagsResult = await blobClient.GetTagsAsync();
                if (tagsResult.Value.Tags.TryGetValue("employeename", out var empName) && 
                    empName.Contains(employeeName, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(blobItem.Name);
                    continue;
                }
                
                // Fallback: extract content from PDF and search for employee name
                if (blobItem.Name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    var blobUri = blobClient.Uri.AbsoluteUri;
                    var extractedContent = await _documentExtractionService.ExtractContentFromPdfAsync(blobUri);
                    if (extractedContent.Contains(employeeName, StringComparison.OrdinalIgnoreCase))
                    {
                        results.Add(blobItem.Name);
                    }
                }
            }
            
            if (results.Count == 0)
            {
                return $"No files found for employee '{employeeName}'";
            }

            var response = new System.Text.StringBuilder();
            response.AppendLine($"Found {results.Count} files for employee '{employeeName}':");
            response.AppendLine();

            foreach (var fileName in results)
            {
                var blobClient = _containerClient.GetBlobClient(fileName);
                var blobUri = blobClient.Uri.AbsoluteUri;
                response.AppendLine($"File: {fileName}");
                response.AppendLine($"URL: {blobUri}");
                
                // Extract content if it's a PDF
                if (fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    var extractedContent = await _documentExtractionService.ExtractContentFromPdfAsync(blobUri);
                    response.AppendLine($"Extracted Content:\n{extractedContent}");
                }
                response.AppendLine();
            }

            return response.ToString();
        }
        catch (Exception ex)
        {
            return $"Error searching files: {ex.Message}";
        }
    }

    /// <summary>
    /// Searches for files by employee ID blob index tag and returns their URLs and extracted content
    /// </summary>
    [McpServerTool(Name = "searchFilesByEmployeeId")]
    [Description("Searches for files by employee ID blob index tag and returns their download URLs and extracted content from PDF payslips")]
    public async Task<string> SearchFilesByEmployeeId([Description("Employee ID to search for")] string employeeId)
    {
        try
        {
            var results = new List<string>();
            await foreach (var blobItem in _containerClient.GetBlobsAsync())
            {
                var blobClient = _containerClient.GetBlobClient(blobItem.Name);
                
                // Get blob index tags
                var tagsResult = await blobClient.GetTagsAsync();
                if (tagsResult.Value.Tags.TryGetValue("employeeid", out var empId) && 
                    empId.Equals(employeeId, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(blobItem.Name);
                }
            }
            
            if (results.Count == 0)
            {
                return $"No files found for employee {employeeId}";
            }

            var response = new System.Text.StringBuilder();
            response.AppendLine($"Found {results.Count} files for employee {employeeId}:");
            response.AppendLine();

            foreach (var fileName in results)
            {
                var blobClient = _containerClient.GetBlobClient(fileName);
                var blobUri = blobClient.Uri.AbsoluteUri;
                response.AppendLine($"File: {fileName}");
                response.AppendLine($"URL: {blobUri}");
                
                // Extract content if it's a PDF
                if (fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    var extractedContent = await _documentExtractionService.ExtractContentFromPdfAsync(blobUri);
                    response.AppendLine($"Extracted Content:\n{extractedContent}");
                }
                response.AppendLine();
            }

            return response.ToString();
        }
        catch (Exception ex)
        {
            return $"Error searching by employee ID: {ex.Message}";
        }
    }
}
