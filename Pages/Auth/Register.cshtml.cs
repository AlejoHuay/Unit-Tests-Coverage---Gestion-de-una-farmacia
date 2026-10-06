using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProyectoArqSoft.Domain.DTOs;
using ProyectoArqSoft.Infrastructure.Helpers;
using ProyectoArqSoft.Application.Interfaces;
using ProyectoArqSoft.Domain.Validators;

namespace ProyectoArqSoft.Pages.Auth
{
    public class RegisterModel : PageModel
    {
        private readonly IUsuarioService _usuarioService;

        public RegisterModel(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [BindProperty]
        public UsuarioRegistroDto Registro { get; set; } = new();

        public string MensajeError { get; set; } = string.Empty;
        public string MensajeOk { get; set; } = string.Empty;

        public IActionResult OnGet()
        {
            return Page();
        }

        public IActionResult OnPost()
        {
            string role = "Bioquimico";

            Registro.UserName = CredencialesHelper.GenerarUserName(
                Registro.Nombres,
                Registro.ApellidoPaterno, 
                Registro.Ci
            );

            Registro.Password = CredencialesHelper.GenerarPasswordTemporal();

            ModelState.Remove("Registro.UserName");
            ModelState.Remove("Registro.Password");

            Result resultado = _usuarioService.CrearUsuario(Registro, role, null);

            if (!resultado.IsSuccess)
            {
                MensajeError = resultado.Error;
                return Page();
            }

            MensajeOk = "Usuario registrado correctamente. Revisa las credenciales generadas y tu correo electrónico.";
            return Page();
        }
        
    }
}

