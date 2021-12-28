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
            return Validator.For(name)
                .Transform(n => (n ?? string.Empty).Trim())
                .NotNull()
                .Length(3, 10)
                .WhenValid(n => new ApplicationName(n));
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

            return Validator
                .Group(streetValidator, numberValidator)
                .WhenValid((s, n) => new Address(s, n));
        }
    }

    public class CreateApplication
    {
        public record Command(string? Name, string? Street, string? Number, int Tmp, string NonNullStr);
        public record CommandWithAddress(string? Name, AddressDto Address);
        public record AddressDto(string Street, string Number);

        public record ValidCommand(ApplicationName Name);

        public class RequestValidatorTransformator : AbstractRequestValidator<Command, ValidCommand>
        {
            public override Result<ValidCommand> Validate(Command command)
            {
                var r3 = Rule(x => x.NonNullStr, Result<string>.Success);
                return RequestValidator.For(command)
                    .Group(
                        Rule(cmd => cmd.Name, ApplicationName.TryCreate),
                        Rule(cmd => (cmd.Street, cmd.Number), v => Address.TryCreate(v.Street, v.Number))
                            .WithPropertyName("Address")
                        )
                    .WhenValid((name, address) => new ValidCommand(name));
            }
        }

        public class AddressDtoValidator : AbstractRequestValidator<AddressDto, Address>
        {
            public override Result<Address> Validate(AddressDto value)
            {
                return RequestValidator
                    .For(value)
                    .Group(
                        Rule(cmd => (cmd.Street, cmd.Number), tuple => Address.TryCreate(tuple.Street, tuple.Number)),
                        Rule(cmd => cmd.Number, x => Result<string>.Failure(new IValidationFailure[] { new ValueNullFailure() }))
                    )
                    .WhenValid((a, x) => a);
            }
        }

        public class Handler
        {
            void Handle(ValidCommand command) { }
        }
    }
}