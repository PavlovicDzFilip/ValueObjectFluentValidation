using System.Collections.ObjectModel;
using ValueObjectFluentValidation.Single;

namespace ValueObjectFluentValidation.Group;

internal class GroupValidationFailureCollection : IGroupValidationFailureCollection
{
    private readonly Dictionary<int, IValidationFailureCollection> _failuresByIndex = new();
    public void AddFailure(int index, IValidationFailureCollection failureCollection)
    {
        _failuresByIndex.Add(index, failureCollection);
    }

    public bool HasFailures()
        => _failuresByIndex.Any();

#if DEBUG
    public IReadOnlyDictionary<int, IValidationFailureCollection> AsDictionary()
        => new ReadOnlyDictionary<int, IValidationFailureCollection>(_failuresByIndex); 
#endif
}