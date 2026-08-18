using System;

namespace PharmacyInventory.Application.Exceptions
{
    /// <summary>
    /// Thrown by the Application layer when a requested resource doesn't exist.
    /// The API's global exception middleware maps this to a 404 response.
    /// </summary>
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }

        public NotFoundException(string entityName, object key)
            : base($"{entityName} with id '{key}' was not found.") { }
    }
}
