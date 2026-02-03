using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LuxRentals.Data;

public class LuxRentalsDbContext(DbContextOptions<LuxRentalsDbContext> options) : IdentityDbContext(options)
{
}