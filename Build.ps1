param([string]$Editor = 'E:\Program Files\6000.6.7f1\Editor\Unity.exe')
$ErrorActionPreference = 'Stop'
$projectDirectory = $PSScriptRoot
if (-not (Test-Path -LiteralPath $Editor)) { throw '请使用 -Editor 指定 Unity 6000.6.7f1 编辑器路径。' }
New-Item -ItemType Directory -Path (Join-Path $projectDirectory 'Logs') -Force | Out-Null
$buildArguments = @('-batchmode','-nographics','-projectPath',('"' + $projectDirectory + '"'),'-executeMethod','ClockworkCourier.Editor.ProjectBuild.BuildWindows','-quit','-logFile',('"' + (Join-Path $projectDirectory 'Logs/build-user.log') + '"'))
$buildProcess = Start-Process -FilePath $Editor -ArgumentList $buildArguments -WindowStyle Hidden -Wait -PassThru
if ($buildProcess.ExitCode -ne 0) { throw "Unity 构建失败，退出码 $($buildProcess.ExitCode)，请查看 Logs/build-user.log。" }
Write-Output (Join-Path $projectDirectory 'Builds/Windows/ClockworkCourier.exe')
