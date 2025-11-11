using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;


namespace ApiEcommerce.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")] //http://localhost:5000/api/products
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Authorize(Roles = "Admin")]

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

    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetProducts()
    {
        var products = _productRepository.GetProducts();
        var productsDto = _mapper.Map<List<ProductDto>>(products);
        return Ok(productsDto);
    }

    [AllowAnonymous]
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

    [HttpGet("searchProductByCategory/{categoryId:int}", Name = "GetProductsForCategory")]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetProductsForCategory(int categoryId)
    {
        var products = _productRepository.GetProductsInCategory(categoryId);
        if (products == null || !products.Any())
        {
            return NotFound("No se encontraron productos para la categoría especificada.");
        }
        var productsDto = _mapper.Map<List<ProductDto>>(products);
        return Ok(productsDto);
    }

    [HttpGet("searchProductByNameDescription/{name}", Name = "GetProductsForNameDescription")]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetProductsForNameDescription(string name)
    {
        var products = _productRepository.SearchProduct(name);
        if (products == null || !products.Any())
        {
            return NotFound("No se encontraron productos para el nombre o descripción especificada.");
        }
        var productsDto = _mapper.Map<List<ProductDto>>(products);
        return Ok(productsDto);
    }
    [HttpPatch("buyProduct/{name}/{quantity:int}", Name = "BuyProduct")]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult BuyProduct(string name, int quantity)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest("El nombre del producto no puede estar vacío.");
        }
        if (quantity <= 0)
        {
            return BadRequest("La cantidad debe ser mayor que cero.");
        }
        var foundProduct = _productRepository.GetProductByName(name);
        if (foundProduct == null)
        {
            return NotFound("El producto con el nombre especificado no existe.");
        }

        var result = _productRepository.BuyProduct(name, quantity);
        if (!result)
        {
            ModelState.AddModelError("CustomError", "No hay suficiente stock para completar la compra.");
            return BadRequest(ModelState);
        }
        
        var units = quantity == 1 ? "unidad" : "unidades";
        return Ok($"Compra exitosa de {quantity} {units} del producto '{name}'.");
    }

    [HttpPut("{productId:int}", Name = "UpdateProduct")]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult UpdateProduct(int productId, [FromBody] UpdateProductDto updateProductDto)
    {
        if (updateProductDto == null)
        {
            return BadRequest(ModelState);
        }
        if (!_productRepository.ProductExists(productId))
        {
            ModelState.AddModelError("CustomError", "El producto no existe.");
            return BadRequest(ModelState);
        }
        if (!_categoryRepository.CategoryExists(updateProductDto.CategoryId))
        {
            ModelState.AddModelError("CustomError", "La categoria especificada no existe.");
            return BadRequest(ModelState);
        }

        var product = _mapper.Map<Product>(updateProductDto);
        product.ProductId = productId;
        if (!_productRepository.UpdateProduct(product))
        {
            ModelState.AddModelError("CustomError", $"Algo salio mal guardando el registro {product.Name}");
            return StatusCode(500, ModelState);
        }
        // Usar la clave primaria definida en el modelo (ProductId)
        return NoContent();
    }

    [HttpDelete("{productId:int}", Name = "DeleteProduct")]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult DeleteProduct(int productId)
    {
        if (productId <= 0)
        {
            return BadRequest(ModelState);
        }
        var product = _productRepository.GetProduct(productId);
        if (product == null)
        {
            return NotFound("El producto con el id especificado no existe.");
        }

        if (!_productRepository.DeleteProduct(product))
        {
            ModelState.AddModelError("CustomError", $"Algo salio mal eliminando el registro {product.Name}");
            return StatusCode(500, ModelState);
        }

        return NoContent();
    }

}