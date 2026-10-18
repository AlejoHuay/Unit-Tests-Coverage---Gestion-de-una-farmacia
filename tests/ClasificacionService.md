# Pruebas de ClasificacionService

Clase: `ProyectoArqSoft.Application.Services.ClasificacionService`.
Pruebas: `tests/ProyectoArqSoft.Tests/Services/ClasificacionServiceTests.cs`.

Se siguen las pruebas de clientes y medicamentos: xUnit, Moq con
`MockBehavior.Strict`, nombres en espanol, resultados observables y llamadas
verificadas una sola vez. `VerifyNoOtherCalls` comprueba que no se ejecuten
operaciones adicionales. El repositorio y el validador estan simulados; estas
pruebas no utilizan PostgreSQL ni Supabase.

## Casos necesarios

| Metodo | Pruebas | Comportamientos comprobados |
| --- | ---: | --- |
| ObtenerTodos() | 1 | Devuelve exactamente la tabla del repositorio |
| ObtenerTodos(filtro) | 1 | Conserva el filtro recibido y devuelve la misma tabla |
| ObtenerPorId(id) | 1 | Envia el ID correcto y devuelve el mismo objeto |
| Crear(...) | 4 | Validacion rechazada; nombre activo duplicado; cero filas insertadas; exito |
| Actualizar(...) | 4 | Validacion rechazada; nombre de otra clasificacion; cero filas actualizadas; exito |
| EliminarLogicamente(...) | 3 | Medicamentos activos asociados; cero filas eliminadas; exito |

Son **14 metodos Fact y 14 casos ejecutados**. Las pruebas de exito de crear y
actualizar comprueban tambien los tres textos normalizados antes de validar,
el ID de la entidad, el usuario y que se persista el mismo objeto validado.
`MockSequence` verifica el orden validar -> consultar duplicados -> persistir.
En la actualizacion, la consulta de duplicados excluye expresamente el propio ID.
La eliminacion comprueba primero las asociaciones y envia ambos identificadores.

Los rechazos del validador deben devolver el mismo `Result`. Los demas errores
comprueban `IsSuccess` y su mensaje. Los mocks estrictos, las verificaciones y
la ausencia de configuraciones posteriores detectan escrituras o consultas que
no deberian ejecutarse despues de un rechazo.

## Seleccion sin redundancia

- Cero representa la rama `<= 0`; uno representa el exito. No se repiten los
  mismos caminos con filas negativas ni con otras cantidades positivas.
- Las consultas delegadas se prueban una vez por metodo. No se agrega un caso
  nulo a ObtenerPorId porque el servicio no decide segun ese resultado.
- El constructor y ConstruirClasificacion se ejercitan desde la API publica;
  no se invocan privados mediante reflexion ni se agregan pruebas duplicadas.
- Las reglas concretas del validador, la busqueda SQL de duplicados y las ramas
  internas de StringHelper pertenecen a otros componentes. No se vuelven a
  probar aqui. El validador simulado permite observar el mapeo del servicio.

## Complejidad ciclomatica por tres formulas

Conteo manual por metodo, con un grafo conectado (`P = 1`), una entrada y una
salida comun que recibe todos los retornos. Los numeros dependen de la division
en bloques indicada abajo, pero V(G) no cambia al subdividir una arista.

| Metodo | N | E | D | E - N + 2P | D + 1 | Regiones internas + exterior |
| --- | ---: | ---: | ---: | --- | --- | --- |
| Constructor | 3 | 2 | 0 | 2 - 3 + 2 = **1** | 0 + 1 = **1** | 0 + 1 = **1** |
| ObtenerTodos() | 4 | 3 | 0 | 3 - 4 + 2 = **1** | 0 + 1 = **1** | 0 + 1 = **1** |
| ObtenerTodos(filtro) | 4 | 3 | 0 | 3 - 4 + 2 = **1** | 0 + 1 = **1** | 0 + 1 = **1** |
| ObtenerPorId(id) | 4 | 3 | 0 | 3 - 4 + 2 = **1** | 0 + 1 = **1** | 0 + 1 = **1** |
| Crear(...) | 11 | 13 | 3 | 13 - 11 + 2 = **4** | 3 + 1 = **4** | 3 + 1 = **4** |
| Actualizar(...) | 11 | 13 | 3 | 13 - 11 + 2 = **4** | 3 + 1 = **4** | 3 + 1 = **4** |
| EliminarLogicamente(...) | 8 | 9 | 2 | 9 - 8 + 2 = **3** | 2 + 1 = **3** | 2 + 1 = **3** |
| ConstruirClasificacion(...) | 4 | 3 | 0 | 3 - 4 + 2 = **1** | 0 + 1 = **1** | 0 + 1 = **1** |

