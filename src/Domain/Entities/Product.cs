using CQRS.POC.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQRS.POC.Domain.Entities
{
    public class Product
    {
        protected Product() { }

        public Guid Id { get; private set; }
        public string Name { get; private set; } = default!;
        public string Description { get; private set; } = default!;
        public decimal Price { get; private set; }
        public int Stock { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        public static Product Create(string name, string description, decimal price, int stock)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Il nome è obbligatorio.");
            if (price < 0)
                throw new DomainException("Il prezzo non può essere negativo.");
            if (stock < 0)
                throw new DomainException("Lo stock non può essere negativo.");

            return new Product
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description,
                Price = price,
                Stock = stock,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public void UpdatePrice(decimal newPrice)
        {
            if (newPrice < 0) throw new DomainException("Il prezzo non può essere negativo.");
            Price = newPrice;
            UpdatedAt = DateTime.UtcNow;
        }

        public void DecreaseStock(int quantity)
        {
            if (quantity <= 0) throw new DomainException("La quantità deve essere positiva.");
            if (Stock < quantity) throw new DomainException($"Stock insufficiente. Disponibili: {Stock}");
            Stock -= quantity;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
