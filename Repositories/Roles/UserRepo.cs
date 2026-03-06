using LuxRentals.Data;
using LuxRentals.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace LuxRentals.Repositories.Roles
{
    public class UserRepo
    {
        private readonly LuxRentalsDbContext _db;

        public UserRepo(LuxRentalsDbContext context)
        {
            _db = context;
        }

        public async Task<List<UserVm>> GetAllUsersAsync()
        {
            return  await _db.Users
                .Select(u => new UserVm
                {
                    Email = u.Email
                }).ToListAsync();
        }

        public async Task<UserVm?> GetUserByEmailAsync(string userName)
        {
             return await _db.Users
                .Where(u => u.Email == userName)
                .Select(u => new UserVm
                    {
                        Email = u.Email
                    }
                ).FirstOrDefaultAsync();
        }
    }
}