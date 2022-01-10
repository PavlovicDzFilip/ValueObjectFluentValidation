using ValueObjectFluentValidation.Single;

namespace ValueObjectFluentValidation.Validators;

public static class ValidatorExtensions
{
    public static ISinglePropertyValidatorBuilder<T> NotNull<T>(this ISinglePropertyValidatorBuilder<T?> singlePropertyValidatorBuilder)
    {
        return singlePropertyValidatorBuilder.AddValidator(new NotNullValidator<T>());
    }

    public static ISinglePropertyValidatorBuilder<string> Length(this ISinglePropertyValidatorBuilder<string> singlePropertyValidatorBuilder, int minLength, int maxLength)
    {
        return singlePropertyValidatorBuilder.AddValidator(new StringLengthValidator(minLength, maxLength));
    }

    public static ISinglePropertyValidatorBuilder<T> ValidAsync<T>(this ISinglePropertyValidatorBuilder<T> singlePropertyValidatorBuilder, int delayMs)
    {
        return singlePropertyValidatorBuilder.AddValidator(new AsyncDelayedValidator<T>(delayMs));
    }
}