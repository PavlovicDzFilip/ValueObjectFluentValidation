using System.Collections;

namespace ValueObjectFluentValidation.Single;

internal class ValidationFailureCollection : IValidationFailureCollection
{
    private readonly List<IValidationFailure> _failures = new();

    public void AddFailure(IValidationFailure failure)
        => _failures.Add(failure);

    public IEnumerator<IValidationFailure> GetEnumerator()
    {
        return _failures.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_failures).GetEnumerator();
    }
}