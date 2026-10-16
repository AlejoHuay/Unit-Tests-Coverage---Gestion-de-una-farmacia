using System.Data;
using ProyectoArqSoft.Domain.Models;
using Xunit;

namespace ProyectoArqSoft.Tests.Repositories;

[Trait("Category", "PostgresIntegration")]
public class VentaRepositoryTests
{
    private static Venta VentaNueva() => new()
    {
        IdCliente = 1, IdUsuario = 1, IdUsuarioEditor = 2, MetodoPago = "Efectivo", Total = 35m,
        Detalles = new List<DetalleVenta>
        {
            new() { IdMedicamento = 2, Cantidad = 3, PrecioUnitario = 5m },
            new() { IdMedicamento = 1, Cantidad = 2, PrecioUnitario = 10m }
        }
    };

    private static int Registrar(PostgresTestDatabase db)
    {
        var result = db.Repository.RegistrarVenta(VentaNueva());
        Assert.True(result.IsSuccess, result.Error);
        return db.Scalar<int>("SELECT max(id) FROM venta");
    }

    [PostgresFact]
    public void Consultas_BaseSinVentas_DevuelvenResultadosVacios()
    {
        using var db = new PostgresTestDatabase();
        Assert.Empty(db.Repository.GetAll().Rows.Cast<DataRow>());
        Assert.Null(db.Repository.GetById(999));
        Assert.Empty(db.Repository.GetDetallesByVentaId(999));
        Assert.Equal(0, db.Repository.Count());
    }

    [PostgresFact]
    public void RegistrarVenta_ConfirmaCabeceraDetallesStockYAuditoria()
    {
        using var db = new PostgresTestDatabase();
        int id = Registrar(db);
        var venta = Assert.IsType<Venta>(db.Repository.GetById(id));
        Assert.Equal(id, venta.Id);
        Assert.Equal(1, venta.IdCliente);
        Assert.Equal(1, venta.IdUsuario);
        Assert.Equal(35m, venta.Total);
        Assert.Equal("Efectivo", venta.MetodoPago);
        Assert.Equal("111", venta.Nit);
        Assert.Equal("Cliente Uno", venta.RazonSocial);
        Assert.Equal(1, venta.Estado);
        Assert.Null(venta.IdUsuarioEditor);
        Assert.Null(venta.UltimaActualizacion);
        Assert.NotEqual(default, venta.FechaHora);
        Assert.NotEqual(default, venta.FechaRegistro);
        Assert.Collection(db.Repository.GetDetallesByVentaId(id),
            d => { Assert.Equal(1, d.IdMedicamento); Assert.Equal(id, d.IdVenta); Assert.Equal(2, d.Cantidad); Assert.Equal(10m, d.PrecioUnitario); },
            d => { Assert.Equal(2, d.IdMedicamento); Assert.Equal(3, d.Cantidad); Assert.Equal(5m, d.PrecioUnitario); });
        Assert.Equal(18, db.Scalar<int>("SELECT stock FROM medicamento WHERE id=1"));
        Assert.Equal(27, db.Scalar<int>("SELECT stock FROM medicamento WHERE id=2"));
        Assert.Equal(1, db.Scalar<int>("SELECT count(*) FROM auditoria WHERE tabla='venta' AND operacion='INSERT' AND usuario_app_id=2"));
    }

    [PostgresFact]
    public void GetAll_OrdenaVentasPorFechaDescendente()
    {
        using var db = new PostgresTestDatabase();
        int antiguo = Registrar(db);
        int reciente = Registrar(db);
        db.Execute($"UPDATE venta SET fecha_hora='2020-01-01T00:00:00Z' WHERE id={antiguo}");
        db.Execute($"UPDATE venta SET fecha_hora='2021-01-01T00:00:00Z' WHERE id={reciente}");
        var tabla = db.Repository.GetAll();
        Assert.Equal(new[] { reciente, antiguo }, tabla.Rows.Cast<DataRow>().Select(r => Convert.ToInt32(r["id"])));
    }

