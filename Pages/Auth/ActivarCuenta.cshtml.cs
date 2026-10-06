using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProyectoArqSoft.Application.Interfaces;
using ProyectoArqSoft.Domain.Validators;

namespace ProyectoArqSoft.Pages.Auth
{
    public class ActivarCuentaModel : PageModel
    {
        private readonly IUsuarioService _usuarioService;

        public ActivarCuentaModel(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [BindProperty(SupportsGet = true)]
        public string Token { get; set; } = string.Empty;

        [BindProperty]
        public string NuevaPassword { get; set; } = string.Empty;

        [BindProperty]
        public string ConfirmarPassword { get; set; } = string.Empty;

        public string Mensaje { get; set; } = string.Empty;
        public bool EsError { get; set; } = false;
        public bool MostrarFormulario { get; set; } = false;

        public IActionResult OnGet(string token)
        {
            Token = token?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(Token))
            {
                Mensaje = "Token inválido.";
                EsError = true;
                MostrarFormulario = false;
                return Page();
            }

            Result resultado = _usuarioService.ValidarActivacionCuenta(Token);

            if (!resultado.IsSuccess)
            {
                Mensaje = resultado.Error;
                EsError = true;
                MostrarFormulario = false;
                return Page();
            }

            Mensaje = "Token válido. Ahora debes definir tu nueva contraseña.";
            EsError = false;
            MostrarFormulario = true;
            return Page();
        }

        public IActionResult OnPost()
        {
            Token = Token?.Trim() ?? string.Empty;
            NuevaPassword = NuevaPassword?.Trim() ?? string.Empty;
            ConfirmarPassword = ConfirmarPassword?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(Token))
            {
                Mensaje = "Token inválido.";
                EsError = true;
                MostrarFormulario = false;
                return Page();
            }

            if (string.IsNullOrWhiteSpace(NuevaPassword))
            {
                Mensaje = "La nueva contraseña es obligatoria.";
                EsError = true;
                MostrarFormulario = true;
                return Page();
            }

            if (NuevaPassword != ConfirmarPassword)
            {
                Mensaje = "La contraseña y su confirmación no coinciden.";
                EsError = true;
                MostrarFormulario = true;
                return Page();
            }

            Result resultado = _usuarioService.ActivarCuenta(Token, NuevaPassword);

            if (!resultado.IsSuccess)
            {
                Mensaje = resultado.Error;
                EsError = true;
                MostrarFormulario = true;
                return Page();
            }

            Mensaje = "Cuenta activada correctamente. Ya puedes iniciar sesión.";
            EsError = false;
            MostrarFormulario = false;

            return Page();
        }
    }
}
