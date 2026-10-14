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

    [Fact]
    public void Verify_ConPasswordCorrecto_RetornaTrue()
    {
        // Preparar
        string password = "Password123!";
        string hash = PasswordHelper.Hash(password);

        // Actuar
        bool resultado = PasswordHelper.Verify(password, hash);

        // Comprobar
        Assert.True(resultado);
    }

    [Fact]
    public void Verify_ConPasswordIncorrecto_RetornaFalse()
    {
        // Preparar
        string passwordCorrecto = "Password123!";
        string passwordIncorrecto = "ClaveIncorrecta456";
        string hash = PasswordHelper.Hash(passwordCorrecto);

        // Actuar
        bool resultado = PasswordHelper.Verify(passwordIncorrecto, hash);

        // Comprobar
        Assert.False(resultado);
    }
}