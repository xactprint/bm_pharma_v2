using Npgsql;
using System;

const string CONN = "Host=127.0.0.1;Port=5432;Database=CHIFA_OFFICINE;Username=pharm;Password=;SslMode=Disable;TrustServerCertificate=true";

using var conn = new NpgsqlConnection(CONN);
await conn.OpenAsync();

Console.WriteLine("=== COLUMNS WITH SPECIAL TYPES IN facture ===\n");

var sql = @"SELECT column_name, data_type, is_nullable
FROM information_schema.columns
WHERE table_name = 'facture'
AND data_type IN ('xml', 'timestamp with time zone', 'timestamp without time zone', 'date', 'boolean')
ORDER BY ordinal_position";

using (var cmd = new NpgsqlCommand(sql, conn))
using (var rd = await cmd.ExecuteReaderAsync())
{
    while (await rd.ReadAsync())
        Console.WriteLine($"  {rd["column_name"],-20} = {rd["data_type"],-30} nullable={rd["is_nullable"]}");
}

Console.WriteLine("\n=== ALL COLUMNS WITH DATA TYPES IN facture ===\n");

var sql2 = @"SELECT column_name, data_type FROM information_schema.columns
WHERE table_name = 'facture' ORDER BY ordinal_position";

using (var cmd2 = new NpgsqlCommand(sql2, conn))
using (var rd2 = await cmd2.ExecuteReaderAsync())
{
    while (await rd2.ReadAsync())
        Console.WriteLine($"  {rd2["column_name"],-25} {rd2["data_type"]}");
}
