using Shouldly;
using ValueObjectFluentValidation;
using Xunit;

namespace ValueObjectFluentValidationTests
{
    public class MultiFieldValidator
    {
        [Fact]
        public void WithNoRules_CreatesValueObject()
        {
            // Arrange
            var street = "street";
            var number = "1a";

            // Act
            var result = Address.WithNoRules(street, number);

            // Assert
            result.TryGet(out var address)
                .ShouldBeTrue();

            address.ShouldNotBeNull();
            address.Street.ShouldBe(street);
            address.Number.ShouldBe(number);
        }

        [Fact]
        public void WithNotSatisfiedRules_FailsToCreateValueObject()
        {
            // Arrange
            var street = "street";
            var number = "1a";

            // Act
            var result = Address.WithNotSatisfiedRules(street, number);

            // Assert
            result.TryGet(out var address)
                .ShouldBeFalse();

            address.ShouldBeNull();
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

            public static GroupResult<Address> WithNoRules(string street, string number)
            {
                var streetValidator = Validator.For(street);
                var numberValidator = Validator.For(number);

                return Validator
                    .Group(streetValidator, numberValidator)
                    .WhenValid((s, n) => new Address(s, n));
            }

            public static GroupResult<Address> WithNotSatisfiedRules(string street, string number)
            {
                var streetValidator = Validator.For(street)
                    .Length(10000, -5);

                var numberValidator = Validator.For(number)
                    .Length(10001, -6);

                return Validator
                    .Group(streetValidator, numberValidator)
                    .WhenValid((s, n) => new Address(s, n));
            }
        }
    }
    //public record ApplicationName
    //{
    //    public string Name { get; }

    //    private ApplicationName(string name)
    //    {
    //        Name = name;
    //    }

    //    public static Result<ApplicationName> TryCreate(string? name)
    //    {
    //        return Validator.For(name)
    //            .Transform(n => (n ?? string.Empty).Trim())
    //            .NotNull()
    //            .Length(3, 10)
    //            .WhenValid(n => new ApplicationName(n));
    //    }
    //}

    //public record Address
    //{
    //    public string Street { get; }
    //    public string Number { get; }

    //    private Address(string street, string number)
    //    {
    //        Street = street;
    //        Number = number;
    //    }

    //    public static Result<Address> TryCreate(string? street, string? number)
    //    {
    //        var streetValidator = Validator.For(street)
    //            .Transform(n => (n ?? string.Empty).Trim())
    //            .NotNull()
    //            .Length(3, 10);

    //        var numberValidator = Validator.For(number)
    //            .NotNull();

    //        return Validator
    //            .Group(streetValidator, numberValidator)
    //            .WhenValid((s, n) => new Address(s, n));
    //    }
    //}

    //public class CreateApplication
    //{
    //    public record Command(string? Name, string? Street, string? Number, int Tmp, string NonNullStr);
    //    public record CommandWithAddress(string? Name, AddressDto Address);
    //    public record AddressDto(string Street, string Number);

    //    public record ValidCommand(ApplicationName Name);

    //    public class RequestValidatorTransformator : AbstractRequestValidator<Command, ValidCommand>
    //    {
    //        public override Result<ValidCommand> Validate(Command command)
    //        {
    //            var r3 = Rule(x => x.NonNullStr, Result<string>.Success);
    //            return RequestValidator.For(command)
    //                .Group(
    //                    Rule(cmd => cmd.Name, ApplicationName.TryCreate),
    //                    Rule(cmd => cmd.Street, cmd => cmd.Number, Address.TryCreate))
    //                .WhenValid((name, address) => new ValidCommand(name));
    //        }
    //    }

    //    public class AddressDtoValidator : AbstractRequestValidator<AddressDto, Address>
    //    {
    //        public override Result<Address> Validate(AddressDto value)
    //        {
    //            return RequestValidator
    //                .For(value)
    //                .Group(
    //                    Rule(cmd => (cmd.Street, cmd.Number), tuple => Address.TryCreate(tuple.Street, tuple.Number)),
    //                    Rule(cmd => cmd.Number, x => Result<string>.Failure(new ValueNullFailure()))
    //                )
    //                .WhenValid((a, x) => a);
    //        }
    //    }

    //    public class Handler
    //    {
    //        void Handle(ValidCommand command) { }
    //    }
    //}
}