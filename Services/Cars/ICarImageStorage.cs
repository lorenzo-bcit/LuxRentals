namespace LuxRentals.Services.Cars;

public interface ICarImageStorage
{
    public IReadOnlyList<string> Validate(IFormFile? file);
    public Task<string?> SaveNewAsync(IFormFile file);
    public Task DeleteAsync(string? relativePath);
}
