using MediatR;
using Microsoft.AspNetCore.Mvc;
using CQRS.POC.Application.Products.Commands;
using CQRS.POC.Application.Products.Queries;
using CQRS.POC.API.Requests;
using CQRS.POC.Application.Common.Models;

namespace CQRS.POC.API.Controllers;

[ApiController]
[Route("api/[controller]")]
//public class ProductsController(IMediator mediator) : ControllerBase => // C# 12 syntax for constructor injection
public class ProductsController : ControllerBase
{
    private readonly IMediator mediator;

    public ProductsController(IMediator _mediator)
    {
        this.mediator = _mediator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request, CancellationToken ct)
    {
        var command = new CreateProductCommand(
            request.Name,
            request.Description,
            request.Price,
            request.InitialStock
        );

        var id = await mediator.Send(command, ct);

        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    // placeholder — lo implementiamo nel prossimo step con le Query
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductDTO), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetProductByIdQuery(id), ct);

        return Ok(result);
    }


    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductDTO>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetProductsPagedQuery(page, pageSize, search), ct);

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request, CancellationToken ct)
    {
        var command = new UpdateProductCommand(id, request.Name, request.Description, request.Price);

        await mediator.Send(command, ct);

        return NoContent();
    }
}

