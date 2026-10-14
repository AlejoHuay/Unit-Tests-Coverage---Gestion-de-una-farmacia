using System.Data;
using Moq;
using ProyectoArqSoft.Application.Interfaces;
using ProyectoArqSoft.Application.Ports.Output;
using ProyectoArqSoft.Application.Services;
using ProyectoArqSoft.Domain.DTOs;
using ProyectoArqSoft.Domain.Models;
using ProyectoArqSoft.Domain.Validators;
using Xunit;

namespace ProyectoArqSoft.Tests.Services;

public class MedicamentoServiceTests
{

    [Fact]
    public void UpdateStock_SinFilasActualizadas_DevuelveError()
    {
        _repository.Setup(r => r.GetById(17)).Returns(new Medicamento { Id = 17, Stock = 20 });
        _movimientoValidador.Setup(v => v.Validar(It.IsAny<MovimientoStockDto>())).Returns(Result.Ok());
        _repository.Setup(r => r.UpdateStock(17, 4, true, 9)).Returns(0);

        var resultado = CrearServicio().UpdateStock(17, 4, true, 9);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("No se pudo actualizar el stock del medicamento.", resultado.Error);
        _repository.Verify(r => r.GetById(17), Times.Once);
        _movimientoValidador.Verify(v => v.Validar(It.IsAny<MovimientoStockDto>()), Times.Once);
        _repository.Verify(r => r.UpdateStock(17, 4, true, 9), Times.Once);
        VerificarSinOtrasLlamadas();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void UpdateStock_ConMovimientoValido_EnviaLosDatosYDevuelveExito(bool esEntrada)
    {
        _repository.Setup(r => r.GetById(17)).Returns(new Medicamento { Id = 17, Stock = 20 });
        MovimientoStockDto? validado = null;
        _movimientoValidador.Setup(v => v.Validar(It.IsAny<MovimientoStockDto>()))
            .Callback<MovimientoStockDto>(m => validado = m).Returns(Result.Ok());
        _repository.Setup(r => r.UpdateStock(17, 4, esEntrada, 9)).Returns(1);

        var resultado = CrearServicio().UpdateStock(17, 4, esEntrada, 9);

        Assert.True(resultado.IsSuccess);
        Assert.NotNull(validado);
        Assert.Equal(17, validado.IdMedicamento);
        Assert.Equal(4, validado.Cantidad);
        Assert.Equal(esEntrada, validado.EsEntrada);
        Assert.Equal(9, validado.IdUsuario);
        Assert.Equal(20, validado.StockActual);
        _repository.Verify(r => r.GetById(17), Times.Once);
        _movimientoValidador.Verify(v => v.Validar(It.IsAny<MovimientoStockDto>()), Times.Once);
        _repository.Verify(r => r.UpdateStock(17, 4, esEntrada, 9), Times.Once);
        VerificarSinOtrasLlamadas();
    }

    [Fact]
    public void UpdateStock_ConValidacionRechazada_DevuelveElRechazoSinActualizar()
    {
        var rechazo = Result.Fail("Movimiento invalido.");
        _repository.Setup(r => r.GetById(17)).Returns(new Medicamento { Id = 17, Stock = 20 });
        _movimientoValidador.Setup(v => v.Validar(It.IsAny<MovimientoStockDto>())).Returns(rechazo);

        var resultado = CrearServicio().UpdateStock(17, 4, false, 9);

        Assert.Same(rechazo, resultado);
        _repository.Verify(r => r.GetById(17), Times.Once);
        _movimientoValidador.Verify(v => v.Validar(It.IsAny<MovimientoStockDto>()), Times.Once);
        VerificarSinOtrasLlamadas();
    }

    [Fact]
    public void UpdateStock_ConMedicamentoInexistente_FallaSinValidarNiActualizar()
    {
        _repository.Setup(r => r.GetById(17)).Returns((Medicamento?)null);

        var resultado = CrearServicio().UpdateStock(17, 4, true, 9);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("El medicamento no existe o no está activo.", resultado.Error);
        _repository.Verify(r => r.GetById(17), Times.Once);
        VerificarSinOtrasLlamadas();
    }

    [Fact]
    public void EliminarLogicamente_SinFilasAfectadas_DevuelveError()
    {
        _repository.Setup(r => r.Delete(It.Is<Medicamento>(
            m => m.Id == 17 && m.IdUsuario == 9))).Returns(0);

        var resultado = CrearServicio().EliminarLogicamente(17, 9);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("No se pudo eliminar el medicamento.", resultado.Error);
        _repository.Verify(r => r.Delete(It.Is<Medicamento>(
            m => m.Id == 17 && m.IdUsuario == 9)), Times.Once);
        VerificarSinOtrasLlamadas();
    }

    [Fact]
    public void EliminarLogicamente_ConFilasAfectadas_EnviaAmbosIdsYDevuelveExito()
    {
        _repository.Setup(r => r.Delete(It.Is<Medicamento>(
            m => m.Id == 17 && m.IdUsuario == 9))).Returns(1);

        var resultado = CrearServicio().EliminarLogicamente(17, 9);

        Assert.True(resultado.IsSuccess);
        _repository.Verify(r => r.Delete(It.Is<Medicamento>(
            m => m.Id == 17 && m.IdUsuario == 9)), Times.Once);
        VerificarSinOtrasLlamadas();
    }

    [Fact]
    public void Actualizar_SinFilasActualizadas_DevuelveError()
    {
        var entrada = new MedicamentoActualizarDto
        {
            Id = 17, Nombre = "Ibuprofeno", Presentacion = "Caja",
            IdClasificacion = 6, Concentracion = "400 mg",
            Precio = 18.75m, Stock = 32, IdUsuario = 9
        };
        _validador.Setup(v => v.Validar(It.IsAny<Medicamento>())).Returns(Result.Ok());
        _repository.Setup(r => r.Update(It.IsAny<Medicamento>())).Returns(0);

        var resultado = CrearServicio().Actualizar(entrada);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("No se pudo actualizar el medicamento.", resultado.Error);
        _validador.Verify(v => v.Validar(It.IsAny<Medicamento>()), Times.Once);
        _repository.Verify(r => r.Update(It.IsAny<Medicamento>()), Times.Once);
        VerificarSinOtrasLlamadas();
    }

    [Fact]
    public void Actualizar_ConValidacionRechazada_DevuelveElRechazoSinActualizar()
    {
        var rechazo = Result.Fail("Medicamento invalido.");
        _validador.Setup(v => v.Validar(It.IsAny<Medicamento>())).Returns(rechazo);
        var entrada = new MedicamentoActualizarDto { Id = 17, IdUsuario = 9 };

        var resultado = CrearServicio().Actualizar(entrada);

        Assert.Same(rechazo, resultado);
        _validador.Verify(v => v.Validar(It.IsAny<Medicamento>()), Times.Once);
        VerificarSinOtrasLlamadas();
    }

    [Fact]
    public void Actualizar_ConDatosValidos_LimpiaCamposYActualizaElMedicamentoValidado()
    {
        var entrada = new MedicamentoActualizarDto
        {
            Id = 17, Nombre = "  Ibuprofeno   Forte  ", Presentacion = "  Caja   x20  ",
            IdClasificacion = 6, Concentracion = "  400   mg  ",
            Precio = 18.75m, Stock = 32, IdUsuario = 9
        };
        Medicamento? validado = null;
        Medicamento? actualizado = null;
        _validador.Setup(v => v.Validar(It.IsAny<Medicamento>()))
            .Callback<Medicamento>(m => validado = m).Returns(Result.Ok());
        _repository.Setup(r => r.Update(It.IsAny<Medicamento>()))
            .Callback<Medicamento>(m => actualizado = m).Returns(1);

        var resultado = CrearServicio().Actualizar(entrada);

        Assert.True(resultado.IsSuccess);
        Assert.NotNull(validado);
        Assert.Same(validado, actualizado);
        Assert.Equal(17, validado.Id);
        Assert.Equal("Ibuprofeno Forte", validado.Nombre);
        Assert.Equal("Caja x20", validado.Presentacion);
        Assert.Equal(6, validado.IdClasificacion);
        Assert.Equal("400 mg", validado.Concentracion);
        Assert.Equal(18.75m, validado.Precio);
        Assert.Equal(32, validado.Stock);
        Assert.Equal(9, validado.IdUsuario);
        _validador.Verify(v => v.Validar(It.IsAny<Medicamento>()), Times.Once);
        _repository.Verify(r => r.Update(It.IsAny<Medicamento>()), Times.Once);
        VerificarSinOtrasLlamadas();
    }

    [Fact]
    public void Crear_SinFilasInsertadas_DevuelveError()
    {
        _validador.Setup(v => v.Validar(It.IsAny<Medicamento>())).Returns(Result.Ok());
        _repository.Setup(r => r.Insert(It.IsAny<Medicamento>())).Returns(0);

        var resultado = CrearServicio().Crear("Paracetamol", "Caja", 5, "500 mg", 12.50m, 20, 9);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("No se pudo registrar el medicamento.", resultado.Error);
        _validador.Verify(v => v.Validar(It.IsAny<Medicamento>()), Times.Once);
        _repository.Verify(r => r.Insert(It.IsAny<Medicamento>()), Times.Once);
        VerificarSinOtrasLlamadas();
    }

    [Fact]
    public void Crear_ConValidacionRechazada_DevuelveElRechazoSinInsertar()
    {
        var rechazo = Result.Fail("Medicamento invalido.");
        _validador.Setup(v => v.Validar(It.IsAny<Medicamento>())).Returns(rechazo);

        var resultado = CrearServicio().Crear("Paracetamol", "Caja", 5, "500 mg", 12.50m, 20, 9);

        Assert.Same(rechazo, resultado);
        _validador.Verify(v => v.Validar(It.IsAny<Medicamento>()), Times.Once);
        // Sin llamadas verificadas al repositorio: tampoco se permite insertar.
        VerificarSinOtrasLlamadas();
    }

    [Fact]
    public void Crear_ConDatosValidos_LimpiaCamposYRegistraElMedicamentoValidado()
    {
        Medicamento? validado = null;
        Medicamento? insertado = null;
        _validador.Setup(v => v.Validar(It.IsAny<Medicamento>()))
            .Callback<Medicamento>(m => validado = m).Returns(Result.Ok());
        _repository.Setup(r => r.Insert(It.IsAny<Medicamento>()))
            .Callback<Medicamento>(m => insertado = m).Returns(1);

        var resultado = CrearServicio().Crear(
            "  Paracetamol   Forte  ", "  Caja   x10  ", 5, "  500   mg  ", 12.50m, 20, 9);

        Assert.True(resultado.IsSuccess);
        Assert.NotNull(validado);
        Assert.Same(validado, insertado);
        Assert.Equal(0, validado.Id);
        Assert.Equal("Paracetamol Forte", validado.Nombre);
        Assert.Equal("Caja x10", validado.Presentacion);
        Assert.Equal(5, validado.IdClasificacion);
        Assert.Equal("500 mg", validado.Concentracion);
        Assert.Equal(12.50m, validado.Precio);
        Assert.Equal(20, validado.Stock);
        Assert.Equal(9, validado.IdUsuario);
        _validador.Verify(v => v.Validar(It.IsAny<Medicamento>()), Times.Once);
        _repository.Verify(r => r.Insert(It.IsAny<Medicamento>()), Times.Once);
        VerificarSinOtrasLlamadas();
    }

    [Fact]
    public void ObtenerDestacados_DevuelveLaTablaDelRepositorio()
    {
        var tabla = new DataTable();
        _repository.Setup(r => r.GetDestacados()).Returns(tabla);

        var resultado = CrearServicio().ObtenerDestacados();

        Assert.Same(tabla, resultado);
        _repository.Verify(r => r.GetDestacados(), Times.Once);
        VerificarSinOtrasLlamadas();
    }

    [Fact]
    public void ObtenerPorId_EnviaElIdYDevuelveElMedicamentoDelRepositorio()
    {
        var medicamento = new Medicamento { Id = 17 };
        _repository.Setup(r => r.GetById(17)).Returns(medicamento);

        var resultado = CrearServicio().ObtenerPorId(17);

        Assert.Same(medicamento, resultado);
        _repository.Verify(r => r.GetById(17), Times.Once);
        VerificarSinOtrasLlamadas();
    }

    [Fact]
    public void ObtenerTodos_ConFiltro_EnviaElFiltroSinModificar()
    {
        var tabla = new DataTable();
        const string filtro = "  Paracetamol  ";
        _repository.Setup(r => r.GetAll(filtro)).Returns(tabla);

        var resultado = CrearServicio().ObtenerTodos(filtro);

        Assert.Same(tabla, resultado);
        _repository.Verify(r => r.GetAll(filtro), Times.Once);
        VerificarSinOtrasLlamadas();
    }
    private readonly Mock<IMedicamentoRepository> _repository = new(MockBehavior.Strict);
    private readonly Mock<IResult<Medicamento>> _validador = new(MockBehavior.Strict);
    private readonly Mock<IResult<MovimientoStockDto>> _movimientoValidador = new(MockBehavior.Strict);

    private MedicamentoService CrearServicio() =>
        new(_repository.Object, _validador.Object, _movimientoValidador.Object);

    private void VerificarSinOtrasLlamadas()
    {
        _repository.VerifyNoOtherCalls();
        _validador.VerifyNoOtherCalls();
        _movimientoValidador.VerifyNoOtherCalls();
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
