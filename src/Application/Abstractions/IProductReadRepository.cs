using CQRS.POC.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQRS.POC.Application.Abstractions
{
    public interface IProductReadRepository : IRepositoryRead<ProductDTO, Guid>
    {
    }
}