    [PostgresFact]
    public async Task RegistrarVenta_DosOperacionesConcurrentes_NoVendenMasStockDelDisponible()
    {
        using var db = new PostgresTestDatabase();
        using var inicio = new System.Threading.Barrier(2);
        Task<ProyectoArqSoft.Domain.Validators.Result> Ejecutar() => Task.Run(() =>
        {
            var venta = VentaNueva();
            venta.Total = 150;
            venta.Detalles = new List<DetalleVenta> { new() { IdMedicamento=1, Cantidad=15, PrecioUnitario=10 } };
            if (!inicio.SignalAndWait(TimeSpan.FromSeconds(10))) throw new TimeoutException("Las operaciones no coincidieron.");
            return db.Repository.RegistrarVenta(venta);
        });
        var resultados = await Task.WhenAll(Ejecutar(), Ejecutar());
        Assert.Single(resultados, r => r.IsSuccess);
        var rechazo = Assert.Single(resultados, r => !r.IsSuccess);
        Assert.Equal("Stock insuficiente para el medicamento Paracetamol.", rechazo.Error);
        Assert.Equal(5, db.Scalar<int>("SELECT stock FROM medicamento WHERE id=1"));
        Assert.Equal(1, db.Repository.Count());
        Assert.Equal(1, db.Scalar<int>("SELECT count(*) FROM detalle_venta"));
    }

    [PostgresTheory]
    [InlineData("", 1)]
    [InlineData("efectivo", 1)]
    [InlineData("CLIENTE cajero", 1)]
    [InlineData("111", 1)]
    [InlineData("ausente", 0)]
    [InlineData("' OR 1=1 --", 0)]
    public void GetAll_FiltraConParametrosYSoloDevuelveVentasActivas(string filtro, int filas)
    {
        using var db = new PostgresTestDatabase();
        int activo = Registrar(db);
        int anulado = Registrar(db);
        Assert.True(db.Repository.AnularVentaLogicamente(anulado, 2).IsSuccess);
        var tabla = db.Repository.GetAll(filtro);
        Assert.Equal(filas, tabla.Rows.Count);
        if (filas > 0)
        {
            Assert.Equal(activo, Convert.ToInt32(tabla.Rows[0]["id"]));
            Assert.Equal("cajero", tabla.Rows[0]["usuario"]);
            Assert.Equal("111 - Cliente Uno", tabla.Rows[0]["cliente"]);
        }
        Assert.Equal(2, db.Repository.Count()); // Count incluye anuladas por contrato actual.
    }

    [PostgresTheory]
    [InlineData("cliente_ausente", "El cliente no existe o está inactivo.")]
    [InlineData("cliente_inactivo", "El cliente no existe o está inactivo.")]
    [InlineData("usuario_ausente", "El usuario que registra la venta no existe o está inactivo.")]
    [InlineData("usuario_inactivo", "El usuario que registra la venta no existe o está inactivo.")]
    [InlineData("medicamento_ausente", "El medicamento con id 999 no existe.")]
    [InlineData("medicamento_inactivo", "El medicamento Ibuprofeno esta inactivo.")]
    [InlineData("stock_cero", "El medicamento Ibuprofeno no tiene stock disponible.")]
    [InlineData("stock_insuficiente", "Stock insuficiente para el medicamento Ibuprofeno.")]
    public void RegistrarVenta_ValidacionFallida_NoPersisteCambios(string caso, string error)
    {
        using var db = new PostgresTestDatabase();
        var venta = VentaNueva();
        switch (caso)
        {
            case "cliente_ausente": venta.IdCliente = 999; break;
            case "cliente_inactivo": db.Execute("UPDATE cliente SET estado=0 WHERE id=1"); break;
            case "usuario_ausente": venta.IdUsuario = 999; break;
            case "usuario_inactivo": db.Execute("UPDATE usuario SET activo=0 WHERE id=1"); break;
            case "medicamento_ausente": venta.Detalles[0].IdMedicamento = 999; break;
            case "medicamento_inactivo": db.Execute("UPDATE medicamento SET estado=0 WHERE id=2"); break;
            case "stock_cero": db.Execute("UPDATE medicamento SET stock=0 WHERE id=2"); break;
            case "stock_insuficiente": venta.Detalles[0].Cantidad = 31; break;
        }
        string antes = db.Snapshot();
        var resultado = db.Repository.RegistrarVenta(venta);
        Assert.False(resultado.IsSuccess);
        Assert.Equal(error, resultado.Error);
        Assert.Equal(antes, db.Snapshot());
    }

