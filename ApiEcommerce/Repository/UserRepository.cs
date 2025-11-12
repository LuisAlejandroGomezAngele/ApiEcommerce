using System.Text;
using System;
using System.Threading.Tasks;
using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;

namespace ApiEcommerce.Repository
{
    public class UserRepository : IUserRepository
    {
        public readonly ApplicationDbContext _db;
        private string? secretKey;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

    private readonly MapsterMapper.IMapper _mapper;

    public UserRepository(ApplicationDbContext db, IConfiguration configuration, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, MapsterMapper.IMapper mapper)
        {
            _db = db;
            secretKey = configuration.GetValue<string>("AppSettings:SecretKey");
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
        }

        public ApplicationUser? GetUser(string id)
        {
            return _db.ApplicationUsers.FirstOrDefault(u => u.Id == id);
        }

        public ICollection<ApplicationUser> GetUsers()
        {
            return _db.ApplicationUsers.OrderBy(u => u.UserName).ToList();
        }
        public bool IsUniqueUser(string username)
        {
            return !_db.ApplicationUsers.Any(u => u.UserName != null && u.UserName.ToLower().Trim() == username.ToLower().Trim());
        }

        public async Task<UserLoginResponseDto> Login(UserLoginDto userLoginDto)
        {
            if(string.IsNullOrWhiteSpace(userLoginDto.Username) || string.IsNullOrWhiteSpace(userLoginDto.Password))
            {
                return new UserLoginResponseDto()
                {
                    Token = string.Empty,
                    User = null,
                    Message = "Username or password is empty"
                };
            }
            var user = await _db.ApplicationUsers.FirstOrDefaultAsync(u => u.UserName != null && u.UserName.ToLower().Trim() == userLoginDto.Username.ToLower().Trim());

            if (user == null)
            {
                return new UserLoginResponseDto()
                {
                    Token = string.Empty,
                    User = null,
                    Message = "User not found"
                };
            }

            if (userLoginDto.Password == null)
            {
                return new UserLoginResponseDto()
                {
                    Token = string.Empty,
                    User = null,
                    Message = "Password is null"
                };
            }
            
            bool isValid = await _userManager.CheckPasswordAsync(user, userLoginDto.Password);

            if (!isValid)
            {
                return new UserLoginResponseDto()
                {
                    Token = string.Empty,
                    User = null,
                    Message = "Password is incorrect"
                };
            }

            //Generar token
            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            if (secretKey == null)
            {
                throw new InvalidOperationException("Secret key is null");
            }

            var roles = await _userManager.GetRolesAsync(user);

            var key = Encoding.UTF8.GetBytes(secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(new[]
                {
                    new Claim("id", user.Id.ToString()),
                    new Claim("userName", user.UserName ?? string.Empty),
                    new Claim(ClaimTypes.Role, roles.FirstOrDefault() ?? string.Empty)
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return new UserLoginResponseDto()
            {
                Token = tokenHandler.WriteToken(token),
                User = _mapper.Map<UserDataDto>(user),
                Message = "Login successful"
            };
        }

        public async Task<UserDataDto> Register(CreateUserDto createUserDto)
        {
            if (string.IsNullOrWhiteSpace(createUserDto.Username) || string.IsNullOrWhiteSpace(createUserDto.Password))
            {
                throw new ArgumentException("Username or password is empty");
            }

            if (createUserDto.Password == null)
            {
                throw new ArgumentException("Password is null");
            }

            var user = new ApplicationUser()
            {
                UserName = createUserDto.Username,
                Email = createUserDto.Username,
                NormalizedEmail = createUserDto.Username.ToUpper(),
                Name = createUserDto.Name
            };

            var result = await _userManager.CreateAsync(user, createUserDto.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"User creation failed: {errors}");
            }

            var userRole = createUserDto.Role ?? "User";
            var roleexists = await _roleManager.RoleExistsAsync(userRole);
            if (!roleexists)
            {
                var identityRole = await _roleManager.CreateAsync(new IdentityRole(userRole));
            }

            await _userManager.AddToRoleAsync(user, userRole);

            var createdUser = await _db.ApplicationUsers.FirstOrDefaultAsync(u => u.UserName == createUserDto.Username);
            if (createdUser == null)
            {
                throw new InvalidOperationException("User was created but could not be retrieved from the database.");
            }
            return _mapper.Map<UserDataDto>(createdUser);
        }
    }
}