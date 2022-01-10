using ValueObjectFluentValidation.Util;

namespace ValueObjectFluentValidation.Group;

public class GroupResult<T> : Result<T, IGroupValidationFailureCollection>
{
    internal GroupResult(T success) : base(success)
    {
    }

    internal GroupResult(IGroupValidationFailureCollection failure) : base(failure)
    {
    }
}