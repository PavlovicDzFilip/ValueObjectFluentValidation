using ValueObjectFluentValidation.Single;

namespace ValueObjectFluentValidation.Group;

internal class GroupValidator<T1, T2> : IGroupValidator<T1, T2>
{
    private readonly ISinglePropertyValidatorBuilder<T1> _firstSinglePropertyValidator;
    private readonly ISinglePropertyValidatorBuilder<T2> _secondSinglePropertyValidator;

    public GroupValidator(
        ISinglePropertyValidatorBuilder<T1> firstSinglePropertyValidator,
        ISinglePropertyValidatorBuilder<T2> secondSinglePropertyValidator)
    {
        _firstSinglePropertyValidator = firstSinglePropertyValidator;
        _secondSinglePropertyValidator = secondSinglePropertyValidator;
    }

    public GroupResult<TValueObject> WhenValid<TValueObject>(Func<T1, T2, TValueObject> createValueObjectFunc)
    {
        var groupFailures = new GroupValidationFailureCollection();

        var v1 = _firstSinglePropertyValidator.WhenValid(w => w);
        var v2 = _secondSinglePropertyValidator.WhenValid(w => w);

        if (!v1.TryGet(out var v1Value))
        {
            groupFailures.AddFailure(0, v1.Failure);
        }

        if (!v2.TryGet(out var v2Value))
        {
            groupFailures.AddFailure(1, v2.Failure);
        }

        if (groupFailures.HasFailures())
        {
            return new GroupResult<TValueObject>(groupFailures);
        }

        var valueObject = createValueObjectFunc.Invoke(v1Value!, v2Value!);
        return new GroupResult<TValueObject>(valueObject);
    }

    public async Task<GroupResult<TValueObject>> WhenValidAsync<TValueObject>(Func<T1, T2, TValueObject> createValueObjectFunc)
    {
        var groupFailures = new GroupValidationFailureCollection();

        var v1 = await _firstSinglePropertyValidator.WhenValidAsync(w => w);
        var v2 = await _secondSinglePropertyValidator.WhenValidAsync(w => w);

        if (!v1.TryGet(out var v1Value))
        {
            groupFailures.AddFailure(0, v1.Failure);
        }

        if (!v2.TryGet(out var v2Value))
        {
            groupFailures.AddFailure(1, v2.Failure);
        }

        if (groupFailures.HasFailures())
        {
            return new GroupResult<TValueObject>(groupFailures);
        }

        var valueObject = createValueObjectFunc.Invoke(v1Value!, v2Value!);
        return new GroupResult<TValueObject>(valueObject);
    }
}