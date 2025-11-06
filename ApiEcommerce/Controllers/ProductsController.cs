using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository.IRepository;
using AutoMapper;

namespace ApiEcommerce.Controllers;

[ApiController]
[Route("api/[controller]")] //http://localhost:5000/api/products
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _productRepository;

    private readonly ICategoryRepository _categoryRepository;

    private readonly IMapper _mapper;

    public ProductsController(IProductRepository productRepository, ICategoryRepository categoryRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetProducts()
    {
        var products = _productRepository.GetProducts();
        var productsDto = _mapper.Map<List<ProductDto>>(products);
        return Ok(productsDto);
    }

    [HttpGet("{productId:int}", Name = "GetProduct")]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetProduct(int productId)
    {
        var product = _productRepository.GetProduct(productId);
        if (product == null)
        {
            return NotFound("El producto con el id especificado no existe.");
        }
        var productDto = _mapper.Map<ProductDto>(product);
        return Ok(productDto);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult CreateProduct([FromBody] CreateProductDto createProductDto)
    {
        if (createProductDto == null)
        {
            return BadRequest(ModelState);
        }
        if (_productRepository.ProductExists(createProductDto.Name))
        {
            ModelState.AddModelError("CustomError", "El producto ya existe.");
            return BadRequest(ModelState);
        }
        if (_categoryRepository.CategoryExists(createProductDto.CategoryId) == false)
        {
            ModelState.AddModelError("CustomError", "La categoria especificada no existe.");
            return BadRequest(ModelState);
        }

        var product = _mapper.Map<Product>(createProductDto);
        if (!_productRepository.CreateProduct(product))
        {
            ModelState.AddModelError("CustomError", $"Algo salio mal guardando el registro {product.Name}");
            return StatusCode(500, ModelState);
        }
        // Usar la clave primaria definida en el modelo (ProductId)
        var createProduct = _productRepository.GetProduct(product.ProductId);
        var productDto = _mapper.Map<ProductDto>(createProduct);
        return CreatedAtRoute("GetProduct", new { productId = product.ProductId }, productDto);
    }
}