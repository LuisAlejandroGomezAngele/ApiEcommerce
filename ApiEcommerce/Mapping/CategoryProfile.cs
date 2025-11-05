using System;
using AutoMapper;

namespace ApiEcommerce.Mapping;

public class CategoryProfile : Profile
{
    public CategoryProfile()
    {
        CreateMap<Category, CategoryDto>().reverseMap();
        CreateMap<Category, CreateCategoryDto>().reverseMap();
    }
}