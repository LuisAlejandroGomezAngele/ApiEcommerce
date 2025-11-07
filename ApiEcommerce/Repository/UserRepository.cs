using System;
using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository.IRepository;

namespace ApiEcommerce.Repository
{
    public class UserRepository : IUserRepository
    {
        public readonly ApplicationDbContext _db;

        public UserRepository(ApplicationDbContext db)
        {
            _db = db;
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
            throw new NotImplementedException();
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