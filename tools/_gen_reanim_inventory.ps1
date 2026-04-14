$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$root = Join-Path $repoRoot 'Assets\Reanim2UnityAnim'
$outFile = Join-Path $repoRoot 'docs\Reanim2UnityAnim-inventory.md'
$docDir = Split-Path $outFile
if (-not (Test-Path $docDir)) { New-Item -ItemType Directory -Path $docDir -Force | Out-Null }

$lines = New-Object System.Collections.Generic.List[string]
function L([string]$s) { [void]$lines.Add($s) }

L '# Reanim2UnityAnim resource index'
L ''
L 'Scope: `Assets/Reanim2UnityAnim` (no `.meta` files listed).'
L ''
L '## Top-level folders'
L ''
Get-ChildItem $root -Directory | Sort-Object Name | ForEach-Object { L ("- ``{0}/``" -f $_.Name) }
L ''
$rc = (Get-ChildItem $root -Recurse -File -Filter '*.reanim').Count
$pc = (Get-ChildItem $root -Recurse -File -Filter '*.png').Count
$jc = (Get-ChildItem $root -Recurse -File -Filter '*.jpg').Count
L '## Counts'
L ''
L '| Kind | Count |'
L '|------|-------|'
L ("| ``.reanim`` | {0} |" -f $rc)
L ("| ``.png`` | {0} |" -f $pc)
L ("| ``.jpg`` | {0} |" -f $jc)
L ''
L '## Per subfolder'
L ''
L '| Subfolder | .reanim | .png | .jpg |'
L '|-----------|---------|------|------|'
Get-ChildItem $root -Directory | Sort-Object Name | ForEach-Object {
    $d = $_.FullName
    $r = (Get-ChildItem $d -Recurse -File -Filter '*.reanim').Count
    $p = (Get-ChildItem $d -Recurse -File -Filter '*.png').Count
    $j = (Get-ChildItem $d -Recurse -File -Filter '*.jpg').Count
    L ("| ``{0}/`` | {1} | {2} | {3} |" -f $_.Name, $r, $p, $j)
}
L ''
L '## All .reanim files (relative to Reanim2UnityAnim/)'
L ''
$rootLen = $root.Length
Get-ChildItem $root -Recurse -File -Filter '*.reanim' |
    Sort-Object FullName |
    ForEach-Object {
        $rel = $_.FullName.Substring($rootLen).TrimStart('\')
        L ("- ``{0}``" -f ($rel -replace '\\','/'))
    }
L ''
L '## Image files by subfolder (filename only)'
L ''
L ("Total: **{0}** png, **{1}** jpg." -f $pc, $jc)
L ''
Get-ChildItem $root -Directory | Sort-Object Name | ForEach-Object {
    $dir = $_
    L ("### ``{0}/``" -f $dir.Name)
    L ''
    Get-ChildItem $dir.FullName -Recurse -File |
        Where-Object { $_.Extension -eq '.png' -or $_.Extension -eq '.jpg' } |
        Sort-Object FullName |
        ForEach-Object { L ("- ``{0}``" -f $_.Name) }
    L ''
}

[System.IO.File]::WriteAllLines($outFile, $lines, [System.Text.UTF8Encoding]::new($false))
Write-Host "Wrote $outFile ($($lines.Count) lines)"
