using ValueObjectFluentValidation;
using Xunit;

namespace ValueObjectFluentValidationTests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var appName = ApplicationName.TryCreate("nesto");
            var a = 5;
        }
    }

    public record ApplicationName
    {
        public string Name { get; }

        private ApplicationName(string name)
        {
            Name = name;
        }

        public static Result<ApplicationName> TryCreate(string name)
        {
            return Validator.For(name)
                .NotNull()
                .Length(3, 10)//.FailWith(invalidValue => new IFailure())
                .WhenValid(validName => new ApplicationName(validName));
        }
    }

    //public static class ValidatorExtensions
    //{
    //    public static IValidatorBuilder<string> IsEmailAddress<T>(this IValidatorBuilder<T> asd)
    //    {

    //    }
    //}
}