using System.Data;
using ProyectoArqSoft.Domain.Models;
using Xunit;

namespace ProyectoArqSoft.Tests.Repositories;

[Trait("Category", "PostgresIntegration")]
public class MedicamentoRepositoryTests
{
    [PostgresFact]
    public void Insert_GetById_Update_Delete_ConservaLosCambiosEsperados()
    {
        // Arrange
        using var db = new PostgresTestDatabase();
        var medicamento = new Medicamento("Amoxicilina", "Capsula", 1, "500 mg", 25m, 12)
        {
            IdUsuario = 2
        };

        // Act
        int filasInsertadas = db.MedicamentoRepository.Insert(medicamento);
        int id = db.Scalar<int>("SELECT id FROM medicamento WHERE nombre='Amoxicilina'");
        var insertado = db.MedicamentoRepository.GetById(id);

        // Assert
        Assert.Equal(1, filasInsertadas);
        var medicamentoInsertado = Assert.IsType<Medicamento>(insertado);
        Assert.Equal("Capsula", medicamentoInsertado.Presentacion);
        Assert.Equal(1, medicamentoInsertado.IdClasificacion);
        Assert.Equal("500 mg", medicamentoInsertado.Concentracion);
        Assert.Equal(25m, medicamentoInsertado.Precio);
        Assert.Equal(12, medicamentoInsertado.Stock);

        // Arrange
        medicamentoInsertado.Nombre = "Amoxicilina Plus";
        medicamentoInsertado.Presentacion = "Tableta";
        medicamentoInsertado.Concentracion = "875 mg";
        medicamentoInsertado.Precio = 30m;
        medicamentoInsertado.Stock = 20;

        // Act
        int filasActualizadas = db.MedicamentoRepository.Update(medicamentoInsertado);
        var medicamentoActualizado = db.MedicamentoRepository.GetById(id);

        // Assert
        Assert.Equal(1, filasActualizadas);
        var actualizado = Assert.IsType<Medicamento>(medicamentoActualizado);
        Assert.Equal("Amoxicilina Plus", actualizado.Nombre);
        Assert.Equal("Tableta", actualizado.Presentacion);
        Assert.Equal("875 mg", actualizado.Concentracion);
        Assert.Equal(30m, actualizado.Precio);
        Assert.Equal(20, actualizado.Stock);

        // Act
        int filasEliminadas = db.MedicamentoRepository.Delete(actualizado);
        var medicamentoEliminado = db.MedicamentoRepository.GetById(id);
        var idsActivos = db.MedicamentoRepository.GetAll().Rows.Cast<DataRow>()
            .Select(row => Convert.ToInt32(row["id"])).ToArray();
        short estadoGuardado = db.Scalar<short>($"SELECT estado FROM medicamento WHERE id={id}");

        // Assert
        Assert.Equal(1, filasEliminadas);
        Assert.Null(medicamentoEliminado);
        Assert.DoesNotContain(id, idsActivos);
        Assert.Equal((short)0, estadoGuardado);
    }

    [PostgresFact]
    public void GetAll_FiltraOrdenaIncluyeClasificacionYExcluyeInactivos()
    {
        // Arrange
        using var db = new PostgresTestDatabase();

        // Act
        var tabla = db.MedicamentoRepository.GetAll();
        var filtrados = db.MedicamentoRepository.GetAll("paracetamol");
        var filtroConTextoSql = db.MedicamentoRepository.GetAll("' OR 1=1 --");
        string[] nombres = tabla.Rows.Cast<DataRow>()
            .Select(row => row["nombre"].ToString()!).ToArray();

        // Assert
        Assert.Equal(new[] { "Ibuprofeno", "Paracetamol" }, nombres);
        Assert.All(tabla.Rows.Cast<DataRow>(), row => Assert.Equal("Analgesicos", row["clasificacion"]));
        Assert.Equal(1, filtrados.Rows.Count);
        Assert.Equal(0, filtroConTextoSql.Rows.Count);

        // Arrange
        db.Execute("UPDATE medicamento SET estado=0 WHERE id=1");

        // Act
        var activos = db.MedicamentoRepository.GetAll().Rows.Cast<DataRow>()
            .Select(row => Convert.ToInt32(row["id"])).ToArray();

        // Assert
        Assert.DoesNotContain(1, activos);
    }

    [PostgresFact]
    public void GetById_IdInexistente_DevuelveNull()
    {
        // Arrange
        using var db = new PostgresTestDatabase();

        // Act
        var medicamento = db.MedicamentoRepository.GetById(9999);

        // Assert
        Assert.Null(medicamento);
    }

    [PostgresFact]
    public void Count_IncluyeMedicamentosInactivos()
    {
        // Arrange
        using var db = new PostgresTestDatabase();

        // Act
        int cantidadInicial = db.MedicamentoRepository.Count();
        int filasEliminadas = db.MedicamentoRepository.Delete(new Medicamento { Id = 1, IdUsuario = 2 });
        int cantidadDespuesDeEliminar = db.MedicamentoRepository.Count();

        // Assert
        Assert.Equal(1, filasEliminadas);
        Assert.Equal(2, cantidadInicial);
        Assert.Equal(2, cantidadDespuesDeEliminar);
    }

    [PostgresFact]
    public void GetDestacados_DevuelveMedicamentosActivosConLasColumnasEsperadas()
    {
        // Arrange
        using var db = new PostgresTestDatabase();

        // Act
        var destacados = db.MedicamentoRepository.GetDestacados();
        string[] columnas = destacados.Columns.Cast<DataColumn>()
            .Select(column => column.ColumnName).ToArray();

        // Assert
        Assert.Equal(2, destacados.Rows.Count);
        Assert.Contains("nombre", columnas);
        Assert.Contains("presentacion", columnas);
        Assert.Contains("clasificacion", columnas);
        Assert.Contains("concentracion", columnas);
        Assert.Contains("precio", columnas);
    }

    [PostgresTheory]
    [InlineData(true, 5, 1, 1, 25)]
    [InlineData(false, 5, 1, 1, 15)]
    [InlineData(false, 26, 1, 0, 20)]
    [InlineData(true, 5, 9999, 0, 20)]
    public void UpdateStock_AplicaSoloEntradasValidas(
        bool esEntrada, int cantidad, int idMedicamento, int filasAfectadas, int stockEsperado)
    {
        // Arrange
        using var db = new PostgresTestDatabase();

        // Act
        int resultado = db.MedicamentoRepository.UpdateStock(idMedicamento, cantidad, esEntrada, 2);
        int stockActual = db.Scalar<int>("SELECT stock FROM medicamento WHERE id=1");

        // Assert
        Assert.Equal(filasAfectadas, resultado);
        Assert.Equal(stockEsperado, stockActual);
    }

    [PostgresFact]
    public void UpdateStock_NoModificaMedicamentoInactivo()
    {
        // Arrange
        using var db = new PostgresTestDatabase();
        db.Execute("UPDATE medicamento SET estado=0 WHERE id=1");

        // Act
        int filasActualizadas = db.MedicamentoRepository.UpdateStock(1, 3, true, 2);
        int stockActual = db.Scalar<int>("SELECT stock FROM medicamento WHERE id=1");

        // Assert
        Assert.Equal(0, filasActualizadas);
        Assert.Equal(20, stockActual);
    }
}
