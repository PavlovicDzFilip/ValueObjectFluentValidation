//using Shouldly;
//using ValueObjectFluentValidation;
//using Xunit;

//namespace ValueObjectFluentValidationTests;

//public class MultiFieldValidatorTests
//{
//    [Fact]
//    public void WithNoRules_CreatesValueObject()
//    {
//        // Arrange
//        var street = "street";
//        var number = "1a";

//        // Act
//        var result = Address.WithNoRules(street, number);

//        // Assert
//        result.TryGet(out var address)
//            .ShouldBeTrue();

//        address.ShouldNotBeNull();
//        address.Street.ShouldBe(street);
//        address.Number.ShouldBe(number);
//    }

//    [Fact]
//    public void WithNotSatisfiedRules_FailsToCreateValueObject()
//    {
//        // Arrange
//        var street = "street";
//        var number = "1a";

//        // Act
//        var result = Address.WithNotSatisfiedRules(street, number);

//        // Assert
//        result.TryGet(out var address)
//            .ShouldBeFalse();

//        address.ShouldBeNull();
//    }

//    public record Address
//    {
//        public string Street { get; }
//        public string Number { get; }

//        private Address(string street, string number)
//        {
//            Street = street;
//            Number = number;
//        }

//        public static GroupResult<Address> WithNoRules(string street, string number)
//        {
//            var streetValidator = Validator.For(street);
//            var numberValidator = Validator.For(number);

//            return Validator
//                .Group(streetValidator, numberValidator)
//                .WhenValid((s, n) => new Address(s, n));
//        }

//        public static GroupResult<Address> WithNotSatisfiedRules(string street, string number)
//        {
//            var streetValidator = Validator.For(street)
//                .Length(10000, -5);

//            var numberValidator = Validator.For(number)
//                .Length(10001, -6);

//            return Validator
//                .Group(streetValidator, numberValidator)
//                .WhenValid((s, n) => new Address(s, n));
//        }
//    }
//}