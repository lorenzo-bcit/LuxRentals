using LuxRentals.Data;
using LuxRentals.ViewModels.Roles;
using Microsoft.EntityFrameworkCore;

namespace LuxRentals.Repositories.Roles
{
    public class ProfileRepo
    {
        private readonly LuxRentalsDbContext _db;
        private readonly ILogger<ProfileRepo> _logger;

        public ProfileRepo(LuxRentalsDbContext context, ILogger<ProfileRepo> logger)
        {
            _db = context;
            _logger = logger;
        }

        public async Task<List<ProfileVm>> GetAllProfilesAsync()
        {
            try
            {
            return await _db.Customers
                    .Select(c => new ProfileVm
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all profiles");
                throw;
            }
        }

        public async Task<ProfileVm?> GetUserByCustAsync(int id)
        {
            try
            {
            return await _db.Customers
                    .Where(c => c.PkCustomerId == id)
                    .Select(c => new ProfileVm
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving profile for customer ID {CustomerId}", id);
                throw;
            }
        }

        public async Task<ProfileVm?> GetUserByEmailAsync(string email)
        {
            try
        {
            return await _db.Customers
                    .Where(c => c.Email == email)
                    .Select(c => new ProfileVm
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving profile for email {Email}", email);
                throw;
            }
        }

        public async Task<bool> UpdateProfileAsync(ProfileVm model)
        {
            try
        {
            var customer = await _db.Customers
                .FirstOrDefaultAsync(c => c.PkCustomerId == model.PkCustomerId);

            if (customer == null)
            {
                    _logger.LogWarning("Attempted to update non-existent customer ID {CustomerId}", model.PkCustomerId);
                return false;
            }

                // Update fields (except for EMAIL which is not editable)
            customer.FirstName = model.FirstName;
            customer.LastName = model.LastName;
            customer.Email = model.Email;
            customer.PhoneNumber = model.PhoneNumber;
            customer.DriverLicenceNo = model.DriverLicenceNo;

            await _db.SaveChangesAsync();

                _logger.LogInformation("Profile updated successfully for customer ID {CustomerId}", model.PkCustomerId);
            return true;
        }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for customer ID {CustomerId}", model.PkCustomerId);
                throw;
            }
        }
    }
}