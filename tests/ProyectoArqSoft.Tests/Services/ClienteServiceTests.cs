using System.Data;
using Moq;
using ProyectoArqSoft.Application.Interfaces;
using ProyectoArqSoft.Application.Ports.Output;
using ProyectoArqSoft.Application.Services;
using ProyectoArqSoft.Domain.Models;
using ProyectoArqSoft.Domain.Validators;
using Xunit;

namespace ProyectoArqSoft.Tests.Services;

public class ClienteServiceTests
{

    [Fact]
    public void Actualizar_ConValidacionRechazada_DevuelveElRechazoSinConsultarNiActualizar()
    {
        var rechazo = Result.Fail("Cliente invalido.");
        _validador.Setup(v => v.Validar(It.IsAny<Cliente>())).Returns(rechazo);

        var resultado = CrearServicio().Actualizar(
            17, false, "123456", "Cliente", "cliente@correo.com", 9);

        Assert.Same(rechazo, resultado);
        _validador.Verify(v => v.Validar(It.IsAny<Cliente>()), Times.Once);
        VerificarSinOtrasLlamadas();
    }

    [Fact]
    public void Actualizar_ConNitDelMismoCliente_IgnoraOtrasFilasYActualizaLosDatosLimpios()
    {
        Cliente? validado = null;
        Cliente? actualizado = null;
        _validador.Setup(v => v.Validar(It.IsAny<Cliente>()))
            .Callback<Cliente>(c =>
            {
                Assert.Equal(" 123 456 ", c.Nit);
                Assert.Equal("  Razon   Editada  ", c.RazonSocial);
                Assert.Equal(" nuevo @correo.com ", c.CorreoElectronico);
                validado = c;
            }).Returns(Result.Ok());
        // DBNull y una coincidencia parcial no son duplicados; el propio ID tampoco.
        _repository.Setup(r => r.GetAll("123456"))
            .Returns(TablaClientes((21, null), (22, "1234567"), (17, " 123 456 ")));
        _repository.Setup(r => r.Update(It.IsAny<Cliente>()))
            .Callback<Cliente>(c => actualizado = c).Returns(1);

        var resultado = CrearServicio().Actualizar(
            17, false, " 123 456 ", "  Razon   Editada  ", " nuevo @correo.com ", 9);

        Assert.True(resultado.IsSuccess);
        Assert.NotNull(actualizado);
        Assert.Same(validado, actualizado);
        Assert.Equal(17, actualizado.IdCliente);
        Assert.Equal(9, actualizado.IdUsuario);
        Assert.Equal("123456", actualizado.Nit);
        Assert.Equal("Razon Editada", actualizado.RazonSocial);
        Assert.Equal("nuevo@correo.com", actualizado.CorreoElectronico);
        _validador.Verify(v => v.Validar(It.IsAny<Cliente>()), Times.Once);
        _repository.Verify(r => r.GetAll("123456"), Times.Once);
        _repository.Verify(r => r.Update(It.IsAny<Cliente>()), Times.Once);
        VerificarSinOtrasLlamadas();
    }

    [Fact]
    public void Crear_SinFilasInsertadas_DevuelveError()
    {
        _validador.Setup(v => v.Validar(It.IsAny<Cliente>())).Returns(Result.Ok());
        _repository.Setup(r => r.GetAll("123456")).Returns(TablaClientes());
        _repository.Setup(r => r.Insert(It.IsAny<Cliente>())).Returns(0);

        var resultado = CrearServicio().Crear(false, "123456", "Cliente", "cliente@correo.com", 9);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("No se pudo registrar el cliente.", resultado.Error);
        _validador.Verify(v => v.Validar(It.IsAny<Cliente>()), Times.Once);
        _repository.Verify(r => r.GetAll("123456"), Times.Once);
        _repository.Verify(r => r.Insert(It.IsAny<Cliente>()), Times.Once);
        VerificarSinOtrasLlamadas();
    }

    [Fact]
    public void Crear_ConNitDuplicado_DevuelveErrorSinInsertar()
    {
        _validador.Setup(v => v.Validar(It.IsAny<Cliente>())).Returns(Result.Ok());
        // La coincidencia debe ser exacta tras quitar espacios, no solo un filtro parecido.
        _repository.Setup(r => r.GetAll("123456"))
            .Returns(TablaClientes((17, " 123 456 ")));

        var resultado = CrearServicio().Crear(false, "123456", "Cliente", "cliente@correo.com", 9);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("Ya existe un cliente con ese NIT.", resultado.Error);
        _validador.Verify(v => v.Validar(It.IsAny<Cliente>()), Times.Once);
        _repository.Verify(r => r.GetAll("123456"), Times.Once);
        VerificarSinOtrasLlamadas();
    }

