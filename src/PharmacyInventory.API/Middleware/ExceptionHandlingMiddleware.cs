using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PharmacyInventory.API.Common;
using PharmacyInventory.Application.Exceptions;

namespace PharmacyInventory.API.Middleware
{
    /// <summary>
    /// Sits at the very top of the pipeline and converts every exception into a consistent
    /// <see cref="ApiErrorResponse"/> JSON body, instead of leaking stack traces or letting
    /// each controller action handle its own try/catch.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException ex)
            {
                // Thrown by FluentValidation's ValidateAndThrowAsync in the controller.
                var errors = ex.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                await WriteResponseAsync(context, HttpStatusCode.BadRequest, "Validation failed.", errors);
            }
            catch (NotFoundException ex)
            {
                await WriteResponseAsync(context, HttpStatusCode.NotFound, ex.Message, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unhandled exception while processing {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);

                // Never leak internal exception details to the client outside Development.
                var message = _environment.IsDevelopment()
                    ? ex.Message
                    : "An unexpected error occurred. Please try again later.";

                await WriteResponseAsync(context, HttpStatusCode.InternalServerError, message, null);
            }
        }

        private static async Task WriteResponseAsync(
            HttpContext context,
            HttpStatusCode statusCode,
            string message,
            IDictionary<string, string[]>? errors)
        {
            if (context.Response.HasStarted)
            {
                // Response already partially sent - nothing safe we can do.
                return;
            }

            context.Response.Clear();
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var payload = new ApiErrorResponse
            {
                StatusCode = (int)statusCode,
                Message = message,
                Errors = errors,
                TraceId = context.TraceIdentifier
            };

            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}
