param([string]$Version = '0.1.0')
$ErrorActionPreference = 'Stop'
$projectDirectory = $PSScriptRoot
$artifactDirectory = Join-Path $projectDirectory 'Artifacts'
$buildResultPath = Join-Path $projectDirectory 'Validation/build-result.json'
if (-not (Test-Path -LiteralPath $buildResultPath)) { throw '缺少构建结果，先完成构建。' }
$buildResult = Get-Content -LiteralPath $buildResultPath -Raw | ConvertFrom-Json
$productionRecord = Get-Content -LiteralPath (Join-Path $projectDirectory 'Provenance/production-start.json') -Raw | ConvertFrom-Json
if ($buildResult.result -ne 'Succeeded') { throw '构建未成功，不能打包为可运行版本。' }
$runtimeReport = Get-Content -LiteralPath (Join-Path $projectDirectory 'Validation/runtime-checks.txt')
if (@($runtimeReport | Where-Object { $_ -like 'FAIL*' }).Count -gt 0 -or @($runtimeReport | Where-Object { $_ -like 'PASS*' }).Count -lt 13) { throw '运行检查尚未全部通过。' }
New-Item -ItemType Directory -Path $artifactDirectory -Force | Out-Null
Add-Type -AssemblyName System.IO.Compression.FileSystem
$sourceRoots = @('Assets','Packages','ProjectSettings','Provenance','Validation','ThirdPartyNotices')
$sourceFiles = @()
foreach ($sourceRoot in $sourceRoots) {
    $directory = Join-Path $projectDirectory $sourceRoot
    if (Test-Path -LiteralPath $directory) { $sourceFiles += Get-ChildItem -LiteralPath $directory -File -Recurse }
}
$sourceFiles += Get-ChildItem -LiteralPath $projectDirectory -File | Where-Object { $_.Extension -in @('.md','.ps1','.cmd','.json') -or $_.Name -eq '.gitignore' }
$sourceFiles = @($sourceFiles | Sort-Object FullName -Unique)
$fileManifest = foreach ($sourceFile in $sourceFiles) {
    [pscustomobject]@{path=[IO.Path]::GetRelativePath($projectDirectory,$sourceFile.FullName).Replace('\','/');sha256=(Get-FileHash -LiteralPath $sourceFile.FullName -Algorithm SHA256).Hash.ToLowerInvariant();bytes=$sourceFile.Length}
}
$fileManifest | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $artifactDirectory 'source-files.json') -Encoding utf8
$sourceArchivePath = Join-Path $artifactDirectory ("U2-ClockworkCourier-" + $Version + '-source.zip')
if (Test-Path -LiteralPath $sourceArchivePath) { throw '源码归档已存在，请使用新的 Version，避免覆盖固定版本。' }
$sourceArchive = [IO.Compression.ZipFile]::Open($sourceArchivePath,[IO.Compression.ZipArchiveMode]::Create)
try {
    foreach ($sourceFile in $sourceFiles) {
        $entryPath = [IO.Path]::GetRelativePath($projectDirectory,$sourceFile.FullName).Replace('\','/')
        [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($sourceArchive,$sourceFile.FullName,$entryPath,[IO.Compression.CompressionLevel]::Optimal) | Out-Null
    }
} finally { $sourceArchive.Dispose() }
$windowsArchivePath = Join-Path $artifactDirectory ("U2-ClockworkCourier-" + $Version + '-windows.zip')
if (Test-Path -LiteralPath $windowsArchivePath) { throw '运行包已存在，请使用新的 Version。' }
[IO.Compression.ZipFile]::CreateFromDirectory((Join-Path $projectDirectory 'Builds/Windows'),$windowsArchivePath,[IO.Compression.CompressionLevel]::Optimal,$false)
$sourceHash = (Get-FileHash -LiteralPath $sourceArchivePath -Algorithm SHA256).Hash.ToLowerInvariant()
$windowsHash = (Get-FileHash -LiteralPath $windowsArchivePath -Algorithm SHA256).Hash.ToLowerInvariant()
$codeFiles = @(Get-ChildItem -LiteralPath (Join-Path $projectDirectory 'Assets/Scripts') -Filter '*.cs' -Recurse -File | Where-Object Name -ne 'RuntimeSmoke.cs')
$codeLines = 0
foreach ($codeFile in $codeFiles) { $codeLines += @(Get-Content -LiteralPath $codeFile.FullName | Where-Object { $_.Trim().Length -gt 0 }).Count }
$delivery = [ordered]@{
    project_id='U2';name='发条信使';supply_type='new_production';version=$Version;technology='Unity/C#/URP';project_type='single-player 2.5D action exploration game';complexity='multi-system complete-loop game';model='gpt-6-astra';provider='OpenAI';model_snapshot=$null;subscription=$null;session_id=$null;source_record='Provenance/production-start.json';source_archive=[IO.Path]::GetFileName($sourceArchivePath);source_sha256=$sourceHash;windows_archive=[IO.Path]::GetFileName($windowsArchivePath);windows_sha256=$windowsHash;subproject_path='U2_Unity_发条信使';build_status=$buildResult.result;runtime_report='Validation/runtime-checks.txt';user_playtest='not_tested';rights='pending_review';human_code_modifications='none recorded';duplicate_cluster='U2-clockwork-courier';cross_batch_deduplication='pending external inventory';raw_repo_count=1;independent_repo_count_within_current_task=1;project_count=1;procurement_accepted_count=0;runtime_source_files=$codeFiles.Count;nonblank_runtime_source_lines=$codeLines;code_count_method='Nonblank C# lines under Assets/Scripts, excluding RuntimeSmoke. This is a transparent size measure, not procurement-confirmed effective volume.';excluded_code='Unity engine, packages, generated meshes/materials/scenes, editor tooling, tests, caches and build outputs';created_at_utc=[DateTime]::UtcNow.ToString('O')
}
$delivery.session_id = $productionRecord.session_id
$delivery | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $artifactDirectory 'delivery-manifest.json') -Encoding utf8
$delivery | ConvertTo-Json -Compress -Depth 5 | Set-Content -LiteralPath (Join-Path $artifactDirectory 'batch.jsonl') -Encoding utf8
Write-Output "Source SHA-256: $sourceHash"
Write-Output "Windows SHA-256: $windowsHash"
