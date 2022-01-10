using ValueObjectFluentValidation.Util;

namespace ValueObjectFluentValidation.Single;

public sealed class SingleResult<T> : Result<T, IValidationFailureCollection>
{
    internal SingleResult(T success) : base(success)
    {
    }

    internal SingleResult(IValidationFailureCollection failure) : base(failure)
    {
    }
}