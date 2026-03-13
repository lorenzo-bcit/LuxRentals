namespace LuxRentals.Services.Cars;

public class SaveResult
{
    public bool IsSuccess { get; private set; }
    public string? Message { get; private set; }
    public List<(string Field, string Message)> Errors { get; private set; } = [];

    public static SaveResult Ok(string? message = null) => new() { IsSuccess = true, Message = message };

    public static SaveResult Fail(string field, string message)
    {
        return new SaveResult
        {
            IsSuccess = false,
            Message = message,
            Errors = [(field, message)]
        };
    }

    public static SaveResult FailMany(IEnumerable<(string Field, string Message)> errors)
    {
        var errorList = errors.ToList();
        return new SaveResult
        {
            IsSuccess = false,
            Message = errorList.FirstOrDefault().Message,
            Errors = errorList
        };
    }
}