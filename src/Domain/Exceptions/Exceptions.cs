using System;
using System.Collections.Generic;
using System.Text;

namespace CQRS.POC.Domain.Exceptions
{
    public class DomainException(string message) : Exception(message);

    public class NotFoundException(string entity, object key) : Exception($"{entity} con id '{key}' non trovato.");
}
