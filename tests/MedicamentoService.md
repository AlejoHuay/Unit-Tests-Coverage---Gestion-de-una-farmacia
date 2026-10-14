# Pruebas de MedicamentoService

Clase evaluada: `ProyectoArqSoft.Application.Services.MedicamentoService`.
Pruebas: `ProyectoArqSoft.Tests/Services/MedicamentoServiceTests.cs`.

Se usa xUnit y Moq con `MockBehavior.Strict`, siguiendo las pruebas existentes
de ventas. Se simulan el repositorio y los dos validadores; no se necesita una
base de datos. Cada prueba prepara sus dependencias, ejecuta un metodo publico
y comprueba resultados y llamadas. `VerifyNoOtherCalls` comprueba que no se
usen dependencias adicionales, especialmente despues de un rechazo.

## Caminos y complejidad ciclomatica

Conteo manual sobre el cuerpo de cada metodo del servicio: V(G) = decisiones
binarias + 1. Los `if` de estos metodos tienen condiciones simples. No se suman
las decisiones internas de repositorios, validadores ni helpers a las del
metodo que los llama. Esta tabla sirve de base para elaborar los grafos de flujo.

| Metodo | Decisiones | V(G) | Caminos comprobados |
| --- | ---: | ---: | --- |
| ObtenerTodos() | 0 | 1 | Devuelve la tabla del repositorio |
| ObtenerTodos(filtro) | 0 | 1 | Envia el filtro exacto y devuelve la tabla |
| ObtenerPorId(id) | 0 | 1 | Envia el ID y conserva el objeto recibido |
| ObtenerDestacados() | 0 | 1 | Devuelve la tabla de destacados |
| Crear(...) | 2 | 3 | Validacion rechazada; insercion fallida; exito |
| Actualizar(dto) | 2 | 3 | Validacion rechazada; actualizacion fallida; exito |
| EliminarLogicamente(...) | 1 | 2 | Eliminacion fallida; exito |
| UpdateStock(...) | 3 | 4 | Medicamento inexistente; validacion rechazada; persistencia fallida; exito |

Los ocho metodos publicos (contando ambas sobrecargas) tienen en conjunto
16 caminos base por metodo, representados por 16 metodos de prueba.
No es una afirmacion de que la complejidad de toda la clase sea 16: el
constructor y los metodos privados tambien tienen su propia complejidad.
El constructor, `ConstruirMedicamento` y `LimpiarCampos` no tienen decisiones
en sus cuerpos (V(G) = 1 cada uno) y se ejercitan desde las pruebas publicas.

## Seleccion de casos sin duplicacion

- Para `Insert`, `Update`, `Delete` y `UpdateStock`, cero representa el fallo
  `<= 0` y uno el exito. No se agregan negativos, que recorren la misma rama.
- No se duplica `ObtenerPorId` con un resultado nulo: el metodo solo delega,
  sin decidir segun el resultado. En cambio, el nulo de `UpdateStock` si tiene
  una rama propia y una prueba especifica.
- La prueba exitosa de stock es un `[Theory]` con `true` y `false`: ambos
  recorren el mismo camino, pero comprueban que el servicio conserve la
  direccion del movimiento. Detecta, por ejemplo, un booleano fijo al delegar.
  Por eso hay 17 casos ejecutados, aunque sean 16 metodos de prueba.
- Los exitos de crear y actualizar comprueban tambien limpieza de espacios,
  mapeo de campos, usuario y que se persista el mismo objeto validado.
- Los rechazos comprueban que se devuelva el mismo `Result` y no se persista.
- Las reglas internas de validacion y el calculo real de stock en la base de
  datos pertenecen a otros componentes; no se duplican aqui.

Cubrir ramas y caminos base no demuestra por si solo ausencia de errores.
Las aserciones de datos, resultados y ausencia de escrituras complementan
la medicion de cobertura.

## Ejecucion y evidencia

Solo este modulo:

```powershell
dotnet test .\tests\ProyectoArqSoft.Tests\ProyectoArqSoft.Tests.csproj --filter FullyQualifiedName~MedicamentoServiceTests
```

Suite completa y reporte:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tests\Invoke-Coverage.ps1
```

Resultado verificado el 14/10/2026:

- Modulo: 17 casos aprobados, ninguno fallido.
- Suite completa: 57 casos aprobados, ninguno fallido ni omitido.
- `MedicamentoService`: 100 % de lineas y 100 % de ramas.
- Aplicacion completa: 9,7 % de lineas y 3,8 % de ramas.
- HTML local: `coverage/runs/20261014-123556-716/html/index.html`.

Los porcentajes de clase se verificaron en `coverage.cobertura.xml`.
La suite completa incluye las pruebas de integracion de PDF preexistentes.
Los reportes estan ignorados por Git; deben compartirse aparte como evidencia.

Se crearon 16 commits progresivos de pruebas. Antes de cada commit se ejecuto
el conjunto acumulado de `MedicamentoServiceTests` y paso correctamente.
Para verlos en orden:

```powershell
git log --reverse --oneline 1652315^..f4173aa
```
