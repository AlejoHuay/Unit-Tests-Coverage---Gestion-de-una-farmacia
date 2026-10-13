using Moq;
using ProyectoArqSoft.Application.Interfaces;
using ProyectoArqSoft.Application.Ports.Output;
using ProyectoArqSoft.Application.Services;
using ProyectoArqSoft.Domain.DTOs;
using ProyectoArqSoft.Domain.Models;
using ProyectoArqSoft.Domain.Validators;
using Xunit;

namespace ProyectoArqSoft.Tests.Services;

public class VentaServiceActualizarTests
{
    private readonly Mock<IVentaRepository> _repository = new(MockBehavior.Strict);
    private readonly Mock<IMedicamentoRepository> _medicamentos = new(MockBehavior.Strict);
    private readonly Mock<IResult<Venta>> _validador = new(MockBehavior.Strict);

    [Fact]
    public void Actualizar_ConVentaValida_ActualizaLaVentaValidadaYConservaDatosOriginales()
    {
        PrepararVentaYMedicamento();
        var esperado = Result.Ok();
        Venta? validada = null;
        Venta? actualizada = null;
        _validador.Setup(v => v.Validar(It.IsAny<Venta>()))
            .Callback<Venta>(venta => validada = venta).Returns(Result.Ok());
        _repository.Setup(r => r.ActualizarVenta(It.IsAny<Venta>()))
            .Callback<Venta>(venta => actualizada = venta).Returns(esperado);

        var resultado = Servicio().Actualizar(17, 7, "  Tarjeta  ", Detalles(), 9);

        Assert.Same(esperado, resultado);
        Assert.NotNull(validada);
        Assert.Same(validada, actualizada);
        Assert.Equal(17, validada.Id);
        Assert.Equal(7, validada.IdCliente);
        Assert.Equal(3, validada.IdUsuario);
        Assert.Equal(9, validada.IdUsuarioEditor);
        Assert.Equal("Tarjeta", validada.MetodoPago);
        Assert.Equal(24.50m, validada.Total);
        var detalle = Assert.Single(validada.Detalles);
        Assert.Equal(8, detalle.IdMedicamento);
        Assert.Equal(2, detalle.Cantidad);
        // Conserva el precio anterior: no usa el precio actual ni el del DTO.
        Assert.Equal(12.25m, detalle.PrecioUnitario);
        VerificarLlamadas(construccionCompleta: true, valida: true, actualiza: true);
    }

    [Theory]
    [InlineData("null", "La venta no existe.")]
    [InlineData("id-cero", "La venta no existe.")]
    [InlineData("anulada", "No se puede modificar una venta anulada.")]
    public void Actualizar_ConVentaNoEditable_DevuelveFalloSinConstruirNiActualizar(
        string escenario, string errorEsperado)
    {
        Venta? existente = escenario switch
        {
            "null" => null,
            "id-cero" => new Venta { Id = 0 },
            _ => new Venta { Id = 17, Estado = 0 }
        };
        _repository.Setup(r => r.GetById(17)).Returns(existente);

        var resultado = Servicio().Actualizar(17, 7, "Tarjeta", Detalles(), 9);

        Assert.False(resultado.IsSuccess);
        Assert.Equal(errorEsperado, resultado.Error);
        VerificarLlamadas(construccionCompleta: false, valida: false, actualiza: false);
    }

    [Fact]
    public void Actualizar_ConValidacionRechazada_DevuelveElMismoRechazoSinActualizar()
    {
        PrepararVentaYMedicamento();
        var rechazo = Result.Fail("Cliente no valido.");
        _validador.Setup(v => v.Validar(It.IsAny<Venta>())).Returns(rechazo);

        var resultado = Servicio().Actualizar(17, 7, "Tarjeta", Detalles(), 9);

        Assert.Same(rechazo, resultado);
        VerificarLlamadas(construccionCompleta: true, valida: true, actualiza: false);
    }

    [Theory]
    [InlineData("consulta", "Error de consulta simulado.")]
    [InlineData("construccion", "Debe agregar al menos un medicamento.")]
    [InlineData("validacion", "Error de validacion simulado.")]
    [InlineData("actualizacion", "Error de actualizacion simulado.")]
    public void Actualizar_SiUnaEtapaLanzaExcepcion_DevuelveElMensajeYDetieneElFlujo(
        string etapa, string errorEsperado)
    {
        if (etapa == "consulta")
            _repository.Setup(r => r.GetById(17))
                .Throws(new InvalidOperationException(errorEsperado));
        else if (etapa == "construccion")
            _repository.Setup(r => r.GetById(17)).Returns(VentaExistente());
        else
        {
            PrepararVentaYMedicamento();
            if (etapa == "validacion")
                _validador.Setup(v => v.Validar(It.IsAny<Venta>()))
                    .Throws(new InvalidOperationException(errorEsperado));
            else
            {
                _validador.Setup(v => v.Validar(It.IsAny<Venta>())).Returns(Result.Ok());
                _repository.Setup(r => r.ActualizarVenta(It.IsAny<Venta>()))
                    .Throws(new InvalidOperationException(errorEsperado));
            }
        }

        // ConstruirVenta se ejecuta realmente; la lista vacia provoca su excepcion.
        var detalles = etapa == "construccion" ? new List<DetalleVentaInputDto>() : Detalles();
        var resultado = Servicio().Actualizar(17, 7, "Tarjeta", detalles, 9);

        Assert.False(resultado.IsSuccess);
        Assert.Equal(errorEsperado, resultado.Error);
        bool construccionCompleta = etapa is "validacion" or "actualizacion";
        VerificarLlamadas(construccionCompleta, valida: construccionCompleta,
            actualiza: etapa == "actualizacion");
    }

    private VentaService Servicio() => new(_repository.Object, _medicamentos.Object, _validador.Object);

    private static Venta VentaExistente() => new() { Id = 17, IdUsuario = 3, Estado = 1 };

    private static List<DetalleVentaInputDto> Detalles() => new()
    {
        new DetalleVentaInputDto { IdMedicamento = 8, Cantidad = 2, PrecioUnitario = 999m }
    };

    private void PrepararVentaYMedicamento()
    {
        _repository.Setup(r => r.GetById(17)).Returns(VentaExistente());
        _repository.Setup(r => r.GetDetallesByVentaId(17)).Returns(new List<DetalleVenta>
        {
            new() { IdVenta = 17, IdMedicamento = 8, Cantidad = 1, PrecioUnitario = 12.25m }
        });
        _medicamentos.Setup(r => r.GetById(8)).Returns(new Medicamento
        {
            Id = 8, Nombre = "Paracetamol", Precio = 20m, Stock = 10
        });
    }

    private void VerificarLlamadas(bool construccionCompleta, bool valida, bool actualiza)
    {
        _repository.Verify(r => r.GetById(17), Times.Once);
        _repository.Verify(r => r.GetDetallesByVentaId(17),
            construccionCompleta ? Times.Once() : Times.Never());
        _medicamentos.Verify(r => r.GetById(8),
            construccionCompleta ? Times.Once() : Times.Never());
        _validador.Verify(v => v.Validar(It.IsAny<Venta>()), valida ? Times.Once() : Times.Never());
        _repository.Verify(r => r.ActualizarVenta(It.IsAny<Venta>()),
            actualiza ? Times.Once() : Times.Never());
        _repository.VerifyNoOtherCalls();
        _medicamentos.VerifyNoOtherCalls();
        _validador.VerifyNoOtherCalls();
    }
}
