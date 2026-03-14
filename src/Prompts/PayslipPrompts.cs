namespace EchoMcpServer.Prompts;

/// <summary>
/// Prompt templates for payslip analysis and validation
/// Follows the separation of concerns principle: prompts are stored separately from business logic
/// </summary>
public static class PayslipPrompts
{
    /// <summary>
    /// System prompt for payslip analyzer - defines the role and expertise
    /// </summary>
    public static string SystemPrompt => 
        "You are a payslip document analyzer with expertise in HR, payroll, and document verification. Analyze documents accurately and flag any discrepancies or authenticity concerns.";

    /// <summary>
    /// Template for main payslip analysis prompt
    /// Use BuildAnalysisPrompt() to generate with parameters
    /// </summary>
    public static string AnalysisPromptTemplate =>
        "Analyze the following payslip document content:\n\n" +
        "{DOCUMENT_CONTENT}\n\n" +
        "=== DOCUMENT INFO ===\n" +
        "{EMPLOYEE_INFO}\n\n" +
        "You are a payslip authenticity validator. Your ONLY job is to identify if a document is a REAL professional payslip.\n\n" +
        "=== CRITICAL: REJECT IMMEDIATELY ===\n" +
        "Documents showing ONLY these fields are AUTOMATICALLY INVALID (NOT payslips, they are memos):\n" +
        "- Name\n" +
        "- Month\n" +
        "- Salary\n\n" +
        "ANY document matching this pattern must be rejected as INVALID.\n\n" +
        "=== VALID PAYSLIP REQUIREMENTS (ALL MUST BE PRESENT) ===\n" +
        "1. Specific pay period dates (e.g., 01/01/2026 - 01/31/2026) - NOT just January 2026\n" +
        "2. Gross pay amount (from earnings breakdown)\n" +
        "3. MINIMUM 2-3 different deduction types (taxes, social security, healthcare, pension, etc.)\n" +
        "4. Net pay (take-home amount) shown separately\n" +
        "5. Professional structure: organized sections/table format, NOT plain text\n\n" +
        "=== WHAT MAKES IT INVALID ===\n" +
        "✗ Plain text format: Name: X, Company: Y, Month: Z, Salary: $5000\n" +
        "✗ Missing deductions (salary with no taxes/deductions)\n" +
        "✗ Missing net pay line\n" +
        "✗ Generic month without specific dates\n" +
        "✗ Unstructured memo-style layout\n\n" +
        "=== RESPONSE FORMAT ===\n\n" +
        "Document Validity: VALID or INVALID\n" +
        "Document Type: [payslip / memo / note / salary summary]\n\n" +
        "If INVALID:\n" +
        "- Reason: [This is NOT a payslip because: ...]\n" +
        "- Missing Elements: [deductions / net pay / specific dates / etc.]\n\n" +
        "If VALID (only when ALL requirements are met):\n" +
        "- Employee: [name and ID if present]\n" +
        "- Pay Period: [specific dates]\n" +
        "- Gross Pay: [amount]\n" +
        "- Deductions: [list items with amounts]\n" +
        "- Net Pay: [amount]\n" +
        "- Verification: Gross - Deductions = Net? [YES/NO]\n\n" +
        "Remember: Simple documents with Name, Month, Salary are MEMOS, not payslips. ALWAYS reject them.";

    /// <summary>
    /// Template for quick validation prompt
    /// Use BuildValidationPrompt() to generate with parameters
    /// </summary>
    public static string ValidationPromptTemplate =>
        "Quickly validate if this document is a payslip:\n\n" +
        "=== DOCUMENT CONTENT ===\n" +
        "{DOCUMENT_CONTENT}\n\n" +
        "=== VALIDATION CRITERIA ===\n" +
        "Expected Employee ID: {EXPECTED_EMPLOYEE_ID}\n\n" +
        "Respond with:\n" +
        "1. Is this likely a payslip? (Yes/No/Unknown)\n" +
        "2. Confidence level (High/Medium/Low)\n" +
        "3. Any red flags? (Yes - describe / No)\n\n" +
        "Keep response concise.";

    /// <summary>
    /// Template for batch payslip analysis prompt
    /// Use BuildBatchPrompt() to generate with parameters
    /// </summary>
    public static string BatchPromptTemplate =>
        "Analyze the following {PAYSLIP_COUNT} payslips for employee ID: {EMPLOYEE_ID}\n\n" +
        "=== PAYSLIP DOCUMENT CONTENTS ===\n" +
        "{PAYSLIPS_FORMATTED}\n\n" +
        "=== BATCH ANALYSIS INSTRUCTIONS ===\n" +
        "1. Verify all payslips belong to the same employee ({EMPLOYEE_ID})\n" +
        "2. Check for consistency across payslips\n" +
        "3. Calculate totals:\n" +
        "   - Total gross pay\n" +
        "   - Total deductions\n" +
        "   - Total net pay\n" +
        "   - Average pay per period\n" +
        "4. Identify trends or anomalies\n" +
        "5. Flag any discrepancies or concerns\n\n" +
        "=== RESPONSE FORMAT ===\n" +
        "- Employee Verification: [Consistent/Inconsistent/Flagged]\n" +
        "- Payslip Count: [number]\n" +
        "- Date Range: [first to last pay period]\n" +
        "- Financial Summary:\n" +
        "  * Total Gross: [amount]\n" +
        "  * Total Deductions: [amount]\n" +
        "  * Total Net: [amount]\n" +
        "  * Average per Period: [amount]\n" +
        "- Trends & Anomalies: [any notable patterns]\n" +
        "- Issues/Flags: [any concerns or None]\n" +
        "- Summary: [overall assessment]";
}
