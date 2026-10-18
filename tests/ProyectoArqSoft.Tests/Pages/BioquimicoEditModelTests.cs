using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using ProyectoArqSoft.Application.Interfaces;
using ProyectoArqSoft.Domain.DTOs;
using ProyectoArqSoft.Domain.Validators;
using ProyectoArqSoft.Pages.Bioquimico;
using Xunit;

namespace ProyectoArqSoft.Tests.Pages;

[Trait("Category", "Unit")]
public class BioquimicoEditModelTests
{
    [Theory]
    [InlineData(null, "Admin", "/Auth/Login")]
    [InlineData("ana", "Bioquimico", "/Index")]
    [InlineData("ana", "Admin", null)]
    public void OnGet_SegunAcceso_RedirigeOMuestraFormulario(
        string? usuario, string rol, string? destino)
    {
        var servicio = new Mock<IUsuarioService>(MockBehavior.Strict);
        var pagina = new BioquimicoEditModel(servicio.Object)
        {
            PageContext = PaginaTestContext.Crear(usuario: usuario, rol: rol)
        };

        var resultado = pagina.OnGet();

        if (destino is null) Assert.IsType<PageResult>(resultado);
        else Assert.Equal(destino, Assert.IsType<RedirectToPageResult>(resultado).PageName);
        Assert.Equal("Bioquimico", pagina.Input.Role);
        Assert.Equal(1, pagina.Input.Activo);
        Assert.Equal(1, pagina.Input.MustChangePassword);
        servicio.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(false, null)]
    [InlineData(true, "Admin")]
    [InlineData(true, null)]
    public void OnPostCargarBioquimicoParaEdicion_UsuarioAusenteORolInvalido_Redirige(
        bool existe, string? rol)
    {
        var servicio = new Mock<IUsuarioService>(MockBehavior.Strict);
        servicio.Setup(s => s.ObtenerUsuarioPorId(17))
            .Returns(existe ? new UsuarioDto { IdUsuario = 17, Role = rol! } : null);
        var pagina = new BioquimicoEditModel(servicio.Object);

        var resultado = Assert.IsType<RedirectToPageResult>(pagina.OnPostCargarBioquimicoParaEdicion(17));

        Assert.Equal("Bioquimico", resultado.PageName);
        Assert.Equal("Bioquímico no encontrado", resultado.RouteValues!["error"]);
        servicio.Verify(s => s.ObtenerUsuarioPorId(17), Times.Once);
        servicio.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(" 123 - a1 ", "123", "a1", false)]
    [InlineData(" 123 ", "123", "", false)]
    [InlineData(null, "", "", true)]
    public void OnPostCargarBioquimicoParaEdicion_Existente_CopiaDatosYSeparaCi(
        string? ci, string ciBase, string complemento, bool datosNulos)
    {
        var usuario = new UsuarioDto
        {
            IdUsuario = 17,
            Role = " bIoQuImIcO ",
            Ci = ci!,
            Nombres = datosNulos ? null! : "Ana",
            ApellidoPaterno = datosNulos ? null! : "Rojas",
            ApellidoMaterno = datosNulos ? null : "Lima",
            CiExtencion = datosNulos ? null! : " LP ",
            Telefono = datosNulos ? null! : "76543210",
            Email = datosNulos ? null! : "ana@example.test",
            UserName = datosNulos ? null! : "ana.rojas"
        };
        var servicio = new Mock<IUsuarioService>(MockBehavior.Strict);
        servicio.Setup(s => s.ObtenerUsuarioPorId(17)).Returns(usuario);
        var pagina = new BioquimicoEditModel(servicio.Object);

        var resultado = pagina.OnPostCargarBioquimicoParaEdicion(17);

        Assert.IsType<PageResult>(resultado);
        Assert.Equal(ciBase, pagina.CiBase);
        Assert.Equal(complemento, pagina.CiComplemento);
        Assert.Equal(17, pagina.Input.IdUsuario);
        Assert.Equal("Bioquimico", pagina.Input.Role);
        Assert.Equal(ci?.Trim() ?? "", pagina.Input.Ci);
        Assert.Equal(datosNulos ? "" : "Ana", pagina.Input.Nombres);
        Assert.Equal(datosNulos ? "" : "Rojas", pagina.Input.ApellidoPaterno);
        Assert.Equal(datosNulos ? "" : "Lima", pagina.Input.ApellidoMaterno);
        Assert.Equal(datosNulos ? "" : "LP", pagina.Input.CiExtencion);
        Assert.Equal(datosNulos ? "" : "76543210", pagina.Input.Telefono);
        Assert.Equal(datosNulos ? "" : "ana@example.test", pagina.Input.Email);
        Assert.Equal(datosNulos ? "" : "ana.rojas", pagina.Input.UserName);
        servicio.Verify(s => s.ObtenerUsuarioPorId(17), Times.Once);
        servicio.VerifyNoOtherCalls();
    }
}

