using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using ProyectoArqSoft.Pages.Base;
using ProyectoArqSoft.Pages.EstadoPaginas;
using Xunit;

namespace ProyectoArqSoft.Tests.Pages;

[Trait("Category", "Unit")]
public class BasePageModelTests
{
    [Theory]
    [InlineData(null, "Admin", "/Auth/Login")]
    [InlineData(" \t ", "Admin", "/Auth/Login")]
    [InlineData("ana", null, "/Index")]
    [InlineData("ana", "Bioquimico", "/Index")]
    [InlineData("ana", " aDmIn ", null)]
    public void ValidarAccesoAdmin_ResuelveAccesoSegunUsuarioYRol(
        string? usuario, string? rol, string? paginaEsperada)
    {
        var sesion = new Mock<ISession>(MockBehavior.Strict);
        byte[]? usuarioBytes = usuario is null ? null : Encoding.UTF8.GetBytes(usuario);
        byte[]? rolBytes = rol is null ? null : Encoding.UTF8.GetBytes(rol);
        sesion.Setup(s => s.TryGetValue("UserName", out usuarioBytes)).Returns(usuario is not null);
        sesion.Setup(s => s.TryGetValue("Role", out rolBytes)).Returns(rol is not null);
        var pagina = new PaginaDePrueba
        {
            PageContext = new PageContext
            {
                HttpContext = new DefaultHttpContext { Session = sesion.Object }
            }
        };

        var resultado = pagina.ComprobarAcceso();

        if (paginaEsperada is null)
            Assert.Null(resultado);
        else
            Assert.Equal(paginaEsperada, Assert.IsType<RedirectToPageResult>(resultado).PageName);

        sesion.Verify(s => s.TryGetValue("UserName", out usuarioBytes), Times.Once);
        sesion.Verify(s => s.TryGetValue("Role", out rolBytes),
            string.IsNullOrWhiteSpace(usuario) ? Times.Never() : Times.Once());
        sesion.VerifyNoOtherCalls();
    }

    [Fact]
    public void Estado_SeCreaPorPaginaYPermiteReemplazo()
    {
        var primera = new PaginaDePrueba();
        var segunda = new PaginaDePrueba();
        var reemplazo = new EstadoPagina { Mensaje = "Guardado." };

        Assert.NotSame(primera.Estado, segunda.Estado);
        primera.Estado = reemplazo;

        Assert.Same(reemplazo, primera.Estado);
        Assert.Equal("Guardado.", primera.Estado.Mensaje);
        Assert.Equal(string.Empty, segunda.Estado.Mensaje);
    }

    private sealed class PaginaDePrueba : BasePageModel
    {
        public IActionResult? ComprobarAcceso() => ValidarAccesoAdmin();
    }
}

