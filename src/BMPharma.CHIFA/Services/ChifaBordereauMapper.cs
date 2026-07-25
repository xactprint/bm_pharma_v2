using BMPharma.CHIFA.Interfaces;
using BMPharma.Domain.Entities;

namespace BMPharma.CHIFA.Services;

public class ChifaBordereauMapper
{
    public ChifaBordereauRequest MapToChifaRequest(Bordereau bordereau, IEnumerable<Invoice> invoices)
    {
        if (bordereau == null)
            throw new ArgumentNullException(nameof(bordereau));

        var invoiceNumbers = invoices
            .Where(i => i.ChifaNumFact != null)
            .Select(i => i.ChifaNumFact!)
            .ToList();

        return new ChifaBordereauRequest
        {
            NumBord = MapNumBord(bordereau.BordereauNumber),
            TypeBord = bordereau.CnasType ?? "BORD_CNAS",
            DateBord = bordereau.BordereauDate,
            InvoiceNumbers = invoiceNumbers
        };
    }

    public string MapNumBord(string bordereauNumber)
    {
        if (string.IsNullOrWhiteSpace(bordereauNumber))
            return string.Empty;

        var cleaned = bordereauNumber.Replace("-", "").Replace(" ", "");

        if (cleaned.Length > ChifaBordereauValidator.MaxNumBordLength)
            return cleaned[^ChifaBordereauValidator.MaxNumBordLength..];

        return cleaned.PadLeft(ChifaBordereauValidator.MaxNumBordLength, '0');
    }

    public void ApplyChifaFieldsToBordereau(Bordereau bordereau, ChifaBordereauResult result)
    {
        bordereau.SignatureId = result.SignatureId;
    }
}
