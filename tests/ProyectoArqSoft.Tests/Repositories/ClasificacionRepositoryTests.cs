using System.Data;
using ProyectoArqSoft.Domain.Models;
using Xunit;

namespace ProyectoArqSoft.Tests.Repositories;

[Trait("Category", "PostgresIntegration")]
public class ClasificacionRepositoryTests
{
    [PostgresFact]
    public void Insert_GetById_Update_Delete_ConservaLosCambiosEsperados()
    {
        // Arrange
        using var db = new PostgresTestDatabase();
        var clasificacion = new Clasificacion("Dermocosmetica", "Sintetico", "Cuidado de la piel")
        {
            IdUsuario = 2
        };

        // Act
        int filasInsertadas = db.ClasificacionRepository.Insert(clasificacion);
        int id = db.Scalar<int>("SELECT id FROM clasificacion WHERE nombre='Dermocosmetica'");
        var clasificacionInsertada = db.ClasificacionRepository.GetById(id);

        // Assert
        Assert.Equal(1, filasInsertadas);
        var insertada = Assert.IsType<Clasificacion>(clasificacionInsertada);
        Assert.Equal("Sintetico", insertada.Origen);
        Assert.Equal("Cuidado de la piel", insertada.Descripcion);
        Assert.Equal((short)1, insertada.Estado);
        Assert.NotEqual(default, insertada.FechaRegistro);
        Assert.Null(insertada.UltimaActualizacion);

        // Arrange
        insertada.Nombre = "Dermatologia";
        insertada.Origen = "Natural";
        insertada.Descripcion = "Productos dermatologicos";

        // Act
        int filasActualizadas = db.ClasificacionRepository.Update(insertada);
        var clasificacionActualizada = db.ClasificacionRepository.GetById(id);

        // Assert
        Assert.Equal(1, filasActualizadas);
        var actualizada = Assert.IsType<Clasificacion>(clasificacionActualizada);
        Assert.Equal("Dermatologia", actualizada.Nombre);
        Assert.Equal("Natural", actualizada.Origen);
        Assert.Equal("Productos dermatologicos", actualizada.Descripcion);
        Assert.NotNull(actualizada.UltimaActualizacion);

        // Act
        int filasEliminadas = db.ClasificacionRepository.Delete(actualizada);
        var clasificacionEliminada = db.ClasificacionRepository.GetById(id);
        var idsActivos = db.ClasificacionRepository.GetAll().Rows.Cast<DataRow>()
            .Select(row => Convert.ToInt32(row["id"])).ToArray();

        // Assert
        Assert.Equal(1, filasEliminadas);
        var eliminada = Assert.IsType<Clasificacion>(clasificacionEliminada);
        Assert.Equal((short)0, eliminada.Estado);
        Assert.DoesNotContain(id, idsActivos);
    }

    [PostgresFact]
    public void GetAll_FiltraOrdenaYExcluyeEliminadas()
    {
        // Arrange
        using var db = new PostgresTestDatabase();

        // Act
        var iniciales = db.ClasificacionRepository.GetAll();
        string[] nombresIniciales = iniciales.Rows.Cast<DataRow>()
            .Select(row => row["nombre"].ToString()!).ToArray();

        // Assert
        Assert.Equal(new[] { "Analgesicos" }, nombresIniciales);

        // Arrange
        var nueva = new Clasificacion("Vitaminas", "Natural", "Suplementos varios");

        // Act
        int filasInsertadas = db.ClasificacionRepository.Insert(nueva);
        var todas = db.ClasificacionRepository.GetAll();
        var filtradas = db.ClasificacionRepository.GetAll("vitamin");
        var filtroConTextoSql = db.ClasificacionRepository.GetAll("' OR 1=1 --");
        string[] nombresTodas = todas.Rows.Cast<DataRow>()
            .Select(row => row["nombre"].ToString()!).ToArray();

        // Assert
        Assert.Equal(1, filasInsertadas);
        Assert.Equal(new[] { "Analgesicos", "Vitaminas" }, nombresTodas);
        Assert.Equal(1, filtradas.Rows.Count);
        Assert.Equal(0, filtroConTextoSql.Rows.Count);

        // Arrange
        int id = db.Scalar<int>("SELECT id FROM clasificacion WHERE nombre='Vitaminas'");

        // Act
        int filasEliminadas = db.ClasificacionRepository.Delete(new Clasificacion { Id = id });
        var idsActivos = db.ClasificacionRepository.GetAll().Rows.Cast<DataRow>()
            .Select(row => Convert.ToInt32(row["id"])).ToArray();

        // Assert
        Assert.Equal(1, filasEliminadas);
        Assert.DoesNotContain(id, idsActivos);
    }

