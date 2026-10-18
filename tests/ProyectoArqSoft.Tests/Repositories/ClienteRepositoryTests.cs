using System.Data;
using ProyectoArqSoft.Domain.Models;
using Xunit;

namespace ProyectoArqSoft.Tests.Repositories;

[Trait("Category", "PostgresIntegration")]
public class ClienteRepositoryTests
{
    [PostgresFact]
    public void Insert_GetById_Update_Delete_ConservaLosCambiosEsperados()
    {
        // Arrange
        using var db = new PostgresTestDatabase();
        var cliente = new Cliente("333333", "Cliente Tres", "")
        {
            IdUsuario = 2
        };

        // Act
        int filasInsertadas = db.ClienteRepository.Insert(cliente);
        int id = db.Scalar<int>("SELECT id FROM cliente WHERE nit='333333'");
        var clienteInsertado = db.ClienteRepository.GetById(id);

        // Assert
        Assert.Equal(1, filasInsertadas);
        var insertado = Assert.IsType<Cliente>(clienteInsertado);
        Assert.Equal("Cliente Tres", insertado.RazonSocial);
        Assert.Equal(string.Empty, insertado.CorreoElectronico);
        Assert.Equal(2, insertado.IdUsuario);
        Assert.NotEqual(default, insertado.FechaRegistro);
        Assert.Null(insertado.UltimaActualizacion);

        // Arrange
        insertado.RazonSocial = "Cliente Actualizado";
        insertado.CorreoElectronico = "tres@example.test";

        // Act
        int filasActualizadas = db.ClienteRepository.Update(insertado);
        var clienteActualizado = db.ClienteRepository.GetById(id);

        // Assert
        Assert.Equal(1, filasActualizadas);
        var actualizado = Assert.IsType<Cliente>(clienteActualizado);
        Assert.Equal("Cliente Actualizado", actualizado.RazonSocial);
        Assert.Equal("tres@example.test", actualizado.CorreoElectronico);
        Assert.NotNull(actualizado.UltimaActualizacion);

        // Act
        int filasEliminadas = db.ClienteRepository.Delete(actualizado);
        var clienteEliminado = db.ClienteRepository.GetById(id);
        var clientesActivos = db.ClienteRepository.GetAll().Rows.Cast<DataRow>()
            .Select(row => Convert.ToInt32(row["id"])).ToArray();

        // Assert
        Assert.Equal(1, filasEliminadas);
        Assert.Null(clienteEliminado);
        Assert.DoesNotContain(id, clientesActivos);
    }

    [PostgresFact]
    public void Insert_SinCorreoNiUsuario_GuardaValoresOpcionalesComoNulos()
    {
        // Arrange
        using var db = new PostgresTestDatabase();
        var cliente = new Cliente("444444", "Cliente Cuatro", " ");

        // Act
        int filasInsertadas = db.ClienteRepository.Insert(cliente);
        int id = db.Scalar<int>("SELECT id FROM cliente WHERE nit='444444'");
        var clientePersistido = db.ClienteRepository.GetById(id);

        // Assert
        Assert.Equal(1, filasInsertadas);
        var persistido = Assert.IsType<Cliente>(clientePersistido);

        Assert.Equal(string.Empty, persistido.CorreoElectronico);
        Assert.Null(persistido.IdUsuario);
    }

    [PostgresFact]
    public void GetAll_FiltraOrdenaYNoIncluyeInactivos()
    {
        // Arrange
        using var db = new PostgresTestDatabase();

        // Act
        var todos = db.ClienteRepository.GetAll();
        var nombresTodos = todos.Rows.Cast<DataRow>()
            .Select(row => row["razon_social"].ToString()).ToArray();

        // Assert
        Assert.Equal(new[] { "Cliente Dos", "Cliente Uno" }, nombresTodos);

        // Act
        var filtrados = db.ClienteRepository.GetAll("111");
        var filtroConTextoSql = db.ClienteRepository.GetAll("' OR 1=1 --");

        // Assert
        Assert.Equal(1, filtrados.Rows.Count);
        Assert.Equal(0, filtroConTextoSql.Rows.Count);

        // Arrange
        int id = db.Scalar<int>("SELECT id FROM cliente WHERE nit='111'");

        // Act
        int filasEliminadas = db.ClienteRepository.Delete(new Cliente
        {
            IdCliente = id,
            IdUsuario = 2
        });
        var clientesActivos = db.ClienteRepository.GetAll().Rows.Cast<DataRow>()
            .Select(row => Convert.ToInt32(row["id"])).ToArray();

        // Assert
        Assert.Equal(1, filasEliminadas);
        Assert.DoesNotContain(id, clientesActivos);
    }

    [PostgresFact]
    public void GetById_IdInexistenteORowInactiva_DevuelveNull()
    {
        // Arrange
        using var db = new PostgresTestDatabase();
        int id = db.Scalar<int>("SELECT id FROM cliente WHERE nit='222'");
        db.Execute($"UPDATE cliente SET estado=0 WHERE id={id}");

        // Act
        var clienteInactivo = db.ClienteRepository.GetById(id);
        var clienteInexistente = db.ClienteRepository.GetById(9999);

        // Assert
        Assert.Null(clienteInactivo);
        Assert.Null(clienteInexistente);
    }

    [PostgresFact]
    public void Count_IncluyeRegistrosInactivos()
    {
        // Arrange
        using var db = new PostgresTestDatabase();

        // Act
        int cantidadInicial = db.ClienteRepository.Count();
        int filasEliminadas = db.ClienteRepository.Delete(new Cliente { IdCliente = 1, IdUsuario = 2 });
        int cantidadDespuesDeEliminar = db.ClienteRepository.Count();

        // Assert
        Assert.Equal(1, filasEliminadas);
        Assert.Equal(2, cantidadInicial);
        Assert.Equal(2, cantidadDespuesDeEliminar);
    }
}
