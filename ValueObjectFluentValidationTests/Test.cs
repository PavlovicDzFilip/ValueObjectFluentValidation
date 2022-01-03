using System.Reflection.Metadata;
using Shouldly;
using ValueObjectFluentValidation;
using Xunit;

namespace ValueObjectFluentValidationTests
{
    public class Test { }

    public class RequestValidator
    {
        [Fact]
        public void WithNotSatisfiedRules_ReturnsAllFailures()
        {
            // Arrange
            var request = new CreateCustomer("n", "", "very big city name");
            var validator = new CommandValidator();

            // Act
            var result = validator.Validate(request);

            // Assert
            result.TryGet(out var validCommand)
                .ShouldBeFalse();

            validCommand.ShouldBeNull();

        }

        public record CreateCustomer(string Name, string CountryIso, string City);

        public class CustomerName
        {
            public string Name { get; }

            private CustomerName(string name)
            {
                Name = name;
            }

            public static Result<CustomerName> TryCreate(string name)
                => Validator.For(name)
                    .NotNull()
                    .Length(3, 10)
                    .WhenValid(n => new CustomerName(n));
        }

        public class Address
        {
            public string CountryIso { get; }
            public string City { get; }

            private Address(string countryIso, string city)
            {
                CountryIso = countryIso;
                City = city;
            }

            public static GroupResult<Address> TryCreate(string countryIso, string city)
                => Validator.Group(
                        Validator
                            .For(countryIso)
                            .NotNull(),
                        Validator
                            .For(city)
                            .NotNull()
                            .Length(1, 5))
                    .WhenValid((iso, c) => new Address(iso, c));
        }

        public record CreateCustomerValidCommand(CustomerName CustomerName, Address Address);

        public class CommandValidator : AbstractRequestValidator<CreateCustomer, CreateCustomerValidCommand>
        {
            public override ValidationResult<CreateCustomerValidCommand> Validate(CreateCustomer value)
                => ValueObjectFluentValidation.RequestValidator.For(value)
                    .Group(
                        Rule(cmd => cmd.Name, CustomerName.TryCreate),
                        Rule(cmd => cmd.CountryIso, cmd => cmd.City, Address.TryCreate))
                    .WhenValid((name, address) => new CreateCustomerValidCommand(name, address));
        }
    }
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