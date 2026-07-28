$pgbin = "D:\Download\Softwares\CHIFA_OFFICINE\CHIFA_OFFICINE_DB\bin"
$outfile = "C:\Users\pc\Desktop\bm_stock\BMPharma\queries_captured_3.txt"
Remove-Item $outfile -ErrorAction SilentlyContinue

$prevQueries = @{}
$newConns = @{}

"=== Capture demarree: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') ===" | Out-File $outfile
"" | Out-File -Append $outfile

# Capture for 300 seconds (5 min)
$endTime = (Get-Date).AddSeconds(300)
while ((Get-Date) -lt $endTime) {
    # Get ALL pharm connections and their queries
    $result = & "$pgbin\psql.exe" -h 127.0.0.1 -U pharm -d CHIFA_OFFICINE -t -A -c @"
SELECT p.pid, p.state, 
       coalesce(p.query_start::text,''),
       coalesce(p.xact_start::text,''),
       coalesce(p.backend_start::text,''),
       left(p.query, 500)
FROM pg_stat_activity p
WHERE p.usename = 'pharm'
  AND p.pid != pg_backend_pid()
ORDER BY p.pid;
"@ 2>&1

    foreach ($line in $result) {
        if (-not $line) { continue }
        $parts = $line -split '\|'
        if ($parts.Count -lt 6) { continue }
        
        $procId = $parts[0].Trim()
        $st = $parts[1].Trim()
        $qStart = $parts[2].Trim()
        $xStart = $parts[3].Trim()
        $bStart = $parts[4].Trim()
        $q = $parts[5].Trim()
        
        # Track new connections
        if (-not $newConns.ContainsKey($procId)) {
            $ts = Get-Date -Format "HH:mm:ss.fff"
            $msg = "[$ts] NOUVEAU PID=$procId BackendStart=$bStart Query=$q"
            Write-Host $msg -ForegroundColor Cyan
            $msg | Out-File -Append $outfile
            $newConns[$procId] = $true
        }
        
        $key = "pid_$procId"
        $prevQ = ""
        if ($prevQueries.ContainsKey($key)) { $prevQ = $prevQueries[$key] }
        
        # Detect query changes
        if ($q -ne $prevQ -and $q -ne "" -and $q -ne "unlisten *") {
            $ts = Get-Date -Format "HH:mm:ss.fff"
            $msg = "[$ts] NOUVELLE REQUETE PID=$procId State=$st QStart=$qStart`n  => $q"
            Write-Host $msg -ForegroundColor Green
            $msg | Out-File -Append $outfile
        }
        
        # Also log active queries
        if ($st -eq "active" -and $q -ne "unlisten *") {
            $ts = Get-Date -Format "HH:mm:ss.fff"
            $msg = "[$ts] *** ACTIVE *** PID=$procId Q=$q"
            Write-Host $msg -ForegroundColor Yellow
            $msg | Out-File -Append $outfile
        }
        
        $prevQueries[$key] = $q
    }
    
    Start-Sleep -Milliseconds 100
}

"--- Capture terminee: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') ---" | Out-File -Append $outfile
Get-Content $outfile
