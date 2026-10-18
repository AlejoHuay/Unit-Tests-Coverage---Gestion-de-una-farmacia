using ProyectoArqSoft.Domain.Models;
using Xunit;

namespace ProyectoArqSoft.Tests.Models;

[Trait("Category", "Unit")]
public class UsuarioTests
{
    [Fact]
    public void Constructor_YAsignacion_ConservanElContratoDeDatos()
    {
        var antes = DateTime.Now;
        var modelo = new Usuario();

        Assert.Equal(1, modelo.Activo);
        Assert.Equal(1, modelo.MustChangePassword);
        Assert.Null(modelo.ApellidoMaterno);
        Assert.Null(modelo.UltimaActualizacion);
        Assert.Null(modelo.IdUsuarioCreador);
        Assert.Equal(string.Empty, modelo.PasswordHash);
        Assert.InRange(modelo.FechaRegistro, antes, DateTime.Now);

        modelo.IdUsuario = 9;
        modelo.Nombres = "Ana";
        modelo.ApellidoMaterno = "Lima";
        modelo.ApellidoPaterno = "Rojas";
        modelo.Ci = "1234567";
        modelo.Telefono = "76543210";
        modelo.Activo = 0;
        modelo.FechaRegistro = new DateTime(2026, 1, 10);
        modelo.UltimaActualizacion = new DateTime(2026, 1, 11);
        modelo.IdUsuarioCreador = 2;
        modelo.CiExtencion = "LP";
        modelo.Email = "ana@example.test";
        modelo.UserName = "ana";
        modelo.PasswordHash = "hash-solo-prueba";
        modelo.Role = "Bioquimico";
        modelo.MustChangePassword = 0;

        Assert.Equal(9, modelo.IdUsuario);
        Assert.Equal("Ana", modelo.Nombres);
        Assert.Equal("Lima", modelo.ApellidoMaterno);
        Assert.Equal("Rojas", modelo.ApellidoPaterno);
        Assert.Equal("1234567", modelo.Ci);
        Assert.Equal("76543210", modelo.Telefono);
        Assert.Equal(0, modelo.Activo);
        Assert.Equal(new DateTime(2026, 1, 10), modelo.FechaRegistro);
        Assert.Equal(new DateTime(2026, 1, 11), modelo.UltimaActualizacion);
        Assert.Equal(2, modelo.IdUsuarioCreador);
        Assert.Equal("LP", modelo.CiExtencion);
        Assert.Equal("ana@example.test", modelo.Email);
        Assert.Equal("ana", modelo.UserName);
        Assert.Equal("hash-solo-prueba", modelo.PasswordHash);
        Assert.Equal("Bioquimico", modelo.Role);
        Assert.Equal(0, modelo.MustChangePassword);
    }
}

