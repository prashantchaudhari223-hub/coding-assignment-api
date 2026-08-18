using System.Collections.Generic;

namespace PharmacyInventory.API.Common
{
    /// <summary>
    /// Every error response (validation failure, not-found, unhandled exception) is shaped
    /// like this, so the Angular client can rely on one consistent contract instead of
    /// branching on error format per status code.
    /// </summary>
    public class ApiErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;

        /// <summary>Field -> list of messages. Populated for validation errors, null otherwise.</summary>
        public IDictionary<string, string[]>? Errors { get; set; }

        /// <summary>ASP.NET Core's per-request trace id, handy when correlating with server logs.</summary>
        public string TraceId { get; set; } = string.Empty;
    }
}
