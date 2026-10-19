using ProyectoArqSoft.Infrastructure.Helpers;
using Xunit;

namespace ProyectoArqSoft.Tests.Helpers;

public class CredencialesHelperTests
{
    [Theory]
    [InlineData("Juan Carlos", "Perez", "1234567", "juan.perez.1234567")]
    [InlineData("Ana", "Lopez", "123-ABC", "ana.lopez.123abc")]
    [InlineData("  Luis Alberto  ", "Gomez", "98 76", "luis.gomez.9876")]
    public void GenerarUserName_ConDatosValidos_RetornaUserNameEsperado(
        string nombres,
        string apellidoPaterno,
        string ci,
        string esperado)
    {
        // Preparar
        // Los datos llegan mediante InlineData.

        // Actuar
        string resultado = CredencialesHelper.GenerarUserName(
            nombres,
            apellidoPaterno,
            ci);

        // Comprobar
        Assert.Equal(esperado, resultado);
    }

    [Fact]
    public void GenerarPasswordTemporal_LongitudCuatro_GeneraCategoriasObligatorias()
    {
        // Arrange - Camino C1: el ciclo no se ejecuta
        const int longitud = 4;

        // Act
        string password = CredencialesHelper.GenerarPasswordTemporal(longitud);

        // Assert
        Assert.Equal(longitud, password.Length);
        Assert.Contains(password, char.IsUpper);
        Assert.Contains(password, char.IsLower);
        Assert.Contains(password, char.IsDigit);
        Assert.Contains(password, c => "!@#$%^&*()_-+=<>?".Contains(c));
    }

    [Fact]
    public void GenerarPasswordTemporal_LongitudCinco_EjecutaCicloYConservaCategorias()
    {
        // Arrange - Camino C2: una iteracion del ciclo
        const int longitud = 5;

        // Act
        string password = CredencialesHelper.GenerarPasswordTemporal(longitud);

        // Assert
        Assert.Equal(longitud, password.Length);
        Assert.Contains(password, char.IsUpper);
        Assert.Contains(password, char.IsLower);
        Assert.Contains(password, char.IsDigit);
        Assert.Contains(password, c => "!@#$%^&*()_-+=<>?".Contains(c));
    }

    [Fact]
    public void GenerarUserName_ConNombreVacio_RetornaSegmentoVacio()
    {
        // Arrange - Camino pendiente de ObtenerPrimerNombre
        string nombres = "";
        string apellido = "Perez";
        string ci = "1234567";

        // Act
        string resultado = CredencialesHelper.GenerarUserName(
            nombres, apellido, ci);

        // Assert
        Assert.Equal(".perez.1234567", resultado);
    }

    [Fact]
    public void GenerarUserName_ConCiVacio_RetornaSegmentoVacio()
    {
        // Arrange - Camino pendiente de NormalizarCi
        string nombres = "Ana";
        string apellido = "Lopez";
        string ci = "";

        // Act
        string resultado = CredencialesHelper.GenerarUserName(
            nombres, apellido, ci);

        // Assert
        Assert.Equal("ana.lopez.", resultado);
    }
}