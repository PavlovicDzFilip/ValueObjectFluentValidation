using ValueObjectFluentValidation.Single;
using ValueObjectFluentValidation.Util;

namespace ValueObjectFluentValidation;

internal class ValidatorWrapper<T> : 
    Either<IAsyncValidator<T>, IValidator<T>>, 
    IValidatorInternal<T>
{
    public ValidatorWrapper(IValidator<T> syncValidator) : base(syncValidator)
    {
    }

    public ValidatorWrapper(IAsyncValidator<T> asyncValidator) : base(asyncValidator)
    {
    }

    public void Validate(T value, IValidationFailureCollection context)
    {
        WhenLeft(asyncValidator =>
                asyncValidator
                    .ValidateAsync(value, context)
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult());

        WhenRight(syncValidator => syncValidator.Validate(value, context));
    }

    public async Task ValidateAsync(T value, IValidationFailureCollection context)
    {
        await WhenLeftAsync(asyncValidator => asyncValidator.ValidateAsync(value, context));
        WhenRight(syncValidator => syncValidator.Validate(value, context));
    }
}