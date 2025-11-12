using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;

namespace ApiEcommerce.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")] //http://localhost:5000/api/users
[ApiVersionNeutral]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    private readonly MapsterMapper.IMapper _mapper;

    public UsersController(IUserRepository userRepository, MapsterMapper.IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }
    
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetUsers()
    {
        var users = _userRepository.GetUsers();
        var usersDto = _mapper.Map<List<UserDto>>(users);
        return Ok(usersDto);
    }

    [HttpGet("{userId}", Name = "GetUser")]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetUser(string userId)
    {
        var user = _userRepository.GetUser(userId);
        if (user == null)
        {
            return NotFound("El usuario con el id especificado no existe.");
        }
        var userDto = _mapper.Map<UserDto>(user);
        return Ok(userDto);
    }

    [AllowAnonymous]
    [HttpPost(Name = "RegisterUser")]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RegisterUser([FromBody] CreateUserDto createUserDto)
    {
        if (createUserDto == null || !ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (string.IsNullOrWhiteSpace(createUserDto.Username))
        {
            return BadRequest("El nombre de usuario no puede estar vacío.");
        }

        if (!_userRepository.IsUniqueUser(createUserDto.Username))
        {
            return BadRequest("El nombre de usuario ya existe.");
        }

        try
        {
            var result = await _userRepository.Register(createUserDto);

            if (result == null)
            {
                ModelState.AddModelError("", "Error al registrar el usuario");
                return StatusCode(500, ModelState);
            }

            return CreatedAtRoute("GetUser", new { userId = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            // Retornar errores de validación (por ejemplo reglas de contraseña) como 400 Bad Request
            ModelState.AddModelError("", ex.Message);
            return BadRequest(ModelState);
        }
    }

    [AllowAnonymous]
    [HttpPost("Login", Name = "LoginUser")]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> LoginUser([FromBody] UserLoginDto userLoginDto)
    {
        if (userLoginDto == null || !ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _userRepository.Login(userLoginDto);

        if (result == null)
        {
            return Unauthorized();
        }

        return Ok(result);
    }

}