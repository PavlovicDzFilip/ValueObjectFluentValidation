using System.Diagnostics;
using System.Threading.Tasks;
using Shouldly;
using ValueObjectFluentValidation;
using Xunit;

namespace ValueObjectFluentValidationTests;

public class SingleFieldValidator
{
    [Fact]
    public void WithNoRules_IsValid()
    {
        // Arrange
        var value = string.Empty;

        // Act
        var result = SingleFieldValueObject.TryCreateWithNoRules(value);

        // Assert
        result
            .ShouldNotBeNull()
            .TryGet(out var valueObject).ShouldBeTrue();

        valueObject.ShouldNotBeNull();
    }

    [Fact]
    public void WithSatisfiedRules_IsValid()
    {
        // Arrange
        var value = "not null value";

        // Act
        var result = SingleFieldValueObject.TryCreateWithNotNullRule(value);

        // Assert
        result
            .ShouldNotBeNull()
            .TryGet(out var valueObject)
            .ShouldBeTrue();

        valueObject.ShouldNotBeNull();
        valueObject.Value.ShouldBe(value);
    }

    [Fact]
    public void WithNotSatisfiedRule_IsNotValid()
    {
        // Arrange

        // Act
        var result = SingleFieldValueObject.TryCreateWithNotNullRule(null);

        // Assert
        result
            .ShouldNotBeNull()
            .TryGet(out var valueObject)
            .ShouldBeFalse();

        valueObject.ShouldBeNull();
        result.Failures
            .ShouldHaveSingleItem()
            .ShouldBeOfType<ValueNullFailure>();
    }

    [Theory]
    [InlineData("2c")]
    [InlineData("11character")]
    public void WithNotSatisfiedRules_IsNotValid(string? value)
    {
        // Arrange

        // Act
        var result = SingleFieldValueObject.TryCreateWithMultipleRules(value);

        // Assert
        result
            .ShouldNotBeNull()
            .TryGet(out var valueObject)
            .ShouldBeFalse();

        valueObject.ShouldBeNull();
        var failure = result.Failures
            .ShouldHaveSingleItem()
            .ShouldBeOfType<StringLengthFailure>();
        failure.MinLength.ShouldBe(3);
        failure.MaxLength.ShouldBe(10);
        failure.ActualLength.ShouldBe(value!.Length);
    }

    [Fact]
    public async Task WithSatisfiedAsyncRules_IsValid()
    {
        // Arrange
        var value = "value";

        // Act
        var stopwatch = Stopwatch.StartNew();
        var result = await SingleFieldValueObject.TryCreateWithMultipleRulesAsync(value);
        stopwatch.Stop();

        // Assert
        result
            .ShouldNotBeNull()
            .TryGet(out var valueObject)
            .ShouldBeTrue();

        valueObject.ShouldNotBeNull();

        stopwatch.ElapsedMilliseconds.ShouldBeGreaterThan(2500);
    }


    [Fact]
    public void InvokesAsyncValidatorSynchronously()
    {
        // Arrange
        var value = "value";

        // Act
        var stopwatch = Stopwatch.StartNew();
        var result =  SingleFieldValueObject.TryCreateWithMultipleRulesSync(value);
        stopwatch.Stop();

        // Assert
        result
            .ShouldNotBeNull()
            .TryGet(out var valueObject)
            .ShouldBeTrue();

        valueObject.ShouldNotBeNull();

        stopwatch.ElapsedMilliseconds.ShouldBeGreaterThan(2500);
    }

    public record SingleFieldValueObject
    {
        public string? Value { get; }

        private SingleFieldValueObject(string? value)
        {
            Value = value;
        }

        public static Result<SingleFieldValueObject> TryCreateWithNoRules(string? value)
        {
            return Validator
                .For(value)
                .WhenValid(s => new SingleFieldValueObject(s));
        }

        public static Result<SingleFieldValueObject> TryCreateWithNotNullRule(string? value)
        {
            return Validator
                .For(value)
                .NotNull()
                .WhenValid(s => new SingleFieldValueObject(s));
        }

        public static Result<SingleFieldValueObject> TryCreateWithMultipleRules(string? value)
        {
            return Validator
                .For(value)
                .NotNull()
                .Length(3, 10)
                .WhenValid(s => new SingleFieldValueObject(s));
        }

        public static Task<Result<SingleFieldValueObject>> TryCreateWithMultipleRulesAsync(string? value)
        {
            return Validator
                .For(value)
                .NotNull()
                .Length(3, 10)
                .ValidAsync(3000)
                .WhenValidAsync(s => new SingleFieldValueObject(s));
        }

        public static Result<SingleFieldValueObject> TryCreateWithMultipleRulesSync(string? value)
        {
            return Validator
                .For(value)
                .NotNull()
                .Length(3, 10)
                .ValidAsync(3000)
                .WhenValid(s => new SingleFieldValueObject(s));
        }
    }
}