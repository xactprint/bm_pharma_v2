using Npgsql;
using System;

Console.WriteLine("=== Final PostgreSQL TCP Verification ===\n");

int passed = 0, failed = 0;

void Test(string name, Action action)
{
    try { action(); Console.WriteLine($"  PASS: {name}"); passed++; }
    catch (Exception ex) { Console.WriteLine($"  FAIL: {name} - {ex.Message}"); failed++; }
}

// Test 1: pharm user (production connection string)
Test("pharm @ CHIFA_OFFICINE via TCP", () =>
{
    using var conn = new NpgsqlConnection("Host=127.0.0.1;Port=5432;Database=CHIFA_OFFICINE;Username=pharm;Password=;SslMode=Disable;Timeout=10;CommandTimeout=30");
    conn.Open();
    Console.WriteLine($"    State={conn.State}, Version={conn.ServerVersion}");
    using var cmd = new NpgsqlCommand("SELECT count(*) FROM medicament", conn);
    Console.WriteLine($"    medicament={cmd.ExecuteScalar()}");
    conn.Close();
});

// Test 2: postgres user (now granted)
Test("postgres @ CHIFA_OFFICINE via TCP", () =>
{
    using var conn = new NpgsqlConnection("Host=127.0.0.1;Port=5432;Database=CHIFA_OFFICINE;Username=postgres;Password=;SslMode=Disable;Timeout=10;CommandTimeout=30");
    conn.Open();
    Console.WriteLine($"    State={conn.State}, Version={conn.ServerVersion}");
    using var cmd = new NpgsqlCommand("SELECT count(*) FROM medicament", conn);
    Console.WriteLine($"    medicament={cmd.ExecuteScalar()}");
    conn.Close();
});

// Test 3: Query multiple tables (production scenario)
Test("Multi-table query as pharm", () =>
{
    using var conn = new NpgsqlConnection("Host=127.0.0.1;Port=5432;Database=CHIFA_OFFICINE;Username=pharm;Password=;SslMode=Disable;Timeout=10;CommandTimeout=30");
    conn.Open();
    string[] tables = { "medicament", "facture", "detail_fact", "bordereau", "parametre", "ln", "signature" };
    foreach (var t in tables)
    {
        using var cmd = new NpgsqlCommand($"SELECT count(*) FROM {t}", conn);
        Console.WriteLine($"    {t}: {cmd.ExecuteScalar()}");
    }
    conn.Close();
});

// Test 4: parametre data
Test("Read parametre data", () =>
{
    using var conn = new NpgsqlConnection("Host=127.0.0.1;Port=5432;Database=CHIFA_OFFICINE;Username=pharm;Password=;SslMode=Disable;Timeout=10;CommandTimeout=30");
    conn.Open();
    using var cmd = new NpgsqlCommand("SELECT * FROM parametre", conn);
    using var reader = cmd.ExecuteReader();
    while (reader.Read())
    {
        Console.WriteLine($"    {reader[0]} = {reader[1]}");
    }
    conn.Close();
});

Console.WriteLine($"\n=== Results: {passed} passed, {failed} failed ===");
Environment.ExitCode = failed > 0 ? 1 : 0;
