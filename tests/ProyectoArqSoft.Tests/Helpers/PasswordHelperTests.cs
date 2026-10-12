using ProyectoArqSoft.Infrastructure.Helpers;
using Xunit;

namespace ProyectoArqSoft.Tests.Helpers;

public class PasswordHelperTests
{
    [Theory]
    [InlineData("Password123!")]
    [InlineData("ClaveSegura456")]
    [InlineData("")]
    public void Hash_ConPassword_RetornaHashValido(string password)
    {
        // Preparar
        // Los datos de entrada llegan mediante InlineData.

        // Actuar
        string hash = PasswordHelper.Hash(password);

        // Comprobar
        Assert.False(string.IsNullOrWhiteSpace(hash));
        Assert.NotEqual(password, hash);
    }
}