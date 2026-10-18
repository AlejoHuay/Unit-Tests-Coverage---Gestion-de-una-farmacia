using Microsoft.AspNetCore.Http;
using ProyectoArqSoft.Infrastructure.Middleware;
using Xunit;

namespace ProyectoArqSoft.Tests.Infrastructure;

public class SessionTokenMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_TokenEnSesionYAuthorizationVacio_EstableceHeaderYLlamaSiguiente()
    {
        // Arrange
        var context = CrearContexto("token-prueba");
        bool siguienteLlamado = false;
        var middleware = new SessionTokenMiddleware(_ =>
        {
            siguienteLlamado = true;
            return Task.CompletedTask;
        });

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        string authorization = context.Request.Headers.Authorization.ToString();
        Assert.Equal("Bearer token-prueba", authorization);
        Assert.True(siguienteLlamado);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task InvokeAsync_SinTokenEnSesion_NoCambiaAuthorization(string? token)
    {
        // Arrange
        var context = CrearContexto(token);
        bool siguienteLlamado = false;
        var middleware = new SessionTokenMiddleware(_ =>
        {
            siguienteLlamado = true;
            return Task.CompletedTask;
        });

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(string.Empty, context.Request.Headers.Authorization.ToString());
        Assert.True(siguienteLlamado);
    }

    [Fact]
    public async Task InvokeAsync_AuthorizationExistente_NoLoReemplaza()
    {
        // Arrange
        var context = CrearContexto("token-prueba");
        context.Request.Headers.Authorization = "Bearer existente";
        bool siguienteLlamado = false;
        var middleware = new SessionTokenMiddleware(_ =>
        {
            siguienteLlamado = true;
            return Task.CompletedTask;
        });

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal("Bearer existente", context.Request.Headers.Authorization.ToString());
        Assert.True(siguienteLlamado);
    }

    private static DefaultHttpContext CrearContexto(string? token)
    {
        var session = new SessionEnMemoria();
        if (token is not null)
            session.SetString("Token", token);

        return new DefaultHttpContext { Session = session };
    }

    private sealed class SessionEnMemoria : ISession
    {
        private readonly Dictionary<string, byte[]> _values = new();

        public bool IsAvailable => true;
        public string Id => "middleware-unit-test";
        public IEnumerable<string> Keys => _values.Keys;

        public void Clear() => _values.Clear();
        public void Remove(string key) => _values.Remove(key);
        public void Set(string key, byte[] value) => _values[key] = value;
        public bool TryGetValue(string key, out byte[] value) => _values.TryGetValue(key, out value!);
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
