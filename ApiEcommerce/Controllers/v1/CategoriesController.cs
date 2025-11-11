using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Cors;
using ApiEcommerce.Constants;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;

namespace ApiEcommerce.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")] //http://localhost:5000/api/categories
[ApiVersion("1.0")]
[Authorize(Roles = "Admin")]

//[EnableCors(PolicyNames.AllowSpecificOrigin)]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryRepository _categoryRepository;

    private readonly IMapper _mapper;

    public CategoriesController(ICategoryRepository categoryRepository, IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetCategories()
    {
        var categories = _categoryRepository.GetCategories();
        var categoriesDto = new List<CategoryDto>();
        foreach (var category in categories)
        {
            categoriesDto.Add(_mapper.Map<CategoryDto>(category));
        }
        return Ok(categoriesDto);
    }

    [AllowAnonymous]
    [HttpGet("{id:int}", Name = "GetCategory")]
    [ResponseCache(CacheProfileName = CacheProfiles.Default20)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetCategory(int id)
    {
        var category = _categoryRepository.GetCategory(id);
        if (category == null)
        {
            return NotFound("La categoria con el id especificado no existe.");
        }
        var categoryDto = _mapper.Map<CategoryDto>(category);

        return Ok(categoryDto);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult CreateCategory([FromBody] CreateCategoryDto createCategoryDto)
    {
        if (createCategoryDto == null)
        {
            return BadRequest(ModelState);
        }
        if (_categoryRepository.CategoryExists(createCategoryDto.Name))
        {
            ModelState.AddModelError("CustomError", "La categoria ya existe.");
            return BadRequest(ModelState);
        }

        var category = _mapper.Map<Category>(createCategoryDto);
        if (!_categoryRepository.CreateCategory(category))
        {
            ModelState.AddModelError("CustomError", $"Algo salio mal guardando el registro {category.Name}");
            return StatusCode(500, ModelState);
        }

        return CreatedAtRoute("GetCategory", new { id = category.Id }, category);
    }

    [HttpPatch("{id:int}", Name = "UpdateCategory")]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult UpdateCategory(int id, [FromBody] CreateCategoryDto updateCategoryDto)
    {
        if (updateCategoryDto == null)
        {
            return BadRequest(ModelState);
        }

        var category = _categoryRepository.GetCategory(id);
        if (category == null)
        {
            return NotFound("La categoria con el id especificado no existe.");
        }
        if (_categoryRepository.CategoryExists(updateCategoryDto.Name))
        {
            ModelState.AddModelError("CustomError", "La categoria ya existe.");
            return BadRequest(ModelState);
        }

        _mapper.Map(updateCategoryDto, category);
        category.Id = id;
        if (!_categoryRepository.UpdateCategory(category))
        {
            ModelState.AddModelError("CustomError", $"Algo salio mal actualizando el registro {category.Name}");
            return StatusCode(500, ModelState);
        }

        return NoContent();
    }
    
    [HttpDelete("{id:int}", Name = "DeleteCategory")]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult DeleteCategory(int id)
    {
        if (!_categoryRepository.CategoryExists(id))
        {
            return NotFound("La categoria con el id especificado no existe.");
        }
        var category = _categoryRepository.GetCategory(id);

        if (!_categoryRepository.DeleteCategory(category!))
        {
            ModelState.AddModelError("CustomError", $"Algo salio mal eliminando el registro {category!.Name}");
            return StatusCode(500, ModelState);
        }
        return NoContent();
        
    }
}
