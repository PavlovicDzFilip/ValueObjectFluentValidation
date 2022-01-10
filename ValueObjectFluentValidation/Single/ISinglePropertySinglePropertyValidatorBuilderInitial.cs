namespace ValueObjectFluentValidation.Single;

public interface ISinglePropertySinglePropertyValidatorBuilderInitial<T> : ISinglePropertyValidatorBuilder<T>
{
    ISinglePropertyValidatorBuilder<TTransformed> Transform<TTransformed>(Func<T, TTransformed> transformFunc);
}