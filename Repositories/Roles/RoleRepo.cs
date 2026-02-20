using LuxRentals.Data;
using LuxRentals.ViewModels;
using LuxRentals.ViewModels.Roles;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace LuxRentals.Repositories.Roles
{
    public class RoleRepo
    {
        private readonly LuxRentalsDbContext _context;
        private readonly ILogger<RoleRepo> _logger;

        public RoleRepo(LuxRentalsDbContext context, ILogger<RoleRepo> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Return a list of RoleVM records
        public List<RoleVm> GetAllRoles()
        {
            return _context.Roles
                .Select(r => new RoleVm
                {
                    Id = r.Id,
                    RoleName = r.Name
                })
                .ToList();
        }

        // Return a single RoleVM record.
        public RoleVm? GetRole(string roleId)
        {
            var role = _context.Roles.FirstOrDefault(r => r.Id == roleId);
            if (role != null)
            {
                return new RoleVm
                {
                    Id = role.Id,
                    RoleName = role.Name
                };
            }

            return null;
        }

        // Create a new role record.
        public bool CreateRole(string roleName)
        {
            try
            {
                // Generate ID from first two letters of role name
                string roleId = GenerateRoleId(roleName);

                if (_context.Roles.Any(r => r.Id == roleId))
                {
                    int counter = 1;
                    string tempId = roleId;

                    while (_context.Roles.Any(r => r.Id == tempId))
                    {
                        tempId = $"{roleId}{counter}";
                        counter++;
                    }

                    roleId = tempId;
                }

                _context.Roles.Add(new IdentityRole
                {
                    Id = roleId,
                    Name = roleName,
                    NormalizedName = roleName.ToUpper()
                });

                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating role");
                return false;
            }
        }

        // Helper method to generate role ID from role name
        private string GenerateRoleId(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentException("Role name cannot be empty", nameof(roleName));
            }

            // Remove spaces and special characters
            string cleanName = new string(roleName.Where(c => char.IsLetterOrDigit(c)).ToArray());

            if (cleanName.Length < 2)
            {
                // If less than 2 characters, pad with 'X'
                return cleanName.PadRight(2, 'X');
            }

            // Take first two letters and capitalize
            return cleanName.Substring(0, 2);
        }

        public bool DeleteRole(string roleId)
        {
            try
            {
                var role = _context.Roles.Find(roleId);
                if (role == null)
                {
                    return false;
                }

                // Check if role is in use
                bool roleInUse = _context.UserRoles.Any(ur => ur.RoleId == roleId);
                if (roleInUse)
                {
                    return false;
                }

                _context.Roles.Remove(role);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting role {RoleId}", roleId);
                return false;
            }
        }
    }
}
