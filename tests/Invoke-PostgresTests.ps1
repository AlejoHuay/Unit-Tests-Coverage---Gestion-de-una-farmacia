[CmdletBinding()]
param(
    [string]$PostgresBin = 'C:\Program Files\PostgreSQL\17\bin',
    [switch]$Coverage,
    [switch]$OpenReport
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
foreach ($binary in @('initdb.exe', 'pg_ctl.exe')) {
    if (-not (Test-Path -LiteralPath (Join-Path $PostgresBin $binary))) {
        throw "No se encontro $binary en $PostgresBin. Indica -PostgresBin con PostgreSQL 17 o posterior."
    }
}
$temporaryRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
$clusterRoot = Join-Path $temporaryRoot ('venta-pg-tests-' + [Guid]::NewGuid().ToString('N'))
$dataPath = Join-Path $clusterRoot 'data'
$passwordFile = Join-Path $clusterRoot 'password.txt'
$serverLog = Join-Path $clusterRoot 'postgres.log'
$previousConnection = $env:VENTA_TEST_POSTGRES
$started = $false
$mayDelete = $true
New-Item -ItemType Directory -Path $clusterRoot | Out-Null
Push-Location $repoRoot
try {
    $password = [Guid]::NewGuid().ToString('N')
    [IO.File]::WriteAllText($passwordFile, $password)
    & (Join-Path $PostgresBin 'initdb.exe') -D $dataPath -U venta_test --auth=scram-sha-256 --encoding=UTF8 --locale=C --pwfile=$passwordFile
    if ($LASTEXITCODE -ne 0) { throw 'No se pudo inicializar PostgreSQL temporal.' }
    Remove-Item -LiteralPath $passwordFile

    $listener = [Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback, 0)
    $listener.Start()
    $port = $listener.LocalEndpoint.Port
    $listener.Stop()
    # Solo loopback; no utiliza los servicios ni bases PostgreSQL existentes.
    $mayDelete = $false
    & (Join-Path $PostgresBin 'pg_ctl.exe') -D $dataPath -l $serverLog -o "-h 127.0.0.1 -p $port" -w -t 30 start
    if ($LASTEXITCODE -ne 0) { throw "No se pudo iniciar PostgreSQL temporal. Log: $serverLog" }
    $started = $true
    $env:VENTA_TEST_POSTGRES = "Host=127.0.0.1;Port=$port;Database=postgres;Username=venta_test;Password=$password;Pooling=false;Timeout=5;Command Timeout=30"
    Write-Host "PostgreSQL temporal disponible en 127.0.0.1:$port. Cada caso crea su propia base."
    if ($Coverage) {
        & (Join-Path $PSScriptRoot 'Invoke-Coverage.ps1') -OpenReport:$OpenReport
    } else {
        dotnet test tests/ProyectoArqSoft.Tests/ProyectoArqSoft.Tests.csproj --filter 'Category=PostgresIntegration'
        if ($LASTEXITCODE -ne 0) { throw 'Fallaron las pruebas de integracion PostgreSQL.' }
    }
}
finally {
    $env:VENTA_TEST_POSTGRES = $previousConnection
    if ($started) {
        & (Join-Path $PostgresBin 'pg_ctl.exe') -D $dataPath -m fast -w -t 30 stop
        $mayDelete = $LASTEXITCODE -eq 0
    }
    # Verificar destino absoluto antes de borrar recursivamente; conservarlo si no se confirmo la parada.
    $resolvedCluster = [IO.Path]::GetFullPath($clusterRoot)
    if ($mayDelete -and (Split-Path -Parent $resolvedCluster).TrimEnd('\') -eq $temporaryRoot.TrimEnd('\') -and
        (Split-Path -Leaf $resolvedCluster) -match '^venta-pg-tests-[a-f0-9]{32}$') {
        Remove-Item -LiteralPath $resolvedCluster -Recurse -Force
    } else {
        Write-Warning "Se conserva el cluster temporal para revision: $resolvedCluster"
    }
    Pop-Location
}
