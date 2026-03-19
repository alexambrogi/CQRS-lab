using MediatR;
using FluentValidation;

namespace CQRS.POC.Application.Common.Behaviors
{
    //public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) 
    //                            : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
    public class ValidationBehavior<TRequest, TResponse>
                                : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
    {
        //(IEnumerable<IValidator<TRequest>> validators)
        private readonly IEnumerable<IValidator<TRequest>> validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> _validators)
        {
            this.validators = _validators;
        }

        //public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        //{
        //    throw new NotImplementedException();
        //}

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
        {
            // Se non ci sono validator registrati per questa request, vai avanti
            if (!validators.Any())
                return await next();

            var context = new ValidationContext<TRequest>(request);

            var failures = validators
                .Select(v => v.Validate(context))
                .SelectMany(r => r.Errors)
                .Where(f => f is not null)
                .ToList();

            if (failures.Any())
                throw new ValidationException(failures);

            return await next();
        }

    }
}

