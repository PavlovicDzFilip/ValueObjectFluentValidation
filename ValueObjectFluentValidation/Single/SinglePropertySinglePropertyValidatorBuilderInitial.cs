namespace ValueObjectFluentValidation.Single;

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