    [Fact]
    public void Crear_ConValidacionRechazada_DevuelveElRechazoSinConsultarNiInsertar()
    {
        var rechazo = Result.Fail("Cliente invalido.");
        _validador.Setup(v => v.Validar(It.IsAny<Cliente>())).Returns(rechazo);

        var resultado = CrearServicio().Crear(false, "123456", "Cliente", "cliente@correo.com", 9);

        Assert.Same(rechazo, resultado);
        _validador.Verify(v => v.Validar(It.IsAny<Cliente>()), Times.Once);
        VerificarSinOtrasLlamadas();
    }

    private static DataTable TablaClientes(params (int Id, string? Nit)[] filas)
    {
        var tabla = new DataTable();
        tabla.Columns.Add("id", typeof(int));
        tabla.Columns.Add("nit", typeof(string));
        foreach (var (id, nit) in filas)
            tabla.Rows.Add(id, (object?)nit ?? DBNull.Value);
        return tabla;
    }

    [Fact]
    public void Crear_ConDatosValidos_ValidaAntesDeLimpiarYRegistraElCliente()
    {
        Cliente? validado = null;
        Cliente? insertado = null;
        _validador.Setup(v => v.Validar(It.IsAny<Cliente>()))
            .Callback<Cliente>(c =>
            {
                // Capturar el estado al validar, antes de la limpieza del servicio.
                Assert.Equal(" 123 456 ", c.Nit);
                Assert.Equal("  Cliente   Prueba  ", c.RazonSocial);
                Assert.Equal(" cliente @correo.com ", c.CorreoElectronico);
                validado = c;
            }).Returns(Result.Ok());
        _repository.Setup(r => r.GetAll("123456")).Returns(TablaClientes());
        _repository.Setup(r => r.Insert(It.IsAny<Cliente>()))
            .Callback<Cliente>(c => insertado = c).Returns(1);

        var resultado = CrearServicio().Crear(
            false, " 123 456 ", "  Cliente   Prueba  ", " cliente @correo.com ", 9);

        Assert.True(resultado.IsSuccess);
        Assert.NotNull(insertado);
        Assert.Same(validado, insertado);
        Assert.Equal(0, insertado.IdCliente);
        Assert.Equal(9, insertado.IdUsuario);
        Assert.Equal("123456", insertado.Nit);
        Assert.Equal("Cliente Prueba", insertado.RazonSocial);
        Assert.Equal("cliente@correo.com", insertado.CorreoElectronico);
        _validador.Verify(v => v.Validar(It.IsAny<Cliente>()), Times.Once);
        _repository.Verify(r => r.GetAll("123456"), Times.Once);
        _repository.Verify(r => r.Insert(It.IsAny<Cliente>()), Times.Once);
        VerificarSinOtrasLlamadas();
    }

    [Fact]
    public void ObtenerPorId_EnviaElIdYDevuelveElClienteDelRepositorio()
    {
        var cliente = new Cliente { IdCliente = 17 };
        _repository.Setup(r => r.GetById(17)).Returns(cliente);

        var resultado = CrearServicio().ObtenerPorId(17);

        Assert.Same(cliente, resultado);
        _repository.Verify(r => r.GetById(17), Times.Once);
        VerificarSinOtrasLlamadas();
    }

    [Fact]
    public void ObtenerTodos_ConFiltro_EnviaElFiltroSinModificar()
    {
        const string filtro = "  Cliente de prueba  ";
        var tabla = new DataTable();
        _repository.Setup(r => r.GetAll(filtro)).Returns(tabla);

        var resultado = CrearServicio().ObtenerTodos(filtro);

        Assert.Same(tabla, resultado);
        _repository.Verify(r => r.GetAll(filtro), Times.Once);
        VerificarSinOtrasLlamadas();
    }
    private readonly Mock<IRepository<Cliente>> _repository = new(MockBehavior.Strict);
    private readonly Mock<IResult<Cliente>> _validador = new(MockBehavior.Strict);

    private ClienteService CrearServicio() => new(_repository.Object, _validador.Object);

    private void VerificarSinOtrasLlamadas()
    {
        _repository.VerifyNoOtherCalls();
        _validador.VerifyNoOtherCalls();
    }

    [Fact]
    public void ObtenerTodos_SinFiltro_DevuelveLaTablaDelRepositorio()
    {
        // Preparar.
        var tabla = new DataTable();
        _repository.Setup(r => r.GetAll()).Returns(tabla);

        // Actuar.
        var resultado = CrearServicio().ObtenerTodos();

        // Comprobar.
        Assert.Same(tabla, resultado);
        _repository.Verify(r => r.GetAll(), Times.Once);
        VerificarSinOtrasLlamadas();
    }
}
