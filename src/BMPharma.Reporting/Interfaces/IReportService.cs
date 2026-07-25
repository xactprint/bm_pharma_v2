namespace BMPharma.Reporting.Interfaces;

public interface IReportService
{
    Task<byte[]> GenerateDailySalesReportAsync(DateTime date, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateStockReportAsync(CancellationToken cancellationToken = default);
    Task<byte[]> GenerateInvoiceReportAsync(Guid invoiceId, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateBordereauReportAsync(string bordereauNumber, CancellationToken cancellationToken = default);
}
