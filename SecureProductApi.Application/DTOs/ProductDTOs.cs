using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SecureProductApi.Application.DTOs
{
    public record CreateProductRequest(
        [Required, MaxLength(200)] string Name,
        [MaxLength(1000)] string? Description,
        [Range(0.01, 1000000)] decimal Price,
        [Range(0, 100000)] int Quantity
    );

    public record UpdateProductRequest(
        [Required, MaxLength(200)] string Name,
        [MaxLength(1000)] string? Description,
        [Range(0.01, 1000000)] decimal Price,
        [Range(0, 100000)] int Quantity
    );

    public record ProductResponse(int Id, string Name, string? Description, decimal Price, int Quantity);
}