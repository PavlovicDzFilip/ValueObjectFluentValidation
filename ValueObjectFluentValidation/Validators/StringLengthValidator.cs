using ValueObjectFluentValidation.Single;

namespace ValueObjectFluentValidation.Validators;

public class StringLengthValidator : IValidator<string>
{
    private readonly int _minLength;
    private readonly int _maxLength;

    public StringLengthValidator(int minLength, int maxLength)
    {
        _minLength = minLength;
        _maxLength = maxLength;
    }

    public void Validate(string value, IValidationFailureCollection context)
    {
        if (value.Length < _minLength ||
            value.Length > _maxLength)
        {
            var failure = new StringLengthFailure(_minLength, _maxLength, value.Length);
            context.AddFailure(failure);
        }
    }
}