using System;
using System.Linq.Expressions;
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

        public static Result<ApplicationName> TryCreate(string? name)
        {
            var validator = Validator.For(name)
                .Transform(n => (n ?? string.Empty).Trim())
                .NotNull()
                .Length(3, 10)
                .WhenValid(n => new ApplicationName(n));

            var result = validator.Validate();
            return result;
        }
    }

    public record Address
    {
        public string Street { get; }
        public string Number { get; }

        private Address(string street, string number)
        {
            Street = street;
            Number = number;
        }

        public static Result<Address> TryCreate(string? street, string? number)
        {
            var streetValidator = Validator.For(street)
                .Transform(n => (n ?? string.Empty).Trim())
                .NotNull()
                .Length(3, 10);

            var numberValidator = Validator.For(number)
                .NotNull();

            var validator = Validator.Group(streetValidator, numberValidator)
                .WhenValid((s, n) => new Address(s, n));

            return validator.Validate();
        }
    }


    //public class GenericValidator<T, TValueObject> : AbstractValidator<T>
    //{
    //    public Result<TValueObject> ValidateAndTransform(T value, Func<T, TValueObject> createValueObjectFunc)
    //    {
    //        if (typeof(T) == typeof(string) && value is null)
    //        {
    //            value = (T)(object)string.Empty;
    //        }

    //        var validationResult = Validate(value);
    //        if (validationResult.IsValid)
    //        {
    //            return Result<TValueObject>.Success(createValueObjectFunc(value));
    //        }

    //    }
    //}

    public class CreateApplication
    {
        public record Command(string? Name, string? Street, string? Number, string Tmp);

        private record ValidCommand(ApplicationName Name);

        public class RequestValidatorTransformator
        {
            public Result<ValidCommand> Validate(Command command)
            {
                return RequestValidator.For(command)
                    .RuleFor(cmd => cmd.Name, ApplicationName.TryCreate)
                    .RuleFor(cmd => (cmd.Street, cmd.Number), v => Address.TryCreate(v.Street, v.Number))
                    .WhenValid((name, address) => new ValidCommand(name))
                    .Validate();
                //return new ValidCommand(
                //    DoValidation(command, cmd => cmd.Name, ApplicationName.TryCreate));
            }

            public TValueObject DoValidation<TCommand, TValueObject, TProperty>(
                TCommand command,
                Expression<Func<TCommand, TProperty>> expr,
                Func<TProperty, Result<TValueObject>> valueObjectFactory)
            {
                //
                return default;
            }
        }

        public class Handler
        {
            void Handle(ValidCommand command) { }
        }
    }
}