N son nodos; E, aristas; D, decisiones binarias. Las regiones corresponden a
una representacion plana del grafo e incluyen la exterior. No se suman las
decisiones de metodos llamados a las del metodo que se analiza. Los 14 casos
representan la suma de caminos base de los seis metodos publicos; no significan
que la complejidad de toda la clase sea 14.

## Nodos y caminos independientes

En cada consulta: 1 inicio, 2 llamada al repositorio, 3 retorno del resultado,
4 salida. Camino unico: **1 -> 2 -> 3 -> 4**.

Crear y Actualizar comparten la forma del grafo, pero usan operaciones distintas:

| Nodo | Operacion |
| --- | --- |
| 1 | Inicio |
| 2 | Construir la entidad, limpiar campos y asignar IdUsuario |
| 3 | Validar la entidad |
| 4 | Decidir si la validacion fue rechazada |
| 5 | Retornar el rechazo original |
| 6 | Consultar y decidir si existe nombre duplicado; en Actualizar se excluye el ID |
| 7 | Retornar error por duplicado |
| 8 | Insertar o actualizar y decidir si las filas afectadas son <= 0 |
| 9 | Retornar error de persistencia |
| 10 | Retornar exito |
| 11 | Salida comun |

Los cuatro caminos de cada metodo tienen una prueba propia:

1. Rechazo de validacion: **1 -> 2 -> 3 -> 4 -> 5 -> 11**.
2. Nombre duplicado: **1 -> 2 -> 3 -> 4 -> 6 -> 7 -> 11**.
3. Persistencia sin filas: **1 -> 2 -> 3 -> 4 -> 6 -> 8 -> 9 -> 11**.
4. Exito: **1 -> 2 -> 3 -> 4 -> 6 -> 8 -> 10 -> 11**.

En EliminarLogicamente: 1 inicio; 2 consultar y decidir si tiene medicamentos
activos; 3 retornar error por asociaciones; 4 construir la entidad con los dos
IDs; 5 eliminar y decidir si las filas son <= 0; 6 retornar error de eliminacion;
7 retornar exito; 8 salida comun.

1. Con asociaciones: **1 -> 2 -> 3 -> 8**.
2. Eliminacion sin filas: **1 -> 2 -> 4 -> 5 -> 6 -> 8**.
3. Exito: **1 -> 2 -> 4 -> 5 -> 7 -> 8**.

El constructor usa 1 inicio -> 2 asignaciones -> 3 salida. ConstruirClasificacion
usa 1 inicio -> 2 construir y limpiar -> 3 retornar entidad -> 4 salida.

## Resultados verificados el 16/10/2026

- Antes de agregar estas pruebas: **90 casos aprobados**.
- ClasificacionService: **14 casos aprobados**.
- Suite completa: **104 aprobados, 0 fallidos, 0 omitidos**.
- Clase: **61/61 lineas (100 %) y 16/16 ramas (100 %)**.
- Constructor: **7/7 lineas**; ConstruirClasificacion: **9/9 lineas**.
- Aplicacion completa: **13,7 % de lineas y 6,5 % de ramas**.
- Reporte HTML: `coverage/runs/20261016-214841-088/html/index.html`.
- Detalle: `coverage/runs/20261016-214841-088/html/ProyectoArqSoft_ClasificacionService.html`.

La cobertura se obtuvo con la configuracion compartida del repositorio, sin
restringir la medicion a esta clase. Una cobertura completa no garantiza que
no existan errores: se complementa con las aserciones de datos y llamadas.

## Commits progresivos y ejecucion

Se agrego una prueba en cada uno de **14 commits**. Antes de confirmar cada paso
se compilaron y ejecutaron todas las pruebas acumuladas de ClasificacionService:
de 1 a 14 casos aprobados. Este documento y su enlace se guardan en un commit
adicional. No se modifico codigo de produccion ni la configuracion de cobertura.

```powershell
git log --reverse --oneline cf22d3a^..5791459
dotnet test .\tests\ProyectoArqSoft.Tests\ProyectoArqSoft.Tests.csproj --filter FullyQualifiedName~ClasificacionServiceTests
powershell -NoProfile -ExecutionPolicy Bypass -File .\tests\Invoke-Coverage.ps1 -OpenReport
```

El reporte incluye la suite preexistente, incluidas sus pruebas PDF. Los
resultados TRX/XML y el HTML permanecen en `coverage/`, ignorado por Git.
