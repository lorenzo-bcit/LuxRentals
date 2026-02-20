using LuxRentals.Data;
using LuxRentals.ViewModels;

namespace LuxRentals.Repositories.Roles
{
    public class UserRepo
    {
        private readonly LuxRentalsDbContext _db;

        public UserRepo(LuxRentalsDbContext context)
        {
            _db = context;
        }

        public List<UserVm> GetAllUsers()
        {
            return _db.Users
                .Select(u => new UserVm
                {
                    Email = u.Email
                })
                .ToList();
        }

        public UserVm? GetUserByEmail(string userName)
        {
            var user = _db.Users
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