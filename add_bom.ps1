param([string]$Target)
$bytes = [System.IO.File]::ReadAllBytes($Target)
if (-not ($bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF)) {
    $bom = [byte[]]@(0xEF, 0xBB, 0xBF)
    [System.IO.File]::WriteAllBytes($Target, $bom + $bytes)
    Write-Host "BOM added: $Target"
} else {
    Write-Host "BOM already present: $Target"
}