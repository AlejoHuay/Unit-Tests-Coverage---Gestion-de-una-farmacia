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
}