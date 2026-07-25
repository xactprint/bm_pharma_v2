using BMPharma.CHIFA.Interfaces;
using BMPharma.Domain.Entities;

namespace BMPharma.CHIFA.Services;

public class ChifaInvoiceMapper
{
    public const int DefaultCodeCentre = 11600;
    public const string DefaultNumAssurePrefix = "00";

    public ChifaInvoiceRequest MapToChifaRequest(Invoice invoice, Customer? customer = null)
    {
        if (invoice == null)
            throw new ArgumentNullException(nameof(invoice));

        var numAssure = customer?.InsuranceNumber ?? invoice.Customer?.InsuranceNumber ?? invoice.ChifaNumAssure ?? string.Empty;
        var codeCentre = invoice.ChifaCodeCentre ?? DefaultCodeCentre;

        var request = new ChifaInvoiceRequest
        {
            NumFact = MapNumFact(invoice.InvoiceNumber),
            NumAssure = numAssure,
            CodeCentre = codeCentre,
            DateSoin = invoice.InvoiceDate,
            Lines = invoice.Lines.Select(MapLine).ToList()
        };

        return request;
    }

    public string MapNumFact(string invoiceNumber)
    {
        if (string.IsNullOrWhiteSpace(invoiceNumber))
            return string.Empty;

        var cleaned = invoiceNumber.Replace("-", "").Replace(" ", "");

        if (cleaned.Length > ChifaInvoiceValidator.MaxNumFactLength)
            return cleaned[^ChifaInvoiceValidator.MaxNumFactLength..];

        return cleaned.PadLeft(ChifaInvoiceValidator.MaxNumFactLength, '0');
    }

    public string MapNumAssure(Customer customer)
    {
        if (customer == null)
            return string.Empty;

        var numAssure = customer.InsuranceNumber ?? string.Empty;

        if (numAssure.Length > ChifaInvoiceValidator.MaxNumAssureLength)
            return numAssure[^ChifaInvoiceValidator.MaxNumAssureLength..];

        return numAssure;
    }

    public ChifaInvoiceLineRequest MapLine(InvoiceLine line)
    {
        if (line == null)
            throw new ArgumentNullException(nameof(line));

        var medicCode = MapMedicCode(line.Product?.CIPCode ?? line.Product?.Code);

        return new ChifaInvoiceLineRequest
        {
            NumEnr = MapNumEnr(line.Product?.Code),
            MedicCode = medicCode,
            PrixUnit = line.UnitPriceDA,
            Quantite = line.Quantity,
            InfTr = 1,
            ApplicTr = 1,
            Medic = 1,
            Ts = 4,
            DureeTrait = 5
        };
    }

    public string MapNumEnr(string? productCode)
    {
        if (string.IsNullOrWhiteSpace(productCode))
            return "00001";

        var cleaned = productCode.Replace("-", "").Replace(" ", "");

        if (cleaned.Length > ChifaInvoiceValidator.MaxNumEnrLength)
            return cleaned[^ChifaInvoiceValidator.MaxNumEnrLength..];

        return cleaned.PadLeft(ChifaInvoiceValidator.MaxNumEnrLength, '0');
    }

    public int MapMedicCode(string? cipCode)
    {
        if (string.IsNullOrWhiteSpace(cipCode))
            return 1;

        var cleaned = cipCode.Replace("-", "").Replace(" ", "");

        if (int.TryParse(cleaned, out var code) && code > 0)
            return code;

        if (cleaned.Length > 9 && int.TryParse(cleaned[^9..], out var truncatedCode))
            return truncatedCode;

        return 1;
    }

    public decimal CalculateMontFact(Invoice invoice)
    {
        return invoice.Lines.Sum(l => l.Quantity * l.UnitPriceDA);
    }

    public decimal CalculateMontAs(Invoice invoice, decimal reimbursementRate = 0.70m)
    {
        var montFact = CalculateMontFact(invoice);
        return Math.Round(montFact * reimbursementRate, 2);
    }

    public decimal CalculateMontMut(Invoice invoice, decimal reimbursementRate = 0.70m)
    {
        var montAs = CalculateMontAs(invoice, reimbursementRate);
        var montFact = CalculateMontFact(invoice);
        return Math.Round(montFact - montAs, 2);
    }

    public void ApplyChifaFieldsToInvoice(Invoice invoice, ChifaInvoiceRequest request, ChifaInvoiceResult result)
    {
        invoice.ChifaNumFact = result.ChifaNumFact;
        invoice.ChifaNumAssure = request.NumAssure;
        invoice.ChifaCodeCentre = request.CodeCentre;
        invoice.ChifaDateFact = request.DateSoin;
        invoice.ChifaMontFact = CalculateMontFact(invoice);
        invoice.ChifaMontAs = CalculateMontAs(invoice);
        invoice.ChifaMontMut = CalculateMontMut(invoice);
        invoice.ChifaDateFinMut = request.DateSoin.AddYears(1);
        invoice.ChifaLastUpdated = DateTime.UtcNow;
    }
}
