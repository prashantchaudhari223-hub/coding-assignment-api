using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace PharmacyInventory.API.Middleware
{
    /// <summary>
    /// Adds a small set of recommended security headers.
    /// </summary>
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Prevent MIME sniffing
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            // Prevent clickjacking
            context.Response.Headers["X-Frame-Options"] = "DENY";

            // Content Security Policy: allow same-origin assets and permit inline scripts/styles
            // (Relaxed so Swagger UI assets and inline scripts/styles can load. Tighten for production.)
            context.Response.Headers["Content-Security-Policy"] =
                "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data:;";

            // Referrer policy
            context.Response.Headers["Referrer-Policy"] = "no-referrer";
            // HSTS is set via UseHttpsRedirection + server config (do not set here for dev http)
            await _next(context);
        }
    }
}
