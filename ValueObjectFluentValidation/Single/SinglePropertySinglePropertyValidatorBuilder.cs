namespace ValueObjectFluentValidation.Single;

internal class SinglePropertySinglePropertyValidatorBuilder<T> : ISinglePropertyValidatorBuilder<T>
{
    private protected readonly T Value;
    private readonly Queue<IValidatorInternal<T>> _validators;

    public SinglePropertySinglePropertyValidatorBuilder(T value)
    {
        Value = value;
        _validators = new Queue<IValidatorInternal<T>>();
    }

    public ISinglePropertyValidatorBuilder<T> AddValidator(IValidator<T> validator)
    {
        _validators.Enqueue(new ValidatorWrapper<T>(validator));
        return this;
    }

    public ISinglePropertyValidatorBuilder<T> AddValidator(IAsyncValidator<T> validator)
    {
        _validators.Enqueue(new ValidatorWrapper<T>(validator));
        return this;
    }

    public SingleResult<TValueObject> WhenValid<TValueObject>(
        Func<T, TValueObject> createValueObjectFunc)
    {
        var failureCollection = new ValidationFailureCollection();

        while (_validators.TryDequeue(out var validator))
        {
            validator.Validate(Value, failureCollection);
            if (failureCollection.Any())
            {
                return new SingleResult<TValueObject>(failureCollection);
            }
        }

        var valueObject = createValueObjectFunc(Value);
        return new SingleResult<TValueObject>(valueObject);
    }

    public async Task<SingleResult<TValueObject>> WhenValidAsync<TValueObject>(
        Func<T, TValueObject> createValueObjectFunc)
    {
        var failureCollection = new ValidationFailureCollection();

        while (_validators.TryDequeue(out var validator))
        {
            await validator.ValidateAsync(Value, failureCollection);
            if (failureCollection.Any())
            {
                return new SingleResult<TValueObject>(failureCollection);
            }
        }

        var valueObject = createValueObjectFunc(Value);
        return new SingleResult<TValueObject>(valueObject);
    }
}