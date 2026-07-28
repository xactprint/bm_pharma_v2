$pgbin = "D:\Download\Softwares\CHIFA_OFFICINE\CHIFA_OFFICINE_DB\bin"
$outfile = "C:\Users\pc\Desktop\bm_stock\BMPharma\queries_captured.txt"

# CHIFA connection PIDs observed earlier
$targetPids = @(6680, 12240)

# Track previous queries
$prev = @{}

"=== PostgreSQL Query Capture ===" | Out-File $outfile
"Started: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" | Out-File -Append $outfile
"Target PIDs: $($targetPids -join ', ')" | Out-File -Append $outfile
"" | Out-File -Append $outfile

while ($true) {
    $result = & "$pgbin\psql.exe" -h 127.0.0.1 -U pharm -d CHIFA_OFFICINE -t -A -c "
        SELECT pid, query, state, query_start, xact_start
        FROM pg_stat_activity
        WHERE pid IN ($($targetPids -join ','))
        ORDER BY pid;"
    
    foreach ($line in $result) {
        if (-not $line) { continue }
        $parts = $line -split '\|'
        if ($parts.Count -ge 2) {
            $pid = $parts[0]
            $query = $parts[1]
            $state = $parts[2]
            $qstart = $parts[3]
            $xstart = $parts[4]
            
            $key = "pid_$pid"
            $prevQuery = ""
            if ($prev.ContainsKey($key)) {
                $prevQuery = $prev[$key]
            }
            
            if ($query -ne $prevQuery -and $query -ne "unlisten *") {
                $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss.fff"
                $msg = "[$timestamp] PID=$pid State=$state QueryStart=$qstart XactStart=$xstart`nQuery: $query`n"
                Write-Host $msg -ForegroundColor Green
                $msg | Out-File -Append $outfile
            }
            
            $prev[$key] = $query
        }
    }
    
    Start-Sleep -Milliseconds 200
}
