# Pruebas unitarias y cobertura

Este directorio contiene un proyecto xUnit independiente del proyecto web. Ambos
estan en `ProyectoArqSoft.sln`. El proyecto web excluye `tests/**` y `coverage/**`
de sus elementos para evitar compilar las pruebas o publicar los reportes.

## Flujo configurado

```text
xUnit + Microsoft.NET.Test.Sdk + xunit.runner.visualstudio
                         |
                    dotnet test
                         |
        coverlet.collector (XPlat Code Coverage)
                         |
             +-----------+-----------+
             |                       |
   coverage.cobertura.xml   coverage.opencover.xml
             |                       |
       ReportGenerator        SonarScanner (opcional)
             |                       |
       HTML + historial        dashboard SonarQube
```

xUnit define pruebas y aserciones; el SDK y el adaptador las ejecutan mediante
VSTest. XPlat Code Coverage es el nombre del recolector de Coverlet, no una
segunda herramienta. ReportGenerator presenta sus resultados. SonarQube importa
la cobertura: no ejecuta las pruebas.

Versiones fijadas: Microsoft.NET.Test.Sdk 17.14.1, xunit 2.9.3,
xunit.runner.visualstudio 3.1.4, coverlet.collector 6.0.4 y ReportGenerator 5.5.1.
La version del adaptador no tiene que coincidir con la del framework xUnit.

## Ejecutar desde la raiz del repositorio

Necesitas un SDK compatible con .NET 9 y los runtimes .NET/ASP.NET Core 9.
Esta configuracion se verifico con SDK 10.0.103 y runtimes 9.0.13.
La primera restauracion necesita acceso a NuGet. No necesitas levantar la web,
PostgreSQL ni SonarQube para ejecutar la prueba inicial.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tests\Invoke-Coverage.ps1 -OpenReport
```

El script restaura ReportGenerator como herramienta local del repositorio,
restaura/compila el proyecto de pruebas, ejecuta las pruebas, recoge cobertura y
genera HTML. `-OpenReport` abre el resultado en el navegador; puedes omitirlo.
El script devuelve error si fallan las pruebas o la generacion del reporte.

Cada ejecucion guarda:

```text
coverage/
  runs/<fecha-hora>/
    TestResults/<identificador>/coverage.cobertura.xml
    TestResults/<identificador>/coverage.opencover.xml
    TestResults/tests.trx
    html/index.html
    html/Summary.txt
    html/Summary.json
  history/
```

No se mezclan coberturas de ejecuciones anteriores. `history/` permite mostrar
la evolucion en los nuevos HTML; los reportes anteriores se conservan.
La carpeta `coverage/` ya esta ignorada por Git: comparte los reportes aparte si
los necesitas como evidencia.

## Agregar pruebas y comparar

1. Crea archivos `*Tests.cs` dentro de `tests/ProyectoArqSoft.Tests/`, agrupados
   por funcionalidad si lo prefieres. Usa `[Fact]` para un caso y `[Theory]` con
   `[InlineData]` para varios datos de entrada.
2. Usa la prueba en `Helpers/StringHelperTests.cs` como ejemplo. Comprueba
   resultados observables con `Assert`; no hace falta registrar cada archivo.
3. Ejecuta nuevamente el mismo script y compara cobertura de lineas y ramas,
   tanto global como por clase, en el HTML y su historial.

Para ejecutar solo las pruebas:

```powershell
dotnet test .\tests\ProyectoArqSoft.Tests\ProyectoArqSoft.Tests.csproj
```

La medicion incluye el ensamblado de la aplicacion completo (incluye paginas
Razor), y excluye el ensamblado de pruebas y dependencias externas. No se
restringe al helper del ejemplo: la cobertura inicial sera muy baja. Mantengan
los mismos filtros y configuracion al comparar; cambiar el codigo tambien puede
cambiar el total de lineas. Una cobertura alta no garantiza buenas aserciones.

Referencia inicial verificada: 1 prueba correcta, 3 de 4790 lineas cubiertas
(aproximadamente 0,063 %) y 2 de 1854 ramas (aproximadamente 0,108 %).
ReportGenerator muestra 0 % de lineas con su precision actual; no significa
que no se haya ejecutado codigo. La clase StringHelper muestra 3,2 %.

Durante la verificacion aparecio un aviso de SonarScanner sobre un archivo
`SonarQube.Integration.targets` ausente en la configuracion local previa.
No impidio las pruebas ni los reportes. El envio al servidor SonarQube no se
ejecuto; requiere el servidor y un token vigente.

## Comandos individuales del flujo

Desde la raiz, ejecuta cada paso solo si el anterior termino correctamente:

```powershell
dotnet tool restore
$ejecucion = Join-Path 'coverage/runs' (Get-Date -Format 'yyyyMMdd-HHmmss-fff')
dotnet test .\tests\ProyectoArqSoft.Tests\ProyectoArqSoft.Tests.csproj --configuration Debug --collect:"XPlat Code Coverage" --settings .\tests\coverage.runsettings --results-directory "$ejecucion/TestResults"
dotnet tool run reportgenerator "-reports:$ejecucion/TestResults/*/coverage.cobertura.xml" "-targetdir:$ejecucion/html" "-reporttypes:Html;TextSummary;JsonSummary" "-historydir:coverage/history"
Start-Process "$ejecucion/html/index.html"
```

## SonarQube opcional

El flujo local de HTML ya funciona sin servidor. Para el dashboard necesitas
SonarQube iniciado y un token autorizado. `SONARQUBE.md` documenta el servidor
local y la clave de proyecto usados en este repositorio.

Para C#, se entrega el formato **OpenCover** usando
`sonar.cs.opencover.reportsPaths`, no el Cobertura directamente.
En PowerShell, con `SONAR_TOKEN` previamente configurado en el entorno, ejecuta
desde la raiz estos pasos uno a uno, continuando solo si el anterior tuvo exito:

```powershell
$sonarRun = 'coverage/sonar/' + (Get-Date -Format 'yyyyMMdd-HHmmss-fff')
dotnet sonarscanner begin /k:"VitalCare-farmacia-SIS312-Sept" /d:sonar.host.url="http://localhost:9000" /d:sonar.token="$env:SONAR_TOKEN" /d:sonar.cs.opencover.reportsPaths="$sonarRun/*/coverage.opencover.xml" /d:sonar.exclusions="coverage/**"
dotnet build .\ProyectoArqSoft.sln --configuration Debug --no-incremental
dotnet test .\tests\ProyectoArqSoft.Tests\ProyectoArqSoft.Tests.csproj --configuration Debug --no-build --collect:"XPlat Code Coverage" --settings .\tests\coverage.runsettings --results-directory $sonarRun
dotnet sonarscanner end /d:sonar.token="$env:SONAR_TOKEN"
```

El orden `begin -> build -> test con cobertura -> end` permite al scanner importar
el reporte de esa ejecucion. No guardes el token en el repositorio. El dashboard
es `http://localhost:9000/dashboard?id=VitalCare-farmacia-SIS312-Sept`.
Sus porcentajes pueden diferir del HTML por el alcance, exclusiones o la metrica
de codigo nuevo; compara siempre el mismo indicador.

## Referencias

- [Coverlet: integracion con VSTest y formatos](https://github.com/coverlet-coverage/coverlet/blob/master/Documentation/VSTestIntegration.md)
- [ReportGenerator](https://github.com/danielpalme/ReportGenerator)
- [SonarQube: cobertura .NET](https://docs.sonarsource.com/sonarqube-server/2025.5/analyzing-source-code/test-coverage/dotnet-test-coverage)
