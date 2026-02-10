namespace LuxRentals.Services.Cars;

public class SaveResult
{
    public bool IsSuccess { get; private set; }
    public List<(string Field, string Message)> Errors { get; private set; } = [];

    public static SaveResult Ok() => new() { IsSuccess = true };

    public static SaveResult Fail(string field, string message)
    {
        return new SaveResult
        {
            IsSuccess = false,
            Errors = [(field, message)]
        };
    }

    public static SaveResult FailMany(IEnumerable<(string Field, string Message)> errors)
    {
        return new SaveResult
        {
            IsSuccess = false,
            Errors = errors.ToList()
        };
    }
}