using System.Collections;
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


public interface IValidationFailureCollection : IEnumerable<IValidationFailure>
{
    void AddFailure(IValidationFailure failure);
}

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

internal class SinglePropertySinglePropertyValidatorBuilderInitial<T> :
    SinglePropertySinglePropertyValidatorBuilder<T>,
    ISinglePropertySinglePropertyValidatorBuilderInitial<T>
{
    public SinglePropertySinglePropertyValidatorBuilderInitial(T value) : base(value)
    {
    }

    public ISinglePropertyValidatorBuilder<TTransformed> Transform<TTransformed>(Func<T, TTransformed> transformFunc)
    {
        var transformed = transformFunc(Value);
        return new SinglePropertySinglePropertyValidatorBuilder<TTransformed>(transformed);
    }
}


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

public interface ISinglePropertyValidatorBuilder<out T>
{
    ISinglePropertyValidatorBuilder<T> AddValidator(IValidator<T> validator);
    ISinglePropertyValidatorBuilder<T> AddValidator(IAsyncValidator<T> validator);

    public SingleResult<TValueObject> WhenValid<TValueObject>(
        Func<T, TValueObject> createValueObjectFunc);

    public Task<SingleResult<TValueObject>> WhenValidAsync<TValueObject>(
        Func<T, TValueObject> createValueObjectFunc);
}

public interface ISinglePropertySinglePropertyValidatorBuilderInitial<T> : ISinglePropertyValidatorBuilder<T>
{
    ISinglePropertyValidatorBuilder<TTransformed> Transform<TTransformed>(Func<T, TTransformed> transformFunc);
}