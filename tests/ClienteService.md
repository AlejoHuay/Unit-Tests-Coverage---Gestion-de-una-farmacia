# Pruebas de ClienteService

Clase: `ProyectoArqSoft.Application.Services.ClienteService`.
Archivo: `tests/ProyectoArqSoft.Tests/Services/ClienteServiceTests.cs`.

Se mantienen xUnit, Moq estricto y nombres en espanol, como en las pruebas de
ventas y medicamentos. Cada prueba comprueba resultados y llamadas unicas;
`VerifyNoOtherCalls` detecta operaciones inesperadas. No se conecta a PostgreSQL.

## Casos y decisiones

| Metodo publico | Decisiones en su cuerpo | V(G) | Casos |
| --- | ---: | ---: | --- |
| ObtenerTodos() | 0 | 1 | Devuelve la misma tabla |
| ObtenerTodos(filtro) | 0 | 1 | Conserva filtro y tabla |
| ObtenerPorId(id) | 0 | 1 | Conserva ID y objeto devuelto |
| Crear(...) | 3 | 4 | Rechazo del validador; duplicado; fallo de insercion; exito |
| Actualizar(...) | 3 | 4 | Rechazo del validador; duplicado; fallo de actualizacion; exito |
| Eliminar(...) | 1 | 2 | Cero filas; una fila |

La complejidad se cuenta por metodo: decisiones binarias + 1. No se agregan
las decisiones de los metodos llamados a las del llamador. Tampoco debe
confundirse numero de casos de xUnit con complejidad ciclomatica.

Los caminos internos se prueban mediante los metodos publicos:

- `ConstruirCliente`: correo presente y correo nulo (`??`); V(G) = 2 al contar
  ese operador como decision.
- `AplicarConsumidorFinal`: bandera falsa conserva los datos; bandera verdadera
  establece `CF` y `Consumidor Final`; V(G) = 2.
- `LimpiarCampos`: sin decisiones propias, V(G) = 1. Se comprueban los tres
  campos normalizados en los exitos de crear y actualizar.
- `ValidarDuplicado`: CF evita consultar; tabla vacia; filas sin coincidencia
  exacta; mismo NIT y propio ID; mismo NIT y otro ID. Se comprueba tambien que
  encontrar el propio ID no detenga la busqueda de otro cliente duplicado.
  Contando `if`, `foreach`, `?.` y las dos condiciones del `&&` como decisiones
  binarias, V(G) = 6. Una de esas decisiones tiene una rama no alcanzable con
  los valores normales de DataRow, descrita abajo.
- Constructor: asignaciones sin decisiones, V(G) = 1; se ejercita en todas
  las pruebas.

## Evitar redundancia

Son 14 metodos de prueba y 15 casos ejecutados. La unica Theory usa dos entradas:
la bandera de consumidor final activa (transformacion de datos) y un NIT `cF`
sin esa bandera (comparacion de CF sin distinguir mayusculas). Ambos verifican
que no se consulten duplicados y que el correo nulo pase a cadena vacia.

No se repite cada rechazo con cantidades negativas: cero representa la rama
`<= 0`. No se repite una consulta delegada con null cuando el servicio no toma
ninguna decision sobre ese resultado. No se prueban privados por reflexion.

El validador es simulado: los datos con espacios permiten comprobar el orden
real del servicio (validar, limpiar y persistir). No afirman que el validador
real acepte esos datos. Sus reglas requieren pruebas de ese otro componente.

## Resultados verificados el 14/10/2026

- Antes del cambio: 75 casos aprobados.
- ClienteService: 15 casos aprobados.
- Suite completa: 90 aprobados, 0 fallidos, 0 omitidos.
- ClienteService: 100 % de lineas y 27/28 ramas (aproximadamente 96,4 %).
- Metodos publicos de ClienteService: todas sus ramas cubiertas.
- Aplicacion completa: 12,3 % de lineas y 5,7 % de ramas.
- HTML: `coverage/runs/20261014-174438-724/html/index.html`.

La rama pendiente corresponde a `row["nit"]?.ToString()` en
`ValidarDuplicado` (linea 141 del archivo revisado). DataRow representa la
ausencia de un valor mediante `DBNull.Value`, no null. La prueba de actualizacion
incluye ese caso real, cuyo ToString devuelve una cadena vacia, pero no activa
el salto de `?.` por null. No se modifica produccion ni se fabrica un caso
imposible para forzar 100 % de ramas. La cobertura y las aserciones son evidencia
complementaria; no demuestran por si solas ausencia de errores.

Se hicieron 14 commits progresivos, ejecutando todas las pruebas acumuladas de
la clase antes de cada commit. Este documento se guarda en un commit adicional.

```powershell
git log --reverse --oneline a8c9478^..c28b66f
```

## Ejecutar

En un checkout sin las copias locales duplicadas:

```powershell
dotnet test .\tests\ProyectoArqSoft.Tests\ProyectoArqSoft.Tests.csproj --filter FullyQualifiedName~ClienteServiceTests
powershell -NoProfile -ExecutionPolicy Bypass -File .\tests\Invoke-Coverage.ps1
```

### Particularidad del equipo usado para la verificacion

Hay dos carpetas sin commit que declaran otra vez la clase Clasificacion:
`Domain/Model/Clasificacion copy/` y `Domain/Model/Clasificacion copy 2/`.
La ejecucion normal falla con CS0101/CS0111 antes de llegar a las pruebas.
Esas copias se conservaron intactas y no forman parte de los commits.

Para verificar, se excluyeron exclusivamente esas copias de la compilacion
mediante un archivo temporal. El proyecto y los filtros de cobertura no se
modificaron. Mientras sigan presentes las copias, esta es la reproduccion:

```powershell
$targetPath = Join-Path $env:TEMP 'ClienteService-verificacion.targets'
@'
<Project>
  <ItemGroup>
    <Compile Remove="$(MSBuildProjectDirectory)/Domain/Model/Clasificacion copy/**/*.cs" />
    <Compile Remove="$(MSBuildProjectDirectory)/Domain/Model/Clasificacion copy 2/**/*.cs" />
  </ItemGroup>
</Project>
'@ | Set-Content -LiteralPath $targetPath -Encoding UTF8

$runDirectory = Join-Path 'coverage/runs' (Get-Date -Format 'yyyyMMdd-HHmmss-fff')
dotnet test .\tests\ProyectoArqSoft.Tests\ProyectoArqSoft.Tests.csproj --configuration Debug --collect:'XPlat Code Coverage' --settings tests/coverage.runsettings --results-directory "$runDirectory/TestResults" --logger 'trx;LogFileName=tests.trx' "-p:DirectoryBuildTargetsPath=$targetPath"
# Continuar solo si dotnet test termino correctamente.
dotnet tool restore
# Continuar solo si la restauracion termino correctamente.
dotnet tool run reportgenerator "-reports:$runDirectory/TestResults/*/coverage.cobertura.xml" "-targetdir:$runDirectory/html" '-reporttypes:Html;TextSummary;JsonSummary' '-historydir:coverage/history'
```

La suite incluye las pruebas de integracion PDF preexistentes. Los reportes de
coverage estan ignorados por Git y se comparten por separado.
