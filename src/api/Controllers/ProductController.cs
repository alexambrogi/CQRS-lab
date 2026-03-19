using MediatR;
using Microsoft.AspNetCore.Mvc;
using CQRS.POC.Application.Products.Commands;
using CQRS.POC.Application.Products.Queries;

namespace CqrsPoc.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
//public class ProductsController(IMediator mediator) : ControllerBase
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
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetProductByIdQuery(id), ct);

        return Ok(result);
    }


    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetProductsPagedQuery(page, pageSize, search), ct);

        return Ok(result);
    }
}

// DTO di input — separato dal Command per disaccoppiare API da Application
public record CreateProductRequest(string Name, string Description, decimal Price, int InitialStock);