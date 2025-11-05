using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository.IRepository;
using AutoMapper;

namespace ApiEcommerce.Controllers;

[ApiController]
[Route("api/[controller]")] //http://localhost:5000/api/products
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _productRepository;

    private readonly IMapper _mapper;

    public ProductsController(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }
}