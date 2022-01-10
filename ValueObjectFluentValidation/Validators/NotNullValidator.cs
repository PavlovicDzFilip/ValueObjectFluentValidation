using ValueObjectFluentValidation.Single;

namespace ValueObjectFluentValidation.Validators;

public class NotNullValidator<T> : IValidator<T>
{
    public void Validate(T value, IValidationFailureCollection context)
    {
        if (value is null)
        {
            context.AddFailure(new ValueNullFailure());
        }
    }
}