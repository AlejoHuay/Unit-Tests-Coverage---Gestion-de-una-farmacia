[CmdletBinding()]
param([switch]$OpenReport)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$runId = Get-Date -Format 'yyyyMMdd-HHmmss-fff'
$runDirectory = Join-Path $repoRoot "coverage/runs/$runId"
$resultsDirectory = Join-Path $runDirectory 'TestResults'
$reportDirectory = Join-Path $runDirectory 'html'
$historyDirectory = Join-Path $repoRoot 'coverage/history'

Push-Location $repoRoot
try {
    dotnet tool restore
    if ($LASTEXITCODE -ne 0) { throw 'No se pudo restaurar ReportGenerator.' }

    dotnet test tests/ProyectoArqSoft.Tests/ProyectoArqSoft.Tests.csproj --configuration Debug --collect:'XPlat Code Coverage' --settings tests/coverage.runsettings --results-directory $resultsDirectory --logger 'trx;LogFileName=tests.trx'
    $testExitCode = $LASTEXITCODE

    # El logger TRX puede copiar los adjuntos: usar solo la salida directa del collector.
    $coverageFiles = @(Get-ChildItem -LiteralPath $resultsDirectory -Filter coverage.cobertura.xml -Recurse -ErrorAction SilentlyContinue | Where-Object { $_.Directory.Parent.FullName -eq $resultsDirectory })
    if ($coverageFiles.Count -eq 0) { throw 'No se genero cobertura. Revisa la salida de dotnet test.' }
    $reports = ($coverageFiles.FullName -join ';')
    dotnet tool run reportgenerator "-reports:$reports" "-targetdir:$reportDirectory" '-reporttypes:Html;TextSummary;JsonSummary' "-historydir:$historyDirectory" "-tag:$runId"
    if ($LASTEXITCODE -ne 0) { throw 'No se pudo generar el reporte HTML.' }

    Get-Content -LiteralPath (Join-Path $reportDirectory 'Summary.txt') -TotalCount 23
    $indexPath = Join-Path $reportDirectory 'index.html'
    Write-Host "Reporte: $indexPath"
    if ($OpenReport) { Start-Process $indexPath }
    if ($testExitCode -ne 0) { throw 'Hay errores en las pruebas. Se genero el reporte disponible, pero la ejecucion no fue exitosa.' }
}
finally {
    Pop-Location
}
