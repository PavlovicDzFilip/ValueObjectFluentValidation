namespace ValueObjectFluentValidation.Single;

public interface IValidationFailureCollection : IEnumerable<IValidationFailure>
{
    void AddFailure(IValidationFailure failure);
}