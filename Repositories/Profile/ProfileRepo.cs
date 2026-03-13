using LuxRentals.Data;
using LuxRentals.ViewModels.Roles;
using Microsoft.EntityFrameworkCore;

namespace LuxRentals.Repositories.Roles
{
    public class ProfileRepo
    {
        private readonly LuxRentalsDbContext _db;

        public ProfileRepo(LuxRentalsDbContext context)
        {
            _db = context;
        }

        public async Task<List<ProfileVM>> GetAllProfilesAsync()
        {
            return await _db.Customers
                .Select(c => new ProfileVM
                {
                    PkCustomerId = c.PkCustomerId,
                    UserId = c.UserId,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    Email = c.Email,
                    PhoneNumber = c.PhoneNumber,
                    DriverLicenceNo = c.DriverLicenceNo
                })
                .ToListAsync();
        }

        public async Task<ProfileVM?> GetUserByCustAsync(int id)
        {
            return await _db.Customers
                .Where(c => c.PkCustomerId== id)
                .Select(c => new ProfileVM
                {
                    PkCustomerId = c.PkCustomerId,
                    UserId = c.UserId,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    Email = c.Email,
                    PhoneNumber = c.PhoneNumber,
                    DriverLicenceNo = c.DriverLicenceNo
                })
                .FirstOrDefaultAsync();
        }
        public async Task<ProfileVM?> GetUserByEmailAsync(string id)
        {
            return await _db.Customers
                .Where(c => c.Email == id)
                .Select(c => new ProfileVM
                {
                    PkCustomerId = c.PkCustomerId,
                    UserId = c.UserId,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    Email = c.Email,
                    PhoneNumber = c.PhoneNumber,
                    DriverLicenceNo = c.DriverLicenceNo
                })
                .FirstOrDefaultAsync();
        }
        public async Task<bool> UpdateProfileAsync(ProfileVM model)
        {
            var customer = await _db.Customers
                .FirstOrDefaultAsync(c => c.PkCustomerId == model.PkCustomerId);

            if (customer == null)
            {
                return false;
            }

            customer.FirstName = model.FirstName;
            customer.LastName = model.LastName;
            customer.Email = model.Email;
            customer.PhoneNumber = model.PhoneNumber;
            customer.DriverLicenceNo = model.DriverLicenceNo;

            await _db.SaveChangesAsync();
            return true;
        }
    }
}