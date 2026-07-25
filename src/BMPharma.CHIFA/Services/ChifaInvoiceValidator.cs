using BMPharma.CHIFA.Interfaces;
using Microsoft.Extensions.Logging;

namespace BMPharma.CHIFA.Services;

public class ChifaInvoiceValidator
{
    private readonly ILogger<ChifaInvoiceValidator> _logger;

    public const int MaxNumFactLength = 8;
    public const int MaxNumEnrLength = 5;
    public const int MaxNumAssureLength = 12;
    public const int MaxCodeCentreLength = 5;
    public const int MaxQteValue = 999;

    public ChifaInvoiceValidator(ILogger<ChifaInvoiceValidator> logger)
    {
        _logger = logger;
    }

    public ChifaInvoiceValidationResult Validate(ChifaInvoiceRequest request)
    {
        var result = new ChifaInvoiceValidationResult();

        if (request.NumFact.Length > MaxNumFactLength)
        {
            result.AddError("NUM_FACT_TOO_LONG", "num_fact",
                $"Invoice number exceeds {MaxNumFactLength} characters (got {request.NumFact.Length})");
        }

        if (string.IsNullOrWhiteSpace(request.NumFact))
        {
            result.AddError("NUM_FACT_EMPTY", "num_fact", "Invoice number is required");
        }

        if (string.IsNullOrWhiteSpace(request.NumAssure))
        {
            result.AddError("NUM_ASSURE_EMPTY", "num_assure", "Insurance number is required");
        }

        if (request.NumAssure.Length > MaxNumAssureLength)
        {
            result.AddError("NUM_ASSURE_TOO_LONG", "num_assure",
                $"Insurance number exceeds {MaxNumAssureLength} characters");
        }

        if (request.CodeCentre.ToString().Length > MaxCodeCentreLength)
        {
            result.AddError("CODE_CENTRE_TOO_LONG", "code_centre",
                $"Centre code exceeds {MaxCodeCentreLength} characters");
        }

        if (request.Lines.Count == 0)
        {
            result.AddError("NO_LINES", "lines", "Invoice must have at least one line");
        }

        for (int i = 0; i < request.Lines.Count; i++)
        {
            var line = request.Lines[i];
            var prefix = $"lines[{i}]";

            if (line.NumEnr.Length > MaxNumEnrLength)
            {
                result.AddError("NUM_ENR_TOO_LONG", $"{prefix}.num_enr",
                    $"Registration number exceeds {MaxNumEnrLength} characters");
            }

            if (line.Quantite <= 0)
            {
                result.AddError("QTE_INVALID", $"{prefix}.qte",
                    "Quantity must be positive");
            }

            if (line.Quantite > MaxQteValue)
            {
                result.AddError("QTE_TOO_HIGH", $"{prefix}.qte",
                    $"Quantity exceeds maximum of {MaxQteValue}");
            }

            if (line.PrixUnit <= 0)
            {
                result.AddError("PPA_INVALID", $"{prefix}.ppa",
                    "PPA price must be positive");
            }
        }

        return result;
    }

    public void ApplyDefaults(ChifaInvoiceRequest request)
    {
        if (request.DateSoin == default)
            request.DateSoin = DateTime.Today;
    }

    public void ApplyLineDefaults(ChifaInvoiceLineRequest line)
    {
        line.InfTr = line.InfTr == 0 ? 1 : line.InfTr;
        line.ApplicTr = line.ApplicTr == 0 ? 1 : line.ApplicTr;
        line.Medic = line.Medic == 0 ? 1 : line.Medic;
        line.Ts = line.Ts == 0 ? 4 : line.Ts;
        line.DureeTrait = line.DureeTrait == 0 ? 5 : line.DureeTrait;
    }
}

public class ChifaInvoiceValidationResult
{
    public bool IsValid => Errors.Count == 0;
    public List<ChifaValidationError> Errors { get; } = new();

    public void AddError(string code, string field, string message)
    {
        Errors.Add(new ChifaValidationError { Code = code, Field = field, Message = message });
    }
}

public class ChifaValidationError
{
    public string Code { get; set; } = string.Empty;
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
