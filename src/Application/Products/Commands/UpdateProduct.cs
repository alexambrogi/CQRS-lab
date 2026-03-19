using CQRS.POC.Application.Abstractions;
using CQRS.POC.Domain.Entities;
using CQRS.POC.Domain.Exceptions;
using FluentValidation;
using MediatR;

namespace CQRS.POC.Application.Products.Commands
{
    // --- COMMAND ---
    public record UpdateProductCommand(
        Guid Id,
        string Name,
        string Description,
        decimal Price
    ) : IRequest;

    // --- VALIDATOR ---
    public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
    {
        public UpdateProductCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("L'id è obbligatorio.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Il nome è obbligatorio.")
                .MaximumLength(200).WithMessage("Max 200 caratteri.");

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Il prezzo non può essere negativo.");
        }
    }

    // --- HANDLER ---
    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand>
    {
        private readonly IProductRepository repository;

        public UpdateProductCommandHandler(IProductRepository _repository)
        {
            repository = _repository;
        }

        public async Task Handle(UpdateProductCommand command, CancellationToken ct)
        {
            var product = await repository.GetByIdAsync(command.Id, ct)
                ?? throw new NotFoundException(nameof(Product), command.Id);

            product.Update(command.Name, command.Description);
            product.UpdatePrice(command.Price);

            await repository.UpdateAsync(product, ct);
            await repository.SaveChangesAsync(ct);
        }
    }
}
