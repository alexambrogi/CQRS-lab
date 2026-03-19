namespace CQRS.POC.API.Requests;

public record UpdateProductRequest(string Name, string Description, decimal Price);
