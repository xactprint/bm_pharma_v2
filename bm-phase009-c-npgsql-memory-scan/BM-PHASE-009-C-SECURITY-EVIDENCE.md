# BM-PHASE-009-C — Security Evidence

## SQL in Plaintext in Process Memory

All SQL queries found in the process memory of CHIFA-OFFICINE were stored as **plaintext strings**. Despite ConfuserEx obfuscation, the SQL command texts were not encrypted, not dynamically constructed, and not split across memory regions in any meaningful way that prevented reconstruction.

### Credentials Found

The following database credentials were observed in process memory:

```
Server=.; Port=5432; User Id=stock; Password=stock; Database=STOCK;
```

This connection string was observed in multiple memory locations, confirming that:

- PostgreSQL credentials are stored in plaintext
- The application uses a single, static database user (`stock`)
- The password (`stock`) is a simple, low-entropy value

### Implications

1. **Memory dumping risk**: Any process with sufficient privileges (including malware running under the same user account) can extract all SQL queries and database credentials from the running CHIFA process.

2. **No SQL encryption**: Despite ConfuserEx protection of the code flow, the string data itself is not protected. This renders the obfuscation ineffective for protecting business logic embodied in SQL queries.

3. **Credential exposure**: The PostgreSQL `stock` user has wide access to the `STOCK` database. A memory dump provides full database access credentials.

4. **Npgsql 2.0.14.3 limitations**: The old Npgsql version does not support encrypted command texts or any form of SQL masking.

### Observational vs. Exploitative

This investigation was purely **observational** (read-only). No credentials were used outside the CHIFA application context. No data was extracted, modified, or transmitted.

### Recommendations for BM Pharma

- Database credentials should not be stored or transmitted in plaintext
- Connection strings in CHIFA-OFFICINE configuration files should be reviewed
- The use of a single shared database user (`stock`) with static password should be replaced with more granular access control if BM Pharma assumes management of the PostgreSQL instance
