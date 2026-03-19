using System;
using System.Collections.Generic;
using System.Text;

namespace CQRS.POC.Application.Common.Models
{
    public record ProductDTO
    (
        Guid Id,
        string Name,
        string Description,
        decimal Price,
        int Stock,
        bool IsActive,
        DateTime CreatedAt
    );
}
