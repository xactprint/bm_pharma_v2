# BM-PHASE-012-E — Security Review

## ReadOnly Mode

```
ChifaIntegrationModeProvider.IsReadOnly
  ├── Guards all write operations in Facade
  ├── ChifaWriteGuard.EnsureWriteAllowedAsync() throws ChifaWriteBlockedException
  └── FakeChifaIntegrationProvider simulates all writes
```

✅ Effective — no write can bypass mode check at the facade level.

## WriteGuard

```csharp
public class ChifaWriteGuard
{
    public async Task EnsureWriteAllowedAsync() { ... }
    public async Task EnsureTestOrProductionAsync() { ... }
}
```

✅ Injected into all write services. Blocks writes in ReadOnly mode.

## Credentials

| Credential | Storage | Risk |
|---|---|---|
| `ConnectionString` in `CHIFA` config section | `IConfiguration` (appsettings.json or env) | ⚠️ **MEDIUM** — `Password=` in connection string |
| PostgreSQL user `pharm` (superuser) | Connection string | ⚠️ **HIGH** — superuser with trust auth in production |
| Database: CHIFA_OFFICINE | Connection string | Low — well-known |

**Recommendations:**
1. Replace trust auth with `md5` or `scram-sha-256` in production
2. Create restricted PostgreSQL role for BM Pharma writes (INSERT on facture + detail_fact only)
3. Move connection string to environment variable or User Secrets

## Logging

```
ILogger.LogError — exception details with stack trace
ILogger.LogWarning — health check failures, non-critical errors
```

✅ No credentials logged. Exception messages are user-friendly. Stack traces may leak internal paths — acceptable for on-premise deployment.

## Token

| Token | Type | Risk |
|---|---|---|
| Identiv uTrust 3512 | PKCS#11 hardware token | Managed by CHIFA-OFFICINE, not BM Pharma |
| Software token stub | ChifaTokenServiceStub | ✅ Simulation only — no real token in BM |

✅ Option D ensures BM Pharma never handles the real token.

## SQL Injection

All queries use EF Core LINQ with parameterized SQL. **No raw SQL, no string concatenation.** ✅

## Configuration Exposure

| Setting | Exposure | Risk |
|---|---|---|
| CHIFA:Mode | config file | Low — ReadOnly/Test/Production enum |
| CHIFA:ConnectionString | config file | ⚠️ See credentials above |
| Service IPs | config file | Low — on-premise network |

## Secrets Management

Currently: plain text in configuration. **No encryption, no secret store, no Key Vault.**

**Recommendation for production:**
- Use `dotnet user-secrets` for development
- Use environment variables or encrypted config for production
- Never commit connection strings to source control

## SAM / CNAS

Currently: no SAM integration, no CNAS transmission in BM Pharma.
CHIFA-OFFICINE handles all SAM-signed PKCS#7 operations and FTP transmission to CNAS at 41.111.149.250:21.

✅ Option D minimizes attack surface.