    [PostgresFact]
    public void Count_CuentaSoloClasificacionesActivas()
    {
        // Arrange
        using var db = new PostgresTestDatabase();

        // Act
        int cantidadInicial = db.ClasificacionRepository.Count();
        int filasEliminadas = db.ClasificacionRepository.Delete(new Clasificacion { Id = 1 });
        int cantidadDespuesDeEliminar = db.ClasificacionRepository.Count();

        // Assert
        Assert.Equal(1, filasEliminadas);
        Assert.Equal(1, cantidadInicial);
        Assert.Equal(0, cantidadDespuesDeEliminar);
    }

    [PostgresFact]
    public void TieneMedicamentosActivosAsociados_ConsideraSoloMedicamentosActivos()
    {
        // Arrange
        using var db = new PostgresTestDatabase();

        // Act
        bool tieneActivos = db.ClasificacionRepository.TieneMedicamentosActivosAsociados(1);

        // Assert
        Assert.True(tieneActivos);

        // Arrange
        db.Execute("UPDATE medicamento SET estado=0 WHERE id IN (1,2)");

        // Act
        bool tieneSoloInactivos = db.ClasificacionRepository.TieneMedicamentosActivosAsociados(1);
        bool clasificacionInexistenteTieneActivos =
            db.ClasificacionRepository.TieneMedicamentosActivosAsociados(9999);

        // Assert
        Assert.False(tieneSoloInactivos);
        Assert.False(clasificacionInexistenteTieneActivos);
    }

    [PostgresFact]
    public void ExisteNombreActivo_ComparaTextoNormalizado()
    {
        // Arrange
        using var db = new PostgresTestDatabase();

        // Act
        bool existeNombreEnMayusculas = db.ClasificacionRepository.ExisteNombreActivo("ANALGESICOS");
        bool existeNombreNuevo = db.ClasificacionRepository.ExisteNombreActivo("Vitaminas");

        // Assert
        Assert.True(existeNombreEnMayusculas);
        Assert.False(existeNombreNuevo);
    }

    [PostgresFact]
    public void ExisteNombreActivoExcluyendoId_ExcluyeSoloLaClasificacionIndicada()
    {
        // Arrange
        using var db = new PostgresTestDatabase();

        // Act
        bool coincideExcluyendoMismoId =
            db.ClasificacionRepository.ExisteNombreActivoExcluyendoId(1, "Analgesicos");
        bool coincideConOtroId =
            db.ClasificacionRepository.ExisteNombreActivoExcluyendoId(9999, "Analgesicos");
        bool coincideNombreNuevo =
            db.ClasificacionRepository.ExisteNombreActivoExcluyendoId(9999, "Vitaminas");

        // Assert
        Assert.False(coincideExcluyendoMismoId);
        Assert.True(coincideConOtroId);
        Assert.False(coincideNombreNuevo);
    }

    [PostgresFact]
    public void GetById_IdInexistente_DevuelveNull()
    {
        // Arrange
        using var db = new PostgresTestDatabase();

        // Act
        var clasificacion = db.ClasificacionRepository.GetById(9999);

        // Assert
        Assert.Null(clasificacion);
    }
}
