using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ProyectoArqSoft.Tests.Pages;

// Contexto de pagina y sesion en memoria: no necesita servidor ni base de datos.
internal static class PaginaTestContext
{
    public static PageContext Crear(int? idUsuario = 9, string? usuario = "admin", string? rol = "Admin")
    {
        var sesion = new SesionEnMemoria();
        if (idUsuario.HasValue) sesion.SetInt32("IdUsuario", idUsuario.Value);
        if (usuario is not null) sesion.SetString("UserName", usuario);
        if (rol is not null) sesion.SetString("Role", rol);
        return new PageContext { HttpContext = new DefaultHttpContext { Session = sesion } };
    }

    private sealed class SesionEnMemoria : ISession
    {
        private readonly Dictionary<string, byte[]> datos = new();
        public bool IsAvailable => true;
        public string Id => "sesion-unitaria";
        public IEnumerable<string> Keys => datos.Keys;
        public void Clear() => datos.Clear();
        public void Remove(string key) => datos.Remove(key);
        public void Set(string key, byte[] value) => datos[key] = value;
        public bool TryGetValue(string key, out byte[] value) => datos.TryGetValue(key, out value!);
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

