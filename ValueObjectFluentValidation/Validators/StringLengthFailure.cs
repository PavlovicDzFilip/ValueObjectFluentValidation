namespace ValueObjectFluentValidation.Validators;

public record StringLengthFailure(int MinLength, int MaxLength, int ActualLength) : IValidationFailure
{
}