using ValueObjectFluentValidation.Single;

namespace ValueObjectFluentValidation.Validators;

public class AsyncDelayedValidator<T> : IAsyncValidator<T>
{
    private readonly int _delayMs;

    public AsyncDelayedValidator(int delayMs)
    {
        _delayMs = delayMs;
    }

    public async Task ValidateAsync(T value, IValidationFailureCollection context)
    {
        await Task.Delay(_delayMs);
    }
}