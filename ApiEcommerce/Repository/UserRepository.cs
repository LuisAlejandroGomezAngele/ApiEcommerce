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
using AutoMapper;

namespace ApiEcommerce.Repository
{
    public class UserRepository : IUserRepository
    {
        public readonly ApplicationDbContext _db;
        private string? secretKey;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly IMapper _mapper;

        public UserRepository(ApplicationDbContext db, IConfiguration configuration, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IMapper mapper)
        {
            _db = db;
            secretKey = configuration.GetValue<string>("AppSettings:SecretKey");
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
        }

        public User? GetUser(int id)
        {
            return _db.Users.FirstOrDefault(u => u.Id == id);
        }

        public ICollection<User> GetUsers()
        {
            return _db.Users.OrderBy(u => u.Username).ToList();
        }
        public bool IsUniqueUser(string username)
        {
            return !_db.Users.Any(u => u.Username.ToLower().Trim() == username.ToLower().Trim());
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

        public async Task<User> Register(CreateUserDto createUserDto)
        {
            var encriptedPassword = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password);
            var user = new User()
            {
                Name = createUserDto.Name ?? "No Name",
                Username = createUserDto.Username ?? "No UserName",
                Password = encriptedPassword,
                Role = createUserDto.Role
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }
    }
}