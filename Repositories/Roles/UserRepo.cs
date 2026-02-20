using LuxRentals.Data;
using LuxRentals.ViewModels;

namespace LuxRentals.Repositories.Roles
{
    public class UserRepo
    {
        private readonly LuxRentalsDbContext _context;

        public UserRepo(LuxRentalsDbContext context)
        {
            _context = context;
        }

        public List<UserVm> GetAllUsers()
        {
            return _context.Users
                .Select(u => new UserVm
                {
                    Email = u.Email
                })
                .ToList();
        }

        public UserVm? GetUserByEmail(string userName)
        {
            var user = _context.Users
                .Where(u => u.Email == userName)
                .Select(u => new UserVm
                {
                    Email = u.Email
                })
                .FirstOrDefault();

            return user;
        }
    }
}