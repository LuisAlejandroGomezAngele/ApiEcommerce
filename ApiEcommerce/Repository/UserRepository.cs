using System.Reflection.Metadata;
using System.Collections.Specialized;
using System.Text;
using System;
using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository.IRepository;

namespace ApiEcommerce.Repository
{
    public class UserRepository : IUserRepository
    {
        public readonly ApplicationDbContext _db;
        private string? secretKey;

        public UserRepository(ApplicationDbContext db, IConfiguration configuration)
        {
            _db = db;
            secretKey = configuration.GetValue<string>("AppSettings:SecretKey");
        }

        public User? GetUser(int id)
        {
            return _db.Users.FirstOrDefault(u => u.UserId == id);
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
                    User = null
                    Message = "Username or password is empty"
                };
            }
            var user = await _db.Users.FirstOrDefault<User>(u => u.Username.ToLower().Trim() == userLoginDto.Username.ToLower().Trim());

            if(user == null)
            {
                return new UserLoginResponseDto()
                {
                    Token = string.Empty,
                    User = null
                    Message = "User not found"
                };
            }
            if (!BCrypt.Net.BCrypt.Verify(userLoginDto.Password, user.Password))
            {
                return new UserLoginResponseDto()
                {
                    Token = string.Empty,
                    User = null
                    Message = "Password is incorrect"
                };
            }

            //Generar token
            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            if (secretKey == null)
            {
                throw new InvalidOperationException("Secret key is null");
            }

            var key = Encoding.UTF8.GetBytes(secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(new[]
                {
                    new System.Security.Claims.Claim("id", user.UserId.ToString()),
                    new System.Security.Claims.Claim("userName", user.Username),
                    new System.Security.Claims.Claim("role", user.Role ?? string.Empty)
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return new UserLoginResponseDto()
            {
                Token = tokenHandler.WriteToken(token),
                User = new UserRegisterDto()
                {
                    Name = user.Name,
                    Username = user.Username,
                    Role = user.Role,
                    Password = user.Password ?? string.Empty
                },
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