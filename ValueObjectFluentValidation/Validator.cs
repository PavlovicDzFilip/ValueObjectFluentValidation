using ValueObjectFluentValidation.Group;
using ValueObjectFluentValidation.Single;

namespace ValueObjectFluentValidation;

public class Validator
{
    public static ISinglePropertySinglePropertyValidatorBuilderInitial<T> For<T>(T value)
    {
        return new SinglePropertySinglePropertyValidatorBuilderInitial<T>(value);
    }

    public static IGroupValidator<T1, T2> Group<T1, T2>(
        ISinglePropertyValidatorBuilder<T1> firstSinglePropertyValidator,
        ISinglePropertyValidatorBuilder<T2> secondSinglePropertyValidator)
    {
        return new GroupValidator<T1, T2>(firstSinglePropertyValidator, secondSinglePropertyValidator);
    }
}