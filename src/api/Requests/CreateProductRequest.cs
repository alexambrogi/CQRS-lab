namespace CQRS.POC.API.Requests;

public record CreateProductRequest(string Name, string Description, decimal Price, int InitialStock);
