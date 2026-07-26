using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using BMPharma.CHIFA.Interfaces;
using BMPharma.Domain.Enums;

namespace BMPharma.CHIFA.Services;

public class FakeChifaIntegrationProvider : IChifaIntegrationService, IChifaInvoiceService, IChifaBordereauService, IChifaSigningService, IChifaTokenService
{
    private readonly ILogger<FakeChifaIntegrationProvider> _logger;

    private readonly ConcurrentDictionary<string, ChifaInvoiceRequest> _invoices = new();
    private readonly ConcurrentDictionary<string, ChifaBordereauRequest> _bordereaux = new();
    private readonly ConcurrentDictionary<string, ChifaSigningStatus> _signingStatuses = new();
    private readonly List<ChifaAuditEntry> _auditLog = new();
    private int _bordereauCounter = 0;
    private bool _isOnline = true;
    private bool _tokenPresent = false;

    public FakeChifaIntegrationProvider(ILogger<FakeChifaIntegrationProvider> logger)
    {
        _logger = logger;
    }

    public bool IsOnline { get => _isOnline; set => _isOnline = value; }
    public bool TokenPresent { get => _tokenPresent; set => _tokenPresent = value; }
    public IReadOnlyList<ChifaAuditEntry> AuditLog => _auditLog.AsReadOnly();
    public int InvoiceCount => _invoices.Count;
    public int BordereauCount => _bordereaux.Count;

    public void Reset()
    {
        _invoices.Clear();
        _bordereaux.Clear();
        _signingStatuses.Clear();
        _auditLog.Clear();
        _bordereauCounter = 0;
        _isOnline = true;
        _tokenPresent = false;
        _simulateSigningRequired = false;
        _simulateCloseFails = false;
    }

    public void SimulateOffline() => _isOnline = false;
    public void SimulateOnline() => _isOnline = true;
    public void SimulateTokenPresent() => _tokenPresent = true;
    public void SimulateTokenAbsent() => _tokenPresent = false;
    public int InvoicesCreated => _invoices.Count;

    public void SimulateInvoiceExists(string numFact)
    {
        _invoices[numFact] = new ChifaInvoiceRequest { NumFact = numFact, Lines = new List<ChifaInvoiceLineRequest>() };
    }

    public void SimulateInvoiceVisible(string numFact) => SimulateInvoiceExists(numFact);

    private bool _simulateSigningRequired = false;
    public void SimulateSigningRequired() => _simulateSigningRequired = true;
    public void SimulateSigningOk() => _simulateSigningRequired = false;

    private bool _simulateCloseFails = false;
    public void SimulateCloseFails() => _simulateCloseFails = true;
    public void SimulateCloseOk() => _simulateCloseFails = false;

