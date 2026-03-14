namespace EchoMcpServer.Prompts;

/// <summary>
/// Builds prompts by interpolating templates with actual data
/// Centralizes prompt construction logic
/// </summary>
public class PromptBuilder
{
    /// <summary>
    /// Builds the main payslip analysis prompt
    /// </summary>
    public static string BuildAnalysisPrompt(string documentContent, string employeeId = "", string employeeName = "")
    {
        var employeeInfo = "";
        if (!string.IsNullOrEmpty(employeeId))
            employeeInfo += $"Employee ID: {employeeId}\n";
        if (!string.IsNullOrEmpty(employeeName))
            employeeInfo += $"Employee Name: {employeeName}\n";

        return PayslipPrompts.AnalysisPromptTemplate
            .Replace("{DOCUMENT_CONTENT}", documentContent)
            .Replace("{EMPLOYEE_INFO}", employeeInfo);
    }

    /// <summary>
    /// Builds the quick validation prompt
    /// </summary>
    public static string BuildValidationPrompt(string documentContent, string expectedEmployeeId = "")
    {
        return PayslipPrompts.ValidationPromptTemplate
            .Replace("{DOCUMENT_CONTENT}", documentContent)
            .Replace("{EXPECTED_EMPLOYEE_ID}", expectedEmployeeId);
    }

    /// <summary>
    /// Builds the batch analysis prompt
    /// </summary>
    public static string BuildBatchPrompt(string employeeId, int payslipCount, string payslipcontent)
    {
        return PayslipPrompts.BatchPromptTemplate
            .Replace("{EMPLOYEE_ID}", employeeId)
            .Replace("{PAYSLIP_COUNT}", payslipCount.ToString())
            .Replace("{PAYSLIPS_FORMATTED}", payslipcontent);
    }
}
