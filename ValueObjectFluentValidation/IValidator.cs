using ValueObjectFluentValidation.Single;

namespace ValueObjectFluentValidation;

public interface IValidator<in T>
{
    void Validate(T value, IValidationFailureCollection context);
}
public interface IAsyncValidator<in T>
{
    Task ValidateAsync(T value, IValidationFailureCollection context);
}

internal interface IValidatorInternal<in T> :
    IValidator<T>,
    IAsyncValidator<T>
{
}
