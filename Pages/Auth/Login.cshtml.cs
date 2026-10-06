using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProyectoArqSoft.Domain.DTOs;
using ProyectoArqSoft.Application.Interfaces;

namespace ProyectoArqSoft.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly IAuthService _authService;

        public LoginModel(IAuthService authService)
        {
            _authService = authService;
        }

        [BindProperty]
        public UsuarioLoginRequestDto LoginRequest { get; set; } = new();

        public string MensajeError { get; set; } = string.Empty;

        public IActionResult OnGet()
        {
            if (!string.IsNullOrWhiteSpace(HttpContext.Session.GetString("Token")))
                return RedirectToPage("/Index");

            return Page();
        }

        public IActionResult OnPost()
        {
            var validacion = _authService.IniciarSesion(LoginRequest, out UsuarioLoginResponseDto? respuesta);

            if (!validacion.IsSuccess)
            {
                MensajeError = validacion.Error;
                return Page();
            }

            if (respuesta == null)
            {
                MensajeError = "No se pudo iniciar sesión.";
                return Page();
            }

            HttpContext.Session.SetString("Token", respuesta.Token);
            HttpContext.Session.SetInt32("IdUsuario", respuesta.IdUsuario);
            HttpContext.Session.SetString("UserName", respuesta.UserName);
            HttpContext.Session.SetString("Role", respuesta.Role);
            HttpContext.Session.SetString("MustChangePassword", respuesta.MustChangePassword.ToString());

            return RedirectToPage("/Index");
        }
    }
}
