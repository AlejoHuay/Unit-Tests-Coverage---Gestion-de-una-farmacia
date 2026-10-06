namespace ProyectoArqSoft.Infrastructure.Middleware
{
    public class SessionTokenMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionTokenMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var token = context.Session.GetString("Token");
            
            if (!string.IsNullOrEmpty(token) && string.IsNullOrEmpty(context.Request.Headers.Authorization))
            {
                context.Request.Headers["Authorization"] = $"Bearer {token}";
            }

            await _next(context);
        }
    }
}
