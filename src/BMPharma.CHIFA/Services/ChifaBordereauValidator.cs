namespace BMPharma.CHIFA.Services;

public class ChifaBordereauValidator
{
    public const int MaxNumBordLength = 6;

    public ChifaBordereauValidationResult Validate(string numBord, string codeCentre, List<string> invoiceNumbers)
    {
        var result = new ChifaBordereauValidationResult();

        if (string.IsNullOrWhiteSpace(numBord))
        {
            result.AddError("NUM_BORD_EMPTY", "num_bord", "Bordereau number is required");
        }

        if (numBord.Length > MaxNumBordLength)
        {
            result.AddError("NUM_BORD_TOO_LONG", "num_bord",
                $"Bordereau number exceeds {MaxNumBordLength} characters (got {numBord.Length})");
        }

        if (string.IsNullOrWhiteSpace(codeCentre))
        {
            result.AddError("CODE_CENTRE_EMPTY", "code_centre", "Centre code is required");
        }

        if (invoiceNumbers.Count == 0)
        {
            result.AddError("NO_INVOICES", "invoices", "Bordereau must contain at least one invoice");
        }

        return result;
    }
}

public class ChifaBordereauValidationResult
{
    public bool IsValid => Errors.Count == 0;
    public List<ChifaBordereauValidationError> Errors { get; } = new();

    public void AddError(string code, string field, string message)
    {
        Errors.Add(new ChifaBordereauValidationError { Code = code, Field = field, Message = message });
    }
}

public class ChifaBordereauValidationError
{
    public string Code { get; set; } = string.Empty;
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
