using ValueObjectFluentValidation.Single;

namespace ValueObjectFluentValidation.Group;

public interface IGroupValidationFailureCollection
{
    void AddFailure(int index, IValidationFailureCollection failureCollection);
    bool HasFailures();
#if DEBUG
    IReadOnlyDictionary<int, IValidationFailureCollection> AsDictionary(); 
#endif
}