    [PostgresTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void RegistrarVenta_FalloTrasEscrituras_RevierteTodaLaTransaccion(bool falloStock)
    {
        using var db = new PostgresTestDatabase();
        var venta = VentaNueva();
        if (falloStock)
        {
            // Cada linea pasa la prevalidacion, pero la segunda agota el stock acumulado.
            venta.Detalles = new List<DetalleVenta>
            {
                new() { IdMedicamento=1, Cantidad=15, PrecioUnitario=10 },
                new() { IdMedicamento=1, Cantidad=10, PrecioUnitario=10 }
            };
        }
        else venta.Detalles[1].PrecioUnitario = 0; // CHECK real despues de insertar el primer detalle.
        string antes = db.Snapshot();
        var resultado = db.Repository.RegistrarVenta(venta);
        Assert.False(resultado.IsSuccess);
        if (falloStock) Assert.Equal("No se pudo descontar stock del medicamento con id 1.", resultado.Error);
        else Assert.StartsWith("No se pudo registrar la venta.", resultado.Error);
        Assert.Equal(antes, db.Snapshot());
    }

    [PostgresFact]
    public void RegistrarVenta_SinDetalles_ReflejaContratoActualDelRepositorio()
    {
        using var db = new PostgresTestDatabase();
        var venta = VentaNueva();
        venta.Detalles.Clear();
        Assert.True(db.Repository.RegistrarVenta(venta).IsSuccess);
        Assert.Equal(1, db.Repository.Count());
        Assert.Empty(db.Repository.GetDetallesByVentaId(1));
        Assert.Equal(20, db.Scalar<int>("SELECT stock FROM medicamento WHERE id=1"));
        // La validacion de al menos un detalle pertenece a las capas superiores.
    }

    [PostgresFact]
    public void ActualizarVenta_ConservaPrecioHistoricoReemplazaDetallesYConfirmaStock()
    {
        using var db = new PostgresTestDatabase();
        int id = Registrar(db);
        db.Execute("UPDATE medicamento SET precio=99 WHERE id=1");
        db.Execute("INSERT INTO medicamento(id,nombre,presentacion,id_clasificacion,concentracion,precio,stock) VALUES(3,'Nuevo producto','Tableta',1,'100mg',7,10)");
        var venta = VentaNueva();
        venta.Id = id; venta.IdCliente = 2; venta.MetodoPago = "Tarjeta";
        venta.Detalles = new List<DetalleVenta>
        {
            new() { IdMedicamento=1, Cantidad=4, PrecioUnitario=999 },
            new() { IdMedicamento=3, Cantidad=2, PrecioUnitario=7 }
        };
        var resultado = db.Repository.ActualizarVenta(venta);
        Assert.True(resultado.IsSuccess, resultado.Error);
        var persistida = db.Repository.GetById(id)!;
        Assert.Equal(54m, persistida.Total);
        Assert.Equal(2, persistida.IdCliente);
        Assert.Equal(1, persistida.IdUsuario);
        Assert.Equal(2, persistida.IdUsuarioEditor);
        Assert.NotNull(persistida.UltimaActualizacion);
        Assert.Equal("222", persistida.Nit);
        Assert.Equal("Cliente Dos", persistida.RazonSocial);
        Assert.Equal("Tarjeta", persistida.MetodoPago);
        Assert.Collection(db.Repository.GetDetallesByVentaId(id),
            d => { Assert.Equal(1,d.IdMedicamento); Assert.Equal(4,d.Cantidad); Assert.Equal(10m,d.PrecioUnitario); },
            d => { Assert.Equal(3,d.IdMedicamento); Assert.Equal(2,d.Cantidad); Assert.Equal(7m,d.PrecioUnitario); });
        Assert.Equal(16, db.Scalar<int>("SELECT stock FROM medicamento WHERE id=1"));
        Assert.Equal(30, db.Scalar<int>("SELECT stock FROM medicamento WHERE id=2"));
        Assert.Equal(8, db.Scalar<int>("SELECT stock FROM medicamento WHERE id=3"));
    }

    [PostgresFact]
    public void ActualizarVenta_SinDetallesAnteriores_AgregaLosNuevos()
    {
        using var db = new PostgresTestDatabase();
        var inicial = VentaNueva(); inicial.Detalles.Clear();
        Assert.True(db.Repository.RegistrarVenta(inicial).IsSuccess);
        var venta = VentaNueva(); venta.Id=db.Scalar<int>("SELECT max(id) FROM venta");
        var resultado = db.Repository.ActualizarVenta(venta);
        Assert.True(resultado.IsSuccess, resultado.Error);
        Assert.Equal(2, db.Repository.GetDetallesByVentaId(venta.Id).Count);
        Assert.Equal(35m, db.Repository.GetById(venta.Id)!.Total);
        Assert.Equal(18, db.Scalar<int>("SELECT stock FROM medicamento WHERE id=1"));
        Assert.Equal(27, db.Scalar<int>("SELECT stock FROM medicamento WHERE id=2"));
    }

    [PostgresTheory]
    [InlineData("ausente", "La venta no existe.")]
    [InlineData("anulada", "No se puede modificar una venta anulada.")]
    [InlineData("cliente", "El cliente no existe o está inactivo.")]
    [InlineData("editor_nulo", "El usuario editor no existe o está inactivo.")]
    [InlineData("editor_ausente", "El usuario editor no existe o está inactivo.")]
    [InlineData("editor_inactivo", "El usuario editor no existe o está inactivo.")]
    [InlineData("stock", "Stock insuficiente para el medicamento Ibuprofeno.")]
    public void ActualizarVenta_Rechazo_ConservaEstadoPersistido(string caso, string error)
    {
        using var db = new PostgresTestDatabase();
        var venta = VentaNueva(); venta.Id = Registrar(db);
        switch (caso)
        {
            case "ausente": venta.Id = 999; break;
            case "anulada": Assert.True(db.Repository.AnularVentaLogicamente(venta.Id,2).IsSuccess); break;
            case "cliente": venta.IdCliente=999; break;
            case "editor_nulo": venta.IdUsuarioEditor=null; break;
            case "editor_ausente": venta.IdUsuarioEditor=999; break;
            case "editor_inactivo": db.Execute("UPDATE usuario SET activo=0 WHERE id=2"); break;
            case "stock": venta.Detalles[0].Cantidad=31; break;
        }
        string antes = db.Snapshot();
        var resultado = db.Repository.ActualizarVenta(venta);
        Assert.False(resultado.IsSuccess); Assert.Equal(error, resultado.Error);
        Assert.Equal(antes, db.Snapshot());
    }

    [PostgresTheory]
    [InlineData("sin_filas")]
    [InlineData("excepcion")]
    [InlineData("stock_acumulado")]
    [InlineData("sin_detalles")]
    public void ActualizarVenta_FalloDespuesDeRestaurarStock_HaceRollback(string caso)
    {
        using var db = new PostgresTestDatabase();
        var venta = VentaNueva(); venta.Id = Registrar(db);
        if (caso == "sin_filas" || caso == "excepcion") db.FailVentaUpdate(caso == "excepcion");
        if (caso == "sin_detalles") venta.Detalles.Clear(); // total cero viola CHECK de venta.
        if (caso == "stock_acumulado") venta.Detalles = new List<DetalleVenta>
        {
            new() { IdMedicamento=1, Cantidad=15, PrecioUnitario=10 },
            new() { IdMedicamento=1, Cantidad=10, PrecioUnitario=10 }
        };
        string antes = db.Snapshot();
        var resultado = db.Repository.ActualizarVenta(venta);
        Assert.False(resultado.IsSuccess);
        if(caso == "sin_filas") Assert.Equal("No se pudo actualizar la venta.", resultado.Error);
        else if(caso == "stock_acumulado") Assert.Equal("No se pudo descontar stock del medicamento con id 1.", resultado.Error);
        else Assert.StartsWith("No se pudo actualizar la venta.", resultado.Error);
        Assert.Equal(antes, db.Snapshot());
    }

    [PostgresTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void AnularVenta_ConfirmaBajaYRestituyeStockUnaSolaVez(bool sinDetalles)
    {
        using var db = new PostgresTestDatabase();
        var venta = VentaNueva();
        if (sinDetalles) venta.Detalles.Clear();
        Assert.True(db.Repository.RegistrarVenta(venta).IsSuccess);
        int id = db.Scalar<int>("SELECT max(id) FROM venta");
        var resultado = db.Repository.AnularVentaLogicamente(id, 2);
        Assert.True(resultado.IsSuccess, resultado.Error);
        var persistida = db.Repository.GetById(id)!;
        Assert.Equal(0, persistida.Estado); Assert.Equal(2, persistida.IdUsuarioEditor);
        Assert.NotNull(persistida.UltimaActualizacion);
        Assert.Equal(20, db.Scalar<int>("SELECT stock FROM medicamento WHERE id=1"));
        Assert.Equal(30, db.Scalar<int>("SELECT stock FROM medicamento WHERE id=2"));
        Assert.Equal(sinDetalles ? 0 : 2, db.Repository.GetDetallesByVentaId(id).Count);
        string antes = db.Snapshot();
        var repetida = db.Repository.AnularVentaLogicamente(id, 2);
        Assert.False(repetida.IsSuccess);
        Assert.Equal("La venta ya está anulada lógicamente.", repetida.Error);
        Assert.Equal(antes, db.Snapshot());
    }

    [PostgresTheory]
    [InlineData("ausente", "La venta no existe.")]
    [InlineData("editor_ausente", "El usuario editor no existe o está inactivo.")]
    [InlineData("editor_inactivo", "El usuario editor no existe o está inactivo.")]
    [InlineData("sin_filas", "No se pudo anular la venta.")]
    [InlineData("excepcion", "No se pudo anular la venta.")]
    public void AnularVenta_RechazoOExcepcion_NoDejaCambiosParciales(string caso, string error)
    {
        using var db = new PostgresTestDatabase();
        int id = Registrar(db); int editor = 2;
        if (caso == "ausente") id=999;
        if (caso == "editor_ausente") editor=999;
        if (caso == "editor_inactivo") db.Execute("UPDATE usuario SET activo=0 WHERE id=2");
        if (caso == "sin_filas" || caso == "excepcion") db.FailVentaUpdate(caso == "excepcion");
        string antes = db.Snapshot();
        var resultado = db.Repository.AnularVentaLogicamente(id, editor);
        Assert.False(resultado.IsSuccess);
        if(caso == "excepcion") Assert.StartsWith(error, resultado.Error);
        else Assert.Equal(error, resultado.Error);
        Assert.Equal(antes, db.Snapshot());
    }
}
