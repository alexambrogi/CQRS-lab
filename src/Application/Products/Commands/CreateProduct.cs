using CQRS.POC.Application.Abstractions;
using CQRS.POC.Domain.Entities;
using CQRS.POC.Domain.Exceptions;
using FluentValidation;
using MediatR;

namespace CQRS.POC.Application.Products.Commands
{
    // --- COMMAND ---
    // Record immutabile: esprime l'intenzione di creare un prodotto
    public record CreateProductCommand(
        string Name,
        string Description,
        decimal Price,
        int InitialStock
    ) : IRequest<Guid>;

    // --- VALIDATOR ---
    // FluentValidation: validazione dichiarativa del command
    public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Il nome è obbligatorio.")
                .MaximumLength(200).WithMessage("Max 200 caratteri.");

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Il prezzo non può essere negativo.");

            RuleFor(x => x.InitialStock)
                .GreaterThanOrEqualTo(0).WithMessage("Lo stock non può essere negativo.");
        }
    }

    // --- HANDLER ---
    // Unica responsabilità: orchestrare la creazione e la persistenza
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Guid>
    {

        private readonly IProductRepository repository;

        public CreateProductCommandHandler(IProductRepository _repository)
        {
            repository = _repository;
        }

        public async Task<Guid> Handle(CreateProductCommand command, CancellationToken ct)
        {
            var product = Product.Create(
                command.Name,
                command.Description,
                command.Price,
                command.InitialStock
            );

            await repository.AddAsync(product, ct);
            await repository.SaveChangesAsync(ct);

            return product.Id;
        }
    }
}

