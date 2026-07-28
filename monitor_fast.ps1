$outfile = "C:\Users\pc\Desktop\bm_stock\BMPharma\queries_final_capture.txt"
Remove-Item $outfile -ErrorAction SilentlyContinue

# Load Npgsql
Add-Type -Path "D:\Download\Softwares\CHIFA_OFFICINE\Npgsql.dll"

$connStr = "Server=127.0.0.1;Port=5432;Database=CHIFA_OFFICINE;User ID=pharm;"
$conn = New-Object Npgsql.NpgsqlConnection($connStr)
$conn.Open()

"=== Capture started: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss.fff') ===" | Out-File $outfile
Write-Host "Fast capture started - monitoring ALL pharm connections"

$prevQueries = @{}
$prevQStarts = @{}
$start = Get-Date
$timeout = 180 # seconds

# Poll as fast as possible using Npgsql
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT pid, state, coalesce(query_start::text,''), left(query,1000) as q FROM pg_stat_activity WHERE usename = 'pharm' AND pid != pg_backend_pid() ORDER BY pid"

while (((Get-Date) - $start).TotalSeconds -lt $timeout) {
    try {
        $reader = $cmd.ExecuteReader()
        while ($reader.Read()) {
            $pid = $reader["pid"].ToString()
            $state = $reader["state"].ToString()
            $qstart = $reader["coalesce"].ToString()
            $query = $reader["q"].ToString()
            
            if ($query -eq "") { continue }
            
            $key = "pid_$pid"
            $prevQ = if ($prevQueries.ContainsKey($key)) { $prevQueries[$key] } else { "" }
            $prevQS = if ($prevQStarts.ContainsKey($key)) { $prevQStarts[$key] } else { "" }
            
            # Detect ANY query change (new query or new query_start)
            if ($query -ne $prevQ -or ($qstart -ne "" -and $qstart -ne $prevQS)) {
                $ts = Get-Date -Format "HH:mm:ss.fff"
                
                if ($query -ne $prevQ) {
                    if ($query -ne "unlisten *") {
                        $msg = "[$ts] PID=$pid State=$state QStart=$qstart`n  ** $query"
                        Write-Host $msg -ForegroundColor Green
                    } else {
                        $msg = "[$ts] PID=$pid State=$state (last was: unwatch)" 
                    }
                    $msg | Out-File -Append $outfile
                }
                
                $prevQueries[$key] = $query
                $prevQStarts[$key] = $qstart
            }
        }
        $reader.Close()
    } catch {
        Write-Host "Error: $_" -ForegroundColor Red
    }
}

$conn.Close()
"=== Capture ended: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss.fff') ===" | Out-File -Append $outfile

Write-Host "`n=== CAPTURE RESULTS ==="
Get-Content $outfile
