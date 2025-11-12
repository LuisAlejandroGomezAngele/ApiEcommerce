using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;
using Mapster;

namespace ApiEcommerce.Mapping;

public static class ProductMapping
{
    public static void RegisterMappings(TypeAdapterConfig config)
    {
        config.NewConfig<Product, ProductDto>()
            .Map(dest => dest.CategoryName, src => src.Category != null ? src.Category.Name : null)
            .TwoWays();
        config.NewConfig<Product, CreateProductDto>().TwoWays();
        config.NewConfig<Product, UpdateProductDto>().TwoWays();
    }
}