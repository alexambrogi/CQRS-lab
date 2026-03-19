using CQRS.POC.Application.Abstractions;
using CQRS.POC.Application.Common.Models;
using CQRS.POC.Domain.Entities;
using CQRS.POC.Domain.Exceptions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQRS.POC.Application.Products.Queries
{
    public record GetProductByIdQuery(Guid ProductId) : IRequest<ProductDTO>;

    //public class GetProductByIdQueryHandler( IProductReadRepository readRepository) : IRequestHandler<GetProductByIdQuery, ProductDTO>
    public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDTO>
    {
        private readonly IProductReadRepository _readRepository;

        public GetProductByIdQueryHandler(IProductReadRepository readRepository)
        {
            this._readRepository = readRepository;
        }

        public async Task<ProductDTO> Handle(GetProductByIdQuery query, CancellationToken ct)
        {
            var product = await _readRepository.GetByIdAsync(query.ProductId, ct);

            if (product is null)
                throw new NotFoundException(nameof(Product), query.ProductId);

            return product;
        }


    }

    public record GetProductsPagedQuery(int Page = 1,int PageSize = 10,string? SearchTerm = null) : IRequest<PagedResult<ProductDTO>>;

    //public class GetProductsPagedQueryHandler(IProductReadRepository readRepository) : IRequestHandler<GetProductsPagedQuery, PagedResult<ProductDto>>
    public class GetProductsPagedQueryHandler : IRequestHandler<GetProductsPagedQuery, PagedResult<ProductDTO>>
    {
        private readonly IProductReadRepository _readRepository;

        public GetProductsPagedQueryHandler(IProductReadRepository readRepository)
        {
            this._readRepository = readRepository;
        }

        public async Task<PagedResult<ProductDTO>> Handle(GetProductsPagedQuery query, CancellationToken ct)
        {
            return await _readRepository.GetPagedAsync(
                query.Page,
                query.PageSize,
                query.SearchTerm,
                ct);
        }
    }

}