    public Task<bool> IsChifaAvailableAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_isOnline);
    }

    public Task<ChifaHealthStatus> GetHealthStatusAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ChifaHealthStatus
        {
            IsOnline = _isOnline,
            IsDatabaseConnected = _isOnline,
            IsTokenPresent = _tokenPresent,
            ErrorMessage = _isOnline ? null : "Simulated offline mode",
            CheckedAt = DateTime.UtcNow
        });
    }

    public Task<ChifaInvoiceResult> CreateInvoiceAsync(ChifaInvoiceRequest request, CancellationToken cancellationToken = default)
    {
        var entry = new ChifaAuditEntry
        {
            Timestamp = DateTime.UtcNow,
            Operation = "CREATE_INVOICE",
            EntityType = "facture",
            EntityKey = request.NumFact,
            Source = "BM_PHARMA",
            Destination = "CHIFA_SIMULATION"
        };

        if (!_isOnline)
        {
            entry.Result = "ERROR";
            entry.Error = "Simulated: CHIFA is offline";
            _auditLog.Add(entry);
            return Task.FromResult(new ChifaInvoiceResult
            {
                Success = false,
                ErrorMessage = "Simulated: CHIFA database is offline"
            });
        }

        if (_invoices.ContainsKey(request.NumFact))
        {
            entry.Result = "ERROR";
            entry.Error = $"Simulated: Invoice {request.NumFact} already exists";
            _auditLog.Add(entry);
            return Task.FromResult(new ChifaInvoiceResult
            {
                Success = false,
                ErrorMessage = $"Simulated: Invoice {request.NumFact} already exists in CHIFA"
            });
        }

        _invoices[request.NumFact] = request;

        entry.Result = "SUCCESS";
        entry.Details = $"Lines: {request.Lines.Count}, NumFact: {request.NumFact}";
        _auditLog.Add(entry);

        _logger.LogInformation("[SIM] CHIFA invoice {NumFact} created (simulated)", request.NumFact);

        return Task.FromResult(new ChifaInvoiceResult
        {
            Success = true,
            ChifaNumFact = request.NumFact
        });
    }

    public Task<bool> InvoiceExistsInChifaAsync(string numFact, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_invoices.ContainsKey(numFact));
    }

    public Task<ChifaBordereauResult> CreateBordereauAsync(ChifaBordereauRequest request, CancellationToken cancellationToken = default)
    {
        var entry = new ChifaAuditEntry
        {
            Timestamp = DateTime.UtcNow,
            Operation = "CREATE_BORDEREAU",
            EntityType = "bordereau",
            EntityKey = request.NumBord,
            Source = "BM_PHARMA",
            Destination = "CHIFA_SIMULATION"
        };

        if (!_isOnline)
        {
            entry.Result = "ERROR";
            entry.Error = "Simulated: CHIFA is offline";
            _auditLog.Add(entry);
            return Task.FromResult(new ChifaBordereauResult
            {
                Success = false,
                ErrorMessage = "Simulated: CHIFA database is offline"
            });
        }

        if (_bordereaux.ContainsKey(request.NumBord))
        {
            entry.Result = "ERROR";
            entry.Error = $"Simulated: Bordereau {request.NumBord} already exists";
            _auditLog.Add(entry);
            return Task.FromResult(new ChifaBordereauResult
            {
                Success = false,
                ErrorMessage = $"Simulated: Bordereau {request.NumBord} already exists"
            });
        }

        foreach (var invNum in request.InvoiceNumbers)
        {
            if (!_invoices.ContainsKey(invNum))
            {
                entry.Result = "ERROR";
                entry.Error = $"Simulated: Invoice {invNum} not found in CHIFA";
                _auditLog.Add(entry);
                return Task.FromResult(new ChifaBordereauResult
                {
                    Success = false,
                    ErrorMessage = $"Simulated: Invoice {invNum} not found in CHIFA. Create invoice first."
                });
            }
        }

        _bordereaux[request.NumBord] = request;
        _signingStatuses[request.NumBord] = ChifaSigningStatus.SigningRequired;

        entry.Result = "SUCCESS";
        entry.Details = $"Bordereau: {request.NumBord}, Invoices: {request.InvoiceNumbers.Count}";
        _auditLog.Add(entry);

        _logger.LogInformation("[SIM] CHIFA bordereau {NumBord} created (simulated)", request.NumBord);

        return Task.FromResult(new ChifaBordereauResult
        {
            Success = true,
            NumBord = request.NumBord
        });
    }

    public Task<ChifaBordereauResult> SignBordereauAsync(string numBord, CancellationToken cancellationToken = default)
    {
        var entry = new ChifaAuditEntry
        {
            Timestamp = DateTime.UtcNow,
            Operation = "SIGN_BORDEREAU",
            EntityType = "bordereau",
            EntityKey = numBord,
            Source = "BM_PHARMA",
            Destination = "CHIFA_SIMULATION"
        };

        if (!_bordereaux.ContainsKey(numBord))
        {
            entry.Result = "ERROR";
            entry.Error = $"Bordereau {numBord} not found";
            _auditLog.Add(entry);
            return Task.FromResult(new ChifaBordereauResult
            {
                Success = false,
                ErrorMessage = $"Bordereau {numBord} not found in CHIFA"
            });
        }

        if (!_tokenPresent)
        {
            entry.Result = "BLOCKED";
            entry.Error = "Professional token not present. Signing requires CHIFA-OFFICINE.";
            _auditLog.Add(entry);
            return Task.FromResult(new ChifaBordereauResult
            {
                Success = false,
                ErrorMessage = "SIGNING BLOCKED: Professional token not present. Use CHIFA-OFFICINE with the physical token."
            });
        }

        if (_simulateSigningRequired)
        {
            entry.Result = "BLOCKED";
            entry.Error = "Simulated signing required failure";
            _auditLog.Add(entry);
            return Task.FromResult(new ChifaBordereauResult
            {
                Success = false,
                ErrorMessage = "Simulated: Signing requires CHIFA-OFFICINE with the physical token."
            });
        }

        _signingStatuses[numBord] = ChifaSigningStatus.Signed;

        entry.Result = "SUCCESS";
        entry.Details = $"Bordereau {numBord} signed (SIMULATED — real signing requires CHIFA-OFFICINE)";
        _auditLog.Add(entry);

        _logger.LogWarning("[SIM] CHIFA bordereau {NumBord} signed (SIMULATED — not real signing)", numBord);

        return Task.FromResult(new ChifaBordereauResult
        {
            Success = true,
            NumBord = numBord,
            SignatureId = $"SIM-{Guid.NewGuid():N}"[..12]
        });
    }

    public Task<ChifaBordereauResult> CloseBordereauAsync(string numBord, CancellationToken cancellationToken = default)
    {
        var entry = new ChifaAuditEntry
        {
            Timestamp = DateTime.UtcNow,
            Operation = "CLOSE_BORDEREAU",
            EntityType = "bordereau",
            EntityKey = numBord,
            Source = "BM_PHARMA",
            Destination = "CHIFA_SIMULATION"
        };

        if (!_bordereaux.ContainsKey(numBord))
        {
            entry.Result = "ERROR";
            entry.Error = $"Bordereau {numBord} not found";
            _auditLog.Add(entry);
            return Task.FromResult(new ChifaBordereauResult
            {
                Success = false,
                ErrorMessage = $"Bordereau {numBord} not found in CHIFA"
            });
        }

        if (_simulateCloseFails)
        {
            entry.Result = "BLOCKED";
            entry.Error = "Simulated close failure";
            _auditLog.Add(entry);
            return Task.FromResult(new ChifaBordereauResult
            {
                Success = false,
                ErrorMessage = "Simulated: Bordereau must be signed before closure"
            });
        }

        if (!_signingStatuses.TryGetValue(numBord, out var status) || status != ChifaSigningStatus.Signed)
        {
            entry.Result = "BLOCKED";
            entry.Error = "Bordereau must be signed before closure";
            _auditLog.Add(entry);
            return Task.FromResult(new ChifaBordereauResult
            {
                Success = false,
                ErrorMessage = "Cannot close bordereau: it must be signed first through CHIFA-OFFICINE"
            });
        }

        entry.Result = "SUCCESS";
        entry.Details = $"Bordereau {numBord} closed (SIMULATED)";
        _auditLog.Add(entry);

        _logger.LogWarning("[SIM] CHIFA bordereau {NumBord} closed (SIMULATED)", numBord);

        return Task.FromResult(new ChifaBordereauResult
        {
            Success = true,
            NumBord = numBord
        });
    }

    public Task<string> GetNextBordereauNumberAsync(CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref _bordereauCounter);
        return Task.FromResult(_bordereauCounter.ToString("D6"));
    }

    public Task<ChifaSigningStatus> GetSigningStatusAsync(string bordereauNumber, CancellationToken cancellationToken = default)
    {
        if (_signingStatuses.TryGetValue(bordereauNumber, out var status))
            return Task.FromResult(status);

        return Task.FromResult(ChifaSigningStatus.NotSigned);
    }

    public Task<bool> IsTokenAvailableAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_tokenPresent);
    }

    public Task<bool> IsTokenPresentAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_tokenPresent);
    }

    public Task<TokenInfo?> GetTokenInfoAsync(CancellationToken cancellationToken = default)
    {
        if (!_tokenPresent)
            return Task.FromResult<TokenInfo?>(null);

        return Task.FromResult<TokenInfo?>(new TokenInfo
        {
            SerialNumber = "SIM-TOKEN-001",
            Label = "Simulated Professional Token",
            ExpiryDate = DateTime.UtcNow.AddYears(1)
        });
    }

    public void SimulateSignBordereau(string numBord)
    {
        if (_bordereaux.ContainsKey(numBord))
        {
            _signingStatuses[numBord] = ChifaSigningStatus.Signed;
            _auditLog.Add(new ChifaAuditEntry
            {
                Timestamp = DateTime.UtcNow,
                Operation = "SIMULATE_SIGN",
                EntityType = "bordereau",
                EntityKey = numBord,
                Source = "SIMULATION",
                Destination = "CHIFA_SIMULATION",
                Result = "SIMULATED"
            });
        }
    }
}

public class ChifaAuditEntry
{
    public DateTime Timestamp { get; set; }
    public string Operation { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public string? EntityKey { get; set; }
    public string? Source { get; set; }
    public string? Destination { get; set; }
    public string Result { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? Error { get; set; }
}
