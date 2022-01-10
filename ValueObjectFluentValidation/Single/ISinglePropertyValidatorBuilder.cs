namespace ValueObjectFluentValidation.Single;

public interface ISinglePropertyValidatorBuilder<out T>
{
    ISinglePropertyValidatorBuilder<T> AddValidator(IValidator<T> validator);
    ISinglePropertyValidatorBuilder<T> AddValidator(IAsyncValidator<T> validator);

    public SingleResult<TValueObject> WhenValid<TValueObject>(
        Func<T, TValueObject> createValueObjectFunc);

    public Task<SingleResult<TValueObject>> WhenValidAsync<TValueObject>(
        Func<T, TValueObject> createValueObjectFunc);
}