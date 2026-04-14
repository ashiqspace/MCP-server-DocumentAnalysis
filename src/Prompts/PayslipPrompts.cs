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
        "You are a payslip fraud detection specialist with expertise in HR, payroll, financial analysis, and document verification. " +
        "Your primary role is to identify fraudulent or fabricated payslips through rigorous validation of financial consistency, " +
        "data completeness, employer credibility, and logical consistency. Respond ONLY with valid JSON output as specified.";

    /// <summary>
    /// Template for main payslip analysis prompt with enhanced fraud detection
    /// Use BuildAnalysisPrompt() to generate with parameters
    /// </summary>
    public static string AnalysisPromptTemplate =>
        "Analyze the following payslip document for fraud indicators:\n\n" +
        "{DOCUMENT_CONTENT}\n\n" +
        "=== DOCUMENT INFO ===\n" +
        "{EMPLOYEE_INFO}\n\n" +
        "=== STRUCTURAL VALIDATION ===\n" +
        "FRAUDULENT PATTERN - IMMEDIATE REJECTION:\n" +
        "Documents with ONLY Name + Month + Salary fields = AUTOMATICALLY INVALID (memo/note, NOT payslip)\n" +
        "Example: 'Name: Shaahid, Month: Jan 2026, Salary: 2500 $' → HIGH FRAUD RISK → REJECT\n" +
        "This minimal pattern indicates:\n" +
        "- Missing all earnings breakdown (no basic/incentive/allowance separation)\n" +
        "- ZERO deduction items (no taxes, insurance, or payroll deductions)\n" +
        "- No employer information or department\n" +
        "- No net pay calculation or verification\n" +
        "- No professional payroll structure\n" +
        "→ Result: FABRICATED DOCUMENT - Risk: HIGH - Recommendation: REJECT\n\n" +
        "Valid payslips MUST have:\n" +
        "1. Pay period identification (month/year acceptable; specific date ranges also acceptable)\n" +
        "2. Earnings breakdown: Gross pay amount clearly stated (basic, incentives, allowances separated)\n" +
        "3. At least 2-3 different deduction types (taxes, social security, healthcare, pension, loan payments, etc.)\n" +
        "4. Net pay (take-home) shown separately and mathematically verified\n" +
        "5. Professional payroll structure (organized table/sections with clear delineation)\n" +
        "6. Employee name AND employer information (company, department, or contact details)\n\n" +
        "VALID PAYSLIP CHARACTERISTICS (Generic Pattern):\n" +
        "✓ Complete earnings section: Multiple income sources listed (basic, incentives, allowances, bonuses, etc.)\n" +
        "✓ Gross pay total: Clear sum of all earnings\n" +
        "✓ Comprehensive deductions: Multiple deduction types present (minimum 2-3 different types)\n" +
        "✓ Deduction breakdown: Individual amounts for each deduction type shown\n" +
        "✓ Total deductions: Sum of all deductions clearly stated\n" +
        "✓ Net pay calculation: Gross - Total Deductions = Net Pay (mathematically verified)\n" +
        "✓ Professional formatting: Organized table/section structure with clear labels\n" +
        "✓ Complete identification: Employee name AND employer name (company, department, or contact info)\n" +
        "✓ Pay period clarity: Month/year or specific date range clearly marked\n\n" +
        "=== FINANCIAL CONSISTENCY CHECKS ===\n" +
        "CRITICAL: Verify mathematical relationships:\n" +
        "- Net salary MUST NOT exceed gross salary\n" +
        "- Net salary calculation: Gross - Total Deductions must equal Net Pay shown\n" +
        "- Total deduction percentage typically 10%–40% of gross (realistic range)\n" +
        "- Individual deductions should be proportional to gross pay (e.g., Provident Fund ~8-12%, Tax 3-20%, other deductions 2-5%)\n" +
        "- ACCEPT deductions including: Provident Fund, Social Security, Professional Tax, Healthcare, Loan payments, Insurance\n" +
        "- Flag only mathematically impossible values or extreme ratios (deductions >50% or suspiciously round numbers)\n\n" +
        "=== DATA COMPLETENESS ASSESSMENT ===\n" +
        "Critical missing fields increase fraud risk:\n" +
        "- Employee name or ID absent\n" +
        "- Employer information missing or vague\n" +
        "- Pay period dates incomplete or ambiguous\n" +
        "- Gross salary not clearly stated\n" +
        "- Deduction breakdown missing\n\n" +
        "=== EMPLOYER CREDIBILITY ===\n" +
        "Positive signals for authentic payslips:\n" +
        "- Formal company name (e.g., Tech Cloud Tree)\n" +
        "- Specific employer address\n" +
        "- Department information provided\n" +
        "- System-generated notation (e.g., 'This is system generated payslip')\n\n" +
        "Red flags for fraudulent documents:\n" +
        "- Unknown, generic, or suspicious employer names only\n" +
        "- Missing ALL employer contact information or details\n" +
        "- Unusual company structure described (inconsistent with industry norms)\n\n" +
        "=== LOGICAL ANOMALY DETECTION ===\n" +
        "Flag only GENUINE inconsistencies such as:\n" +
        "- Salary amounts that are obviously fabricated or use suspiciously round numbers (e.g., exactly 10,000 across all payslips)\n" +
        "- Deduction types completely mismatched with salary level (e.g., $5000 deduction on $6000 gross)\n" +
        "- Duplicate or directly contradictory values (Net pay listed differently in two places)\n" +
        "- Document formatting suggesting manual manipulation (cut/paste artifacts, inconsistent fonts, misaligned columns)\n\n" +
        "DO NOT flag as suspicious:\n" +
        "- Part-month salary (e.g., 26 worked days instead of 30) - normal variance\n" +
        "- Legitimate deductions like loan payments or insurance - expected in payroll\n" +
        "- Net pay shown in both numeric and word format - professional practice\n" +
        "- Multiple income sources (basic, incentive, allowances) - common for professionals\n\n" +
        "=== CRITICAL RULES ===\n" +
        "RULE 1: Minimal Payslip Pattern Detection\n" +
        "- If document contains ONLY Name + Month/Year + Salary: AUTOMATICALLY assign isValid=false, riskLevel=high, confidence=95+\n" +
        "- Recommendation: REJECT (fabricated memo, not authentic payslip)\n" +
        "- Add to structuralIssues: 'Minimal memo pattern with no payroll structure'\n" +
        "- Add to summary: 'This is a memo or summary stub, not an actual payslip. No earnings breakdown, deductions, or employer information.'\n\n" +
        "RULE 2: Earnings/Deductions Requirement\n" +
        "- Absence of deduction breakdown = strong fraud signal (HIGH RISK)\n" +
        "- Missing earnings breakdown = absence of payroll structure (HIGH RISK)\n" +
        "- Both absent together = AUTOMATIC REJECTION\n\n" +
        "RULE 3: Data Validation\n" +
        "- Do NOT assume missing information\n" +
        "- Do NOT fabricate details\n" +
        "- Base reasoning ONLY on provided input\n" +
        "- Treat minimal patterns as STRONG FRAUD SIGNALS\n" +
        "- Multiple issues compound risk assessment\n\n" +
        "=== RESPONSE FORMAT (STRICT JSON ONLY) ===\n" +
        "Return ONLY valid JSON:\n\n" +
        "{\n" +
        "  \"isValid\": boolean,\n" +
        "  \"documentType\": \"payslip\" | \"memo\" | \"note\" | \"salary_summary\" | \"unknown\",\n" +
        "  \"riskLevel\": \"low\" | \"medium\" | \"high\",\n" +
        "  \"confidence\": number (0-100),\n" +
        "  \"structuralIssues\": [\"reason1\", \"reason2\"],\n" +
        "  \"financialInconsistencies\": [\"issue1\", \"issue2\"],\n" +
        "  \"dataGaps\": [\"missing_field1\", \"missing_field2\"],\n" +
        "  \"credibilityFlags\": [\"concern1\", \"concern2\"],\n" +
        "  \"logicalAnomalies\": [\"anomaly1\", \"anomaly2\"],\n" +
        "  \"summary\": \"Concise factual summary (2-3 sentences)\",\n" +
        "  \"recommendation\": \"REJECT\" | \"INVESTIGATE\" | \"ACCEPT\"\n" +
        "}\n\n" +
        "=== RISK SCORING GUIDELINES ===\n" +
        "IMMEDIATE REJECTION (HIGH RISK, CONFIDENCE 95+):\n" +
        "- Minimal memo pattern: Name + Month + Salary ONLY → No deductions, no earnings breakdown, no employer info\n" +
        "- Mathematically impossible values (Net > Gross, negative deductions)\n" +
        "- Missing ALL critical fields with no meaningful payroll structure\n\n" +
        "HIGH RISK (CONFIDENCE 70-95): Multiple inconsistencies, impossible values, or strong validation issues\n" +
        "MEDIUM RISK (CONFIDENCE 40-70): Some inconsistencies or suspicious patterns\n" +
        "LOW RISK (CONFIDENCE 0-40): Complete, consistent, verifiable payslip with proper structure\n\n" +
        "Remember: Be concise, factual, and base all reasoning ONLY on provided input.";

    /// <summary>
    /// Template for quick validation prompt with fraud risk assessment
    /// Use BuildValidationPrompt() to generate with parameters
    /// </summary>
    public static string ValidationPromptTemplate =>
        "Quick fraud risk assessment for payslip:\n\n" +
        "=== DOCUMENT CONTENT ===\n" +
        "{DOCUMENT_CONTENT}\n\n" +
        "=== VALIDATION CRITERIA ===\n" +
        "Expected Employee ID: {EXPECTED_EMPLOYEE_ID}\n\n" +
        "Assess:\n" +
        "1. Document type (payslip/memo/unknown)\n" +
        "2. Financial consistency (Net ≤ Gross? Tax % realistic? Math checks out?)\n" +
        "3. Data completeness (employee, employer, dates, deductions present?)\n" +
        "4. Red flags or anomalies?\n\n" +
        "Return JSON:\n" +
        "{\n" +
        "  \"isPayslip\": boolean,\n" +
        "  \"riskLevel\": \"low\" | \"medium\" | \"high\",\n" +
        "  \"confidence\": number (0-100),\n" +
        "  \"concerns\": [\"concern1\", \"concern2\"]\n" +
        "}";

    /// <summary>
    /// Template for batch payslip analysis prompt
    /// Use BuildBatchPrompt() to generate with parameters
    /// </summary>
    public static string BatchPromptTemplate =>
        "Analyze the following batch of payslips for Employee ID: {EMPLOYEE_ID}\n\n" +
        "Total payslips to analyze: {PAYSLIP_COUNT}\n\n" +
        "=== PAYSLIP DOCUMENTS ===\n" +
        "{PAYSLIPS_FORMATTED}\n\n" +
        "=== BATCH ANALYSIS TASK ===\n" +
        "For each payslip, perform fraud detection and consistency analysis.\n" +
        "Then compare across payslips to identify patterns, inconsistencies, or anomalies.\n\n" +
        "=== CROSS-PAYSLIP ANALYSIS ===\n" +
        "1. Salary progression: Does salary remain consistent or change realistically?\n" +
        "2. Deduction patterns: Are deduction types and amounts consistent across time?\n" +
        "3. Employer consistency: Is employer information identical across all payslips?\n" +
        "4. Time gaps: Do pay periods form a logical sequence (weekly/bi-weekly/monthly)?\n" +
        "5. Date consistency: Are all dates valid and chronologically ordered?\n" +
        "6. Anomalies: Flag any sudden changes in salary, deductions, or structure\n\n" +
        "=== RESPONSE FORMAT (STRICT JSON ONLY) ===\n" +
        "Return ONLY valid JSON:\n\n" +
        "{\n" +
        "  \"employeeId\": \"{EMPLOYEE_ID}\",\n" +
        "  \"totalPayslips\": {PAYSLIP_COUNT},\n" +
        "  \"validPayslips\": number,\n" +
        "  \"fraudulentPayslips\": number,\n" +
        "  \"overallRiskLevel\": \"low\" | \"medium\" | \"high\",\n" +
        "  \"overallConfidence\": number (0-100),\n" +
        "  \"individual_analyses\": [\n" +
        "    {\n" +
        "      \"payslipNumber\": 1,\n" +
        "      \"isValid\": boolean,\n" +
        "      \"riskLevel\": \"low\" | \"medium\" | \"high\",\n" +
        "      \"issues\": [\"issue1\", \"issue2\"]\n" +
        "    }\n" +
        "  ],\n" +
        "  \"crossPayslipFindings\": [\"finding1\", \"finding2\"],\n" +
        "  \"recommendation\": \"REJECT\" | \"INVESTIGATE\" | \"ACCEPT\",\n" +
        "  \"summary\": \"Concise summary of batch analysis (2-3 sentences)\"\n" +
        "}";
}
