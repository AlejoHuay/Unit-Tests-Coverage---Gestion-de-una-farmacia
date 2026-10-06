using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProyectoArqSoft.Pages.EstadoPaginas;

namespace ProyectoArqSoft.Pages.Base
{
    public abstract class BasePageModel : PageModel
    {
        public EstadoPagina Estado { get; set; } = new EstadoPagina();

        protected IActionResult? ValidarAccesoAdmin()
        {
            string? usuario = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrWhiteSpace(usuario))
                return RedirectToPage("/Auth/Login");

            string role = HttpContext.Session.GetString("Role")?.Trim() ?? string.Empty;
            if (!role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                return RedirectToPage("/Index");

            return null;
        }
    }
}

